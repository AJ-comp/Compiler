# The pipeline at a glance

In [Getting Started](getting-started.md) we ran parsing in just 5 lines.\
Inside those 5 lines, several stages actually happen
in order.\
In this chapter we look at that **whole flow (the pipeline)** as a big picture.\
The details are
covered in their own chapters, so here just put the "map" into your head.

## From text to a result

Let's follow the path our input `a + a * a` walks.

```
   "a + a * a"            ← ① just a sequence of characters (text)
        │
        ▼  Lexing
   [a] [+] [a] [*] [a]    ← ② meaningful pieces = tokens
        │
        ▼  Parsing
        Expr              ← ③ grammatical structure = parse tree
       /  |  \
     Expr '+' Term
      |       / | \
     Term  Term '*' Factor
      ...
        │
        ▼  (the stages after that — semantic analysis, code generation …)
     run / translate / compile
```

Each arrow is one **transformation stage**.

### ① text → ② tokens : "Lexing"

To a computer, `a + a * a` is at first just 9 characters including spaces.\
The first stage is to split this into
**the smallest meaningful units (tokens)**.

- `a` → a "name (identifier)" token
- `+` → an "addition operator" token
- `*` → a "multiplication operator" token
- space → thrown away

The thing that does this is the **lexer** (or **tokenizer**). (Lexing gets its own chapter soon, where we cover it in more detail.)

### ② tokens → ③ tree : "Parsing"

Now we check whether the tokens are **arranged according to the grammar**, and we build that structure into a tree.\
For example,
in `a + a * a` the fact that multiplication should be grouped before addition (precedence) also shows up
in the shape of this tree.

```
a + a * a   →   a + (a * a)      ← '*' is grouped more deeply (first)
```

The thing that does this is the **parser**.\
And this is exactly the part Janglim focuses on.

### ③ tree → what comes next

Once you have a parse tree, what follows splits by purpose.\
A calculator walks the tree and computes a value,
a compiler analyzes the meaning and translates it into machine code / LLVM IR.\
The **AJ language**, which Janglim built for dogfooding (self-verification),
goes all the way here.

## The LR parser's hidden stage 0: "building the table in advance"

The picture above actually has one more **hidden stage**.\
*Before* the parser receives any tokens, it analyzes the grammar and
precomputes a huge table called the **parse table**.

This table writes down, in full, "if you're in this state and the next token is this → act like this": it's an
**action manual**.\
When parsing, the parser doesn't think — it just looks at this table and moves mechanically.\
That's why LR parsing is fast and accurate.

> 💡 **That's why an LR parser is called a "deterministic parser."** Each cell of the table has
> **exactly one** action fixed in it. So at every moment the parser **doesn't agonize at a fork or backtrack** —
> it goes down a single path. A parser whose *next action is always uniquely determined* this way is called a deterministic parser,
> and LR is the prime example. (If the grammar has a *conflict*, or you use the GLR/LGLR style that explores multiple branches, it stops being
> deterministic — but that's covered in the later [Conflict] chapter.)

```
grammar  ──(analyzed in advance)──▶  parse table  ──(consulted while parsing)──▶  decision (shift? reduce?)
```

To build this table you need concepts like **FIRST/FOLLOW**, **LR items**, and the **canonical collection**.\
**The journey of building this table** is the main body of the manual.\
We start from [FIRST/FOLLOW](first-follow.md).

## The layers of the code — just a peek

The inside of Janglim is several modules stacked **layer upon layer**.\
The further down you go, the more fundamental the part.

```
  ┌─────────────────────────────────────────────┐
  │  AJ language → semantic analysis → LLVM IR (compiler) │  ← top tier (application)
  ├─────────────────────────────────────────────┤
  │  parser engine: canonical collection · parse table · driver │
  ├─────────────────────────────────────────────┤
  │  Grammar container                            │
  ├─────────────────────────────────────────────┤
  │  symbols · lexer · FIRST/FOLLOW · normalization · parse tree │  ← the core parts
  ├─────────────────────────────────────────────┤
  │  token types · common helpers                 │  ← bottom tier (foundation)
  └─────────────────────────────────────────────┘
```

> **A fun point:** FIRST/FOLLOW is *near the bottom tier*, right? In terms of code structure it's a very basic part.
> But **in the order you learn the concepts**, learning it right before the parse table makes it click best (that's when
> *why you need it* really sinks in). So this manual goes in *understanding order, not code-layer order*.
> On each page we'll mark separately "which layer/file this concept lives in."
>
> A full map of the module layers we'll organize separately later in the *appendix*.

## Next chapter

Now that you have the map in hand, let's step into the first key concept of LR parsing.

👉 **[FIRST / FOLLOW](first-follow.md)**
