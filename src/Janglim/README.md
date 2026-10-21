# Janglim

**A general-purpose, embeddable LALR parser-generator engine for .NET.**

[![NuGet](https://img.shields.io/nuget/v/Janglim.svg?label=NuGet)](https://www.nuget.org/packages/Janglim)

Define a grammar in C# — or in plain EBNF text — and get a full LR pipeline you can
look inside: parse tables, FIRST/FOLLOW, step-by-step parse traces, conflict reports,
grammar normalization, incremental reparsing, and pluggable error recovery.

> **Engine `0.5.0`.** `LALRParser` is the supported parser.
> The public API may evolve across `0.x`; SLR and CLR remain experimental and incomplete.

Install the engine:

```bash
dotnet add package Janglim --version 0.5.0
```

See the [release notes](https://github.com/AJ-comp/Compiler/blob/master/RELEASE_NOTES.md) and
[changelog](https://github.com/AJ-comp/Compiler/blob/master/CHANGELOG.md) for release scope and history.

Version `0.5.0` adds longest-match registered-rule lexing, parser-driven lexical policies and
independent input cursors, plus ECMAScript 2026 profiles for strict Script expressions, Scripts,
and Modules. The JavaScript language layer includes templates, destructuring, functions,
async functions, generators, object methods, classes, modules, and regular-expression validation.

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

Use the `StringLiteral` / `CharLiteral` token kinds with a regex describing the complete
quoted token. All regex rules use the supplied pattern without automatic `\b` boundaries:

```csharp
using Janglim;
using Janglim.FrontEnd.RegularGrammar;

new Terminal(TokenType.Literal.StringLiteral, "\"[^\"\\r\\n]*\"", "string", true, true);
new Terminal(TokenType.Literal.CharLiteral, "'[^'\\r\\n]'", "char", true, true);
```

EBNF text also supports optional token kinds:

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
Omitting the kind selects `Identifier`, so `id := "[a-zA-Z]+" ;` remains valid.
The regex determines accepted text for each kind. `Comment` tokens are skipped before parsing.
`Reject` tokens mark invalid input;
after lexing, check each token's terminal with `EbnfGrammar.IsRejectedToken` and reject matching
input before invoking the parser. The MCP adapter performs this check automatically.
`Comment` and `Reject` are lexical-only rules and cannot be referenced in productions.

Double a double quote inside the double-quoted regex (`""` represents `"`). Backslashes stay
unchanged, so regex escapes such as `\r` and `\n` reach the regex engine directly. Single-quoted
grammar literals keep their existing syntax. C# and JSON escaping still apply separately
when embedding the EBNF in those formats.

These simple examples recognize basic quoted tokens. Token kinds classify the result and
select comment/rejection behavior; the regex determines accepted text, and literal values are
not decoded. Full C escape validation belongs to the language profile.
The repository provides a [C11 Core syntax profile](https://github.com/AJ-comp/Compiler/blob/master/grammars/c/README.md) with a documented
scope and regression examples. It does not implement a complete C compiler.

## Longest-match and parser-driven lexing

`Lexer.Lexing`, edited-input lexing, and token sources compare each registered rule's match at
the current source position. The match consuming the most text wins; equal lengths use terminal
category priority, then registration order within that category. Thus `ifValue` stays one
identifier, while a literal `if` keyword wins an equal-length tie on the input `if`.

Regex rules retain their authored .NET semantics, including explicit `\b`, anchors, alternation,
and greediness. Longest-match selection compares returned matches from separate rules; it does
not rewrite `a|ab` to prefer `ab`. A rule matching zero characters at the requested position
raises `EmptyTokenMatchException`. When upgrading from `0.4.0`, review any patterns that relied
on automatic word boundaries or shorter-rule precedence. The legacy combined-regex string API
continues to use ordered .NET alternation.

For context-sensitive tokenization, create `LexicalGoal` rule groups, register them in a
`LexicalPolicy`, then call `lexer.CreateTokenSource(input, policy)` and `parser.Parsing(source)`.
The parser supplies a read-only `LexicalContext` to the policy. The policy selects a group and
the parser requests the next token from the source with that group. `Peek` returns a token and
`Next` cursor; a Shift adopts that next cursor. Prediction candidates retain independent cursors
and lexical states. A custom pure selector handles contexts that need the state stack or
consumed-token history.

The [illustrated guide](https://github.com/AJ-comp/Compiler/blob/master/docs/ko/parser-driven-lexing.html)
explains the setup, division/regular-expression example, cursor fields, and responsibilities.

## JavaScript profiles

Use `Janglim.FrontEnd.Grammars.JavaScript.JavaScriptExpressionGrammar` for one expression in a
strict Script context, `JavaScriptScriptGrammar` for strict declarations and statements, or
`JavaScriptModuleGrammar` for Module input. The profiles target ECMAScript 2026 with Unicode 17
identifiers and provide their own lexical policies and registered predicates.

```csharp
using System;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new JavaScriptScriptGrammar();
var parser = new LALRParser(language.Grammar);
var parsed = parser.Parsing(language.CreateTokenSource(
    "async function load({path}) { return /ab/g.test(await read(path)); }"));
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

**Check both `parsed.Success` and `language.Validate(...)`.** Parsing builds the syntax tree;
validation checks assignment/binding restrictions, declaration and function contexts, private
names, regular-expression bodies, and other documented static errors. The code does not execute
JavaScript. Diagnostic offsets and lengths use .NET UTF-16 code units.

Supported syntax includes templates, destructuring assignments and bindings, ordinary/arrow/async
functions, generators and async generators, object methods and accessors, classes, imports and
exports, dynamic import, `import.meta`, top-level await, and automatic semicolon insertion.
Regular-expression bodies are validated in ordinary, `u`, and `v` modes. The current profile scope
is strict Script and Module syntax with the documented static checks; non-strict syntax and
full specification conformance remain follow-up work. See the
[profile guide](https://github.com/AJ-comp/Compiler/blob/master/grammars/javascript/README.md)
for the support matrix, examples, Unicode licensing, and authorship context.

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
  pluggable error recovery, typed EBNF tokens, token-boundary trivia, registered C# predicates,
  selective guarded-conflict prediction, and grammar progress diagnostics. Version `0.5.0` adds
  longest-match registered-rule lexing, parser-driven token sources and lexical policies, and
  the documented ECMAScript 2026 strict Script/Module profiles.
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
