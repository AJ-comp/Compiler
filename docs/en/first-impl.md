# FIRST · Implementation (code)

> 🎓 This is the **deep-dive track · implementation**.\
> In the previous [FIRST · Calculation rules](first-rules.md) we looked at the three cases (terminal / nonterminal / ε) and the iteration as *rules*.\
> This time we follow how those rules made their way into the `FirstFollowAnalyzer` code, **almost line by line**.\
> (If you haven't seen [Definition and derivation](first-formula.md) yet, I'd recommend starting there.)

## Try it — the public API (one line)

Before we dive into the details, let's start with *how you actually use it*.\
From the parser, **one line** gives you the FIRST/FOLLOW of every symbol. (This entry point is shared with FOLLOW — a single call gives you both.)

```csharp
var parser = new LALRParser(grammar);

foreach (var item in parser.GetFirstAndFollow())   // FirstAndFollowCollection
{
    Console.WriteLine($"{item.Symbol}");
    Console.WriteLine($"   FIRST  = {item.First}");
    Console.WriteLine($"   FOLLOW = {item.Follow}");
}
```

The very sets you worked out by hand in [Calculation rules](first-rules.md) get printed exactly.\
So let's go in and see how this FIRST is built *internally*.

## First — the code comes in two kinds: **calculator** and **getter**

When you first open the code, the many methods named `First…` can be confusing.\
But really they split into just **two kinds**.

- **`FirstSet(...)` = calculator.**\
  It recursively *computes FIRST directly* and fills it into the cache (`_cache`). All the real algorithm lives here.
- **`First(...)` = getter.**\
  It only *looks up or assembles* results from the cache once the computation is done.

And both have **overloads by argument type** — because FIRST is defined for all three: *a single symbol (`Symbol`)*, *a sequence of symbols (`Concat`)*, and *a specific production (`Single`)*.

So all we need to follow is the **calculator `FirstSet`**.\
The *three cases + ⊕ + iteration* we saw earlier are all in there.

## Where to keep it — `_cache` and `_bChanged`

```csharp
private bool _bChanged = false;                                    // "did anything grow this round?" (for ending the iteration)
private Dictionary<NonTerminalSingle, TerminalSet> _cache = new(); // FIRST stored per production (filled up gradually)
```

`_cache` is the ledger that gathers the FIRST of *each individual production ([Single](deep-single.md))*.\
`_bChanged` is the flag that remembers *"did anything grow this one round"*. (It decides when to stop iterating.)

## Calculator (1) — FIRST of a single symbol: `FirstSet(Symbol)`

This is where **case ① (terminal) · case ② (nonterminal)** live, exactly as written.

```csharp
public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null)
{
    if (symbol is Terminal)                                       // ── case ① : if it's a terminal
        return new TerminalSet(symbol as Terminal);               //    return a set holding just itself

    TerminalSet result = new TerminalSet();                       // ── case ② : if it's a nonterminal
    foreach (NonTerminalSingle singleNT in symbol as NonTerminal) //    loop over all productions
    {
        … (left-recursion guard — below) …
        result.UnionWith(FirstSet(singleNT, seenNT));             //    take the FIRST of each production
        result.UnionWith(_cache[singleNT]);                       //    and union them all
    }
    return result;
}
```

- **If the symbol is a terminal**, it returns a set holding just itself, as is → **case ①** exactly.
- **If the symbol is a nonterminal**, it **loops over all of that nonterminal's productions with `foreach`** and **unions** each FIRST.\
  → **case ②** + *"the FIRST of a nonterminal is the union of the FIRST of all its productions"*, exactly.\
  (Remember how `foreach` over a `NonTerminal` yields the productions ([Single](deep-single.md)) one by one?)

## Calculator (2) — FIRST of a production (sequence): `FirstSet(Concat)` = ⊕

A production is, after all, a *sequence* of symbols (`Term '*' Factor`).\
Its FIRST is — as we concluded in the previous chapter — the **⊕ (ring sum)** of the symbols' FIRST.

```csharp
public TerminalSet FirstSet(NonTerminalConcat singleNT, ...)
{
    TerminalSet result = new TerminalSet();
    foreach (var symbol in singleNT)                        // each symbol, in order
    {
        result = result.RingSum(FirstSet(symbol, seenNT));  // ⊕ one slot
        if (!result.IsNullAble) break;                      // no more ε to look at → stop
    }
    return result;
}
```

`RingSum` is exactly ⊕. The definition is 1:1, so **all three cases are inside it.**

```csharp
// TerminalSet.RingSum
if (result.IsNull)            result.UnionWith(param);   //  ∅ ⊕ B = B
else if (result.IsNullAble) { result.Remove(ε);          //  if it has ε (= can vanish), drop ε
                              result.UnionWith(param); }  //          and also add the next slot B   → case ③
// otherwise leave it as is = A   (don't look at B)                       → cases ①·②
```

- `IsNullAble` = *"does it contain ε"* (`Contains(Epsilon)`), i.e. the **nullable test**.
- `if (!result.IsNullAble) break;` = *"if the front can't vanish, stop right there"* → **cases ①·②** stop here.
- If there's an ε, it doesn't `break` and moves on to the next slot → **case ③**.

So **all three cases are inside this one loop, and it's just a matter of where ⊕ stops.**

## So it doesn't blow up on left recursion — the guard

In **case ②** we saw left recursion like `Term → Term '*' …`.\
If you recurse naively, `FirstSet(Term)` → `FirstSet(Term)` → … is an infinite loop.\
So inside `FirstSet(Symbol)` there are two guard lines.

```csharp
if (seenNT.Contains(singleNT)) { result.UnionWith(_cache[singleNT]); … }  // production already seen?
if (singleNT[0] == symbol)     { result.UnionWith(_cache[singleNT]); … }  // is the front itself? (left recursion)
```

If the production was *already visited* or the *front is itself*, it doesn't recurse further and only pulls in **the cached value accumulated so far**.\
It breaks the infinite loop, and the iteration (next section) fills in the rest as it goes one more round.\
The previous chapter's *"add the current value of the front `Term`"* is exactly these two lines.

## The whole thing — iterate until nothing changes: `CalculateAllFirst`

```csharp
public void CalculateAllFirst(HashSet<NonTerminal> nonTerminals)
{
    do
    {
        _bChanged = false;
        foreach (var nonTerminal in nonTerminals) FirstSet(nonTerminal);
    }
    while (_bChanged);     // if a full round adds nothing → the answer
}
```

`do { _bChanged = false; … } while(_bChanged)` — this is the previous chapter's **fixpoint iteration** itself.\
If the cache grows even a little inside `FirstSet`, it leaves `_bChanged` on, and when a round goes by with no change, it stops.

## Getting the result — `First(...)`

Once the computation (`CalculateAllFirst`) is done, you now read the result through the **getter** side, `First(...)`.

```csharp
public TerminalSet First(NonTerminalSingle key) => _cache[key];   // look up directly from the cache
public TerminalSet First(NonTerminalConcat concat);               // assemble known FIRSTs with ⊕
public TerminalSet First(Symbol key);                             // if terminal {self}, if nonterminal gather from the cache
```

The very first public API we saw, `GetFirstAndFollow()`, internally calls exactly these `First(...)` methods to build and return a `FirstAndFollowCollection`.

## Formula ↔ code at a glance

| Calculation rule | Code |
|---|---|
| Case ① — starts with a terminal | `if (symbol is Terminal) return new TerminalSet(...)` |
| Case ② — starts with a nonterminal (+ union) | `foreach (singleNT in NonTerminal) result.UnionWith(FirstSet(singleNT))` |
| Case ③ — ε / ⊕ | `result.RingSum(...)` + `if (!IsNullAble) break;` |
| Left-recursion guard | `if (singleNT[0] == symbol) …` |
| Fixpoint iteration | `do { _bChanged=false; … } while(_bChanged)` |
| Compute vs look up | `FirstSet(...)` computes / `First(...)` gets |

> 📌 The deep-dive track always pairs up **"rule ↔ our code"** like this.

## Following along with an example

Let's start with `FIRST(Factor)`. `Factor : '(' Expr ')' | id`.

- Production 1 `'(' Expr ')'` → `FirstSet(Concat)`: the first symbol `'('` is a terminal → `RingSum` result `{ '(' }`, no ε → **immediate break.** → `{ '(' }`
- Production 2 `id` → `{ id }`
- Union the two → **`FIRST(Factor) = { '(', id }`** ✓

`FIRST(Term)` is `Term : Term '*' Factor | Factor`.

- Production `Factor` → `{ '(', id }`
- Production `Term '*' Factor` → the first symbol is `Term` (itself, left recursion) → the guard pulls in the cached value.\
  As the iteration goes one more round and `Term`'s cache fills up to `{ '(', id }`, that flows in.
- Converges → **`FIRST(Term) = { '(', id }`** ✓

`Expr` follows the same flow to **`{ '(', id }`**.\
Exactly the same answer you got on the [Calculation rules page](first-rules.md) · [Definition and derivation page](first-formula.md). ✓

## At a glance — the whole FIRST-related spec

This is the FIRST-side skeleton of `FirstFollowAnalyzer`.\
The logic is left out and it only shows *what's there*. (You can see it splits into the calculator `FirstSet` / getter `First`.)

```csharp
public partial class FirstFollowAnalyzer
{
    private bool _bChanged;
    private Dictionary<NonTerminalSingle, TerminalSet> _cache;

    // ── getter (look up / assemble the finished result) ─────
    public TerminalSet First(NonTerminalSingle key);     // get from the cache
    public TerminalSet First(NonTerminalConcat concat);  // FIRST of a sequence (assembled with ⊕)
    public TerminalSet First(Symbol key);

    // ── calculator (recursion + ⊕ + left-recursion guard) ───────
    public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null);
    public TerminalSet FirstSet(NonTerminalConcat singleNT, HashSet<NonTerminalSingle> seenNT = null);

    // ── whole-grammar fixpoint iteration ─────────────────────
    public void CalculateAllFirst(HashSet<NonTerminal> nonTerminals);
}
```

The helper type `TerminalSet : HashSet<Terminal>` carries `IsNull` (empty set) · `IsNullAble` (contains ε) · `RingSum` (⊕).

## Next chapter

We've gone a full round through FIRST — **definition · derivation · calculation rules · code**.\
That's the answer to **"which token does it *start* with"**.

Now for its partner — **"which token comes *after* this"**, i.e. **FOLLOW**.\
Because FOLLOW uses FIRST *as an ingredient* (the first line of `CalculateAllFollow` is `CalculateAllFirst`), what we just built carries straight over.

👉 **[FOLLOW · Definition and derivation](follow-formula.md)**
