# Janglim — TODO

## 0.5.0 release checkpoint (2026-10-21)

Engine 0.5.0 adds parser-driven input and longest-match lexing, together with the
ECMAScript 2026 expression, strict Script and Module language APIs described below.
It builds on the typed tokens, source trivia, predicates and grammar validation shipped in 0.4.0.
The [release notes](../RELEASE_NOTES.md) describe the supported workflows and validation results.

The current source also includes parser-driven token requests: `LexicalGoal` groups
rules, `LexicalPolicy` selects a group, and `Lexer.CreateTokenSource` prepares an input
source for `Parsing(source)`. Prediction keeps a separate cursor and consumed-token
history for each candidate when token boundaries differ. Complete JavaScript and
TypeScript grammars remain follow-up work.

The [ECMAScript 2026 language profile](ko/javascript-expressions.html) supplies
embedded EBNF grammars, Unicode 17 token rules, parser-driven lexical goals, strict
Script early-error checks, templates, and array/object destructuring assignment.
The [Script API](ko/javascript-statements.html) adds ordinary functions, arrow
functions, statement lists, declarations and declaration/parameter binding patterns.
The [function language layer](ko/javascript-functions.html) adds async functions/arrows,
generators, async generators, their object/class method forms and object accessors.
Classes also include fields, private names and static blocks.
The [Module API](ko/javascript-modules.html) adds static import/export, import
attributes, dynamic import, import.meta, top-level await and for-await-of.
RegExpPattern-body validation runs on the final syntax tree in all
three JavaScript entry points.

## JavaScript: async, generators and method bodies (source update 2026-10-20)

Expression, strict Script and Module inputs now connect async functions/arrows,
generators, async generators and object/class methods, including object getters/setters.
The consumer contract remains Parsing then Validate; input is not executed.
The common lexer, LR parser and EBNF reader are unchanged for this language stage.

`JavaScriptFunctionGrammar` prepares Await-off and Await-on EBNF rule variants and
connects function-specific parameter/body resets. Computed method names keep the
outer expression context. Yield expressions retain a common syntax shape; final-tree
validation enforces the owning function kind and parameter/body boundaries.

Language validation checks direct await/yield in parameters, nested function resets,
async-arrow call covers, strict names, duplicate parameters, lexical conflicts,
non-simple parameters with a direct use-strict directive, accessor arity and special
constructor/super conditions. Existing policies/predicates handle async and yield
line-break boundaries, slash goals and ASI with independent token/cursor state.

The [function and method guide](ko/javascript-functions.html) explains the public API,
each form and why these context checks are required. Non-strict Script support and
full specification/Test262 syntax conformance remain follow-up work.

## JavaScript: RegExpPattern body validation (source update 2026-10-20)

The JavaScript layer now validates ordinary, Unicode and Unicode-sets pattern
bodies after the parser has selected the final concrete syntax tree. Literal
boundaries and flags retain their existing lexical checks. Deferring body checks
keeps discarded speculative regexp candidates from rejecting a valid division
parse; the common lexer/parser engine is unchanged.

`JavaScriptRegExpValidation` checks groups, alternatives, captures/backreferences,
assertions, scoped i/m/s modifiers, quantifiers and traditional character classes.
It supports legacy Annex B.1.2 syntax independently of Script/Module strictness,
and uses explicit stacks and exact decimal comparisons. Named captures can share
a decoded name across disjoint alternatives; same-sequence and nested duplicates
are rejected using the specification's structural rule.

`JavaScriptRegExpSets` checks nested v-mode sets, union/intersection/subtraction,
string operands and syntactic MayContainStrings restrictions on negation.
`JavaScriptRegExpProperties` uses ECMAScript's property-name whitelist and pinned
Unicode 17 value aliases generated from hash-verified official data.

All three entry points use the existing Parsing then Validate contract and return
UTF-16 source spans for regexp_syntax errors without modifying the tree or running
a matching engine. Tests cover acceptance/rejection, diagnostic spans, repeated
inputs, speculative reads and expression/Script/Module integration. Independent
engine comparisons are checked against the fixed specification where numeric,
duplicate-name and Unicode alias behaviour differs.

See the [regexp grammar and implementation guide](ko/javascript-regexp.html).

## JavaScript: modules and import expressions (source update 2026-10-20)

`JavaScriptModuleGrammar` is a distinct source-goal entry with the same Grammar,
Policy, GrammarText, CreateTokenSource and Validate contract. A fifth embedded
resource, `ecmascript-2026-modules.ebnf`, supplies static import/export forms,
re-exports, string export names, with attributes and module termination predicates.
The expression and Script entries also connect dynamic import expressions.

`JavaScriptModuleValidation` checks decoded export/attribute names, duplicate
exports, local export resolution and the input/body contexts of import.meta and
await. Statement validation collects imported bindings, named default declarations
and Module lexical/var scopes; top-level function declarations are lexical here.
Module code reserves await as an identifier even in nested ordinary functions.
Top-level await and for-await-of propagate through module blocks and class
heritage/computed names, while synchronous parameters/bodies, field initializers
and static blocks establish their own restrictions.

The existing JavaScript policy and predicates keep responsibility for slash goals
and declaration/ASI boundaries. The common lexer/parser engine is unchanged.
The language layer creates syntax trees and early-error diagnostics; dependency
loading, cross-module linking and JavaScript evaluation belong to the host.
See the [module usage and implementation guide](ko/javascript-modules.html).

## JavaScript: classes (source update 2026-10-20)

The existing expression and Script APIs now connect a fourth embedded EBNF resource,
`ecmascript-2026-classes.ebnf`. It supplies declarations/expressions, extends,
synchronous methods/accessors, instance/static/private fields and static blocks.
Private access, optional private access, private-in checks, super and new.target
are parsed with their language-specific context checks.

`JavaScriptClassValidation` collects class member metadata and private environments,
then validates member names, constructors/accessors, private declarations/duplicates,
private deletion and super/new.target/arguments placement. Class body private names
are visible in computed keys; heritage keeps the outer private environment. Ordinary
functions reset super permissions while arrows inherit lexical permissions.
Statement validation connects class lexical declarations, method parameters/body
scopes, and independent static-block declaration/control-flow scopes.

Class-field termination reuses pure JavaScript predicates and token line-break data.
The JavaScript lexical policy retains ownership of slash-goal selection, with
immutable class/body context tied to each candidate's token history. The common
lexer and parser engine remain unchanged. Parsing success and an empty Validate
result remain the consumer contract; input is never executed by this syntax layer.

See the [class usage and implementation guide](ko/javascript-classes.html).
Async/generator methods are now connected through the function language layer described above.

## JavaScript: non-async arrow functions (source update 2026-10-20)

Both expression and Script entry points accept identifier, parenthesized and empty
arrow parameter lists, defaults, nested destructuring and final rest parameters.
Expression and block bodies support nested arrows and existing function statements.
The grammar enforces the no-line-terminator boundary before `=>`, separates a block
body from an object-valued expression, and carries NoIn into expression bodies in a
classic-for initializer. Parentheses explicitly restore ordinary expression syntax.

`JavaScriptArrowFunctions` refines the existing expression cover tree into binding
parameters, preserving the concrete tree and source. Only actual parameter nodes
receive binding-pattern rules; defaults and computed keys remain ordinary expressions.
Validation rejects invalid binding targets, rest placement/defaults, duplicate names,
parameter/body lexical conflicts and non-simple parameters with a direct use-strict
directive. Nested arrows establish declaration and control-flow boundaries.

JavaScript termination predicates distinguish an arrow block followed by a new-line
`(value)` statement from a parenthesized arrow called as `(x => {})(value)`. Expression
bodies can still continue calls across lines. `JavaScriptSlashContext` derives immutable
delimiter context from the previous token when the source reads a token. The lexical
policy and termination predicates share that context through a weak cache keyed by
token object identity, keeping different tokenizations and speculative branches apart.
The common lexer and parser engine remain unchanged for this language stage.

See the [arrow parameters, bodies and statement boundaries guide](ko/javascript-statements.html#arrows).

## JavaScript: ordinary functions, statements and binding patterns (source update 2026-10-20)

`JavaScriptScriptGrammar` accepts strict Script statement/declaration lists, while
`JavaScriptExpressionGrammar` remains the single-expression entry point and now
also accepts ordinary function expressions with their parameters and statement bodies.
Both APIs expose `Grammar`, `Policy`, `GrammarText`, `CreateTokenSource` and `Validate`.
Consumers check parsing success and then inspect the successful tree with `Validate`.

The language layer now covers ordinary function declarations/expressions, var/let/const,
blocks and expression statements, if/else, while/do/for/in/of, switch, try/catch/finally,
throw/return, break/continue/labels and debugger. Declaration, parameter and catch
patterns use dedicated BindingIdentifier/BindingPattern syntax. The rules distinguish
creating names from assigning to existing member targets, preserve ordinary expression
checks in defaults/computed keys, and enforce rest-position restrictions.

`JavaScriptStatementValidation` checks strict binding names, parameter/lexical/catch
duplicates, lexical-var and parameter-body conflicts, const initializers, non-simple
parameter lists with a direct use-strict directive, and control-flow destinations.
Nested functions establish independent declaration and control-flow boundaries.
For/in and for/of assignment targets also receive the existing assignment-pattern checks.

ASI uses JavaScript EBNF termination productions with pure registered predicates.
The language layer reuses existing trivia/line-break metadata and predicate prediction;
it does not add synthetic semicolon tokens to the common engine. Regression cases
distinguish `a` followed by `(b)` on a new line, restricted return/throw boundaries,
postfix updates, bare declarations and mandatory classic-for separators.

The JavaScript lexical policy owns Div/RegExp selection. When a merged LR row admits
both goals, `JavaScriptSlashContext` supplies the immutable context attached to the
candidate's previous token. That context distinguishes control/call parentheses,
block/object braces and ordinary/arrow function bodies. Sources preserve original
tokens and positions, and each prediction candidate retains its own cursor/history.
The common LR/lexer implementation is reused unchanged for this language stage.
See the [usage and implementation guide](ko/javascript-statements.html).

Async functions/arrows, generators and their method forms, plus object methods/accessors,
are now connected by the function language stage above. RegExpPattern body checks run
in Validate. Non-strict Script support and a full specification/Test262 syntax audit
remain follow-up language work.

## JavaScript: destructuring assignment (source update 2026-10-20)

The expression profile now supports array/object assignment patterns with nested
patterns, defaults, elisions, and rest targets. Patterns belong to plain `=`;
compound assignment and update operators retain simple-target requirements.
Rest must be last, without a trailing comma or direct initializer. Array rest can
contain a nested pattern; object rest requires an assignable identifier or property.

The EBNF uses array/object cover grammar and `CoverInitializedName` for shorthand
defaults. After parsing, `Validate` identifies actual pattern nodes and checks
targets, rest restrictions, and strict-mode assignment conditions. It preserves
the concrete `ArrayLiteral`/`ObjectLiteral` nodes and original source text.
`CoverInitializedName` remains an error in ordinary object literals, and duplicate
`__proto__` checks apply to actual object literals rather than assignment patterns.
Default right-hand sides, computed keys, and member-target bases remain ordinary
expressions and receive the existing checks. This distinction also keeps nested
real assignments valid inside defaults and computed keys.

Regression tests cover accepted/rejected targets, parenthesized simple targets
versus parenthesized patterns, nested defaults/rest, strict identifiers, template
and slash interactions, precise diagnostics, and tree preservation. See the
[destructuring walkthrough](ko/javascript-expressions.html#destructuring) and
[grammar source notes](ko/javascript-expressions.html#grammar-source). Variable declarations
and function parameters now use [dedicated binding-pattern contexts](ko/javascript-statements.html#binding-patterns).

## JavaScript: template literals (source update 2026-10-20)

The expression profile now parses no-substitution templates, multiple `${...}`
substitutions, nested templates, and tagged templates. The existing parser-driven
interface is unchanged: the parser passes its read-only context to the policy,
the policy selects a registered rule group, and the parser requests a token from
the source using that returned group.

The JavaScript policy combines two slash goals (Div/RegExp) with two brace contexts
(ordinary `}` / template continuation), giving four lexical rule groups. The
JavaScript token-source wrapper records interpolation brace depths in immutable
`TokenCursor.LexicalState` values. Object braces change the active substitution's
depth; nested templates add and remove their own substitution contexts. Braces in
string/regex tokens or skipped comments do not change that depth. Each speculative
parser candidate keeps its own cursor, and the real parser adopts the next cursor
only on Shift.

`Validate` rejects invalid escapes in untagged template text while allowing the
same spellings in tagged template text, as ECMAScript requires. Nested untagged
templates inside a tagged template's substitutions still receive their own checks.
Tokens preserve original delimiters, escapes and line endings; cooked string values
and tag execution remain separate from syntax parsing. See the
[template walkthrough](ko/javascript-expressions.html#templates) for examples and
the reason for this lexical/early-error split.

## Lexer: longest-match tokenization (source update 2026-10-20)

The lexer now compares the **actual match length** returned by each eligible token
regex at the current source position. The longest match wins. Equal-length matches
use the existing token-category priority, including literal keywords before regex
identifiers; rules in the same category use registration `Key` order. Pattern-string
length no longer determines priority.

Automatic `\b` wrapping has been removed. For example, with keyword `if` and
identifier `[$A-Za-z_][$0-9A-Za-z_]*`, `if` is a keyword, while `ifx`, `if$`, `$name`,
and `name$` are identifiers. The same length comparison selects `==` over `=` and
`a+` over `[a-z]` on input `aaaa`.

This selection applies to both `Lexer.Lexing` and `source.Peek`. The source compares
only rules in the selected `LexicalGoal`, plus shared skipped-whitespace/comment
rules. Each individual regex retains .NET semantics: authored `\b`, ordered
alternatives, and lazy quantifiers still behave as written. In particular, a lazy
block-comment regex still stops at its first closing delimiter. Longest-match
selection compares the matches returned by different rules; it does not reinterpret
the alternatives or quantifiers inside one regex.

Regression verification covers keyword/identifier boundaries, overlapping regexes,
equal-length priorities, operator prefixes, active-goal filtering, comments, source
positions, and incremental lexing. See the
[lexer selection explanation](ko/parser-driven-lexing.html#token-selection) for the
rule-by-rule examples.

---

## Parser-generator completeness audit (2026-07-01; status updated for 0.5.0)

Source-grounded audit of Janglim vs mature generators (ANTLR / yacc-bison /
tree-sitter / Roslyn), one agent per dimension, every claim cited to source.
The original audit recorded a working core and follow-up work in each dimension.
The scorecard below reflects the current source tree; the completed entries retain
their implementation dates so historical findings and delivered work can be distinguished.

### Scorecard

| Dimension | Have | Missing (key) |
|---|---|---|
| Lexer | longest returned regex match, stable equal-length priority, parser-driven lexical goals, source positions, comment/whitespace split, typed EBNF tokens, leading trivia and line-break metadata | literal-value decoding and language-specific escape validation |
| Grammar spec | C# API + EBNF text with groups and `*`/`+`/`?`, registered C# predicates, ordinary LR left recursion, progress and guarded-reentry validation | optional precedence/associativity declaration syntax; grammar structure already expresses both |
| Conflict reporting | opt-in conflict reports and fail-on-conflict, on-demand prediction for a two-way Shift / predicate epsilon Reduce cell | automatic reporting for general conflicts and prediction beyond the supported two-action shape |
| Tree / AST | CST + MeaningUnit AST, child iteration | no visitor/listener; some display methods throw `NotImplementedException` |
| Semantics | MeaningUnit tagging → post-parse AST, pure host-registered grammar predicates | reduction-time semantic actions and an attribute system |
| Errors / recovery | default positioned error reporter, predicate/prediction diagnostics, pluggable recovery infrastructure | recovery strategies remain host-provided |
| Output / tooling | runtime table parsing, FIRST/FOLLOW | no parser code-gen (runtime-only); no CLI diagnostics (`-v`); conflict reports in-memory only |

### P1 — high-value gaps

- [x] **Default error reporter, ON by default.** ✅ **DONE 2026-07-01 (Minimal-B baseline).**
  Before: `LRParser.ErrorHandler` was null → `ParsingFailedProcess` called `ErrorHandler?.Call(...)`,
  so with no handler a failed parse threw `ParsingException` and came back with an **empty**
  `AllErrors` — no position for a squiggle. Now a grammar-agnostic `DefaultLRErrorHandler`
  (`LR/DefaultLRErrorHandler.cs`) is used whenever no custom handler is registered
  (`ErrorHandler ?? _defaultErrorHandler` in `LRParser.cs`). On failure it records ONE positioned
  error — the unexpected token, carrying its `StartIndex/EndIndex` — as a `ParsingErrorInfo`
  (code CE0001) into the failing block, then reports "not recovered" so the parser **stops exactly
  as before** (behaviour unchanged; only the empty-result gap is filled). Verified: `S : 'a' 'b'`
  on `"a a"` → `AllErrors` has one error at the 2nd `a` (offset 2); valid `"a b"` → none. Same
  plug-in point can later grow panic-mode recovery + an "expected {…}" message + multiple errors.
  **Directly unblocks editor squiggles (Ket VS Code extension / Playground).**
  Files: `LR/DefaultLRErrorHandler.cs`, `LR/LRParser.cs`, test `Conformance/DefaultErrorHandlerTests.cs`.

  - **Root-cause fix shipped alongside (resource-namespace mismatch).** Building the default
    handler surfaced a latent bug from the `Parse.* → Janglim.*` rename (commit `ec6e7cd`): the
    code/`.resx` Designers request `Janglim.FrontEnd.*.Properties.*` resources, but each project's
    `RootNamespace` still defaulted to its old `Parse.FrontEnd.*` assembly name, so the *embedded*
    `.resources` kept the old prefix → **every** resource lookup threw
    `MissingManifestResourceException` (swallowed by the parser's catch → empty result). Fixed by
    setting `<RootNamespace>Janglim.FrontEnd.*</RootNamespace>` in the 4 affected projects
    (`Parse.FrontEnd`, `Parse.FrontEnd.Parsers`, `Parse.FrontEnd.Grammars`, `Parse.FrontEnd.Support`).
    Bonus: this also cleared the **10 pre-existing `Ex8_10Grammar` test failures** (same root cause) —
    the suite is now fully green (116/0, 1 skip). At the time of this audit, `legacy/` and
    `src/Cli/CommandPrompt.Compiler` were still broken from the same rename
    (code-namespace/missing-type errors, unrelated to resources) — out of scope here.
    **Cleanup update, 2026-10-11:** the unused top-level `AJStudio` and
    `CommandPrompt.Compiler` executables were removed, followed by the unused
    `ApplicationLayer.Views`, `ApplicationLayer.ViewModels`, `ApplicationLayer.Models`, `ApplicationLayer.Common`,
    `Parse.WpfControls.SyntaxEditor`, and `Parse.WpfControls`
    libraries; the original author's designs and source
    remain in Git history. The projects with `ApplicationLayer.Common` namespace/project-reference
    errors and the XAML reference errors have been removed; the historical solution remains
    outside the supported default build and CI scope.

- [x] **Report grammar conflicts to the author.** ✅ **DONE 2026-07-01 (opt-in surface).**
  Detection already existed (`CheckAmbiguity()` → `AmbiguityCheckResult`) but was only reachable
  via the CLI `print` command. Added an **opt-in reporting API on `LRParser`** (base, so every
  LR/LALR/SLR/CLR parser gets it), computed lazily from the built table: `Conflicts`
  (the shift/reduce + reduce/reduce items), `HasConflicts`, `ConflictReport` (readable multi-line
  state + kind + seeing-symbol), and `ThrowIfConflicts()` → `GrammarConflictException` (opt-in
  hard-stop / fail-on-conflict). Zero behaviour change — nothing runs unless you ask. Verified
  empirically: ambiguous `E : E '+' E | E '*' E | id` → HasConflicts true; the layered
  (unambiguous) arithmetic grammar → none; ThrowIfConflicts throws only on conflict. Full suite
  unchanged (still the 10 pre-existing resource-load failures). Files:
  `LR/LRParser.cs`, `LR/GrammarConflictException.cs`, test `Conformance/ConflictReportingTests.cs`.
  Possible follow-up: an *automatic* warning/opt-out-able on every build (currently opt-in only).

- [x] **EBNF text quantifiers & groups.** ✅ **DONE 2026-07-01.** Added `*`/`+`/`?`
  and `( … )` grouping to the EBNF text reader. Approach A (reuse the C# API machinery):
  the reader lowers each factor's quantifier via `Symbol.ZeroOrMore/OneOrMore/Optional`
  and a group to an anonymous NT; the auto-generated NTs self-register through the base
  `Grammar` AutoGenerator callback, and `EbnfGrammar` now calls `Optimization()` (absorb
  optionals / flatten) — so EBNF text and the C# API produce identical grammars, and the
  `?`-before-context conflict is auto-absorbed. Verified: 5 new xUnit theories
  (`id (',' id)*`, `w+`, `('=' id)?`, `('x'|'y')+`, trailing-sep rejection) green; full
  suite unchanged (the 10 pre-existing `Ex8_10Grammar` resource-load failures are
  unrelated). Files: `EbnfModel.cs`, `EbnfGrammarReader.cs`, `EbnfGrammar.cs`.

- [x] **EBNF text token kinds.** **DONE 2026-10-11; included in 0.4.0.** Token rules now accept
  an optional, case-sensitive `Identifier`, `StringLiteral`, `CharLiteral`, `NumberLiteral`,
  `Comment`, or `Reject` before the double-quoted regex. Omitted kinds keep the existing
  `Identifier` behavior. Literal rules reuse the existing raw-regex terminal types;
  the grammar tree API is unchanged. `Comment` and `Reject` are lexical-only rules and
  cannot enter productions. Comments are skipped; consumers check lexed terminals with
  `EbnfGrammar.IsRejectedToken` and reject matching input before parsing. The MCP adapter
  performs this check automatically, including unclosed-comment rejection.
  A doubled double quote inside a regex represents one double quote, while backslashes
  pass through unchanged. Single-quoted grammar literal syntax remains unchanged.
  Literal-value decoding, complete C support, and validation beyond supplied regexes remain
  separate work. A [C11 Core syntax profile and regression examples](../grammars/c/README.md)
  now provide a bounded C baseline. See the [EBNF token syntax](../README.md#ebnf-text-token-declarations).

- [x] **Source trivia and registered predicates.** **Included in 0.4.0.** Tokens retain
  leading whitespace/comments and line-break information. C# hosts register pure callbacks
  for `{Name()}?`, evaluated at their grammar boundary. The real parser selects one action
  after prediction at an original two-way Shift / predicate epsilon Reduce conflict.
  Prediction uses work/storage limits and reports failure, ambiguity, cycles, and exhausted
  resources. Grammar preparation rejects nullable `*`/`+` bodies and zero-token recursive
  paths that pass a condition. Ordinary LR left recursion and consuming repetitions remain
  supported. The .NET host supplies callbacks; web and MCP inputs use the reader without a
  callback registry.

### P2 — polish

- [ ] Optional operator precedence/associativity declarations such as `%left` and `%right`.
  Existing EBNF rule structure and recursion direction already express these choices;
  declarations would provide a shorter authoring form.
- [ ] Visitor / listener pattern for tree walking (today: manual `.Items` iteration).
- [ ] Parser **code generation** (runtime-table-only today; no emitted source / table export).
- [ ] Reduction-time semantic actions + synthesized/inherited **attribute** system (today: post-parse AST only).
- [ ] Lexer **modes/states**, literal-value decoding and language-specific escape validation, user token-filter API
  (comment/delimiter are enum-hardwired). Raw-regex string/char literal *tokens* shipped in
  0.3.0-preview.1 (`StringLiteral`/`CharLiteral`); the EBNF text route was added in the source
  tree on 2026-10-11, as noted above.
- [ ] CLI diagnostics (`-v` / report export, FIRST/FOLLOW dump) and fill the
  `NotImplementedException` stubs (`ParseTreeTerminal.ToTreeString/ToGrammarString`,
  CLR `CheckAmbiguity`).

### Notes
- All 7 dimensions verified against source — cited files include `Tokenizer.cs`,
  `TokenCell.cs`, `Lexer [Common].cs`, `EbnfGrammarReader`, `LRParser[Parsing].cs`,
  `GrammarPrivateLRErrorHandler.cs`, `PanicMode.cs`, `AJ_LRErrorHandlerFactory.cs`.
- Overall: solid core for a personal project; gaps are production-grade last-mile,
  not fundamental holes. **P1** items lift it toward ANTLR/Roslyn-grade ergonomics;
  the error-default item is the one that unblocks current Ket editor tooling.
