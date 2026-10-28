#!/usr/bin/env python3
"""Prepare source-only Test262 parse/early-error cases without executing JavaScript.

The default snapshot is the last official main-branch commit before the end of
2026-06-30 UTC. Test262 also contains proposals, so its feature tags are retained
and counted, not silently treated as ECMAScript 2026 conformance coverage.

Examples (run from the repository root):
  python tools/Test262/prepare_test262.py --download --root artifacts/ecmascript-2026/test262 --output artifacts/ecmascript-2026/cases.jsonl
  python tools/Test262/prepare_test262.py --root artifacts/ecmascript-2026/test262 --output artifacts/ecmascript-2026/modules.jsonl --include-prefix language/module-code

Requires PyYAML. The source text is decoded directly from UTF-8 bytes; CR, LF,
BOM, copyright comments, frontmatter, and all test-body characters are retained.
Only the official strict-mode variant adds a leading use-strict directive.
Harness includes are metadata only: this tool validates source syntax, not test
execution, module resolution, or runtime assertions.
"""

from __future__ import annotations

import argparse
from collections import Counter
from contextlib import nullcontext
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path, PurePosixPath
import re
import shutil
import subprocess
import sys
import urllib.request
import zipfile

import yaml


SAFE_YAML_LOADER = getattr(yaml, "CSafeLoader", yaml.SafeLoader)
PINNED_COMMIT = "6eec1ac9ee144dafd8f344d73a21f36bfc9f6755"
PINNED_COMMIT_DATE = "2026-06-29T14:09:27Z"
REPOSITORY = "https://github.com/tc39/test262"
DEFAULT_PREFIXES = ("language", "annexB", "built-ins/RegExp")
STRICT_PREFIX = '"use strict";\n'
FRONTMATTER = re.compile(r"/\*---(.*?)---\*/", re.DOTALL)
VARIANT_FLAGS = {"module", "raw", "noStrict", "onlyStrict"}
KNOWN_FLAGS = VARIANT_FLAGS | {
    "async", "generated", "non-deterministic", "CanBlockIsTrue", "CanBlockIsFalse",
}


def sha256(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def file_sha256(path: Path) -> str:
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(block)
    return digest.hexdigest()


def json_line(value: object) -> str:
    # ASCII escaping also preserves lone surrogate escapes if a fixture has one.
    return json.dumps(value, ensure_ascii=True, separators=(",", ":")) + "\n"


def frontmatter(source: str) -> dict:
    match = FRONTMATTER.search(source)
    if match is None:
        raise ValueError("missing_frontmatter")
    try:
        value = yaml.load(match.group(1), Loader=SAFE_YAML_LOADER)
    except yaml.YAMLError as error:
        raise ValueError("invalid_frontmatter_yaml") from error
    if not isinstance(value, dict):
        raise ValueError("frontmatter_is_not_mapping")
    return value


def string_list(metadata: dict, key: str) -> list[str]:
    value = metadata.get(key, [])
    if value is None:
        return []
    if not isinstance(value, list) or any(not isinstance(item, str) for item in value):
        raise ValueError("invalid_" + key)
    return value


def expected_acceptance(metadata: dict) -> tuple[bool, str, str | None, str | None]:
    """A runtime/resolution negative must still be valid source syntax."""
    negative = metadata.get("negative")
    if negative is None:
        return True, "positive", None, None
    if not isinstance(negative, dict):
        raise ValueError("invalid_negative_metadata")
    phase = negative.get("phase")
    error_type = negative.get("type")
    if not isinstance(phase, str) or not isinstance(error_type, str):
        raise ValueError("invalid_negative_metadata")
    if phase == "parse":
        if error_type != "SyntaxError":
            raise ValueError("parse_negative_is_not_syntax_error")
        return False, "parse-negative", phase, error_type
    if phase in {"resolution", "runtime"}:
        return True, phase + "-negative-source-only", phase, error_type
    raise ValueError("unknown_negative_phase")


def variants(source: str, flags: list[str]) -> list[tuple[str, str, str, str]]:
    """Return (variant, inputKind, mode, source), following INTERPRETING.md."""
    chosen = set(flags) & VARIANT_FLAGS
    if len(chosen) > 1 and chosen != {"module", "raw"}:
        # Module+raw preserves a module exactly (notably its hashbang). Module
        # source remains strict by definition. Other conflicting combinations
        # cannot be interpreted unambiguously and are reported in the audit.
        raise ValueError("conflicting_variant_flags")
    if "module" in chosen:
        return [("module", "Module", "Strict", source)]
    if "raw" in chosen:
        return [("raw", "Script", "NonStrict", source)]
    non_strict = ("non-strict", "Script", "NonStrict", source)
    strict = ("strict", "Script", "Strict", STRICT_PREFIX + source)
    if "noStrict" in chosen:
        return [non_strict]
    if "onlyStrict" in chosen:
        return [strict]
    return [non_strict, strict]


def normalized_prefix(value: str) -> str:
    value = value.replace("\\", "/").removeprefix("test/").strip("/")
    if not value or any(part in {"", ".", ".."} for part in value.split("/")):
        raise ValueError("include prefixes must be paths below test/")
    return value


def has_prefix(path: str, prefix: str) -> bool:
    return path == prefix or path.startswith(prefix + "/")


def path_exclusion(path: str, prefixes: tuple[str, ...]) -> str | None:
    parts = PurePosixPath(path).parts
    if "staging" in parts:
        return "staging_not_standard_corpus"
    if parts and parts[0] == "intl402":
        return "intl402_outside_ecma262"
    if "_FIXTURE" in PurePosixPath(path).name:
        return "fixture_not_standalone"
    if not any(has_prefix(path, prefix) for prefix in prefixes):
        return "outside_selected_prefixes"
    return None


def feature_exclusions(path: Path) -> dict[str, str]:
    """Read an edition filter whose exclusions each carry an auditable reason."""
    value = json.loads(path.read_text(encoding="utf-8-sig"))
    if isinstance(value, dict) and "excludedFeatures" in value:
        value = value["excludedFeatures"]
    if not isinstance(value, dict) or any(
        not isinstance(key, str) or not key or not isinstance(reason, str) or not reason.strip()
        for key, reason in value.items()
    ):
        raise ValueError("feature filter must map feature tags to nonempty exclusion reasons")
    return value


def official_commit_date(commit: str) -> str:
    request = urllib.request.Request(
        "https://api.github.com/repos/tc39/test262/commits/" + commit,
        headers={"Accept": "application/vnd.github+json", "User-Agent": "Janglim-Test262-corpus-preparation"},
    )
    with urllib.request.urlopen(request) as response:
        metadata = json.load(response)
    if metadata.get("sha") != commit:
        raise ValueError("official commit metadata does not match requested revision")
    value = metadata.get("commit", {}).get("committer", {}).get("date")
    if not isinstance(value, str) or not value:
        raise ValueError("official commit metadata is missing its committer date")
    return value


def download_snapshot(root: Path, commit: str = PINNED_COMMIT) -> dict:
    """Extract an official immutable archive, retaining every original byte."""
    if not re.fullmatch(r"[0-9a-f]{40}", commit):
        raise ValueError("revision must be a full lowercase Git commit SHA")
    root = root.resolve()
    if root.exists() and any(root.iterdir()):
        raise ValueError("download root must be absent or empty; existing files are preserved")
    commit_date = PINNED_COMMIT_DATE if commit == PINNED_COMMIT else official_commit_date(commit)
    root.mkdir(parents=True, exist_ok=True)
    archive = root.parent / ("test262-" + commit + ".zip")
    url = "https://codeload.github.com/tc39/test262/zip/" + commit
    if not archive.exists():
        urllib.request.urlretrieve(url, archive)
    with zipfile.ZipFile(archive) as bundle:
        for member in bundle.infolist():
            if "\\" in member.filename:
                raise ValueError("unsafe archive path")
            parts = PurePosixPath(member.filename).parts
            if not parts or parts[0] != "test262-" + commit:
                raise ValueError("archive root does not match the pinned Test262 commit")
            if len(parts) < 2:
                continue
            relative = PurePosixPath(*parts[1:])
            if relative.is_absolute() or ".." in relative.parts:
                raise ValueError("unsafe archive path")
            target = root.joinpath(*relative.parts)
            if member.is_dir():
                target.mkdir(parents=True, exist_ok=True)
            else:
                target.parent.mkdir(parents=True, exist_ok=True)
                with bundle.open(member) as src, target.open("wb") as dst:
                    shutil.copyfileobj(src, dst)
    snapshot = {
        "repository": REPOSITORY,
        "commit": commit,
        "commitDate": commit_date,
        "commitMetadataUrl": "https://api.github.com/repos/tc39/test262/commits/" + commit,
        "selectionCutoff": "2026-06-30T23:59:59Z" if commit == PINNED_COMMIT else None,
        "archiveUrl": url,
        "archiveSha256": file_sha256(archive),
    }
    (root / "test262.snapshot.json").write_text(json.dumps(snapshot, indent=2) + "\n", encoding="utf-8")
    return snapshot


def provenance(root: Path) -> dict:
    snapshot = root / "test262.snapshot.json"
    if snapshot.exists():
        return json.loads(snapshot.read_text(encoding="utf-8"))
    if (root / ".git").exists():
        completed = subprocess.run(
            ["git", "-C", str(root), "rev-parse", "HEAD"], capture_output=True, text=True, check=True,
        )
        dirty = subprocess.run(
            ["git", "-C", str(root), "status", "--porcelain", "--untracked-files=no"],
            capture_output=True, text=True, check=True,
        )
        return {"repository": REPOSITORY, "commit": completed.stdout.strip(), "trackedFilesModified": bool(dirty.stdout)}
    return {"repository": REPOSITORY, "commit": None, "note": "unversioned local input"}


def prepare(
    root: Path,
    output: Path,
    *,
    prefixes: tuple[str, ...] = DEFAULT_PREFIXES,
    exclude_features: frozenset[str] = frozenset(),
    exclusion_reasons: dict[str, str] | None = None,
    feature_filter_path: Path | None = None,
    source_archive: Path | None = None,
    limit: int | None = None,
    offset: int = 0,
    shard_index: int = 0,
    shard_count: int = 1,
) -> dict:
    root = root.resolve()
    test_root = root / "test"
    if not test_root.is_dir():
        raise ValueError("root must contain the Test262 test/ directory")
    if not (0 <= shard_index < shard_count) or offset < 0 or (limit is not None and limit < 0):
        raise ValueError("invalid shard, offset, or limit")
    prefixes = tuple(normalized_prefix(value) for value in prefixes)
    output.parent.mkdir(parents=True, exist_ok=True)
    selection_path = output.with_suffix(".selection.jsonl")
    counts = Counter()
    feature_counts = Counter()
    selected_feature_counts = Counter()
    excluded_feature_counts = Counter()
    excluded_feature_variants = Counter()
    unknown_flags = Counter()
    negative_phases = Counter()
    variant_counts = Counter()
    digest = hashlib.sha256()
    planned_index = 0
    snapshot = provenance(root)
    archive_hash = file_sha256(source_archive) if source_archive else None
    if source_archive and snapshot.get("archiveSha256") and archive_hash != snapshot["archiveSha256"]:
        raise ValueError("source archive hash does not match snapshot provenance")
    exclusion_reasons = dict(exclusion_reasons or {})
    for feature in exclude_features:
        exclusion_reasons.setdefault(feature, "explicit --exclude-feature filter")
    exclude_features = frozenset(exclusion_reasons)
    with (zipfile.ZipFile(source_archive) if source_archive else nullcontext(None)) as source_zip, output.open("w", encoding="utf-8", newline="\n") as cases, selection_path.open("w", encoding="utf-8", newline="\n") as audit:
        # Sort by the case-sensitive portable path, rather than the host OS's
        # Path comparison, so ordinals/shards are reproducible across platforms.
        if source_zip:
            expected_root = "test262-" + snapshot["commit"] if snapshot.get("commit") else None
            test_files = []
            for member in source_zip.namelist():
                parts = PurePosixPath(member).parts
                if len(parts) >= 3 and parts[1] == "test" and member.endswith(".js"):
                    if expected_root and parts[0] != expected_root:
                        raise ValueError("archive root does not match snapshot provenance")
                    test_files.append((PurePosixPath(*parts[2:]).as_posix(), member))
        else:
            test_files = [(file.relative_to(test_root).as_posix(), file) for file in test_root.rglob("*.js")]
        for path, file in sorted(test_files, key=lambda item: item[0]):
            counts["discoveredFiles"] += 1
            exclusion = path_exclusion(path, prefixes)
            if exclusion:
                counts["excluded:" + exclusion] += 1
                audit.write(json_line({"path": path, "status": "excluded", "reason": exclusion}))
                continue
            counts["candidateFiles"] += 1
            raw = source_zip.read(file) if source_zip else file.read_bytes()
            original_hash = sha256(raw)
            digest.update(path.encode("utf-8") + b"\0" + original_hash.encode("ascii") + b"\n")
            counts["candidateSourceBytes"] += len(raw)
            base = {"path": path, "originalSha256": original_hash, "byteLength": len(raw)}
            try:
                source = raw.decode("utf-8")
                metadata = frontmatter(source)
                flags = string_list(metadata, "flags")
                features = string_list(metadata, "features")
                includes = string_list(metadata, "includes")
                accept, classification, phase, error_type = expected_acceptance(metadata)
                prepared = variants(source, flags)
            except (UnicodeDecodeError, ValueError) as error:
                reason = "invalid_utf8" if isinstance(error, UnicodeDecodeError) else str(error)
                counts["excluded:" + reason] += 1
                audit.write(json_line({**base, "status": "excluded", "reason": reason}))
                continue
            feature_counts.update(set(features))
            unknown_flags.update(set(flags) - KNOWN_FLAGS)
            negative_phases[phase or "positive"] += 1
            excluded = sorted(set(features) & exclude_features)
            if excluded:
                counts["excluded:feature_filter"] += 1
                excluded_feature_counts.update(excluded)
                excluded_feature_variants.update({feature: len(prepared) for feature in excluded})
                counts["excludedVariants:feature_filter"] += len(prepared)
                audit.write(json_line({**base, "status": "excluded", "reason": "feature_filter", "excludedFeatures": excluded, "exclusionReasons": {feature: exclusion_reasons[feature] for feature in excluded}, "features": features, "variantCount": len(prepared)}))
                continue
            counts["selectedFiles"] += 1
            selected_feature_counts.update(set(features))
            audit.write(json_line({**base, "status": "selected", "features": features, "flags": flags, "classification": classification, "variantCount": len(prepared)}))
            for variant, input_kind, mode, text in prepared:
                index = planned_index
                planned_index += 1
                counts["plannedVariants"] += 1
                if index < offset:
                    counts["omittedVariants:offset"] += 1
                    continue
                if index % shard_count != shard_index:
                    counts["omittedVariants:shard"] += 1
                    continue
                if limit is not None and counts["emittedVariants"] >= limit:
                    counts["omittedVariants:limit"] += 1
                    continue
                case = {
                    "id": path + "#" + variant,
                    "path": path,
                    "variant": variant,
                    "inputKind": input_kind,
                    "mode": mode,
                    "webCompatibility": True,
                    "expectAccept": accept,
                    "source": text,
                    "features": features,
                    "esid": metadata.get("esid"),
                    "description": metadata.get("description"),
                    "flags": flags,
                    "includes": includes,
                    "classification": classification,
                    "negativePhase": phase,
                    "negativeType": error_type,
                    "originalSha256": original_hash,
                    "sourceSha256": sha256(text.encode("utf-8")),
                    "strictPrefixLength": len(STRICT_PREFIX) if variant == "strict" else 0,
                    "ordinal": index,
                }
                cases.write(json_line(case))
                counts["emittedVariants"] += 1
                counts["expectedAccept" if accept else "expectedReject"] += 1
                variant_counts[variant] += 1
    license_file = root / "LICENSE"
    manifest = {
        "schemaVersion": 1,
        "pythonVersion": sys.version.split()[0],
        "pyYamlVersion": yaml.__version__,
        "createdUtc": datetime.now(timezone.utc).isoformat(),
        "snapshot": snapshot,
        "root": str(root),
        "sourceArchive": {"path": str(source_archive.resolve()), "sha256": archive_hash} if source_archive else None,
        "output": str(output.resolve()),
        "selectionAudit": str(selection_path.resolve()),
        "license": {"path": str(license_file), "sha256": file_sha256(license_file) if license_file.exists() else None},
        "sourcePolicy": "UTF-8 bytes decoded without newline conversion; metadata and copyright retained; only strict variants prepend a directive; no harness injected; no JavaScript executed",
        "expectationPolicy": "parse SyntaxError -> reject; positive, resolution-negative, runtime-negative -> accept source syntax only",
        "featurePolicy": "feature tags are retained and counted; snapshot date is not a standard-edition filter; excluded features are explicit in selection audit",
        "includePrefixes": list(prefixes),
        "excludeFeatures": sorted(exclude_features),
        "featureExclusionReasons": exclusion_reasons,
        "featureFilter": {"path": str(feature_filter_path.resolve()), "sha256": file_sha256(feature_filter_path)} if feature_filter_path else None,
        "offset": offset,
        "limit": limit,
        "shardIndex": shard_index,
        "shardCount": shard_count,
        "counts": dict(sorted(counts.items())),
        "variantCounts": dict(sorted(variant_counts.items())),
        "negativePhaseFileCounts": dict(sorted(negative_phases.items())),
        "featureFileCounts": dict(sorted(feature_counts.items())),
        "selectedFeatureFileCounts": dict(sorted(selected_feature_counts.items())),
        "excludedFeatureFileCounts": dict(sorted(excluded_feature_counts.items())),
        "excludedFeatureVariantCounts": dict(sorted(excluded_feature_variants.items())),
        "unknownFlagFileCounts": dict(sorted(unknown_flags.items())),
        "candidateSourceSetSha256": digest.hexdigest(),
        "casesSha256": file_sha256(output),
        "selectionAuditSha256": file_sha256(selection_path),
    }
    manifest_path = output.with_suffix(".manifest.json")
    manifest_path.write_text(json.dumps(manifest, indent=2, ensure_ascii=True) + "\n", encoding="utf-8")
    return manifest


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    parser.add_argument("--root", type=Path, required=True, help="Test262 checkout/snapshot directory")
    parser.add_argument("--output", type=Path, required=True, help="UTF-8 JSONL case output; adjacent manifest and selection audit are also written")
    parser.add_argument("--download", action="store_true", help="download the official pinned snapshot into an absent/empty --root")
    parser.add_argument("--archive", type=Path, help="read source bytes directly from the official snapshot zip; --root supplies provenance/license; cases and hashes match an unchanged extracted snapshot")
    parser.add_argument("--revision", default=PINNED_COMMIT, help="full immutable commit SHA for --download")
    parser.add_argument("--include-prefix", action="append", help="path below test/; repeatable; default language, annexB, built-ins/RegExp")
    parser.add_argument("--exclude-feature", action="append", default=[], help="explicit feature tag exclusion; recorded in manifest and selection audit")
    parser.add_argument("--exclude-feature-file", type=Path, help='JSON feature-to-reason map, optionally under an "excludedFeatures" key; edition exclusions remain auditable')
    parser.add_argument("--limit", type=int, help="maximum emitted variants; omitted count remains visible")
    parser.add_argument("--offset", type=int, default=0, help="skip this many planned variant ordinals")
    parser.add_argument("--shard-index", type=int, default=0)
    parser.add_argument("--shard-count", type=int, default=1)
    args = parser.parse_args(argv)
    try:
        if args.download:
            download_snapshot(args.root, args.revision)
        manifest = prepare(
            args.root, args.output,
            prefixes=tuple(args.include_prefix or DEFAULT_PREFIXES),
            exclude_features=frozenset(args.exclude_feature), limit=args.limit, offset=args.offset,
            exclusion_reasons=feature_exclusions(args.exclude_feature_file) if args.exclude_feature_file else None,
            feature_filter_path=args.exclude_feature_file,
            source_archive=args.archive,
            shard_index=args.shard_index, shard_count=args.shard_count,
        )
    except (ValueError, OSError, subprocess.SubprocessError, zipfile.BadZipFile) as error:
        parser.error(str(error))
    print(json.dumps({"output": manifest["output"], "snapshot": manifest["snapshot"], "counts": manifest["counts"]}, indent=2))
    return 0


if __name__ == "__main__":
    sys.exit(main())
