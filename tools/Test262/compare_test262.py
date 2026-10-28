#!/usr/bin/env python3
"""Create a reproducible Test262 delta without changing either source corpus.

Only cases with identical ID, source hash, expectation, input kind, mode,
web-compatibility setting, and variant are eligible to reuse an earlier result.
The caller must separately verify that the product binaries are unchanged.
"""

from __future__ import annotations

import argparse
from collections import Counter
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import sys


SIGNATURE_FIELDS = ("id", "sourceSha256", "expectAccept", "inputKind", "mode", "webCompatibility", "variant")


def file_sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def line(value: object) -> str:
    return json.dumps(value, ensure_ascii=True, separators=(",", ":")) + "\n"


def signature(case: dict) -> tuple:
    if not isinstance(case, dict):
        raise ValueError("case must be an object")
    for field in ("id", "path", "sourceSha256", "inputKind", "mode", "variant"):
        if not isinstance(case.get(field), str) or not case[field]:
            raise ValueError("case requires a nonempty string field: " + field)
    for field in ("expectAccept", "webCompatibility"):
        if not isinstance(case.get(field), bool):
            raise ValueError("case requires a boolean field: " + field)
    if case["inputKind"] not in {"Script", "Module"} or case["mode"] not in {"Strict", "NonStrict"}:
        raise ValueError("unknown inputKind or mode: " + case["id"])
    if not isinstance(case.get("source"), str):
        raise ValueError("case requires source text: " + case["id"])
    actual_hash = hashlib.sha256(case["source"].encode("utf-8")).hexdigest()
    if case["sourceSha256"] != actual_hash:
        raise ValueError("source hash mismatch: " + case["id"])
    return tuple(case[field] for field in SIGNATURE_FIELDS)


def records(path: Path):
    with path.open(encoding="utf-8-sig") as stream:
        for number, text in enumerate(stream, 1):
            if text.strip():
                try:
                    case = json.loads(text)
                    identity = signature(case)
                except (ValueError, UnicodeError) as error:
                    raise ValueError(f"{path}:{number}: {error}") from error
                yield case, identity


def corpus_provenance(path: Path) -> dict:
    checksum = file_sha256(path)
    result = {"path": str(path.resolve()), "sha256": checksum}
    manifest_path = path.with_suffix(".manifest.json")
    if manifest_path.exists():
        manifest = json.loads(manifest_path.read_text(encoding="utf-8-sig"))
        if manifest.get("casesSha256") != checksum:
            raise ValueError("corpus manifest hash mismatch: " + str(path))
        result.update(manifestPath=str(manifest_path.resolve()), manifestSha256=file_sha256(manifest_path),
                      snapshot=manifest.get("snapshot"), featureFilter=manifest.get("featureFilter"))
    return result


def compare(baseline: Path, latest: Path, output: Path) -> dict:
    baseline = baseline.resolve()
    latest = latest.resolve()
    output = output.resolve()
    audit_path = output.with_suffix(".comparison.jsonl")
    manifest_path = output.with_suffix(".manifest.json")
    temporary_output = output.with_name(output.name + ".tmp")
    temporary_audit = audit_path.with_name(audit_path.name + ".tmp")
    protected = {baseline, latest, baseline.with_suffix(".manifest.json"), latest.with_suffix(".manifest.json")}
    if protected & {output, audit_path, manifest_path, temporary_output, temporary_audit}:
        raise ValueError("comparison outputs must not overwrite either input corpus or its manifest")
    baseline_info = corpus_provenance(baseline)
    latest_info = corpus_provenance(latest)
    before = {}
    for case, identity in records(baseline):
        if case["id"] in before:
            raise ValueError("duplicate baseline case ID: " + case["id"])
        before[case["id"]] = identity
    output.parent.mkdir(parents=True, exist_ok=True)
    counts = Counter(baselineVariants=len(before))
    profile_counts = {"added": Counter(), "changed": Counter(), "reusable": Counter()}
    changed_fields = Counter()
    seen = set()
    try:
        with temporary_output.open("w", encoding="utf-8", newline="\n") as delta, temporary_audit.open("w", encoding="utf-8", newline="\n") as audit:
            for case, identity in records(latest):
                case_id = case["id"]
                if case_id in seen:
                    raise ValueError("duplicate latest case ID: " + case_id)
                seen.add(case_id)
                counts["latestVariants"] += 1
                old = before.get(case_id)
                status = "added" if old is None else "reusable" if old == identity else "changed"
                counts[status + "Variants"] += 1
                profile = "module" if case["inputKind"] == "Module" else "script-strict" if case["mode"] == "Strict" else "script-nonstrict"
                profile_counts[status][profile] += 1
                record = {"id": case_id, "path": case["path"], "status": status,
                          "latestSignature": dict(zip(SIGNATURE_FIELDS, identity))}
                if status == "changed":
                    fields = [field for field, old_value, new_value in zip(SIGNATURE_FIELDS, old, identity) if old_value != new_value]
                    record.update(changedFields=fields, baselineSignature=dict(zip(SIGNATURE_FIELDS, old)))
                    changed_fields.update(fields)
                if status != "reusable":
                    delta.write(line(case))
                    counts["deltaVariants"] += 1
                audit.write(line(record))
            for case_id in sorted(set(before) - seen):
                counts["removedVariants"] += 1
                audit.write(line({"id": case_id, "status": "removed", "baselineSignature": dict(zip(SIGNATURE_FIELDS, before[case_id]))}))
        # Publish complete files only after validating every case and checksum.
        temporary_output.replace(output)
        temporary_audit.replace(audit_path)
    except BaseException:
        temporary_output.unlink(missing_ok=True)
        temporary_audit.unlink(missing_ok=True)
        raise
    for field in ("latestVariants", "addedVariants", "changedVariants", "reusableVariants", "removedVariants", "deltaVariants"):
        counts.setdefault(field, 0)
    manifest = {
        "schemaVersion": 1, "createdUtc": datetime.now(timezone.utc).isoformat(),
        "kind": "test262-corpus-delta", "baseline": baseline_info, "latest": latest_info,
        "signatureFields": list(SIGNATURE_FIELDS), "counts": dict(sorted(counts.items())),
        "profileCounts": {key: dict(sorted(value.items())) for key, value in profile_counts.items()},
        "changedFieldCounts": dict(sorted(changed_fields.items())),
        "output": str(output), "casesSha256": file_sha256(output),
        "comparisonAudit": str(audit_path), "comparisonAuditSha256": file_sha256(audit_path),
        "reuseCondition": "case signature identical; product/runner binaries and execution options must also match the earlier run before reusing its result",
    }
    manifest_path.write_text(json.dumps(manifest, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")
    return manifest


def main(argv=None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--baseline", type=Path, required=True)
    parser.add_argument("--latest", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True, help="delta JSONL; adjacent manifest and per-ID comparison audit are written")
    options = parser.parse_args(argv)
    try:
        result = compare(options.baseline, options.latest, options.output)
    except (ValueError, OSError, UnicodeError) as error:
        parser.error(str(error))
    print(json.dumps({"output": result["output"], "counts": result["counts"], "casesSha256": result["casesSha256"]}, indent=2))
    return 0


if __name__ == "__main__":
    sys.exit(main())
