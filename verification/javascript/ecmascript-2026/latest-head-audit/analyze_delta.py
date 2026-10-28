import hashlib
import importlib.util
import json
from collections import Counter
from pathlib import Path
import zipfile

here = Path(__file__).resolve().parent
repo = here.parents[2]
spec = importlib.util.spec_from_file_location("prepare_test262", repo / "tools/Test262/prepare_test262.py")
prepare = importlib.util.module_from_spec(spec)
spec.loader.exec_module(prepare)
base_sha = prepare.PINNED_COMMIT
head_sha = "d53ebc8b1931d9f1541e43d59650776e4b766248"
base_path = here.parent / ("test262-" + base_sha + ".zip")
head_path = here / ("test262-" + head_sha + ".zip")
excluded = prepare.feature_exclusions(repo / "tools/Test262/ecmascript-2026-features.json")
counts = Counter()
variants = Counter()
feature_counts = Counter()
scope_counts = Counter()
phase_counts = Counter()
rows = []
delta_cases = []


def files(bundle):
    return {name.split("/", 1)[1]: name for name in bundle.namelist() if "/" in name and not name.endswith("/")}


def parse(bundle, name):
    raw = bundle.read(name)
    source = raw.decode("utf-8")
    metadata = prepare.frontmatter(source)
    features = prepare.string_list(metadata, "features")
    flags = prepare.string_list(metadata, "flags")
    accept, classification, phase, error_type = prepare.expected_acceptance(metadata)
    cases = prepare.variants(source, flags)
    body = source[prepare.FRONTMATTER.search(source).end():]
    return raw, source, metadata, features, flags, accept, classification, phase, error_type, cases, body


with zipfile.ZipFile(base_path) as base, zipfile.ZipFile(head_path) as head:
    before = files(base)
    after = files(head)
    added = sorted(set(after) - set(before))
    removed = sorted(set(before) - set(after))
    modified = sorted(path for path in set(before) & set(after) if base.getinfo(before[path]).CRC != head.getinfo(after[path]).CRC or base.getinfo(before[path]).file_size != head.getinfo(after[path]).file_size)
    counts.update(addedFiles=len(added), removedFiles=len(removed), modifiedFiles=len(modified))
    base_features = Counter()
    head_features = Counter()
    for bundle, mapping, target in ((base, before, base_features), (head, after, head_features)):
        for full_path, zip_name in mapping.items():
            if not full_path.startswith("test/") or not full_path.endswith(".js"):
                continue
            path = full_path.removeprefix("test/")
            if prepare.path_exclusion(path, prepare.DEFAULT_PREFIXES):
                continue
            target.update(set(parse(bundle, zip_name)[3]))
    for status, paths in (("added", added), ("modified", modified), ("removed", removed)):
        for full_path in paths:
            if not full_path.startswith("test/") or not full_path.endswith(".js"):
                continue
            counts["changedJavaScriptFiles"] += 1
            path = full_path.removeprefix("test/")
            reason = prepare.path_exclusion(path, prepare.DEFAULT_PREFIXES)
            if reason:
                counts["outsideScope:" + reason] += 1
                continue
            data = parse(base if status == "removed" else head, before[full_path] if status == "removed" else after[full_path])
            raw, source, metadata, features, flags, accept, classification, phase, error_type, cases, body = data
            row = {
                "path": path, "status": status, "features": features, "flags": flags,
                "negativePhase": phase, "negativeType": error_type, "esid": metadata.get("esid"),
                "expectAccept": accept, "description": metadata.get("description"),
                "excludedFeatures": sorted(set(features) & set(excluded)),
                "sha256": hashlib.sha256(raw).hexdigest(),
                "variants": len(cases),
            }
            if status == "modified":
                old = parse(base, before[full_path])
                row["bodyChanged"] = old[10] != body
                row["flagsChanged"] = old[4] != flags
                row["expectationChanged"] = old[5:9] != data[5:9]
                row["featuresChanged"] = old[3] != features
            rows.append(row)
            if row["excludedFeatures"]:
                counts["excludedFutureSyntaxFiles:" + status] += 1
                continue
            counts["editionEligibleFiles:" + status] += 1
            feature_counts.update(set(features))
            phase_counts[phase or "positive"] += 1
            scope = "/".join(path.split("/")[:3])
            scope_counts[scope] += 1
            if status == "removed":
                continue
            if status == "modified" and not row["bodyChanged"] and not row["flagsChanged"] and not row["expectationChanged"]:
                counts["editionEligibleModifiedMetadataOnly"] += 1
            else:
                counts["editionEligibleActionableFiles"] += 1
                for variant, input_kind, mode, text in cases:
                    variants[variant] += 1
                    counts["actionableVariants"] += 1
                    counts["actionableExpectedAccept" if accept else "actionableExpectedReject"] += 1
                    delta_cases.append({
                        "id": path + "#" + variant, "path": path, "variant": variant,
                        "inputKind": input_kind, "mode": mode, "webCompatibility": True,
                        "expectAccept": accept, "source": text, "features": features,
                        "esid": metadata.get("esid"), "description": metadata.get("description"),
                        "classification": classification, "negativePhase": phase, "negativeType": error_type,
                        "sourceSha256": prepare.sha256(text.encode("utf-8")), "originalSha256": prepare.sha256(raw),
                    })
    summary = {
        "baseCommit": base_sha, "headCommit": head_sha,
        "headCommitDate": "2026-10-13T22:44:04Z", "aheadCommits": 74,
        "headArchiveSha256": prepare.file_sha256(head_path),
        "counts": dict(sorted(counts.items())), "actionableVariantCounts": dict(variants),
        "changedEligiblePhaseCounts": dict(phase_counts),
        "changedEligibleFeatureFileCounts": dict(sorted(feature_counts.items())),
        "changedEligibleScopeFileCounts": dict(sorted(scope_counts.items())),
        "newFeatureTags": {feature: head_features[feature] for feature in sorted(set(head_features) - set(base_features))},
        "removedFeatureTags": sorted(set(base_features) - set(head_features)),
        "allChanges": {"added": added, "removed": removed, "modified": modified},
    }
    (here / "summary.json").write_text(json.dumps(summary, indent=2) + "\n", encoding="utf-8")
    (here / "changed-candidate-files.json").write_text(json.dumps(rows, indent=2) + "\n", encoding="utf-8")
    with (here / "actionable-delta-cases.jsonl").open("w", encoding="utf-8", newline="\n") as stream:
        for case in delta_cases:
            stream.write(prepare.json_line(case))
    print(json.dumps({key: value for key, value in summary.items() if key != "allChanges"}, indent=2))
