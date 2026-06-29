# GOTO — read one symbol and move to the next state

> 🎓 This is an **advanced track** chapter.\
> In the previous [Closure · How to compute](closure-calc.md), we built the start state `I₀`.\
> Now, from that state, **if you read one symbol**, which state do you go to — that's what **GOTO** decides.

> 📍 **Where it lives** · `Analyzer.Goto` · `…/Parsers/Analyzer.cs`

## Definition

> **GOTO(I, X)** = inside state `I`, pick the **items whose symbol right after the dot is `X`**,\
> move that dot **one slot past `X`** (`A → α • X β` → `A → α X • β`),\
> and run **[closure](closure-def.md)** again on the items collected that way — that's the state you reach after reading `X`.

GOTO is an operation that *finishes in one shot.*\
(Unlike closure, it doesn't "repeat until it closes" — move the dot, run closure once, and you're done. That's why here *the definition is also the how-to-compute.*)

## Try it yourself — one symbol at a time from `I₀`

Let me bring back the start state `I₀` (7 items) that we built in the previous [how to compute](closure-calc.md).

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

In this state, the symbols that can come *right after the dot* are — `Expr`, `Term`, `Factor`, `'('`, `id`.\
(This is the `MarkSymbolSet` we saw in the [State](lr-state.md) chapter.) Let's do GOTO once for each of these symbols.

### Reading `id` — `GOTO(I₀, id)`

The only item whose symbol after the dot is `id` is `Factor → • id`. Move the dot past `id`:

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span>        ──( read id )──▶        <span class="nt">Factor</span> → <span class="setm">id</span> <span class="lrdot">•</span></pre>

In `Factor → id •` the dot has reached the end — it's a *completed (reduce) item.* (When you arrive in this state it means *"bundle `id` into `Factor`."*) There's no nonterminal after the dot, so closure adds nothing more either. → A next state **with just 1 item.**

### Reading `Term` — `GOTO(I₀, Term)`

There are *two* items whose symbol after the dot is `Term`. Move both dots past `Term`:

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="lrdot">•</span> <span class="nt">Term</span>              ──( Term )──▶   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>
   <span class="nt">Term</span> → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>   ──( Term )──▶   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

Collecting these two (no new nonterminal after the dot, so closure adds nothing more):

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

> 💡 This state — where have we seen it? It's exactly **that `id * id` state from the [State](lr-state.md) chapter**!\
> That state from *"when we'd read `id` up to `Term`"* was actually **the state you reach by reading `Term` from `I₀`.**
> The two chapters that seemed scattered meet right here.

### Reading `Expr` — `GOTO(I₀, Expr)`

There are also two items whose symbol after the dot is `Expr` (`Accept → •Expr`, `Expr → •Expr '+' Term`). Moving them:

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>          ──( Expr )──▶   <span class="nt">Accept</span> → <span class="nt">Expr</span> <span class="lrdot">•</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> ──( Expr )──▶   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">'+'</span> <span class="nt">Term</span></pre>

Here `Accept → Expr •` is special — *the virtual start rule has gone all the way to the end*, which means **if the input ends here (`$`), we accept the parse (accept)**!\
The `Expr → Expr • '+' Term` sitting alongside it — if more `'+'` comes, we continue the expression.\
(So this state is exactly *"finish and accept, or keep going with `+`."* It's the **goal point** of the automaton we're building.)

### Reading `'('` — `GOTO(I₀, '(')` · here closure really gets to work

The only item whose symbol after the dot is `'('` is `Factor → • '(' Expr ')'`. Move the dot past `'('`:

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>    ──( read '(' )──▶    <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">')'</span></pre>

But this time it's different — the **symbol after the moved dot is the nonterminal `Expr`**!\
With `id`·`Term`·`Expr` we were done just by moving the dot, but this one item alone is an **incomplete state** that's missing *how `Expr` starts.* So **closure kicks in again.**

Following the `Expr` after the dot — `Expr`'s rules, then from there `Term`'s rules, and `Factor`'s rules too — they get pulled in one after another and **fill up to 7 items.**

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">')'</span>        <span style="opacity:.65">← dot moved</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>         <span style="opacity:.65">← filled in by closure</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

This is exactly **`I₄`** (it appears just like this in the [canonical collection](canonical-set.md)).

> 💡 Look — with `id`·`Term`·`Expr` we were *done just by moving the dot*, but for `'('` the symbol after the dot was a nonterminal, so **closure did real work** (1 item → 7 items). This is the final *"closure again"* part of the GOTO definition.\
> So **closure isn't only used when building `I₀` — it's used again at every GOTO.** Thanks to that, *the result of any GOTO is always a complete state with no gaps.*\
> In one line — **closure = complete one state, GOTO = move the dot → closure again.**

### Recap — every GOTO result gets a number (`Iₙ`)

The next states we just went to from `I₀` via GOTO — each one is a **new state that gets a number.**\
`I₀`'s `MarkSymbolSet` (the symbols it can read) was `{ Expr, Term, Factor, '(', id }`, five of them, so there are five next states too. When we build the canonical collection, `I₁`, `I₂`, … are assigned *in the order the states are discovered.*

| Symbol read from `I₀` | GOTO result |
|:--|:--|
| `Expr`   | `I₁` |
| `Term`   | `I₂` |
| `Factor` | `I₃` |
| `'('`    | `I₄` |
| `id`     | `I₅` |

(That's why the result of `'('` above was `I₄`. We didn't look at `Factor` as an example, but it's the same idea — moving the dot in `Term → • Factor` gives `Term → Factor •`, which is `I₃`.)\
One thing to watch out for — above we looked at `id`·`Term`·`Expr`·`'('` in *an order that was good for explaining*, but **the numbers themselves are assigned in "discovery order."** So the order we showed (`id` first) and the number (`id` = `I₅`) aren't necessarily the same.

## Implementation — `Analyzer.Goto`

```csharp
public static CanonicalState Goto(CanonicalState iStatus, Symbol toSeeSymbol)
{
    if (toSeeSymbol == null) return null;
    var param = new CanonicalState();

    foreach (var item in iStatus)
    {
        if (item.MarkSymbol == toSeeSymbol)   // only items whose symbol after the dot equals the symbol to read
        {
            var clone = item.Clone() as LRItem;
            clone.MoveMarkSymbol();            // move the dot one slot forward
            param.Add(clone);
        }
    }

    return Analyzer.Closure(param);            // run closure again on the moved items
}
```

- `item.MarkSymbol == toSeeSymbol` — this picks only the items *whose symbol right after the dot is the symbol `X` to read.*
- `clone.MoveMarkSymbol()` — exactly the *"advance the dot one slot"* we saw in the [LR item](lr-item.md) chapter. (We `Clone` first so as not to touch the original — we have to leave `I₀` as it is.)
- `return Closure(param)` — runs closure on the moved items *again.* The result of GOTO has to be a **complete state** too. (Like the `'('` example just now, if the symbol after the dot is a nonterminal, closure fills it in; otherwise, like the `id`·`Term` examples, it returns them as-is.) — that closure is used not only for `I₀` but *here too* is captured in this one line.

## Next chapter

Doing GOTO from a single `I₀` for `id`·`Term`·`Expr`·`'('`…, the *next states* came out one after another.

If you **repeat this for every state, until no new state appears anymore** — then *all the states reachable* from the start state come together.\
That collection of states is exactly the **canonical collection.**

👉 **[The canonical collection — building all the states](canonical-set.md)**

---

👈 Previously: [Closure · Implementation](closure-impl.md)
