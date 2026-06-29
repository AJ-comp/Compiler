# The Dot and the State — How the Parser Remembers "Roughly Where" It Is

In the previous chapter, [FIRST / FOLLOW](first-follow.md), we saw the **cheat sheet** the parser builds up so it can judge *"start/end."*\
This time, let's see how the parser remembers **"where roughly am I right now?"** as it reads the input.\
(Here too, no formulas or code — just by *feel*. 😀)

## As You Read Along — You Need a Mark for "How Far You've Read"

The parser reads tokens **one at a time, from the left.**\
As it does, situations keep coming up where some rule has been *started but not yet finished.*

For example, suppose you're following the rule `Expr : Expr '+' Term`.\
You've read up to `Expr`, and now you're waiting for `'+'` to come.\
How should you note down this *"how far you've come"*?

There's a very simple way — **put a single dot (`•`) right in the middle of the rule.**

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Expr</span> • <span class="setm">'+'</span> <span class="nt">Term</span>
</pre>

Everything **before** the dot is *already read*, everything **after** the dot is *still to be read*.\
The dot above means *"read up to Expr, next comes '+'."*

> 📖 A rule with a dot placed in it like this is called an **LR item**.\
> The name sounds grand, but the *"rule + how far"* you just saw is all there is to it. From now on we'll just say **"item."**

The dot moves **one slot to the right** every time you read a token.

<pre class="lrbox">
   <span class="nt">Expr</span> → • <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      (nothing read yet)
   <span class="nt">Expr</span> → <span class="nt">Expr</span> • <span class="setm">'+'</span> <span class="nt">Term</span>      (read up to Expr)
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> • <span class="nt">Term</span>      (read up to '+')
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> •      (all read — now it's time to bundle it into one piece!)
</pre>

When the dot reaches the **very end**, you've *finished reading* that rule.\
That's exactly when it's **time to reduce.** (It's that moment from [FIRST / FOLLOW](first-follow.md) where *"when it ends, you bundle it."*)

## But — There Are Usually *Several* Possibilities

Let's take just one more step.\
When the parser is standing at some point, there's no rule that says exactly one rule must be *in progress*.

Suppose, while reading `a + a * a`, you've seen the first `a` up to `Term`. (`a` is a name, so it's a `Factor`, and that `Factor` is a `Term`.)\
At that very moment, **two things are possible at once.**

- It might be *"this one `Term` is the end of a piece"* → `Expr → Term •`
- Or it might be *"something more, `* something`, attaches behind it"* → `Term → Term • '*' Factor`

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Term</span> •                 ← could be the end here
   <span class="nt">Term</span> → <span class="nt">Term</span> • <span class="setm">'*'</span> <span class="nt">Factor</span>      ← or '*' could attach more
</pre>

Gathering these *"items possible right now"* into one bundle is — the parser's **state.**

> 🧭 To make an analogy, a state is like a **navigation screen standing at a fork.**\
> It shows you at a glance *"these are the roads you can take from here."*\
> And the **next token** picks one of those roads for you — if it's `*`, go further; if it's `+` or the end of the input, reduce.\
> (That thing where [FOLLOW](first-follow.md) tells you *"the next tokens it's OK to reduce on"* — this is exactly where it gets used.)

## Every Time You Read — From State to State

Every time the parser reads a token, it **moves from one state to another.**\
Just like going *from station to station.*

```
   (start state) ──read──▶ (next state) ──read──▶ (next state) ──▶ …
```

So an LR parser is really a **"machine that moves around among states."**\
*Which state you're in + what the next token is* — those two together decide the next action (read more / reduce).

> 📦 *Exactly how many of these states there are and how they get made* follows a fixed **assembly process** (move the dot, fill in the missing possibilities…). It's a somewhat mechanical job, so if you're curious, start with [LR item](lr-item.md) in the **Advanced track** and go through it one step at a time.\
> **The Basics track is fine up to here** — as long as you've got the feel of *"a state = a bundle of what's possible right now, and reading moves you to the next state over."* 🙂

## Next Chapter

Now all the ingredients are gathered — the **dot** (how far you've read), the **state** (what's possible right now), and **[FIRST / FOLLOW](first-follow.md)** (the start/end cheat sheet).

Put these three together and you get — the parser's **manual of conduct**, namely the **parse table**.\
And with that table we'll **actually parse** `a + a * a`. (It's the most fun part 👀)

👉 **[Actually Parsing With the Table](parsing-in-action.md)**

---

👈 Previously: [FIRST / FOLLOW](first-follow.md)
