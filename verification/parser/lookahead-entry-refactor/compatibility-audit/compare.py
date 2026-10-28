import hashlib
import json
import statistics
from pathlib import Path

root = Path(__file__).resolve().parent
baseline = [json.loads((root / f"baseline-{i}" / "summary.json").read_text()) for i in range(1, 4)]
changed = [json.loads((root / f"new-{i}" / "summary.json").read_text()) for i in range(1, 4)]

def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

baseline_dlls = {p.name: digest(p) for p in (root / "harness").glob("*.dll")}
new_dlls = {p.name: digest(p) for p in (root / "new-harness").glob("*.dll")}
changed_dlls = [name for name in sorted(baseline_dlls.keys() | new_dlls.keys())
                if baseline_dlls.get(name) != new_dlls.get(name)]
assert changed_dlls == ["Parse.FrontEnd.Parsers.dll"], changed_dlls

semantic_fields = ["grammarAssemblySha256", "apiLines", "apiSha256", "states", "lrItems",
                   "lookaheadEntries", "lookaheadSha256", "tableEntries", "tableSha256"]
all_runs = baseline + changed
for field in semantic_fields:
    assert len({run[field] for run in all_runs}) == 1, field

files = ["api.txt", "lookaheads.txt", "table.txt"]
for name in files:
    hashes = {digest(root / f"{kind}-{i}" / name)
              for kind in ["baseline", "new"] for i in range(1, 4)}
    assert len(hashes) == 1, name

old_median_ms = statistics.median(run["parserMs"] for run in baseline)
new_median_ms = statistics.median(run["parserMs"] for run in changed)
old_median_bytes = statistics.median(run["parserAllocatedBytes"] for run in baseline)
new_median_bytes = statistics.median(run["parserAllocatedBytes"] for run in changed)
report = {
    "status": "all_api_and_table_comparisons_match",
    "method": "Independent isolated .NET reflection and frozen JavaScript strict Script construction; only the parser DLL is replaced.",
    "api_scope": "Externally visible public/protected types and declared constructors/methods/fields/properties/events, with base types, interfaces, generic constraints, parameter modifiers and defaults. Private nested implementation types are excluded.",
    "table_scope": "Every LR(0) item core, every item lookahead set, and every typed ACTION/GOTO entry; state targets normalized by full sorted LR item core hash.",
    "unchangedDllCount": len(baseline_dlls) - 1,
    "changedDlls": changed_dlls,
    "baselineParserSha256": baseline[0]["parserAssemblySha256"],
    "newParserSha256": changed[0]["parserAssemblySha256"],
    "semanticComparison": {field: all_runs[0][field] for field in semantic_fields},
    "exactUtf8FileHashes": {name: digest(root / "baseline-1" / name) for name in files},
    "parserConstruction": {
        "processesPerVersion": 3,
        "baselineMedianMs": old_median_ms,
        "newMedianMs": new_median_ms,
        "timeChangePercent": (new_median_ms / old_median_ms - 1) * 100,
        "baselineMedianAllocatedBytes": old_median_bytes,
        "newMedianAllocatedBytes": new_median_bytes,
        "allocationChangePercent": (new_median_bytes / old_median_bytes - 1) * 100,
        "timingLimitation": "Other validation/build processes may overlap; elapsed time is an indicative smoke measurement, not an isolated statistical benchmark. Allocations cover complete parser construction, including unchanged algorithms.",
    },
    "baselineRuns": baseline,
    "newRuns": changed,
}
(root / "comparison-report.json").write_text(json.dumps(report, indent=2) + "\n", encoding="utf-8")
print(json.dumps({"status": report["status"], "apiLines": all_runs[0]["apiLines"],
                  "lookaheadEntries": all_runs[0]["lookaheadEntries"], "tableEntries": all_runs[0]["tableEntries"],
                  "parserConstruction": report["parserConstruction"]}, indent=2))
