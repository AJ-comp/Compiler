# FIRST · FOLLOW — Worked Examples (try them yourself)

> 🎓 This is the **Advanced track · Theory**.\
> It's a **practice pad** for *applying the [FIRST computation rules](first-rules.md) · [FOLLOW computation rules](follow-rules.md) directly to several grammars.*

The `Expr` grammar we've used throughout the main text has no ε (no disappearing nonterminal), so we never saw some of the rules *actually run*. So here, with grammars **that include ε**, let's work through them *from easy to gradually tougher*. It's best to follow along by hand together.

---

## The rules at a glance (cheat sheet)

Before solving — the formulas we'll use are laid out below.

- **[FIRST formula summary](first-rules.md)**
- **[FOLLOW formula summary](follow-rules.md)**

> 📎 In the example grammars below — *lowercase* `a b c d x y …` = **terminals**, *uppercase* `A B S E T …` = **nonterminals**, `ε` = *disappears (nothing)*.

---

## Example 1 — ε appears for the first time (an optional symbol)

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="setm">b</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
</pre>

`A` can be `a` and can also be *nothing at all (ε)* — an "optional" symbol.

### FIRST

`S → A b` is a symbol sequence, so we start with the ⊕ formula.

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = FIRST(<span class="nt">A</span>) ⊕ FIRST(<span class="setm">b</span>)
</pre>

Filling in the pieces —

<pre class="lrbox">
   FIRST(<span class="nt">A</span>) = { <span class="setm">a</span>, ε }     ← A → a | ε so nullable
   FIRST(<span class="setm">b</span>) = { <span class="setm">b</span> }
</pre>

Since `FIRST(A)` has ε — drop ε and ⊕ on to the next slot, `b`:

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = { <span class="setm">a</span>, ε } ⊕ { <span class="setm">b</span> }
            = ( { <span class="setm">a</span>, ε } − ε ) ∪ { <span class="setm">b</span> }
            = { <span class="setm">a</span>, <span class="setm">b</span> }
</pre>

→ **`FIRST(S) = { a, b }`** (S can't produce ε, so it's not nullable).

### FOLLOW

<pre class="lrbox">
   ① start symbol <span class="nt">S</span>   →  FOLLOW(<span class="nt">S</span>) = { $ }
   ② in <span class="nt">S</span> → <span class="nt">A</span> <span class="setm">b</span>, after A β = "<span class="setm">b</span>"  →  FIRST(<span class="setm">b</span>) − ε = { <span class="setm">b</span> }  →  FOLLOW(<span class="nt">A</span>) ⊇ { <span class="setm">b</span> }
</pre>

`A` is not at the *very end* of any production (`b` is always behind it) → rule ③ doesn't apply.

<pre class="lrbox">
   FOLLOW(<span class="nt">S</span>) = { $ }
   FOLLOW(<span class="nt">A</span>) = { <span class="setm">b</span> }
</pre>

> 🔖 **What's new in this example** — ε goes into `FIRST(A)`, and when working out `FIRST(S)`, ⊕ moves *"on to the next slot because the front can disappear."* (Something we never saw in the Expr grammar.)

---

## Example 2 — nullables in a row (two ε in a row)

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span> <span class="setm">c</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

This time both `A` and `B` can disappear. Let's see whether ⊕ skips *two slots* when working out `S`'s FIRST.

### FIRST

`S → A B c` is a symbol sequence, so we start with the ⊕ formula.

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = FIRST(<span class="nt">A</span>) ⊕ FIRST(<span class="nt">B</span>) ⊕ FIRST(<span class="setm">c</span>)
</pre>

Filling in the pieces —

<pre class="lrbox">
   FIRST(<span class="nt">A</span>) = { <span class="setm">a</span>, ε }     ← nullable
   FIRST(<span class="nt">B</span>) = { <span class="setm">b</span>, ε }     ← nullable
   FIRST(<span class="setm">c</span>) = { <span class="setm">c</span> }
</pre>

⊕ from the left — if there's ε, drop that slot and go on to the next:

- `FIRST(A)` has ε → take `a` and **move to the next slot**
- `FIRST(B)` has ε → take `b` and **move to the next slot again**
- `FIRST(c)` has no ε → take `c` and **stop**

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = { <span class="setm">a</span>, ε } ⊕ { <span class="setm">b</span>, ε } ⊕ { <span class="setm">c</span> }
            = { <span class="setm">a</span> } ∪ { <span class="setm">b</span> } ∪ { <span class="setm">c</span> } = { <span class="setm">a</span>, <span class="setm">b</span>, <span class="setm">c</span> }
</pre>

→ **`FIRST(S) = { a, b, c }`** (the trailing `c` is a terminal, so S is not nullable).

### FOLLOW

<pre class="lrbox">
   ① start symbol <span class="nt">S</span>   →  FOLLOW(<span class="nt">S</span>) = { $ }
</pre>

**② — look at after `A`, after `B`.**

<pre class="lrbox">
   <span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span> <span class="setm">c</span>   :  after A β = "<span class="nt">B</span> <span class="setm">c</span>"   →  FIRST(<span class="nt">B</span> <span class="setm">c</span>) − ε
   <span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span> <span class="setm">c</span>   :  after B β = "<span class="setm">c</span>"     →  FIRST(<span class="setm">c</span>)   − ε = { <span class="setm">c</span> }
</pre>

`FIRST(B c)` with ⊕: `FIRST(B)={b,ε}` has ε → `b` + next slot `FIRST(c)={c}` → `{ b, c }` (no ε, stop). No ε mixed in, so as is:

<pre class="lrbox">
   FOLLOW(<span class="nt">A</span>) ⊇ { <span class="setm">b</span>, <span class="setm">c</span> }
   FOLLOW(<span class="nt">B</span>) ⊇ { <span class="setm">c</span> }
</pre>

Both `A` and `B` are not at the *very end*, and what's behind them (`B c`, `c`) doesn't all disappear into ε either (terminal `c` at the end) → rule ③ doesn't apply.

<pre class="lrbox">
   FOLLOW(<span class="nt">S</span>) = { $ }
   FOLLOW(<span class="nt">A</span>) = { <span class="setm">b</span>, <span class="setm">c</span> }
   FOLLOW(<span class="nt">B</span>) = { <span class="setm">c</span> }
</pre>

> 🔖 **What's new in this example** — ⊕ skipping over *two nullable slots* in a row (`FIRST(S)`), and FOLLOW ② using *not a single terminal but `FIRST(β)` (a bundle of several symbols)* as its raw material.

---

## Example 3 — the really tough one (recursion + ε + inheritance) ★

This is the cousin of the main text's `Expr` grammar, solved **with ε** instead of left recursion. It's a staple grammar in compiler textbooks — and *every* rule runs at least once.

<pre class="lrbox">
<span class="nt">E</span>  → <span class="nt">T</span> <span class="nt">E'</span>
<span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> | ε
<span class="nt">T</span>  → <span class="nt">F</span> <span class="nt">T'</span>
<span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span> | ε
<span class="nt">F</span>  → <span class="setm">'('</span> <span class="nt">E</span> <span class="setm">')'</span> | <span class="setm">id</span>
</pre>

`E'` and `T'` are *tails* — they can either get more attached (`+ T E'`) or end (ε). So both are nullable.

### FIRST

Going up from the easy ones (the bottom end).

<pre class="lrbox">
   FIRST(<span class="nt">F</span>)  : <span class="nt">F</span> → <span class="setm">'('</span> <span class="nt">E</span> <span class="setm">')'</span>  → { <span class="setm">(</span> }
               <span class="nt">F</span> → <span class="setm">id</span>         → { <span class="setm">id</span> }              →  FIRST(<span class="nt">F</span>)  = { <span class="setm">(</span>, <span class="setm">id</span> }

   FIRST(<span class="nt">T'</span>) : <span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span>  → { <span class="setm">*</span> }
               <span class="nt">T'</span> → ε         → { ε }               →  FIRST(<span class="nt">T'</span>) = { <span class="setm">*</span>, ε }

   FIRST(<span class="nt">T</span>)  : <span class="nt">T</span> → <span class="nt">F</span> <span class="nt">T'</span>  =  FIRST(<span class="nt">F</span>) ⊕ FIRST(<span class="nt">T'</span>)
                         =  { <span class="setm">(</span>, <span class="setm">id</span> } (no ε, stop)  →  FIRST(<span class="nt">T</span>)  = { <span class="setm">(</span>, <span class="setm">id</span> }

   FIRST(<span class="nt">E'</span>) : <span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> → { <span class="setm">+</span> }
               <span class="nt">E'</span> → ε        → { ε }                →  FIRST(<span class="nt">E'</span>) = { <span class="setm">+</span>, ε }

   FIRST(<span class="nt">E</span>)  : <span class="nt">E</span> → <span class="nt">T</span> <span class="nt">E'</span>  =  FIRST(<span class="nt">T</span>) ⊕ FIRST(<span class="nt">E'</span>)
                         =  { <span class="setm">(</span>, <span class="setm">id</span> } (no ε, stop)  →  FIRST(<span class="nt">E</span>)  = { <span class="setm">(</span>, <span class="setm">id</span> }
</pre>

> The FIRST of `E` · `T` · `F` is the same, `{ (, id }` — exactly the same answer as the main text's `Expr` grammar. We only changed the shape; it's the *same language*.

### FOLLOW

**Initial values — from ①②.**

<pre class="lrbox">
   ① start symbol <span class="nt">E</span>                →  FOLLOW(<span class="nt">E</span>) ⊇ { $ }
   ② <span class="nt">F</span> → <span class="setm">'('</span> <span class="nt">E</span> <span class="setm">')'</span> : after E ")"   →  FOLLOW(<span class="nt">E</span>) ⊇ FIRST("<span class="setm">)</span>") = { <span class="setm">)</span> }
</pre>

Now let's find the spots that need **inheritance (③)**. Because of the *nullable tails*, ③ kicks in all over the place.

<pre class="lrbox">
   <span class="nt">E</span>  → <span class="nt">T</span> <span class="nt">E'</span>   : E' is at the very end          →  FOLLOW(<span class="nt">E'</span>) ⊇ FOLLOW(<span class="nt">E</span>)
   <span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> : E' is at the very end         →  FOLLOW(<span class="nt">E'</span>) ⊇ FOLLOW(<span class="nt">E'</span>)   (itself, nothing new)
   <span class="nt">E</span>  → <span class="nt">T</span> <span class="nt">E'</span>   : after T β = "<span class="nt">E'</span>", but E' is nullable!
                   → ② FIRST(<span class="nt">E'</span>) − ε = { <span class="setm">+</span> }   and  ③ FOLLOW(<span class="nt">T</span>) ⊇ FOLLOW(<span class="nt">E</span>)
   <span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> : after T "<span class="nt">E'</span>" — same as above  →  { <span class="setm">+</span> },  FOLLOW(<span class="nt">T</span>) ⊇ FOLLOW(<span class="nt">E'</span>)
   <span class="nt">T</span>  → <span class="nt">F</span> <span class="nt">T'</span>   : T' is at the very end          →  FOLLOW(<span class="nt">T'</span>) ⊇ FOLLOW(<span class="nt">T</span>)
   <span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span> : T' is at the very end         →  FOLLOW(<span class="nt">T'</span>) ⊇ FOLLOW(<span class="nt">T'</span>)   (itself)
   <span class="nt">T</span>  → <span class="nt">F</span> <span class="nt">T'</span>   : after F β = "<span class="nt">T'</span>", T' is nullable too!
                   → ② FIRST(<span class="nt">T'</span>) − ε = { <span class="setm">*</span> }   and  ③ FOLLOW(<span class="nt">F</span>) ⊇ FOLLOW(<span class="nt">T</span>)
   <span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span> : after F "<span class="nt">T'</span>" — same as above  →  { <span class="setm">*</span> },  FOLLOW(<span class="nt">F</span>) ⊇ FOLLOW(<span class="nt">T'</span>)
</pre>

**Repeat to firm it up** (`E'` inherits from `E`, `T` from `E`, `T'`·`F` from `T`):

<pre class="lrbox">
   FOLLOW(<span class="nt">E</span>)  = { $, <span class="setm">)</span> }
   FOLLOW(<span class="nt">E'</span>) = { $, <span class="setm">)</span> }                  ← inherited from E
   FOLLOW(<span class="nt">T</span>)  = { <span class="setm">+</span> } ∪ FOLLOW(<span class="nt">E</span>)  = { <span class="setm">+</span>, $, <span class="setm">)</span> }
   FOLLOW(<span class="nt">T'</span>) = FOLLOW(<span class="nt">T</span>)          = { <span class="setm">+</span>, $, <span class="setm">)</span> }   ← inherited from T
   FOLLOW(<span class="nt">F</span>)  = { <span class="setm">*</span> } ∪ FOLLOW(<span class="nt">T</span>)  = { <span class="setm">*</span>, <span class="setm">+</span>, $, <span class="setm">)</span> }
</pre>

> 🔖 **What's new in this example** — inheritance (③) *actually* running. The key scene is especially **the `E'` after `T` being nullable, so `T` gets not only ②'s `{+}` but also `FOLLOW(E)` via ③.** ("if what's behind can disappear, it's effectively at the very end" → ③.)

---

## Try it yourself

To see whether the rules are second nature — give it a go yourself. (The answer is folded away below.)

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">B</span> <span class="setm">a</span>
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

Work out `FIRST(S)` · `FIRST(B)` · `FOLLOW(S)` · `FOLLOW(B)`.

<details>
<summary>Show answer</summary>

<pre class="lrbox">
   FIRST(<span class="nt">B</span>) = { <span class="setm">b</span>, ε }
   FIRST(<span class="nt">S</span>) = FIRST(<span class="nt">B</span>) ⊕ FIRST(<span class="setm">a</span>) = { <span class="setm">b</span> } ∪ { <span class="setm">a</span> } = { <span class="setm">a</span>, <span class="setm">b</span> }
              (B is nullable, so it carries over to a)

   FOLLOW(<span class="nt">S</span>) = { $ }                          ① start symbol
   FOLLOW(<span class="nt">B</span>) = FIRST(<span class="setm">a</span>) − ε = { <span class="setm">a</span> }           ② after B "<span class="setm">a</span>"
              (B is not at the very end, and behind it is the terminal a, so no ③)
</pre>

</details>

---

## Next

With this practice, you've probably got a feel for how FIRST/FOLLOW run *for any grammar.*\
Now, using these two as *raw material*, we head to — **the parse table of an LR parser.**

👉 **[LR Parser — Building the Parse Table](lr-item.md)**

---

👈 Previous: [FOLLOW · Implementation](follow-impl.md)
