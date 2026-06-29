# Parse table · LALR — Implementation

In [How It Works](parse-table-lalr.md) we said LALR = *merged CLR*, and that the engine doesn't build CLR wholesale — it **propagates lookahead directly onto the LR(0) states** to get the same result. Let's look at that code.

---

## First — building states, which splits three ways

The code that builds the state collection (`CanonicalRelation`) splits *three ways depending on the method*.

```csharp
if (canonicalType == CanonicalType.C0)            // SLR
    ConstructC0(...);                             //   LR(0) states, reduce uses FOLLOW
else if (canonicalType == CanonicalType.LALRC1)   // LALR   ← the path actually used
    if (ConstructC0(...)) ConvertC0ToC1(...);     //   LR(0) states + lookahead propagation
else if (canonicalType == CanonicalType.C1)       // CLR
    ConstructC1(...);                             //   ← the body is empty ([CLR Implementation](parse-table-clr-impl.md))
```

As we saw in [SLR Implementation](parse-table-slr-impl.md), SLR was done with `ConstructC0` *alone*.\
LALR (`LALRC1`) **starts out exactly the same** — it builds *the same LR(0) states* with `ConstructC0`. (That's why, as said in [How It Works](parse-table-lalr.md), *the state count stays exactly LR(0)*.)

The difference is a single line — **`ConvertC0ToC1`.** This is the heart of LALR.

---

## The heart of LALR — `ConvertC0ToC1`

In [How It Works](parse-table-lalr.md) we said LALR *"uses only the lookahead that actually reaches this state,"* and that it computes that by **lookahead propagation**. The place where that propagation *actually happens* is this method.

```csharp
private void ConvertC0ToC1(RelationData relationData)
{
    CalculateLookAhead();                       // ① compute how lookahead flows between states (propagation)

    foreach (var state in IndexStateDic)        // ② work out FOLLOW for each state ahead of time
        state.Value.CalculateFollow(relationData);

    foreach (var entry in LookAheadTable)       // ③ take the computed 'precise lookahead' and
    {                                           //    attach it to exactly that state's · that item's lookahead
        var state = IndexStateDic[entry.Key.Item1];
        state.GetItem(entry.Key.Item2).LookAhead = entry.Value;
    }
}
```

The three steps carry out *word for word* what [How It Works](parse-table-lalr.md) describes.

1. **`CalculateLookAhead()`** — it traces and computes which symbol flows into *which item of which state*. This is exactly *lookahead propagation*.
2. It does the groundwork per state with `CalculateFollow`, and
3. **it attaches the result — not the *whole* FOLLOW — but *pinpointed* per state and per item as `LookAhead`.**

This way each complete item ends up with *its own precise lookahead*.\
So when deciding the reduce cells, SLR looked at `FOLLOW`, but **LALR looks at this `LookAhead`.** The branch is written that way too.

```csharp
ReduceParameter = ReduceParameter.LalrLookAhead;   // where SLR had .Follow
```

This *one hair's-breadth* difference — **the per-state precise `LookAhead` (just this spot) instead of `FOLLOW` (the whole grammar)** — is the whole of what removes [SLR's spurious conflict](parse-table-slr.md). And it doesn't add a single state.

---

## Wrapping up — the LR story has come full circle

If we retrace the path we've walked:

> reading a grammar → FIRST / FOLLOW → LR items → states → closure → GOTO → canonical collection → parse table → conflicts → **precision (SLR · CLR · LALR)**

With that, the story of **"*how* you build an LR parser"** has come full circle.\
Now you've followed — from start to finish — how a single line of grammar becomes a state, how states become a table, why that table runs *deterministically*, and why *conflicts* arise and how to tame them. 🎉

> You can get your hands on that table *actually running* (the scene of shifting and reducing one symbol at a time as the tree grows) in the basics track's **[Parsing in action with the table](parsing-in-action.md)**. Take the table you *built* in theory, *run* it there, and the circle closes completely.
