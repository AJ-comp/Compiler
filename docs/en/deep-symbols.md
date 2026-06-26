# Symbol — the smallest unit of a grammar

> 🎓 This is the **deep-dive track**. If the basics track was about the *concept*, the deep-dive track follows **how Janglim built that concept up in code** — and it does so **in the exact order it was built, slowly**.

> Let me make one promise before we go. In this deep-dive track, whenever I say **"the author"**, I mean the **person** who designed and wrote Janglim's code (the owner of this project). *Not the AI that is putting these notes together right now.*

> And to be honest, the "author's thoughts" captured here are really **a mix of two things**:

> - **Some come from asking the author directly**, so the intent is recorded as-is. (Though I did *not* ask about *every* decision.)
> - **Many of the rest are inferred from reading the code** — "they probably made this call."

> So it doesn't split cleanly into *"this is the author's own words, that is a guess read off the code"* — the two are blended together naturally. But don't worry, **the code tells you more than you'd think.** 🙂

## The author's design starting point — "when you see the concrete, grab the abstraction first"

The author has one consistent habit when designing.

**When you see several different concrete things, grab the *abstract class* that ties them together first.**

Why? — **Because that's how you can gather the *common part* of those concrete things into one place.** (Put the common behavior and common data in the abstract base just once, and the concrete classes can simply inherit it.)

`Symbol` was born exactly like that.

A grammar has two things with clearly different characters — **Terminal (a terminal, a token)** and **NonTerminal (a nonterminal, a rule)**.\
Looking at these two *concrete classes*, the author probably reasoned like this:

> *"These two are different, but in the end they're family in that they're both 'symbols that appear in a grammar.' So let me first grab the **common class that abstracts them both**."*

That's how `Symbol` came to be **designed as an abstract class (`abstract`) from the very start**.\
So `Symbol` can't be born on its own (`new Symbol()` is impossible) — it must be **made concrete** as either Terminal or NonTerminal.

> 📍 **`Symbol`** · module `Janglim.FrontEnd` (Layer 2) · `src/FrontEnd/Janglim.FrontEnd/RegularGrammar/Symbol.cs`

```csharp
public abstract class Symbol : IShowable, IQuantifiable, IConvertableEbnfString
{
    // A common base abstracting the two concretes (Terminal · NonTerminal)
}
```

```
        Symbol  (abstract — the common abstraction of the two concretes)
        ├── Terminal      ← leaf: a token that splits no further   (next chapter)
        └── NonTerminal   ← branch: a rule that splits further     (the chapter after)
```

> 💡 **This habit shows up again throughout the manual.** From now on, whenever you *see several concrete classes, expect that above them there's a class that abstracts them all* — read it that way and it's much easier to follow the author's train of thought.

## The first common part — identity (UniqueKey)

Now that we've grabbed the abstract base, it's time to fill in the *common parts to tie together* one by one.\
The first is — **"by what do we judge whether two symbols are the same or different?"** — that is, *identity*. (Whether it's a Terminal or a NonTerminal, this is needed all the same — so it's common.) The parser constantly has to ask "this symbol = that symbol?"

The easiest answer is to *compare by name*.\
But — the author probably went one step further here:

> *"What if the name (the label shown on screen) changes later? For example, what if I change the display of `+` to 'plus'? Then it's the same symbol but suddenly it looks different. The whole parse would wobble... no good. **Let's separate 'the visible name' from 'the real identity.'**"*

That's what brought in **`UniqueKey`**.\
It's a numeric unique identifier, *entirely separate* from the display name.

```csharp
public UInt32 UniqueKey { get; internal set; } = UInt32.MaxValue;

public override int GetHashCode() => (int)this.UniqueKey;   // the hash too
// == too, Equals too — all compared by UniqueKey alone
```

Equality (`==`), hashing too — **all of it is done by `UniqueKey` alone**.

Thanks to this, no matter how much you change the display name, from the parser's point of view **the same symbol is forever the same symbol**.

It looks small — but in *a project like a compiler, where a small mistake spreads big*, this separation is a major safety device.

> (This "identity ↔ display" separation gets made concrete again in the next chapter, [Terminal](deep-terminal.md), as `Value`/`Caption`.)

## Another common part — operators and quantifiers

Finally, when we write a grammar in C#, we write things like `Expr + plus + Term` or `... | Term`, right?\
The **`+` (join) and `|` (choose) operators** used here, and the `?`·`*`·`+` (quantifiers) — where should they live?

> The author's call: *"This has to be usable on **any symbol**, whether Terminal or NonTerminal. So putting it on `Symbol`, the common abstraction of the two, is the right move."*

So these operators and quantifiers all live on `Symbol` too. (Gathering the common behavior in the abstract base.)

```csharp
public static NonTerminal operator +(Symbol left, Symbol right);   // join (concatenation)
public static NonTerminal operator |(Symbol left, Symbol right);   // choose (choice)
// ?(Optional) · *(ZeroOrMore) · +(OneOrMore) are on Symbol too (IQuantifiable)
```

For now it's just *"ah, they live here"* — **what structure these actually produce** we'll dig into one chapter at a time, slowly, later on.

## 📐 The author's design diagram

This is a diagram the author drew when designing this part (the same link is embedded in the code comments too).\
Looking at it alongside makes the mental picture clearer.

- Symbol and operator design — <https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/0>

> (It's the author's own design note, so it may require access permission.)

## At a glance — the full shape of Symbol

Now that we've seen the pieces, let's look at the **whole skeleton** of the `Symbol` class all at once.

The logic is emptied out — just *what's in it*. (So you can go "ah, so that's what it looks like.")

```csharp
public abstract class Symbol : IShowable, IQuantifiable, IConvertableEbnfString
{
    // ── identity ──────────────────────────────
    public UInt32 UniqueKey { get; internal set; }
    protected string EbnfString { get; set; }

    // ── representation (filled in by the children) ───
    public abstract string ToEbnfString(bool bContainLHS = false);
    public abstract string ToGrammarString();
    public abstract string ToTreeString(ushort depth = 1);

    // ── equality (all based on UniqueKey) ───────────
    public bool Equals(Symbol other);
    public override int GetHashCode();
    public static bool operator ==(Symbol left, Symbol right);
    public static bool operator !=(Symbol left, Symbol right);

    // ── join / choose ────────────────────────
    public static NonTerminal operator +(Symbol left, Symbol right);   // concatenation: a then b
    public static NonTerminal operator |(Symbol left, Symbol right);   // choice: a or b

    // ── quantifiers (IQuantifiable) ───────────────
    public NonTerminal ZeroOrMore();   // *
    public NonTerminal OneOrMore();    // +
    public NonTerminal Optional();     // ?
}
```

Not big, right? **Identity · representation · equality · join/choose · quantifiers** — just these five bundles.

## Next chapter

We've seen `Symbol` — including *why* it was grabbed as abstract from the start (abstracting the two concretes), and *why* identity was put on a key rather than a name.\
Now, of those two branches, let's look at the simpler side first — **Terminal, the leaf (token)**.

👉 **[Terminal — the leaf that splits no further](deep-terminal.md)**
