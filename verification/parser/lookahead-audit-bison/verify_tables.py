"""Execute the enabled actions in GNU Bison's generated XML tables.

This diagnostic does not import Janglim or alter its parser/table/source.
The XML includes explicit reductions (lr.default-reduction=accepting).
"""
import json
import xml.etree.ElementTree as ET
from pathlib import Path


def simulate(path, tokens):
    root = ET.parse(path).getroot()
    rules = {}
    for rule in root.findall("./grammar/rules/rule"):
        rules[int(rule.attrib["number"])] = (
            rule.findtext("lhs"), [s.text for s in rule.findall("./rhs/symbol")]
        )
    states = {int(s.attrib["number"]): s for s in root.findall("./automaton/state")}
    stack, position, trace = [0], 0, []
    for _ in range(10000):
        state = states[stack[-1]]
        token = tokens[position] if position < len(tokens) else "$end"
        shifts = [t for t in state.findall("./actions/transitions/transition")
                  if t.attrib["type"] == "shift" and t.attrib["symbol"] == token]
        reductions = [r for r in state.findall("./actions/reductions/reduction")
                      if r.attrib["enabled"] == "true" and r.attrib["symbol"] == token]
        if not shifts and not reductions:
            reductions = [r for r in state.findall("./actions/reductions/reduction")
                          if r.attrib["enabled"] == "true" and r.attrib["symbol"] == "$default"]
        assert len(shifts) + len(reductions) <= 1, (path, stack[-1], token)
        if shifts:
            target = int(shifts[0].attrib["state"])
            trace.append({"state": stack[-1], "token": token, "action": "shift", "next": target})
            stack.append(target)
            position += 1
        elif reductions:
            rule = reductions[0].attrib["rule"]
            if rule == "accept":
                trace.append({"state": stack[-1], "token": token, "action": "accept"})
                return {"accepted": True, "trace": trace}
            lhs, rhs = rules[int(rule)]
            trace.append({"state": stack[-1], "token": token, "action": "reduce", "lhs": lhs, "rhs": rhs})
            if rhs:
                del stack[-len(rhs):]
            transitions = [t for t in states[stack[-1]].findall("./actions/transitions/transition")
                           if t.attrib["type"] == "goto" and t.attrib["symbol"] == lhs]
            assert len(transitions) == 1
            stack.append(int(transitions[0].attrib["state"]))
        else:
            return {"accepted": False, "state": stack[-1], "token": token, "trace": trace}
    raise AssertionError("XML table simulation did not terminate")


directory = Path(__file__).resolve().parent
reports = {}
for name, tokens in [("explicit-epsilon", ["ID"]), ("plain-bnf", ["ID"]),
                     ("zero-conflict-epsilon", ["'a'"]), ("tiny-bnf", ["'a'"])]:
    reports[name] = simulate(directory / (name + ".xml"), tokens)
    assert reports[name]["accepted"], name
    if name not in ("plain-bnf", "tiny-bnf"):
        assert any(t.get("lhs") == "G5" and t["token"] == "$end"
                   for t in reports[name]["trace"]), name
    if name == "tiny-bnf":
        assert any(t.get("lhs") == "R" and t.get("rhs") == ["'a'"] and t["token"] == "$end"
                   for t in reports[name]["trace"]), name
report = {"generator": "GNU Bison 3.8.2 / Win flex-bison 2.5.25", "tableType": "lalr",
          "input": "a", "reports": reports}
(directory / "bison-table-verification.json").write_text(json.dumps(report, indent=2), encoding="utf-8")
for name, result in reports.items():
    print(name, "accepted=", result["accepted"], "steps=", len(result["trace"]))
    for step in result["trace"]:
        if step.get("lhs") == "G5":
            print("  after a:", step)
