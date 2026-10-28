# ECMAScript 2026 RegExpPattern audit

Sources: [22.2.1 syntax and early errors](https://tc39.es/ecma262/2026/multipage/text-processing.html#sec-patterns), [ParsePattern](https://tc39.es/ecma262/2026/multipage/text-processing.html#sec-parsepattern), [Annex B.1.2](https://tc39.es/ecma262/2026/multipage/additional-ecmascript-features-for-web-browsers.html#sec-regular-expressions-patterns), [12.9.5 literal lexical boundary](https://tc39.es/ecma262/2026/multipage/ecmascript-language-lexical-grammar.html#sec-literals-regular-expression-literals).

## Mode, lexical boundary, and phase

- Existing lexer finds a complete literal and validates flags; body checks belong to final CST validation, never speculative source.Peek. A regex candidate can be invalid while an actual division parse is valid.
- No flags: UTF-16 code units, Annex B.1.2 extensions supported as a deliberate web-compatible RegExp profile. Their applicability does not depend on Script strict mode or Module; Annex B.1.2 has no such guard.
- `u`: Unicode code points, strict pattern grammar; `v`: same UnicodeMode plus ClassSet grammar. `u` and `v` mutually exclusive.
- NamedCaptureGroups always enabled in `u`/`v`. Legacy initially parses without it, then parses with it if any named capture occurs anywhere. This globally changes `\\k`, even preceding the named declaration or inside a character class.
- Constructor Pattern permits empty pattern, raw slash and raw line terminator; literal lexing does not. These are separate validations. Empty literal `//` is a comment, while `/(?:)/` is an empty-matching literal.
- Pattern grammar validation does not execute JS or run matching; .NET regex syntax/semantics cannot stand in for ECMAScript validation.

## Groups and assertions

- Empty alternatives/groups and variable-length lookbehind are syntactically permitted.
- Capturing `(...)`, noncapture `(?:...)`, named `(?<name>...)`, lookahead `(?=...)`/`(?!...)`, lookbehind `(?<=...)`/`(?<!...)`.
- Modifier groups only `i`,`m`,`s`; `(?im-s:...)`, `(?i-:...)`, `(?-i:...)` valid. Duplicate add/remove flag, overlap, unknown flag, multiple minus, or both add/remove sides empty after minus are errors. `(?:...)` is ordinary noncapture.
- Inline non-scoped `(?i)`, atomic `(?>...)`, comments `(?#...)`, numeric subroutine `(?1)`, conditionals are not ECMAScript syntax.
- Assertions `^`,`$`,`\\b`,`\\B` are never quantifiable. Lookahead can have a quantifier only in legacy Annex B; lookbehind never can. A group *containing* an assertion remains an Atom and can be quantified, e.g. `(?:(?=a))*`.
- Numeric capture backreference can be forward, and numeric validity uses total capture count across the entire pattern. Escaped `\\(`, character-class `[(]`, and noncapture/assertion/modifier groups must not inflate capture count.
- Group names use IdentifierStart/Part including `$`, `_`, ZWNJ/ZWJ as part only; language reserved words remain valid names. Unicode escapes in names use Unicode escape grammar even without a `u`/`v` flag. Literal and escaped astral identifiers combine surrogate pairs correctly.
- Duplicate normalized group names permitted only if at least one enclosing Disjunction separates the two GroupSpecifier nodes into different alternatives. Repeat count, lookaround success, or semantic exclusivity is irrelevant. Outer capture and its descendant same-name capture always overlap structurally and are errors.
- Named backreferences normalized by escaped group names, permitted before declaration, but must resolve to at least one capture.

## Quantifiers and escapes

- Quantifier optional final `?` denotes lazy. No possessive `+`, second quantifier, or quantifier without a preceding Atom.
- Bounds contain nonempty decimal digits, no separators/whitespace, minimum <= maximum using exact mathematical integers. Leading zeroes valid, arbitrary numeric width must not overflow or collapse to Infinity.
- Legacy invalid/incomplete braces can be ordinary characters, but a well-formed `{m,n}` quantifier without a target is always an early error, and reversed `{m,n}` always an error. `a{,2}` is legacy literal suffix, not a bounded quantifier.
- Decimal escapes `\\1`... require existing capture in Unicode modes; legacy missing backref falls back to octal or identity 8/9. Inside traditional classes decimal backreferences never exist; legacy octal is allowed, Unicode only `\\0` with no following decimal digit.
- `\\c` requires ASCII letter for control escape outside class. Legacy fallback consumes the backslash as a literal and leaves `c` for the next atom; this matters for range endpoints. In legacy classes `\\c` followed by digit or `_` is also a control escape.
- `\\xHH`, `\\uHHHH`, and Unicode `\\u{H...}` valid; malformed x/u can fall back to identity only in legacy mode. Braced Unicode scalar range <=0x10FFFF, including lone-surrogate values; long leading zeroes allowed.
- Adjacent escaped lead/trail `\\uD83D\\uDE00` combine to one character in Unicode modes and remain two code units in legacy. Mixed raw/escaped surrogate composition follows the grammar rather than blindly decoding all output chars.
- Identity escapes legacy broadly allowed; Unicode only SyntaxCharacter or slash outside classes, additionally hyphen in traditional class. `\\-` outside class fails Unicode; `\\/` valid.

## Classes and Unicode properties

- Empty `[]`/`[^]` and dangling class hyphen at either end are valid traditional classes. Validate reversed numeric ranges. Class `\\b` is backspace, not an assertion.
- Traditional `u` range endpoints cannot be class escapes such as `\\d`; Annex B permits `legacy [\\d-z]` and `[z-\\d]` but still rejects reversed ranges when both endpoints are characters.
- `v` supports nested classes, union, intersection `&&`, subtraction `--`, and `\\q{...}` string operands; no mixed operator at the same nesting level. `&&` requires following non-`&`.
- `v` reserved syntax and doubled punctuation enforce exact ClassSetCharacter grammar. A range needs two single characters, never set/string operands.
- MayContainStrings is syntactic: union OR, intersection AND, subtraction first operand; a `\\q` alternative empty or >=2 characters => true, exactly 1 => false.
- Thus `[^\\q{ab}&&a]v` valid, while `[^\\q{ab}--\\q{ab}]v` remains invalid even though the computed set would be empty. Negated outer/nested classes require MayContainStrings=false.
- Unicode property names/aliases use exact case-sensitive whitelist from Tables64/65; gc/sc/scx values use Unicode17 PropertyValueAliases. No loose ICU spelling, spaces, hyphens, or `Is` prefix.
- Properties of strings require `v` and cannot be complemented with `\\P`. Support Table66 names exactly, not arbitrary Unicode properties.

## Resource and runtime review

- Prefer explicit frame stack for pattern groups and nested v classes; no CLR stack overflow for deep valid/invalid patterns.
- Do not allocate a substring of the entire remaining body on every character; aim at linear scan plus necessary duplicate-name/path analysis.
- Exact decimal bounds can compare normalized digit spans by significant length then ordinal characters; no BigInteger/runtime dependency required.
- Guard all end-of-input reads, lone backslash, incomplete Unicode, unclosed group/class/q/property, UTF-16 surrogate accesses, diagnostic length/offset arithmetic.
- Avoid arbitrary rejection presented as syntax error for valid deep patterns. If an explicit resource cap is needed, distinguish its diagnostic from malformed grammar and discuss scope with user.
- Node24.12.0 V8 13.6, Unicode16.0 oracle differs from ECMAScript2026 Unicode17 names. New aliases must be checked against official data rather than Node alone.

## Independent oracle output

`standard-audit-cases.json`: 2,268 cases. `standard-audit-oracle.json`: compile-only strict literal and constructor verdicts, exact source/flags, runtime versions, and spec overrides.

18 known Node24 deviations are explicitly overridden: six cases with very large reversed quantifier bounds, six cases of a same-name capture nested in its outer capture's first alternative, and six cases of the non-whitelisted WSpace property alias. These violate exact MV comparison, MightBothParticipate, and the Table65 whitelist respectively. Constructor-vs-literal differences are recorded separately, never mistaken for body grammar differences.
