# Janglim — TODO

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

## Parser-generator completeness audit (2026-07-01)

Source-grounded audit of Janglim vs mature generators (ANTLR / yacc-bison /
tree-sitter / Roslyn), one agent per dimension, every claim cited to source.
**Every dimension came back PARTIAL** — the core is real and working (lexer with
positions, LALR tables, CST + MeaningUnit AST, FIRST/FOLLOW, error infrastructure
with messages), but each axis stops short of a mature generator in a specific
"last-mile" way.

### Scorecard

| Dimension | Have | Missing (key) |
|---|---|---|
| Lexer | regex tokens, keyword priority + longest-match, positions (offset/line/col), comment/whitespace split, int/hex/bin/real literals | string + escape handling, lexer modes/states, token-filter API |
| Grammar spec | C# API + EBNF reader, `*`/`+`/`?` (code), priority field, left-recursion | EBNF **text** has no `*`/`+`/`?`/groups (code only); no precedence/associativity decls (`%left`/`%right`) |
| Conflict reporting | LALR tables, conflict *detection*, priority resolution | conflicts resolved **silently** — no "your grammar is ambiguous here" warning; no fail-on-conflict |
| Tree / AST | CST + MeaningUnit AST, child iteration | no visitor/listener; some display methods throw `NotImplementedException` |
| Semantics | MeaningUnit tagging → post-parse AST | no reduction-time actions (post-parse only); no attribute system |
| Errors / recovery | positions + expected-token messages (CE0000/CE0004) + panic-mode infra | **opt-in, OFF by default** — throws on first error unless a handler is registered; no universal default handler |
| Output / tooling | runtime table parsing, FIRST/FOLLOW | no parser code-gen (runtime-only); no CLI diagnostics (`-v`); conflict reports in-memory only |

### P1 — high-value gaps

- [ ] **Default error reporter, ON by default.** Today `LRParser.ErrorHandler`
  is null → `LRParser[Parsing].cs` (~line 353) throws `ParsingException` on the
  first error. The generic pieces already exist (`GrammarPrivateLRErrorHandler`,
  `PanicMode`, messages CE0000 "{0} is expected" / CE0004 "{0} was missed"), but
  nothing wires a *universal* default. Goal: a grammar-agnostic default handler
  that fills empty parse-table cells (see `AJ_LRErrorHandlerFactory` for the
  pattern) and is registered automatically, so ANY grammar gets position +
  expected-token errors + panic recovery for free.
  **Directly unblocks editor squiggles (Ket VS Code extension / Playground).**

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

- [ ] **Precedence / associativity declarations.** Only per-production `uint`
  priority exists; no `%left`/`%right`/`%nonassoc`. Goal: declarative operator
  precedence + associativity so expression grammars don't need hand-encoding.

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

### P2 — polish

- [ ] Visitor / listener pattern for tree walking (today: manual `.Items` iteration).
- [ ] Parser **code generation** (runtime-table-only today; no emitted source / table export).
- [ ] Reduction-time semantic actions + synthesized/inherited **attribute** system (today: post-parse AST only).
- [ ] Lexer **modes/states**, string + escape tokenizing, user token-filter API (comment/delimiter are enum-hardwired).
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
