# FOLLOW · Computation rules

> 🎓 This is the **advanced track · theory**.\
> In the previous page, [FOLLOW · Definition and derivation](follow-formula.md), we grabbed the definition, and while deriving by it we hit the wall of *"when it comes at the very end, inherit the LHS's FOLLOW."*\
> This page tidies that process into a **set of three rules** and solves it by **iteration**, just like with FIRST. (For the implementation → **[FOLLOW · Implementation](follow-impl.md)**.)

> Don't try to take it all in at once.\
> We'll go **one step at a time, starting with the easy parts.**

## First — what do we fill in

FOLLOW differs from FIRST starting with *what we fill in*.\
For FIRST we computed all 8, down to the terminals, but **for FOLLOW we only compute nonterminals.**\
(There's no need to ask what comes after a terminal — deciding "which rule just finished" is the job of a nonterminal.)

So in our grammar we only need to fill in **the FOLLOW of the three**: `Expr` · `Term` · `Factor`.\
The tools for filling them in are a set of three rules — the very same three we ran into while deriving earlier.

- **Rule ①** — the start symbol gets `$`
- **Rule ②** — the FIRST of what comes right after `B`
- **Rule ③** — if `B` comes at the very end, inherit the LHS's FOLLOW

One at a time, starting with the easy one.

## Rule ① — the start symbol gets `$`

**Put `$` (the end of input) into the FOLLOW of the start symbol.**

Why is that?\
As we saw in [Definition and derivation](follow-formula.md), the start symbol is *the entire input*, so once you've read all of it to the end, what comes right after is the **end of input**.

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>) ⊇ { $ }
</pre>

## Rule ② — the FIRST of what comes right after `B`

This is the case where, in a production like `A → α B β` (`α` is whatever comes *before* `B`, `β` is whatever comes *after* `B`), **something (β)** comes after `B`.

**Then the first terminal that `β` produces can come right after `B`.** → We put `FIRST(β)` into `FOLLOW(B)`. (Except for `ε`, though.)

Why is that?\
Since `β` is attached after `B`, the *frontmost terminal* that `β` derives is exactly the terminal that comes right after `B`.\
That "frontmost terminal" is precisely [FIRST(β)](first-rules.md). (**This is the point where FOLLOW uses FIRST as an ingredient.**)

> 📎 Why do we leave out `ε`?\
> Having `ε` in `FIRST(β)` means *"β might disappear entirely."*\
> `ε` is not a terminal but 'disappearance,' so we don't put it into FOLLOW, which is a set of terminals.\
> Instead, if β disappears, `B` effectively becomes the very end — and that's handled by **Rule ③**.

In our grammar, `β` always starts with a terminal, so it's simple.

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      after the leading <span class="nt">Expr</span>, β = "<span class="setm">'+'</span> <span class="nt">Term</span>"  →  FIRST = <span class="setm">'+'</span>   →  FOLLOW(<span class="nt">Expr</span>) ⊇ { <span class="setm">'+'</span> }
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     after <span class="nt">Expr</span>, β = "<span class="setm">')'</span>"          →  FIRST = <span class="setm">')'</span>   →  FOLLOW(<span class="nt">Expr</span>) ⊇ { <span class="setm">')'</span> }
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    after the leading <span class="nt">Term</span>, β = "<span class="setm">'*'</span> <span class="nt">Factor</span>" →  FIRST = <span class="setm">'*'</span>   →  FOLLOW(<span class="nt">Term</span>) ⊇ { <span class="setm">'*'</span> }
</pre>

## Rule ③ — if `B` comes at the very end, inherit the LHS's FOLLOW  ★

This is **that core rule** we flagged in a callout in [Definition and derivation](follow-formula.md).

When, in a production like `A → α B`, `B` comes at the **very end**:

> **`FOLLOW(B)` inherits the entire FOLLOW of `A`, the LHS of that production.** → `FOLLOW(B) ⊇ FOLLOW(A)`.

Why is that?\
Since `B` occupies the end slot of `A`, *what can come after `A`* is exactly *what can come after `B`*.\
(It's that scene from `( Expr )` → `( Term )` where `)` moved over behind `Term`.)

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      <span class="nt">Term</span> is at the end   →  FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)
   <span class="nt">Expr</span> → <span class="nt">Term</span>               <span class="nt">Term</span> is at the end   →  FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    <span class="nt">Factor</span> is at the end  →  FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)
   <span class="nt">Term</span> → <span class="nt">Factor</span>             <span class="nt">Factor</span> is at the end  →  FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)
</pre>

## Why it doesn't finish in one pass — iteration

Rule ③ is the tricky one.\
We need to pour `FOLLOW(Expr)` into `FOLLOW(Term)`, but that `FOLLOW(Expr)` may itself still be filling in.\
It's exactly what blocked us with recursion in FIRST — they **depend on each other**, so it doesn't resolve in one pass.

So the remedy is the same too — **fill in the initial values with Rules ①·②, then repeat Rule ③ *until nothing changes*.**

## By the definition — running it on our grammar

<div class="ex-card">

**① `Initial values` — fill in first with Rules ①·②**

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }      ← the $ from ① , the <span class="setm">'+'</span> <span class="setm">')'</span> from ②
   FOLLOW(<span class="nt">Term</span>)   = { <span class="setm">'*'</span> }              ← the <span class="setm">'*'</span> from ②
   FOLLOW(<span class="nt">Factor</span>) = { }                  ← still empty
</pre>

</div>

<div class="ex-card">

**② `Pass 1` — inheriting via Rule ③, things grew**

<pre class="lrbox">
   FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)  →  { <span class="setm">'*'</span> } ∪ { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }  =  { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }   (grew)
   FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)  →  { }    ∪ { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> } = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> } (grew)
</pre>

→ Something grew, so we go around one more time.

</div>

<div class="ex-card">

**③ `Pass 2` — when nothing grows any more, we stop**

<pre class="lrbox">
   FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)  →  no change
   FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)  →  no change
</pre>

→ Nothing grew this pass. **Stop!**

</div>

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
   FOLLOW(<span class="nt">Term</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
   FOLLOW(<span class="nt">Factor</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

Exactly the same answer we got on the [Definition and derivation page](follow-formula.md) and in [the basic track](first-follow.md). ✓

## When ε is involved — once, with a small grammar

The expr grammar above has no disappearing (nullable) nonterminal, so the *`− ε`* of rule ② and the *"if β disappears"* branch of rule ③ never ran. To see them both, here's the grammar we'll use for this section:

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

`A` and `B` can each disappear into ε.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">A</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">a</span>, ε <span class="setb">}</span>
   <span class="setf">FIRST(</span><span class="nt">B</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">b</span>, ε <span class="setb">}</span>
</pre>

Now look at `FOLLOW(A)`: in `S → A B`, `A` is followed by `β = B`.

- **rule ②** — put `FIRST(B) − ε` into `FOLLOW(A)`: `{ b, ε } − ε` = `{ b }`. ← *here the `− ε` actually does work.*
- **rule ③** — but since `B` *can disappear*, `A` too can effectively be at the very end. So it also inherits the FOLLOW of the LHS `S`: `FOLLOW(A) ⊇ FOLLOW(S)`.

`FOLLOW(S)` is the start symbol, so by rule ① it's `{ $ }`. And `B` is at the very end of `S → A B`, so by rule ③ it inherits that `FOLLOW(S)`.

<pre class="lrbox">
   FOLLOW(<span class="nt">S</span>) = <span class="setb">{</span> <span class="setm">$</span> <span class="setb">}</span>
   FOLLOW(<span class="nt">B</span>) = <span class="setb">{</span> <span class="setm">$</span> <span class="setb">}</span>              <span style="opacity:.6">B at the end → inherits FOLLOW(S)</span>
   FOLLOW(<span class="nt">A</span>) = <span class="setb">{</span> <span class="setm">b</span>, <span class="setm">$</span> <span class="setb">}</span>          <span style="opacity:.6">FIRST(B)−ε = {b}, plus B can disappear so it inherits FOLLOW(S)</span>
</pre>

If `B` *couldn't disappear*, `FOLLOW(A) = { b }` would be the end of it, but since `B` can become ε, `$` gets inherited too. Rule ②'s `− ε` and rule ③'s "inherit when it disappears" both actually run here.

## Summary

The only symbols the rules use are `A → α B β`.

<pre class="lrbox">
   <span class="nt">A</span> → α <span class="nt">B</span> β
   <span class="nt">A</span>, <span class="nt">B</span> = nonterminals
   α, β = strings of symbols <span style="opacity:.6">(the parts before/after B — terminals and nonterminals mixed, or even none at all)</span>
</pre>

1. The **start symbol** (`Expr`)'s FOLLOW gets `$`.
2. The **`FIRST(β) − ε`** of the `β` right after `B` goes into `FOLLOW(B)`. (FIRST as an ingredient!)
3. If `B` is at the **very end** (or if everything after `B` *can disappear*), **inherit the LHS (`A`)'s FOLLOW** — *repeat until nothing changes*.

Use FIRST as an ingredient, and solve inheritance by iteration — that's all there is to computing FOLLOW. 🎯

## Next — these rules in code

Let's see how these three rules and the iteration made it into the `FirstFollowAnalyzer` code.\
(From the very first line of `CalculateAllFollow` being `CalculateAllFirst`, you can see right there that it uses FIRST as an ingredient.)

👉 **[FOLLOW · Implementation](follow-impl.md)**

---

👈 Previously: [FOLLOW · Definition and derivation](follow-formula.md)
