# FIRST · Computation Rules

> 🎓 This is the **Advanced track · Theory**.\
> In the previous page, [FIRST · Definition & Derivation](first-formula.md), we grabbed the *definition* and even *derived it straight from the definition*.\
> But we ran into **the wall where, with recursion, the derivation grows infinitely long.**
>
> So this page is — a **computation rule** that pulls out the same FIRST **without expanding the derivation directly**.\
> (Recursion gets handled gracefully too.)\
> How that rule is implemented in code is over at → **[FIRST · Implementation](first-impl.md)**.

> Don't try to take it all in at once.\
> We'll go **one step at a time, starting from the easy bits.**

## First — let's lay out the whole cast and begin

Before working out FIRST, let's see *what's in* this grammar at a glance.\
A grammar's symbols come in just **two kinds**.

- **terminal** — a token that actually shows up in the input
- **nonterminal** — a rule name

Splitting the two apart in our example grammar, it looks like this.

<pre class="lrbox">
   terminal list    :   <span class="setm">(</span>    <span class="setm">)</span>    <span class="setm">+</span>    <span class="setm">*</span>    <span class="setm">id</span>        <span style="opacity:.6">← 5 of them</span>
   nonterminal list :   <span class="nt">Expr</span>    <span class="nt">Term</span>    <span class="nt">Factor</span>        <span style="opacity:.6">← 3 of them</span>
</pre>

FIRST is worked out *for each one of these symbols*.\
So our job is clear — **fill in the FIRST of 5 terminals + 3 nonterminals, 8 in all.**\
And here's the good news — **the terminal side is almost free.** Let's start there.

## FIRST of a terminal — every one is itself (done in one shot)

A terminal starts with *itself*.\
Of course it does — `+` starts with `+`.

**How to read it first.** `FIRST( '(' ) = { '(' }` reads as — *"the FIRST set of the terminal `(` is the single `(`."* (`{ }` is a *set*, and what's inside it are the elements.)

So the 5 terminals, all in a row —

<pre class="lrbox">
<span class="setf">FIRST(</span> <span class="setm">'('</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span> <span class="setb">}</span>   <span style="opacity:.6">← the FIRST of terminal '(' is itself</span>
<span class="setf">FIRST(</span> <span class="setm">'+'</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'+'</span> <span class="setb">}</span>   <span style="opacity:.6">← the FIRST of terminal '+' is itself</span>
<span class="setf">FIRST(</span> <span class="setm">')'</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">')'</span> <span class="setb">}</span>   <span style="opacity:.6">← the FIRST of terminal ')' is itself</span>
<span class="setf">FIRST(</span> <span class="setm">'*'</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'*'</span> <span class="setb">}</span>   <span style="opacity:.6">← the FIRST of terminal '*' is itself</span>
<span class="setf">FIRST(</span> <span class="setm">id </span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">id </span> <span class="setb">}</span>   <span style="opacity:.6">← the FIRST of terminal id is itself</span>
</pre>

Summed up in one line — **`FIRST(terminal a) = { a }`** (*a terminal's FIRST is always itself*).\
5 terminals, and that's **done.** We filled in 5 of the 8 for free. 🙂

## FIRST of a nonterminal — the big picture first

Now the real subject, the three nonterminals (`Expr` `Term` `Factor`).\
Before that, let me grab the big picture first.

> 📎 Quick aside, one term. In `Factor : '(' Expr ')' | id`, **each individual** alternative split by `|` is called a
> **production**.\
> It's *"a single line that makes a nonterminal,"* like `Factor → id`. (Details in [Single](deep-single.md).)\
> From here on we'll use this word.

The FIRST of one nonterminal is worked out like this — **take the FIRST of each of that nonterminal's
productions, and combine them all (union).**

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Factor</span>'s production 1<span class="setf">)</span> ∪ <span class="setf">FIRST(</span><span class="nt">Factor</span>'s production 2<span class="setf">)</span> ∪ …
</pre>

So the question we really have to solve narrows down to one — **how do you work out the FIRST of a single production?**

The answer is surprisingly simple.\
It depends on **what that production starts with**, and there are exactly **three** cases.

1. **Case ①** — when it starts with a terminal
2. **Case ②** — when it starts with a nonterminal
3. **Case ③** — when the front can disappear (ε)

Let's look at them one by one, starting from the easy one.

## Case ① — when a production **starts with a terminal**

This is the easiest case. In fact, it's just **using what we already saw above.**

If the front is a terminal, that production's FIRST is exactly **that terminal**.\
Just above, we said *"a terminal's FIRST is itself,"* right? **This is the exact same story** — the front terminal is the answer.\
(It doesn't matter if there are more symbols after the production. If the front is a terminal, that's the first terminal, so it ends right there.)

For example, `Factor`'s two productions are these.

<pre class="lrbox">
   <span class="nt">Factor</span> → <span class="setm">id</span>            <span style="opacity:.6">front is terminal id   →   FIRST = { id }</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>  <span style="opacity:.6">front is terminal (    →   FIRST = { '(' }</span>
</pre>

Even though `'(' Expr ')'` has `Expr ')'` after it, it ends right at the front `(`.

Since `Factor` has only these two productions, combining them completes it right away.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span> <span class="setb">}</span> ∪ <span class="setb">{</span> <span class="setm">id</span> <span class="setb">}</span> = <span class="setb">{</span> <span class="setm">'('</span>, <span class="setm">id</span> <span class="setb">}</span>
</pre>

First nonterminal done! 🙂

## Case ② — when a production **starts with a nonterminal**

This time the front isn't a terminal but *another nonterminal*. Our grammar's `Expr → Term` is exactly that — the front is the nonterminal `Term`.

So what's this production's FIRST? — **we bring the front `Term`'s FIRST over as is.** That is, **`FIRST(Expr) = FIRST(Term)`**.

**Why is that?**\
Back to the definition — FIRST is *"the terminal that appears first when you derive."* When you derive `Expr → Term`, `Term` takes the front spot, so the front terminal that comes out when you expand all the way is also ultimately *decided by `Term`.*

**Let's see it directly with a derivation.** Expanding `Expr → Term` all the way —

<pre class="lrbox">
<span class="nt">Expr</span>  ⇒  <span class="nt">Term</span>  ⇒  <span class="nt">Factor</span>  ⇒  <span class="setm">id</span>            <span style="opacity:.6">front terminal = id</span>
<span class="nt">Expr</span>  ⇒  <span class="nt">Term</span>  ⇒  <span class="nt">Factor</span>  ⇒  <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span>      <span style="opacity:.6">front terminal = (</span>
</pre>

> 🎨 *purple = nonterminal (`Expr`·`Term`·`Factor`), teal = terminal (`id`·`(`·`)`).*

The front spot is held by `Term` (→ `Factor` → …) from start to finish, right?\
So the terminals that come out at the front, `id` · `(`, are exactly **the first terminals `Term` produces** — that is, `FIRST(Term)` itself.\
So **`FIRST(Expr) = FIRST(Term)`**.\
(`Expr`'s other production, `Expr '+' Term`, is the *left recursion* we'll see shortly, so it adds nothing new, and this equality holds exactly.)

> 🔖 **Generalized in one line:** *if the front of a production is a nonterminal — bring that nonterminal's FIRST over as is.* (Except, if that nonterminal is *itself*, you get snagged once — right below.)

### But — what if that nonterminal is *itself*? (left recursion)

Here you get snagged once.\
Look at the first production of `Term : Term '*' Factor | Factor`.

<pre class="lrbox">
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>     <span style="opacity:.6">front is again Term — that's itself?!</span>
</pre>

Trying to work out `FIRST(Term)`, the front nonterminal is again `Term`.\
That is, to work out `FIRST(Term)` you need `FIRST(Term)` — a **chicken-and-egg** situation.\
As is, this doesn't solve in one go. But this kind of *direct* left recursion (where it bites itself directly) is actually easy.

**Easy recursion — just drop that rule**

Just ask what `Term` can *start* with. `Term` has two rules, and they differ in what the front becomes.

<pre class="lrbox">   ① <span class="nt">Term</span> → <span class="nt">Factor</span>            <span style="opacity:.6">front is Factor (settled!)</span>
   ② <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>   <span style="opacity:.6">front is Term again (no progress)</span></pre>

`②` leaves a `Term` at the front, so it never settles the first symbol (`Term` just comes back as `Term`). So the only rule that settles the front is `①`, and it makes the front a `Factor`. So **`Term` always starts with a `Factor`**, which means **`FIRST(Term)` = `FIRST(Factor)` = `{ '(', id }`.**

`Expr` (`Expr : Expr '+' Term | Term`) is the same direct left recursion, so by the same reasoning it's `{ '(', id }`.

But this "just drop the in-place rule" shortcut only works for *direct* left recursion like `Term` — where it bites itself directly. When nonterminals bite each other *in a loop* (**indirect left recursion**), it's a different story.

<pre class="lrbox">
   <span class="nt">A</span> → <span class="nt">B</span> …
   <span class="nt">B</span> → <span class="nt">A</span> …
</pre>

`A` starts with `B`, and that `B` starts with `A` again, so there's no *directly self-biting* "in-place rule" anywhere to drop. To handle even cases like this all at once, the engine doesn't distinguish direct from indirect — it **handles every nonterminal the same way, by starting from the empty set and repeating until nothing more grows.**

For direct left recursion like `Term`, this repetition finishes in a single round, so it's practically free. **Where the repetition really shows its power is when nonterminals tangle up like this** — and that comes up naturally in the next [FOLLOW](follow-formula.md) chapter, on this very expr grammar.

> 💡 The wall of recursion where the derivation kept *growing infinitely long* on the previous page ([Definition & Derivation](first-formula.md)) — **getting over that wall is exactly this "repetition."**\
> Instead of expanding all the way, we grow the set little by little and stop when it stops changing.

## Case ③ — when the front nonterminal **can disappear** (ε)

The last case.\
In Case ② we said *"bring the front nonterminal `Y`'s FIRST over,"* right?\
But if that `Y` can also **disappear into nothing (ε)**, there's one more thing to take care of.\
(When a nonterminal can derive even the empty string, we call it *nullable*.)

**Why do we have to look at the next symbol too?**\
Again, it's the definition — FIRST is *"the terminal that appears first when you derive."*\
But if the front `Y` disappears into ε, the front of the derivation result is taken not by `Y` but by **the very next symbol**.\
Then the first terminal that next symbol derives can come to the front too.\
So we have to add **the next symbol's FIRST too** to `Y`'s FIRST to get it right.\
This rule of *"if the front can disappear, move on to the next symbol and combine"* is called **⊕ (ring-sum)**.

```
   A ⊕ B =  A              (if A can't disappear → it ends there)
            (A-ε) ∪ B      (if A can disappear → drop ε, and add B too)
```

We need to show ε here, but **our expr grammar has no nullable**, so instead of expr we use a small grammar. Here's the grammar we'll use for examples in this chapter:

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

A grammar where `A` and `B` can *each disappear into ε* (nullable).

Let's start with `A` and `B`'s FIRST. Both have an `ε` branch, so `ε` gets in.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">A</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">a</span>, ε <span class="setb">}</span>
   <span class="setf">FIRST(</span><span class="nt">B</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">b</span>, ε <span class="setb">}</span>
</pre>

Now for `FIRST(S)`. In `S → A B`, since **the front `A` can disappear**, ⊕ doesn't stop at the first slot — it moves on to the next slot `B`.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">S</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">A</span><span class="setf">)</span> ⊕ <span class="setf">FIRST(</span><span class="nt">B</span><span class="setf">)</span> = ( <span class="setb">{</span> <span class="setm">a</span>, ε <span class="setb">}</span> − ε ) ∪ <span class="setb">{</span> <span class="setm">b</span>, ε <span class="setb">}</span> = <span class="setb">{</span> <span class="setm">a</span>, <span class="setm">b</span>, ε <span class="setb">}</span>
</pre>

If `A` didn't disappear it would end at `{ a }`, but since `A` can become ε, `B`'s first symbol `b` can come to the front too. And if `B` disappears as well, `S` becomes empty entirely, so `ε` gets in too. This is ⊕ actually at work.

> Our expr grammar (Expr/Term/Factor) has no nullable, so there ⊕ always stops right at the front symbol. Still, the rule has to include this ε handling to be correct.

## Summary — the three cases are really one formula

We split it into **Cases ①②③** above, but in fact these three combine into **a single formula.**

A production is, after all, *a sequence of symbols* (like `Term '*' Factor`).\
Its FIRST is — **the FIRST of the component symbols, ⊕ (ring-summed) in order**, that's all.

```
   FIRST(X₁ X₂ … Xₙ) = FIRST(X₁) ⊕ FIRST(X₂) ⊕ … ⊕ FIRST(Xₙ)
```

So the three cases from before are just a difference in **where this ⊕ *stops*.**

- **Case ①** : the first symbol is a terminal. A terminal can't be ε, so ⊕ **stops at the first slot** and becomes `{ that terminal }`.
- **Case ②** : the first symbol is a nonterminal (that can't be ε). ⊕ **stops at its FIRST** and becomes `FIRST(that nonterminal)`.
- **Case ③** : the first symbol can be ε. So ⊕ **moves on to the next slot** and keeps combining.

For example, ⊕-ing `Term '*' Factor` directly looks like this.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span> ⊕ <span class="setf">FIRST(</span><span class="setm">'*'</span><span class="setf">)</span> ⊕ <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span>
                          = <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span>              <span style="opacity:.6">← Term can't be ε, so it stops at the first slot</span>
</pre>

The code is exactly this too — it `RingSum(⊕)`s the symbols of a production ([Concat](deep-concat.md)) in order, and stops when there's no more ε to look at.

```csharp
// FirstFollowAnalyzer [First].cs
public TerminalSet FirstSet(NonTerminalConcat singleNT, ...)
{
    TerminalSet result = new TerminalSet();

    foreach (var symbol in singleNT)                        // the production's symbols in order
    {
        result = result.RingSum(FirstSet(symbol, seenNT));  // ⊕ one slot
        if (!result.IsNullAble) break;                      // no more ε to look at → stop
    }

    return result;
}
```

**Verification** — run this rule on our grammar, and all three come out the same.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Expr</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span>, <span class="setm">id</span> <span class="setb">}</span>
</pre>

Exactly the same as the answer we worked out by hand on the [Definition & Derivation page](first-formula.md). ✓

---

Finally — take the **⊕** we've seen so far (one production), wrap one more layer of *alternative (`|`) combining* **`∪`** around it, and **this is the *whole* of FIRST:**

<pre class="lrbox">
   FIRST(<span class="nt">A</span>) = ⋃ ( FIRST(X₁) ⊕ FIRST(X₂) ⊕ … )
</pre>

- outer **`⋃`** : *combines* `A`'s several productions (the `|` alternatives). &nbsp;*(= a nonterminal's FIRST = the union of all its productions' FIRST)*
- inner **`⊕`** : *joins* one production (a symbol sequence), *but moves to the next slot if the front disappears*. &nbsp;*(= Cases ①②③)*
- bottom **`FIRST(terminal a) = { a }`**.

In this one line — *Cases ①②③* and *alternative combining* are **all** in there. FIRST really is just this. 🎯

## Next — this rule, in code

The three cases we just saw — starts with terminal · starts with nonterminal · ε — and "repeat until nothing
changes," are baked into the `FirstFollowAnalyzer` code **almost line for line.**\
Let's look on.

👉 **[FIRST · Implementation (code)](first-impl.md)**

---

👈 Previous: [FIRST · Definition & Derivation](first-formula.md)
