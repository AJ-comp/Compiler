# Parsing for Real with the Table

Finally — let's put together everything we've gathered so far and **actually parse** `a + a * a`. (This is the most fun part 👀)\
We have all the ingredients: [dots and states](dot-and-state.md) (how far we've gotten · what's possible right now) and
[FIRST / FOLLOW](first-follow.md) (the start · end cheat sheet). The **parse table (the playbook of actions)** built from these two will tell us, at every
moment, *"do this."*

## A parser does exactly two things — `shift` and `reduce`

At every moment the parser does *one* of two things.

- **shift** — read one next token and push it onto the **stack**.
- **reduce** — when the top few items on the stack match the *right side* of some rule exactly, bundle them into the *left-side nonterminal*.
  (= one tree branch completed!)

> 🍽️ The **stack** is like a *stack of plates* — you only put things on top (shift), and you only take things off the top (reduce).\
> The parser piles up *"what it has read and bundled so far"* right here.

So *when do we shift and when do we reduce?* — the **parse table** decides that.\
*"Given this current state and if the next token is this → shift / reduce."* (How to *build* the table is in the
[advanced track](parse-table-build.md). Here, let's just follow what the table tells us.)

## Following `a + a * a` all the way through

`a` is a name, so it's the terminal `id`. We'll mark the end of input with `$`.\
Let's go one line at a time — **the `Action` column is exactly what the table told us to do.**

| # | Stack | Remaining input | Action |
|:--:|:--|:--|:--|
| 1  | (empty) | `a + a * a $` | **shift** `a` |
| 2  | `a` | `+ a * a $` | **reduce** `Factor → id` |
| 3  | `Factor` | `+ a * a $` | **reduce** `Term → Factor` |
| 4  | `Term` | `+ a * a $` | **reduce** `Expr → Term` |
| 5  | `Expr` | `+ a * a $` | **shift** `+` |
| 6  | `Expr +` | `a * a $` | **shift** `a` |
| 7  | `Expr + a` | `* a $` | **reduce** `Factor → id` |
| 8  | `Expr + Factor` | `* a $` | **reduce** `Term → Factor` |
| 9  | `Expr + Term` | `* a $` | **shift** `*` |
| 10 | `Expr + Term *` | `a $` | **shift** `a` |
| 11 | `Expr + Term * a` | `$` | **reduce** `Factor → id` |
| 12 | `Expr + Term * Factor` | `$` | ★ **reduce** `Term → Term '*' Factor` |
| 13 | `Expr + Term` | `$` | ★ **reduce** `Expr → Expr '+' Term` |
| 14 | `Expr` | `$` | **accept** 🎉 |

See how the stack *grows with shift, and shrinks as it bundles with reduce?*\
At the very end only a single `Expr` remains — the whole input has been bundled into *one expression*.

## Look — `a * a` got bundled *before* `+`

Look again at line **12**, marked with ★.\
Only **after** `a * a` was bundled into a single `Term`, was `+` bundled at line **13**.

In other words — `*` was bundled **earlier and deeper** than `+`. **Multiplication's precedence** was honored *all by itself*!\
(This is exactly the *"multiplication gets bundled before addition"* that we promised in [the pipeline at a glance](the-big-picture.md).)

Seeing it as the finished tree makes it obvious at a glance.

```
   Expr
   ├─ Expr
   │  └─ Term
   │     └─ Factor
   │        └─ a
   ├─ +
   └─ Term            ← a * a bundled into one chunk right here!
      ├─ Term
      │  └─ Factor
      │     └─ a
      ├─ *
      └─ Factor
         └─ a
```

The top `Expr` is *(left `a`) `+` (right `a * a`)*. See how `a * a` is bundled into one chunk deep on the right side? — the table led us there.

## The end — `accept` 🎉

When only a single `Expr` remains on the stack and the input has reached `$` (the end), the table finally says **accept**.\
*"The whole input is one `Expr` that fits the grammar"* — **parsing succeeded!**

> So what about *wrong* input? (e.g. `a + + a`)\
> If during parsing we hit a cell in the table that is **empty** — *"there's nothing we can do here"* → that's a **grammar error**.
> The table sorts out right from wrong too.

## You made it 🎉

This is **all there is to LR parsing** — *read (shift), bundle when it matches (reduce), do as the table says, all the way to `accept`.*

**Congratulations on completing the basic track!** 🎊\
From [how to read a grammar](grammar-reading.md) → [FIRST / FOLLOW](first-follow.md) → [dots and states](dot-and-state.md) →
*parsing for real*, you've now grasped the big picture of LR parsing.

> 🎓 If you're curious about *how to build the table yourself* and *the Orchid code* — head to the **advanced track**.\
> In the order [LR item](lr-item.md) → [state](lr-state.md) → [closure](closure-def.md) → [GOTO](goto.md) →
> [canonical collection](canonical-set.md) → [parse table](parse-table-build.md), you'll *build with your own hands* what you just saw.
> (Even without it, you already have the concepts down. 🙂)

---

👈 Previous: [dots and states](dot-and-state.md)
