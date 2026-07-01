# Parse table · LALR — How It Works

As we saw in the [CLR chapter](parse-table-clr.md) — CLR is *perfectly precise*, but the **state explosion** is expensive.\
**LALR** reconciles the two into a *practical form* — precision at CLR grade, state count at LR(0) grade.

---

## LALR = merging CLR

The idea in one line: **keep CLR's precise lookahead, but *merge the states with the same surface (core) back into one* so the state count shrinks down to LR(0).**

(In fact, the LR(0) states we built in the [canonical collection](canonical-set.md) are exactly those "merged" cores. So the engine doesn't build CLR wholesale — it *propagates* lookahead *directly* onto the LR(0) states and gets the same result efficiently. That code is in [Implementation](parse-table-lalr-impl.md).)

> 🔖 **LALR (Look-Ahead LR)** — the way that *merges the CLR states with the same surface (core)*, delivering LR(1)-grade precision at the LR(0) state count.

### Seeing the merge with your own eyes — real states and a table

Let's *actually build the states* for a small grammar.

<pre class="lrbox">
1:  <span class="nt">S</span> → <span class="setm">b</span> <span class="nt">A</span> <span class="setm">x</span>
2:  <span class="nt">S</span> → <span class="setm">d</span> <span class="nt">A</span> <span class="setm">y</span>
3:  <span class="nt">A</span> → <span class="setm">c</span>
</pre>

`A → c` is a rule that finishes (completes) when you read `c`. But there are *two* paths that reach this rule — the one through `b`, and the one through `d`.

<div class="ex-card">

**① `CLR` — the state splits in two**

**Build it with CLR — and the `A → c •` state splits into *two*.** (Because the lookahead differs per path.)

<pre class="lrbox">
state 5a :  <span class="nt">A</span> → <span class="setm">c</span> <span class="lrdot">•</span>   lookahead { <span class="setm">x</span> }    <span style="opacity:.65">arrived via b c — the A in b A x is followed by x</span>
state 5b :  <span class="nt">A</span> → <span class="setm">c</span> <span class="lrdot">•</span>   lookahead { <span class="setm">y</span> }    <span style="opacity:.65">arrived via d c — the A in d A y is followed by y</span>
</pre>

So the CLR states come to **10 in all** — `0, 1, 2, 3, 4,` **`5a, 5b`** `, 6, 7, 8`.

</div>

<div class="ex-card">

**② `LALR` — same core, so we merge**

**LALR — `5a` and `5b` have the exact same item `A → c •` (the core is the same).** So we *merge them into one state* and take the union of the lookaheads.

<pre class="lrbox">
state 5 :  <span class="nt">A</span> → <span class="setm">c</span> <span class="lrdot">•</span>   lookahead { <span class="setm">x</span> , <span class="setm">y</span> }
</pre>

`5a` and `5b` disappear, and in their place **a single state 5.** That's **10 → 9** in all.

</div>

<div class="ex-card">

**③ `parse table` — the merge as it lands in the table**

**As a result, the actual LALR parse table comes out like this.**\
(`sN` = shift to state N, `rN` = reduce by rule N, `acc` = accept, blank = error.)

| State | `b` | `d` | `c` | `x` | `y` | `$` | **S** | **A** |
|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| **0** | s2 | s3 | | | | | 1 | |
| **1** | | | | | | acc | | |
| **2** | | | s**5** | | | | | 4 |
| **3** | | | s**5** | | | | | 6 |
| **4** | | | | s7 | | | | |
| **5** | | | | **r3** | **r3** | | | |
| **6** | | | | | s8 | | | |
| **7** | | | | | | r1 | | |
| **8** | | | | | | r2 | | |

There are two spots where the merge *actually lands* in the table.

1. **State 2 and state 3 both go to *state 5* on `c`** (`s5`). Under CLR this is where 2 would go to `5a` and 3 to `5b` — *two different* states.
2. **The single row of state 5** has `r3` (= reduce by `A→c`) on *both* `x` and `y`. Under CLR this would have been *two rows* — `5a` filling only the `x` cell, `5b` only the `y` cell — now folded into one row.

Both cells have a single action each (one `r3`) — so **there's no conflict.**

> *What about wrong input?* Try feeding `b c y`: `b` → state 2, `c` → state 5. There it sees `y` and reduces to `A` by `r3` (since `y` is in the lookahead `{ x , y }`). Then right away, in the next state 4 (`S → b A • x`), it's waiting for `x`, but `y` came → **error.** Wrong input still gets caught all the same — *just one cell later.*

</div>

---

(Note: in the a/b grammar of the [CLR chapter](parse-table-clr.md), the two states holding `A → b •` (`a b` and `e b`) have *different cores from each other* — so there's no partner to merge with. LALR there just uses CLR's `{ c }` as-is, and likewise no spurious conflict arises.)

→ **Precision at CLR grade, state count at LR(0) grade.** That's why yacc and bison — and **the actual working parser of our engine — are all LALR.**

---

## But — the merge *rarely* revives a conflict

Almost always, merging is harmless. But **very rarely**, the moment you merge, *a conflict that wasn't there comes alive*. (It's a little tight, but follow along just once and you've got it.)

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">b</span> <span class="nt">B</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">e</span>
<span class="nt">S</span> → <span class="setm">b</span> <span class="nt">A</span> <span class="setm">e</span>
<span class="nt">A</span> → <span class="setm">c</span>
<span class="nt">B</span> → <span class="setm">c</span>
</pre>

The state right after reading `c` appears in *two places*. The items are the same — `{ A→c•, B→c• }` — but depending on *where you came from*, the lookaheads cross over.

- **arriving via `a`**: `S → a A d`, so `A` is followed by `d`; `S → a B e`, so `B` is followed by `e`
- **arriving via `b`**: `S → b A e`, so `A` is followed by `e`; `S → b B d`, so `B` is followed by `d`

Gathered into a table:

| state reached after reading `c` | `A → c •` | `B → c •` |
|:--|:--:|:--:|
| after `a c` | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> | <code><span class="setb">{</span><span class="setm"> e </span><span class="setb">}</span></code> |
| after `b c` | <code><span class="setb">{</span><span class="setm"> e </span><span class="setb">}</span></code> | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> |

- **CLR** keeps the two *separate*. Within each state, nothing overlaps. → no conflict.
- **LALR** has the same core, so it *merges*. Then the lookaheads become a union, both becoming <code><span class="setb">{</span><span class="setm"> d e </span><span class="setb">}</span></code> → on `d` (and `e`) both A and B reduce → **reduce/reduce conflict!** Something *that wasn't there before merging* came alive.

> In other words, this grammar is the rare case that *is solvable with LR(1) (CLR) but conflicts under LALR*. But real-world grammars almost never have this kind of crossover, so LALR's merging mostly *saves states for free*.

> 📎 **A conflict newly created by merging is *reduce/reduce only*** — a *shift/reduce conflict* can never arise from merging. (It's been proven that if CLR has no conflict, the only thing LALR can add is an r/r conflict. That's why the example above was r/r too.)

> 💡 **One step further** — *"So what if we merge only the ones that don't conflict? Wouldn't that keep LR(1) precision while keeping the state count almost as low as LALR?"* — Right. That's **minimal LR(1)** (Pager, 1977) and its modern version **IELR(1)**. (GNU Bison supports it too, via `%define lr.type ielr`.) Our engine only goes up to LALR for now, so this is a *potential future improvement*.

---

## The precision ladder — at a glance

| Method | symbols reduce is written on | precision | state count | spurious conflicts |
|:--|:--|:--:|:--:|:--:|
| **LR(0)** | every terminal | lowest | few | swarming |
| **SLR** | FOLLOW(A) | medium | few | sometimes |
| **LALR** | precise per-core lookahead *(merged CLR)* | high | few (= LR(0)) | almost none |
| **CLR / LR(1)** | per-context lookahead *(splits states)* | highest | **explodes** | none |

The higher you go the more precise it gets, but on the very last rung (CLR) you pay the expensive price of *state count*.\
That's why **LALR is the spot that catches both rabbits at once** — precision and state count.

---

## Next

That's it for how it works. So how does *our engine* do this "merging (= propagation)" in code?

👉 **[Parse table · LALR — Implementation](parse-table-lalr-impl.md)**
