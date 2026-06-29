# Closure · Implementation (code)

> 🎓 This is an **advanced track** chapter.\
> Earlier we saw the [definition](closure-def.md) of closure (the smallest closed set) and its [computation method](closure-calc.md) (one
> step at a time until it closes).\
> This time, let's see how that went into the `Analyzer.Closure` code, **almost line by line.**

## Code — `Analyzer.Closure`

```csharp
public static CanonicalState Closure(CanonicalState iStatus, HashSet<NonTerminal> exploredSet = null)
{
    if (exploredSet == null) exploredSet = new HashSet<NonTerminal>();
    var result = new CanonicalState();

    foreach (var item in iStatus)
    {
        result.Add(item);                               // ① embrace I's items as they are

        if (item.MarkSymbol == null)        continue;   // dot at the end  → nothing to expand
        if (item.MarkSymbol is Terminal)    continue;   // terminal after the dot → nothing to expand

        NonTerminal B = item.MarkSymbol as NonTerminal; // nonterminal B after the dot
        if (exploredSet.Contains(B)) continue;          // B already expanded → skip (don't add again)
        exploredSet.Add(B);

        CanonicalState param = new CanonicalState();
        foreach (NonTerminalSingle single in B)         // ② take every production of B
            param.Add(new LRItem(single));              //    as dot-at-the-front (B → •γ)

        result.UnionWith(Analyzer.Closure(param, exploredSet));   // if there's more to expand, recurse
    }
    return result;
}
```

## Definition · computation method ↔ code, line by line

Inside this short function, the previous two pages sit exactly as they were.

- **`result.Add(item)`** → this is **①** of the definition ("embrace `I`'s items as they are").
- **`continue` when `item.MarkSymbol == null` / `is Terminal`** → *if what's after the dot is a terminal or it's the end, there's nothing to expand.* (This is **step 3** of the [computation method](closure-calc.md) — that part where we stopped at a terminal like `(` or `id`.)
- **nonterminal `B` after the dot → `foreach (single in B) param.Add(new LRItem(single))`** → this is **②** of the definition ("add every production of `B` as `B → •γ`"). `new LRItem(single)` is an item with the *dot at the very front* (the default constructor of [LR item](lr-item.md), `markIndex = 0`).
- **`exploredSet`** → the computation method's *"don't expand a nonterminal you've already expanded again."* Once `Expr` has been expanded once, it blocks it from being expanded again, breaking the **infinite loop.**
- **`result.UnionWith(Closure(param, exploredSet))`** → this accomplishes *"until it closes"* through **recursion** (a function calling itself again).\
  If what's after the dot of the newly added `B → •γ` items is *also* a nonterminal, that recursion goes on to expand it. (This is that flow where the computation method moved from step 1 → step 2.)

> 💡 In the [computation method](closure-calc.md) we said *"repeat one step at a time,"* but the code uses **recursion** rather than a `do…while` loop, right?\
> The two do the same thing — recursion "follows the nonterminals after the dot inward" and expands until it closes, and `exploredSet` guarantees that *each nonterminal is expanded exactly once*, so in the end it settles into a single *smallest closed set.*

## 📐 The author's closure design diagram

- Closure algorithm — <https://www.lucidchart.com/documents/edit/515ff26b-2649-4150-86ec-80288ef51570/0>

> (Since these are the author's own notes, you may need access permission.)

## Next chapter

We've now seen **how to fill one state completely** with closure — all the way from definition to computation to implementation.

Now it's **GOTO**'s turn: deciding which state you go to **when you read one symbol** from that state.\
(And GOTO too uses *this closure again at the very end* — that's why the state you newly arrive at is also completed with no gaps. You'll see it soon.)

👉 **[GOTO — read one symbol and move to the next state](goto.md)**

---

👈 Previously: [Closure · How to compute](closure-calc.md)
