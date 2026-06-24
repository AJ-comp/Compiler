# Orchid

**A general-purpose, embeddable LR/LALR parser-generator engine for .NET.**

Write your grammar in plain C# — no external `.g` files, no build-time codegen step — and get a full
LR pipeline you can look inside: parse tables, FIRST/FOLLOW, step-by-step parse traces, conflict
reports, automatic grammar normalization, optional LGLR backtracking, incremental reparsing, and
pluggable error recovery.

> **Status — early preview (`0.1.0-preview`).** The public API is still unstable and will change
> across `0.x`. Not yet on NuGet; build from source for now. (Planned package id: `Orchidaceae`;
> the brand is **Orchid**.)

---

## Table of contents

- [Why Orchid](#why-orchid)
- [Quick start](#quick-start)
- [Defining a grammar](#defining-a-grammar) — terminals, rules, EBNF operators, semantic actions
- [Parsing](#parsing)
- [Looking inside the parser](#looking-inside-the-parser) — parse trace, FIRST/FOLLOW, conflicts, tables
- [Automatic grammar normalization](#automatic-grammar-normalization)
- [LGLR: parsing conflict-laden grammars](#lglr-parsing-conflict-laden-grammars)
- [Incremental reparse & error recovery](#incremental-reparse--error-recovery)
- [The AJ language — a worked example](#the-aj-language--a-worked-example)
- [Project layout](#project-layout)
- [Status & known limitations](#status--known-limitations)
- [Roadmap](#roadmap)
- [Publications](#publications)
- [License](#license)

---

## Why Orchid

Most parser generators ask you to learn a separate grammar language and run a code-generation step.
Orchid takes a different approach: **the grammar _is_ C#.** Terminals and non-terminals are fields,
productions are built with operators (`+` for sequence, `|` for choice, `?` `*` `+` for EBNF
repetition), and the whole thing is just an object you construct at runtime.

That makes the entire pipeline **inspectable and debuggable from your own code** — you can ask the
parser for its FIRST/FOLLOW sets, dump its ACTION/GOTO table, watch every shift/reduce it makes, or
get a structured report of every conflict. It is well suited to building compilers, linters,
DSLs, and language tooling where you want to *understand* the parse, not just get an AST.

The repository also ships the **AJ language** — a small C#-like systems language whose compiler
(parse → semantic analysis → **LLVM IR**) is built end-to-end on this engine, as the flagship example.

---

## Quick start

```csharp
using Parse;                          // TokenType
using Parse.FrontEnd;                 // MeaningUnit
using Parse.FrontEnd.Grammars;        // Grammar
using Parse.FrontEnd.RegularGrammar;  // Terminal, NonTerminal
using Parse.FrontEnd.Parsers.LR;      // LALRParser
using Parse.FrontEnd.Tokenize;        // Lexer

// 1. Define a grammar
var grammar = new ExprGrammar();

// 2. Build a lexer from the grammar's terminals
var lexer = new Lexer();
foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);

// 3. Lex + parse
var tokens = lexer.Lexing("a + a * a").TokensForParsing;
var result = new LALRParser(grammar, bLogging: false).Parsing(tokens);

Console.WriteLine(result.Success);    // True
```

`ExprGrammar` is defined below.

---

## Defining a grammar

A grammar is a class that derives from `Grammar`. You declare **terminals** and **non-terminals** as
fields, then wire up the productions in the constructor.

```csharp
using Parse;
using Parse.FrontEnd;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.RegularGrammar;

// E -> E + T | T
// T -> T * F | F
// F -> ( E ) | ident
public class ExprGrammar : Grammar
{
    // ----- terminals -----
    private Terminal plus  = new Terminal(TokenType.Operator, "+", false);
    private Terminal mul   = new Terminal(TokenType.Operator, "*", false);
    private Terminal open  = new Terminal(TokenType.Operator, "(");
    private Terminal close = new Terminal(TokenType.Operator, ")");
    // a regex-pattern terminal (an identifier): (type, pattern, display name, isMeaningful, isWordPattern)
    private Terminal ident = new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "ident", true, true);

    // ----- non-terminals (the 'true' marks the start symbol) -----
    private NonTerminal E = new NonTerminal("E", true);
    private NonTerminal T = new NonTerminal("T");
    private NonTerminal F = new NonTerminal("F");

    // ----- optional semantic-action tags (see "Semantic actions" below) -----
    public MeaningUnit Add { get; } = new MeaningUnit("Add");
    public MeaningUnit Mul { get; } = new MeaningUnit("Mul");

    public override NonTerminal EbnfRoot => E;

    public ExprGrammar()
    {
        E.AddItem(E + plus + T, Add);             // E -> E + T
        E.AddItem(T);                             // E -> T
        T.AddItem(T + mul + F, Mul);              // T -> T * F
        T.AddItem(F);                             // T -> F
        F.SetItem((open + E + close) | ident);    // F -> ( E ) | ident
    }
}
```

### Terminals

```csharp
new Terminal(TokenType.Operator, "+", meaning: false);                       // a literal operator
new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "ident", true, true);  // a regex pattern
```

- The **value** is either a literal string (`"+"`) or a regular-expression pattern for the lexer.
- `meaning: false` marks punctuation/keywords that should be matched but dropped from the semantic
  tree (parentheses, operators); meaningful terminals (identifiers, literals) stay.
- The display-name and `isWordPattern` arguments help the lexer and diagnostics.

### Building rules

- **`+`** sequences symbols: `E + plus + T`.
- **`|`** offers a choice: `(open + E + close) | ident`.
- **`AddItem`** appends one production to a non-terminal (call it once per alternative); **`SetItem`**
  replaces all of a non-terminal's productions in one call.
- Pass an optional `MeaningUnit` as the last argument to tag a production with a semantic action.

### EBNF operators: `?`, `*`, `+`

```csharp
Declare.AddItem(constKw.Optional()   + type + name + semicolon, Decl);  // const? type name ;
Block.SetItem(open + statement.ZeroOrMore() + close);                   // { statement* }
Args.SetItem(arg + (comma + arg).OneOrMore());                         // arg (, arg)+
```

`Optional()` / `ZeroOrMore()` / `OneOrMore()` generate the helper rules for you. Optionals are
**absorbed automatically** so that an optional in front of other symbols does not create a
shift/reduce conflict — see [Automatic grammar normalization](#automatic-grammar-normalization).

### Semantic actions

Tag the productions you care about with a `MeaningUnit`. After a successful parse the engine builds
an AST keyed by those tags, which you walk to produce your own tree / IR. (Productions without a tag
— pure "pass-through" rules like `E -> T` — are folded away, so your tree stays clean.)

---

## Parsing

```csharp
var lexer = new Lexer();
foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);

var lexed  = lexer.Lexing("a + a * a");
var result = new LALRParser(grammar, bLogging: false).Parsing(lexed.TokensForParsing);

if (result.Success)
{
    // result.AstRoot  -> the semantic tree (when the grammar uses MeaningUnits)
}
```

`LALRParser` is the supported parser today. (SLR and CLR table builders exist but are incomplete —
see [Known limitations](#status--known-limitations).)

---

## Looking inside the parser

This is where Orchid earns its keep. Everything the parser computes is available to you.

### Step-by-step parse trace

Construct the parser with `bLogging: true` and the result carries a full trace of every action.

```csharp
var result = new LALRParser(grammar, bLogging: true).Parsing(tokens);

foreach (var step in result.Logger)
    Console.WriteLine(step.Unit.Action.Direction);   // Shift, Reduce, Goto, Accept, EpsilonReduce
```

For `a + a * a` the action sequence is:

```
Shift, Reduce, Goto, Reduce, Goto, Reduce, Goto, Shift, Shift, Reduce, Goto,
Reduce, Goto, Shift, Shift, Reduce, Goto, Reduce, Goto, Reduce, Goto, Accept
```

`result.ToParsingHistory` gives the same trace as a full table — stack, lookahead, action, and the
resulting stack at each step. The opening of the `a + a * a` trace:

```
 stack          | input | action            | stack'
 0              | a     | shift 5           | 0 a 5
 0 a 5          | +     | reduce F -> ident | 0 F
 0 F            | +     | goto 3            | 0 F 3
 0 F 3          | +     | reduce T -> F     | 0 T
 0 T            | +     | goto 2            | 0 T 2
 0 T 2          | +     | reduce E -> T     | 0 E
 0 E            | +     | goto 1            | 0 E 1
 0 E 1          | +     | shift 6           | 0 E 1 + 6
 ...            |       |                   |
 0 E 1          | $     | accept            |
```

### FIRST / FOLLOW

```csharp
var ff = new LALRParser(grammar, false).GetFirstAndFollow();
```

```
 symbol | FIRST       | FOLLOW
 E      | ( ident     | $ + )
 T      | ( ident     | $ + ) *
 F      | ( ident     | $ + ) *
```

### Conflict reports

Ask the parser what is ambiguous before you ship the grammar:

```csharp
var conflicts = new LALRParser(grammar, false)
    .CheckAmbiguity()
    .Where(c => c.IsShiftReduceConflict || c.IsReduceReduceConflict);
```

For the classic ambiguous grammar `S -> S + S | a`, the report pinpoints the offending item set —
the state reached after `S + S`, where the parser cannot decide between reducing `S -> S + S` and
shifting another `+`:

```
 state          | items                  | result
 ...
 (after S + S)  | S -> S + S •           | shift-reduce conflict
                | S -> S • + S           |
```

### Parse table & item sets

The full ACTION/GOTO table the parser runs on is available too — for debugging, teaching, or building
a visualizer:

```csharp
var table = new LALRParser(grammar, false).ParsingTable;
```

For `ExprGrammar`, the first few states (`$` = end of input):

```
 state | (       | ident   | +          | $          | *       | )          | E      | T      | F
 I0    | shift 4 | shift 5 |            |            |         |            | goto 1 | goto 2 | goto 3
 I1    |         |         | shift 6    | accept     |         |            |        |        |
 I2    |         |         | reduce E→T | reduce E→T | shift 7 | reduce E→T |        |        |
 I3    |         |         | reduce T→F | reduce T→F | reduce T→F | reduce T→F |      |        |
 ...   |         |         |            |            |         |            |        |        |
```

`parser.Canonical` (the canonical collection of LR item sets) and `parser.AnalysisResult` (the same
as text) are available as well.

> The textual dumps above are **diagnostic-format**; the underlying data — actions, states, sets,
> conflicts — is what the parser actually uses.

---

## Automatic grammar normalization

Before the parse table is built, Orchid normalizes the grammar **once, automatically** — you do not
have to call anything. Two things happen:

1. **Flattening.** The helper rules that `+`, `|`, `?`, `*`, `+` generate are folded away where they
   are redundant, keeping the table small. Your `MeaningUnit`/priority tags ride along.

2. **Optional-absorb.** An optional written `X?` in front of other symbols (e.g. `const? type name`)
   would normally force the parser to decide "is the optional here?" too early, creating a
   shift/reduce conflict. Orchid rewrites it into explicit alternatives so the decision is deferred
   to where it can be made deterministically:

   ```
   Declare -> const? type name ;
   ```
   becomes
   ```
   Declare -> const type name ;     (const present)
   Declare ->       type name ;     (const absent)
   ```

   The result parses under plain LALR with **no conflict and no backtracking** — and the absorb is
   AST-neutral, so your semantic handlers see the same children either way.

This is on by default. A grammar that wants to keep its natural (conflicting) form — for example to
drive it with backtracking instead — opts out:

```csharp
public override bool AbsorbOptionals => false;
```

---

## LGLR: parsing conflict-laden grammars

Sometimes you want to keep a grammar in its natural, conflicting shape (so the parse tree mirrors the
grammar you wrote) and resolve conflicts at parse time instead. Enable selective backtracking on
conflict:

```csharp
var parser = new LALRParser(grammar, false)
    .AddErrorHandler(myErrorHandler)
    .UseBackTrackingOnConflict();
```

On a conflict the parser tries one branch and backtracks to the alternative if it fails. This is the
mechanism the AJ language uses.

---

## Incremental reparse & error recovery

- **Incremental reparse:** after an edit, reparse only the changed token range instead of the whole
  input (`Parsing(lexingData, previousResult)`).
- **Error recovery:** attach an `IErrorHandlable` to recover from a syntax error and keep going
  (insert/replace/delete a virtual token), so one mistake doesn't abort the whole parse.

---

## The AJ language — a worked example

Orchid is dogfooded by the **AJ language**, a small C#-like systems language. Its compiler runs
parse → semantic analysis → **LLVM IR** entirely on this engine, and its grammar shows off optionals,
repetition, semantic actions, and LGLR backtracking on a real language.

See **[README.aj.md](README.aj.md)** for AJ setup (the `ajbuild` / `ajutil` tools), writing `.aj`
code, the problem/diagnostics view, and the LLVM-IR / assembly walkthrough.

---

## Project layout

```
src/
  Common/     shared utilities (AJ.Common)
  FrontEnd/   the parsing engine — the publishable "Orchid":
                Parse, Parse.FrontEnd, .Grammars, .Parsers, .ErrorHandler, .Support
              example front ends built on the engine:
                Parse.FrontEnd.AJ (the AJ language), Parse.FrontEnd.Grammars.MiniC (a MiniC sample)
  MiddleEnd/  IR  (Parse.MiddleEnd.IR)
  BackEnd/    code-generation targets  (Parse.BackEnd)
  Compile/    the AJ compiler driver
  Cli/        command-line tools (ajbuild, ajutil, …)
  Orchid/     NuGet packaging project (bundles the engine assemblies)
Tests/        xUnit suite for the engine  (Parse.FrontEnd.Parsers.Tests)
legacy/       deprecated IDE / WPF projects, kept for history
```

## Status & known limitations

This is an honest `0.x` preview. What works well today:

- **LALR** table construction, parsing, conflict detection, FIRST/FOLLOW, normalization, EBNF
  operators, semantic actions, incremental reparse, error recovery, LGLR backtracking.

Rough edges to be aware of:

- **SLR and CLR are incomplete** — `SLRParser` builds reduce-deficient tables and `CLRParser` is not
  implemented. **Use `LALRParser`.**
- **The end-of-input marker prints a debug placeholder** in the textual dumps. Its display label is
  still being decided (`$`, the usual choice, collides with grammars that use `$` as a real token).
  The underlying data is correct — and terminal labels and the parse-table renderer were just made
  null-safe, so missing labels no longer crash or blank out the dumps.
- **The public API is unstable** and leaks some implementation detail (e.g. `System.Data.DataTable`
  in introspection results, and process-global state in the grammar builder). These are being cleaned
  up over `0.x`.

## Roadmap

- Stabilize the public API (drop `DataTable` from the public surface, remove process-global static
  state, add a clean generic semantic-action hook).
- Polish the diagnostic renderers (tables, FIRST/FOLLOW, item sets).
- An external grammar-file format (`.ebnf` / `.g`) and/or a codegen backend.
- Repair/finish the SLR and CLR table builders.

## Publications

Videos about this project:

- https://www.youtube.com/watch?v=oMhS3l0DyLo&t=187s
- https://www.youtube.com/watch?v=m1ylQsQRPF8&t=29s
- https://www.youtube.com/watch?v=QwPlyto8JAA&t=22s

## License

MIT — see [LICENSE](LICENSE).

The name **Orchid** (난, the orchid) carries a small dedication; see [DEDICATION](DEDICATION).
