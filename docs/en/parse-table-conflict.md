# The Parse Table · What Is a Conflict?

On the last page we finished a **parse table** for the Expr grammar. Let's bring that table back to mind for a moment.\
Each and every cell held **exactly one** action — `s5`, `r2`, `goto 3`, `acc` … So at every moment the parser just had to look up a single cell and follow it, *without hesitating*.

But at the end of the "how to build it" page, I slipped in one question.

> **"What if *two* actions land in one cell?"**

That's exactly what this page is about. Let's take it slowly.

---

## First — why did the table roll along so smoothly?

Boil down what a parse table does to one line, and it's this.

> **current state + next symbol → what action to take**, written down in each individual cell.

At every moment the parser uses the *current state* and the *next symbol* to pick out one cell in the table, and moves exactly as that cell says.\
Because each cell holds **one** answer, the parser never once hesitates. We call this property of *rolling along effortlessly, without any deliberation* **deterministic**.

> 🔖 **deterministic** — the action to take at every moment is fixed to *exactly one*, so the parser never gets confused.

Now — what happens when this promise of **"one per cell"** breaks?

---

## A conflict — two actions in one cell

With some grammars, as you fill in the table, **two actions land in a single cell**.\
The parser that arrives at that cell stalls right there: *"uh… which of the two am I supposed to do?"*

A situation where **a single cell would hold two or more actions, so the parser can't decide what to do** is called a **conflict**.

> 🔖 **conflict** — a single cell of the parse table holds two or more actions, leaving the parser unable to decide.

Our Expr grammar was lucky — really, it was just *well designed* — and had no conflicts at all. That's why the table came out so clean.\
But many grammars in the world aren't like that. Let's look at **how conflicts arise**, going through the two kinds one at a time.

---

## Kind ① — shift / reduce conflict

To show off a conflict *on purpose*, let me bring in a tiny grammar instead of our Expr.

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">S</span> <span class="setm">'+'</span> <span class="nt">S</span>
<span class="nt">S</span> → <span class="setm">a</span>
</pre>

"S is *S plus S*, or just *a*" — a teeny-tiny grammar for addition.

As you build the [states](lr-state.md) for this grammar, one particular state ends up containing **these two items together**. (If items and states are hazy, recall the [LR item](lr-item.md) and [State](lr-state.md) chapters.)

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">S</span> <span class="setm">'+'</span> <span class="nt">S</span> <span class="lrdot">•</span>      <span style="opacity:.65">← dot at the very end. A <b>complete</b> item → on '+', reduce (fold S+S into S)</span>
<span class="nt">S</span> → <span class="nt">S</span> <span class="lrdot">•</span> <span class="setm">'+'</span> <span class="nt">S</span>      <span style="opacity:.65">← '+' after the dot. An <b>in-progress</b> item → on '+', shift (read the '+' and move on)</span>
</pre>

Same state, same next symbol `+`, yet —\
the **top item shouts "reduce!"** while the **bottom item shouts "shift!"** — they call for *different actions*.

So the `+` cell of this state ends up holding **two** actions.

| State | … | `+` | … |
|:--|:--:|:--:|:--:|
| (this state) | | **shift / reduce** ⚠️ | |

Because one side is **shift** and the other is **reduce**, we call this clash exactly what it is — a **shift/reduce conflict**.

> 💡 **Why does it happen?** — Picture `a + a + a`. The parser, having read up to the middle, has to decide: *reduce the left `a + a` into a single chunk first* (reduce), *or hold off, take in another `+`, and decide later* (shift). But this grammar says *both* `(a+a)+a` *and* `a+(a+a)` are valid. With two answers, the parser can't pick one. A grammar that lets *one sentence be read two ways* is called an **ambiguous grammar**. (Note: *a conflict is not the same as ambiguity* — there are cases where a conflict shows up even though the grammar isn't ambiguous, and many of those are **spurious conflicts** that vanish once you fill the table in more precisely. That's the very next story.)

---

## Kind ② — reduce / reduce conflict

This time, another small grammar.

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span>
<span class="nt">S</span> → <span class="nt">B</span>
<span class="nt">A</span> → <span class="setm">a</span>
<span class="nt">B</span> → <span class="setm">a</span>
</pre>

It's a grammar where "a single `a` can become either *A* or *B*."

As you build the table, the state right after reading `a` ends up with these two items together.

<pre class="lrbox">
<span class="nt">A</span> → <span class="setm">a</span> <span class="lrdot">•</span>      <span style="opacity:.65">← complete. reduce <b>A→a</b></span>
<span class="nt">B</span> → <span class="setm">a</span> <span class="lrdot">•</span>      <span style="opacity:.65">← complete. reduce <b>B→a</b></span>
</pre>

This time both say **reduce**. But — **by which rule** should we fold? By `A→a`? By `B→a`?\
Both say "reduce," but they *fold into different things*, so they clash. This is called a **reduce/reduce conflict**.

Putting the two kinds side by side:

| Kind of conflict | What clashes in one cell | The parser's dilemma |
|:--|:--|:--|
| **shift / reduce** | one shift + one reduce | "read more, or reduce now?" |
| **reduce / reduce** | two reduces (different rules) | "which rule do I reduce by?" |

---

## The seed planted in the [State] chapter

Remember? In the [State](lr-state.md) chapter we said a single state can hold a **mix** of **complete items** (dot at the end, *wanting to reduce*) and **in-progress items** (a symbol still left after the dot, *wanting to read*), and we called that the 🌱 **conflict seed**.

Now that seed has grown and borne fruit.

- When, in one state, a **complete item and an in-progress item** clash over the *same symbol* → **shift/reduce conflict**
- When, in one state, **two complete items** clash → **reduce/reduce conflict**

That story we put off back when learning about states — "this turns into a conflict later" — we've now paid it back in full.

---

## So — what happens when a conflict occurs?

If a grammar has a conflict, that table breaks its promise of **"one answer per cell."**\
= That grammar can't be parsed deterministically *as is*. The parser just stops dead at that cell.

There are two ways out.

1. **Fix the grammar** — rewrite it to clear out the ambiguity so the conflict never arises at all.\
   (Our Expr grammar is exactly such a *well-tuned* grammar. That's why its table was spotless.)
2. **Fill the table in more cleverly** — surprisingly, some of what *looks like* a conflict is **not a real conflict** but a **spurious conflict**, born of filling the table in *less precisely*. Fill it in more precisely and it quietly disappears.

The journey toward that **"fill it in more precisely"** begins on the very next page (SLR).\
We'll start by seeing *why* the way we set reduce cells by **FOLLOW** on the "how to build it" page (its name was **SLR**, remember?) *creates spurious conflicts* — and from there we'll climb the precision ladder one rung at a time through **lookahead → CLR → LALR**, watching *how* those spurious conflicts get eliminated.

---

## Next

- A conflict = two actions in one cell. Two kinds: **shift/reduce** and **reduce/reduce**.
- Some conflicts are *real*, and some are a **phantom** caused by filling the table in less precisely — the heart of the next chapter.

👉 **[The Parse Table · SLR and Spurious Conflicts](parse-table-slr.md)**
