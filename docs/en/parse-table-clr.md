# Parse table · CLR — How It Works

In the [SLR chapter](parse-table-slr.md) we saw SLR's weak spot — the **spurious conflict**. The root of it was that when SLR decides on a reduce, it pulls its `FOLLOW` from the *whole grammar*. So symbols that have nothing to do with *the state we're actually in* got dragged in too.

The answer is clear — **instead of a global `FOLLOW`, let's use a precise lookahead that fits *exactly this state*.** We already saw *what* that lookahead is and *how* to compute it in the [previous chapter — lookahead](parse-table-lookahead.md): it's the next symbol attached *to each item* (the LR(1) item), and its value comes from `FIRST(β t)`.

The **CLR** of this chapter is the way that uses that lookahead *most thoroughly* — fully, **right from the moment the states are built.**

---

## The most thorough answer — lookahead from the very start

We take the LR(1) items (rule + dot + lookahead) from the [previous chapter](parse-table-lookahead.md) and carry them along, in full, *from the moment* we build the states.

Then *even if the surface (the dot position) is the same, a different lookahead* splits them into different states. Because every state carries only the lookahead that *fits its context exactly* — **no spurious conflict ever arises.**

> 🔖 **CLR (Canonical LR) = LR(1)** — the way that bakes the lookahead into the states from the very start, splitting them precisely by context.

---

## Example — SLR's spurious conflict, done in CLR

Let's revisit *the very grammar and the very state* where the spurious conflict happened in the [SLR chapter](parse-table-slr.md), this time with CLR.

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">c</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">e</span> <span class="nt">A</span> <span class="setm">d</span>
<span class="nt">A</span> → <span class="setm">b</span>
<span class="nt">B</span> → <span class="setm">b</span>
</pre>

In the `a b` state, SLR writes the reduce of `A → b •` across the *whole* of <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> — and so it collided with `B → b •` on `d`. (That `d` had drifted in *all the way from `e A d`*.)

**CLR carries its lookahead *per context*.** What that means is — when it builds a state, it writes, next to each item, *"the symbols that can come after me on this particular path."* Let's walk through it slowly.

**① Once we've read `a` — which path are we on right now?**\
In the grammar, only *two* rules start with `a`.

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">c</span>      <span style="opacity:.65">← a, then A, then c</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">d</span>      <span style="opacity:.65">← a, then B, then d</span>
</pre>

`S → e A d` starts with `e`, so it has nothing to do with *us, right now*, having read `a`.\
So the moment we read `a` — the only paths we can be on are **these two.**

**② So on this path, what comes after `A` and `B`?**\
Here's the *key*. **`A` appears in *two places* in the grammar — and the symbol after it differs at each place.**

- the `A` in `a A c` → followed by **`c`**
- the `A` in `e A d` → followed by **`d`**

*Lumping both of these together* gives <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> — that *blurred-together* set is exactly what SLR uses.

But we just read **`a`**. So we've stepped onto *the first of the two* — the `A` in `a A c`. (The later `e A d` starts with `e`, so it was *already eliminated*.)\
→ So the `A` at *this very spot* is followed by **`c` only.** The `d` from `e A d` is the business of *a different spot's A* — it can't come here.

(`B` appears only in the one place `a B d`, so it's always followed by `d`.)

**③ CLR writes exactly that onto the items.**\
So the two complete items in the `a b` state each carry *their own symbol*.

<pre class="lrbox">
<span class="nt">A</span> → <span class="setm">b</span> <span class="lrdot">•</span>   <span style="opacity:.65">symbols that can come next: c</span>
<span class="nt">B</span> → <span class="setm">b</span> <span class="lrdot">•</span>   <span style="opacity:.65">symbols that can come next: d</span>
</pre>

**This is exactly where CLR parts ways with SLR.** SLR wrote the *global* `FOLLOW(A)` wholesale onto `A → b •`, dragging `d` along too — but CLR writes only the symbols that *can truly come on this path*.

| `a b` state | symbols `A → b •` reduces on | symbols `B → b •` reduces on |
|:--|:--:|:--:|
| SLR (global FOLLOW) | <code><span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> |
| **CLR (per context)** | <code><span class="setb">{</span><span class="setm"> c </span><span class="setb">}</span></code> ← `d` is gone! | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> |

Now **when `d` comes in** — it's not in the symbols of `A → b •` (`c`), so we *don't reduce* there, and it matches only the symbols of `B → b •` (`d`), so we *reduce to `B`*. → just *one* thing to do in that cell → **the spurious conflict is gone.** ✅

Because CLR looks only at *"the symbols that can truly come on this path,"* there's no room for the business of *some other path* like `e A d` to butt in. That's why it's *perfectly* precise.

---

## It isn't free — the state explosion

But there's a price. States with the same surface (the same core) keep splitting whenever the lookahead differs — so **the number of states balloons (explodes).** It's fine for small grammars, but for large grammars this explosion is quite a burden.

---

## Next

CLR is *perfect, but expensive.* The practical form that gets rid of this explosion while keeping almost all of the precision is **LALR** — *coming right up.*

But first — how did *our engine* implement CLR? (To be honest, it's a little short.)

👉 **[Parse table · CLR — Implementation](parse-table-clr-impl.md)**
