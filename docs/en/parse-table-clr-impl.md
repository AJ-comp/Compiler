# Parse table · CLR — Implementation

Honestly — **the engine doesn't implement CLR yet.** So this page is short.\
But spelling out *what works and what doesn't* is this manual's promise, so I'll show you *exactly as it is*.

---

## Where CLR sits in the state-building branch

The code that builds the state collection (`CanonicalRelation`) splits into three ways depending on the method, and CLR is this path.

```csharp
else if (canonicalType == CanonicalType.C1)   // CLR
    ConstructC1(...);
```

But the body of that `ConstructC1` is — **completely empty.**

```csharp
private void ConstructC1(NonTerminal virtualStartSymbol, RelationData relationData)
{
    // (empty — LR(1) state building not implemented)
}
```

And so the conflict check in `CLRParser` looks like this, too.

```csharp
public override AmbiguityCheckResult CheckAmbiguity()
    => throw new NotImplementedException();   // CLR — not implemented
```

---

## Why it wasn't built

As we saw in [How It Works](parse-table-clr.md), CLR pays the expensive price of **state explosion**. And the **LALR** we'll see in the next chapter gives *almost the same precision (LR(1)-grade)* without that explosion.

So the engine chose *not to build true CLR, and instead **to reach nearly LR(1) power through LALR***. (This is a common choice for real-world parser generators.) Rather than writing "LR(1) support!" on the surface, it honestly notes that *what actually works is LALR*.

---

## Next

So how does that LALR *"merge" perfect CLR* to get the precision without the explosion?

👉 **[Parse table · LALR — How It Works](parse-table-lalr.md)**
