# How to Read a Grammar

This manual uses **this one tiny grammar** as its main example (for special concepts like ε, it occasionally shows a small auxiliary grammar separately).\
Before we dive into the real material, you'll want to be able to *read* this grammar, right?\
Don't worry — once you know **exactly four symbols**, it reads smoothly.\
Let's take it apart together, slowly.

Here's that grammar.\
The first time you see it, with so many symbols, it can feel a little unfamiliar.

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
<span class="setm">id</span>     := "[a-zA-Z]+" ;
</pre>

> 🎨 *Color hint — <span class="nt">**purple**</span> is a **nonterminal**, <span class="setm">**teal**</span> is a **terminal**. (We'll explain both properly down below. For now, just "ah, there are two kinds in different colors.")*

## The Four Symbols

| Symbol | Meaning | In plain words |
|---|---|---|
| `:` | "is made like this" | make the name on the left, using the method on the right |
| <code>&#124;</code> | "or" | splits things up when there are several ways to make it |
| `;` | "this rule ends here" | like the period at the end of a sentence |
| `'+'` (quotes) | the character itself | the actual `+` symbol that gets printed on screen |

## If We Translate the First Line into English

> `Expr : Expr '+' Term | Term ;`
>
> → "**Expr** can be made like this: **an Expr, then `+`, then a Term**, **or** just **a single Term**."

Read it again slowly.\
`:` means "is made like this," `|` means "or," `;` means "the end."\
Not so hard, right?

## Wait, There's Another Expr Inside Expr (Recursion)

Did you notice?\
**We're explaining Expr, and another Expr shows up inside it.**\
It points to itself.\
This is called **recursion**.

You might feel a little uneasy — "wait, isn't that an infinite loop?" — but **don't worry. Not at all.**\
This just means **"you can keep chaining several things together with `+`."**\
For example, something like `a + a + a`.\
(`a + a` is an Expr, and then we attach `+ a` to it again.)\
Natural, right?\
Only the word "recursion" sounds scary; the actual content is nothing more than "chaining several things together."

## The Other Lines Work the Same Way

- `Term : Term '*' Factor | Factor ;`
  → "Term is **Term `*` Factor**, or **a single Factor**." (chaining together with `*`, multiplication)
- `Factor : '(' Expr ')' | id ;`
  → "Factor is **`(` Expr `)`** (an expression wrapped in parentheses), or **id** (a single name)."

## Only the Last Line Is a Little Different

> `id := "[a-zA-Z]+" ;`
>
> → "**id** is a token made of alphabet letters."

Here, `"[a-zA-Z]+"` is a thing called a **regular expression**.\
But — **it's really fine if you don't know regular expressions.**\
For now, understanding it as just **"a name made of English alphabet letters"** (e.g., `a`, `x`, `foo`) is 100% enough.\
(And `:=` is the mark that says "this is a token defined by a character pattern.")

## That Thing We Just Read — It Has a Name

If you've made it this far, you've now learned how to *read* this grammar.\
And — this **notation for writing a grammar as rules**, the one we just worked through, has a name.\
It's called **EBNF** (Extended Backus–Naur Form).\
The name sounds grand, but its form is nothing more than what you just saw.\
From now on, when someone says "EBNF," you can think *"Ah, that way of writing rules!"*\
**You've picked up a piece of jargon** 😎

## Just Two Words: Terminal / Nonterminal

Finally, just two more words.\
The names in a grammar split into two kinds.\
**Tell these two apart and you're done.**

- **nonterminal** = a "rule name" that **breaks down further** → `Expr`, `Term`, `Factor`
  (the "way to make it" is written out on the right.)
- **terminal** = an "actual token" that **doesn't break down any further** → `+`, `*`, `(`, `)`, `id`
  (the pieces that actually show up in the input.)

> **Why the names are like this** — "terminal" means **end, terminus**. There's nothing left to break apart, so it "ends" right there. A **non**terminal is the opposite — there's still more to unfold. Just knowing what the names mean cuts down on confusion a lot.

To put it as an analogy — **a nonterminal is a "dish name"** (e.g., *kimchi stew*), and **a terminal is the "actual ingredients"** (kimchi, tofu, water).\
A dish name can be unfolded into other dishes or ingredients, but an ingredient is the end in itself.

> One-line summary: **anything starting with a capital letter (Expr, Term…) is a nonterminal; a symbol or `id` is a terminal.**\
> (In the dish/ingredient analogy from above — nonterminal = dish name, terminal = ingredient.)

## Next Chapter

You've learned how to read a grammar!\
**The toughest hurdle is actually the one you just cleared** 👍\
Now we move on to the first concept of LR parsing, **FIRST / FOLLOW**.

👉 **[FIRST / FOLLOW](first-follow.md)**
