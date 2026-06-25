# Parse Table · How to Build It

> 🎓 This is an **advanced track** chapter.\
> In [Canonical Collection](canonical-set.md) we gathered all 12 states and the *transitions* between them.\
> Now we turn that — into **a single table** the parser actually looks at and moves by. That's the **parse table**.

> 📍 **Where it lives** · `LRParsingTable.CreateParsingTable` · `…/Parsers/Collections/…`

## What a Parse Table Is

As the parser reads its input, *at every moment* it has to decide "what should I do right now?" The table that has all of those answers written down in advance is the
**parse table**.\
In a word — it's the **parser's instruction manual for behavior**. *"You're in this state right now (row), and the next input symbol is this (column) →
then do this action"* is written in each cell.

The table splits into two parts.

- **ACTION** — the columns are *terminals + `$` (end of input)*. Each cell holds one of three actions.
  - **shift** — read the next terminal, push it onto the stack, and go to the next state it points to. `s5` = *"read, then go to `I₅`"*.
  - **reduce** — you've read all of the right-hand side of some production, so *group* it into the nonterminal on its left. `r2` = *"reduce by production 2"*.
  - **accept** — the whole input matched the grammar. Done. `acc`.
- **GOTO** — the columns are *nonterminals*. It's *which state to go to after grouping one nonterminal*. (It's exactly the *nonterminal columns* of the [Canonical Collection](canonical-set.md)
  transition table.)

> 💡 `shift` and `reduce` — where have you seen these? In the [State](lr-state.md) chapter, that *"if a terminal follows the dot, read more (shift); if the dot is at the
> end, group (reduce)"*.\
> Writing that judgment down **in advance** as a table for *every state × every input* — that's the parse table.

## The Rules for Filling It In

Filling in the table is just the work of copying over the *items of the states* and the *transitions* you built in the [Canonical Collection](canonical-set.md), as-is. There are only **four** rules.

First, we number the productions. (reduce points at *"which number to group by"*.)

```
   1:  Expr   → Expr '+' Term
   2:  Expr   → Term
   3:  Term   → Term '*' Factor
   4:  Term   → Factor
   5:  Factor → '(' Expr ')'
   6:  Factor → id
```

Now, for each state `Iᵢ` —

① **shift** — if there's an item with a *terminal* `a` after the dot and `GOTO(Iᵢ, a) = Iⱼ`, then → `ACTION[Iᵢ][a] = sⱼ`\
② **goto** — if a *nonterminal* `A` follows the dot so that `GOTO(Iᵢ, A) = Iⱼ`, then → `GOTO[Iᵢ][A] = j`\
③ **reduce** — if there's a *complete item* `A → α •` (dot at the end), then → for each terminal `x` **in `A`'s [FOLLOW](follow-formula.md)**, `ACTION[Iᵢ][x] = rN` (N = that production's number)\
④ **accept** — if `Accept → Expr •` is present, then → `ACTION[Iᵢ][$] = acc`

①② are *just copying over the transition table* (terminals into ACTION, nonterminals into GOTO); ③④ come from *complete items*.

> The *"for each terminal in FOLLOW"* in ③ is the key — you only group when it's *"a token that can come right after grouping by this rule"*. That's the promise from the [State](lr-state.md) chapter, *"reduce only on FOLLOW"*.\
> (This way of **deciding reduce cells using FOLLOW** is called **SLR** — properly on the next page.)

## Filling It In Yourself

Let's apply the rules directly to a few of our states. (Keep the [Canonical Collection](canonical-set.md)'s state and transition tables next to you.)

**ACTION·GOTO for `I₀`** — `I₀` has no complete item, so we only use ①②.

- Terminals after the dot: `'('` (`Factor → • '(' Expr ')'`) and `id` (`Factor → • id`). In the transition table we had
  `GOTO(I₀,'(') = I₄`, `GOTO(I₀,id) = I₅` → **`'('` = s4,  `id` = s5**
- Nonterminals after the dot: `Expr`→`I₁`, `Term`→`I₂`, `Factor`→`I₃` → **GOTO: `Expr` = 1, `Term` = 2, `Factor` = 3**

**reduce for `I₂`** — `I₂` has the complete item `Expr → Term •` (rule 2).

- `Expr`'s FOLLOW is `{ $, '+', ')' }` → into those three cells **`'+'` = r2,  `')'` = r2,  `$` = r2**
- A shift also comes out of the `Term → Term • '*' Factor` that's there too: `'*'`→`I₇` → **`'*'` = s7**

**accept for `I₁`** — `I₁` has `Accept → Expr •`, so → **`$` = acc**.\
(And from `Expr → Expr • '+' Term`, `'+'`→`I₆` → **`'+'` = s6**.)

Fill in all 12 states this way — and the table is complete.

## The Finished SLR Parse Table

The left six columns (`id` ~ `$`) are **ACTION**, and the right three columns (`Expr`·`Term`·`Factor`) are **GOTO**.

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

**How to read the table** — `sⱼ` = read and go to `Iⱼ` (shift), `rN` = group by rule N (reduce), `acc` = accept,
the number in a GOTO cell = the state to go to *after grouping a nonterminal*.\
**An empty cell** means *that input is an error here*.

## Next

The table is all built. But two things remain.

- What if **two actions** end up in **one cell**? (That **conflict** seed we planted in the [State](lr-state.md) chapter.)
- This way of deciding reduce cells with FOLLOW (**SLR**) — how does it differ from a smarter way (**LALR**)?

👉 **Parse Table · Conflicts and SLR/LALR** *(coming soon)*

---

👈 Previously: [Canonical Collection](canonical-set.md)
