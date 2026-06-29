# Closure · How to Compute

> 🎓 This is an **advanced track** chapter.\
> In the previous [Closure · Definition](closure-def.md) — we said `CLOSURE(I)` is *"the smallest set closed under ②."*\
> The definition only tells us *"which set it is."* This page shows **how you actually compute it** — and we'll do it by building, by hand, the **real start state `I₀`** of our example
> grammar, one step at a time.

## The method

The method itself is simple.

> Start from some item set `I`, and **apply rule ② one step at a time *until there's nothing left to add*.**

The key is *"until there's nothing left to add."* The set **grows** one step at a time, and at some moment *nothing new comes in* — that's when it's closed, and you stop.\
It's exactly the same flavor as the **fixed point (repeat until nothing changes)** we saw in [FIRST/FOLLOW](first-rules.md).

## Before that — where do we start? (the augmented grammar)

Closure starts from *a single start item.* Where does that one item come from?

Here a small device shows up. An LR parser **tacks exactly one start rule onto the very top** of the original grammar —
`Accept → Expr`. (`Expr` is the original start symbol of our grammar.)\
A grammar with *one extra start rule laid on top* like this is called an **augmented grammar.**

Why tack on such a thing? — **so the parser can cleanly tell that "now the whole input is done."**\
The original start symbol `Expr` shows up *all over the inside* of expressions (the `Expr` in `Expr '+' Term`, the `Expr` in `'(' Expr ')'`, and so on). So finishing a single `Expr` doesn't mean the *whole input* is done — it might just be part of a bigger expression.\
But if you lay on a single `Accept` that *isn't used anywhere else* — then **the very moment you finish `Accept → Expr` is exactly "the whole thing is done (accept)"**, and you know it at a glance. (How this "done signal" actually gets used is something we'll tie up in the *parse table* chapter. For now we just use it as a *starting point.*)

In plain terms — picture a **shipping box.** A box can have yet another smaller box inside it (just as our `Expr` has another `Expr` inside, like in `'(' Expr ')'`). Then it gets confusing *"which one is the outermost box."*\
So you put **one more outer box wrapping the whole thing** and write on it *"this is the final outer wrapping."*\
That outer box is exactly `Accept → Expr` — once even the outer box is closed (completed), you know at a glance that *"it's all done (accept)!"*

So the starting item is just one — `Accept → • Expr`.\
The result of running closure on it is precisely the **start state `I₀`**. Let's watch it grow with our own eyes.

## One step at a time — watching `I₀` grow

Before we follow along, let me lay the **augmented grammar** out here. (So that at each step we can pin down right away *which production* comes in.)

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>   |  <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>  |  <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     |  <span class="setm">id</span></pre>

**Start — 1 item.** We begin from a single virtual start item.

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span></pre>

**Step 1.** After the dot in `Accept → • Expr` comes `Expr`.\
Looking at the `Expr` line in the grammar, its productions are the two **`Expr → Expr '+' Term`** and **`Expr → Term`**.\
We add these two to the set — with the dot at the very front, since we haven't read anything yet.

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>       ← new
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>                ← new</pre>

→ **3 items.**

**Step 2.** Let's look at what comes after the dot in the two that just came in.

- After the dot in `Expr → • Term` comes `Term`. In the grammar, `Term`'s productions are the two **`Term → Term '*' Factor`**
  and **`Term → Factor`**. We add these two (with the dot at the very front).
- After the dot in `Expr → • Expr '+' Term` comes `Expr` too — but `Expr` was **already expanded in Step 1** (those two
  rules are already in the set). So there's nothing new to add.

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>     ← new
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>              ← new</pre>

→ **5 items.**

**Step 3.** Again, let's look at what comes after the dot in the two that just came in.

- After the dot in `Term → • Factor` comes `Factor`. In the grammar, `Factor`'s productions are the two **`Factor → '(' Expr ')'`**
  and **`Factor → id`**. We add these two.
- After the dot in `Term → • Term '*' Factor` comes `Term` → **already expanded, so nothing new.**

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>        ← new
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span>                  ← new</pre>

→ **7 items.**

**Step 4.** Let's look at the remaining dots. After the dot in `Factor → • '(' Expr ')'` comes `'('`, and after the dot in
`Factor → • id` comes `id` — both are **terminals.**\
A terminal has no *production to start from*, so there's nothing to expand. And nothing left to add either → **it's closed. Done!**

## Wrapping up — this is `I₀`

The set grew **1 → 3 → 5 → 7**, and then *stopped, because there was nothing left to grow.*\
This final **7-item closed set** is precisely the **start state `I₀`** of our grammar.

<pre class="lrbox">   I₀ = CLOSURE( { <span class="nt">Accept</span> → •<span class="nt">Expr</span> } )
      = the 7 items where the productions of <span class="nt">Accept</span>·<span class="nt">Expr</span>·<span class="nt">Term</span>·<span class="nt">Factor</span> all gather with the dot at the very front</pre>

*"Everything that can come at the very front"* is what's gathered into `I₀` — the `(` and `id` after the dot in
`Factor → •'(' Expr ')'` and `Factor → •id` are exactly the *terminals you can read first.* (Hey, that's the same as
[FIRST(Expr)](first-rules.md) `= { '(', id }`! That's no coincidence.)

## One more step — the author writes this process as 'recursion'

Just now we watched the set **grow sideways one step at a time.** The author's design notes write the same computation
in a slightly more compact form — a *recursive* shape where **`Closure` contains another `Closure`** inside it.

Once you know how to read it, it isn't hard.

- `Closure({ … })` is a mark meaning *"these items still need more expanding."*
- The <span class="lrmark">red symbol</span> is the *symbol after the dot we're currently expanding* (the marker).
- Each time we go down one line, the productions of that red nonterminal get pulled in as a new `Closure({ … })`. (Items
  already sorted out on an earlier line are *omitted*, and all gathered on the very last line.)

<pre class="lrbox">Closure({ <span class="nt">Accept</span> → • <span class="lrmark">Expr</span> })
 = { <span class="nt">Accept</span> → • <span class="nt">Expr</span>,   Closure({ <span class="nt">Expr</span> → • <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>,  <span class="nt">Expr</span> → • <span class="nt">Term</span> }) }
 = { <span class="nt">Expr</span> → • <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>,  <span class="nt">Expr</span> → • <span class="lrmark">Term</span>,   Closure({ <span class="nt">Term</span> → • <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>,  <span class="nt">Term</span> → • <span class="nt">Factor</span> }) }
 = { <span class="nt">Term</span> → • <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>,  <span class="nt">Term</span> → • <span class="lrmark">Factor</span>,   Closure({ <span class="nt">Factor</span> → • <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>,  <span class="nt">Factor</span> → • <span class="setm">id</span> }) }
 = { <span class="nt">Factor</span> → • <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>,  <span class="nt">Factor</span> → • <span class="setm">id</span> }      <span style="opacity:.6">(after the dot is '(' · id — terminals, so it stops)</span>
 = { <span class="nt">Accept</span>→•<span class="nt">Expr</span>, <span class="nt">Expr</span>→•<span class="nt">Expr</span><span class="setm">'+'</span><span class="nt">Term</span>, <span class="nt">Expr</span>→•<span class="nt">Term</span>, <span class="nt">Term</span>→•<span class="nt">Term</span><span class="setm">'*'</span><span class="nt">Factor</span>, <span class="nt">Term</span>→•<span class="nt">Factor</span>, <span class="nt">Factor</span>→•<span class="setm">'('</span><span class="nt">Expr</span><span class="setm">')'</span>, <span class="nt">Factor</span>→•<span class="setm">id</span> }   <span style="opacity:.6">= I₀</span></pre>

At the very end the `Closure({ … })` *disappeared, didn't it?* That means there's nothing left to expand — that is, it's
**closed.** And that's `I₀`.\
(The promise that *"a nonterminal already expanded won't be expanded again"* holds here too — which is why the `Expr` in
`Expr → •Expr '+' Term` and the `Term` in `Term → •Term '*' Factor` don't get caught as markers. Otherwise it would loop
forever.)

And this **recursive shape of `Closure` calling `Closure`** is *the spitting image* of the code in the next
[implementation](closure-impl.md), `result.UnionWith(Closure( … ))`. It's just what we wrote by hand, moved straight
into code.\
(The author's original notes are drawn with their own test grammar `S' → G`, `G → E = E | f`, …, but the expansion
principle is exactly the same.)

## Next

This "one step at a time until it closes" that we ran by hand — how did the code do it?

👉 **[Closure · Implementation](closure-impl.md)**

---

👈 Previously: [Closure · Definition](closure-def.md)
