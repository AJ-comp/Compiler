# FOLLOW · Definition and Derivation

> 🎓 This is the **Advanced track · Theory**.\
> Now that you've finished the [FIRST track](first-formula.md), here comes its partner, **FOLLOW**.\
> If FIRST was about *"what does it *start* with"*, then FOLLOW is about *"what comes *after* it"*.\
> On this page we'll look at the **definition** and at **deriving it directly from the definition**. Then comes → **computation rules** → **implementation**.

> 📍 **Where it lives** · engine `FirstFollowAnalyzer` · module `Parse.FrontEnd` — **Layer 2** (the floor
> *below* the parse table)

This is the example grammar we keep using.\
This time the **start symbol** matters, so let me mark it — the top `Expr` is the start symbol.

```
Expr   : Expr '+' Term | Term ;      ← start symbol
Term   : Term '*' Factor | Factor ;
Factor : '(' Expr ')' | id ;
```

In the [basic track](first-follow.md), the answer we worked out by hand was this.

```
   FOLLOW(Expr)   = { $, '+', ')' }
   FOLLOW(Term)   = { $, '+', ')', '*' }
   FOLLOW(Factor) = { $, '+', ')', '*' }
```

Let's look at this again, carefully, starting from the *definition*.

---

## Definition — what is FOLLOW

Let me pin it down in one line first.

> **FOLLOW(B)** = the set of **terminals** that can appear **immediately after** the nonterminal `B`
> somewhere in a valid sentence.\
> (And if `B` can come at the *very end* of a sentence, we also add `$`, which means the end of input.)

Paired up with FIRST, it goes like this — FIRST is the terminals at the *very front* of that symbol, FOLLOW is the terminals *immediately after* that symbol.

### Seeing "immediately after" as derivation

In the [FIRST definition](first-formula.md) we defined *derivation (⇒)* — replacing a nonterminal with the
right-hand side of a production.\
FOLLOW is about starting to derive from the start symbol `Expr` and seeing **what terminal ends up
attached right after `B`**.

```
   Expr  ⇒*  …  B  a  …       →  a came right after B   ⇒   a ∈ FOLLOW(B)
```

Written down tightly in symbols, it's this (`S` = start symbol, `T` = set of terminals).

```
   FOLLOW(B) = { a ∈ T | S ⇒* … B a … }   ∪   ( { $ }  if  S ⇒* … B )
```

> 📎 One thing that's decisively different from FIRST.\
> For FIRST you only had to look at that **one** symbol, but for FOLLOW you have to **scan
> *everywhere `B` is used* across the whole grammar**. Because what comes after `B` differs at every
> place where `B` is used.

---

## By the definition — deriving it directly

Let's work it out by *applying the definition directly*. Gathering "the terminals that come immediately after `B`" by hand.

### FOLLOW(Expr) — this comes out cleanly

`Expr` is the **start symbol** — that is, the *entire input* is one `Expr`.\
So once you've read that `Expr` all the way to the end, what comes right after it?\
The **end of input**, where there's nothing left to read.\
The imaginary token that marks that end of input was `$`.\
So the thing that comes right after `Expr` is `$` — **`$ ∈ FOLLOW(Expr)`.**

Now let's find, across the whole grammar, what comes *right after* `Expr`.

```
   Expr → Expr '+' Term      right after the front Expr :  '+'
   Factor → '(' Expr ')'     right after Expr :            ')'
```

What can come after `Expr` is `'+'` and `')'`, plus `$` at the very end. Gathering them all:

```
   FOLLOW(Expr) = { $, '+', ')' }
```

### FOLLOW(Term) — here we get stuck once

Let's find in the grammar what comes *right after* `Term`.

```
   Term → Term '*' Factor    right after the front Term :  '*'           →  add '*'
   Expr → Expr '+' Term      Term is at the very end of the production …  →  nothing comes after?
   Expr → Term               Term is at the very end of the production …  →  again nothing after
```

`'*'` goes straight in.\
But the last two lines are odd — `Term` is at the **very end** of the production, so no terminal comes right after it.

So what comes after `Term`?\
Looking at `Expr → Term`, `Term` is *all* of `Expr` — that is, **`Expr` and `Term` end at the exact same spot.**

That's abstract in words, so let's look at it as one scene.\
Picture the fragment `( Expr )` (`Factor → '(' Expr ')'`). Here `)` comes right after `Expr`.\
But if that `Expr` is just a single `Term` (`Expr → Term`), the same spot becomes `( Term )`.

```
   ( Expr )        →   ) after Expr
   ( Term )        →   now ) after Term   (same spot!)
```

Look — the `)` that was after `Expr` a moment ago is now **right after `Term`**.\
Because `Term` inherited `Expr`'s end position exactly as it was.

> 💡 **This is the core rule of FOLLOW.**\
> When some nonterminal comes at the **very end** of a production, it **inherits the entire FOLLOW of
> that production's LHS (the left-hand nonterminal).**\
> Here, because of `Expr → Term`, **`FOLLOW(Expr)` flows directly into `FOLLOW(Term)`.**

```
   FOLLOW(Term) = { '*' }  ∪  FOLLOW(Expr)
                = { '*' }  ∪  { $, '+', ')' }   =   { $, '+', ')', '*' }
```

### Why we get stuck here — "at the end, you inherit the LHS's FOLLOW"

As we just saw, when `B` comes at the **very end** of a production, you need that production's **left-hand
nonterminal (LHS)'s FOLLOW**.\
But that LHS's FOLLOW may itself still be in the middle of being filled in. (Exactly the same situation
where we got stuck on recursion back in FIRST.)\
So just *following the definition step by step* doesn't finish in one pass.

That's why on the next page — we'll **organize this process into a few rules** and, just like with FIRST,
solve it by **repeating until nothing changes**.

---

## Summary

Following the definition, FOLLOW ended up being filled in by three things.

1. The FOLLOW of the **start symbol** gets `$`. (Because it can come at the very end of a sentence.)
2. When **something comes right after `B`**, the *first terminal* of that following thing goes into FOLLOW(B).
3. When `B` comes at the **very end** of a production, it inherits that production's **LHS's FOLLOW**.

(In our example, whatever comes after `B` is always a terminal, so ② was simple; but if what comes after is a nonterminal, you look at its *FIRST* — we'll cover that on the next
page.)

## Next — turning this process into rules

Organizing these three into computation rules that work for *any grammar*, and solving them by repetition, is what comes next.

👉 **[FOLLOW · Computation Rules](follow-rules.md)**

---

👈 Back to the basic concepts: [FIRST / FOLLOW](first-follow.md)
