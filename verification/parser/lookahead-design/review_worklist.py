"""Compare the isolated design model with canonical LR(1)->LALR and Bison."""
import contextlib
import io
import json
import runpy
import sys
from pathlib import Path

root = Path(__file__).resolve().parents[3]
reference = root / "artifacts/typescript/lookahead-audit-reference/canonical_reference.py"
dump = root / "artifacts/typescript/lookahead-audit-reference/plain-small-dump.log"
bison = root / "artifacts/typescript/lookahead-audit-bison/tiny-bnf.xml"
sys.argv = [str(reference), str(dump), "../lookahead-design/independent-design", str(bison)]
with contextlib.redirect_stdout(io.StringIO()):
    expected = runpy.run_path(str(reference))
text = (Path(__file__).resolve().parent / "worklist-probe.json").read_text(encoding="utf-8-sig")
actual = json.loads(text[text.index("{"):])
observed = {(i["state"], i["rule"], i["dot"]): set(i["lookahead"]) for i in actual["items"]}
differences = []
for state in expected["data"]["states"]:
    core = frozenset((i["rule"], i["dot"]) for i in state["items"])
    m = expected["core_numbers"][core]
    for item in state["items"]:
        key = state["id"], item["rule"], item["dot"]
        want = expected["merged"][m][item["rule"], item["dot"]]
        got = observed[key]
        if want != got:
            differences.append({"key": key, "expected": sorted(want), "actual": sorted(got)})
assert actual["originalTableUnchanged"] and actual["originalStillRejectsA"]
assert not differences
assert not expected["report"]["bison_crosscheck"]["lookahead_differences"]
result = {k: actual[k] for k in ["designModelOnly", "originalTableUnchanged", "originalStillRejectsA", "nodes", "edges", "dequeued"]}
result.update(checked_items=len(observed), canonical_lr1_lalr_differences=differences,
              bison_crosscheck=expected["report"]["bison_crosscheck"])
(Path(__file__).resolve().parent / "worklist-review-result.json").write_text(json.dumps(result, indent=2), encoding="utf-8")
print(json.dumps(result, indent=2))
