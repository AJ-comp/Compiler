# LR item — marking "how far we've read" with a dot

> 🎓 This is an **advanced track** lesson.\
> So far we've covered the **grammar structure** ([Symbol](deep-symbols.md) ~ [Alter](deep-alter.md)) and **FIRST/FOLLOW**. Now we take those as *ingredients* and — at last — start **building an LR parser**.\
> The first brick is the **LR item**.

## What are we trying to mark

An LR parser reads the input **one token at a time, from the left**.\
As it reads, it has to keep remembering *"which production am I in, and how far have I read it?"*

The simplest way to mark that "how far" — put a single **dot (`•`)** somewhere in the middle of the production.

## Definition — what an LR item is

> An **LR item** = a single production with a **dot (`•`) placed at one spot.**\
> It has the form `A → α • β`. The part **before the dot (α)** is *what we've already read*, and the part **after the dot (β)** is *what's still to be read*.

For example, in the single production `Expr → Expr '+' Term`, the spots where the dot can go are these.

<pre class="lrbox">   Expr → <span class="lrdot">•</span> Expr '+' Term      nothing read yet
   Expr → Expr <span class="lrdot">•</span> '+' Term      read up to Expr
   Expr → Expr '+' <span class="lrdot">•</span> Term      read up to '+'
   Expr → Expr '+' Term <span class="lrdot">•</span>      all read — now it's time to fold (reduce) by this rule!</pre>

**Even for the same production, a different dot position is a different item.** Because the dot is the "progress."

## Code — `LRItem` · how the author represented it

From here on, the **author's design** comes back into play.\
The *concept* of an LR item — placing a dot in a production to show "how far we've read" — is **standard** in compiler textbooks.\
(Both the concept and the name come from textbooks. In English it's called an *item*; in Korean textbooks it's often called **"항목".**)\
What's the author's choice, though, is **how to express it in code** — that approach of reusing the production and just adding a `markIndex` on top.

The author probably reasoned like this.

> *"An LR item is, after all, just 'one production + a dot position', right? The production is already built as a [Single](deep-single.md) — so **there's nothing new to make; I just point at it** and add a single dot position (`markIndex`) on top."*

So `LRItem` has just **two things** — *which production*, and *where the dot is*.

> 📍 **`LRItem : ICloneable`** · `…/Parsers/Datas/LR/LRItem.cs`

```csharp
public class LRItem : ICloneable
{
    public NonTerminalSingle SingleNT { get; }   // which production this is  (A → α β)
    private sbyte markIndex = 0;                  // which symbol the dot sits in front of (0 = at the very front)
}
```

- `SingleNT` — the very **production (`NonTerminalSingle`)** we saw back in the [Single](deep-single.md) chapter, unchanged.\
  (An LR item *doesn't make a new* production; it points at it as-is.)
- `markIndex` — the position of the dot. There are `markIndex` symbols in front of the dot (α).

### Before the dot / after the dot — a seed the author planted ahead of time

Remember in the [Concat](deep-concat.md) chapter, when we looked at `PrevSymbolListFrom`/`PostSymbolListFrom` and said *"these get used later, at the before/after of the dot in LR parsing"*?\
The truth is, the author — **foreseeing that they'd split before/after the dot (α/β) in the LR item** — planted those methods in [Concat](deep-concat.md).\
**This is where that seed grows.**

```csharp
public NonTerminalConcat SymbolListBeforeMarkSymbol => SingleNT.PrevSymbolListFrom(markIndex);  // α (before the dot)
public NonTerminalConcat SymbolListAfterMarkSymbol  => SingleNT.PostSymbolListFrom(markIndex);  // β (after the dot)
public Symbol            MarkSymbol                  // the single symbol right after the dot (SingleNT[markIndex])
```

The `α` and `β` of `A → α • β` fall out precisely, via the `Prev/PostSymbolListFrom` we built back then.\
And `MarkSymbol` is the single symbol right after the dot — *the symbol to read next.*

### When the dot reaches the end = time to fold (`IsReachedHandle`)

When the dot goes all the way to the **very end** of the production, we've *read the whole* rule.\
And that's exactly the **time to fold (reduce).**

```csharp
public bool IsReachedHandle => markIndex >= SingleNT.Count;   // dot at the end = complete (reduce) item
```

In the code, such a *complete item* is called **"reached the handle (reached handle)".** (You'll see this a lot from now on — like *"if a state has a complete item, reduce."*)

### Moving the dot — `MoveMarkSymbol`

When you read one symbol, you move the dot **one slot forward.** (`A → α • X β` → `A → α X • β`)

```csharp
public void MoveMarkSymbol() { if (this.MarkSymbol != null) this.markIndex++; }
```

(If the dot is already at the end — `MarkSymbol` is `null` — it does nothing.)

### Drawing it as text — `ToString`

`ToString()` draws the dotted shape *exactly* as it is. It's that same shape we drew by hand above.

```csharp
//  e.g.) an LRItem with markIndex = 2  →
//      "Expr -> Expr '+'•Term"
```

### Identity — "which production, and where the dot is"

Whether two LR items are *the same* is decided only by — **production + dot position.**

```csharp
public override int GetHashCode()
    => Convert.ToInt32(this.SingleNT.GetHashCode().ToString() + this.markIndex.ToString());
```

It layers the **dot position (`markIndex`)** one more level on top of the [Single](deep-single.md) identity (`UniqueKey + alterIndex`).\
*"Which rule, which alternative, and where the dot is"* is exactly the identity of an LR item. (This becomes the key to building the **canonical collection** *without duplicates* — in the chapter after next.)

## At a glance — the full shape of LRItem

```csharp
public class LRItem : ICloneable
{
    public NonTerminalSingle SingleNT { get; }   // production (which rule)
    private sbyte markIndex;                       // dot position

    // ── around the dot ──────────────────────────────
    public Symbol MarkSymbol      { get; }         // symbol right after the dot (the one to read next)
    public Symbol PrevMarkSymbol  { get; }         // symbol right before the dot
    public NonTerminalConcat SymbolListBeforeMarkSymbol { get; }   // α (everything before the dot)
    public NonTerminalConcat SymbolListAfterMarkSymbol  { get; }   // β (everything after the dot)

    // ── state ─────────────────────────────────
    public bool IsFirst         { get; }           // is the dot at the very front (markIndex == 0)
    public bool IsReachedHandle { get; }           // is the dot at the end = complete (reduce) item

    // ── lookahead (later on) ────────────────────
    public TerminalSet Follow    { get; }          // FOLLOW (for SLR)
    public TerminalSet LookAhead { get; }          // lookahead (for LALR/LR(1))

    // ── manipulation ─────────────────────────────────
    public void   MoveMarkSymbol();                // advance the dot one slot
    public LRItem FirstLRItem();                   // move the dot to the very front (0)
    public LRItem PrevLRItem();                    // move the dot back one slot
    public object Clone();

    // ── identity / representation ────────────────────────
    public override int    GetHashCode();          // SingleNT hash + markIndex
    public override string ToString();             // "Expr -> Expr '+'•Term"
}
```

In one line — **an LR item = a production ([Single](deep-single.md)) + a dot position (`markIndex`).** That's all there is to it.

## Next chapter

A single LR item expresses *"how far we've read this rule"* — just the progress of one rule.

But at one actual spot where the parser stands — *several* items can be possible **at the same time.**\
(For example, the spot "having read one `Term`" could be `Expr → Term •`, or it could be `Term → Term • '*' Factor`.)\
That *bundle of simultaneously-possible items* is exactly a **state.**

👉 **[State — a set of LR items](lr-state.md)**

---

👈 Previous: [FOLLOW · implementation](follow-impl.md)
