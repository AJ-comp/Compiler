# The Parse Table · SLR — Implementation

In [How It Works](parse-table-slr.md) we saw SLR's definition — **LR(0) states + reduce only on FOLLOW(A).**\
So what does this look like in *engine code*? The definition we put into words shows up *astonishingly literally*.

---

## `SLRParser` — the heart is a single constructor line

In the engine, SLR is `SLRParser` — and its heart is a *single line* in the constructor.

```csharp
public SLRParser(Grammar grammar, bool bLogging)
    : base(grammar, CanonicalType.C0, bLogging) { }   // ← C0 = build LR(0) states
```

Passing this `CanonicalType.C0` sends `CanonicalRelation`, which builds the states, down *this branch*.

```csharp
if (canonicalType == CanonicalType.C0)            // ← SLR
{
    ConstructC0(...);                             //  build the LR(0) states (that automaton from the canonical collection chapter)
    ReduceParameter = ReduceParameter.Follow;     //  decide reduce cells by FOLLOW
}
```

---

## Two lines are the whole SLR definition

These exact **two lines** are the *entire* SLR definition from [How It Works](parse-table-slr.md):

> **LR(0) states (`ConstructC0`) + reduce by FOLLOW (`ReduceParameter.Follow`).**

That is, take those LR(0) states built in [the canonical collection](canonical-set.md) *as they are*, and fill in only the reduce cells by FOLLOW.\
The conflict check (`SLRParser.CheckAmbiguity`) likewise weighs the reduce cells per state against the `ReduceParameter.Follow` criterion, and if two land in one cell, it reports a conflict.

The *how it works* we spelled out in words is, in code, *just two lines* — which is also why SLR is called "Simple."

---

## Next

SLR is this simple — *LR(0) states + FOLLOW*. But as we saw in [How It Works](parse-table-slr.md) — that very **FOLLOW** is what creates spurious conflicts. (Which is why the engine keeps SLR only as a *fallback*, and the recommended parser is **LALR**.)

Now let's climb the next rung up the ladder. The key to clearing SLR's spurious conflicts is — *not a global FOLLOW, but a precise lookahead attached to each item.* We'll first see *what that is* (next chapter), then move to **CLR**, which uses it, and on to **LALR**.

👉 **[lookahead (LR(1) items)](parse-table-lookahead.md)**
