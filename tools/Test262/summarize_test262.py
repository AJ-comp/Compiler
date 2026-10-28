#!/usr/bin/env python3
"""Verify every source-only Test262 result before writing a compact report.

This reads a complete prepared corpus, its manifest, and a completed coordinator
run. It never executes JavaScript or changes input/result evidence. Missing,
duplicate, stale, contradictory, or incomplete evidence raises an error. A
complete run with genuine failed outcomes produces a report and exit code 1.
"""

from __future__ import annotations

import argparse
from collections import Counter
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path, PurePosixPath
import re
import sys


PROFILES = ("script-strict", "script-nonstrict", "module")
OUTCOMES = {"pass", "unexpected-accept", "unexpected-reject", "error", "timeout", "crash"}
SYNTAX_EXCEPTIONS = {"ParsingException", "JavaScriptSyntaxException", "PredictionFailedException"}
HASH = re.compile(r"[0-9a-f]{64}\Z")
COMMIT = re.compile(r"[0-9a-f]{40}\Z")


def file_sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def load_json(path: Path) -> dict:
    value = json.loads(path.read_text(encoding="utf-8-sig"))
    if not isinstance(value, dict):
        raise ValueError("JSON document must be an object: " + str(path))
    return value


def rows(path: Path):
    with path.open(encoding="utf-8-sig") as stream:
        for number, text in enumerate(stream, 1):
            if text.strip():
                try:
                    value = json.loads(text)
                    if not isinstance(value, dict):
                        raise ValueError("JSONL record must be an object")
                except ValueError as error:
                    raise ValueError(f"{path}:{number}: {error}") from error
                yield value


def require(condition: bool, message: str):
    if not condition:
        raise ValueError(message)


def checksum(value: object, description: str) -> str:
    require(isinstance(value, str) and HASH.fullmatch(value) is not None, "invalid SHA-256: " + description)
    return value


def integer(value: object, description: str) -> int:
    require(type(value) is int and value >= 0, "invalid nonnegative integer: " + description)
    return value


def counter(value: object, description: str, allowed=None) -> dict:
    require(isinstance(value, dict), "invalid counter: " + description)
    for key, count in value.items():
        require(isinstance(key, str) and (allowed is None or key in allowed), "unknown counter key: " + description)
        integer(count, description + "." + key)
    return {key: count for key, count in value.items() if count}


def string_list(value: object, description: str) -> list[str]:
    if value is None:
        return []
    require(isinstance(value, list) and all(isinstance(item, str) for item in value), "invalid string list: " + description)
    return value


def profile_for(case: dict) -> str:
    return "module" if case["inputKind"] == "Module" else "script-strict" if case["mode"] == "Strict" else "script-nonstrict"


def classify(case: dict) -> str:
    phase = case.get("negativePhase")
    expected = {None: (True, "positive"), "parse": (False, "parse-negative"),
                "resolution": (True, "resolution-negative-source-only"), "runtime": (True, "runtime-negative-source-only")}
    require((phase is None or isinstance(phase, str)) and phase in expected, "unknown negative phase: " + case["id"])
    accept, classification = expected[phase]
    require(case["expectAccept"] is accept and case.get("classification") == classification,
            "source-only expectation/classification mismatch: " + case["id"])
    if phase == "parse":
        require(case.get("negativeType") == "SyntaxError", "parse-negative requires SyntaxError: " + case["id"])
    return classification


def read_cases(path: Path) -> tuple[dict, dict, Counter, Counter]:
    cases = {}
    grouped = {profile: [] for profile in PROFILES}
    classifications = Counter()
    variant_counts = Counter()
    for case in rows(path):
        for field in ("id", "path", "variant", "inputKind", "mode", "source", "sourceSha256"):
            require(isinstance(case.get(field), str) and (field == "source" or bool(case[field])), "invalid input field: " + field)
        for field in ("expectAccept", "webCompatibility"):
            require(type(case.get(field)) is bool, "invalid input boolean field: " + field)
        require(case["id"] not in cases, "duplicate input ID: " + case["id"])
        require(case["inputKind"] in {"Script", "Module"} and case["mode"] in {"Strict", "NonStrict"}, "unknown input kind/mode: " + case["id"])
        require(case["inputKind"] != "Module" or case["mode"] == "Strict", "Module must use strict mode: " + case["id"])
        checksum(case["sourceSha256"], case["id"])
        require(hashlib.sha256(case["source"].encode("utf-8")).hexdigest() == case["sourceSha256"], "input source hash mismatch: " + case["id"])
        classification = classify(case)
        compact = {key: case[key] for key in ("id", "path", "variant", "inputKind", "mode", "expectAccept", "webCompatibility", "sourceSha256")}
        compact["classification"] = classification
        cases[case["id"]] = compact
        grouped[profile_for(case)].append(case["id"])
        classifications[classification] += 1
        variant_counts[case["variant"]] += 1
    require(bool(cases), "input corpus is empty")
    return cases, grouped, classifications, variant_counts


def validate_result(result: dict, case: dict) -> tuple[str, bool]:
    for field in ("id", "path", "variant", "inputKind", "mode", "sourceSha256"):
        require(result.get(field) == case[field], "result " + field + " differs from input: " + case["id"])
    require(type(result.get("expectedAccept")) is bool and result["expectedAccept"] is case["expectAccept"], "result expectation differs from input: " + case["id"])
    if "webCompatibility" in result:
        require(type(result["webCompatibility"]) is bool and result["webCompatibility"] is case["webCompatibility"], "result webCompatibility differs from input: " + case["id"])
    require(type(result.get("accepted")) is bool, "result accepted must be boolean: " + case["id"])
    outcome = result.get("outcome")
    stage = result.get("stage")
    require(isinstance(outcome, str) and outcome in OUTCOMES, "unknown result outcome: " + case["id"])
    require(isinstance(stage, str), "unknown result stage: " + case["id"])
    accepted = result["accepted"]
    exception_chain = string_list(result.get("exceptionChain"), case["id"] + ".exceptionChain")
    diagnostics = string_list(result.get("diagnostics"), case["id"] + ".diagnostics")
    if stage == "accepted":
        require(accepted and not diagnostics and not exception_chain, "contradictory accepted result: " + case["id"])
    elif stage == "early-error":
        require(not accepted and bool(diagnostics) and not exception_chain, "contradictory early-error result: " + case["id"])
    elif stage == "parse":
        require(not accepted, "contradictory parse rejection: " + case["id"])
        if exception_chain:
            names = {item.partition(":")[0].rsplit(".", 1)[-1] for item in exception_chain}
            require(bool(names & SYNTAX_EXCEPTIONS), "unexpected exception marked as syntax rejection: " + case["id"])
    elif stage in {"error", "timeout", "crash"}:
        require(not accepted and outcome == stage, "runtime error/timeout/crash cannot be a pass: " + case["id"])
    else:
        raise ValueError("unknown result stage: " + case["id"])
    if stage in {"accepted", "parse", "early-error"}:
        actual_outcome = "pass" if accepted is case["expectAccept"] else "unexpected-accept" if accepted else "unexpected-reject"
        require(outcome == actual_outcome, "result outcome contradicts accepted/expected values: " + case["id"])
    return outcome, bool(exception_chain) and stage == "parse"


def verify_binaries(directory: Path, hashes: dict):
    directory = directory.resolve()
    for name, expected in hashes.items():
        relative = PurePosixPath(name.replace("\\", "/"))
        require(not relative.is_absolute() and ".." not in relative.parts and all(":" not in part for part in relative.parts), "unsafe binary manifest path: " + name)
        target = directory.joinpath(*relative.parts).resolve()
        require(target.is_relative_to(directory), "binary path escapes verification directory: " + name)
        require(file_sha256(target) == expected, "recorded binary hash mismatch: " + name)


def summarize(input_path: Path, results_dir: Path, output: Path, *, manifest_path: Path | None = None, binary_dir: Path | None = None) -> dict:
    input_path, results_dir, output = input_path.resolve(), results_dir.resolve(), output.resolve()
    manifest_path = (manifest_path or input_path.with_suffix(".manifest.json")).resolve()
    run_path, summary_path = results_dir / "run-manifest.json", results_dir / "summary.json"
    protected = {input_path, manifest_path, run_path, summary_path}
    for profile in PROFILES:
        protected.update(results_dir / (profile + suffix) for suffix in (".jsonl", ".jsonl.failures.jsonl", ".jsonl.summary.json", ".log"))
    require(output not in protected, "report must not overwrite input or run evidence")
    if binary_dir:
        require(not output.is_relative_to(binary_dir.resolve()), "report must not overwrite a binary verification directory")
    initial_hashes = {path: file_sha256(path) if path.is_file() else None
                      for path in protected if path.suffix in {".json", ".jsonl"}}
    source_manifest, run_manifest, run_summary = load_json(manifest_path), load_json(run_path), load_json(summary_path)
    for label, value in (("source manifest", source_manifest), ("run manifest", run_manifest), ("run summary", run_summary)):
        require(type(value.get("schemaVersion")) is int and value["schemaVersion"] == 1, "unsupported schema: " + label)
    input_hash = file_sha256(input_path)
    for label, document, field in (("source manifest", source_manifest, "casesSha256"), ("run manifest", run_manifest, "inputSha256"), ("run summary", run_summary, "inputSha256")):
        require(document.get(field) == input_hash, label + " input digest mismatch")
    snapshot = source_manifest.get("snapshot")
    require(isinstance(snapshot, dict) and isinstance(snapshot.get("commit"), str) and COMMIT.fullmatch(snapshot["commit"]) is not None, "source manifest requires a pinned source commit")
    require(snapshot.get("trackedFilesModified") is not True, "source snapshot contains modified tracked files")
    require(run_summary.get("syntaxOnly") is True and run_summary.get("complete") is True, "run summary must be complete and source-only")
    require(isinstance(run_manifest.get("profiles"), list) and len(run_manifest["profiles"]) == 3 and all(isinstance(item, str) for item in run_manifest["profiles"]) and set(run_manifest["profiles"]) == set(PROFILES), "run manifest must cover all three profiles")
    require(type(run_manifest.get("timeoutMs")) is int and run_manifest["timeoutMs"] > 0, "run manifest requires a positive timeout")
    hashes = run_manifest.get("binarySha256")
    require(isinstance(hashes, dict) and bool(hashes), "run manifest requires binary hashes")
    for name, digest in hashes.items():
        require(isinstance(name, str) and bool(name), "invalid binary filename")
        checksum(digest, name)
    runner = run_manifest.get("runner")
    require(isinstance(runner, str) and runner in hashes, "runner is missing from binary manifest")
    require(run_summary.get("binarySha256") == hashes and run_summary.get("runnerSha256") == hashes[runner], "summary binary hashes differ from run manifest")
    if binary_dir:
        verify_binaries(binary_dir, hashes)
    cases, grouped, classifications, variant_counts = read_cases(input_path)
    source_counts = source_manifest.get("counts")
    require(isinstance(source_counts, dict), "source manifest requires preparation counts")
    expected_accept = sum(case["expectAccept"] for case in cases.values())
    for key, expected in (("emittedVariants", len(cases)), ("plannedVariants", len(cases)), ("selectedFiles", len({case["path"] for case in cases.values()})), ("expectedAccept", expected_accept), ("expectedReject", len(cases) - expected_accept)):
        require(integer(source_counts.get(key), "source counts." + key) == expected, "source manifest count mismatch: " + key)
    for key, value in source_counts.items():
        if key.startswith("omittedVariants:"):
            require(integer(value, key) == 0, "source preparation omitted variants: " + key)
    require(counter(source_manifest.get("variantCounts"), "source variantCounts") == dict(variant_counts), "source manifest variant counts mismatch")
    summaries = run_summary.get("profiles")
    require(isinstance(summaries, list) and len(summaries) == 3 and all(isinstance(item, dict) for item in summaries), "summary requires all profile records")
    require(all(isinstance(item.get("profile"), str) for item in summaries) and {item["profile"] for item in summaries} == set(PROFILES), "summary has duplicate, missing, or unknown profiles")
    profile_summaries = {item["profile"]: item for item in summaries}
    seen = set()
    totals, stages = Counter(), Counter()
    profile_reports = {}
    raw_files = {}
    syntax_exception_rejections = 0
    for profile in PROFILES:
        result_path = results_dir / (profile + ".jsonl")
        failed_results = {}
        local_counts, local_stages = Counter(), Counter()
        profile_seen = set()
        if grouped[profile]:
            require(result_path.is_file(), "missing profile results: " + profile)
        if result_path.exists():
            for result in rows(result_path):
                case_id = result.get("id")
                require(isinstance(case_id, str) and case_id in cases, "unknown result ID: " + str(case_id))
                require(case_id not in seen, "duplicate result ID: " + case_id)
                case = cases[case_id]
                require(profile_for(case) == profile, "result is in the wrong profile: " + case_id)
                outcome, syntax_exception = validate_result(result, case)
                seen.add(case_id)
                profile_seen.add(case_id)
                local_counts[outcome] += 1
                local_stages[result["stage"]] += 1
                syntax_exception_rejections += syntax_exception
                if outcome != "pass":
                    failed_results[case_id] = result
            raw_files[profile] = {"path": str(result_path), "sha256": file_sha256(result_path), "records": len(profile_seen)}
        else:
            raw_files[profile] = {"path": str(result_path), "sha256": None, "records": 0, "reason": "no-selected-cases"}
        missing = set(grouped[profile]) - profile_seen
        require(not missing, "missing result IDs for " + profile + ": " + ", ".join(sorted(missing)[:3]))
        summary = profile_summaries[profile]
        require(integer(summary.get("expected"), profile + ".expected") == len(grouped[profile]) and integer(summary.get("processed"), profile + ".processed") == len(profile_seen), "profile summary count mismatch: " + profile)
        require(counter(summary.get("outcomes"), profile + ".outcomes", OUTCOMES) == dict(local_counts), "profile summary outcomes mismatch: " + profile)
        if grouped[profile]:
            require(summary.get("status") == "complete" and summary.get("complete") is True, "profile is incomplete: " + profile)
        else:
            require(summary.get("status") == "skipped" and summary.get("complete") is None and summary.get("reason") == "no-selected-cases", "empty profile must be explicitly skipped: " + profile)
        failure_path = Path(str(result_path) + ".failures.jsonl")
        if failure_path.exists():
            failure_ids = set()
            for failure in rows(failure_path):
                test, result = failure.get("test"), failure.get("result")
                require(isinstance(test, dict) and isinstance(result, dict), "malformed failure sidecar: " + profile)
                case_id = result.get("id")
                require(isinstance(case_id, str) and case_id in failed_results and case_id not in failure_ids, "stale or duplicate failure sidecar ID: " + str(case_id))
                require(result == failed_results[case_id], "failure sidecar differs from profile result: " + case_id)
                require(all(test.get(field) == cases[case_id][field] for field in ("id", "path", "variant", "inputKind", "mode", "sourceSha256"))
                        and test.get("expectAccept") is cases[case_id]["expectAccept"]
                        and type(test.get("webCompatibility")) is bool and test["webCompatibility"] is cases[case_id]["webCompatibility"],
                        "failure sidecar source differs from input: " + case_id)
                require(isinstance(test.get("source"), str) and hashlib.sha256(test["source"].encode("utf-8")).hexdigest() == cases[case_id]["sourceSha256"],
                        "failure sidecar source hash mismatch: " + case_id)
                failure_ids.add(case_id)
            require(failure_ids == set(failed_results), "failure sidecar is missing failed results: " + profile)
            raw_files[profile]["failureSidecar"] = {"path": str(failure_path), "sha256": file_sha256(failure_path), "records": len(failure_ids)}
        accepted = sum(cases[case_id]["expectAccept"] for case_id in grouped[profile])
        profile_reports[profile] = {"variants": len(profile_seen), "expectedAccept": accepted,
                                    "expectedReject": len(grouped[profile]) - accepted,
                                    "classifications": dict(sorted(Counter(cases[case_id]["classification"] for case_id in grouped[profile]).items())),
                                    "outcomes": dict(sorted(local_counts.items())), "stages": dict(sorted(local_stages.items())),
                                    "status": summary["status"]}
        totals.update(local_counts)
        stages.update(local_stages)
    require(seen == set(cases), "run does not cover every input ID")
    require(integer(run_summary.get("processed"), "summary.processed") == len(seen), "run summary processed count mismatch")
    require(counter(run_summary.get("outcomes"), "summary.outcomes", OUTCOMES) == dict(totals), "run summary total outcomes mismatch")
    for path, initial_hash in initial_hashes.items():
        require((file_sha256(path) if path.is_file() else None) == initial_hash, "evidence changed during verification: " + str(path))
    passed = set(totals) == {"pass"}
    report = {
        "schemaVersion": 1, "createdUtc": datetime.now(timezone.utc).isoformat(),
        "scope": "source-only syntax and static early errors; no JavaScript evaluation, module resolution, or dynamic-string compilation",
        "complete": True, "passed": passed,
        "input": {"path": str(input_path), "sha256": input_hash, "manifestPath": str(manifest_path), "manifestSha256": file_sha256(manifest_path),
                  "snapshot": snapshot, "includePrefixes": source_manifest.get("includePrefixes"), "featureFilter": source_manifest.get("featureFilter"),
                  "preparationCounts": source_counts},
        "run": {"directory": str(results_dir), "manifestSha256": file_sha256(run_path), "summarySha256": file_sha256(summary_path),
                "runner": runner, "binarySha256": hashes, "binaryFilesVerified": binary_dir is not None,
                "timeoutMs": run_manifest["timeoutMs"], "elapsedSeconds": run_summary.get("elapsedSeconds")},
        "counts": {"variants": len(cases), "sourceFiles": len({case["path"] for case in cases.values()}),
                   "expectedAccept": expected_accept, "expectedReject": len(cases) - expected_accept,
                   "classifications": dict(sorted(classifications.items())), "outcomes": dict(sorted(totals.items())), "stages": dict(sorted(stages.items()))},
        "checks": {"duplicateInputIds": 0, "duplicateResultIds": 0, "missingResults": 0,
                   "evidenceChangedDuringVerification": 0,
                   "unexpectedAccept": totals["unexpected-accept"], "unexpectedReject": totals["unexpected-reject"],
                   "unexpectedExceptions": totals["error"], "timeouts": totals["timeout"], "crashes": totals["crash"],
                   "syntaxExceptionRejections": syntax_exception_rejections},
        "profiles": profile_reports, "rawResults": raw_files,
    }
    output.parent.mkdir(parents=True, exist_ok=True)
    output.write_text(json.dumps(report, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")
    return report


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--input", type=Path, required=True)
    parser.add_argument("--manifest", type=Path, help="defaults to the input's adjacent .manifest.json")
    parser.add_argument("--results-dir", "--run-dir", type=Path, required=True, dest="results_dir")
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--binary-dir", type=Path, help="also verify every recorded DLL/JSON checksum against these actual files")
    options = parser.parse_args(argv)
    try:
        report = summarize(options.input, options.results_dir, options.output, manifest_path=options.manifest, binary_dir=options.binary_dir)
    except (ValueError, OSError, UnicodeError) as error:
        parser.error(str(error))
    print(json.dumps({"output": str(options.output.resolve()), "complete": report["complete"], "passed": report["passed"], "counts": report["counts"], "checks": report["checks"]}, indent=2))
    return 0 if report["passed"] else 1


if __name__ == "__main__":
    sys.exit(main())
