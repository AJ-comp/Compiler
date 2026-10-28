# Read-only ordinary-grammar audit

The parser engine and product source files were not modified. Only diagnostic files
under this artifact directory were created. The console references the current source
projects and builds a fresh isolated parser binary, rather than reusing the previous
frozen JavaScript runner.

## Strongest counterexample

Ordinary BNF (C# tree API, no EBNF reader, no predicates, no TypeScript):

```text
S -> G C
C -> R | 'e' S
R -> 'a' | 'b' S
G -> epsilon
```

Its language is `(e|b)*a`; in particular, `a` has the derivation
`S -> G C -> C -> R -> a`. Its alternatives have disjoint FIRST sets,
and the current parser reports zero conflicts. The grammar has 10 LR(0) states,
including the augmented Accept production.

Current parser result: reject `a`.

```text
a: epsilon reduce G -> epsilon
a: goto 2
a: shift 5
EOF: failed (expected set is empty)
```

The completed item `I5: R -> 'a' .` has an empty lookahead set. Therefore the
required Reduce on EOF is missing. `C -> R .` elsewhere has `{EOF}`.

The lexer was independently bypassed: a manually constructed TokenCell with exact
terminal identity `'a'` gives the same rejection at EOF. No regex or lexical-policy
decision participates in that run.

## Controls

- Inlining `R` into `C`, giving `C -> 'a' | 'b' S | 'e' S`, accepts `a`, conflicts 0.
  This grammar defines exactly the same language as the counterexample.
- Inlining only the nullable `G`, giving `S -> C; C -> R | 'e' S; R -> 'a' | 'b' S`,
  also accepts `a`, conflicts 0 (tiny-4). This is another exactly equivalent language.
- Removing only the `C -> 'e' S` alternative, leaving `C -> R`, accepts `a`, conflicts 0.
- Fully removing the original predicate epsilon helper nodes accepts `a` for the
  original larger shape. Keeping ordinary explicit epsilon nodes reproduces rejection.
- The original predicate reproducer is also rejected by the fresh source build.
- Ordinary explicit epsilon helper nodes reproduce the original issue through the
  direct C# tree API, so C# predicate callbacks are not required for the defect.

These controls explain why passing a large corpus is consistent with the defect:
equivalent grammar structures can expose different lookahead dependency/cache orders.
They do not establish that a previous user never encountered this bug.

## Evidence and reproducibility

- `Program.cs`: all audit cases, matrix reduction, direct native grammar setup.
- `results.log`: complete current-build trace; search `tiny-0`, `tiny-1`, `tiny-3`, `tiny-4`,
  and `MANUAL TOKEN NO LEXER`.
- Fresh parser binary:
  `bin/Release/net8.0/Parse.FrontEnd.Parsers.dll`.
- Parser DLL SHA256:
  `FB389B9756053AC05436A8265E3A022B733B8BE901D8088BC406148F100CC205`.
- Unmodified CanonicalRelation.cs SHA256:
  `7BF3508A485DC9D401250A0C69FC77D9F45449CEFC7A68E55F3F46935D81145C`.

Run from the repository root:

```powershell
& C:\Users\AhjLv\.dotnet\dotnet.exe run --project artifacts/typescript/lookahead-audit-plain/PlainAudit.csproj -c Release
```

The diagnostic's EBNF experiments using a textual empty production (`G : ;`) are
rejected by the EBNF reader, and are recorded as READ=False. They are not counted as
parser failures. The decisive ordinary-BNF counterexample is built directly through
the tree API, avoiding that reader syntax entirely.
