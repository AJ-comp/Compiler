#!/usr/bin/env python3
"""Run source-only Test262 profiles, retaining timeouts/crashes as failures.

The child runner never evaluates JavaScript. Each profile has its own parser.
A timed-out child exits, so the coordinator resumes with a fresh process rather
than sharing a parser with unfinished work. Results are append-only and a resume
must match the input case IDs and source hashes exactly.
"""
from __future__ import annotations

import argparse
from collections import Counter
from concurrent.futures import ThreadPoolExecutor
import hashlib
import json
from pathlib import Path
import subprocess
import time


PROFILES = ("script-strict", "script-nonstrict", "module")


def profile_for(case):
    if case["inputKind"].lower() == "module":
        return "module"
    return "script-strict" if case["mode"].lower() == "strict" else "script-nonstrict"


def read_rows(path):
    if not path.exists():
        return []
    with path.open(encoding="utf-8-sig") as stream:
        return [json.loads(line) for line in stream if line.strip()]


def validate_resume(cases, results):
    if len(results) > len(cases):
        raise ValueError("Result count exceeds input count")
    for case, result in zip(cases, results):
        if not case.get("sourceSha256") or not result.get("sourceSha256"):
            raise ValueError("Resume requires a source hash for every recorded case")
        if (case["id"] != result["id"] or case["sourceSha256"] != result["sourceSha256"] or
                case["expectAccept"] != result.get("expectedAccept") or
                case["inputKind"].lower() != result.get("inputKind", "").lower() or
                case["mode"].lower() != result.get("mode", "").lower() or
                case.get("variant", "") != result.get("variant", "")):
            raise ValueError("Resume input differs from recorded results: " + case["id"])


def file_sha256(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def execution_manifest(options, selected):
    # Product dependencies are copied beside the runner. Checking only its own
    # DLL would silently retain results from an older JavaScript implementation.
    if not options.runner.is_file():
        raise ValueError("Runner does not exist: " + str(options.runner))
    dependencies = sorted(path for path in options.runner.parent.rglob("*")
                          if path.is_file() and path.suffix.lower() in {".dll", ".json"})
    return {"schemaVersion": 1, "inputSha256": file_sha256(options.input),
            "runner": options.runner.name,
            "binarySha256": {str(path.relative_to(options.runner.parent)): file_sha256(path)
                             for path in dependencies},
            "profiles": sorted(selected), "timeoutMs": options.timeout_ms}


def validate_manifest(options, selected):
    manifest = execution_manifest(options, selected)
    path = options.output_dir / "run-manifest.json"
    if options.resume:
        if not path.exists():
            raise ValueError("Resume requires the original run-manifest.json")
        if json.loads(path.read_text(encoding="utf-8")) != manifest:
            raise ValueError("Resume input, runner/product binaries, or execution options changed; use a new output directory")
    else:
        if path.exists() or any(options.output_dir.glob("*.jsonl")):
            raise ValueError("Existing results require --resume or a new output directory")
        path.write_text(json.dumps(manifest, indent=2) + "\n", encoding="utf-8")
    return manifest


def run_profile(profile, cases, options):
    if not cases:
        return {"profile": profile, "expected": 0, "processed": 0, "complete": None,
                "status": "skipped", "reason": "no-selected-cases", "outcomes": {}, "restarts": 0}
    output = options.output_dir / (profile + ".jsonl")
    failure = Path(str(output) + ".failures.jsonl")
    if output.exists() and not options.resume:
        raise ValueError("Existing results require --resume or a new output directory: " + str(output))
    results = read_rows(output)
    validate_resume(cases, results)
    restarts = 0
    with (options.output_dir / (profile + ".log")).open("a", encoding="utf-8") as log:
        while len(results) < len(cases):
            skip = len(results)
            command = [str(options.dotnet), str(options.runner), "--input", str(options.input),
                       "--output", str(output), "--profile", profile, "--skip", str(skip),
                       "--timeout-ms", str(options.timeout_ms)]
            if skip:
                command.append("--append")
            log.write("\nCOMMAND " + json.dumps(command) + "\n")
            log.flush()
            completed = subprocess.run(command, stdout=log, stderr=log, check=False)
            results = read_rows(output)
            validate_resume(cases, results)
            if completed.returncode == 124 and (len(results) <= skip or results[-1]["outcome"] != "timeout"):
                raise RuntimeError(profile + ": child timeout exit has no recorded timeout")
            if len(results) == len(cases):
                if completed.returncode not in (0, 1, 124):
                    raise RuntimeError(f"{profile}: child crashed after recording results (exit {completed.returncode})")
                if completed.returncode == 1 and all(row["outcome"] == "pass" for row in results):
                    raise RuntimeError(profile + ": child reported failure despite all-pass results")
                break
            if completed.returncode in (0, 1):
                raise RuntimeError(f"{profile}: child ended before all cases ({len(results)}/{len(cases)})")
            if completed.returncode != 124:
                # A process-level crash is never a successful syntax rejection.
                # Preserve the next unrecorded case, then continue independently.
                case = cases[len(results)]
                result = {key: case.get(key) for key in ("id", "path", "variant", "inputKind", "mode", "sourceSha256")}
                result.update(expectedAccept=case["expectAccept"], accepted=False,
                              outcome="crash", stage="crash", processExitCode=completed.returncode)
                with output.open("a", encoding="utf-8") as stream:
                    stream.write(json.dumps(result) + "\n")
                with failure.open("a", encoding="utf-8") as stream:
                    stream.write(json.dumps({"test": case, "result": result}) + "\n")
                results.append(result)
            if len(results) <= skip:
                raise RuntimeError(profile + ": child made no progress")
            restarts += 1
            if restarts > options.max_restarts:
                raise RuntimeError(profile + ": restart limit reached; results remain incomplete")
    counts = dict(Counter(row["outcome"] for row in results))
    return {"profile": profile, "expected": len(cases), "processed": len(results),
            "complete": len(results) == len(cases), "status": "complete",
            "outcomes": counts, "restarts": restarts}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, required=True)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--dotnet", type=Path, default=Path("dotnet"))
    parser.add_argument("--runner", type=Path, default=Path("tools/Janglim.Test262/bin/Release/net8.0/Janglim.Test262.dll"))
    parser.add_argument("--profile", choices=PROFILES, action="append")
    parser.add_argument("--timeout-ms", type=int, default=30000)
    parser.add_argument("--max-restarts", type=int, default=50)
    parser.add_argument("--resume", action="store_true")
    options = parser.parse_args()
    options.input = options.input.resolve()
    options.runner = options.runner.resolve()
    options.output_dir.mkdir(parents=True, exist_ok=True)
    rows = read_rows(options.input)
    selected = options.profile or list(PROFILES)
    if len(set(selected)) != len(selected):
        raise ValueError("Each profile may be selected only once")
    if options.timeout_ms <= 0 or options.max_restarts < 0:
        raise ValueError("Timeout must be positive and max-restarts must be nonnegative")
    manifest = validate_manifest(options, selected)
    grouped = {profile: [row for row in rows if profile_for(row) == profile] for profile in selected}
    started = time.monotonic()
    with ThreadPoolExecutor(max_workers=len(selected)) as pool:
        futures = [pool.submit(run_profile, profile, grouped[profile], options) for profile in selected]
        summaries = [future.result() for future in futures]
    totals = Counter()
    for summary in summaries:
        totals.update(summary["outcomes"])
    result = {"schemaVersion": 1, "syntaxOnly": True, "input": str(options.input),
              "inputSha256": manifest["inputSha256"],
              "runnerSha256": file_sha256(options.runner), "binarySha256": manifest["binarySha256"],
              "profiles": summaries, "processed": sum(totals.values()), "outcomes": dict(totals),
              "complete": bool(totals) and all(item["status"] in {"complete", "skipped"} for item in summaries),
              "elapsedSeconds": round(time.monotonic() - started, 3)}
    text = json.dumps(result, indent=2)
    (options.output_dir / "summary.json").write_text(text + "\n", encoding="utf-8")
    print(text)
    return 0 if totals and set(totals) <= {"pass"} else 1


if __name__ == "__main__":
    raise SystemExit(main())
