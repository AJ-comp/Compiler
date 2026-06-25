# Closure · Definition

> 🎓 This is an **advanced track** chapter.\
> In the previous [State](lr-state.md) chapter — we saw that a state is a *set of LR items `Iₓ`*.\
> But to build a state *properly*, you have to inflate its items with a **closure**.\
> Just like [FIRST/FOLLOW](first-formula.md), let's split closure into **definition · how to compute · implementation** — this page is
> the first step, the **definition**.

> 📍 **Where it lives** · `Analyzer.Closure` · `…/Parsers/Analyzer.cs`

Closure is an operation that works on a **state** — the *set of LR items `Iₓ`* we saw in the previous [State](lr-state.md) chapter.
To be precise, its job is to **fill a state out completely**. Let's see what that means.

## Why do we have to "inflate" it

Suppose some state holds a single item, `Expr → • Term`.\
Right after the dot is the **nonterminal `Term`**. In other words, we're just about to read a `Term`.

But reading a `Term` means — in the end, starting *one of `Term`'s productions*.\
It could be `Term → Term '*' Factor`, or it could be `Term → Factor`.\
So **those productions, in their "about to start" form,** also have to live in this state alongside the original item.

If all you have is the lone `Expr → • Term`, then *how to start a Term* is missing — it's an **incomplete state**.\
Filling that gap to make it a *complete state* is what **closure** does.

## Definition — what closure is

We pinned [FIRST](first-formula.md) down in one sentence as *"of everything that symbol can derive, the **terminals that can come at the very front**"*. Closure can be pinned down in one sentence just the same way.

> **Closure(`I`)** = the set of items you get by **gathering, without leaving a single one out,** every production in that state that *"can just start now."*

*"Can just start now"* — that's the heart of it. The fact that a nonterminal sits right after the dot of some item means *"we're about to start that nonterminal."* So the productions that build that nonterminal are *candidates that may just start*, and we have to pull them all in (with the dot placed at the very front).

Written out a bit more carefully, as **two rules**, it goes like this.

① **Keep every item that was already in `I`** — we throw away nothing.\
② **If some item has a *nonterminal right after the dot*, add that nonterminal's productions (with the dot marked at the very front).**

To see what ② actually looks like, let's pull off just **one slice of our grammar** — how ② now fills the *gap* of that `Expr → • Term` from the *'why inflate it'* section above. (This is also *part* of what really happens inside the start state `I₀` we'll build soon.) Here's the grammar.

```
   Expr   → Expr '+' Term   |  Term
   Term   → Term '*' Factor  |  Factor
   Factor → '(' Expr ')'     |  id
```

Inside the set sits that `Expr → • Term`.

<pre class="lrbox">   Expr → <span class="lrdot">•</span> Term</pre>

Right after the dot is the nonterminal `Term`. So ② kicks in — it finds `Term`'s two productions in the grammar, and\
*since nothing has been read yet*, marks the dot at the very front and adds them to the set.

<pre class="lrbox">   Expr → <span class="lrdot">•</span> Term
   Term → <span class="lrdot">•</span> Term '*' Factor      <span style="opacity:.65">← pulled in by ②</span>
   Term → <span class="lrdot">•</span> Factor               <span style="opacity:.65">← pulled in by ②</span></pre>

Following the *nonterminal after the dot* and dragging its productions in — that's **one application of ②**.\
And the `Term → • Factor` that just came in has a nonterminal after its dot too (`Factor`), so ② applies again… and this keeps going *until there's nothing left to pull in*.

(Here we looked at just *one slice* of `I₀`. The process of starting from `Accept → • Expr` and running it **from start to finish** to complete `I₀`, we'll walk through one step at a time in the next chapter, [How to compute](closure-calc.md).)

Writing the **two rules (①②)** above in short symbolic form gives this. (`A → α • B β` is exactly the notation we saw in the [LR item](lr-item.md) chapter — `α`·`β` are the symbols before and after the dot, `B` is the nonterminal right after the dot, and `γ` is the right-hand side of a `B` rule.)

> **CLOSURE(I)** = the **smallest** item set that is **closed** under ①② below
>
> ① contains every item of `I`.\
> ② if `A → α • B β` is present (a nonterminal `B` after the dot), it also contains every production `B → • γ` of `B`.

Here you only need to grasp these two phrases, **"closed"** and **"smallest."**

- **closed** — a state where *"there's nothing left to add."* No matter how many more times you apply ②, if nothing new comes out, it's closed.\
  If all you have is `{ Expr → • Term }`, then the rules for the `Term` after the dot are missing, so it's *not yet* closed. Only once you've put in the `Term` items, and the `Factor` items that get summoned from there too, does it *finally* become closed. (That's why the name is *closing* = closure.)
- **smallest** — *"only as much as is strictly needed."* You put in only the items ② *told you to*, never something it didn't even ask for.

> 📎 **Note — this is the "LR(0)" closure.** The items here are just *production + dot*, so they carry no **lookahead** information like *"which token must come next."* That's why a nonterminal only needs to be expanded once each, *just by its name* (there's never a reason to expand the same nonterminal twice).\
> *"And so which token to reduce on"* — that's decided not by closure but by a **later stage**. SLR decides it with [FOLLOW](follow-formula.md), and LALR with a separately computed lookahead. At the closure stage we don't worry about any of that.

## Next

The definition said it's *"the smallest set closed under ②."*\
So next is **how you actually compute it** — running it yourself, one step at a time.

👉 **[Closure · How to compute](closure-calc.md)**

---

👈 Previously: [State](lr-state.md)
