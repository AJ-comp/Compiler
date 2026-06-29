# FOLLOW · Definition and derivation

> 🎓 This is the **advanced track · theory**.\
> Now that you've finished [the FIRST track](first-formula.md), here comes its partner, **FOLLOW**.\
> If FIRST was about **"what does it *start* with,"** FOLLOW is about **"what comes *after* it."**\
> On this page we'll look at the **definition** and at **deriving it directly from the definition**. Then on → **Computation rules** → **Implementation**.

> 📍 **Where it lives** · engine `FirstFollowAnalyzer` · module `Janglim.FrontEnd` — **Layer 2** (the tier
> *below* the parse table)

Here's the example grammar we keep using.\
This time the **start symbol** matters, so let me mark it — the `Expr` at the very top is the start symbol.

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;      ← start symbol
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
</pre>

The answer we worked out by hand in [the basic track](first-follow.md) was this.

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
   FOLLOW(<span class="nt">Term</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
   FOLLOW(<span class="nt">Factor</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

Let's go through this again, carefully, starting from the *definition*.

---

## Definition — what FOLLOW is

Let me pin it down in one line first.

> **FOLLOW(B)** = the set of **terminals** that can appear **immediately after** the nonterminal `B`
> somewhere in a valid sentence.\
> (And if `B` can come at the *very end* of a sentence, we also include `$`, which stands for the end of input.)

Lining it up with FIRST as a pair — FIRST is the terminal at the *very front* of a symbol, FOLLOW is the terminal *right behind* it.

### Seeing "right behind" through derivation

Back in [the FIRST definition](first-formula.md) we defined *derivation (⇒)* — swapping a nonterminal for the right-hand side of a production.\
FOLLOW is about deriving outward from the start symbol `Expr` and watching **which terminal ends up attached right behind `B`.**

<pre class="lrbox">
   <span class="nt">Expr</span>  ⇒*  …  <span class="nt">B</span>  <span class="setm">a</span>  …       →  <span class="setm">a</span> came right after <span class="nt">B</span>   ⇒   <span class="setm">a</span> ∈ FOLLOW(<span class="nt">B</span>)
</pre>

Written tightly in symbols (`S` = start symbol, `T` = the set of terminals):

<pre class="lrbox">
   FOLLOW(<span class="nt">B</span>) = { <span class="setm">a</span> ∈ T | <span class="nt">S</span> ⇒* … <span class="nt">B</span> <span class="setm">a</span> … }   ∪   ( { $ }  if  <span class="nt">S</span> ⇒* … <span class="nt">B</span> )
</pre>

> 📎 One thing that's crucially different from FIRST.\
> With FIRST you only had to look at that **one symbol**, but for FOLLOW you have to **scan *everywhere `B`
> is used* across the whole grammar.** What comes after `B` differs at every spot where `B` appears.

---

## By the definition — deriving it directly

Let's *apply the definition directly* and work out "gather the terminals that come right after `B`" by hand.

### FOLLOW(Expr) — it comes out cleanly

`Expr` is the **start symbol** — that is, the *entire input* is one big `Expr`.\
So once you've read that `Expr` all the way to the end, what comes right after?\
The **end of input**, with nothing left to read.\
And the imaginary token that marks the end of input was `$`.\
So in effect `$` comes right after `Expr` — **`$ ∈ FOLLOW(Expr)`.**

Now let's hunt across the whole grammar for what comes *right after* `Expr`.

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      right after the leading <span class="nt">Expr</span> :  <span class="setm">'+'</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     right after <span class="nt">Expr</span> :       <span class="setm">')'</span>
</pre>

What can come after `Expr` is `'+'` and `')'`, plus `$` at the very end. Gathering them all:

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
</pre>

### FOLLOW(Term) — when it sits at the end, it inherits

Let's hunt the grammar for what's *right after* `Term`.

<pre class="lrbox">
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    right after the leading <span class="nt">Term</span> :  <span class="setm">'*'</span>           →  add <span class="setm">'*'</span>
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      <span class="nt">Term</span> is at the end of the production …          →  nothing after it?
   <span class="nt">Expr</span> → <span class="nt">Term</span>               <span class="nt">Term</span> is at the end of the production …          →  again, nothing after
</pre>

`'*'` goes straight in.\
But the last two lines are curious — `Term` sits at the **very end** of the production, so there's no terminal right behind it.

So then what comes after `Term`?\
Look at `Expr → Term`: `Term` is *all* of `Expr` — that is, **`Expr` and `Term` end at the exact same spot.**

Words are abstract, so let's see it as one scene.\
Picture the fragment `( Expr )` (`Factor → '(' Expr ')'`). Here `)` comes right after `Expr`.\
But if that `Expr` is just a single `Term` (`Expr → Term`), then the same spot becomes `( Term )`.

<pre class="lrbox">
   <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span>        →   <span class="setm">)</span> after <span class="nt">Expr</span>
   <span class="setm">(</span> <span class="nt">Term</span> <span class="setm">)</span>        →   now <span class="setm">)</span> after <span class="nt">Term</span>   (same spot!)
</pre>

Look — the `)` that used to be after `Expr` is now sitting **right after `Term`.**\
Because `Term` inherited the end-spot of `Expr` as is.

> 💡 **This is the core rule of FOLLOW.**\
> When a nonterminal comes at the **very end** of a production, it **inherits the entire FOLLOW of that
> production's LHS (the left-hand nonterminal).**\
> Here it's `Expr → Term`, so — **`FOLLOW(Expr)` flows straight into `FOLLOW(Term)`.**

<pre class="lrbox">
   FOLLOW(<span class="nt">Term</span>) = { <span class="setm">'*'</span> }  ∪  FOLLOW(<span class="nt">Expr</span>)
                = { <span class="setm">'*'</span> }  ∪  { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }   =   { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

### Why it doesn't finish in one pass — the FOLLOW you'd inherit may not be settled yet

As we just saw, when `B` comes at the **very end** of a production, you need the **FOLLOW of that production's left-hand nonterminal (LHS).**\
But that LHS's FOLLOW may itself still be filling in. (Exactly the same situation as when recursion blocked us with FIRST.)\
So just *following the definition step by step* won't finish in a single pass.

That's why on the next page we'll — **tidy this process into a handful of rules** and solve it, like with FIRST, by **iterating until nothing changes.**

---

## Summary

Following the definition, FOLLOW ended up being filled in three ways.

1. The **start symbol**'s FOLLOW gets `$`. (Because it can come at the very end of a sentence.)\
   `$ ∈ FOLLOW(start symbol)`
2. If **something comes right after `B`**, the *first terminal* of that something goes into FOLLOW(B).\
   For `A → α B β`, add `FIRST(β) − ε` to `FOLLOW(B)`
3. If `B` comes at the **very end** of a production, it inherits the **FOLLOW of that production's LHS**.\
   For `A → α B`, inherit `FOLLOW(A)` into `FOLLOW(B)`

## Next — turning this process into rules

Next up is tidying these three into computation rules that work for *any grammar*, and solving them by iteration.

👉 **[FOLLOW · Computation rules](follow-rules.md)**

---

👈 Back to the basic concepts: [FIRST / FOLLOW](first-follow.md)
