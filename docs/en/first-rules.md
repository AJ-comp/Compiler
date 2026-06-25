# FIRST · Computation Rules

> 🎓 This is the **advanced track · theory**.\
> In the previous page, [FIRST · Definition and Derivation](first-formula.md), we — pinned down the *definition*, and tried *deriving by the definition* too.\
> But there we hit a **wall: with recursion, the derivation grows infinitely long**.
>
> So this page is — a set of **computation rules** that pull out the same FIRST **without unrolling the derivation by hand**.\
> (Recursion gets handled gracefully too.)\
> How those rules are implemented in code → **[FIRST · Implementation](first-impl.md)**.

> Don't try to take it all in at once.\
> We'll go **one step at a time, starting from the easy parts**.

## First — let's lay out the whole cast before we begin

Before computing FIRST, let's get a single glance at *what's in* this grammar.\
The symbols of a grammar come in exactly **two kinds**.

- **terminal** — a token that actually shows up in the input
- **nonterminal** — a rule name

If we split the two apart in our example grammar, here's how it looks.

```
   terminal list    :   (    )    +    *    id        ← 5 of them
   nonterminal list :   Expr    Term    Factor        ← 3 of them
```

FIRST is computed *for each one of these symbols*.\
So our job is clear — **fill in all 8 FIRSTs: 5 terminals + 3 nonterminals.**\
And the good news — **the terminal side is almost free**. Let's start there.

## FIRST of terminals — each is just itself (done in one shot)

A terminal starts with *itself*.\
Of course — `+` starts with `+`.

```
   FIRST( '(' ) = { '(' }      FIRST( '+' ) = { '+' }      FIRST( id ) = { id }
   FIRST( ')' ) = { ')' }      FIRST( '*' ) = { '*' }
```

`FIRST(terminal a) = { a }`.\
The 5 terminals — **done** with that. We filled 5 of the 8 for free. 🙂

## FIRST of nonterminals — the big picture first

Now the real heart of it: the three nonterminals (`Expr` `Term` `Factor`).\
Before that, let's get the big picture in place.

> 📎 Quick note on one term. In `Factor : '(' Expr ')' | id`, **each single piece** split apart by `|` is called a **production**.\
> It's *"one line of a rule that makes a nonterminal"*, like `Factor → id`. (Details in [Single](deep-single.md).)\
> We'll use this word from here on.

The FIRST of a single nonterminal is computed like this — **compute the FIRST of each and every production of that nonterminal, and combine them all (union)**.

```
   FIRST(Factor) = FIRST(production 1 of Factor) ∪ FIRST(production 2 of Factor) ∪ …
```

So the real question to solve narrows down to one — **how do we compute the FIRST of a single production?**

The answer is surprisingly simple.\
It depends on **what the production starts with**, and there are exactly **three** cases.

1. **Case ①** — when it starts with a terminal
2. **Case ②** — when it starts with a nonterminal
3. **Case ③** — when the front can disappear (ε)

Let's look at them one by one, starting from the easy one.

## Case ① — when the production **starts with a terminal**

This is the easiest case. In fact it's **just reusing what we saw earlier**.

If the front is a terminal, then the FIRST of that production is simply **that terminal**.\
Just above, we said *"the FIRST of a terminal is itself"*, right? **It's exactly the same story** — the front terminal is the answer.\
(It doesn't matter if there are more symbols attached after the production. If the front is a terminal, that's the first terminal, and it ends right there.)

For example, the two productions of `Factor` are each like this.

```
   Factor → id            front is terminal id   →   FIRST = { id }
   Factor → '(' Expr ')'  front is terminal (    →   FIRST = { '(' }
```

Even though `'(' Expr ')'` has `Expr ')'` attached after, it ends right away at the front `(`.

`Factor` has only these two productions, so combining them finishes it right off.

```
   FIRST(Factor) = { '(' } ∪ { id } = { '(', id }
```

First nonterminal done! 🙂

## Case ② — when the production **starts with a nonterminal**

This time the front isn't a terminal, but *another nonterminal*.

Let's call the front nonterminal `Y`.\
The FIRST of this production is **the same as the FIRST of `Y`.**

**Why is that?**\
Once again we go back to the definition — FIRST is *"the front terminal that comes out of deriving"*, right?\
When we derive this production, the one occupying the front spot is `Y`.\
So the front terminal of the string we get from deriving all the way is, in the end, **the front terminal that `Y` derives**.\
Since `Y` holds the front, the first terminal is decided by `Y` as well.\
So we can just bring over the FIRST of `Y`.

For example, one production of `Expr` is like this.

```
   Expr → Term     front is nonterminal Term   →   bring over FIRST(Term) as-is
```

So through this production, `FIRST(Term)` comes straight into the FIRST of `Expr`.\
Written as a formula, it's this clear — **`FIRST(Expr) = FIRST(Term)`**.\
(`Expr`'s other production `Expr '+' Term` is the *left recursion* we'll see shortly, and it adds nothing new, so this equality holds exactly.)

### But — what if that nonterminal is *itself*? (left recursion)

Here we get snagged once.\
Look at the first production of `Term : Term '*' Factor | Factor`.

```
   Term → Term '*' Factor     front is Term again — that's itself?!
```

Trying to compute `FIRST(Term)`, the front nonterminal is `Term` again.\
That is, computing `FIRST(Term)` needs `FIRST(Term)` — a **chicken-or-egg** situation.\
This won't get solved in one go.

So we solve it like this — **start from the empty set, and repeat one round at a time until it stops growing.**\
Let's actually run `FIRST(Term)`. (`FIRST(Factor) = { '(', id }` was already computed in Case ①.)

**Start →** `FIRST(Term) = { }` (empty set)

**Round 1 —** we sweep through `Term`'s two productions.

```
   ① Term → Factor          : add FIRST(Factor) = { '(', id }
                                          →  FIRST(Term) = { '(', id }
   ② Term → Term '*' Factor : add the 'so far' value of the front Term
                                          →  current value { '(', id }, already there → no change
```

→ It **grew** from the empty set to `{ '(', id }`.\
Since it grew, we have to go one more round.

**Round 2 —** we sweep the two productions again.

```
   ① Term → Factor          : { '(', id }                  →  no change
   ② Term → Term '*' Factor : Term's current value { '(', id }    →  no change
```

→ This round **nothing grew. → Stop!**

```
   FIRST(Term) = { '(', id }
```

The key is production ②.\
It points at itself, but it only pulls in "the *so far* value of the front `Term`", so once that value is **filled first** by the other production ① (`Factor`), there's nothing left to add.\
That's why, without unrolling infinitely, the answer settles in just two rounds.

> 💡 The wall of recursion where the derivation kept *growing infinitely long* on the previous page ([Definition and Derivation](first-formula.md)) — **getting past that wall is exactly this "repetition"**.\
> Instead of unrolling to the end, we grow the set little by little and stop when it stops changing.

`Expr` (`Expr : Expr '+' Term | Term`) has the same shape, so by the same method it settles to `{ '(', id }` in two rounds.

## Case ③ — when the front nonterminal **can disappear** (ε)

The last case.\
In Case ② we said *"bring over the FIRST of the front nonterminal `Y`"*, right?\
But if that `Y` **can also vanish into nothing (ε)**, there's one more thing to take care of.\
(When a nonterminal can derive even the empty string, we call it *nullable*.)

**Why do we have to look at the next symbol too?**\
Again, it's the definition — FIRST is *"the front terminal that comes out of deriving"*, right?\
But if the front `Y` vanishes into ε, then the one occupying the front of the derivation result is not `Y` but **the very next symbol**.\
So the first terminal that the next symbol derives can come to the front too.\
That's why we have to add **the FIRST of that next symbol too** to the FIRST of `Y`.\
This rule of *"if the front can disappear, move on to the next symbol and combine"* is called **⊕ (ring sum)**.

```
   A ⊕ B =  A              (if A cannot disappear → it ends there)
            (A-ε) ∪ B      (if A can disappear → drop ε, and add B too)
```

> Luckily, our example grammar has not a single *vanishing (nullable) nonterminal*.\
> So Case ③ doesn't actually happen, and ⊕ almost always ends right at the front symbol.\
> For now it's enough just to know **"there's a safety mechanism like this"**. The true value of ⊕ shows itself in grammars that have ε.

## Summary

The FIRST of a single production split apart by **what it starts with**.

1. **Starts with a terminal** → just that terminal. (Case ①)
2. **Starts with a nonterminal** → the FIRST of that nonterminal as-is. (Case ②; if it's itself, by *repetition*)
3. **If the front can disappear (ε)** → ⊕ in the FIRST of the next symbol too. (Case ③)

And the FIRST of a single nonterminal is **the union of the FIRSTs of all of that nonterminal's productions**.

If we run this rule on our grammar, all three come out with the same answer.

```
   FIRST(Factor) = FIRST(Term) = FIRST(Expr) = { '(', id }
```

It's exactly the same as the answer we computed by hand on the [Definition and Derivation page](first-formula.md). ✓

## One step further — the three cases are really just one ⊕ (ring sum)

We just split it into three, but in fact these three combine into **a single formula**.

A production is, in the end, *a sequence of symbols* (like `Term '*' Factor`).\
Its FIRST is — **the FIRSTs of the constituent symbols ⊕ (ring summed) in order**, that's all.

```
   FIRST(X₁ X₂ … Xₙ) = FIRST(X₁) ⊕ FIRST(X₂) ⊕ … ⊕ FIRST(Xₙ)
```

Then the three earlier cases are — just a difference of **where this ⊕ *stops***.

- **Case ①** : the first symbol is a terminal. A terminal can't be ε, so ⊕ **stops at the first slot** and becomes `{ that terminal }`.
- **Case ②** : the first symbol is a nonterminal (that can't be ε). ⊕ **stops at its FIRST** and becomes `FIRST(that nonterminal)`.
- **Case ③** : the first symbol can be ε. So ⊕ **moves on to the next slot** and keeps combining.

For example, ⊕-ing `Term '*' Factor` straight through goes like this.

```
   FIRST(Term '*' Factor) = FIRST(Term) ⊕ FIRST('*') ⊕ FIRST(Factor)
                          = FIRST(Term)              ← Term can't be ε, so it stops at the first slot
```

The code is exactly this too — it `RingSum(⊕)`s the symbols of the production ([Concat](deep-concat.md)) in order, and stops when there's no more ε to look at.

```csharp
// FirstFollowAnalyzer [First].cs
public TerminalSet FirstSet(NonTerminalConcat singleNT, ...)
{
    TerminalSet result = new TerminalSet();

    foreach (var symbol in singleNT)                        // the symbols of the production, in order
    {
        result = result.RingSum(FirstSet(symbol, seenNT));  // ⊕ one slot
        if (!result.IsNullAble) break;                      // stop when there's no more ε to look at
    }

    return result;
}
```

> 💡 **One-line summary** — the FIRST of a production is *the FIRSTs of its symbols ⊕-ed in order*, and the three cases just differ in where that ⊕ stops.

## Next — this rule, in code

The three cases we just saw — starts-with-terminal · starts-with-nonterminal · ε — together with "repeat until it stops changing", are baked into the `FirstFollowAnalyzer` code **almost one line at a time**.\
Let's look next.

👉 **[FIRST · Implementation (code)](first-impl.md)**

---

👈 Previous: [FIRST · Definition and Derivation](first-formula.md)
