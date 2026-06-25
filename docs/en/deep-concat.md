# Concat — Sequence (concatenation)

> 🎓 This is a **deep-dive section**. In the previous [NonTerminal](deep-nonterminal.md) chapter, we said a rule
> holds `alters` (alternatives). Now we step **inside one of those alternatives**.

What does a single alternative look like?

Look at `Expr '+' Term` — the symbols are lined up **in order**.\
`Expr`, then `+`, then `Term`.

## The author's dilemma — "How do I hold this *order*?"

The answer the author came up with is simple.

> *"A bunch of things lined up in order? That's just a **list**."*

So `NonTerminalConcat` is a **list of Symbols**.\
That's all it is.

> 📍 **`NonTerminalConcat : IList<Symbol>`** · `…/RegularGrammar/NonTerminalConcat.cs`

```csharp
public class NonTerminalConcat : IList<Symbol>
{
    protected List<Symbol> _symbols = new();   // [Expr, '+', Term] 처럼 순서대로
}
```

As a picture, it looks like this.

```
   Expr '+' Term
        │
        ▼
   NonTerminalConcat
     [ Expr ] → [ '+' ] → [ Term ]
       0        1         2
```

Literally just slots lined up in order, a list.\
Nothing hard about it.

## Tags attached to each line — Priority, MeaningUnit

Is holding the order all there is to it?\
The author attached two more things.

**`Priority`** — the priority.\
Later, when a *conflict* arises in the grammar, this is used as the
criterion for deciding "which side to pick first."\
(We'll get to the conflict story much later.)

**`MeaningUnit`** — "what *meaning unit* this line becomes in the AST."\
(This too is covered separately later.)

```csharp
public uint Priority { get; internal set; }
public MeaningUnit MeaningUnit { get; internal set; }
```

For now, *"besides the order (the list), there are also priority and meaning tags attached"* is enough.

## Two small but important ones — peeling off the front / back

`NonTerminalConcat` has two more methods.\
They look trivial right now, but **later they get used as-is in the
heart of LR parsing.**

```csharp
public NonTerminalConcat PrevSymbolListFrom(int index);   // 어떤 위치 *앞쪽* 기호들
public NonTerminalConcat PostSymbolListFrom(int index);   // 어떤 위치 *뒤쪽* 기호들
```

Here's why this matters — an LR parser marks "how far it has read into this rule right now" with a **dot (`•`)**.\
If we call the symbol **right after** the dot `X`, the production takes this shape.

```
   A → α • X β       (α = 이미 읽음,  X = 지금 볼 기호,  β = 남음)
```

At this point, if you give the position of `X` as `index`, the two methods peel off `α` and `β` **as whole ranges**.\
(Looking at the code,
it's `Prev = _symbols.Take(index)`, `Post = _symbols.Skip(index + 1)`.)

```
   A → Expr '+' Term          (인덱스   0     1     2)

   X = '+' (인덱스 1) 로 보면   →   A → Expr • '+' Term

   PrevSymbolListFrom(1) = [ Expr ]        ← α : 점 앞 '전부'  (Take(1))
   PostSymbolListFrom(1) = [ Term ]        ← β : X 뒤 '전부'   (Skip(2))
   ( 인덱스 1의 '+' 자신은 어느 쪽에도 안 들어가요 — 지금 '보는' 기호니까 )
```

The point is — these two return not *a single symbol* but the **entire front/back range**.\
For now, just knowing "this kind of thing is prepared in advance" is enough.\
This comes alive in the *LR item* chapter.

## At a glance — the full shape of Concat

This is the **full skeleton** of `NonTerminalConcat`.\
The logic is emptied out, showing only *what is there*.

```csharp
public class NonTerminalConcat : IList<Symbol>, ...
{
    protected List<Symbol> _symbols;     // 순서대로 늘어선 기호들

    // ── 꼬리표 ───────────────────────────────
    public uint Priority { get; internal set; }
    public MeaningUnit MeaningUnit { get; internal set; }

    // ── 판단 ─────────────────────────────────
    public bool IsNull { get; }          // 비어 있나
    public bool IsEpsilon { get; }       // 빈 것(ε) 하나뿐인가
    public bool IsAllTerminal { get; }   // 전부 단말인가

    // ── 앞/뒤 떼어내기 (LR 의 점 앞/뒤) ──────
    public NonTerminalConcat PrevSymbolListFrom(int index);
    public NonTerminalConcat PostSymbolListFrom(int index);

    // ── 편집 ─────────────────────────────────
    public void Replace(int index, Symbol item);
    public void AddRange(params Symbol[] symbols);
    public NonTerminalConcat ToReverse();
    // … IList<Symbol> 의 Add / Insert / RemoveAt / this[i] …

    // ── 변환 ─────────────────────────────────
    public HashSet<NonTerminal> ToNonTerminalSet();
    public TerminalSet ToTerminalSet();
}
```

The core is just one thing.\
**`NonTerminalConcat` = a list of symbols holding an order + a few tags.**

## Next chapter

We saw how `Concat` holds the *order (concatenation)*.

But — like `Expr`'s "0th alternative," "1st alternative," sometimes you need to know **which alternative of which rule this order is**.\
The next protagonist is `Concat` with one more tag attached for exactly that.

👉 **[Single — a single production](deep-single.md)**
