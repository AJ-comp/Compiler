# Janglim

**A general-purpose, embeddable LR/LALR parser-generator engine for .NET.**

Define a grammar in C# — or in plain EBNF text — and get a full LR pipeline you can
look inside: parse tables, FIRST/FOLLOW, step-by-step parse traces, conflict reports,
grammar normalization, incremental reparsing, and pluggable error recovery.

> **Early preview (`0.1.0-preview`).** The public API is unstable and will change across `0.x`.

## Quick start

```csharp
using Parse.FrontEnd.Grammars.Ebnf;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;

var read = EbnfGrammarReader.Read(@"
    Expr   : Expr '+' Term | Term ;
    Term   : Term '*' Factor | Factor ;
    Factor : '(' Expr ')' | id ;
    id     := ""[a-zA-Z]+"" ;");

var grammar = read.Grammar;
var lexer = new Lexer();
foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);

var parser = new LALRParser(grammar);
var result = parser.Parsing(lexer.Lexing("a + a * a").TokensForParsing);

Console.WriteLine(result.Success);   // True
```

You can also build the grammar directly in C# (terminals and non-terminals as fields,
`+` for sequence, `|` for choice, `?` `*` `+` for EBNF repetition) instead of EBNF text.

## Links

- **Live playground** (define a grammar, step through the parse): https://polite-island-0b2142200.7.azurestaticapps.net
- **Source, docs & samples:** https://github.com/AJ-comp/Compiler

MIT licensed.
