#!/usr/bin/env python3
"""Audit parse-negative Test262 files outside the main source-syntax selection.

Read the same immutable archive as prepare_test262.py, retain all original bytes
and metadata, and emit only additional parse/SyntaxError cases. Staging tests are
retained as explicitly supplementary evidence, separate from the main corpus.
No JavaScript is executed and no product assembly is loaded.
"""

from __future__ import annotations

import argparse
from collections import Counter
from datetime import datetime, timezone
import json
from pathlib import Path, PurePosixPath
import re
import zipfile

import prepare_test262 as preparation


def extract(root: Path, archive: Path, output: Path, feature_filter: Path) -> dict:
    snapshot = preparation.provenance(root)
    if (not isinstance(snapshot, dict)
            or snapshot.get("repository") != preparation.REPOSITORY
            or not isinstance(snapshot.get("commit"), str)
            or not re.fullmatch(r"[0-9a-f]{40}", snapshot["commit"])
            or not isinstance(snapshot.get("archiveSha256"), str)
            or not re.fullmatch(r"[0-9a-f]{64}", snapshot["archiveSha256"])):
        raise ValueError("pinned official snapshot provenance is required")
    archive_hash = preparation.file_sha256(archive)
    if snapshot.get("archiveSha256") != archive_hash:
        raise ValueError("archive hash differs from the pinned snapshot provenance")
    excluded_features = preparation.feature_exclusions(feature_filter)
    counts = Counter()
    scopes = Counter()
    features = Counter()
    variants = Counter()
    metadata_errors = []
    exclusions = []
    selected = []
    output.parent.mkdir(parents=True, exist_ok=True)
    with zipfile.ZipFile(archive) as source_zip:
        members = []
        paths = set()
        for entry in source_zip.infolist():
            # ZipInfo.filename normalizes backslashes on Windows. Validate the
            # literal archive name, before that normalization hides bad paths.
            member = entry.orig_filename
            parts = PurePosixPath(member).parts
            if ("\\" in member or "\x00" in member or not parts or parts[0] != "test262-" + snapshot["commit"]
                    or ".." in parts or PurePosixPath(member).is_absolute()):
                raise ValueError("archive root differs from the pinned commit or contains an unsafe path")
            if len(parts) < 3 or parts[1] != "test" or not member.endswith(".js"):
                continue
            path = PurePosixPath(*parts[2:]).as_posix()
            if path in paths:
                raise ValueError("duplicate JavaScript path in archive: " + path)
            paths.add(path)
            members.append((path, entry))
        with output.open("w", encoding="utf-8", newline="\n") as cases:
            _extract_members(source_zip, members, cases, excluded_features, counts, scopes,
                             features, variants, metadata_errors, exclusions, selected)
    unexpected_errors = sum(not error["fixture"] for error in metadata_errors)
    report = {
        "schemaVersion": 1, "createdUtc": datetime.now(timezone.utc).isoformat(),
        "complete": unexpected_errors == 0,
        "snapshot": snapshot,
        "archive": {"path": str(archive.resolve()), "sha256": archive_hash},
        "mainSelectionPrefixes": list(preparation.DEFAULT_PREFIXES),
        "featureFilter": {"path": str(feature_filter.resolve()), "sha256": preparation.file_sha256(feature_filter)},
        "sourcePolicy": "Archive UTF-8 bytes decoded without newline conversion; all source comments and metadata retained; only strict variants prepend the official directive; JavaScript is not executed",
        "scopePolicy": "Non-staging parse negatives are additional standard-path coverage; staging parse negatives are explicitly supplementary and are not included in the main Test262 success total",
        "counts": dict(sorted(counts.items())), "scopeFileCounts": dict(sorted(scopes.items())),
        "featureFileCounts": dict(sorted(features.items())), "variantCounts": dict(sorted(variants.items())),
        "selectedFiles": selected, "excludedParseNegativeFiles": exclusions,
        "metadataErrors": metadata_errors,
        "unexpectedMetadataErrorCount": unexpected_errors,
        "output": str(output.resolve()), "casesSha256": preparation.file_sha256(output),
    }
    output.with_suffix(".manifest.json").write_text(json.dumps(report, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")
    return report


def _extract_members(source_zip, members, cases, excluded_features, counts, scopes,
                     features, variants, metadata_errors, exclusions, selected):
    for path, member in sorted(members):
        counts["allJavaScriptFiles"] += 1
        if any(preparation.has_prefix(path, prefix) for prefix in preparation.DEFAULT_PREFIXES):
            counts["alreadyWithinMainSelectionPaths"] += 1
            continue
        counts["outsideMainSelectionPaths"] += 1
        raw = source_zip.read(member)
        try:
            source = raw.decode("utf-8")
            metadata = preparation.frontmatter(source)
            flags = preparation.string_list(metadata, "flags")
            feature_tags = preparation.string_list(metadata, "features")
            includes = preparation.string_list(metadata, "includes")
            if set(flags) - preparation.KNOWN_FLAGS:
                raise ValueError("unknown_flags: " + ", ".join(sorted(set(flags) - preparation.KNOWN_FLAGS)))
            prepared = preparation.variants(source, flags)
            _, classification, _, _ = preparation.expected_acceptance(metadata)
        except (UnicodeDecodeError, ValueError) as error:
            # Auxiliary fixture modules need not have standalone-test metadata.
            reason = "invalid_utf8" if isinstance(error, UnicodeDecodeError) else str(error)
            metadata_errors.append({"path": path, "reason": reason, "fixture": "_FIXTURE" in PurePosixPath(path).name})
            continue
        if classification != "parse-negative":
            counts["outsideFilesWithoutParseSyntaxErrorExpectation"] += 1
            continue
        counts["outsideParseSyntaxErrorFiles"] += 1
        forbidden = sorted(set(feature_tags) & set(excluded_features))
        if forbidden or "_FIXTURE" in PurePosixPath(path).name:
            exclusions.append({"path": path, "features": feature_tags, "reason": "fixture_not_standalone" if not forbidden else "future_syntax_feature", "excludedFeatures": forbidden})
            continue
        scope = "supplementary-staging" if "staging" in PurePosixPath(path).parts else "additional-standard-path"
        original_hash = preparation.sha256(raw)
        selected.append({"path": path, "originalSha256": original_hash, "byteLength": len(raw), "scope": scope, "variantCount": len(prepared)})
        scopes[scope] += 1
        features.update(set(feature_tags))
        for variant, input_kind, mode, text in prepared:
            ordinal = counts["emittedVariants"]
            case = {
                "id": path + "#" + variant, "path": path, "variant": variant,
                "inputKind": input_kind, "mode": mode, "webCompatibility": True,
                "expectAccept": False, "source": text, "features": feature_tags,
                "flags": flags, "includes": includes, "esid": metadata.get("esid"),
                "description": metadata.get("description"), "classification": "parse-negative",
                "negativePhase": "parse", "negativeType": "SyntaxError",
                "originalSha256": original_hash, "sourceSha256": preparation.sha256(text.encode("utf-8")),
                "strictPrefixLength": len(preparation.STRICT_PREFIX) if variant == "strict" else 0,
                "ordinal": ordinal, "auditScope": scope,
            }
            cases.write(preparation.json_line(case))
            counts["emittedVariants"] += 1
            variants[variant] += 1


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, required=True)
    parser.add_argument("--archive", type=Path, required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--feature-filter", type=Path, default=Path(__file__).with_name("ecmascript-2026-features.json"))
    arguments = parser.parse_args()
    result = extract(arguments.root, arguments.archive, arguments.output, arguments.feature_filter)
    print(json.dumps({key: result[key] for key in ("counts", "scopeFileCounts", "variantCounts", "unexpectedMetadataErrorCount", "casesSha256")}, indent=2))
    return 1 if result["unexpectedMetadataErrorCount"] else 0


if __name__ == "__main__":
    raise SystemExit(main())
