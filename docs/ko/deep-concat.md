# Concat — 순서(연접)

> 🎓 **심화 과정** 이에요. 앞 [NonTerminal](deep-nonterminal.md) 장에서, 규칙은 `alters`(대안들)를
> 품는다고 했죠. 이제 그 **대안 하나의 속** 으로 들어가요.

대안 하나는 어떻게 생겼을까요?

`Expr '+' Term` 을 보면 — 기호들이 **순서대로** 늘어서 있죠.\
`Expr` 다음 `+` 다음 `Term`.

## 저자의 고민 — "이 *순서* 를 어떻게 담지?"

저자가 떠올린 답은 단순해요.

> *"순서대로 늘어선 거? 그건 그냥 **리스트** 잖아."*

그래서 `NonTerminalConcat` 은 **Symbol들의 리스트** 예요.\
그게 전부예요.

> 📍 **`NonTerminalConcat : IList<Symbol>`** · `…/RegularGrammar/NonTerminalConcat.cs`

```csharp
public class NonTerminalConcat : IList<Symbol>
{
    protected List<Symbol> _symbols = new();   // [Expr, '+', Term] 처럼 순서대로
}
```

그림으로 보면 이래요.

```
   Expr '+' Term
        │
        ▼
   NonTerminalConcat
     [ Expr ] → [ '+' ] → [ Term ]
       0        1         2
```

말 그대로 칸이 순서대로 있는 리스트죠.\
어렵지 않아요.

## 한 줄에 붙는 꼬리표 — Priority, MeaningUnit

순서만 담으면 끝일까요?\
저자는 두 가지를 더 붙였어요.

**`Priority`** — 우선순위예요.\
나중에 문법에 *충돌* 이 생겼을 때, "어느 쪽을 먼저 칠지" 정하는
기준으로 써요.\
(충돌 이야기는 한참 뒤에 나와요.)

**`MeaningUnit`** — "이 줄이 AST에서 어떤 *의미 단위* 가 되는가" 예요.\
(이것도 나중에 따로 다뤄요.)

```csharp
public uint Priority { get; internal set; }
public MeaningUnit MeaningUnit { get; internal set; }
```

지금은 *"순서(리스트) 말고도, 우선순위·의미 꼬리표가 붙는구나"* 정도면 충분해요.

## 작지만 중요한 둘 — 앞쪽 / 뒤쪽 떼어내기

`NonTerminalConcat` 에는 메서드 두 개가 더 있어요.\
지금은 사소해 보이는데, **나중에 LR 파싱의
심장에서 그대로 쓰여요.**

```csharp
public NonTerminalConcat PrevSymbolListFrom(int index);   // 어떤 위치 *앞쪽* 기호들
public NonTerminalConcat PostSymbolListFrom(int index);   // 어떤 위치 *뒤쪽* 기호들
```

왜 이게 중요하냐면 — LR 파서는 "지금 이 규칙을 *어디까지* 읽었나" 를 **점(`•`)** 으로 표시해요.\
점 **바로 뒤** 의 기호를 `X` 라 하면, 생성규칙은 이런 꼴이 돼요.

```
   A → α • X β       (α = 이미 읽음,  X = 지금 볼 기호,  β = 남음)
```

이때 `X` 의 위치를 `index` 로 주면, 두 메서드가 `α` 와 `β` 를 **구간째** 떼어줘요.\
(코드를 보면
`Prev = _symbols.Take(index)`, `Post = _symbols.Skip(index + 1)` 이에요.)

```
   A → Expr '+' Term          (인덱스   0     1     2)

   X = '+' (인덱스 1) 로 보면   →   A → Expr • '+' Term

   PrevSymbolListFrom(1) = [ Expr ]        ← α : 점 앞 '전부'  (Take(1))
   PostSymbolListFrom(1) = [ Term ]        ← β : X 뒤 '전부'   (Skip(2))
   ( 인덱스 1의 '+' 자신은 어느 쪽에도 안 들어가요 — 지금 '보는' 기호니까 )
```

포인트는 — 이 둘이 *한 기호* 가 아니라 **앞/뒤 구간 전체** 를 돌려준다는 거예요.\
지금은 "이런 게 미리 준비돼 있구나" 정도만 알아두면 돼요.\
이게 살아나는 건 *LR 아이템* 장에서예요.

## 한눈에 — Concat의 전체 모습

`NonTerminalConcat` 의 **전체 골격** 이에요.\
로직은 비우고 *무엇이 있는지* 만 보여줘요.

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

핵심은 딱 하나예요.\
**`NonTerminalConcat` = 순서를 담은 기호 리스트 + 꼬리표 몇 개.**

## 다음 장

`Concat` 으로 *순서(연접)* 를 담는 법을 봤어요.

그런데 — `Expr` 의 "0번째 대안", "1번째 대안" 처럼, **이 순서가 *어느 규칙의 몇 번째 대안인지***
까지 알아야 할 때가 있어요.\
그걸 위해 `Concat` 에 꼬리표를 더 붙인 게 다음 주인공이에요.

👉 **[Single — 한 개의 생성규칙](deep-single.md)**
