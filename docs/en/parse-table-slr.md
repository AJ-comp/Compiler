# The Parse Table · SLR and Spurious Conflicts

In the last chapter we met conflicts. And at the end I let slip a remark —

> Among conflicts there are *real* ones, but also *fakes* born of filling the table in **less precisely**.

This chapter starts that **"precision"** story. And remarkably — it all branches out from **just one question**.

---

## It all starts from this one question

Conflicts almost always arise from **complete items**. If a state holds an item whose dot has gone all the way to the end, like `A → α •`, that means *"now reduce."* But —

> **"When I meet `A → α •`, on *which next symbol* should I write down the reduce?"**

That's the question.\
*How precisely* you answer it decides whether a conflict appears or not — and the names coming up, **SLR · CLR · LALR**, are really all just *different answers to this one question*.

> 🔖 **lookahead** — *the one next symbol* the parser *peeks at* while trying to make a decision right now. (That word we brushed past once in the [closure](closure-def.md) chapter.)

Alright, let's raise the precision one rung at a time.

---

## ① The coarsest answer — "on any symbol at all" (LR(0))

The simplest answer.\
If `A → α •`, then reduce *whatever the next symbol is*.

This is exactly the answer of the **LR(0) states** we built all the way up to [the canonical collection](canonical-set.md) — because those states have *no information at all about the next symbol*.

The problem is obvious. It writes reduce so recklessly that the moment a state has even a single shift item, they *clash immediately*. Most real grammars become a sea of conflicts at this stage.\
→ We need more precision.

---

## ② One rung up — "only on FOLLOW" (SLR)

A clever first step.\
Don't reduce `A → α •` just anywhere — **reduce only on the symbols that can come after A.** Those *"symbols that can come after A"* were exactly [FOLLOW(A)](follow-formula.md).

This is the method we actually used on the [how to build it](parse-table-build.md) page, and its name is **SLR**.

> 🔖 **SLR (Simple LR)** — leave the LR(0) states as they are, but write reduce **only on the symbols in FOLLOW(A)**. ("Simple" = it's the easiest one-step improvement.)

**Don't just breeze past this — let's see it with our own eyes.** As you build states for our Expr grammar, one state comes out *right after reading a Term* like this:

<pre class="lrbox">
<span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← complete. reduce Expr→Term?</span>
<span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    <span style="opacity:.65">← in progress. on '*', shift</span>
</pre>

In this state, when **the next symbol is `*`** — there's exactly one thing to decide.\
*Do I reduce the `Expr → Term •` above right now, or not?*

The two methods answer this question **differently**.

**LR(0)'s answer — "reduce no matter what."**\
A complete item (`Expr → Term •`) *doesn't look at the next symbol at all.* Whatever symbol comes, it just says "reduce."\
So even on `*` it writes "reduce by Expr→Term." But the item right below it (`Term → Term • '*' Factor`) says to *shift* on `*`.\
→ "reduce" and "read more" in one cell → **conflict!** ⚠️

**SLR's answer — "reduce only when it's a symbol that can come after `Expr`."**

But — *why* "only when it can come after"? There's logic here.

Reducing `Expr → Term •` means taking the Term you just made and **grouping it into a single `Expr` chunk and pushing it onto the stack**. After that, the parser goes on to read the next input symbol *right after that `Expr`*.

So for reducing to be OK — **that next symbol has to be one that can *genuinely* come after `Expr`.**\
What if it's a symbol that can *never* come after `Expr`? The instant you reduce, you've made the shape **"the symbol after `Expr`"** — a shape that *exists nowhere in the grammar*. And that means *"reducing here is wrong."*

And **"the symbols that can come after `Expr`"** — this was the very **definition** of [FOLLOW](follow-formula.md).

> 🔖 **`FOLLOW(Expr)`** = the set of terminals that can come right after `Expr`. *(The definition itself.)*

So SLR's rule *follows straight from the definition* — **reduce only when the next symbol is in `FOLLOW(Expr)`.**

**Now, what about `*`?** Let's *compute `FOLLOW(Expr)` directly*, by the definition.\
The way to compute FOLLOW was to gather, **for every place `Expr` appears in the grammar, whatever comes *right after* it.** Hunt down every rule where `Expr` appears on the right:

| Rule where `Expr` appears | What comes right after `Expr` |
|:--|:--:|
| `Expr → Expr '+' Term` | `+` |
| `Factor → '(' Expr ')'` | `)` |
| `Accept → Expr` (augmenting rule) | end of input `$` |

That's *all of them*. So — <code>FOLLOW(Expr) = <span class="setb">{</span><span class="setm"> + ) $ </span><span class="setb">}</span></code>.\
(<span class="setb">{ }</span> is just the symbol for a *set*; the `+` · `)` · `$` inside are the actual elements.)

And here the decisive thing shows up — **in no rule does `*` come after `Expr`.**\
It can't, really. `*` appears in the grammar in only **one place**, `Term → Term '*' Factor`. So `*` is always *right after a `Term`*, never *after an `Expr`*.

Now the logic closes:

> `*` is not in `FOLLOW(Expr)` (= `*` can't come after `Expr`).\
> So reducing `Expr → Term •` on `*` would make *"`*` after `Expr`"* — a **shape not in the grammar**.\
> Therefore SLR **does not reduce** on `*`.

With no reduce written, that cell is left with *just one shift* → **the conflict is gone.** ✅

| Method | What it does on `*` |
|:--|:--|
| **LR(0)** | reduce without looking at the next symbol + shift → ⚠️ conflict |
| **SLR** | `*` can't come after Expr, so *no reduce* → shift only → ✅ clean |

**The one-line intuition:** if you're reading `a * …` — of course you should *take in more of the multiplication*; you can't go reducing the whole thing into an `Expr` right there, can you? From the single fact that *"`*` is a symbol that can't come after `Expr`,"* SLR figured out exactly that **"don't reduce here."**

So — by checking just once *"can this symbol come after Expr,"* our Expr grammar comes out with **0** conflicts under SLR. That's exactly why the table on the [how to build it](parse-table-build.md) page was so clean.\
SLR alone handles quite a lot of grammars. But —

---

## ③ SLR's weakness — spurious conflicts

Recall again what `FOLLOW(A)` was.\
It's the set you get by combing **the entire grammar** and gathering *every* symbol that can come after A, *wherever* A shows up. **Regardless of the state you're in right now.**

And right here is the trap.

The **state** the parser is standing in right now is a *specific single spot* in the grammar.\
The symbols that can *actually* come after A at that spot are often — *not the whole of FOLLOW(A), but only part of it*.

But SLR grabs the *whole* FOLLOW(A) and writes the reduce. And so —\
it writes reduce even on a symbol that **could never come in this state**, and that clashes with a shift sitting next to it.\
A conflict born not because the grammar is *truly* ambiguous, but merely because you *wrote too broadly* — this is called a **spurious conflict**.

> 🔖 **spurious conflict** — a *fake* conflict, born not from genuine ambiguity in the grammar but from writing reduce on *a wider set of symbols than necessary*.

That's a bit abstract. **Let's see it directly with a small example.**

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">c</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">e</span> <span class="nt">A</span> <span class="setm">d</span>
<span class="nt">A</span> → <span class="setm">b</span>
<span class="nt">B</span> → <span class="setm">b</span>
</pre>

A grammar where both `A` and `B` expand to `b` — made just slightly confusing on purpose. (It's still *not ambiguous*, though. The surrounding symbols separate it cleanly enough.)

First compute `FOLLOW(A)`. `A` appears in **two places** — `a A c` (followed by `c`) and `e A d` (followed by `d`). So <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code>. (`B` appears in only one place, `a B d`, so <code>FOLLOW(B) = <span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code>.)

Now look at the state where the parser **has read `a b`**. This state holds two complete items.

<pre class="lrbox">
<span class="nt">A</span> → <span class="setm">b</span> <span class="lrdot">•</span>      <span style="opacity:.65">← complete</span>
<span class="nt">B</span> → <span class="setm">b</span> <span class="lrdot">•</span>      <span style="opacity:.65">← complete</span>
</pre>

Fill in the reduce cells with SLR:

- `A → b •` reduces on <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code>
- `B → b •` reduces on <code>FOLLOW(B) = <span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code>
- → **both reduce on `d`!** reduce/reduce conflict ⚠️

**But is this a real conflict?** Think about what follows `a b`.\
We started with `a` — so the only paths open are `a A c` or `a B d`, just those two.

- if next is `c` → the `a A c` path → reducing by `A` is right
- if next is `d` → the `a B d` path → reducing by `B` is right

In other words, **on `d` it's always `B`. There's *never* any reducing by `A` here.**

So why did SLR write the `A → b` reduce in the `d` cell?\
Because it wrote `A → b`'s reduce over the *whole* <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code>. And that `d` flowed in not from *this `a` path* but from the **faraway `e A d` rule**. In `e A d`, `d` comes after `A` — but on *the `a` path we're standing on now*, only `c` comes after `A`.

**This is exactly a spurious conflict.** Not that the grammar is ambiguous — but that *the circumstances of a completely different spot (`e A d`)* got lumped into `FOLLOW`, so a *reduce that wasn't needed at this spot* got written in.

> This is different from the *real* reduce/reduce of the [what is a conflict?](parse-table-conflict.md) chapter. There the grammar was *genuinely* ambiguous (the same `a` could be read as A or as B), but here it isn't ambiguous — SLR just saw a *phantom*. — And this is **exactly the spot the next rung up the ladder (CLR) fixes.** (We'll lay out *this very grammar* again in the CLR chapter.)

One more time, by analogy —\
`FOLLOW(A)` is like *"the list of everyone the person A has ever met in their whole life."*\
But this state right now is *"today, this room."*\
The people coming to this room today are only part of that list — yet looking at the *whole list* and saying "this person could come too, so leave a seat open," you end up *clashing pointlessly* over a seat for someone who won't even show up.

---

## So — next

This spurious conflict vanishes if you reduce only on **"the symbols that genuinely reach this exact state"** rather than on *all* of FOLLOW. That *"symbol that fits this very spot"* is the **lookahead** of the next chapter — and pinning it into the states from the very start is **CLR**, while merging them down is **LALR**. We'll climb up step by step.

But before that — let's take just one page to see what the SLR we just learned looks like as *engine code*. (The definition we put into words is baked right into the code, so seeing it once makes it even clearer.)

👉 **[The Parse Table · SLR — Implementation](parse-table-slr-impl.md)**
