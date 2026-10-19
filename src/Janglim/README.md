# Janglim

**A general-purpose, embeddable LALR parser-generator engine for .NET.**

[![NuGet](https://img.shields.io/nuget/v/Janglim.svg?label=NuGet)](https://www.nuget.org/packages/Janglim)

Define a grammar in C# — or in plain EBNF text — and get a full LR pipeline you can
look inside: parse tables, FIRST/FOLLOW, step-by-step parse traces, conflict reports,
grammar normalization, incremental reparsing, and pluggable error recovery.

> **Engine `0.4.0`.** `LALRParser` is the supported parser.
> The public API may evolve across `0.x`; SLR and CLR remain experimental and incomplete.

Install the engine:

```bash
dotnet add package Janglim --version 0.4.0
```

See the [release notes](https://github.com/AJ-comp/Compiler/blob/master/RELEASE_NOTES.md) and
[changelog](https://github.com/AJ-comp/Compiler/blob/master/CHANGELOG.md) for release scope and history.

Version `0.4.0` adds lexer source trivia and line-break information, typed EBNF token declarations,
registered pure C# predicates, selective Shift / predicate-bearing ε Reduce prediction, and
grammar diagnostics for non-consuming repetitions and predicate-bearing left recursion.

## Quick start

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

You can also build the grammar directly in C# (terminals and non-terminals as fields,
`+` for sequence, `|` for choice, and `.Optional()` / `.ZeroOrMore()` / `.OneOrMore()`
for EBNF repetition) instead of EBNF text.

### String / char literal tokens

Quote-delimited tokens need the `StringLiteral` / `CharLiteral` token types, which use the
pattern as a raw regex (word-shaped tokens are wrapped in `\b...\b`, which can never match
a token that starts with a quote):

```csharp
using Janglim;
using Janglim.FrontEnd.RegularGrammar;

new Terminal(TokenType.Literal.StringLiteral, "\"[^\"\\r\\n]*\"", "string", true, true);
new Terminal(TokenType.Literal.CharLiteral, "'[^'\\r\\n]'", "char", true, true);
```

Engine `0.4.0` also supports optional token kinds in EBNF text:

```ebnf
Value : id | text | ch | number ;
id    := Identifier "[a-zA-Z_][a-zA-Z_0-9]*" ;
text  := StringLiteral """[^""\r\n]*""" ;
ch    := CharLiteral "'[^'\r\n]'" ;
number := NumberLiteral "[0-9]+(?:\.[0-9]+)?" ;
comment := Comment "//[^\r\n]*|/\*[\s\S]*?\*/" ;
invalid_comment := Reject "/\*(?:(?!\*/)[\s\S])*\z" ;
```

The supported kind names are exactly `Identifier`, `StringLiteral`, `CharLiteral`,
`NumberLiteral`, `Comment`, and `Reject`.
Omitting the kind preserves the existing `Identifier` behavior with word boundaries:
`id := "[a-zA-Z]+" ;` remains valid. String and character kinds use the existing raw-regex
terminal types without word boundaries. `NumberLiteral` also uses a raw regex without word
boundaries. `Comment` tokens are skipped before parsing. `Reject` tokens mark invalid input;
after lexing, check each token's terminal with `EbnfGrammar.IsRejectedToken` and reject matching
input before invoking the parser. The MCP adapter performs this check automatically.
`Comment` and `Reject` are lexical-only rules and cannot be referenced in productions.

Double a double quote inside the double-quoted regex (`""` represents `"`). Backslashes stay
unchanged, so regex escapes such as `\r` and `\n` reach the regex engine directly. Single-quoted
grammar literals keep their existing syntax. C# and JSON escaping still apply separately
when embedding the EBNF in those formats.

These simple examples do not define all C string/character escapes. Token kinds select lexer
behavior; the supplied regex determines accepted text, and literal values are not decoded.
The repository provides a [C11 Core syntax profile](https://github.com/AJ-comp/Compiler/blob/master/grammars/c/README.md) with a documented
scope and regression examples. It does not implement a complete C compiler.

## Source trivia and registered predicates

Each parsing token exposes its exact leading whitespace and skipped comments through
`TokenCell.LeadingTrivia`. `TokenData` forwards that information through `LeadingTrivia` and
`HasLineBreakBefore`. For the input string `"return\n    value;"`, the `value` token has
`LeadingTrivia.Text == "\n    "` and `HasLineBreakBefore == true`.

When embedding the engine in a .NET application, register a C# condition and invoke it from EBNF
with **`{Name()}?`**. This example enforces a newline rule between `throw` and its identifier:

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

Predicates consume no input. Their context provides the previous token, next token, input token
index, and end-of-input flag. Callbacks must be pure because prediction and actual parsing may
evaluate them more than once. Line breaks in skipped comments are included in the token's
leading trivia.

For an original table cell with exactly one Shift and one predicate-bearing ε Reduce, the parser
predicts ahead when both actions remain allowed after checking the condition. Prediction uses
copied state stacks and input positions, has resource limits, and reports failed or ambiguous
predictions. The actual parser then executes the selected first action. Other conflicts keep
their existing policy.

Grammar validation rejects `({Allow()}?)*` and `({Allow()}?)+` because an iteration can consume
no input, and rejects `A : {Allow()}? A 'x' | 'x';` because its condition precedes a zero-token
recursive entry. Ordinary left recursion and repetitions that require a token, such as
`('x' {Allow()}?)*`, remain supported. Validation examines the grammar without calling conditions.

Read the [lexer guide](https://github.com/AJ-comp/Compiler/blob/master/docs/ko/lexer.md),
[predicate repetition rules](https://github.com/AJ-comp/Compiler/blob/master/docs/ko/predicate-repetition-rules.html),
[left-recursion validation](https://github.com/AJ-comp/Compiler/blob/master/docs/ko/predicate-left-recursion-validation.html),
and [prediction walkthrough](https://github.com/AJ-comp/Compiler/blob/master/docs/ko/functions/select-first-action.html)
for the detailed behavior and examples.

## Supported scope and limitations

- **Supported:** LALR table construction and parsing, FIRST/FOLLOW, conflict reports, parse traces,
  EBNF operators, grammar normalization, semantic tags and ASTs, incremental reparsing, and
  pluggable error recovery. Version `0.4.0` also supports typed EBNF tokens, token-boundary trivia,
  registered C# predicates, selective guarded-conflict prediction, and grammar progress diagnostics.
- **Experimental and incomplete:** SLR builds reduce-deficient tables; CLR has unimplemented
  behavior. Use `LALRParser` for the supported parsing path.
- **Build grammars sequentially:** grammar generation uses process-global state, so concurrent
  construction can interfere across grammar instances.
- **Recovery needs a handler:** the default handler reports positioned errors. Token recovery and
  LGLR conflict retries require a custom `IErrorHandlable`; `UseBackTrackingOnConflict()` alone
  does not retry alternative branches.
- **The API remains `0.x`:** diagnostic adapters expose `System.Data.DataTable`, and public API
  cleanup may change the surface in later `0.x` releases.

## Web Playground

The separately versioned **Playground `1.1.0`** provides a redesigned React interface
with shadcn components, syntax trees, execution playback, ACTION/GOTO tables, conflict inspection,
and light/dark themes. Selecting a tree node highlights its grammar symbols and exact input range.
The interface supports 14 languages, follows the browser language by default, and remembers an
explicit language choice. Parsing runs locally in the browser through Blazor WebAssembly.
Save and reopen grammar/input workspace files using the versioned `.janglim.json` format.

The web application and **remote MCP `0.1.0`** service have independent versions. The engine's
public API remains `0.x`. See the [MCP guide](https://github.com/AJ-comp/Compiler/blob/master/docs/MCP.md)
for its remote tools and client setup.

## Links

- **Live playground** (define a grammar, step through the parse): https://janglim.mythosia.net
- **Source, docs & samples:** https://github.com/AJ-comp/Compiler
- **Release notes:** https://github.com/AJ-comp/Compiler/blob/master/RELEASE_NOTES.md
- **Changelog:** https://github.com/AJ-comp/Compiler/blob/master/CHANGELOG.md
- **Development roadmap:** https://github.com/AJ-comp/Compiler#roadmap

MIT licensed.
