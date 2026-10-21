# Janglim

**A general-purpose, embeddable LALR parser-generator engine for .NET.**

[![NuGet](https://img.shields.io/nuget/v/Janglim.svg?label=NuGet)](https://www.nuget.org/packages/Janglim)
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
![.NET Standard 2.1](https://img.shields.io/badge/.NET-netstandard2.1-512BD4)
[![Playground 1.1.0](https://img.shields.io/badge/playground-1.1.0-2ea44f)](https://janglim.mythosia.net)

Define a grammar in plain C# or EBNF text and get a full LR pipeline you can look inside:
parse tables, FIRST/FOLLOW, step-by-step parse traces, conflict reports, automatic grammar normalization, optional LGLR
backtracking, incremental reparsing, and pluggable error recovery.

> **Engine `0.5.0` · web Playground `1.1.0` · remote MCP `0.1.0`.**
> These products are versioned independently. `LALRParser` is the supported engine path;
> SLR and CLR remain experimental and incomplete. The engine's public API may evolve across `0.x`.

Install the engine:

```bash
dotnet add package Janglim --version 0.5.0
```

See the [release notes](RELEASE_NOTES.md) for the release scope and
[changelog](CHANGELOG.md) for version history. Maintainers can follow the
[GitHub Actions release guide](.github/RELEASING.md) to publish the engine and deploy the Playground.

Engine `0.5.0` adds:

- [Longest-match lexing](#token-selection): compare the text matched by each registered rule,
  resolve equal lengths by rule priority, and use regexes without adding automatic `\b` boundaries.
- [Parser-driven lexing](#parser-driven-lexing): register token-rule groups in a `LexicalPolicy`,
  create a token source, and let that policy select a group before each requested token.
  Prediction candidates keep independent input cursors and lexical state.
- [JavaScript grammar profiles](#javascript-grammar-profiles) for ECMAScript 2026 strict Script
  expressions, Scripts, and Modules, including templates, destructuring, functions, classes,
  imports/exports, async functions, generators, and regular-expression body validation.
- Illustrated guides for token exchange, cursor ownership, and JavaScript language support.

## Quick start

One copy-paste-runnable block — an expression grammar in EBNF text, lexed and parsed:

```csharp
using System;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;

var read = EbnfGrammarReader.Read(@"
    Expr   : Expr '+' Term | Term ;
    Term   : Term '*' Factor | Factor ;
    Factor : '(' Expr ')' | id ;
    id     := ""[a-zA-Z]+"" ;");

if (!read.Success)
    throw new InvalidOperationException(string.Join(Environment.NewLine, read.Errors));

var grammar = read.Grammar;
var lexer = new Lexer();
foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);

var parser = new LALRParser(grammar);
var result = parser.Parsing(lexer.Lexing("a + a * a").TokensForParsing);

Console.WriteLine(result.Success);   // True
```

Real projects usually define the grammar as a C# class instead — terminals and rules as code,
with semantic actions attached. That form is the heart of the engine:
[Defining a grammar](#defining-a-grammar).

## Try it online

**🌿 [Live playground →](https://janglim.mythosia.net)** — define an EBNF grammar,
give it input, and watch the LALR parser work: the ACTION/GOTO table, the parsing stack at each step,
the syntax tree building up shift by shift, and conflict reports.

**Playground 1.1.0** includes a redesigned React interface with shadcn components,
an interactive tree, execution playback, table filtering, light/dark themes, and an expandable
results workspace. Select a tree node to highlight its matching grammar symbols and exact input
range; repeated tokens are distinguished by their source positions. Editing the source marks the
previous result as out of date until the next run.

Its workspace files are the first step toward Janglim Studio:

1. Save your current EBNF grammar and input string to a `.janglim.json` file.
2. Open a saved file to restore both editors and run the existing parser again.
3. If you have unsaved edits, confirm their replacement in the workspace before opening the file.

The tagged JSON format has its own version. Invalid or unsupported files leave your current work
intact. Files stay on your device and are read in the browser; parse results are rebuilt from the
saved source rather than stored in the file.

The interface supports **14 languages**: English, Korean, Japanese, Russian, Simplified Chinese,
Traditional Chinese (Taiwan), Ukrainian, Vietnamese, German, French, Thai, Spanish, Portuguese,
and Italian. It follows the browser language by default and remembers an explicit choice.
Grammar symbols, parser actions, and engine diagnostics keep their original identifiers and text.

Parsing runs entirely in your browser through **Blazor WebAssembly**, with the React interface in
[`samples/Playground`](samples/Playground). Builds from this source tree use engine `0.5.0`.
The web and engine release numbers are independent.

**📖 [Read the manual →](https://aj-comp.github.io/Compiler/)** — a from-scratch guide to LR/LALR
parsing, built around this engine.

## Use with AI coding tools

The independently versioned **MCP `0.1.0`** service in
[`services/Janglim.Mcp`](services/Janglim.Mcp) exposes five tools over stateless
Streamable HTTP: `list_grammars`, `get_grammar`, `validate_grammar`, `parse_input`, and
`run_tests`. Connect Claude Code, Codex, or another MCP client to retrieve a baseline,
validate EBNF, inspect LALR table conflicts, parse examples, and check positive/negative
cases while the AI edits the grammar in your project.

`get_grammar` with `grammarId: "c11-core"` returns the built-in
[C11 Core profile](grammars/c/README.md), its version, scope, and SHA-256 fingerprint.
The released baseline is read-only: the client edits its own copy and submits that text
to the validation and parsing tools. User requests cannot update the shared baseline,
and the service does not persist user grammar copies or projects. Baseline updates are
controlled repository changes shipped with a new service release.

The service runs the existing engine in bounded, isolated worker processes. It accepts text
and returns structured results; project files and AI reasoning remain in the client. This
first version uses a shared Bearer key for remote access and does not include OAuth, parser
source generation, or automatic synchronization with the web Playground.

The remote service is deployed to Azure Container Apps:
[Janglim MCP endpoint](https://janglim-mcp.wonderfulsand-6b5e00a1.koreacentral.azurecontainerapps.io/mcp).
Connect with an operator-provided Bearer key through the `JANGLIM_MCP_API_KEY` client environment
variable; remote users do not need the .NET SDK. The existing NuGet/Playground release pipeline
validates MCP but does not redeploy it. The included deployment script supports manual deployment
and preserves the existing key. See the [MCP guide](docs/MCP.md) for client setup, limits, Azure
operations, and remote verification. Local development still uses the .NET 10 SDK and included container.

---

## Table of contents

- [Why Janglim](#why-janglim)
- [Use with AI coding tools](#use-with-ai-coding-tools) — remote MCP validation, parsing, and examples
- [Defining a grammar](#defining-a-grammar) — terminals, rules, EBNF operators, semantic actions
- [Registered predicates](#registered-predicates) — host C# conditions and selective conflict prediction
- [Parsing](#parsing)
- [Token selection](#token-selection) — longest returned match and equal-length priority
- [Parser-driven lexing](#parser-driven-lexing) — policy selection and lazy token requests
- [JavaScript grammar profiles](#javascript-grammar-profiles) — strict Script and Module inputs
- [Lexer source trivia](#lexer-source-trivia) — skipped source text and line-break information
- [Looking inside the parser](#looking-inside-the-parser) — parse trace, FIRST/FOLLOW, conflicts, tables
- [Automatic grammar normalization](#automatic-grammar-normalization)
- [LGLR: parsing conflict-laden grammars](#lglr-parsing-conflict-laden-grammars)
- [Incremental reparse & error recovery](#incremental-reparse--error-recovery)
- [The AJ language — a worked example](#the-aj-language--a-worked-example)
- [Project layout](#project-layout)
- [Building from source](#building-from-source)
- [Status & known limitations](#status--known-limitations)
- [Roadmap](#roadmap)
- [Publications](#publications)
- [License](#license)

---

## Why Janglim

Most parser generators ask you to learn a separate grammar language and run a code-generation step.
Janglim takes a different approach: **the grammar _is_ C#.** Terminals and non-terminals are fields,
productions are built with operators (`+` for sequence, `|` for choice, and `.Optional()` /
`.ZeroOrMore()` / `.OneOrMore()` for EBNF repetition), and the whole thing is just an object you
construct at runtime. (For quick experiments there is also the plain EBNF text form you saw above.)

That makes the entire pipeline **inspectable and debuggable from your own code** — you can ask the
parser for its FIRST/FOLLOW sets, dump its ACTION/GOTO table, watch every shift/reduce it makes, or
get a structured report of every conflict. It is well suited to building compilers, linters,
DSLs, and language tooling where you want to *understand* the parse, not just get an AST.

The repository also ships the **AJ language** — a small C#-like systems language whose compiler
(parse → semantic analysis → **LLVM IR**) is built end-to-end on this engine, as the flagship example.

---

## Defining a grammar

A grammar is a class that derives from `Grammar`. You declare **terminals** and **non-terminals** as
fields, then wire up the productions in the constructor.

```csharp
using Janglim;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.RegularGrammar;

// E -> E + T | T
// T -> T * F | F
// F -> ( E ) | ident
public class ExprGrammar : Grammar
{
    // ----- terminals -----
    private Terminal plus  = new Terminal(TokenType.Operator, "+", false);
    private Terminal mul   = new Terminal(TokenType.Operator, "*", false);
    private Terminal open  = new Terminal(TokenType.Operator, "(", false);
    private Terminal close = new Terminal(TokenType.Operator, ")", false);
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
new Terminal(TokenType.Operator, "+", meaning: false);                              // a literal operator
new Terminal(TokenType.Identifier, "[_a-zA-Z][_a-zA-Z0-9]*", "ident", true, true);  // a regex pattern
new Terminal(TokenType.Literal.StringLiteral, "\"[^\"\\r\\n]*\"", "string", true, true);  // a string literal
```

- The **value** is either a literal string (`"+"`) or a regular-expression pattern for the lexer.
- `meaning: false` marks punctuation/keywords that should be matched but dropped from the semantic
  tree (parentheses, operators); meaningful terminals (identifiers, literals) stay.
- **String / char literal tokens** use the `StringLiteral` / `CharLiteral` token types.
  Their regexes describe the complete token, including quote characters. Regex rules of every
  kind use the supplied pattern without automatic `\b` boundaries.
- The display-name argument labels diagnostics. `isWordPattern` distinguishes regex rules from
  literal rules for display and equal-length priority; it does not add word boundaries.

### EBNF text token declarations

Token rules accept an optional, case-sensitive kind before the double-quoted regex:

```ebnf
Value : id | text | ch | number ;
id    := Identifier "[a-zA-Z_][a-zA-Z_0-9]*" ;
text  := StringLiteral """[^""\r\n]*""" ;
ch    := CharLiteral "'[^'\r\n]'" ;
number := NumberLiteral "[0-9]+(?:\.[0-9]+)?" ;
comment := Comment "//[^\r\n]*|/\*[\s\S]*?\*/" ;
invalid_comment := Reject "/\*(?:(?!\*/)[\s\S])*\z" ;
```

| Kind | Lexer behavior |
| --- | --- |
| `Identifier` | Declared regex, mapped to the identifier token kind |
| `StringLiteral` | Declared regex, mapped to `TokenType.Literal.StringLiteral` |
| `CharLiteral` | Declared regex, mapped to `TokenType.Literal.CharLiteral` |
| `NumberLiteral` | Declared regex for numeric tokens |
| `Comment` | Declared regex; lexical-only tokens skipped before parsing |
| `Reject` | Declared regex; lexical-only tokens marking invalid input |

Omitting the kind selects `Identifier`: `id := "[a-zA-Z]+" ;` is equivalent to
`id := Identifier "[a-zA-Z]+" ;`. Other kind names are rejected.

`Comment` and `Reject` rules cannot be referenced in productions. After lexing, check each
token's terminal with `EbnfGrammar.IsRejectedToken` and reject matching input before invoking
the parser. The MCP adapter performs this check automatically. For example, `invalid_comment`
prevents an unclosed block comment from being parsed as division and dereference operators.

Inside a double-quoted regex, write `""` for one literal double quote. Backslashes are passed
unchanged to the regex engine: `\r` and `\n` above are regex escapes, not an extra EBNF escape
layer. Single-quoted grammar literals retain their existing syntax. When embedding EBNF in
C# or JSON, apply that host format's escaping separately.

The example patterns recognize simple quoted strings and single characters. The token kind
does not add C escape validation or decode literal values; the regex defines what is accepted.
The repository includes a [C11 Core syntax profile and regression examples](grammars/c/README.md)
for editing selected C constructs. Its documented scope excludes preprocessing, `typedef`
context handling, and semantic type checking; it is not a complete C compiler.

### Registered predicates

When embedding the engine in a .NET application, register a C# condition and call it from EBNF
with **`{Name()}?`**. The following grammar accepts `throw value;` and rejects a line break
between `throw` and `value`:

```csharp
using System;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;

var options = new EbnfReaderOptions()
    .RegisterPredicate("NoLineBreak", context => !context.NextToken.HasLineBreakBefore);

var read = EbnfGrammarReader.Read(@"
    Throw : 'throw' {NoLineBreak()}? id ';' ;
    id := ""[a-zA-Z_][a-zA-Z_0-9]*"" ;
    comment := Comment ""/\*[\s\S]*?\*/"" ;", options);

if (!read.Success)
    throw new InvalidOperationException(string.Join(Environment.NewLine, read.Errors));

var grammar = read.Grammar;
var lexer = new Lexer();
foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
var parser = new LALRParser(grammar);

bool Parse(string source) => parser.Parsing(lexer.Lexing(source).TokensForParsing).Success;

Console.WriteLine(Parse("throw value;"));    // True
Console.WriteLine(Parse("throw\nvalue;"));  // False
```

A predicate consumes no input. Its `PredicateContext` exposes the previous token, the next token,
the input token index, and the end-of-input flag. Conditions must be pure: prediction and actual
parsing may evaluate a condition more than once. A line break inside a skipped comment is also
visible through `HasLineBreakBefore`.

When an original table cell contains exactly **one Shift and one predicate-bearing ε Reduce**,
and both actions remain allowed after checking the condition, the parser predicts ahead until
it can select one first action. Prediction uses copied state
stacks and input positions; the actual parser then executes that action. It has resource limits
and reports failed or ambiguous predictions. Other conflict forms retain their existing policy.

Grammar construction checks progress before building the table:

- `({Allow()}?)*` and `({Allow()}?)+` produce a grammar error because an iteration can consume
  no token. `('x' {Allow()}?)*` and `('x' {Allow()}?)+` are supported.
- `A : {Allow()}? A 'x' | 'x';` produces a grammar error because the predicate lies on a
  zero-token path back to A. Ordinary left recursion such as `A : A 'x' | 'x';` and a predicate
  after recursive entry, such as `A : A {Allow()}? 'x' | 'x';`, remain supported.

These structural checks never call the condition. EBNF readers return grammar diagnostics;
native C# grammars throw the corresponding exception during `Optimization()` or parser construction.
See the
[predicate repetition rules](docs/ko/predicate-repetition-rules.html),
[left-recursion validation](docs/ko/predicate-left-recursion-validation.html), and
[prediction walkthrough](docs/ko/functions/select-first-action.html) for detailed examples.

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
var result = new LALRParser(grammar).Parsing(lexed.TokensForParsing);

if (result.Success)
{
    // result.AstRoot  -> the semantic tree (when the grammar uses MeaningUnits)
}
```

`LALRParser` is the supported parser for the `0.5.0` release. SLR and CLR variants are experimental
and incomplete — see [Known limitations](#status--known-limitations).

### Token selection

`Lexer.Lexing`, edited-input lexing, and parser-driven token sources compare each active rule's
match at the current source position. The rule that consumes the most text wins. Equal lengths
use the existing terminal-category priority, then registration order within that category.
For example, the identifier rule reads all of `ifValue` instead of splitting off the `if`
keyword; on the input `if`, a literal keyword rule wins its equal-length tie with the identifier.

The lexer uses regexes as declared and does not add `\b`. A language can therefore include `$`
or Unicode characters in its identifiers. Explicit boundaries, anchors, alternation, and
greediness keep their .NET regex meaning: `a|ab` still returns `a` within that rule. Longest-match
selection compares the matches returned by separate rules. A rule that matches an empty string
at the requested position produces `EmptyTokenMatchException` instead of an input-free loop.

When upgrading from `0.4.0`, review patterns that relied on automatically added word boundaries
or on a shorter rule winning before a longer match. Add an explicit boundary only where your
language requires it. The legacy combined-regex string API retains ordered .NET alternation;
the registered-rule lexer paths above use the new selection logic.

### Parser-driven lexing

Some languages need grammar context before the next token can be recognized. JavaScript's `/`
can begin a division operator or a regular-expression literal. `LexicalGoal` holds a group of
token rules; `LexicalPolicy` owns the available groups and selects one from the parsing context.
`Lexer` then creates a source that applies the selected group to the input.

With the lexer, parser, and terminal collections already prepared, the setup is:

```csharp
var div = new LexicalGoal(commonTokens.Append(divisionToken));
var regExp = new LexicalGoal(commonTokens.Append(regexToken));
var policy = new LexicalPolicy(div, regExp);
var source = lexer.CreateTokenSource(input, policy);
var result = parser.Parsing(source);
```

The parser passes a read-only `LexicalContext` to the source's policy. The policy chooses a
group, and the parser passes that returned group to `source.Peek(cursor, goal)`. `Peek` returns
a token and a `Next` cursor; the parser adopts `Next` when it shifts the token. Reductions do
not consume input. Prediction candidates keep their own cursor, previous token, and lexical
state, so speculative reads do not advance the actual parser.

The default policy compares expected terminals with the registered groups. A pure C# selector
can handle contexts that need the state stack or consumed-token history. The JavaScript profiles
provide their own selector for slash, template, and statement contexts. See the
[illustrated token-exchange guide](docs/ko/parser-driven-lexing.html), including
[cursor fields and ownership](docs/ko/parser-driven-lexing.html#cursor-example).

### JavaScript grammar profiles

The package includes `Janglim.FrontEnd.Grammars.JavaScript` with three entry points:

| Class | Input |
| --- | --- |
| `JavaScriptExpressionGrammar` | One expression in a strict Script context |
| `JavaScriptScriptGrammar` | Declarations and statements in a strict Script context |
| `JavaScriptModuleGrammar` | A Module, including import/export and top-level await |

The profiles target ECMAScript 2026. They include Unicode 17 identifiers, numeric/string/BigInt
literals, operator precedence, optional chaining, templates, destructuring, ordinary/arrow/async
functions, generators and async generators, object methods and accessors, classes, and modules.
Regular-expression literals are checked for boundaries, flags, and body grammar in ordinary,
`u`, and `v` modes. Statement support includes automatic semicolon insertion and restricted
line-break rules.

Use both parsing and static validation. Parsing builds the concrete syntax tree; `Validate`
checks restrictions such as assignment targets, declaration collisions, function contexts,
private-name scopes, and regular-expression bodies:

```csharp
using System;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new JavaScriptScriptGrammar();
var parser = new LALRParser(language.Grammar);
var input = @"
async function load({path}) {
    const text = await read(path);
    return /ab/g.test(text);
}";
var parsed = parser.Parsing(language.CreateTokenSource(input));
if (!parsed.Success)
    throw new InvalidOperationException(parser.FiredException?.Message ?? "Parsing failed.");

var diagnostics = language.Validate(parsed.ToParseTree);
if (diagnostics.Count > 0)
{
    foreach (var diagnostic in diagnostics)
        Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message} " +
            $"(offset {diagnostic.SourceOffset}, length {diagnostic.Length})");
    return;
}

Console.WriteLine(parsed.ToParseTree.AllInputDatas);
```

This example parses and validates source; it does not execute the JavaScript. Diagnostic
offsets and lengths use .NET UTF-16 code units. The current support scope is strict Script
and Module syntax with the documented static checks; non-strict syntax and full specification
conformance remain follow-up work.

Read the [JavaScript profile guide](grammars/javascript/README.md) for the supported syntax,
setup, Unicode provenance, and authorship context. Detailed guides cover
[expressions and templates](docs/ko/javascript-expressions.html),
[statements and bindings](docs/ko/javascript-statements.html),
[async functions, generators, and methods](docs/ko/javascript-functions.html),
[classes](docs/ko/javascript-classes.html), [modules](docs/ko/javascript-modules.html), and
[regular expressions](docs/ko/javascript-regexp.html).

### Lexer source trivia

`TokenCell.LeadingTrivia` preserves the exact skipped text before each parsing token, including
whitespace and whole comments. `TokenData` forwards that information through `LeadingTrivia`
and `HasLineBreakBefore`. Trivia also records its source range, so tools can relate a condition
to the original text.

For the input string `"return\n    value\n;"`, the token for `value` has
`LeadingTrivia.Text == "\n    "` and `HasLineBreakBefore == true`. For `return value;`, the same
token has `LeadingTrivia.Text == " "` and `HasLineBreakBefore == false`. This gives a registered
condition the source information it needs to enforce a language's newline rule. See the
[lexer guide](docs/ko/lexer.md) for source-position and comment examples.

---

## Looking inside the parser

This is where Janglim earns its keep. Everything the parser computes is available to you.

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
var ff = new LALRParser(grammar).GetFirstAndFollow();
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
var conflicts = new LALRParser(grammar)
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
var table = new LALRParser(grammar).ParsingTable;
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

Before the parse table is built, Janglim normalizes the grammar **once, automatically** — you do not
have to call anything. Two things happen:

1. **Flattening.** The helper rules that `|` and the EBNF helpers (`Optional()` / `ZeroOrMore()` /
   `OneOrMore()`) generate are folded away where they are redundant, keeping the table small. Your
   `MeaningUnit`/priority tags ride along.

2. **Optional-absorb.** An optional written `X?` in front of other symbols (e.g. `const? type name`)
   would normally force the parser to decide "is the optional here?" too early, creating a
   shift/reduce conflict. Janglim rewrites it into explicit alternatives so the decision is deferred
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
var parser = new LALRParser(grammar)
    .AddErrorHandler(myErrorHandler)
    .UseBackTrackingOnConflict();
```

On a conflict the parser tries one branch and backtracks to the alternative if it fails. This is the
mechanism the AJ language uses.

The attached `IErrorHandlable` must implement the retry/recovery behavior.
`UseBackTrackingOnConflict()` records conflict alternatives; the switch alone does not retry them.

---

## Incremental reparse & error recovery

- **Incremental reparse:** after an edit, reparse only the changed token range instead of the whole
  input (`Parsing(lexingData, previousResult)`).
- **Error recovery:** attach an `IErrorHandlable` to recover from a syntax error and keep going
  (insert/replace/delete a virtual token), so one mistake doesn't abort the whole parse.

---

## The AJ language — a worked example

Janglim is dogfooded by the **AJ language**, a small C#-like systems language. Its compiler runs
parse → semantic analysis → **LLVM IR** entirely on this engine, and its grammar shows off optionals,
repetition, semantic actions, and LGLR backtracking on a real language.

See **[README.aj.md](README.aj.md)** for AJ setup (the `ajbuild` / `ajutil` tools), writing `.aj`
code, the problem/diagnostics view, and the LLVM-IR / assembly walkthrough.

---

## Project layout

```
src/
  Common/     shared utilities (AJ.Common)
  FrontEnd/   the parsing engine — the publishable "Janglim" (namespaces are Janglim.*):
                Parse, Parse.FrontEnd, .Grammars, .Grammars.Ebnf, .Parsers, .ErrorHandler, .Support
              example front ends built on the engine:
                Parse.FrontEnd.AJ (the AJ language), Parse.FrontEnd.Grammars.MiniC (stub)
  MiddleEnd/  IR  (Parse.MiddleEnd.IR)
  BackEnd/    code-generation targets  (Parse.BackEnd)
  Compile/    the AJ compiler driver
  Cli/        command-line tools (ajbuild, ajutil, …)
  Janglim/    NuGet packaging project (bundles the engine assemblies)
samples/      the React + Blazor WebAssembly playground
services/     the .NET 10 remote MCP service  (Janglim.Mcp)
Tests/        xUnit suites for the engine and MCP service
legacy/       historical solution and deprecated IDE / WPF projects, kept for reference
```

## Building from source

[`AJPGS.sln`](AJPGS.sln) is the supported 13-project default solution: the nine engine/package
projects, parser tests, the Playground, the MCP service, and MCP tests. The repository contains
27 project files in total, including historical projects and projects outside both solutions.
Install the **.NET 10 SDK**, the **.NET 8 runtime** for running parser tests, and
**Node.js 22.18.0 or later** with npm. Parser tests target `net8.0`; the Playground, MCP service,
and MCP tests target `net10.0`. The Playground's MSBuild targets install and build the React
frontend automatically. The MCP service can be built separately without Node.js:

```shell
dotnet build services/Janglim.Mcp/Janglim.Mcp.csproj -c Release
dotnet test Tests/Janglim.Mcp.Tests/Janglim.Mcp.Tests.csproj -c Release
```

From the repository root, use a public build with the included loading component. This is also
the build mode used by CI and requires no licensed frontend sources or key:

```powershell
$env:PLAYGROUND_PUBLIC_BUILD = '1'
dotnet build -c Debug
dotnet build -c Release
```

In bash, set the same flag with `export PLAYGROUND_PUBLIC_BUILD=1`. CI builds the default
solution in both configurations, then runs the parser/UI tests and creates the release artifacts.
CI installs both .NET 8 and 10 SDKs; local builds only need the .NET 10 SDK and .NET 8 test runtime.

The 22-project [`legacy/AJPGS.Legacy.sln`](legacy/AJPGS.Legacy.sln) contains the older AJ
compiler, backends, Builder/Util/Lint CLI projects, remaining IDE definitions, and WPF themes. The unused
top-level `AJStudio` and `CommandPrompt.Compiler` executables and the `ApplicationLayer.Views`,
`ApplicationLayer.ViewModels`, `ApplicationLayer.Models`, `ApplicationLayer.Common`, `Parse.WpfControls.SyntaxEditor`,
and `Parse.WpfControls` libraries were removed;
the original author's designs and code remain in Git history. Remaining
project source paths are unchanged. Open or build this solution explicitly when examining that
historical work:

```shell
dotnet build legacy/AJPGS.Legacy.sln -c Release
```

The historical solution is outside the supported build and CI scope. Projects with the known
namespace/reference and XAML errors have been removed; WPF builds require Windows, and the
remaining WPF projects target `netcoreapp3.1`. See [legacy notes](legacy/README.md).

## Status & known limitations

The **engine `0.5.0`** release supports the LALR path. Its `0.x` API policy remains explicit:
public API changes may occur in later `0.x` versions and should be checked in the release notes.
The **web Playground `1.1.0`** is versioned separately; it does not imply an engine `1.0` API contract.

Supported engine features:

- **LALR** table construction, parsing, conflict detection, FIRST/FOLLOW, normalization, EBNF
  operators, semantic actions, incremental reparse, error recovery, LGLR backtracking.
- Typed EBNF tokens, source trivia and line-break information, registered pure C# predicates,
  selective Shift / predicate-bearing ε Reduce prediction, and grammar progress diagnostics.
- Longest-match registered-rule lexing, parser-driven lexical policies, and independent
  speculative cursors, plus the documented ECMAScript 2026 strict Script/Module profiles.

Known limitations and experimental scope:

- **SLR and CLR are experimental and incomplete** — `SLRParser` builds reduce-deficient tables,
  and `CLRParser` has unimplemented behavior. **Use `LALRParser` for supported parsing.**
- **The end-of-input marker prints a debug placeholder** in the textual dumps. Its display label is
  still being decided (`$`, the usual choice, collides with grammars that use `$` as a real token).
  The underlying data is correct — and terminal labels and the parse-table renderer were just made
  null-safe, so missing labels no longer crash or blank out the dumps.
- **Grammar construction must be sequential.** The builder uses process-global static state;
  constructing multiple grammars concurrently can interfere across instances. The MCP service
  isolates separate requests in worker processes rather than sharing grammar instances.
- **MCP remote access uses a shared Bearer key.** Per-user identity, OAuth, and measured public
  service capacity are not part of this first implementation. See [MCP limits](docs/MCP.md#처리-제한과-격리).
- **The `0.x` public API may evolve.** Introspection still exposes `System.Data.DataTable`, and the
  grammar builder still exposes implementation details. API cleanup remains on the roadmap.
- **Recovery is pluggable.** The default error handler reports positioned syntax errors; token
  recovery and LGLR retries require an appropriate custom `IErrorHandlable`.

See [RELEASE_NOTES.md](RELEASE_NOTES.md) for the official release scope and
[CHANGELOG.md](CHANGELOG.md) for the engine and Playground histories.

## Roadmap

- Stabilize the public API (drop `DataTable` from the public surface, remove process-global static
  state, add a clean generic semantic-action hook).
- Polish the diagnostic renderers (tables, FIRST/FOLLOW, item sets).
- Extend the JavaScript profiles with non-strict syntax and broader specification conformance
  checks, then develop TypeScript support on the existing parser-driven lexing APIs.
- Generate parser source at build time from the existing C# and EBNF grammar definitions.
- Repair/finish the SLR and CLR table builders.

## Publications

Videos about this project:

- https://www.youtube.com/watch?v=oMhS3l0DyLo&t=187s
- https://www.youtube.com/watch?v=m1ylQsQRPF8&t=29s
- https://www.youtube.com/watch?v=QwPlyto8JAA&t=22s

## License

MIT — see [LICENSE](LICENSE).

## Release automation

See [publishing and deployment instructions](.github/RELEASING.md) for the master-push release pipeline, dry runs, and retries.
