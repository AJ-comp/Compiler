# FIRST · Definition & Derivation

> 🎓 This is the **Advanced track · Theory**. It's good to grab the *concept* first from the [Basics-track FIRST/FOLLOW](first-follow.md)
> and then come here. On this page we'll look at **exactly what FIRST is (the definition)** and the **process of
> deriving it by hand, straight from that definition.**
>
> Don't let it weigh on you — we'll go slowly.

> 📍 **Where it lives** · engine `FirstFollowAnalyzer` · module `Janglim.FrontEnd` — **Layer 2** (the lower layer,
> *below* the parse table)

The example grammar we keep using.

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
</pre>

The answer we worked out by hand in the basics was `FIRST(Expr) = FIRST(Term) = FIRST(Factor) = { '(', id }`.\
But before that — let's pin down clearly **what FIRST *exactly* is** first.\
(Skip this, and the computation rules that follow become a spell you just memorize.)

---

## Definition — What FIRST is

Let me nail it down in one line first.

> **FIRST(X)** = the set of **terminals that can appear first when you derive** the symbol `X`.

There are two key words — **derive** and **the terminal that appears first** (the terminal that comes at the *very front*).\
Let's look at them one at a time.

### "derivation" — expanding via the rules

We call one step of **substituting** a nonterminal with the right-hand side of its production a *derivation*, and write it with the arrow `⇒`.

Let me expand `Expr` one step at a time.

<pre class="lrbox">
   <span class="nt">Expr</span>  ⇒  <span class="nt">Term</span>  ⇒  <span class="nt">Factor</span>  ⇒  <span class="setm">id</span>
        (Expr:Term)  (Term:Factor)  (Factor:id)
</pre>

By applying rules three times, we finally reached a string made of **terminals only**, `id`.\
Expanding like this in *several steps* is written `⇒*` → `Expr ⇒* id` ("Expr derives id").

### So FIRST is

Expanding `X` *every which way* all the way down, and gathering up **all the terminals that can appear first** (= the terminal that comes at the very front).

Seeing it directly with `Expr`:

<pre class="lrbox">
   <span class="nt">Expr</span> ⇒* <span class="setm">id</span> …             →  front is <span class="setm">id</span>   ⇒   <span class="setm">id</span> ∈ FIRST(<span class="nt">Expr</span>)
   <span class="nt">Expr</span> ⇒* <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span> …       →  front is <span class="setm">(</span>    ⇒   <span class="setm">(</span> ∈ FIRST(<span class="nt">Expr</span>)
</pre>

`Expr` can make countless strings (`id`, `id + id`, `( id ) * id`, …), but the **terminals that can come at the
front** end up being just two, `id` or `(`. So:

<pre class="lrbox">
   FIRST(<span class="nt">Expr</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

Written more firmly in symbols, it's this (`T` = the set of terminals):

<pre class="lrbox">
   FIRST(<span class="nt">X</span>) = { <span class="setm">a</span> ∈ T | <span class="nt">X</span> ⇒* <span class="setm">a</span> … (derives a string starting with <span class="setm">a</span>) }
</pre>

> 📎 **Just one more thing about ε (the empty string).** If `X` can derive even *nothing at all*
> (`X ⇒* ε`), **we put ε into FIRST(X) too.** It's the mark for "X can disappear entirely." (Our
> example has no such nonterminal, so ε doesn't show up — but it's definitely part of the definition.)

To sum up — whether it's a terminal, a nonterminal, or a *sequence of several symbols*, FIRST is **"the set of terminals (plus ε if needed) that can appear first when you derive it."**\
That's the whole definition.

---

## Straight from the definition — deriving it by hand

Now let's work out "deriving and gathering the front terminals" by *applying that definition directly*, by hand.\
Starting with the easy one, `Factor`.

### Factor — expands all the way without a hitch

Let's derive `Factor`'s two **productions** (one per line, split by `|` — of the form `A → α`, where `α` is the *sequence of symbols on the right-hand side*. Details in [Single](deep-single.md)) all the way down.

<pre class="lrbox">
   <span class="nt">Factor</span> ⇒ <span class="setm">id</span>              →  front terminal :  <span class="setm">id</span>
   <span class="nt">Factor</span> ⇒ <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span>        →  front terminal :  <span class="setm">(</span>
</pre>

The front of the strings `Factor` produces is either `id` or `(` — just those two.\
Gathering them up straight from the definition:

<pre class="lrbox">
   FIRST(<span class="nt">Factor</span>) = { <span class="setm">id</span>, <span class="setm">'('</span> }
</pre>

Easy, right?\
Inside `Factor`, it doesn't show up by itself, so the derivation ends cleanly.

### Term — as you expand, itself shows up again

`Term : Term '*' Factor | Factor`.\
Let me derive the two productions.

<pre class="lrbox">
   ① <span class="nt">Term</span> ⇒ <span class="nt">Factor</span> ⇒ … ⇒ <span class="setm">id</span> or <span class="setm">(</span>          →  front :  <span class="setm">id</span>, <span class="setm">(</span>
   ② <span class="nt">Term</span> ⇒ <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>                  →  front is again <span class="nt">Term</span> ?!
</pre>

② is the odd one.\
The front is *itself*, `Term`, so to know the front terminal you have to expand that `Term` **again**.

<pre class="lrbox">
   <span class="nt">Term</span> ⇒ <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
        ⇒ <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
        ⇒ …                              ←  expand and expand, the front is still <span class="nt">Term</span>, no end in sight
</pre>

But eventually the front `Term` too settles down into the `Factor` production, and then the front terminal becomes `id` or `(` again.
So:

<pre class="lrbox">
   FIRST(<span class="nt">Term</span>) = { <span class="setm">id</span>, <span class="setm">'('</span> }
</pre>

`Expr` (`Expr : Expr '+' Term | Term`) has the same shape, so `FIRST(Expr) = { id, '(' }`.

### Here's the snag — we can't derive "all the way"

As we saw with `Term`, **when it bites its own tail (recursion), the derivation can grow infinitely long.**\
The definition is clear ("the terminal that appears first when you derive"), but actually expanding that derivation *all the way to the end, one by one* is awkward both by hand and by computer.

## Next — on to the computation rules

So on the next page we'll move to a **computation rule** that pulls out the same FIRST **without expanding the
derivation directly**.\
(And recursion gets handled gracefully too.)

👉 **[FIRST · Computation Rules](first-rules.md)**

---

👈 Back to the basic concept: [FIRST / FOLLOW](first-follow.md)
