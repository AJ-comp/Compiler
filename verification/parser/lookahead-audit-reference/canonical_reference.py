"""Independent canonical LR(1) closure/goto, followed by LR(0)-core LALR merge.

This does not import any Janglim algorithm, PredEx, FIRST, FOLLOW or table builder.
The only input from Janglim is a serialized normalized grammar and its observed
states, so the results can also be inspected independently of product execution.
"""
import json
import sys
import hashlib
from collections import defaultdict, deque
from pathlib import Path

BASE = Path(__file__).resolve().parent
prefix = sys.argv[2] if len(sys.argv) > 2 else "canonical"
raw = (Path(sys.argv[1]) if len(sys.argv) > 1 else BASE / "fresh-dump.log").read_text(encoding="utf-8-sig")
data = json.loads(raw[raw.index("{"):])
(BASE / (prefix + "-dump.json")).write_text(json.dumps(data, indent=2), encoding="utf-8")
rules = data["rules"]
by_lhs = defaultdict(list)
for rule in rules:
    by_lhs[rule["lhs"]].append(rule["id"])
nonterminals = set(by_lhs)
terminals = {s for r in rules for s in r["rhs"] if s not in nonterminals} | {"$"}
first = {s: {s} for s in terminals}
first.update({s: set() for s in nonterminals})
nullable = set()
changed = True
while changed:
    changed = False
    for r in rules:
        lhs, rhs = r["lhs"], r["rhs"]
        old = len(first[lhs])
        for symbol in rhs:
            first[lhs] |= first[symbol]
            if symbol not in nullable:
                break
        else:
            if lhs not in nullable:
                nullable.add(lhs)
                changed = True
        changed |= old != len(first[lhs])

def first_sequence(sequence):
    answer = set()
    for s in sequence:
        answer |= first[s]
        if s not in nullable:
            break
    return answer

start = next(r["id"] for r in rules if r["lhs"] == "N:Accept")
follow = {s: set() for s in nonterminals}
follow[rules[start]["lhs"]].add("$")
changed = True
while changed:
    changed = False
    for r in rules:
        for i, symbol in enumerate(r["rhs"]):
            if symbol not in nonterminals:
                continue
            old = len(follow[symbol])
            suffix = r["rhs"][i + 1:]
            follow[symbol] |= first_sequence(suffix)
            if all(s in nullable for s in suffix):
                follow[symbol] |= follow[r["lhs"]]
            changed |= old != len(follow[symbol])

def closure(seed):
    result = set(seed)
    pending = deque(seed)
    while pending:
        p, dot, la = pending.popleft()
        rhs = rules[p]["rhs"]
        if dot >= len(rhs) or rhs[dot] not in nonterminals:
            continue
        for q in by_lhs[rhs[dot]]:
            for token in first_sequence(rhs[dot + 1:] + [la]):
                item = (q, 0, token)
                if item not in result:
                    result.add(item)
                    pending.append(item)
    return frozenset(result)

initial = closure({(start, 0, "$")})
states = [initial]
numbers = {initial: 0}
transitions = {}
pending = deque([0])
while pending:
    i = pending.popleft()
    symbols = sorted({rules[p]["rhs"][dot] for p, dot, la in states[i]
                      if dot < len(rules[p]["rhs"])})
    for symbol in symbols:
        advanced = {(p, dot + 1, la) for p, dot, la in states[i]
                    if dot < len(rules[p]["rhs"]) and rules[p]["rhs"][dot] == symbol}
        target = closure(advanced)
        if target not in numbers:
            numbers[target] = len(states)
            states.append(target)
            pending.append(numbers[target])
        transitions[i, symbol] = numbers[target]

def core(state):
    return frozenset((p, dot) for p, dot, la in state)

core_numbers = {}
merged = []
state_to_merged = {}
for i, state in enumerate(states):
    k = core(state)
    if k not in core_numbers:
        core_numbers[k] = len(merged)
        merged.append(defaultdict(set))
    m = core_numbers[k]
    state_to_merged[i] = m
    for p, dot, la in state:
        merged[m][p, dot].add(la)
merged_transitions = {}
for (i, symbol), j in transitions.items():
    key = state_to_merged[i], symbol
    value = state_to_merged[j]
    assert key not in merged_transitions or merged_transitions[key] == value
    merged_transitions[key] = value

comparison = []
missing_state_cores = []
for state in data["states"]:
    k = frozenset((x["rule"], x["dot"]) for x in state["items"])
    if k not in core_numbers:
        missing_state_cores.append(state["id"])
        continue
    m = core_numbers[k]
    for item in state["items"]:
        if item["lookahead"] is None:
            continue
        reference = merged[m][item["rule"], item["dot"]]
        actual = set(item["lookahead"])
        if reference != actual:
            comparison.append({"original_state": state["id"], "rule": item["rule"],
                               "dot": item["dot"], "lhs": rules[item["rule"]]["lhs"],
                               "rhs": rules[item["rule"]]["rhs"],
                               "actual": sorted(actual), "reference": sorted(reference),
                               "missing": sorted(reference - actual), "extra": sorted(actual - reference)})

first_differences = []
follow_differences = []
for nt, actual in data["first"].items():
    expected = first[nt] | ({"EPS"} if nt in nullable else set())
    if set(actual) != expected:
        first_differences.append({"nt": nt, "actual": actual, "reference": sorted(expected)})
for nt, actual in data["follow"].items():
    if set(actual) != follow[nt]:
        follow_differences.append({"nt": nt, "actual": actual, "reference": sorted(follow[nt])})

actions = defaultdict(set)
for m, items in enumerate(merged):
    for (p, dot), lookaheads in items.items():
        r = rules[p]
        if dot < len(r["rhs"]):
            symbol = r["rhs"][dot]
            if symbol in terminals:
                actions[m, symbol].add(("shift", merged_transitions[m, symbol]))
        elif p == start:
            actions[m, "$"].add(("accept", 0))
        else:
            for la in lookaheads:
                actions[m, la].add(("reduce", p))

stream = [next(s for s in terminals if s.startswith("T:id:") or s.endswith(":a")), "$"]
stack, index, trace = [state_to_merged[0]], 0, []
for _ in range(100):
    token = stream[index]
    choices = actions[stack[-1], token]
    choices = {a for a in choices if not (a[0] == "reduce" and rules[a[1]]["predicate"] == "Never")}
    assert len(choices) == 1, (stack, token, choices)
    action, target = next(iter(choices))
    trace.append({"state": stack[-1], "token": token, "action": action,
                  "rule": rules[target] if action == "reduce" else None})
    if action == "accept":
        accepted = True
        break
    if action == "shift":
        stack.append(target)
        index += 1
    else:
        n = len(rules[target]["rhs"])
        if n:
            del stack[-n:]
        stack.append(merged_transitions[stack[-1], rules[target]["lhs"]])
else:
    raise AssertionError("reference parser did not terminate")

report = {
    "canonical_lr1_states": len(states), "merged_lalr_states": len(merged),
    "original_lr0_states": len(data["states"]), "unmatched_lr0_cores": missing_state_cores,
    "first_differences": first_differences, "follow_differences": follow_differences,
    "hash_collisions": data["collisions"], "lookahead_differences": comparison,
    "reference_accepts_a": accepted, "original_accepts_a": data["parse"]["success"],
    "reference_trace": trace,
    "original_always_calls": data["parse"]["alwaysCalls"],
    "original_never_calls": data["parse"]["neverCalls"],
    "source_assembly": data["sourceAssembly"],
    "source_assembly_sha256": hashlib.sha256(Path(data["sourceAssembly"]).read_bytes()).hexdigest(),
}
import xml.etree.ElementTree as ET
bison_xml = Path(sys.argv[3]) if len(sys.argv) > 3 else BASE.parent / "lookahead-audit-bison" / "explicit-epsilon.xml"
if bison_xml.exists():
    xml = ET.parse(bison_xml).getroot()
    def bison_symbol(value):
        if "N:" + value in nonterminals:
            return "N:" + value
        if value == "$end":
            return "$"
        if value == "ID":
            return next(s for s in terminals if s.startswith("T:id:"))
        if value.startswith("'"):
            v = value[1:-1]
            return next(s for s in terminals if s.endswith(":" + v))
        if value.startswith("G") and value[1:].isdigit():
            return "N:__predicate" + value[1:]
        return "N:" + ("Accept" if value == "$accept" else value)
    structural = {(r["lhs"], tuple(r["rhs"])): r["id"] for r in rules}
    bison_rules = {}
    for r in xml.findall("./grammar/rules/rule"):
        if r.findtext("lhs") == "$accept":
            continue
        lhs = bison_symbol(r.findtext("lhs"))
        rhs = tuple(bison_symbol(s.text) for s in r.findall("./rhs/symbol"))
        bison_rules[int(r.attrib["number"])] = structural[lhs, rhs]
    nonaccept_core_to_merged = {frozenset(k for k in state if k[0] != start): m
                              for m, state in enumerate(merged)}
    bison_comparisons = []
    unmatched = []
    checked = 0
    for state in xml.findall("./automaton/state"):
        elements = [i for i in state.findall("./itemset/item") if int(i.attrib["rule-number"]) in bison_rules]
        keys = frozenset((bison_rules[int(i.attrib["rule-number"])], int(i.attrib["dot"])) for i in elements)
        if not keys:
            continue
        if keys not in nonaccept_core_to_merged:
            unmatched.append(int(state.attrib["number"]))
            continue
        m = nonaccept_core_to_merged[keys]
        for item in elements:
            la_element = item.find("lookaheads")
            if la_element is None:
                continue
            p, dot = bison_rules[int(item.attrib["rule-number"])], int(item.attrib["dot"])
            bison_la = {bison_symbol(s.text) for s in la_element.findall("symbol")}
            our_la = merged[m][p, dot]
            checked += 1
            if bison_la != our_la:
                bison_comparisons.append({"state": int(state.attrib["number"]), "rule": p,
                                          "bison": sorted(bison_la), "canonical_merge": sorted(our_la)})
    report["bison_crosscheck"] = {"source": str(bison_xml), "lookahead_items_checked": checked,
                                  "unmatched_nonaccept_state_cores": unmatched,
                                  "lookahead_differences": bison_comparisons}
(BASE / (prefix + "-reference-report.json")).write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps({k: v for k, v in report.items() if k not in {"reference_trace", "lookahead_differences"}}, indent=2))
print("LOOKAHEAD DIFFERENCES:", len(comparison))
for difference in comparison:
    if difference["original_state"] in {2, 4, 5, 6, 18}:
        print(json.dumps(difference))
