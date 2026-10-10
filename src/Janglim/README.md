# Janglim

**A general-purpose, embeddable LALR parser-generator engine for .NET.**

[![NuGet](https://img.shields.io/nuget/v/Janglim.svg?label=NuGet)](https://www.nuget.org/packages/Janglim)

Define a grammar in C# — or in plain EBNF text — and get a full LR pipeline you can
look inside: parse tables, FIRST/FOLLOW, step-by-step parse traces, conflict reports,
grammar normalization, incremental reparsing, and pluggable error recovery.

> **Engine `0.3.0`.** `LALRParser` is the supported parser.
> The public API may evolve across `0.x`; SLR and CLR remain experimental and incomplete.

Install the engine:

```bash
dotnet add package Janglim --version 0.3.0
```

See the [release notes](https://github.com/AJ-comp/Compiler/blob/master/RELEASE_NOTES.md) and
[changelog](https://github.com/AJ-comp/Compiler/blob/master/CHANGELOG.md) for release scope and history.

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

### String / char literal tokens (C# API)

Quote-delimited tokens need the `StringLiteral` / `CharLiteral` token types, which use the
pattern as a raw regex (word-shaped tokens are wrapped in `\b...\b`, which can never match
a token that starts with a quote):

```csharp
using Janglim;
using Janglim.FrontEnd.RegularGrammar;

new Terminal(TokenType.Literal.StringLiteral, "\"[^\"\\r\\n]*\"", "string", true, true);
new Terminal(TokenType.Literal.CharLiteral, "'[^'\\r\\n]'", "char", true, true);
```

These types are currently only available through the C# API — EBNF-text token rules
(`name := "regex"`) still lex as word patterns.

## Supported scope and limitations

- **Supported:** LALR table construction and parsing, FIRST/FOLLOW, conflict reports, parse traces,
  EBNF operators, grammar normalization, semantic tags and ASTs, incremental reparsing, and
  pluggable error recovery.
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

The separately versioned **Playground `1.0.0`** provides a redesigned React interface
with shadcn components, syntax trees, execution playback, ACTION/GOTO tables, conflict inspection,
and light/dark themes. Selecting a tree node highlights its grammar symbols and exact input range.
The interface supports 14 languages, follows the browser language by default, and remembers an
explicit language choice. Parsing runs locally in the browser through Blazor WebAssembly.

The web application's `1.0.0` version does not change the engine's `0.3.0` API version or `0.x` policy.

## Links

- **Live playground** (define a grammar, step through the parse): https://janglim.mythosia.net
- **Source, docs & samples:** https://github.com/AJ-comp/Compiler
- **Release notes:** https://github.com/AJ-comp/Compiler/blob/master/RELEASE_NOTES.md
- **Changelog:** https://github.com/AJ-comp/Compiler/blob/master/CHANGELOG.md

MIT licensed.
