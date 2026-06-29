# FOLLOW · Implementation (code)

> 🎓 This is the **advanced track · implementation**.\
> In the previous [FOLLOW · Computation rules](follow-rules.md), we saw the three rules (① start symbol `$` / ② the `FIRST` of what follows / ③ inherit the LHS when at the end) and the iteration.\
> This time we'll follow how that went into the `FirstFollowAnalyzer` code, **almost line by line.**

## Try it — the public API (one line)

It's the *same entrance* as FIRST.\
A single call gives you FIRST and FOLLOW together.

```csharp
var parser = new LALRParser(grammar);

foreach (var item in parser.GetFirstAndFollow())
{
    Console.WriteLine($"{item.Symbol}");
    Console.WriteLine($"   FOLLOW = {item.Follow}");
}
```

## Where to store it — `Datas`

```csharp
public RelationData Datas { get; private set; } = new();   // stores FOLLOW per nonterminal
```

This is the **per-nonterminal FOLLOW ledger**, corresponding to FIRST's `_cache`.\
Once the computation is done, we pull the results out of here.

## ① · ② initial values — `InitFollowSet`

Rule ① (start symbol `$`) and rule ② (the `FIRST − ε` of what comes right after) both live in this one method.

```csharp
public TerminalSet InitFollowSet(NonTerminal nonTerminal, HashSet<NonTerminal> nonTerminalSet)
{
    TerminalSet result = new TerminalSet();
    if (nonTerminal.IsStartSymbol) result.Add(new EndMarker());   // ── rule ① : if it's the start symbol, $

    foreach (var symbol in GetFollowSymbols(nonTerminalSet, nonTerminal))  // ── rule ② : the symbols right after
    {
        var firstSet = FirstSet(symbol);                          //    the FIRST of what follows
        firstSet.ExceptWith(new TerminalSet(new Epsilon()));      //    drop ε
        result.UnionWith(firstSet);                               //    add it to FOLLOW
    }
    return result;
}
```

- `if (nonTerminal.IsStartSymbol) result.Add(new EndMarker())` → this is **rule ①**. (`$` is `EndMarker` in the code.)
- Once `GetFollowSymbols(...)` gathers *the symbols that come right after `B`*, we drop `ε` from each one's `FirstSet` and merge them → **rule ②**.\
  (Here `FirstSet` calls the [FIRST calculator](first-impl.md) directly. That's why FIRST has to be completely finished before FOLLOW.)

### Finding the symbol right after — `FindNextSymbolSet`

`GetFollowSymbols` scans the whole grammar and collects the results of `FindNextSymbolSet`.\
This is the code that finds *"right after `B`."*

```csharp
foreach (var symbol in singleNT)
{
    if (bFind)                                       // if we've already passed B
    {
        result.Add(symbol);                          //    take the symbol after it
        if (!FirstSet(symbol).IsNullAble) break;     //    stop if it can't become ε
    }
    else if (symbol == findSymbol) bFind = true;     // here we found B
}
```

From the point we meet `B` (`findSymbol`), we collect symbols, but the moment we hit a symbol that *cannot disappear (not ε)* we stop.\
This is exactly the *"FIRST of β"* gathering from computation rule ②.

## ③ inherit, repeatedly — `ConCatExprUpdateFollow`

Rule ③ (*"if it's at the very end, inherit the LHS's FOLLOW"*) is here.

```csharp
private bool ConCatExprUpdateFollow(NonTerminalSingle contents, TerminalSet followSet)
{
    for (int i = contents.Count - 1; i >= 0; i--)            // ← from the very end (right) toward the left
    {
        var symbol = contents[i];
        if (symbol is Terminal) break;                       // stop at a terminal (terminals have no FOLLOW)

        this.Datas[symbol as NonTerminal].UnionWith(followSet);  // pour in the LHS's FOLLOW
        …
        if (!FirstSet(symbol).IsNullAble) break;             // stop if this nonterminal can't become ε
    }
    …
}
```

`followSet` is exactly the FOLLOW of the LHS (`A`).\
Walking the production **from the very end (right)**, we pour `FOLLOW(A)` into the nonterminal at the end → **rule ③**.\
If that nonterminal *can disappear (ε)*, we keep pouring into the nonterminal before it as well; if it can't disappear, we stop — the computation rule's
*"if β disappears, go further back"* is exactly this one line, `if (!IsNullAble) break;`.

(`UpdateFollow` is the wrapper that runs the above over *every production* of a single nonterminal.)

## The full driver — `CalculateAllFollow`

```csharp
public void CalculateAllFollow(HashSet<NonTerminal> nonTerminals)
{
    CalculateAllFirst(nonTerminals);                          // ← FIRST first (because rule ② uses FIRST)

    foreach (var nt in nonTerminals)
        Datas.Add(nt, InitFollowSet(nt, nonTerminals));       // ← initial values from rules ①·②

    do
    {
        bChange = false;
        foreach (var d in Datas)
            if (UpdateFollow(d.Key, d.Value)) bChange = true;  // ← rule ③ inheritance
    }
    while (bChange);                                          //    until nothing changes (fixpoint)
}
```

The computation rules are here **in exactly the same order** — **FIRST first → initial values from rules ①·② → repeat rule ③ until nothing changes.**\
The first line, `CalculateAllFirst`, nails down in code the fact that *"FOLLOW uses FIRST as its ingredient."*

## Reading out the result — `Follow`

```csharp
public TerminalSet Follow(NonTerminal nonTerminal) => this.Datas[nonTerminal];   // just a lookup
```

It **only reads out** from the finished `Datas`.\
The public API `GetFirstAndFollow()` calls this and fills in `item.Follow`.

## Formula ↔ code at a glance

| Computation rule | Code |
|---|---|
| Rule ① — start symbol `$` | `if (IsStartSymbol) result.Add(new EndMarker())` |
| Rule ② — the `FIRST − ε` of what follows | `GetFollowSymbols` + `FirstSet(symbol).ExceptWith(ε)` |
| Rule ③ — inherit the LHS when at the end | `ConCatExprUpdateFollow` (`UnionWith(followSet)` from the right) |
| FIRST as the ingredient (first) | `CalculateAllFollow`'s first line `CalculateAllFirst` |
| Fixpoint iteration | `do { … } while(bChange)` |

## Following it with the example

Run `CalculateAllFollow` on our grammar — and it flows exactly the way we did it by hand in the [computation rules](follow-rules.md).

- **`CalculateAllFirst`** first → the `FIRST` sets get filled in.
- **`InitFollowSet`** (①②) → `FOLLOW(Expr)={$,'+',')'}`, `FOLLOW(Term)={'*'}`, `FOLLOW(Factor)={}`.
- **`do…while`** (③) → it propagates as `Term ⊇ FOLLOW(Expr)`, `Factor ⊇ FOLLOW(Term)`, so both become `{$,'+',')','*'}`.
- When running it more produces no change, `bChange = false` → stop. ✓\
  (How *many rounds* the propagation takes splits into one or two depending on the processing order of `Datas`, but the values after it stops are the same in any order.)

## At a glance — the full FOLLOW-related spec

This is the FOLLOW-side skeleton of `FirstFollowAnalyzer`.\
The logic is emptied out, showing only *what's there*.

```csharp
public partial class FirstFollowAnalyzer   // (FOLLOW side)
{
    public RelationData Datas { get; }

    // ── read out ───────────────────────────────
    public TerminalSet Follow(NonTerminal nonTerminal);   // lookup in Datas

    // ── compute ─────────────────────────────────
    public TerminalSet InitFollowSet(NonTerminal nt, HashSet<NonTerminal> all);     // rules ①·②
    public SymbolSet   GetFollowSymbols(HashSet<NonTerminal> all, NonTerminal nt);  // the symbols right after B
    public void        CalculateAllFollow(HashSet<NonTerminal> nonTerminals);       // FIRST first + initial values + repeat
    // (private) UpdateFollow · ConCatExprUpdateFollow · FindNextSymbolSet — rule ③ + "finding what's after"
}
```

## Next chapter

We've finished FIRST and FOLLOW all the way through — **definition · derivation · computation rules · code.**\
These two are exactly the core ingredients that build the **parse table**.

Next come the **LR item**, which expresses *"how far into this rule the LR parser has read so far,"* and the **canonical collection (the states)** —
those come together and finally become that famous **parse table**.

👉 **[LR item](lr-item.md)**

---

👈 Previously: [FOLLOW · Computation rules](follow-rules.md)
