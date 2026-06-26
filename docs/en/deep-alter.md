# Alter — Choice (the set of alternatives)

> 🎓 **Advanced track**. At last, it's time to open that **`alters` box** that [NonTerminal](deep-nonterminal.md) was holding. Thank you for making it all the way through the previous [Single](deep-single.md) chapter — here, the pieces lock together as one.

In the [NonTerminal](deep-nonterminal.md) chapter we kept saying *"a rule holds `alters` (a bundle of alternatives)"*.

The **true identity of that `alters`** is exactly `NonTerminalAlter`.

## The author's dilemma — "How do I hold several alternatives?"

`Expr` had two ways to be made.\
They were `Expr '+' Term` and `Term`.

Each way is the `Concat` (a list that holds order) we already know.\
So we need *"a container that holds several of those Concats"*.

The author's answer was — a **Set**.

```csharp
public class NonTerminalAlter : ISet<NonTerminalConcat>   // ← a 'set' of Concats
{
    private HashSet<NonTerminalConcat> concatSymbols = new();
}
```

Why a **Set (HashSet)** instead of a List? If we follow the author's thinking:

> *"An alternative is 'the collection of ways to make this rule'. Having the exact same way in there twice is meaningless — duplicates should count as one. So a **Set** fits better than a List."*

So `Expr`'s alternatives are held like this.

```
   Expr : Expr '+' Term | Term ;
                  │
                  ▼
   NonTerminalAlter   (= alters,  a set of Concats)
     ├ NonTerminalConcat  [ Expr ] · [ '+' ] · [ Term ]     ← alternative 1
     └ NonTerminalConcat  [ Term ]                           ← alternative 2
```

Remember the `alters` picture we drew in the [NonTerminal](deep-nonterminal.md) chapter?\
This is the *real type* behind that picture.

## Two doors — AddAsConcat vs AddAsAlter (★ concatenation and choice split right here)

Now `+` (concatenation) and `|` (choice), which briefly appeared back in the [Symbol](deep-symbols.md) chapter — **what difference they actually make** is revealed right here in the code.

`NonTerminalAlter` has *two* doors for putting in an alternative.

```csharp
public void AddAsConcat(params Symbol[] symbols)
{
    this.Add(new NonTerminalConcat(symbols));               // 'one' alternative as a whole
}

public void AddAsAlter(params Symbol[] symbols)
{
    foreach (var symbol in symbols)
        this.Add(new NonTerminalConcat(symbol));            // each one 'separately' as an alternative
}
```

See the difference? **Even putting in the same `[a, b]`, the result is completely different.**

```
   AddAsConcat(a, b)   →   Alter { [ a · b ] }        ← 1 alternative (with a, b in order inside)

   AddAsAlter(a, b)    →   Alter { [ a ] , [ b ] }    ← 2 alternatives (a alone, b alone)
```

And this is exactly the `+` and `|` we use when we write a grammar in code.

```csharp
a + b   // operator+ →  AddAsConcat(a, b)  →  "a then b"   (one alternative, in order)
a | b   // operator| →  AddAsAlter(a, b)   →  "a or b"     (two alternatives, choice)
```

> 💡 What we said in the [Symbol](deep-symbols.md) chapter — *"`|` is not 'merge', it's 'choice'"* — is proven here. `AddAsAlter` **does not combine** a and b into one — it keeps them as **two separate alternatives, `[a]` and `[b]`.** The reading side then *picks one* of them. That's why it's *choose-one* and not *merge*.

## To every alternative at once — AddSymbols / InsertSymbol

Sometimes you need to insert the same symbol into *every alternative already in there*.\
There are methods for that too.

```csharp
public void AddSymbols(params Symbol[] symbols)    // append to the 'back' of every alternative
public void InsertSymbol(int index, params Symbol[] symbols)   // insert at a specific position in every alternative
```

Just keep in mind that, despite the singular-form name `Symbol`, the action is *"broadcast to every Concat in the set"*. (It comes in very handy when transforming a grammar automatically.)

## Because it's a set — combine, subtract, and check for overlap

We said `NonTerminalAlter` is an `ISet<NonTerminalConcat>`.\
So it carries the whole suite of **set operations**.

```csharp
public void UnionWith(...);          // union — pull in other alternatives and combine them
public void IntersectWith(...);      // intersection
public void ExceptWith(...);         // difference
public bool SetEquals(...);          // is the alternative makeup identical
// … IsSubsetOf / Overlaps / … all of ISet …
```

We won't use these right now — but later, when we **normalize or transform a grammar** (for example, when folding an auto-generated rule into an existing one), these "combine and subtract sets of alternatives" operations get pressed straight into service.\
This is where the author's choice of a Set pays off.

## One small convenience — IsInduceEpsilon

There's also a property that gives `true` if even one of the alternatives is **empty (ε)**.

```csharp
public bool IsInduceEpsilon { get; }   // is there an ε (empty alternative) among the alternatives
```

When computing [FIRST/FOLLOW](first-follow.md), "can this rule produce the empty string?" mattered.\
This answers that judgment right here. (You're starting to see how the pieces connect, right?)

## At a glance — the full shape of Alter

Here is the **full skeleton** of `NonTerminalAlter`.\
The logic is emptied out, showing only *what's in there*.

```csharp
public class NonTerminalAlter : ISet<NonTerminalConcat>
{
    private HashSet<NonTerminalConcat> concatSymbols;   // the set of alternatives (Concats)

    public int  Count { get; }
    public bool IsInduceEpsilon { get; }                // is there an ε among the alternatives

    // ── adding an alternative (★ concatenation vs choice) ───────────
    public void AddAsConcat(params Symbol[] symbols);   // → 1 alternative (in order)     = '+'
    public void AddAsAlter(params Symbol[] symbols);    // → N alternatives (separately)  = '|'

    // ── broadcast to every alternative ────────────────
    public void AddSymbols(params Symbol[] symbols);
    public void InsertSymbol(int index, params Symbol[] symbols);

    // ── set operations (ISet) ─────────────────────
    public void UnionWith(...);  public void IntersectWith(...);  public void ExceptWith(...);
    public bool SetEquals(...);  public bool IsSubsetOf(...);     public bool Overlaps(...);
    // … Add / Remove / Contains / GetEnumerator …

    // ── conversion ────────────────────────────────
    public HashSet<NonTerminal> ToNonTerminalSet();
}
```

In one line — **`Alter` = a *set* of Concats (alternatives). And `+`/`|` decide *how* an alternative goes into this set.**

## All the pieces have locked together — the whole picture

This is the **entire skeleton** of Janglim's grammar structure.\
Let me gather it onto one page.

```
   Symbol  (abstract)
    ├ Terminal              ← leaf (token):  not split any further
    └ NonTerminal "Expr"    ← branch (rule)
         └ alters : NonTerminalAlter            ← the 'set' of alternatives   (Alter)
              ├ NonTerminalConcat [Expr · '+' · Term]   ┐  view each element (Concat)
              └ NonTerminalConcat [Term]                ┘  as "Expr's Nth"
                                                            → NonTerminalSingle (production)
```

- **Symbol** — the abstract root of every symbol (identity = `UniqueKey`)
- **Terminal / NonTerminal** — leaf / branch
- **Concat** — the *order (RHS)* of one alternative
- **Alter** — the *set* of alternatives (= `alters`)
- **Single** — one alternative in the set viewed as *"which rule's Nth"* — a *production*

`+` (concatenation) makes a `Concat`, `|` (choice) makes an alternative of `Alter` — this one sentence threads the whole thing together.

## Next chapter

We've now seen all of the *containers that hold* a grammar.\
Now we move on to the computation the parser actually does **on top of** this structure.

First of all comes the step of finding "which token a rule can start with (FIRST), and what can come after it (FOLLOW)".\
The thing you met as a concept in the basic track — this time, as **formulas and code**.

👉 **FIRST / FOLLOW — formulas and implementation** *(coming soon)*
