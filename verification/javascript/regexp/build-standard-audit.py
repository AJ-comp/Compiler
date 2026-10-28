"""Independent RegExp syntax corpus. Never executes a JavaScript program/matcher."""
import json
from pathlib import Path

out = Path(__file__).parent
cases = []
seen = set()


def add(body, flags="", category="syntax", spec=None, note=None):
    key = (body, flags)
    if key in seen:
        return
    seen.add(key)
    item = {"id": len(cases), "body": body, "flags": flags, "category": category}
    if spec is not None:
        item["specValid"] = spec
    if note:
        item["note"] = note
    cases.append(item)


for body in [
    "a", "", "|", "a|", "|a", "a||b", "(?:)", "()", "a.b", "^$",
    "\\b", "\\B", "^*", "$?", "\\b+", "a**", "a*?", "a??", "a???", "a++",
    "a{0}", "a{01}", "a{01,002}", "a{2,1}", "a{0,}", "a{,2}", "a{a}", "a{", "a}",
    "{", "}", "{0}", "{1,}", "{2,1}", "{,2}", "a{1,2}?", "a{1,2}+", "a{ 1}",
    "a{1, 2}", "a{1_0}", "a{1,2,3}", "a{00000000000000000000,0}", "a{2,01}",
    "a{999999999999999999999999999999}",
    "a{999999999999999999999999999999999999998,999999999999999999999999999999999999997}",
    "a{1000000000000000000000000000000000000000,999999999999999999999999999999999999999}",
    "a{999999999999999999999999999999999999999,1000000000000000000000000000000000000000}",
    "a{000000000000000000000000000000000000002,1}", "a{1,000000000000000000000000000000000000002}",
    "[a-z]", "[z-a]", "[a-a]", "[]", "[^]", "[^^]", "[-]", "[--]", "[---]", "[a-b-c]",
    "[a--b]", "[a-b-c-d]", "[a-]", "[-a]", "[a-z-]", "[-a-z]", "[a-zA-Z0-9_]",
    "[\\d-z]", "[z-\\d]", "[\\d-z-a]", "[z-\\d-a]", "[\\w-\\s]", "[a-\\c]", "[\\c-a]",
    "[a-\\u0062]", "[\\u0062-a]", "[\\x61-\\x7a]", "[\\b-a]", "[a-\\b]",
    "[\\u{1F600}-\\u{1F601}]", "[\\uD83D\\uDE00-\\uD83D\\uDE01]",
    "[\\uD83D-\\uDE00]", "[\\uDE00-\\uD83D]", "[😀-😁]", "[😁-😀]",
    "a]", "a[", "a[bc", "a\\", "(a", "a)", "(?a)", "(?#comment)", "(?>a)", "(?1)",
    "(?=a)", "(?!a)", "(?<=a)", "(?<!a)", "(?<=a*)", "(?<!a|bc)",
    "(?=a)*", "(?!a)?", "(?<=a)+", "(?<!a){1}", "(?:(?=a))*", "(?=(?=a))?",
    "(?<a>x)", "(?<a>x)\\k<a>", "\\k<a>(?<a>x)", "(?<a>x)\\k<b>", "(?<a>x)\\k",
    "\\k", "\\k<a>", "\\k<", "[\\k]", "[\\k](?<a>x)", "[\\k<a>](?<a>x)",
    "(?<a>x)|(?<a>y)", "(?<a>x)(?<a>y)", "(?:(?<a>x)|(?<a>y))+", "(?<a>(?<a>x))",
    "(?<a>(?<a>x)|y)", "(?<a>x|(?<a>y))", "(?<a>x(?<a>y)|z)",
    "(?<a>(?:(?<a>x)|y))", "(?<a>x)|(?:(?<a>y)|(?<a>z))", "(?:(?<a>x)|y)(?:(?<a>z)|q)",
    "(?=(?<a>x))(?<a>y)", "(?=(?<a>x))|(?<a>y)", "(?<a>x)|(?<\\u0061>y)",
    "(?<>)", "(?<1>x)", "(?<a-b>x)", "(?<a b>x)", "(?<$>x)", "(?<_>x)", "(?<await>x)",
    "(?<a\\u200C>x)", "(?<\\u200C>x)", "(?<a\\u200D>x)", "(?<\\u200D>x)",
    "(?<\\u{61}>x)", "(?<\\u{61}>x)\\k<a>", "(?<a>x)\\k<\\u{61}>",
    "(?<\\uD801\\uDC00>x)", "(?<\\uD801>x)", "(?<𐐀>x)", "(?<😀>x)",
    "(?<a>x)\\1", "\\1(a)", "(a)\\2", "\\1", "\\8", "\\9", "\\09", "\\08", "\\01", "\\123",
    "\\400", "\\777", "\\999999999999999999999999999999999999999999", "[\\1]", "[\\8]",
    "(a)[\\1]", "\\0", "\\00", "\\000", "[\\0]", "[\\00]", "[\\000]",
    "\\a", "\\_", "\\-", "\\ ", "\\/", "\\!", "\\=", "\\,", "\\:", "\\;", "\\#", "\\@",
    "\\x", "\\x0", "\\xGG", "\\x61", "[\\x]", "[\\x0]", "[\\xGG]", "[\\x61]",
    "\\u", "\\u0", "\\u00GG", "\\u0061", "\\u{61}", "\\u{}", "\\u{110000}",
    "\\u{000000000000000000000000000000000000000061}", "\\u{FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF}",
    "\\uD83D\\uDE00", "\\uD83D\\uD83D\\uDE00", "\\uDE00", "\\uD800", "\\cA", "\\cz",
    "\\c", "\\c_", "\\c0", "\\c8", "\\c*", "[\\c]", "[\\c_]", "[\\c0]", "[\\c8]", "[\\c-]",
    "\\p", "\\p{ASCII}", "\\P{ASCII}", "\\p{Invalid}", "\\p{Script=Latin}", "\\p{sc=Latn}",
    "\\p{General_Category=Lu}", "\\p{gc=Uppercase_Letter}", "\\p{ASCII_Hex_Digit}", "\\p{AHex}",
    "\\p{White_Space}", "\\p{space}", "\\p{WSpace}", "[\\p{WSpace}]", "\\P{WSpace}",
    "\\p{IsLatin}", "\\p{sc=latin}", "\\p{gc=lowercase_letter}", "\\p{Lu}", "\\p{Uppercase_Letter}",
    "\\p{RGI_Emoji}", "\\P{RGI_Emoji}", "[\\p{RGI_Emoji}]", "[^\\p{RGI_Emoji}]",
    "(?i:a)", "(?m:a)", "(?s:a)", "(?ims:a)", "(?i-:a)", "(?-i:a)", "(?-ims:a)", "(?-:a)",
    "(?ii:a)", "(?i-i:a)", "(?im-ms:a)", "(?g:a)", "(?u:a)", "(?v:a)", "(?d:a)",
    "(?i:a(?-i:b))", "(?i)", "(?i-a:a)", "(?i--m:a)", "(?i-m-s:a)",
    "[a&&b]", "[a--b]", "[[a]&&[b]]", "[ab&&c]", "[a&&bc]", "[a&&b--c]", "[[a&&b]--c]",
    "[a&&&b]", "[a---b]", "[a----b]", "[\\q{ab|cd}]", "[\\q{}]", "[^\\q{ab}]",
    "[^\\q{a}]", "[^\\q{ab}&&a]", "[^\\q{ab}--\\q{ab}]", "[\\q{a|}]", "[\\q{a\\|b}]",
    "[[]]", "[()]", "[{}]", "[/]", "[\\/]", "[|]", "[\\|]", "[\\-]", "[a-b]",
]:
    for flags in ["", "u", "v"]:
        spec = None
        note = None
        if body in ["(?<a>(?<a>x)|y)", "(?<a>x(?<a>y)|z)"]:
            spec = False
            note = "ECMA-262 2026 MightBothParticipate: outer capture is not in either alternative; Node24 incorrectly accepts."
        if body in [
            "a{999999999999999999999999999999999999998,999999999999999999999999999999999999997}",
            "a{1000000000000000000000000000000000000000,999999999999999999999999999999999999999}",
        ]:
            spec = False
            note = "ECMA-262 2026 QuantifierPrefix: exact integer minimum exceeds maximum; Node24 clamps large integers."
        if body in ["\\p{WSpace}", "[\\p{WSpace}]", "\\P{WSpace}"] and flags:
            spec = False
            note = "ECMA-262 2026 Table65 lists White_Space and space only; Node24 accepts additional UCD alias WSpace."
        add(body, flags, spec=spec, note=note)

# Combine independent atoms, assertions, and quantifiers across all pattern modes.
atoms = ["a", ".", "[]", "[^]", "[a]", "[z-a]", "[\\d]", "[\\d-z]", "\\d", "\\b", "^", "$", "\\1", "\\0", "\\8", "\\c", "\\x", "\\u{61}", "(?:a)", "(?=a)", "(?<=a)", "(?<n>a)", "(?i:a)", "[\\q{ab}]"]
quantifiers = ["", "*", "+", "?", "*?", "+?", "??", "{0}", "{2,1}", "{01,2}", "{", "{2,}", "{,2}", "{0,1}?", "+?"]
for atom in atoms:
    for quantifier in quantifiers:
        for flags in ["", "u", "v"]:
            add(atom + quantifier, flags, "atom-quantifier")

# Modifier combinations, including empty sides and illegal flags.
for add_flags in ["", "i", "m", "s", "im", "mi", "ims", "ii", "g", "u", "v", "d", "y", "I"]:
    for remove_flags in [None, "", "i", "m", "s", "ims", "ii", "u"]:
        body = "(?" + add_flags + ("-" + remove_flags if remove_flags is not None else "") + ":a)"
        for flags in ["", "u", "v"]:
            add(body, flags, "modifiers")

# Capturing-group counting must ignore escaped parentheses and class characters.
for prefix in ["", "(a)", "(a)(b)", "(?:a)", "(?=a)", "[(]", "\\(", "(?<x>a)", "(?i:a)"]:
    for ref in ["\\1", "\\2", "\\3", "\\10", "\\k<x>", "[\\1]", "[\\k]"]:
        for flags in ["", "u", "v"]:
            add(prefix + ref, flags, "capture-count")

# Literal lexical restrictions are recorded separately from Pattern grammar acceptance.
for body in ["", "a/b", "[a/b]", "a\nb", "a\rb", "a\u2028b", "a\u2029b", "\\\nb", "\\/", "[\\/]"]:
    for flags in ["", "u", "v"]:
        add(body, flags, "literal-boundary")

(out / "standard-audit-cases.json").write_text(json.dumps(cases, ensure_ascii=True, indent=2), encoding="utf8")
print(f"Wrote {len(cases)} independent cases")
