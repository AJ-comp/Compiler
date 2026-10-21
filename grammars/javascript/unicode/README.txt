ECMAScript identifier Unicode data
=================================

Version: Unicode 17.0.0 (published September 2025, before ECMAScript 2026).
Source: https://www.unicode.org/Public/17.0.0/ucd/DerivedCoreProperties.txt
SHA-256: 24c7fed1195c482faaefd5c1e7eb821c5ee1fb6de07ecdbaa64b56a99da22c08
License: Unicode License v3; full license in LICENSE.txt.

generate_identifier_data.py extracts and merges ID_Start / ID_Continue ranges,
then applies the ECMAScript additions '$' and '_'. It generates
src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptUnicodeData.g.cs.
The source hash is verified before generation. Unicode 17.0.0 ID_Continue
already contains '_' and U+200C/U+200D.

Run from any directory:
  python grammars/javascript/unicode/generate_identifier_data.py
For an offline regeneration, pass the downloaded official input file as the
optional command-line argument. No Python packages are required.

The generated ranges are checked in so ordinary builds use no network and do
not vary with the .NET runtime's Unicode version. JavaScriptUnicode converts
astral code points to explicit UTF-16 surrogate-pair regex alternatives.

ECMAScript reference:
https://tc39.es/ecma262/2026/multipage/ecmascript-language-lexical-grammar.html#sec-names-and-keywords


ECMAScript RegExp property aliases
=================================

Version: Unicode 17.0.0, ECMAScript 2026 Tables 64, 65 and 66.
Sources:
  https://www.unicode.org/Public/17.0.0/ucd/PropertyAliases.txt
  SHA-256: 4441f573caf952ffece1d7c892e7715bd7136dfc26f96eb6f268bf1e474715fb
  https://www.unicode.org/Public/17.0.0/ucd/PropertyValueAliases.txt
  SHA-256: 64e9a5f76f7a1e8b5a47d6a1f9a26522a251208f5276bdfa1559dac7cf2e827a
License: Unicode License v3; full license in LICENSE.txt.
ECMAScript whitelist:
  https://tc39.es/ecma262/2026/multipage/text-processing.html#table-nonbinary-unicode-properties
  https://tc39.es/ecma262/2026/multipage/text-processing.html#table-binary-unicode-properties
  https://tc39.es/ecma262/2026/multipage/text-processing.html#table-binary-unicode-properties-of-strings

generate_regexp_property_data.py extracts General_Category and Script value
aliases from the two pinned UCD files and applies ECMAScript's explicit property
name whitelist. The generated JavaScriptRegExpProperties.g.cs contains 80
General_Category aliases, 346 Script aliases, 98 binary property names/aliases,
and the 7 binary properties of strings. The Script_Extensions property uses
the same value aliases as Script. String properties require v mode.

Exact spelling is significant: ECMAScript does not use Unicode loose matching.
The UCD binary-property list also contains properties that ECMAScript does not
support, so importing every UCD property would accept invalid JavaScript.
Table 65 explicitly permits White_Space and space, but omits WSpace. ASCII,
Any and Assigned are ECMAScript additions to the UCD names.

Script aliases are checked as names, independently of whether any characters
currently have that value. Hrkt and Katakana_Or_Hiragana are retained because
the pinned PropertyValueAliases.txt lists them. Unicode UAX #31 describes
this historical Script value as an empty set:
  https://www.unicode.org/reports/tr31/tr31-43.html
The table also includes the new Unicode 17 Script aliases, even when a host
JavaScript runtime still uses Unicode 16. RegExp syntax validation therefore
does not change with the Unicode version installed on the machine.

Regenerate from the official sources:
  python grammars/javascript/unicode/generate_regexp_property_data.py
Regenerate from a directory containing the exact two downloaded input files:
  python grammars/javascript/unicode/generate_regexp_property_data.py path/to/input
Verify the checked-in generated file without changing it:
  python grammars/javascript/unicode/generate_regexp_property_data.py --check

Both input hashes are verified before generation. The generated aliases are
checked in; ordinary builds need neither network access nor a host Unicode
database. Unicode property names are checked without compiling or executing
the regular expression in the .NET regex engine.
