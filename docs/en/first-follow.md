# FIRST / FOLLOW

From this chapter on, we step into the "heart" of LR parsing.\
The first concepts are the **FIRST set** and the **FOLLOW set**.

Let me be honest with you up front — the names are unfamiliar, and the first time you see them you might think, "what on earth is this?"\
**That's okay, though. Everyone stumbles here at least once.**\
This is exactly the part where people get the most lost
in a compiler class.\
So if it doesn't click right away, that's not strange at all.\
I'll go really slowly,
holding your hand the whole way.\
Once you've read it all, you'll think, "oh, that wasn't a big deal after all." 🙂

---

> 📖 **Before we start:** This chapter assumes you can *read* the example grammar. If notation like `:` `|` `;`
> looks unfamiliar, go check out [How to Read Grammars](grammar-reading.md) first — five minutes is plenty.

This is the example grammar we'll keep using throughout this chapter (for reference):

```
Expr   : Expr '+' Term | Term ;
Term   : Term '*' Factor | Factor ;
Factor : '(' Expr ')' | id ;
id     := "[a-zA-Z]+" ;
```

---

## ① Why we need it

As the parser reads tokens one by one from the left, it has to **keep making decisions**.\
The two most
important decisions are these.

1. **"Can I start some rule right now?"** — looking at the next token, it has to pick where to go
2. **"Did the rule I was reading just end?"** — it has to decide where one chunk ends and when to group things up

In words alone this is abstract.\
Let me give an example.\
Suppose we're reading `a + a * a` and we've **just seen the first `a`**.\
The parser falls into this dilemma.

> "Should I count this single `a` as a finished chunk (a `Term`)? Or is it an unfinished chunk that still has
> `* something` coming after it?"

How does it decide?\
Surprisingly simple.\
**It takes a quick peek at the next token.**

- next is `*` → not finished yet (more multiplication is coming)
- next is `+` or the end of input → it ends here (we can group it up)

In other words, the parser has to know in advance **"which tokens can come *after* this chunk."**\
That's exactly
**FOLLOW**.\
And **"which tokens can *start* this chunk"** is **FIRST**.

> 💡 Don't overthink it. The one-line summary is this:
> **FIRST/FOLLOW = a *cheat sheet* the parser prepares in advance to judge "start/end."**
> You need this to build the [parse table](first-follow.md) in the next chapter.

## ② What it does

> From here we'll work out the sets by hand. **Even if the calculation looks like a lot, don't feel pressured.**
> The pattern is simple, and we'll fill it in slowly, one line at a time, together.

### FIRST — "what tokens can this *start* with"

**FIRST(X)** = the **collection of terminals (tokens) that can appear at the very front** of any string X can produce.

Let's start with the easiest, `Factor`.

```
Factor : '(' Expr ')' | id ;
```

`Factor` starts with `(` or starts with `id`, right? So:

```
FIRST(Factor) = { '(', id }
```

(`{ }` is just a mark meaning "collection, set."\
The things inside the braces are the candidates.)

Next, `Term`:

```
Term : Term '*' Factor | Factor ;
```

`Term` starts with `Term` (itself! — that recursion from before) or starts with `Factor`.\
If you keep expanding the
thing that starts with itself, the very front ends up being `Factor`.\
So:

```
FIRST(Term) = FIRST(Factor) = { '(', id }
```

By the same logic, `Expr` too:

```
FIRST(Expr) = { '(', id }
```

All three start with either `(` or `id`.\
Makes sense, right?\
Any expression, in the end, starts with either a **name (`id`)**
or an **opening parenthesis (`(`)**.\
**If you've followed this far, FIRST is done!**\
Not much to it, was there?

### FOLLOW — "what tokens can come *after* this"

**FOLLOW(X)** = the **collection of terminals that can appear right after X** somewhere in a valid sentence.
A special symbol **`$`** (a virtual token meaning the end of input) can also go in here.

Let's start with `Expr`.\
If you search the whole grammar for "what can come after `Expr`?":

- `Expr` is the **start symbol** (the whole sentence is `Expr`) → so after `Expr`, the **end of input `$`** can come
- in `Expr : Expr '+' Term` → after the first `Expr` comes `+`
- in `Factor : '(' Expr ')'` → after `Expr` comes `)`

Gathering them all:

```
FOLLOW(Expr) = { $, '+', ')' }
```

For `Term`, in the same way, sweep through "what comes after Term":

- there are cases where the very end of `Expr` is `Term` (`Expr : ... Term`, `Expr : Term`) → in that case, **whatever can
  come after Expr can also come after Term** → FOLLOW(Expr) carries straight in
- `Term : Term '*' Factor` → after the first `Term` comes `*`

```
FOLLOW(Term) = FOLLOW(Expr) ∪ { '*' } = { $, '+', ')', '*' }
```

(`∪` is the "combine (union)" symbol.\
It just means combining the two collections.)

`Factor` too, the same way:

```
FOLLOW(Factor) = { $, '+', ')', '*' }
```

### Now, back to that first dilemma

Remember it?\
That dilemma from reading `a` (that is, `Factor` → `Term`) and looking at the next token.\
Now you can see the answer.

- next is `*` → a signal to "keep going" (into `Term '*' Factor`)
- next is `+` or `$` → these two are in **FOLLOW(Term)** → "Term is done, so **group it up (reduce)**"

**This is exactly how FOLLOW decides "when to group up."**\
Are you starting to get a feel for why FIRST/FOLLOW are
the ingredients of the parse table? (It's fine if not — in the next chapter, seeing the table directly will make it click hard.)

### Two special situations (just lightly, for now)

You don't need to go too deep.\
Just enough to know "ah, there's this thing."

- **ε (epsilon, the empty string):** a mark used when some nonterminal can become "nothing at all."
  It doesn't show up in our example, so you don't need to worry about it for now. (We'll cover it separately later in the advanced track.)
- **`$` (end mark):** as we just saw, a virtual token representing the end of input. It always goes into the FOLLOW of the start symbol.

## ③ Seeing it in the playground

FIRST/FOLLOW themselves aren't shown directly on screen, but **through the reduce cells of the parse table that they produce**
you can see their effect with your own eyes.\
In the playground:

1. **Run** with the default grammar and the input `a + a * a`
2. In the **Parse table**, find the spots where a green **reduce** badge appears when the next token is `+`/`)`/`$` (that is, FOLLOW(Term)) —
   that's exactly where FOLLOW said "group up here."
3. Going one cell at a time with **Step through**, you can see the action split depending on whether the token after `a` is `*` or `+`.

👉 **[Live playground](https://polite-island-0b2142200.7.azurestaticapps.net)**

> (A panel that shows the FIRST/FOLLOW sets *directly* as a table is planned.)

---

## One step further — the advanced track (optional)

This far is the **concept**.\
And — honestly, for the basic track, **this is enough.**\
You can go straight to the
next basic chapter.\
Still, for those who want to dig deeper, let me open up one path.

> 🎓 We just worked out the FIRST/FOLLOW of *this example grammar* by hand. Tidying that up into a **formula (algorithm) that
> works for any grammar**, and seeing **how it's implemented in the Janglim code** — that's covered separately in the
> **advanced track** at [FIRST — Definition and Derivation](first-formula.md) (→ calculation rules → implementation).
>
> **It's totally fine not to read it.** You've already got the concept down. 🙂

---

## Next chapter

The ingredients called FIRST/FOLLOW are ready.\
**You've really followed along well this far 🎉**\
Next is — how the parser remembers *"how far it has read so far"* (the **dot**), and the **state** that gathers up *"what's possible right now."*\
Once those come together, you finally get that famous **parse table**.

👉 **[Dot and State](dot-and-state.md)**
