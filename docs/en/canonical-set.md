# The canonical collection — every state (I₀ ~ I₁₁)

> 🎓 This is an **advanced track** chapter.\
> With [closure](closure-def.md) we *filled out a single state completely*, and with [GOTO](goto.md) we *read one symbol and moved to the next
> state.*\
> If you repeat these two — starting from the start state `I₀` and going **until no new state appears anymore** — then *every reachable
> state* gathers together. That is the **canonical collection.**

> 📍 **Where it lives** · `CanonicalRelation.Calculate` · `…/Parsers/Collections/CanonicalRelation.cs`

If you run our example grammar (the augmented one) all the way to the end, you get exactly **12 states — `I₀` ~ `I₁₁`**.\
Below we'll write them all out. For each state we've placed the *items*, together with the *transitions (GOTO)* you take by reading a symbol.\
(A **completed (reduce) item**, where the dot has gone all the way to the end, is marked with `← reduce`.)

The augmented grammar is this.

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>   |  <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>  |  <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     |  <span class="setm">id</span></pre>

---

## `I₀` — the start state

This is the state we got by taking the closure of `Accept → • Expr`. (Those same 7 items we built in [how to compute](closure-calc.md).)

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

**Transitions:**

- read `Expr` → `I₁`
- read `Term` → `I₂`
- read `Factor` → `I₃`
- read `'('` → `I₄`
- read `id` → `I₅`

## `I₁` — `GOTO(I₀, Expr)`

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="nt">Expr</span> <span class="lrdot">•</span>              <span style="opacity:.65">← reduce (accept at end of input $)</span>
   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">'+'</span> <span class="nt">Term</span></pre>

**Transition:** read `'+'` → `I₆`

## `I₂` — `GOTO(I₀, Term)`

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>               <span style="opacity:.65">← reduce (reduce: Expr → Term)</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

**Transition:** read `'*'` → `I₇`

## `I₃` — `GOTO(I₀, Factor)`

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Factor</span> <span class="lrdot">•</span>             <span style="opacity:.65">← reduce (reduce: Term → Factor)</span></pre>

**Transition:** none (a state with only a completed item)

## `I₄` — `GOTO(I₀, '(')`

We read `'('` and moved the dot to get `Factor → '(' • Expr ')'`; since `Expr` is right after the dot, the closure attaches again, giving 7 items.

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

**Transitions:**

- read `Expr` → `I₈`
- read `Term` → `I₂`
- read `Factor` → `I₃`
- read `'('` → `I₄`
- read `id` → `I₅`

## `I₅` — `GOTO(I₀, id)`

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">id</span> <span class="lrdot">•</span>               <span style="opacity:.65">← reduce (reduce: Factor → id)</span></pre>

**Transition:** none (a state with only a completed item)

## `I₆` — `GOTO(I₁, '+')`

<pre class="lrbox">   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

**Transitions:**

- read `Term` → `I₉`
- read `Factor` → `I₃`
- read `'('` → `I₄`
- read `id` → `I₅`

## `I₇` — `GOTO(I₂, '*')`

<pre class="lrbox">   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

**Transitions:**

- read `Factor` → `I₁₀`
- read `'('` → `I₄`
- read `id` → `I₅`

## `I₈` — `GOTO(I₄, Expr)`

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">')'</span>
   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">'+'</span> <span class="nt">Term</span></pre>

**Transitions:**

- read `')'` → `I₁₁`
- read `'+'` → `I₆`

## `I₉` — `GOTO(I₆, Term)`

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> <span class="lrdot">•</span>      <span style="opacity:.65">← reduce (reduce: Expr → Expr '+' Term)</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

**Transition:** read `'*'` → `I₇`

## `I₁₀` — `GOTO(I₇, Factor)`

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> <span class="lrdot">•</span>    <span style="opacity:.65">← reduce (reduce: Term → Term '*' Factor)</span></pre>

**Transition:** none (a state with only a completed item)

## `I₁₁` — `GOTO(I₈, ')')`

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> <span class="lrdot">•</span>     <span style="opacity:.65">← reduce (reduce: Factor → '(' Expr ')')</span></pre>

**Transition:** none (a state with only a completed item)

---

## Transitions at a glance

If you gather all the transitions above into one table, it looks like this. A blank cell means *there is nowhere to go on that symbol.*

| State | `Expr` | `Term` | `Factor` | `'+'` | `'*'` | `'('` | `')'` | `id` |
|:--|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| `I₀`  | I₁ | I₂ | I₃ |    |    | I₄ |    | I₅ |
| `I₁`  |    |    |    | I₆ |    |    |    |    |
| `I₂`  |    |    |    |    | I₇ |    |    |    |
| `I₃`  |    |    |    |    |    |    |    |    |
| `I₄`  | I₈ | I₂ | I₃ |    |    | I₄ |    | I₅ |
| `I₅`  |    |    |    |    |    |    |    |    |
| `I₆`  |    | I₉ | I₃ |    |    | I₄ |    | I₅ |
| `I₇`  |    |    | I₁₀|    |    | I₄ |    | I₅ |
| `I₈`  |    |    |    | I₆ |    |    | I₁₁|    |
| `I₉`  |    |    |    |    | I₇ |    |    |    |
| `I₁₀` |    |    |    |    |    |    |    |    |
| `I₁₁` |    |    |    |    |    |    |    |    |

## Next chapter

We've gathered all the states, and all the transitions between them too — the canonical collection is complete.

Now all that's left is to turn this into **a single table.** The *transition table* above becomes the parser's **shift / goto** as-is, and each state's **completed (reduce) item** becomes *"when to bundle things up."* Combine these two, and you get — the **parse table** of the next chapter.

👉 **[The parse table · how to build it](parse-table-build.md)**

---

👈 Previously: [GOTO](goto.md)
