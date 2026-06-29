# The Parse Table · How to Build It

> 🎓 This is the **Advanced track**.\
> Over in [the canonical collection](canonical-set.md), we gathered all 12 states and every *transition* between them.\
> Now we turn that — into a **single table** the parser actually reads and acts on. That table is the **parse table**.

> 📍 **Where it lives** · `LRParsingTable.CreateParsingTable` · `…/Parsers/Collections/…`

## What a parse table is

As it reads its input, the parser has to decide *at every moment*: "what do I do right now?" The table that writes down all of those answers in advance is the
**parse table**.\
In a word — it's the **parser's instruction sheet**. For each cell it spells out: *"you're in this state (row), and the next input symbol is this (column) →
so do this action."*

The table splits into two parts.

- **ACTION** — the columns are *terminals + `$` (end of input)*. Each cell holds one of three actions.
  - **shift** — read the next terminal, push it onto the stack, and go to the next state it points to. `s5` = *"read it and go to `I₅`."*
  - **reduce** — you've read the whole right-hand side of some production, so you *fold* it back into the nonterminal on its left. `r2` = *"reduce by production 2."*
  - **accept** — the whole input matched the grammar. Done. `acc`.
- **GOTO** — the columns are *nonterminals*. This is *which state to go to after reducing a nonterminal*. (It's just the *nonterminal columns* of the [canonical collection's](canonical-set.md)
  transition table, copied over.)

> 💡 `shift` and `reduce` — where have you seen those? In the [State](lr-state.md) chapter, with *"if a terminal follows the dot, read more (shift); if the dot is at
> the end, reduce."*\
> Writing that decision down **in advance**, as a table, for *every state × every input* — that's the parse table.

## The rules for filling it in

Filling in the table is just a matter of copying over the *items of the states* and the *transitions* we built in [the canonical collection](canonical-set.md).
There are only **four** rules.

First we number the productions. (reduce needs to point at *which one to reduce by*.)

<pre class="lrbox">
   1:  <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   2:  <span class="nt">Expr</span>   → <span class="nt">Term</span>
   3:  <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   4:  <span class="nt">Term</span>   → <span class="nt">Factor</span>
   5:  <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   6:  <span class="nt">Factor</span> → <span class="setm">id</span>
</pre>

Now, for each state `Iᵢ` —

① **shift** — if there's an item with a *terminal* `a` right after the dot and `GOTO(Iᵢ, a) = Iⱼ`, then → `ACTION[Iᵢ][a] = sⱼ`\
② **goto** — if a *nonterminal* `A` follows the dot so that `GOTO(Iᵢ, A) = Iⱼ`, then → `GOTO[Iᵢ][A] = j`\
③ **reduce** — if there's a *complete item* `A → α •` (dot at the end), then → for **every terminal** `x` **in the [FOLLOW](follow-formula.md) of `A`**, set `ACTION[Iᵢ][x] = rN` (N = that production's number)\
④ **accept** — if `Accept → Expr •` is present, then → `ACTION[Iᵢ][$] = acc`

①② are just *copying the transition table over* (terminals into ACTION, nonterminals into GOTO); ③④ come from *complete items*.

> The heart of ③ is the *"for every terminal in FOLLOW"* part — we only reduce when the terminal is *"a token that can legitimately come after we reduce by this rule."*
> That's the promise from the [State](lr-state.md) chapter: *"reduce only on FOLLOW."*\
> (This way of **deciding the reduce cells by FOLLOW** is called **SLR** — once we've met *conflicts*, we'll treat it properly in the SLR chapter.)

## Filling it in yourself

Let's apply the rules to a few of our states by hand. (Keep [the canonical collection's](canonical-set.md) states and transition table beside you.)

<div class="ex-card">

**① ACTION·GOTO for `I₀` — no complete items, so only ①②**

`I₀` has no complete items, so we only use ①②.

- Terminals after the dot: `'('` (`Factor → • '(' Expr ')'`) and `id` (`Factor → • id`). In the transition table we had
  `GOTO(I₀,'(') = I₄`, `GOTO(I₀,id) = I₅` → **`'('` = s4,  `id` = s5**
- Nonterminals after the dot: `Expr`→`I₁`, `Term`→`I₂`, `Factor`→`I₃` → **GOTO: `Expr` = 1, `Term` = 2, `Factor` = 3**

</div>

<div class="ex-card">

**② reduce for `I₂` — r2 in the FOLLOW cells of the complete item**

`I₂` has the complete item `Expr → Term •` (rule 2).

- FOLLOW of `Expr` is `{ $, '+', ')' }` → so those three cells get **`'+'` = r2,  `')'` = r2,  `$` = r2**
- The `Term → Term • '*' Factor` sitting alongside it also gives a shift: `'*'`→`I₇` → **`'*'` = s7**

</div>

<div class="ex-card">

**③ accept for `I₁` — the `$` cell accepts**

`I₁` has `Accept → Expr •`, so → **`$` = acc**.\
(And from `Expr → Expr • '+' Term`, `'+'`→`I₆` → **`'+'` = s6**.)

</div>

Fill in all 12 states this way — and the table is complete.

## The finished SLR parse table

The left six columns (`id` ~ `$`) are **ACTION**, the right three columns (`Expr`·`Term`·`Factor`) are **GOTO**.

| State | `id` | `'+'` | `'*'` | `'('` | `')'` | `$` | `Expr` | `Term` | `Factor` |
|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| `I₀`  | s5 |    |    | s4 |     |     | 1 | 2 | 3 |
| `I₁`  |    | s6 |    |    |     | **acc** |   |   |   |
| `I₂`  |    | r2 | s7 |    | r2  | r2  |   |   |   |
| `I₃`  |    | r4 | r4 |    | r4  | r4  |   |   |   |
| `I₄`  | s5 |    |    | s4 |     |     | 8 | 2 | 3 |
| `I₅`  |    | r6 | r6 |    | r6  | r6  |   |   |   |
| `I₆`  | s5 |    |    | s4 |     |     |   | 9 | 3 |
| `I₇`  | s5 |    |    | s4 |     |     |   |   | 10 |
| `I₈`  |    | s6 |    |    | s11 |     |   |   |   |
| `I₉`  |    | r1 | s7 |    | r1  | r1  |   |   |   |
| `I₁₀` |    | r3 | r3 |    | r3  | r3  |   |   |   |
| `I₁₁` |    | r5 | r5 |    | r5  | r5  |   |   |   |

**How to read the table** — `sⱼ` = read and go to `Iⱼ` (shift), `rN` = reduce by rule N, `acc` = accept,
the number in a GOTO cell = the state to go to *after reducing a nonterminal*.\
A **blank cell** means *that input is an error here*.

## Next

The table is built. But two things are still hanging.

- What if **two actions land in one cell**? (That's the **conflict** seed we planted back in the [State](lr-state.md) chapter.)
- This way of deciding reduce cells by FOLLOW (**SLR**) — what are its limits, and how can we make it more precise?

👉 **[The Parse Table · What Is a Conflict?](parse-table-conflict.md)**

---

👈 Previously: [the canonical collection](canonical-set.md)
