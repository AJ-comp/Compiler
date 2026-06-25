# FIRST · Definition and derivation

> 🎓 This is the **advanced track · theory**. It helps to first grab the *concept* over in
> [the basic track's FIRST/FOLLOW](first-follow.md) and then come back. On this page we'll look at
> **what FIRST is exactly (the definition)**, and at **the process of deriving it directly, following
> that very definition**. Continuing on → **[Computation rules](first-rules.md)** → **[Implementation](first-impl.md)**.
>
> Don't feel any pressure — we'll go slowly.

> 📍 **Where it lives** · engine `FirstFollowAnalyzer` · module `Parse.FrontEnd` — **Layer 2** (the
> tier *below* the parse table)

Here's the example grammar we keep using.

```
Expr   : Expr '+' Term | Term ;
Term   : Term '*' Factor | Factor ;
Factor : '(' Expr ')' | id ;
```

The answer we worked out by hand in the basics was `FIRST(Expr) = FIRST(Term) = FIRST(Factor) = { '(', id }`.\
But before that — let's clearly pin down, step by step, **what FIRST is *exactly*** first.\
(If we skip this, the computation rules later just become an incantation you memorize.)

---

## Definition — what FIRST is

Let me nail it down in one line first.

> **FIRST(X)** = the set that gathers up every **leading terminal** of all the terminal strings that the
> symbol `X` **can derive (produce)**.

There are two key words — **derivation** and **leading terminal**.\
Let's look at each.

### "derivation" — unfolding by the rules

A single step of **swapping** a nonterminal for the right-hand side of its production is called a *derivation*,
and we write it with the arrow `⇒`.

Let me unfold `Expr` one step at a time.

```
   Expr  ⇒  Term  ⇒  Factor  ⇒  id
        (Expr:Term)  (Term:Factor)  (Factor:id)
```

We applied the rules three times and finally arrived at a string `id` **made only of terminals**.\
We write this unfolding over *several steps* as `⇒*` → `Expr ⇒* id` ("Expr derives id").

### So FIRST is

It's the collection of every **leading terminal** of the terminal strings you get by unfolding `X` *every which
way* all the way to the end.

Looking directly at `Expr`:

```
   Expr ⇒* id …             →  leading is id   ⇒   id ∈ FIRST(Expr)
   Expr ⇒* ( Expr ) …       →  leading is (    ⇒   ( ∈ FIRST(Expr)
```

`Expr` can build infinitely many strings (`id`, `id + id`, `( id ) * id`, …), but the **terminals that can come
at the very front** are, in the end, just two — `id` or `(`. So:

```
   FIRST(Expr) = { '(', id }
```

Written more rigorously in symbols (`T` = the set of terminals):

```
   FIRST(X) = { a ∈ T | X ⇒* a … (derives a string starting with a) }
```

> 📎 **Just one more thing about ε (the empty string).** If `X` can even derive *nothing at all*
> (`X ⇒* ε`), **we put ε into FIRST(X) too.** It's a mark meaning "X may disappear entirely." (Our
> example has no such nonterminal, so ε doesn't show up — but it's definitely part of the definition.)

To sum up — whether it's a terminal, a nonterminal, or a *sequence of several symbols*, FIRST is **"the set of
leading terminals (plus ε if needed) of the strings you get by derivation."**\
That's the whole definition.

---

## By the definition — deriving it directly

Now let's compute FIRST by *applying that definition directly*.\
"Derive and gather the leading terminals," by hand.\
Starting with the easy one, `Factor`.

### Factor — it unfolds all the way without getting stuck

Let's derive `Factor`'s two **productions** (one per line, split by `|` — `A → α`, in detail [Single](deep-single.md)) all
the way out.

```
   Factor ⇒ id              →  leading terminal :  id
   Factor ⇒ ( Expr )        →  leading terminal :  (
```

The front of the strings `Factor` produces is `id` or `(` — just those two.\
Gathering them up exactly as the definition says:

```
   FIRST(Factor) = { id, '(' }
```

Easy, right?\
Inside `Factor` it never mentions itself, so the derivation ends cleanly.

### Term — when you unfold it, itself shows up again

`Term : Term '*' Factor | Factor`.\
Let's derive the two productions.

```
   ① Term ⇒ Factor ⇒ … ⇒ id or (            →  leading :  id, (
   ② Term ⇒ Term '*' Factor                  →  the front is Term again?!
```

② is the odd one.\
The front is *itself*, `Term`, so to know the leading terminal we have to unfold that `Term` **again**.

```
   Term ⇒ Term '*' Factor
        ⇒ (Term '*' Factor) '*' Factor
        ⇒ …                              ←  unfold and unfold, the front stays Term, no end in sight
```

But eventually that leading `Term` too settles down via the `Factor` production at some point, and then the
leading terminal becomes `id` or `(` again. So:

```
   FIRST(Term) = { id, '(' }
```

`Expr` (`Expr : Expr '+' Term | Term`) has the same shape, so `FIRST(Expr) = { id, '(' }`.

### Here's where we get stuck — we can't derive "all the way"

As we saw with `Term`, **if it holds onto itself (recursion), the derivation can grow infinitely long.**\
The definition is clear ("the leading terminal of what's derived"), but unfolding that derivation *all the way out,
one by one* is awkward to do by hand and by computer alike.

## Next — on to the computation rules

So on the next page we'll move to the **computation rules** that pull out the same FIRST **without unfolding the
derivation directly**.\
(And recursion gets handled gracefully too.)

👉 **[FIRST · Computation rules](first-rules.md)**

---

👈 Back to the basic concepts: [FIRST / FOLLOW](first-follow.md)
