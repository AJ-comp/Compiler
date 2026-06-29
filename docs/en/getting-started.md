# Getting Started

This chapter has just one goal.\
**To run your very first parse — with a grammar you defined yourself — in 5 lines of code.**
We'll save the deep explanations for the chapters ahead and take them slowly. Here we'll touch on just one thing — *what parsing is* — lightly, and then dive right in.

## Before that — what is parsing?

One sentence is enough.\
When we hand the computer a **string of characters (text)** like `a + a * a`, at first, to the computer's eye, it's just a *pile of characters*.\
It means nothing.

**Parsing** is the work of **checking whether this pile of characters fits the grammar, and uncovering its inner structure as a tree shape**.\
It's exactly like splitting a sentence into "subject + predicate" in language class.

```
a + a * a   →   "Ah, this is a (a) plus (a times a) structure"
```

You have to know this structure before you can move on to the next step (calculation, translation, compilation).\
Parsing is the **first gateway** of almost all language processing.\
All right — let's now do this 'parsing' ourselves with Janglim.

## Installation

First, install the package.\
In a .NET project:

```bash
dotnet add package Janglim --prerelease
```

> We add `--prerelease` because it's still an early preview.

> 🌿 **If installation feels like a hassle, you can put it off for a moment.** The *exact same thing* as the code you'll write in a bit can be done **with just clicks in your browser** — there's a [live playground](https://polite-island-0b2142200.7.azurestaticapps.net) for that. There it even shows you the parse table and tree as pictures. For now, it's fine to go take a look with your own eyes first.

## Your first parse (5 lines)

Once you've installed it, paste the following as-is into your console project's `Program.cs`.

```csharp
using Janglim.FrontEnd.Grammars.Ebnf;   // the reader that reads a grammar from text
using Janglim.FrontEnd.Parsers.LR;       // the LALR parser
using Janglim.FrontEnd.Tokenize;         // the lexer (tokenizer)

// ① Define the grammar as EBNF text
var read = EbnfGrammarReader.Read(@"
    Expr   : Expr '+' Term | Term ;
    Term   : Term '*' Factor | Factor ;
    Factor : '(' Expr ')' | id ;
    id     := ""[a-zA-Z]+"" ;");

var grammar = read.Grammar;

// ② Create a lexer and register the token rules the grammar uses
var lexer = new Lexer();
foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);

// ③ Create a parser
var parser = new LALRParser(grammar);

// ④ Split the input into tokens (lexing) and parse it
var result = parser.Parsing(lexer.Lexing("a + a * a").TokensForParsing);

// ⑤ Check the result
Console.WriteLine(result.Success);   // True
```

Run `dotnet run` and **`True`** prints out.\
You just parsed a string with *a grammar you defined yourself*. 🎉

## So what just happened?

The five chunks of code each did one thing.\
For now, **it's enough to just get familiar with the names** — we'll unpack each one slowly, one chapter at a time, going forward.

| Step | What it did |
|---|---|
| ① `EbnfGrammarReader.Read` | grammar written as text → into a `Grammar` object |
| ② `lexer.AddTokenRule` | registers the rules for splitting characters into *tokens* |
| ③ `new LALRParser(grammar)` | precomputes the **parse table** from the grammar |
| ④ `parser.Parsing(...)` | actually parses the tokens (shift / reduce) |
| ⑤ `result.Success` | whether the input fit the grammar (`True` / `False`) |

Let me point out just one thing before we move on.\
In ③ we slipped in **"it precomputes the parse table *in advance*"**, right?\
This is in fact the biggest characteristic of an LR parser, and the central journey of this manual.\
And — **the very next chapter is that story**.

## Next chapter

Just now, we touched "the thing that runs" with our own hands.\
Now let's step back and look at **the whole picture** — what stages flow inside as text becomes a result, and what that 'table built in advance' we just mentioned actually is.

👉 **[The pipeline at a glance](the-big-picture.md)**
