# Independent LALR cross-check

This diagnostic never modifies the parser engine, grammar objects, parsing table,
or product source. `Program.cs` references the current product projects and dumps
their observed grammar/states/sets. `canonical_reference.py` independently builds
canonical LR(1) states, merges equal LR(0) cores into LALR states, and compares sets.
The Python implementation does not call Janglim's `PredEx`, FIRST/FOLLOW analyzer,
lookahead builder, or conflict resolver. Bison 3.8.2 XML is a third comparison.

## Small ordinary-BNF counterexample

```text
S -> G C
C -> R | e S
R -> a | b S
G -> epsilon
```

Input: `a`. This grammar has no predicates, TypeScript rules, or conflicts.
Its language is `(e|b)*a`, and the reference accepts `a`.

```powershell
& C:\Users\AhjLv\.dotnet\dotnet.exe run --project artifacts/typescript/lookahead-audit-reference/ReferenceDump.csproj -c Release -- --plain-small > artifacts/typescript/lookahead-audit-reference/plain-small-dump.log
python artifacts/typescript/lookahead-audit-reference/canonical_reference.py artifacts/typescript/lookahead-audit-reference/plain-small-dump.log plain-small artifacts/typescript/lookahead-audit-bison/tiny-bnf.xml
```

Observed:

- Canonical LR(1): 10 states; merged LALR: 10; Janglim LR(0): 10.
- All LR(0) cores match.
- Independent FIRST/FOLLOW vs Janglim: zero differences.
- LR-item structural identity/hash collisions: zero.
- Independent LALR vs Bison: zero differences in 8 reported lookahead items.
- Janglim misses EOF in 5 cached LR items, including `R -> a .`.
- Janglim rejects `a`; independent LALR accepts it.
- C# predicate evaluations: zero.

Machine result: `plain-small-reference-report.json`.

## Original predicate grammar

The original case is also checked independently: 60 canonical LR(1) states merge
to the exact 20 LR(0) cores used by Janglim. Independent FIRST/FOLLOW and the
15 Bison-reported lookahead items all agree. Janglim has 11 underfilled cached
lookahead sets, and no extra lookahead entries. EOF is missing before any
post-`a` predicate can execute.

The canonical parser treats predicate productions structurally as epsilon, then
filters `Never` reductions during its illustrative `a` parse. The small ordinary
BNF case needs no such treatment and establishes the defect independently.

```powershell
& C:\Users\AhjLv\.dotnet\dotnet.exe run --project artifacts/typescript/lookahead-audit-reference/ReferenceDump.csproj -c Release > artifacts/typescript/lookahead-audit-reference/fresh-dump.log
python artifacts/typescript/lookahead-audit-reference/canonical_reference.py
```

Machine result: `canonical-reference-report.json`.

The reported observations establish a reproducible lookahead-table defect for
these grammars. They do not constitute a whole-engine conformance proof or a fix.
