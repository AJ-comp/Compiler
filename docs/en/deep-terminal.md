# Terminal — the leaf that splits no further

> 🎓 This is a **deep-dive chapter**.\
> In the previous chapter, [Symbol](deep-symbols.md), we saw the common root of every symbol.\
> Now let's start with the simpler of the two branches — the **leaf (token), Terminal**.

A `Terminal` is a piece that truly appears in the input and can't be split any further.\
Things like
`+`, `*`, `(`, `id`.\
Sitting down at the desk to express this in code, what did the author decide **first**?

## The first concern — "the lexer needs a *value* to match against"

The essence of a token is "this piece of the input."\
So the very first thing you need is an **actual value** for the lexer to **match against** the input.\
That's `Value`.

```csharp
public string Value { get; } = string.Empty;
```

> To the author, `Value` is not *"nice to have"* — it's ***"something you absolutely must provide."***\
> A token with
> no value to match against has no reason to exist.\
> So in the code too, `Value` keeps **exactly** the value the user gave it,
> and is never left empty.

## The second concern — "but the *name we show* can differ from the value"

This is where the **"identity ↔ display separation"** philosophy from the [Symbol chapter](deep-symbols.md) takes concrete shape.\
The author probably pictured a
case like this:

> *"The actual value (Value) of the `id` token is the regex `[a-zA-Z]+`, but on screen I just want to show 'id'.\
> The value and the display are different.\
> So let's keep a **separate name for display**."*

That's why `Caption` was added (`ToString()` uses it too).

```csharp
public string Caption { get; } = string.Empty;
public override string ToString() => Caption;
```

And — this part is *not a guess; it's something the author wrote directly in a code comment*.

`Caption` is used for **display** — tables, diagnostics, FIRST/FOLLOW output, and so on.\
So if it's null, the text renderer
blows up.

So when there's no caption, we fill it in with value.\
**But `Value` is left untouched** — it's the lexer's matching value,
not the display string.

```csharp
// If caption is null, fall back to value (a null display would break the text renderer).
// However, Value itself stays exactly as given — it's the lexer's matching value/pattern, so it's never changed.
this.Caption = caption ?? value ?? string.Empty;
```

We use the fallback to keep the display from breaking, but **the value — the identity — stays as is.**\
Symbol's philosophy is captured in exactly one
line.

## The supporting information — kind, meaning, regex

To handle a single token fully, a few more things were needed.\
These are what the author added one by one.

- **`TokenType`** — is this an operator, a delimiter, a number, a keyword? (It changes how the lexer treats it.)
- **`Meaning`** — is this a *meaningful* token? (The criterion for whether to keep or discard it later when building the AST.)
- **`RegexExpression`** — turns `Value` into the actual **regex** the lexer will use (differently for operators / numbers / words).

```csharp
public TokenType TokenType { get; }
public bool Meaning { get; } = true;
public string RegexExpression => (IsOper) ? ... : (IsNumber) ? Value : ... ;
```

## And — being a leaf, *it's empty inside*

One thing that decisively sets `Terminal` apart from `NonTerminal`.\
**A Terminal has no "alternatives (alters)"
inside it.**\
There's nothing left to unfold.\
It's literally a **leaf**.\
(The NonTerminal in the next chapter is the exact opposite —
it holds a whole bunch of alternatives inside.)

## A few special leaves — ε and $

Finally, the author made a few fake tokens that *aren't in the real input but are essential for parsing* as children of
`Terminal`.\
They're exactly the ones we met in [FIRST/FOLLOW](first-follow.md).

```csharp
public class Epsilon   : Terminal { ... }   // ε — "the empty one"
public class EndMarker : Terminal { ... }   // $ — "the end of input"
```

These have a **fixed UniqueKey** (`KeyManager.EpsilonKey`, etc.).\
So that no matter where they're created, they're treated as *always the same
single one*.\
(Once again, Symbol's "identity is the key" philosophy.)

## At a glance — the whole shape of Terminal

This is the **full skeleton** of the `Terminal` class.\
With the logic emptied out, it shows only *what is there*.

```csharp
public class Terminal : Symbol
{
    // ── What it is ──────────────────────────
    public TokenType TokenType { get; }
    public string Value { get; }        // the value the lexer matches against (required · never touched)
    public string Caption { get; }       // the display name (ToString uses this)
    public bool Meaning { get; }         // is it a meaningful token (for the AST)
    public bool IsWordPattern { get; }

    // ── Derived information ─────────────────
    public bool IsOper { get; }          // is it an operator/delimiter kind
    public bool IsNumber { get; }
    public string RegexExpression { get; }   // Value → the actual regex for the lexer

    // ── Construction ────────────────────────
    public Terminal(TokenType type, string value, bool meaning = true, bool bWord = false);
    public Terminal(TokenType type, string value, string caption, ...);

    // ── Representation ──────────────────────
    public override string ToString();   // → Caption
    public override string ToEbnfString(bool bContainLHS = false);
    public override string ToGrammarString();
    public override string ToTreeString(ushort depth = 1);
}

// Special leaves — all children of Terminal, with fixed UniqueKey
public class Epsilon        : Terminal { }   // ε — the empty one
public class EndMarker      : Terminal { }   // $ — the end of input
public class InputTerminal  : Terminal { }
public class NotDefined     : Terminal { }
public class CustomTerminal : Terminal { }
```

## Next chapter

`Terminal` — we saw **what** the author put in, and **why**, to express a single leaf (value vs. display, kind, and even special
leaves like ε and $).\
Now to the other side — the **branch that holds rules inside it, NonTerminal**.

👉 **[NonTerminal — the branch that holds rules inside](deep-nonterminal.md)**
