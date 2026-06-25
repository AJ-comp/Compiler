# FOLLOW · Computation Rules

> 🎓 This is the **advanced track · theory**.\
> In the previous page, [FOLLOW · Definition and Derivation](follow-formula.md), we pinned down the definition, then — while deriving by the definition — ran into the wall of *"when it comes at the very end, it inherits
> the FOLLOW of the LHS."*\
> This page organizes that process into a **set of rules**, and solves it by **iteration**, just like we did for FIRST. (For the implementation, see → **[FOLLOW · Implementation](follow-impl.md)**.)

> Don't try to take it all in at once.\
> We'll go **one step at a time, starting with the easy parts**.

## First — What Do We Fill In

FOLLOW differs from FIRST starting with *what we fill in*.\
For FIRST we computed all 8, down to the terminals, but **for FOLLOW we only compute nonterminals**.\
(There's no need to ask what comes after a terminal — deciding "which rule just finished" is the job of a nonterminal.)

So in our grammar we only need to fill in **the FOLLOW of the three**: `Expr` · `Term` · `Factor`.\
The tools for filling them in are a set of rules — the very same three we ran into while deriving earlier.

- **Rule ①** — the start symbol gets `$`
- **Rule ②** — the FIRST of what comes right after `B`
- **Rule ③** — if `B` comes at the very end, inherit the FOLLOW of the LHS

One at a time, starting with the easy one.

## Rule ① — The Start Symbol Gets `$`

**Put `$` (the end of input) into the FOLLOW of the start symbol.**

Why is that?\
As we saw in [Definition and Derivation](follow-formula.md), the start symbol is *the entire input*, so once you've read all of it to the end, what comes right after is the **end of input**.

```
   FOLLOW(Expr) ⊇ { $ }
```

## Rule ② — The FIRST of What Comes Right After `B`

This is the case where, in a production like `A → α B β`, **something (β)** comes after `B`.

**Then the first terminal that `β` produces can come right after `B`.** → We put `FIRST(β)` into `FOLLOW(B)`. (Except for `ε`, though.)

Why is that?\
Since `β` is attached after `B`, the *frontmost terminal* that `β` derives is exactly the terminal that comes right after `B`.\
That "frontmost terminal" is precisely [FIRST(β)](first-rules.md). (**This is the point where FOLLOW uses FIRST as an ingredient.**)

> 📎 Why do we leave out `ε`?\
> Having `ε` in `FIRST(β)` means *"β might disappear entirely."*\
> `ε` is not a terminal but 'disappearance,' so we don't put it into FOLLOW, which is a set of terminals.\
> Instead, if β disappears, then `B` effectively becomes the very end — and that's handled by **Rule ③**.

In our grammar, `β` always starts with a terminal, so it's simple.

```
   Expr → Expr '+' Term      after the leading Expr, β = "'+' Term"  →  FIRST = '+'   →  FOLLOW(Expr) ⊇ { '+' }
   Factor → '(' Expr ')'     after Expr, β = "')'"          →  FIRST = ')'   →  FOLLOW(Expr) ⊇ { ')' }
   Term → Term '*' Factor    after the leading Term, β = "'*' Factor" →  FIRST = '*'   →  FOLLOW(Term) ⊇ { '*' }
```

## Rule ③ — If `B` Comes at the Very End, Inherit the FOLLOW of the LHS  ★

This is **that core rule** we flagged in a callout in [Definition and Derivation](follow-formula.md).

When, in a production like `A → α B`, `B` comes at the **very end**:

> **`FOLLOW(B)` inherits the entire FOLLOW of `A`, the LHS of that production.** → `FOLLOW(B) ⊇ FOLLOW(A)`.

Why is that?\
Since `B` occupies the end slot of `A`, *what can come after `A`* is exactly *what can come after `B`*.\
(It's that scene from `( Expr )` → `( Term )` where `)` moved over behind `Term`.)

```
   Expr → Expr '+' Term      Term is at the end   →  FOLLOW(Term)   ⊇ FOLLOW(Expr)
   Expr → Term               Term is at the end   →  FOLLOW(Term)   ⊇ FOLLOW(Expr)
   Term → Term '*' Factor    Factor is at the end  →  FOLLOW(Factor) ⊇ FOLLOW(Term)
   Term → Factor             Factor is at the end  →  FOLLOW(Factor) ⊇ FOLLOW(Term)
```

## Why It Doesn't Finish in One Pass — Iteration

Rule ③ is the tricky one.\
We need to pour `FOLLOW(Expr)` into `FOLLOW(Term)`, but that `FOLLOW(Expr)` may itself still be in the middle of being filled in.\
It's exactly what blocked us with recursion in the FIRST case — they **depend on each other**, so it doesn't resolve in one pass.

So the remedy is the same too — **fill in the initial values with Rules ①·②, then repeat Rule ③ *until nothing changes*.**

## By the Definition — Running It on Our Grammar

**Initial values (Rules ①·②):**

```
   FOLLOW(Expr)   = { $, '+', ')' }      ← $ from ① , '+' ')' from ②
   FOLLOW(Term)   = { '*' }              ← '*' from ②
   FOLLOW(Factor) = { }                  ← still empty
```

**Pass 1 (Rule ③ inheritance):**

```
   FOLLOW(Term)   ⊇ FOLLOW(Expr)  →  { '*' } ∪ { $, '+', ')' }  =  { $, '+', ')', '*' }   (grew)
   FOLLOW(Factor) ⊇ FOLLOW(Term)  →  { }    ∪ { $, '+', ')', '*' } = { $, '+', ')', '*' } (grew)
```

→ Something grew, so we go around one more time.

**Pass 2:**

```
   FOLLOW(Term)   ⊇ FOLLOW(Expr)  →  no change
   FOLLOW(Factor) ⊇ FOLLOW(Term)  →  no change
```

→ Nothing grew this pass. **Stop!**

```
   FOLLOW(Expr)   = { $, '+', ')' }
   FOLLOW(Term)   = { $, '+', ')', '*' }
   FOLLOW(Factor) = { $, '+', ')', '*' }
```

Exactly the same answer we got on the [Definition and Derivation page](follow-formula.md) and in the [basic track](first-follow.md). ✓

## Summary

1. The FOLLOW of the **start symbol** gets `$`.
2. The **`FIRST(β) − ε`** of the `β` right after `B` goes into FOLLOW(B). (FIRST as an ingredient!)
3. If `B` is at the **very end**, **inherit the FOLLOW of the LHS** — and *repeat until nothing changes*.

Use FIRST as an ingredient (②), and solve inheritance by iteration (③) — that's all there is to computing FOLLOW.

## Next — These Rules in Code

Let's see how these three rules and the iteration made it into the `FirstFollowAnalyzer` code.\
(From the very first line of `CalculateAllFollow` being `CalculateAllFirst`, you can see right there that it uses FIRST as an ingredient.)

👉 **[FOLLOW · Implementation](follow-impl.md)**

---

👈 Previously: [FOLLOW · Definition and Derivation](follow-formula.md)
