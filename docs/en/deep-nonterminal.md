# NonTerminal — the branch that holds rules inside

> 🎓 This is the **Advanced track**. Earlier we saw [Symbol](deep-symbols.md) (the shared abstraction) and [Terminal](deep-terminal.md) (the leaf).
> Now for the remaining branch — the **branch, NonTerminal**.

A `NonTerminal` is a **rule** like `Expr`, `Term`, or `Factor`.

There's one thing that makes it decisively different from a [Terminal](deep-terminal.md).

**A Terminal is a leaf, so it was empty inside.\
A NonTerminal is the exact opposite — it holds a whole lot inside.**

## The author's worry — "a rule can be built in *several ways*, right?"

Let's look again at the first line of our example.

<pre class="lrbox">
<span class="nt">Expr</span> : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
</pre>

There are **two ways** to build `Expr`.\
Either `Expr '+' Term`, or just `Term` — one of the two.

The author probably reasoned like this here.

> *"A single rule can be built from *several alternatives*. So a NonTerminal had better hold inside itself
> a **bundle of those alternatives**."*

That's why a `NonTerminal` holds something called `alters`.

> 📍 **`NonTerminal : Symbol`** · `…/RegularGrammar/NonTerminal.cs`

```csharp
public class NonTerminal : Symbol, IEnumerable<NonTerminalSingle>
{
    private NonTerminalAlter alters = new();   // ← the "alternatives" of this rule

    public string Name { get; }                // rule name (Expr, Term …)
    public bool IsStartSymbol { get; }          // is this rule the starting point of parsing
}
```

This single line `Expr : Expr '+' Term | Term ;` goes, whole, into this `alters`.

Sounds abstract when it's just words, doesn't it?\
Let me draw out **what shape it concretely takes**.

<pre class="lrbox">
   <span class="nt">Expr</span> : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
                  │
                  ▼   this one line is held inside alters like this
   alters  (NonTerminalAlter)
     ├─ alternative 0 :  [ <span class="nt">Expr</span> ] · [ <span class="setm">'+'</span> ] · [ <span class="nt">Term</span> ]      ← "Expr '+' Term" (in order)
     └─ alternative 1 :  [ <span class="nt">Term</span> ]                            ← "Term"
</pre>

`|` **separates the alternatives**, and within each alternative the symbols are lined up **in order**. (How this "order" and these "alternatives" are each represented
in code — that's the topic starting from the next chapter, [Concat](deep-concat.md).)

## Two tags — the name and the start symbol

A rule carries two more pieces of information.

<div class="ex-card">

**① `Name` — the rule's name**

**`Name`** — the rule's name.\
Names like `Expr`, `Term`. (A Symbol's identity was [`UniqueKey`](deep-symbols.md), remember? `Name` is a separate, *display-only* thing.)

</div>

<div class="ex-card">

**② `IsStartSymbol` — is it the starting point of parsing**

**`IsStartSymbol`** — marks whether this rule is the grammar's **start symbol**.

The start symbol is the rule that *generates the entire language* — it's **the root of the parse tree**, and the symbol parsing must reach when it finishes. (An LR parser reads the input *from the first token* upward, and at the very end folds it into this symbol.)\
Only that start symbol has this mark turned on.

</div>

## Attaching productions to a rule — AddItem / SetItem

So, when we want to **add** an alternative to `Expr` in code, how do we do it?

With the `AddItem` / `SetItem` that `NonTerminal` provides.

```csharp
// NonTerminal.cs
public void AddItem(NonTerminal production, MeaningUnit meaningUnit = null) { ... }
public void SetItem(NonTerminal production, MeaningUnit meaningUnit = null) { ... }
```

- **`AddItem`** — lays *one more* on top of the existing alternatives.
- **`SetItem`** — *wipes out* what's there and lays down a fresh set.

The `MeaningUnit` taken along here is "what meaning unit this rule becomes in the AST" — but we'll cover that
separately later.\
For now, *"so the entryway for attaching alternatives to a rule is AddItem/SetItem"* is enough.

## Iterate over it — and each alternative pops out one by one

A `NonTerminal` is an `IEnumerable<NonTerminalSingle>`.

That is, when you do `foreach (var single in Expr)`, **the alternatives come out one at a time** as `NonTerminalSingle`.

```csharp
foreach (NonTerminalSingle single in Expr)   // the 0th alternative, the 1st alternative …
{
    // single = "the Nth alternative of Expr"
}
```

Exactly what this `NonTerminalSingle` is — that's the very next topic.

## One picture

If we bundle everything so far into a picture, it looks like this.

<pre class="lrbox">
NonTerminal  "Expr"
   ├ Name = "Expr",  IsStartSymbol = ?
   └ alters : NonTerminalAlter      ← the bundle of alternatives
        ├ (alternative 0)  <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
        └ (alternative 1)  <span class="nt">Term</span>

   iterate with foreach → NonTerminalSingle 0, NonTerminalSingle 1
</pre>

## 📐 The author's design diagram

- NonTerminal's child (alternative) structure — <https://www.lucidchart.com/documents/edit/332a9afe-d053-4c13-ab2a-7110f25bff73/0>

> (It's the author's own design note, so you may need viewing permission.)

## At a glance — the full shape of NonTerminal

This is the **full skeleton** of the `NonTerminal` class.\
The logic is emptied out and it only shows *what's there*. (It's
much richer than Terminal — because it holds alternatives inside.)

```csharp
public class NonTerminal : Symbol, IEnumerable<NonTerminalSingle>, ICloneable
{
    private NonTerminalAlter alters;            // ← the bundle of alternatives (the core)

    // ── info ────────────────────────────────
    public string Name { get; }                 // rule name
    public bool IsStartSymbol { get; internal set; }   // is it the parsing start point
    public bool AutoGenerated { get; }           // was it auto-generated by a quantifier, etc.
    public bool IsInduceEpsilon { get; }         // is any of the alternatives empty (ε)
    public int Count { get; }                    // number of alternatives
    public NonTerminalSingle this[int index] { get; }   // the Nth alternative

    // ── construction ────────────────────────────────
    public NonTerminal(string name, bool bStartSymbol = false, bool autoGenerated = false);

    // ── attaching alternatives ───────────────────────────
    public void AddItem(NonTerminal production, MeaningUnit mu = null);   // lay one more on top
    public void AddItem(NonTerminal production, uint priority, MeaningUnit mu = null);
    public void AddItem(Terminal item, MeaningUnit mu = null);
    public void SetItem(NonTerminal production, MeaningUnit mu = null);    // swap out the whole set
    public void AddAsConcat(params Symbol[] symbols);   // one line (concatenation) as an alternative
    public void AddAsAlter(params Symbol[] symbols);    // each one as a separate alternative

    // ── search / replace ─────────────────────────
    public bool IsContain(Symbol item, ...);
    public bool IsExistRefContent(NonTerminal target);
    public void Replace(NonTerminal from, NonTerminal to, ...);
    public HashSet<NonTerminal> ToNonTerminalSet();

    // ── iteration / representation ─────────────────────────
    // explicit implementation of IEnumerable<NonTerminalSingle> — not public, accessed only via foreach
    IEnumerator<NonTerminalSingle> IEnumerable<NonTerminalSingle>.GetEnumerator();   // alternatives one by one → NonTerminalSingle
    public override string ToEbnfString(bool bContainLHS = false);
    public override string ToGrammarString();
    public override string ToString();   // → Name
}
```

## Next chapter

We've seen that a `NonTerminal` holds a **bundle of alternatives (`alters`)**.

But what exact shape does *a single alternative* take inside the code?

The **order (concatenation)** like `Expr '+' Term`, and the **choice (alternation)** like `... | Term` — we dig into the true nature of these two
in the next chapter.

👉 **[Concat — order (concatenation)](deep-concat.md)**
