# lookahead (LR(1) items)

In the [SLR chapter](parse-table-slr.md), we answered the question *"when I meet `A → α •`, **on which next symbol** do I reduce?"* with `FOLLOW(A)`. But as we saw with [spurious conflicts](parse-table-slr.md) — `FOLLOW(A)` lumps together *the whole grammar*, so it was far too broad for *this particular spot*.

So we take one more step — **let's attach, to each individual item, the next symbol that fits *that exact spot*.** This "next symbol attached to an item" is exactly what **lookahead** is.

In this chapter we'll work out *exactly what lookahead is and how to compute it* — **leaving nothing out**. (CLR and LALR, coming next, both stand entirely on this lookahead.)

---

## 0. Symbol conventions first — so nothing gets confusing

The formulas use letters like `α` · `β` · `t`. Before we start, let me make clear *what each of these letters is*. (Skip this and you'll get confused right away.)

So far, the `a` `b` `c` `d` `x` `y` in our examples were **actual symbols baked into a grammar (terminals)**. But when we write a *formula*, we need **blanks (placeholders)** that stand for "*any* rule, *any* symbol."

| Symbol | What kind of *blank* is it? | Examples it can be filled with |
|:--:|:--|:--|
| `A` `B` | a single *nonterminal* (a rule name) | the `A` · `B` · `S` … of the examples |
| `α` `β` | a *string of symbols* — **zero or more** terminals/nonterminals in a row | the empty string · `c` · `A c` · `a A` … |
| `t` | a single *terminal* — **the lookahead slot** | `c` · `d` · or end of input `$` |

> ⚠️ **A note on notation — we write lookahead as `t`, not `a`.** Textbooks usually write the lookahead slot as `a`. But our examples have an *actual symbol* `a` (e.g. `S → a A c`), so `a` would overlap two meanings — the *actual symbol* and the *lookahead slot*.\
> So **in this manual we'll write the lookahead slot as `t`** (terminal). **`t` isn't a specific symbol — it's a blank meaning *"the one next terminal that comes here."*** What actually fills that slot is something *we compute with a formula*.

In short: `α` `β` `t` = **blanks**, &nbsp;&nbsp; `a` `b` `c` … = **actual symbols.**

---

## 1. LR(1) items — attaching a lookahead to an item

The items from the [closure chapter](closure-def.md) were *rule + dot* — e.g. `A → α • β` (everything *before* the dot, `α`, is already read; everything *after*, `β`, is not yet).

An **LR(1) item** is this with **one lookahead symbol (`t`)** attached.

<pre class="lrbox">
[ <span class="nt">A</span> → α <span class="lrdot">•</span> β , <span class="setm">t</span> ]
</pre>

Here's how to read the `t` tacked on at the end:

> *"After reading `A → α β` all the way to the end and reducing into `A`, if the *very next* symbol is `t` — then that reduction is correct."*

In other words, **`t` is *the next symbol on which it's OK to reduce by this rule*.** (Which is why it's called "lookahead.")

**So — "why is the lookahead `t`?"** `t` isn't a symbol's name; it's a blank meaning **"the slot for the next symbol that comes after reducing."** Figuring out *which symbol actually goes into that slot* is exactly what §2 below does.

> 💡 **Compared with SLR:** SLR lumped lookahead at the *nonterminal level* — like "rule `A` gets `FOLLOW(A)`." An LR(1) item works at the *item level* — even the same `A → b •` can carry a different `t` depending on *which state it's in*. This *fine-grainedness* is the heart of the chapters to come.

---

## 2. That lookahead, how do we compute it — from closure, via FIRST

Items multiply through *closure* ([how to compute the closure](closure-calc.md)) — that was *expanding the rules of a nonterminal whenever one sits right after the dot*. **In LR(1), as we expand, we also pin down the lookahead of each new item.**

Here's the situation as we expand. Suppose we already have this item:

<pre class="lrbox">
[ <span class="nt">A</span> → α <span class="lrdot">•</span> <span class="nt">B</span> β , <span class="setm">t</span> ]
</pre>

There's a nonterminal `B` after the dot (with `β` after it, and this item's lookahead `t`). Closure pulls in the `B → γ` rules anew:

<pre class="lrbox">
[ <span class="nt">B</span> → <span class="lrdot">•</span> γ , ? ]      ← what goes in this new item's lookahead slot?
</pre>

**Think about it — once we've finished reducing `B`, what comes *right after* it?**

<pre class="lrbox">
[ <span class="nt">A</span> → α <span class="lrdot">•</span> <span class="nt">B</span> <span class="lrmark">β</span> , <span class="setm">t</span> ]
</pre>

It's the `β` that sat behind `B` in the original item. So `B`'s lookahead is *the first symbols of `β`* (`FIRST(β)`).

But if **`β` is empty or can disappear entirely** (i.e. effectively nothing follows `B`), then what comes after `B` is *the original item's lookahead `t`*.

> new lookahead = <span class="setm">t</span>

**Why `t`?** The original item is `[ A → α • B β , t ]`; when `β` vanishes it effectively becomes `[ A → α • B , t ]`, and expanding `A` by this item leaves `B` at the *very end*. Once `B` is reduced, `A` is finished too. So what comes after `B` is just *what comes after `A`* — and the item already tells us that: its lookahead `t`. That's why `B` inherits that `t`.

Folding both cases into one formula gives —

> ### new lookahead = `FIRST( β t )`

(`FIRST(β t)` = gather the FIRST of `β`, and if `β` can vanish entirely, include `t` too — exactly the [FIRST](first-formula.md) definition.)

That's *all there is* to the formula for computing lookahead. It's *not a new computation* — it's just **applying the FIRST you already learned to "`β` after the dot."**

---

## 3. By hand once — with actual symbols

Let's do it with *actual symbols* instead of blanks (`α β t`). A small grammar:

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">(</span> <span class="nt">A</span> <span class="setm">)</span>
<span class="nt">A</span> → <span class="setm">n</span>
</pre>

(`(` `)` `n` are actual terminals. Think of `n` as a token like a number or a name.)

Starting out and reading `(`, we have this item (the outermost is closed off by end of input `$`, so its lookahead is `$`):

<pre class="lrbox">
[ <span class="nt">S</span> → <span class="setm">(</span> <span class="lrdot">•</span> <span class="nt">A</span> <span class="setm">)</span> , <span class="setm">$</span> ]
</pre>

A nonterminal `A` follows the dot. Time to expand and pull in `A → n` — **plug it into the formula `FIRST(β t)`**:

- nonterminal `B` after the dot = `A`
- the `β` left after `A` = `)`
- this item's lookahead `t` = `$`
- → new lookahead = `FIRST( ) $ )` — but the leading `)` is a terminal, so it can't vanish, which means FIRST never even reaches the `$` behind it. So the `$` drops out and `FIRST( ) )` = <code><span class="setb">{</span><span class="setm"> ) </span><span class="setb">}</span></code> &nbsp; (`)` is a terminal, so it's itself)

So the new item is:

<pre class="lrbox">
[ <span class="nt">A</span> → <span class="lrdot">•</span> <span class="setm">n</span> , <span class="setm">)</span> ]
</pre>

Read `n` and the dot moves over — **`[ A → n • , ) ]`.**\
How to read it: *"right after reducing `A → n`, if `)` comes, it's correct."* — of course, since in `( n )` a `)` comes after `n`.

**Let's also see the case where `β` is empty.** What if the rule weren't `S → ( A )` but just `S → A`? When expanding `A` in `[ S → • A , $ ]` — there's *nothing* left after `A` (`β` is empty). Then the formula is `FIRST( $ )` = <code><span class="setb">{</span><span class="setm"> $ </span><span class="setb">}</span></code>, i.e. it **inherits the parent's lookahead `$` directly**, giving `[ A → • n , $ ]`.

> 🔖 **In one line** — a lookahead is *the "next terminal it's OK to reduce on" attached to each item*, and you compute it in closure as **`FIRST(β t)`** (the FIRST of `β` after the dot, inheriting the parent `t` if `β` vanishes).

---

## Next

Now we know *what a lookahead is and how it comes out*. Carry this **right from the moment you build the states** and fill in the table with it — and that's **CLR**. Let's go.

👉 **[The Parse Table · CLR — How It Works](parse-table-clr.md)**
