# Canonical collection — every state (I₀ ~ I₁₁)

> 🎓 This is the **advanced track**.\
> With [closure](closure-def.md) we *filled out a single state completely*, and with [GOTO](goto.md) we *read one symbol and moved to the next
> state*.\
> If you repeat these two — starting from the initial state `I₀` and going **until no new state appears anymore** — then *every reachable
> state* gathers together. That is the **canonical collection**.

> 📍 **Where it lives** · `CanonicalRelation.Calculate` · `…/Parsers/Collections/CanonicalRelation.cs`

If you run our example grammar (the augmented one) all the way to the end, you get exactly **12 states — `I₀` ~ `I₁₁`**.\
Below we'll write them all out. For each state we've placed the *items*, together with the *transitions (GOTO)* you take by reading a symbol.\
(A **complete (reduce) item**, where the dot has gone all the way to the end, is marked with `← complete`.)

The augmented grammar is this.

```
   Accept → Expr
   Expr   → Expr '+' Term   |  Term
   Term   → Term '*' Factor  |  Factor
   Factor → '(' Expr ')'     |  id
```

---

## `I₀` — initial state

This is the state we got by taking the closure of `Accept → • Expr`. (Those same 7 we built in the [calculation method](closure-calc.md).)

<pre class="lrbox">   Accept → <span class="lrdot">•</span> Expr
   Expr   → <span class="lrdot">•</span> Expr '+' Term
   Expr   → <span class="lrdot">•</span> Term
   Term   → <span class="lrdot">•</span> Term '*' Factor
   Term   → <span class="lrdot">•</span> Factor
   Factor → <span class="lrdot">•</span> '(' Expr ')'
   Factor → <span class="lrdot">•</span> id</pre>

**Transitions:**

- read `Expr` → `I₁`
- read `Term` → `I₂`
- read `Factor` → `I₃`
- read `'('` → `I₄`
- read `id` → `I₅`

## `I₁` — `GOTO(I₀, Expr)`

<pre class="lrbox">   Accept → Expr <span class="lrdot">•</span>              <span style="opacity:.65">← complete (accept at end of input $)</span>
   Expr   → Expr <span class="lrdot">•</span> '+' Term</pre>

**Transitions:** read `'+'` → `I₆`

## `I₂` — `GOTO(I₀, Term)`

<pre class="lrbox">   Expr → Term <span class="lrdot">•</span>               <span style="opacity:.65">← complete (reduce: Expr → Term)</span>
   Term → Term <span class="lrdot">•</span> '*' Factor</pre>

**Transitions:** read `'*'` → `I₇`

## `I₃` — `GOTO(I₀, Factor)`

<pre class="lrbox">   Term → Factor <span class="lrdot">•</span>             <span style="opacity:.65">← complete (reduce: Term → Factor)</span></pre>

**Transitions:** none (a state with only a complete item)

## `I₄` — `GOTO(I₀, '(')`

We read `'('` and moved the dot to get `Factor → '(' • Expr ')'`; since `Expr` is right after the dot, the closure attaches again, giving 7 items.

<pre class="lrbox">   Factor → '(' <span class="lrdot">•</span> Expr ')'
   Expr   → <span class="lrdot">•</span> Expr '+' Term
   Expr   → <span class="lrdot">•</span> Term
   Term   → <span class="lrdot">•</span> Term '*' Factor
   Term   → <span class="lrdot">•</span> Factor
   Factor → <span class="lrdot">•</span> '(' Expr ')'
   Factor → <span class="lrdot">•</span> id</pre>

**Transitions:**

- read `Expr` → `I₈`
- read `Term` → `I₂`
- read `Factor` → `I₃`
- read `'('` → `I₄`
- read `id` → `I₅`

## `I₅` — `GOTO(I₀, id)`

<pre class="lrbox">   Factor → id <span class="lrdot">•</span>               <span style="opacity:.65">← complete (reduce: Factor → id)</span></pre>

**Transitions:** none (a state with only a complete item)

## `I₆` — `GOTO(I₁, '+')`

<pre class="lrbox">   Expr   → Expr '+' <span class="lrdot">•</span> Term
   Term   → <span class="lrdot">•</span> Term '*' Factor
   Term   → <span class="lrdot">•</span> Factor
   Factor → <span class="lrdot">•</span> '(' Expr ')'
   Factor → <span class="lrdot">•</span> id</pre>

**Transitions:**

- read `Term` → `I₉`
- read `Factor` → `I₃`
- read `'('` → `I₄`
- read `id` → `I₅`

## `I₇` — `GOTO(I₂, '*')`

<pre class="lrbox">   Term   → Term '*' <span class="lrdot">•</span> Factor
   Factor → <span class="lrdot">•</span> '(' Expr ')'
   Factor → <span class="lrdot">•</span> id</pre>

**Transitions:**

- read `Factor` → `I₁₀`
- read `'('` → `I₄`
- read `id` → `I₅`

## `I₈` — `GOTO(I₄, Expr)`

<pre class="lrbox">   Factor → '(' Expr <span class="lrdot">•</span> ')'
   Expr   → Expr <span class="lrdot">•</span> '+' Term</pre>

**Transitions:**

- read `')'` → `I₁₁`
- read `'+'` → `I₆`

## `I₉` — `GOTO(I₆, Term)`

<pre class="lrbox">   Expr → Expr '+' Term <span class="lrdot">•</span>      <span style="opacity:.65">← complete (reduce: Expr → Expr '+' Term)</span>
   Term → Term <span class="lrdot">•</span> '*' Factor</pre>

**Transitions:** read `'*'` → `I₇`

## `I₁₀` — `GOTO(I₇, Factor)`

<pre class="lrbox">   Term → Term '*' Factor <span class="lrdot">•</span>    <span style="opacity:.65">← complete (reduce: Term → Term '*' Factor)</span></pre>

**Transitions:** none (a state with only a complete item)

## `I₁₁` — `GOTO(I₈, ')')`

<pre class="lrbox">   Factor → '(' Expr ')' <span class="lrdot">•</span>     <span style="opacity:.65">← complete (reduce: Factor → '(' Expr ')')</span></pre>

**Transitions:** none (a state with only a complete item)

---

## Transitions at a glance

If you gather all the transitions above into one table, it looks like this. A blank cell means *there is nowhere to go on that symbol*.

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

Now all that's left is to turn this into **a single table**. The *transition table* above becomes the parser's **shift / goto** as-is, and each state's **complete (reduce) item** becomes *"when to bundle things up"*. Combine these two, and you get — the **parse table** of the next chapter.

👉 **[Parse table · how to build it](parse-table-build.md)**

---

👈 Previous: [GOTO](goto.md)
