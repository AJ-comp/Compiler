# FIRST / FOLLOW

From this chapter on, we step into the "heart" of LR parsing.\
The first concepts are the **FIRST set** and the **FOLLOW set**.

Let me be honest with you up front — the names are unfamiliar, and the first time you see them you might think, "what on earth is this?"\
**That's okay, though. Everyone pauses here at least once.**\
This is exactly the part people get the most lost in
in a compiler class.\
So if it doesn't click on the first try, that's not strange at all.\
I'll go really slowly,
holding your hand the whole way.\
Follow along slowly, and you'll naturally see why these two sets are needed. 🙂

---

> 📖 **Before we start:** This chapter assumes you can *read* the example grammar. If notation like `:` `|` `;`
> looks unfamiliar, go check out [How to Read Grammars](grammar-reading.md) first — five minutes is plenty.

This is the example grammar we'll keep using throughout this chapter (for reference):

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
<span class="setm">id</span>     := "[a-zA-Z]+" ;
</pre>

---

## ① Why we need it

As the parser reads tokens one by one from the left, it has to **keep making decisions**.\
The two most
important of those decisions are these.

1. **"Can I start some rule right now?"** — looking at the next token, it has to pick where to go
2. **"Did the rule I was reading just end?"** — it has to decide where one chunk ends and when to group it up

In words alone this stays abstract.\
Let me give you an example.\
Suppose we're reading `a + a * a` and we've **just seen the first `a`**.\
The parser falls into this dilemma.

> "Should I count this single `a` as a finished chunk (a `Term`)? Or is it an unfinished chunk that still has
> `* something` coming after it?"

How does it decide?\
It's surprisingly simple.\
**It takes a quick peek at the next token.**

- next is `*` → not finished yet (more multiplication is coming)
- next is `+` or end of input → it ends here (we can group it up)

So the parser has to know in advance **"which token can come *after* this chunk."**\
That's exactly
**FOLLOW**.\
And **"which token can *start* this chunk"** is **FIRST**.

> 💡 Don't overthink it. The one-line summary is this:
> **FIRST/FOLLOW = the *cheat sheet* the parser prepares ahead of time to judge "start/end."**
> You need this to build the [parse table](parse-table-build.md) that comes later.

## ② What it does

> From here we'll work the sets out by hand. **Even if it looks like a lot of calculation, don't let it weigh on you.**
> The pattern is simple, and we'll fill it in slowly, one line at a time, together.

### FIRST — "which token can this *start* with"

**FIRST(X)** = the **collection of terminals (tokens) that can appear first** when you derive X. (= the terminal that comes at the *very front* of that string.)

Let's work out the FIRST of the three nonterminals — `Factor` · `Term` · `Expr` — one by one. Starting with the easiest.

<div class="ex-card">

**① `Factor` — finishes without a hitch**

<pre class="lrbox">
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
</pre>

`Factor` starts with `(` or with `id`, right? So:

<pre class="lrbox">
FIRST(<span class="nt">Factor</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

> `{ }` is the mark that means a *set* — what's inside the braces are the candidates.

</div>

<div class="ex-card">

**② `Term` — itself shows up again**

<pre class="lrbox">
<span class="nt">Term</span> : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
</pre>

`Term` starts with `Term` (itself! — that's the recursion from before) or with `Factor`.\
If you keep
expanding the one that starts with itself, the very front ends up being `Factor`.\
So:

<pre class="lrbox">
FIRST(<span class="nt">Term</span>) = FIRST(<span class="nt">Factor</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

</div>

<div class="ex-card">

**③ `Expr` — same shape**

By the same logic, `Expr` too:

<pre class="lrbox">
FIRST(<span class="nt">Expr</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

</div>

All three start with either `(` or `id`.\
Makes sense, right?\
Whatever the expression, its very front ends up being either a **name (`id`)** or
an **opening paren (`(`)**.\
**If you've followed along this far, FIRST is done!**\
Less to it than you'd expect, right?

### FOLLOW — "which token can come *after* this"

**FOLLOW(X)** = the **collection of terminals that can appear right after X** somewhere in a valid sentence.
A special symbol, **`$`** (an imaginary token meaning end of input), can also be in there.

This time too, let's look at all three — `Expr` · `Term` · `Factor` — one by one.

<div class="ex-card">

**① `Expr` — starting from the start symbol**

If we search the whole grammar for "what can come after `Expr`":

- `Expr` is the **start symbol** (a whole sentence is an `Expr`) → so after `Expr`, **end of input `$`** can come
- in `Expr : Expr '+' Term` → after the first `Expr` comes `+`
- in `Factor : '(' Expr ')'` → after `Expr` comes `)`

Putting it all together:

<pre class="lrbox">
FOLLOW(<span class="nt">Expr</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
</pre>

</div>

<div class="ex-card">

**② `Term` — inherits `Expr`'s FOLLOW**

For `Term`, scanning through "what comes after Term" the same way:

- there are cases where `Term` is at the very end of `Expr` (`Expr : ... Term`, `Expr : Term`) → then **whatever can
  come after Expr can also come after Term** → FOLLOW(Expr) flows straight in
- `Term : Term '*' Factor` → after the first `Term` comes `*`

<pre class="lrbox">
FOLLOW(<span class="nt">Term</span>) = FOLLOW(<span class="nt">Expr</span>) ∪ { <span class="setm">'*'</span> } = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

> `∪` is the *union* symbol — it just means combining the two collections.

</div>

<div class="ex-card">

**③ `Factor` — just like `Term`**

Pointing at the spots for `Factor` the same way:

- `Factor` is always at the *very end* of `Term`'s rules (`Term : Term '*' Factor`, `Term : Factor`) → then **whatever can
  come after Term can also come after Factor** → FOLLOW(Term) flows straight in

<pre class="lrbox">
FOLLOW(<span class="nt">Factor</span>) = FOLLOW(<span class="nt">Term</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

</div>

### Now, back to that first dilemma

Remember it?\
That dilemma when we read `a` (i.e. `Factor` → `Term`) and looked at the next token.\
Now the answer is in sight.

- next is `*` → a "keep going" signal (into `Term '*' Factor`)
- next is `+` or `$` → these two are in **FOLLOW(Term)** → "Term is done, so **group it up (reduce)**"

**This is exactly how FOLLOW decides "when to group up."**\
Here you can see why FIRST/FOLLOW are the raw material for the parse table.\
It gets even clearer once you build that table yourself in the next chapter.

### Two special situations (just lightly for now)

You don't need to go too deep.\
Just enough to think "ah, there's a thing like this."

- **ε (epsilon, the empty string):** the mark used when some nonterminal can become "nothing at all."
  It doesn't show up in our example, so take it lightly for now — the advanced [FIRST · Computation rules](first-rules.md) shows it directly with a small grammar that has ε.
- **`$` (end mark):** as we just saw, an imaginary token representing end of input. It's always in the FOLLOW of the start symbol.

## ③ Seeing it in the playground

FIRST/FOLLOW themselves aren't shown directly on screen, but you can see their effect with your own eyes through
**the reduce cells of the parse table they produce**.\
In the playground:

1. **Run** with the default grammar and the input `a + a * a`
2. In the **Parse table**, find the spots where a green **reduce** badge shows up when the next token is
   `+`/`)`/`$` (that is, FOLLOW(Term)) — that's exactly where FOLLOW said "group up here."
3. Going one cell at a time with **Step through**, you can watch the behavior split depending on whether the token after `a` is `*` or `+`.

👉 **[Live playground](https://polite-island-0b2142200.7.azurestaticapps.net)**

> (A panel that shows the FIRST/FOLLOW sets *directly* in a table is planned to be added.)

---

## One step further — Advanced track (optional)

That's the **concept**.\
And — honestly, the Basics track is **enough right here.**\
You can go straight
to the next basics chapter.\
But for those who want to dig deeper, let me open up one path.

> 🎓 What we just did was work out the FIRST/FOLLOW of *this example grammar* by hand. Taking that and turning it into a
> **formula (algorithm) that works for any grammar**, and then seeing **how it's implemented in Janglim's code** —
> that's covered separately in the **Advanced track**, in [FIRST — Definition & Derivation](first-formula.md) (→ Computation Rules → Implementation).
>
> **It's totally fine not to read it.** You've already got the concept down. 🙂

---

## Next chapter

The raw material called FIRST/FOLLOW is ready.\
**You've really done a great job following along this far 🎉**\
Next up — how the parser remembers *"how far it has read so far"* (the **dot**), and the **state** that
gathers *"the things possible right now."*\
Once those come together, we finally get the famous **parse table**.

👉 **[The Dot and States](dot-and-state.md)**
