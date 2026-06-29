# FIRST · Implementation (code)

> 🎓 This is the **Advanced track · Implementation**.\
> In the previous page, [FIRST · Computation Rules](first-rules.md), we saw the three cases (terminal / nonterminal / ε) and the repetition as a *rule*.\
> This time we trace how that rule went into the `FirstFollowAnalyzer` code **almost line for line.**\
> (If you haven't read from [Definition & Derivation](first-formula.md), I recommend starting there.)

## Trying it out — the public API (one line)

Before going into the details, let's start with *how you actually use it*.\
From the parser, **one line** gives you the FIRST/FOLLOW of every symbol. (This entrance is shared with FOLLOW — a single call gives you both.)

```csharp
var parser = new LALRParser(grammar);

foreach (var item in parser.GetFirstAndFollow())   // FirstAndFollowCollection
{
    Console.WriteLine($"{item.Symbol}");
    Console.WriteLine($"   FIRST  = {item.First}");
    Console.WriteLine($"   FOLLOW = {item.Follow}");
}
```

The very sets we worked out by hand in [Computation Rules](first-rules.md) get printed out as is.\
So let's go inside and see how this FIRST is built *internally*.

## First — the code is two kinds: **the calculator** and **the getter**

When you first open the code, there are several methods called `First…`, which can be confusing.\
But really they split into exactly **two kinds**.

- **`FirstSet(...)` = the calculator.**\
  It computes FIRST *directly* via recursion and fills it into a cache (`_cache`). The real algorithm is all here.
- **`First(...)` = the getter.**\
  Once the computation is done, it just *looks up or assembles* the result from the cache.

And both have **overloads by argument type** — because FIRST is defined for all three of *a single symbol (`Symbol`)*, *a sequence of symbols (`Concat`)*, and *a specific production (`Single`)*.

So we only have to follow **the calculator `FirstSet`**.\
The *three cases + ⊕ + repetition* we saw earlier are all in there.

## Where to keep it — `_cache` and `_bChanged`

```csharp
private bool _bChanged = false;                                    // "did anything grow this round?" (for ending the loop)
private Dictionary<NonTerminalSingle, TerminalSet> _cache = new(); // FIRST stored per production (filled in gradually)
```

`_cache` is the ledger that collects the FIRST of *each individual production ([Single](deep-single.md))*.\
`_bChanged` is the flag that remembers *"did anything grow this round."* (It decides when to stop the repetition.)

## The calculator (1) — FIRST of a single symbol: `FirstSet(Symbol)`

This is where **Case ① (terminal) · Case ② (nonterminal)** sit, as is.

```csharp
public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null)
{
    if (symbol is Terminal)                                       // ── Case ① : if terminal
        return new TerminalSet(symbol as Terminal);               //    return a set holding just itself

    TerminalSet result = new TerminalSet();                       // ── Case ② : if nonterminal
    foreach (NonTerminalSingle singleNT in symbol as NonTerminal) //    go through every production
    {
        … (left-recursion guard — below) …
        result.UnionWith(FirstSet(singleNT, seenNT));             //    union the FIRST
        result.UnionWith(_cache[singleNT]);                       //    of each production
    }
    return result;
}
```

- **If the symbol is a terminal**, it returns a set holding just itself, as is → exactly **Case ①**.
- **If the symbol is a nonterminal**, it goes through **every production of that nonterminal with `foreach`** and **unions** each FIRST.\
  → exactly **Case ②** + *"a nonterminal's FIRST is the union of all its productions' FIRST."*\
  (Remember how `foreach`-ing a `NonTerminal` yields its productions ([Single](deep-single.md)) one at a time?)

## The calculator (2) — FIRST of a production (sequence): `FirstSet(Concat)` = ⊕

A production is, after all, a *sequence* of symbols (`Term '*' Factor`).\
Its FIRST is — per the previous chapter's conclusion — the **⊕ (ring-sum)** of the symbols' FIRST.

```csharp
public TerminalSet FirstSet(NonTerminalConcat singleNT, ...)
{
    TerminalSet result = new TerminalSet();
    foreach (var symbol in singleNT)                        // the symbols in order
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
else if (result.IsNullAble) { result.Remove(ε);          //  if it has ε (= can disappear), drop ε
                              result.UnionWith(param); }  //          and add the next slot B too  → Case ③
// otherwise leave it = A   (don't look at B)              → Cases ①·②
```

- `IsNullAble` = *"does it hold ε"* (`Contains(Epsilon)`), i.e. the **nullable check**.
- `if (!result.IsNullAble) break;` = *"if the front can't disappear, it ends there"* → **Cases ①·②** stop here.
- if it has ε, it doesn't `break` and moves to the next slot → **Case ③**.

That is, **all three cases are inside this one loop — it's just a matter of where ⊕ stops.**

## So it doesn't blow up on left recursion — the guard

In **Case ②** we saw left recursion like `Term → Term '*' …`.\
Recurse naively and it's `FirstSet(Term)` → `FirstSet(Term)` → … an infinite loop.\
So there's a two-line guard inside `FirstSet(Symbol)`.

```csharp
if (seenNT.Contains(singleNT)) { result.UnionWith(_cache[singleNT]); … }  // already-seen production?
if (singleNT[0] == symbol)     { result.UnionWith(_cache[singleNT]); … }  // front is itself? (left recursion)
```

If it's an *already-visited* production or the *front is itself*, it doesn't recurse further and just brings in **the cache value accumulated so far.**\
This breaks the infinite loop, while the repetition (next section) fills in the rest on another round.\
The previous chapter's *"add the so-far value of the front `Term`"* is exactly these two lines.

## The whole thing — repeat until nothing changes: `CalculateAllFirst`

```csharp
public void CalculateAllFirst(HashSet<NonTerminal> nonTerminals)
{
    do
    {
        _bChanged = false;
        foreach (var nonTerminal in nonTerminals) FirstSet(nonTerminal);
    }
    while (_bChanged);     // a full round with no growth → the answer
}
```

`do { _bChanged = false; … } while(_bChanged)` — that's the previous chapter's **fixed-point iteration** itself.\
If the cache grows even a little inside `FirstSet`, it keeps `_bChanged` on, and when there's no change for a full round, it stops.

## Getting the result out — `First(...)`

Once the computation (`CalculateAllFirst`) is done, you now read the result via the **getter** side, `First(...)`.

```csharp
public TerminalSet First(NonTerminalSingle key) => _cache[key];   // looks straight up in the cache
public TerminalSet First(NonTerminalConcat concat);               // assembles known FIRSTs with ⊕
public TerminalSet First(Symbol key);                             // {itself} if terminal, gather from cache if nonterminal
```

The public API `GetFirstAndFollow()` we saw at the very start calls these `First(...)` inside, builds a
`FirstAndFollowCollection`, and returns it.

## Formula ↔ code at a glance

| Computation rule | Code |
|---|---|
| Case ① — starts with terminal | `if (symbol is Terminal) return new TerminalSet(...)` |
| Case ② — starts with nonterminal (+ union) | `foreach (singleNT in NonTerminal) result.UnionWith(FirstSet(singleNT))` |
| Case ③ — ε / ⊕ | `result.RingSum(...)` + `if (!IsNullAble) break;` |
| left-recursion guard | `if (singleNT[0] == symbol) …` |
| fixed-point iteration | `do { _bChanged=false; … } while(_bChanged)` |
| compute vs look up | `FirstSet(...)` computes / `First(...)` gets out |

> 📌 The Advanced track always pairs up **"rule ↔ our code"** like this.

## Following along with the example

Starting with `FIRST(Factor)`. `Factor : '(' Expr ')' | id`.

- production 1 `'(' Expr ')'` → `FirstSet(Concat)`: the first symbol `'('` is a terminal → `RingSum` result `{ '(' }`, no ε → **break immediately.** → `{ '(' }`
- production 2 `id` → `{ id }`
- combine the two → **`FIRST(Factor) = { '(', id }`** ✓

`FIRST(Term)` is `Term : Term '*' Factor | Factor`.

- production `Factor` → `{ '(', id }`
- production `Term '*' Factor` → the first symbol is `Term` (itself, left recursion) → the guard brings in the cache value.\
  As the repetition does one more round and `Term`'s cache fills up to `{ '(', id }`, that flows in.
- converge → **`FIRST(Term) = { '(', id }`** ✓

`Expr` too, by the same flow, is **`{ '(', id }`**.\
Exactly the same as the answer worked out on the [Computation Rules page](first-rules.md) and the [Definition & Derivation page](first-formula.md). ✓

## At a glance — the whole FIRST-related spec

This is the skeleton of the FIRST side of `FirstFollowAnalyzer`.\
The logic is emptied out, showing only *what's there*. (You can see it's split into the calculator `FirstSet` / the getter `First`.)

```csharp
public partial class FirstFollowAnalyzer
{
    private bool _bChanged;
    private Dictionary<NonTerminalSingle, TerminalSet> _cache;

    // ── getter (look up / assemble the finished result) ─────
    public TerminalSet First(NonTerminalSingle key);     // get out of the cache
    public TerminalSet First(NonTerminalConcat concat);  // FIRST of a sequence (assembled with ⊕)
    public TerminalSet First(Symbol key);

    // ── calculator (recursion + ⊕ + left-recursion guard) ───────
    public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null);
    public TerminalSet FirstSet(NonTerminalConcat singleNT, HashSet<NonTerminalSingle> seenNT = null);

    // ── whole-grammar fixed-point iteration ─────────────────────
    public void CalculateAllFirst(HashSet<NonTerminal> nonTerminals);
}
```

The helper type `TerminalSet : HashSet<Terminal>` holds `IsNull` (empty set) · `IsNullAble` (contains ε) · `RingSum` (⊕).

## Next chapter

We've gone a full lap around FIRST — **definition · derivation · computation rules · code.**\
That's the answer to **"which token does it *start* with."**

Now its partner — **"which token comes *after* this,"** that is, **FOLLOW**.\
Because FOLLOW uses FIRST as *raw material* (the first line of `CalculateAllFollow` is `CalculateAllFirst`), what we just built carries straight over.

👉 **[FOLLOW · Definition & Derivation](follow-formula.md)**
