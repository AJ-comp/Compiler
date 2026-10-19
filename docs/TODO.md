# Janglim — TODO

## 0.4.0 release checkpoint (2026-10-19)

The engine now includes typed EBNF tokens, leading source trivia and line-break metadata,
host-registered C# predicates, prediction for two-way Shift / predicate epsilon Reduce
conflicts, and grammar checks for input-free repetition and guarded zero-token left recursion.
The [release notes](../RELEASE_NOTES.md) describe the supported workflows and validation results.

Context-dependent tokenization is the next design topic: a parser requests a token using the
lexical rules allowed at its current grammar position. This requires a token-source interface
and changes to prediction when candidates have different token boundaries. Complete JavaScript
and TypeScript grammars belong to that later stage.

## Lexer: true longest-match tokenization (deferred 2026-06-30)

### Problem
The lexer only *fakes* longest-match. It builds ONE combined regex
`(?<k1>p1)|(?<k2>p2)|…` and runs `Regex.Matches` — but .NET `|` is **ordered**
(the first matching alternative wins), not longest-match. To approximate
longest-match it sorts terminals by **pattern-string length** (longer pattern
first) in `SortTokenPatternList` (`Lexer [Common].cs`).

Pattern-string length ≠ actual matched length, so the heuristic is wrong when a
short-text token has a long pattern (or vice-versa):

- Keyword `'qreg'` (pattern is 4 chars) vs identifier `[_a-zA-Z][_a-zA-Z0-9]*`
  (pattern is 22 chars): both match the 4-char text `"qreg"`, but the longer
  *identifier pattern* sorts first and wins, so the keyword is mis-lexed as an
  identifier. **This specific case is already FIXED** — literals now take
  `bWordPattern=false` so they sort before word-patterns (commit `a23f7e0`).
- Remaining gap: two overlapping **regex** patterns where the shorter pattern
  matches the longer text. E.g. `a+` (matches `"aaaa"`) vs `[a-z]` (matches
  `"a"`) on input `"aaaa"` → pattern-length sort picks `[a-z]` → wrong.

### Goal
Real maximal-munch: at each position pick the terminal with the **longest
actual match**; break ties by priority (the existing sort order).

### Design (from investigation)
Rewrite `Tokenizer.Tokenize` to scan position-by-position:

- At `pos`, try every terminal's regex `\G`-anchored (`regex.Match(input, pos)`,
  require `m.Index == pos`). Keep the longest `m.Length`; on a tie keep the
  higher-priority terminal (iterate `_tokenPatternList` in its sorted order and
  use a strict `>` so the first to reach the max wins).
- Emit a `TokenCell` for the winner and set its `PatternInfo` **directly** — we
  know the terminal, so no combined-regex named-group back-lookup is needed.
  Advance `pos` by the match length.
- Non-matching positions accumulate into one "gap" `TokenCell` (null PatternInfo
  → `NotDefined`), exactly as today, so whitespace filtering is unchanged.
- Preserve line/column tracking exactly, including multi-line matches (block
  comments).

Ripple effects to handle:

- `Lexer [Common].cs`: `GetTokenizeRule` / `GetMatchedPattern` (named-group
  back-mapping) become unnecessary — set `PatternInfo` at tokenize time instead.
  Give the `Tokenizer` the `_tokenPatternList` (each terminal's `\G`-anchored,
  compiled `Regex`).
- `Lexer [Addition].cs` / `Lexer [Deletion].cs`: the 3–5 `_tokenizer.Tokenize(…)`
  call sites (incremental re-lex) — keep the `basisIndex` offset behaviour.
- Keep `SortTokenPatternList` — it is still used as the tie-break order.

### Verification (mandatory — the lexer is foundational)
- Run the FULL test suite (every category), not just `Ebnf`.
- Add longest-match tests: the overlapping-pattern case above; keyword-vs-id;
  operator maximal munch (`==` vs `=`); multi-line block-comment line/col.
- Do it on a branch; merge only when green.

### Why deferred
Not urgent — the common breakage (keyword vs identifier) is already fixed and
pushed. This is a robustness/correctness upgrade (ANTLR-grade) for the rarer
pattern-vs-pattern overlaps. Foundational + wide surface → it deserves a
dedicated, fully-tested effort rather than a rushed edit.

---

## Parser-generator completeness audit (2026-07-01; status updated for 0.4.0)

Source-grounded audit of Janglim vs mature generators (ANTLR / yacc-bison /
tree-sitter / Roslyn), one agent per dimension, every claim cited to source.
The original audit recorded a working core and follow-up work in each dimension.
The scorecard below reflects the current source tree; the completed entries retain
their implementation dates so historical findings and delivered work can be distinguished.

### Scorecard

| Dimension | Have | Missing (key) |
|---|---|---|
| Lexer | regex tokens, keyword priority, source positions, comment/whitespace split, typed EBNF tokens, leading trivia and line-break metadata | true maximal-munch for overlapping regexes (above), context-dependent tokenization, literal-value decoding and escape validation |
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
