#!/usr/bin/env python3
"""Inventory the frozen ECMAScript 2026 syntax specification, using only stdlib.

The output maps specification inventory to implementation files; it deliberately
does not equate a mapped file with conformance. Test results belong to the separate
Test262 runner. Downloaded HTML is cached under ignored artifacts/.
"""
from __future__ import annotations

import argparse
import hashlib
import json
import re
import urllib.request
from html.parser import HTMLParser
from pathlib import Path

BASE = "https://tc39.es/ecma262/2026/multipage/"
ROOT = Path(__file__).resolve().parents[2]
CS = "src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/"
TEST = "Tests/Parse.FrontEnd.Parsers.Tests/JavaScript/"
GRAMMAR = "grammars/javascript/"
PAGES = [
    "notational-conventions", "syntax-directed-operations",
    "ecmascript-language-lexical-grammar", "ecmascript-language-expressions",
    "ecmascript-language-statements-and-declarations",
    "ecmascript-language-functions-and-classes",
    "ecmascript-language-scripts-and-modules", "text-processing",
    "additional-ecmascript-features-for-web-browsers", "grammar-summary",
]


class Node:
    def __init__(self, tag, attrs=(), parent=None):
        self.tag, self.attrs, self.parent, self.children = tag, dict(attrs), parent, []

    def walk(self, tag=None):
        if tag is None or self.tag == tag:
            yield self
        for item in self.children:
            if isinstance(item, Node):
                yield from item.walk(tag)

    def text(self):
        return " ".join("".join(x.text() if isinstance(x, Node) else x for x in self.children).split())

    def heading(self):
        return next((x.text() for x in self.children if isinstance(x, Node) and x.tag == "h1"), "")


class Tree(HTMLParser):
    VOID = set("area base br col embed hr img input link meta param source track wbr".split())

    def __init__(self, source):
        super().__init__(convert_charrefs=True)
        self.root = Node("root")
        self.stack = [self.root]
        self.feed(source)

    def handle_starttag(self, tag, attrs):
        node = Node(tag, attrs, self.stack[-1])
        self.stack[-1].children.append(node)
        if tag not in self.VOID:
            self.stack.append(node)

    def handle_startendtag(self, tag, attrs):
        self.handle_starttag(tag, attrs)
        if tag not in self.VOID:
            self.stack.pop()

    def handle_endtag(self, tag):
        for index in range(len(self.stack) - 1, 0, -1):
            if self.stack[index].tag == tag:
                del self.stack[index:]
                break

    def handle_data(self, data):
        self.stack[-1].children.append(data)


def source_group(section, title, grammars=(), implementations=(), tests=(), scope="syntax-and-static-semantics"):
    return {
        "id": section, "title": title, "scope": scope,
        "grammarFiles": [GRAMMAR + "ecmascript-2026-" + x + ".ebnf" for x in grammars],
        "implementationFiles": [CS + "JavaScript" + x + ".cs" for x in implementations],
        "testFiles": [TEST + "JavaScript" + x + "Tests.cs" for x in tests],
        "reviewStatus": "source-mapped" if scope != "runtime-out-of-parser-scope" else "out-of-scope",
        "productionNames": [], "earlyErrorClauses": [],
    }


def groups():
    result = [
        source_group("5.1", "Grammar notation and lexical/source goals", implementations=("Grammar", "FunctionGrammar"), tests=("Grammar",)),
        source_group("5.2", "Algorithm notation and static semantics", implementations=("ExpressionValidation", "StatementValidation"), tests=("Grammar",)),
        source_group("8.2", "BoundNames and declaration scope analysis", implementations=("StatementValidation", "AssignmentPatterns", "ArrowFunctions"), tests=("Binding", "Statement", "Module", "NonStrictSemantic")),
        source_group("8.3", "Label analysis", ("statements",), ("StatementValidation",), ("Statement", "NonStrictSemantic")),
        source_group("8.5", "Contains and syntax context", implementations=("Syntax", "StatementValidation", "ClassValidation"), tests=("FunctionKinds", "Class", "ArrowFunction")),
        source_group("8.6", "AssignmentTargetType and static helper operations", implementations=("ExpressionValidation", "AssignmentPatterns", "ArrowFunctions"), tests=("Expression", "Destructuring", "ArrowFunction")),
    ]
    lexical = [
        ("12", "Input element lexical goals", ("Grammar", "SlashContext"), ("Lexical", "Template")),
        ("12.1", "Unicode format controls", ("TokenPatterns", "Unicode"), ("Lexical",)),
        ("12.2", "White space", ("TokenPatterns",), ("Lexical",)),
        ("12.3", "Line terminators", ("TokenPatterns", "StatementPredicates"), ("Lexical", "Statement")),
        ("12.4", "Comments", ("TokenPatterns",), ("Lexical", "NonStrictBrowser")),
        ("12.5", "Hashbang comments", ("TokenPatterns",), ("Lexical",)),
        ("12.6", "Input elements and lexical goals", ("Grammar", "SlashContext"), ("Lexical", "Expression", "Statement")),
        ("12.7", "Identifiers, private identifiers and keywords", ("TokenPatterns", "Unicode", "Strictness"), ("Lexical", "Class", "NonStrict")),
        ("12.8", "Punctuators and optional-chain boundary", ("TokenPatterns",), ("Lexical", "Expression")),
        ("12.9.1", "Null literal", ("TokenPatterns",), ("Expression",)),
        ("12.9.2", "Boolean literals", ("TokenPatterns",), ("Expression",)),
        ("12.9.3", "Numeric literals", ("TokenPatterns", "Strictness"), ("Lexical", "NonStrictLexical")),
        ("12.9.4", "String literals", ("TokenPatterns", "ExpressionValidation", "Strictness"), ("Lexical", "NonStrictLexical")),
        ("12.9.5", "Regular-expression literal boundaries and flags", ("TokenPatterns", "RegExpValidation", "SlashContext"), ("Lexical", "RegExp")),
        ("12.9.6", "Template literal lexical grammar", ("TemplateTokens", "SlashContext"), ("Template",)),
        ("12.10", "Automatic semicolon insertion", ("StatementPredicates", "SlashContext"), ("Statement", "FunctionKinds", "NonStrict")),
    ]
    result += [source_group(s, t, ("expressions",), i, ts) for s, t, i, ts in lexical]
    expressions = [
        ("13.1", "Identifier references and bindings", ("ExpressionValidation", "Strictness", "StatementValidation"), ("Expression", "Binding", "NonStrictSemantic")),
        ("13.2", "Primary expressions, literals and cover grammar", ("ExpressionValidation", "ArrowFunctions", "AssignmentPatterns", "TemplateTokens"), ("Expression", "Destructuring", "ArrowFunction", "Template")),
        ("13.3", "Member/call/new/optional chains/import/super", ("ExpressionValidation", "ClassValidation", "ModuleValidation"), ("Expression", "Class", "Module")),
        ("13.4", "Update expressions", ("ExpressionValidation", "StatementPredicates"), ("Expression", "NonStrictBrowser")),
        ("13.5", "Unary operators", ("ExpressionValidation", "ClassValidation"), ("Expression", "Class", "NonStrictSemantic")),
        ("13.6", "Exponentiation", ("ExpressionValidation",), ("Expression",)),
        ("13.7", "Multiplicative operators", ("SlashContext",), ("Expression",)),
        ("13.8", "Additive operators", (), ("Expression",)),
        ("13.9", "Bitwise shift operators", (), ("Expression",)),
        ("13.10", "Relational operators and private-in", ("ClassValidation",), ("Expression", "Class")),
        ("13.11", "Equality operators", (), ("Expression",)),
        ("13.12", "Binary bitwise operators", (), ("Expression",)),
        ("13.13", "Logical and nullish operators", (), ("Expression",)),
        ("13.14", "Conditional operator", (), ("Expression", "Statement")),
        ("13.15", "Assignment and destructuring assignment", ("ExpressionValidation", "AssignmentPatterns"), ("Expression", "Destructuring", "NonStrictBrowser")),
        ("13.16", "Comma operator", (), ("Expression",)),
    ]
    result += [source_group(s, t, ("expressions",), i, ts) for s, t, i, ts in expressions]
    statements = [
        ("14", "Statement and declaration entry productions"),
        ("14.1", "Statement semantics"), ("14.2", "Blocks and lexical scopes"),
        ("14.3", "Declarations, variable statements and binding patterns"),
        ("14.4", "Empty statements"), ("14.5", "Expression statements"),
        ("14.6", "If statements"), ("14.7", "Iteration and for-await-of"),
        ("14.8", "Continue statements"), ("14.9", "Break statements"),
        ("14.10", "Return statements"), ("14.11", "With statements"),
        ("14.12", "Switch statements"), ("14.13", "Labelled statements"),
        ("14.14", "Throw statements"), ("14.15", "Try and catch"), ("14.16", "Debugger statements"),
    ]
    result += [source_group(s, t, ("statements", "bindings"), ("StatementValidation", "StatementPredicates", "NonStrictGrammar"), ("Statement", "Binding", "NonStrictSemantic", "NonStrictBrowser")) for s, t in statements]
    functions = [
        ("15.1", "Parameter lists", ("bindings",), ("StatementValidation",), ("Binding",)),
        ("15.2", "Ordinary functions", ("statements",), ("StatementValidation", "Strictness", "FunctionGrammar"), ("Statement", "NonStrictSemantic")),
        ("15.3", "Arrow functions", ("statements",), ("ArrowFunctions", "FunctionGrammar"), ("ArrowFunction",)),
        ("15.4", "Methods and accessors", ("functions",), ("ClassValidation", "StatementValidation", "Syntax"), ("FunctionKinds", "Class")),
        ("15.5", "Generator functions", ("functions",), ("FunctionGrammar", "StatementValidation"), ("FunctionKinds", "NonStrict")),
        ("15.6", "Async generators", ("functions",), ("FunctionGrammar", "StatementValidation"), ("FunctionKinds", "NonStrict")),
        ("15.7", "Classes, fields, private names and static blocks", ("classes",), ("ClassValidation", "Strictness", "Syntax"), ("Class",)),
        ("15.8", "Async functions", ("functions",), ("FunctionGrammar", "ModuleValidation", "StatementValidation"), ("FunctionKinds",)),
        ("15.9", "Async arrow functions", ("functions",), ("ArrowFunctions", "FunctionGrammar"), ("FunctionKinds", "ArrowFunction")),
    ]
    result += [source_group(s, t, g, i, ts) for s, t, g, i, ts in functions]
    result += [
        source_group("16.1", "Script source goal", ("statements",), ("Grammar", "StatementValidation", "Strictness"), ("Grammar", "Statement", "NonStrict")),
        source_group("16.2", "Module source goal/import/export/import attributes", ("modules",), ("Grammar", "ModuleValidation", "StatementValidation"), ("Module", "Grammar")),
        source_group("22.2.1", "RegExp pattern syntax and early errors", implementations=("RegExpValidation", "RegExpSets", "RegExpProperties"), tests=("RegExp",)),
        source_group("22.2.3.4", "RegExp ParsePattern static semantics", implementations=("RegExpValidation", "RegExpSets", "RegExpProperties"), tests=("RegExp",)),
        source_group("B.1", "Web lexical and RegExp extensions", implementations=("TokenPatterns", "RegExpValidation", "RegExpSets"), tests=("NonStrictLexical", "NonStrictBrowser", "RegExp")),
        source_group("B.3", "Web statement and assignment-target extensions", ("statements",), ("NonStrictGrammar", "StatementValidation", "ExpressionValidation"), ("NonStrictBrowser", "NonStrictSemantic")),
    ]
    return result


def section_number(heading):
    found = re.match(r"(?:[A-Z]|\d+)(?:\.\d+)*", heading)
    return found.group() if found else None


def clause_of(node):
    while node is not None:
        if node.tag in ("emu-clause", "emu-annex"):
            return node
        node = node.parent
    return None


def top_level_assertion(node, clause):
    """Do not count nested conditions or algorithm steps as separate rules."""
    parent = node.parent
    while parent is not clause:
        if parent.tag == "li":
            return False
        parent = parent.parent
    return True


def select_group(clause, mapping):
    current = clause
    while current is not None:
        number = section_number(current.heading())
        if number:
            matches = [g for g in mapping if number == g["id"] or number.startswith(g["id"] + ".")]
            if matches:
                return max(matches, key=lambda g: len(g["id"]))
        current = clause_of(current.parent)
    return None


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--refresh", action="store_true", help="Re-download the frozen edition pages")
    args = parser.parse_args()
    cache = ROOT / "artifacts/ecmascript-2026/spec"
    cache.mkdir(parents=True, exist_ok=True)
    mapping, sources, clauses, productions, errors, unmapped = groups(), [], [], [], [], []
    for page in PAGES:
        path, url = cache / (page + ".html"), BASE + page + ".html"
        if args.refresh or not path.exists():
            request = urllib.request.Request(url, headers={"User-Agent": "Janglim-ECMAScript-syntax-audit/1.0"})
            path.write_bytes(urllib.request.urlopen(request, timeout=90).read())
        data = path.read_bytes()
        tree = Tree(data.decode("utf-8")).root
        sources.append({"url": url, "sha256": hashlib.sha256(data).hexdigest(), "bytes": len(data)})
        # Annex A repeats canonical grammar. Record it as a source for cross-checking,
        # but do not count duplicated definitions as independently supported rules.
        if page == "grammar-summary":
            continue
        for clause in [n for n in tree.walk() if n.tag in ("emu-clause", "emu-annex")]:
            heading = clause.heading()
            number = section_number(heading)
            group = select_group(clause, mapping)
            if group is None:
                continue
            entry = {"section": number, "id": clause.attrs.get("id"), "title": heading,
                     "url": url + "#" + clause.attrs.get("id", ""), "group": group["id"]}
            clauses.append(entry)
            if "Early Errors" in heading:
                bullets = [n for n in clause.walk("li")
                           if clause_of(n) is clause and top_level_assertion(n, clause)]
                error = dict(entry, assertionCount=len(bullets),
                             assertionSha256=[hashlib.sha256(n.text().encode()).hexdigest() for n in bullets],
                             reviewStatus="clause-inventoried-and-source-mapped")
                errors.append(error)
                group["earlyErrorClauses"].append(error["id"])
        for production in tree.walk("emu-production"):
            name, anchor = production.attrs.get("name"), production.attrs.get("id", "")
            # Syntax-directed algorithms repeat productions. Canonical definitions
            # carry an exact prod-<name> or prod-annexB-<name> id.
            if not name or anchor not in ("prod-" + name, "prod-annexB-" + name):
                continue
            clause = clause_of(production)
            group = select_group(clause, mapping)
            if group is None:
                unmapped.append({"name": name, "url": url + "#" + anchor,
                                 "section": clause.heading() if clause else None})
                continue
            rhs = [n for n in production.children if isinstance(n, Node) and n.tag == "emu-rhs"]
            entry = {"name": name, "parameters": production.attrs.get("params", ""),
                     "lexical": production.attrs.get("type") == "lexical",
                     "alternativeCount": len(rhs), "url": url + "#" + anchor,
                     "section": section_number(clause.heading()), "group": group["id"],
                     "rhsSha256": [hashlib.sha256(n.text().encode()).hexdigest() for n in rhs]}
            productions.append(entry)
            group["productionNames"].append(name)
    if unmapped:
        raise SystemExit("Canonical definitions without a source group: " + json.dumps(unmapped))
    ebnf = []
    for path in sorted((ROOT / GRAMMAR).glob("*.ebnf")):
        text = path.read_text(encoding="utf-8-sig")
        ebnf.append({"path": path.relative_to(ROOT).as_posix(),
                     "productions": re.findall(r"(?m)^([A-Za-z][A-Za-z0-9_]*)\s*:(?:=)?", text)})
    for group in mapping:
        group["productionNames"] = sorted(set(group["productionNames"]))
        group["earlyErrorClauses"] = sorted(set(group["earlyErrorClauses"]))
        for kind in ("grammarFiles", "implementationFiles", "testFiles"):
            for filename in group[kind]:
                if not (ROOT / filename).is_file():
                    raise SystemExit("Mapped file is missing: " + filename)
    result = {
        "schemaVersion": 1, "edition": "ECMAScript 2026 (17th edition)",
        "specification": BASE, "auditDate": "2026-10-21",
        "status": "inventory-and-source-mapping-complete",
        "method": {"canonicalProductions": "Only exact prod-<name> and prod-annexB-<name> anchors; Annex A duplicates excluded",
                   "earlyErrors": "Every inventoried Early Errors clause; count/hash top-level rule items only, excluding nested conditions and algorithm steps",
                   "mapping": "Nearest clause-number group; mapping is traceability, not a proof of conformance",
                   "sourceCache": "artifacts/ecmascript-2026/spec (ignored)",
                   "command": "python tools/Test262/extract-ecmascript-coverage.py --refresh"},
        "outOfScope": [
            {"sections": ["6", "7", "8.1", "8.4", "9", "10", "11", "15.10", "17", "18", "19", "20", "21", "22.1", "22.2.2", "22.2.3 except 22.2.3.4", "22.2.4-22.2.9", "23", "24", "25", "26", "27", "28", "B.2", "B.3.6", "B.3.7", "B.3.8"],
             "reason": "Execution algorithms, runtime built-ins and host/module loading; syntax embedded in dynamic constructors is covered separately by the corpus scope."},
            {"sections": ["A"], "reason": "Grammar summary duplicates canonical productions in clauses 12-16, 22.2 and Annex B; used as a cross-check source."},
        ],
        "summary": {"sourcePages": len(sources), "ruleGroups": len(mapping),
                    "mappedClauses": len(clauses), "canonicalProductions": len(productions),
                    "earlyErrorClauses": len(errors), "earlyErrorAssertions": sum(e["assertionCount"] for e in errors)},
        "sources": sources, "groups": mapping, "clauses": clauses,
        "productions": productions, "earlyErrors": errors, "repositoryGrammars": ebnf,
    }
    output = ROOT / "tools/Test262/ecmascript-2026-coverage.json"
    output.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(result["summary"], ensure_ascii=False))
    print(output)


if __name__ == "__main__":
    main()
