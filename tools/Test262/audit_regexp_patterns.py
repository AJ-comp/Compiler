#!/usr/bin/env python3
"""Compare Janglim's existing RegExp Pattern reader with Node, without matching.

Generated probes and static strings extracted from Test262 are independent syntax
probes, not Test262 assertions. A reference-engine mismatch needs specification
review; Node can differ because of Unicode version or implementation limits.
Python stdlib, Node and .NET 8 are sufficient. Product assemblies must exist first.
"""
from __future__ import annotations

import argparse
import base64
import hashlib
import itertools
import json
import random
import re
import subprocess
from pathlib import Path

TOOLS = Path(__file__).resolve().parent
ROOT = TOOLS.parents[1]
STRING = r'''(?:"(?:[^"\\]|\\[\s\S])*"|'(?:[^'\\]|\\[\s\S])*')'''
CONSTRUCTOR = re.compile(
    r"(?<![\w$.])(?:new\s+)?RegExp\s*\(\s*(?P<body>" + STRING +
    r")\s*(?:\)|,\s*(?P<flags>" + STRING + r")\s*\))")


def decode_string(literal):
    """Decode only a statically quoted JS string; never eval input source."""
    source, index, result = literal[1:-1], 0, []
    while index < len(source):
        value = source[index]
        index += 1
        if value != "\\":
            result.append(value)
            continue
        if index == len(source):
            raise ValueError("incomplete escape")
        value = source[index]
        index += 1
        if value in "bfnrtv":
            result.append(dict(zip("bfnrtv", "\b\f\n\r\t\v"))[value])
        elif value in "\r\n\u2028\u2029":
            if value == "\r" and index < len(source) and source[index] == "\n":
                index += 1
        elif value == "x":
            digits = source[index:index + 2]
            if len(digits) != 2 or not re.fullmatch("[0-9A-Fa-f]{2}", digits):
                raise ValueError("invalid hexadecimal escape")
            result.append(chr(int(digits, 16)))
            index += 2
        elif value == "u":
            if index < len(source) and source[index] == "{":
                end = source.index("}", index)
                digits = source[index + 1:end]
                if not re.fullmatch("[0-9A-Fa-f]+", digits):
                    raise ValueError("invalid code-point escape")
                result.append(chr(int(digits, 16)))
                index = end + 1
            else:
                digits = source[index:index + 4]
                if not re.fullmatch("[0-9A-Fa-f]{4}", digits):
                    raise ValueError("invalid Unicode escape")
                result.append(chr(int(digits, 16)))
                index += 4
        elif value in "01234567":
            digits, remaining = value, 2 if value <= "3" else 1
            while remaining and index < len(source) and source[index] in "01234567":
                digits += source[index]
                index += 1
                remaining -= 1
            result.append(chr(int(digits, 8)))
        else:
            result.append(value)
    return "".join(result)


def generated_probes(seed=20261021, random_count=18000):
    cases = {}

    def add(body, group, flags=("", "u", "v")):
        for flag in flags:
            cases.setdefault((body, flag), {"body": body, "flags": flag, "group": group})

    atoms = ["a", ".", r"\d", r"\w", "[a]", "[^a]", "[]", "[^]", "(a)", "(?:a)",
             "(?=a)", "(?!a)", "(?<=a)", "(?<!a)", "(?<x>a)", "(?i:a)", "(?-i:a)",
             "^", "$", r"\b", r"\B", r"\1", r"\0", r"\k<x>", "{", "}", "]", r"\p{L}", r"\P{ASCII}"]
    quantifiers = ["", "*", "+", "?", "*?", "+?", "??", "**", "++", "?+", "{0}", "{1}",
                   "{01}", "{01,0}", "{0,01}", "{1,}", "{1,2}", "{2,1}", "{,1}", "{1,2}?",
                   "{1,2}*", "{999999999999999999999999,888888888888888888888888}",
                   "{999999999999999999999999,}"]
    for atom, quantifier in itertools.product(atoms, quantifiers):
        add(atom + quantifier, "quantifier")
    for point in range(128):
        add("\\" + chr(point), "escape")
    for escape in [r"\x", r"\x0", r"\x00", r"\xgg", r"\u", r"\u0", r"\u0000", r"\uD800",
                   r"\uDC00", r"\uD800\uDC00", r"\u{D800}\u{DC00}", r"\u{0}", r"\u{000001}",
                   r"\u{110000}", r"\u{10FFFF}", r"\c", r"\c0", r"\c_", r"\cA", r"\c[", r"\00",
                   r"\01", r"\08", r"\18", r"\118", r"\999", r"\k", r"\k<", r"\k<>", r"\k<x>",
                   r"\p{}", r"\p{L}", r"\p{gc=L}", r"\p{Script=Latin}", r"\p{sc=Latn}",
                   r"\p{Basic_Emoji}", r"\P{Basic_Emoji}", r"\p{ASCII=Yes}"]:
        add(escape, "escape-boundary")
    class_atoms = ["a", "z", "-", r"\-", r"\d", r"\w", r"\b", r"\B", r"\p{L}", r"\P{L}",
                   r"\p{Basic_Emoji}", r"\q{a}", r"\q{ab}", r"\q{a|b}", r"\q{}", "[a]", "[^a]",
                   "[]", r"\c", r"\c0", r"\x00", r"\u{1F600}", r"\uD83D\uDE00",
                   r"\u{D83D}\u{DE00}", "&&", "--", "!", "!!", "&", r"\&"]
    for left, right in itertools.product(class_atoms, repeat=2):
        for operator in ("", "-", "&&", "--"):
            add("[" + left + operator + right + "]", "class-combination")
            add("[^" + left + operator + right + "]", "class-combination-negated")
    for atom in class_atoms:
        add("[" + atom + "]", "class-single")
        add("[^" + atom + "]", "class-single-negated")
    for prefix in ("", "?i", "?m", "?s", "?-i", "?i-", "?i-i", "?ii", "?i-m", "?-", "?g", "?<",
                   "?<x>", r"?<x\u0061>", r"?<x\u{1F600}>"):
        for content in ("a", "a|b", "(?<x>a)", "(?<x>a)|(?<x>b)"):
            colon = ":" if prefix and not prefix.startswith("?<") else ""
            add("(" + prefix + colon + content + ")", "group-prefix")
    for body in ["(?<x>a)|(?<x>b)", "(?:(?<x>a)|(?<x>b))", "(?<x>(?<x>a))",
                 "(?:(?<x>a)|b)(?:(?<x>c)|d)", "(?:(?<x>a)|(?<x>b))*", "(?<x>a){0}(?<x>b)",
                 "(?<x>a)|((?<x>b)|(?<x>c))", "(?<x>a)|(?:(?<x>b)(?<x>c))",
                 "(?<x>a)|(?:(?<x>b)|(?<x>c))", "(?<x>a)(?!(?<x>b))", r"(?<x>a)|(?<\u0078>b)",
                 r"(?<x>a)\k<x>", r"\k<x>(?<x>a)", r"\k<x>", r"[\k<x>](?<x>a)", r"\k<x>[()]",
                 r"(?<x>a)\2", r"(a)\01", r"(a)\1", r"\2(a)(b)", r"[\2](a)(b)"]:
        add(body, "capture-context")
    for term in ("a", "-", "&", "|", "(", ")", "[", "]", "/", r"\d", r"\q{a}", r"\q{ab}",
                 r"\q{}", r"\p{Basic_Emoji}"):
        for container in ("[{}]", "[[{}]]", "[^{}]", "[a&&{}]", "[{}--a]", "[a--{}]", "[{}&&a]", r"[\q{{ab}}&&{}]"):
            add(container.replace("{}", term), "sets-specific", ("v",))
    randomizer = random.Random(seed)
    tokens = ["a", "b", "(", ")", "[", "]", "{", "}", "*", "+", "?", "|", "-", "&", "^", "$",
              ":", "\\", r"\u", r"\k", r"\1", r"\0", r"\c", r"\p", "<", ">", "i", "s", "0", "1", ","]
    for _ in range(random_count):
        add("".join(randomizer.choices(tokens, k=randomizer.randrange(1, 9))), "random")
    return list(cases.values())


def corpus_probes(root, include_strings):
    cases, strings, skipped = [], {}, []
    for file in sorted((root / "test/built-ins/RegExp").rglob("*.js")):
        raw = file.read_bytes()
        source = raw.decode("utf-8")
        path = file.relative_to(root / "test").as_posix()
        for match in CONSTRUCTOR.finditer(source):
            try:
                body = decode_string(match["body"])
                flags = decode_string(match["flags"]) if match["flags"] else ""
                if set(flags) - set("dgimsuvy") or len(flags) != len(set(flags)) or "u" in flags and "v" in flags:
                    continue  # The private Pattern reader does not validate flags.
                cases.append({"body": body, "flags": flags, "path": path, "offset": match.start(),
                              "constructorSource": match.group(), "sourceSha256": hashlib.sha256(raw).hexdigest()})
            except (ValueError, IndexError) as error:
                skipped.append({"path": path, "offset": match.start(), "reason": str(error)})
        if not include_strings:
            continue
        end = source.find("---*/")
        text = source[end + 5:] if end >= 0 else source
        for match in re.finditer(STRING, text):
            try:
                body = decode_string(match.group())
                if len(body) > 5000:
                    continue
                # Normalize valid surrogate pairs for deduplication only; the
                # emitted UTF-16 payload keeps exactly the original code units.
                key = body.encode("utf-16-le", "surrogatepass")
                for flags in ("", "u", "v"):
                    strings.setdefault((key, flags), {"body": body, "flags": flags, "path": path,
                                                       "offsetAfterMetadata": match.start()})
            except (ValueError, IndexError) as error:
                skipped.append({"path": path, "offsetAfterMetadata": match.start(), "reason": str(error)})
    return cases, list(strings.values()), skipped


def compare(name, cases, args, harness):
    for index, case in enumerate(cases):
        case["id"] = f"{name}-{index:06d}"
    rows = [{"id": c["id"], "flags": c["flags"],
             "bodyUtf16": base64.b64encode(c["body"].encode("utf-16-le", "surrogatepass")).decode("ascii")}
            for c in cases]
    payload = "".join(json.dumps(row) + "\n" for row in rows)
    cases_path = args.output / (name + ".cases.jsonl")
    transport_path = args.output / (name + ".transport.jsonl")
    cases_path.write_text(
        "".join(json.dumps(c, ensure_ascii=True) + "\n" for c in cases), encoding="utf-8")
    transport_path.write_text(payload, encoding="utf-8")
    commands = [[args.node, str(TOOLS / "regexp-oracle.cjs")],
                [args.dotnet, str(harness), str(args.assembly)]]
    outputs = []
    for command in commands:
        completed = subprocess.run(command, input=payload, capture_output=True, text=True, encoding="utf-8", check=True)
        outputs.append([json.loads(line) for line in completed.stdout.split("\n") if line])
    node, janglim = outputs
    if len(node) != len(cases) or len(janglim) != len(cases):
        raise RuntimeError("A probe process did not return exactly one result per input.")
    differences, crashes = [], []
    for case, expected, actual in zip(cases, node, janglim):
        if case["id"] != expected["id"] or case["id"] != actual["Id"]:
            raise RuntimeError("Probe response order changed.")
        original_hash = hashlib.sha256(case["body"].encode("utf-16-le", "surrogatepass")).hexdigest()
        if expected["bodySha256"] != original_hash or actual["bodySha256"] != original_hash:
            raise RuntimeError("A probe process changed the original UTF-16 code units: " + case["id"])
        row = {**case, "nodeAccepted": expected["accepted"], "janglimAccepted": actual["accepted"],
               "nodeError": expected["error"], "janglimError": actual["error"]}
        if expected["unexpectedException"] or actual["unexpectedException"]:
            crashes.append({**row, "nodeException": expected["unexpectedException"],
                            "janglimException": actual["unexpectedException"]})
        if expected["accepted"] != actual["accepted"]:
            differences.append(row)
    report = {"kind": name, "cases": len(cases), "mismatches": len(differences),
              "casesSha256": hashlib.sha256(cases_path.read_bytes()).hexdigest(),
              "transportSha256": hashlib.sha256(transport_path.read_bytes()).hexdigest(),
              "verifiedOriginalUtf16Hashes": len(cases),
              "mismatchCases": differences, "unexpectedExceptionCount": len(crashes), "unexpectedExceptions": crashes,
              "scope": "Pattern compilation probes, not Test262 assertions or matching/runtime results. Review every mismatch against the frozen specification."}
    (args.output / (name + ".report.json")).write_text(json.dumps(report, ensure_ascii=True, indent=2) + "\n", encoding="utf-8")
    return {"kind": name, "cases": len(cases), "mismatches": len(differences),
            "unexpectedExceptions": len(crashes), "verifiedOriginalUtf16Hashes": len(cases),
            "casesSha256": report["casesSha256"], "transportSha256": report["transportSha256"]}


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--assembly", type=Path, required=True, help="Already-built Parse.FrontEnd.Grammars.Ebnf.dll")
    parser.add_argument("--test262-root", type=Path, help="Existing pinned Test262 snapshot; no download is performed")
    parser.add_argument("--string-probes", action="store_true", help="Also compile arbitrary string-literal contents from RegExp tests")
    parser.add_argument("--output", type=Path, default=ROOT / "artifacts/ecmascript-2026/regexp-repro")
    parser.add_argument("--dotnet", default="dotnet")
    parser.add_argument("--node", default="node")
    parser.add_argument("--seed", type=int, default=20261021)
    parser.add_argument("--random-count", type=int, default=18000)
    args = parser.parse_args()
    args.assembly, args.output = args.assembly.resolve(), args.output.resolve()
    if not args.assembly.is_file() or args.random_count < 0:
        parser.error("The assembly must exist and --random-count must be nonnegative.")
    if args.test262_root and not (args.test262_root / "test/built-ins/RegExp").is_dir():
        parser.error("--test262-root must contain test/built-ins/RegExp.")
    args.output.mkdir(parents=True, exist_ok=True)
    build = args.output / "harness"
    build.mkdir(exist_ok=True)
    subprocess.run([args.dotnet, "build", str(TOOLS / "RegExpAudit/RegExpAudit.csproj"), "-c", "Release", "--nologo",
                    "--output", str(build), "-p:BaseIntermediateOutputPath=" + str(build / "obj") + "/"], check=True)
    harness = build / "RegExpAudit.dll"
    summaries = [compare("generated", generated_probes(args.seed, args.random_count), args, harness)]
    skipped = []
    if args.test262_root:
        constructors, strings, skipped = corpus_probes(args.test262_root.resolve(), args.string_probes)
        summaries.append(compare("constructors", constructors, args, harness))
        if args.string_probes:
            summaries.append(compare("source-strings", strings, args, harness))
    versions = subprocess.check_output([args.node, "-p", "JSON.stringify({node:process.version,unicode:process.versions.unicode})"], text=True)
    summary = {"assembly": str(args.assembly), "assemblySha256": hashlib.sha256(args.assembly.read_bytes()).hexdigest(),
               "reference": json.loads(versions), "seed": args.seed, "randomCount": args.random_count,
               "transport": "Base64 of exact little-endian UTF-16 code units, including unmatched surrogates",
               "reports": summaries, "skippedSourceLiterals": skipped}
    (args.output / "summary.json").write_text(json.dumps(summary, ensure_ascii=True, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(summary, ensure_ascii=True, indent=2))
    return 1 if any(report["unexpectedExceptions"] for report in summaries) else 0


if __name__ == "__main__":
    raise SystemExit(main())
