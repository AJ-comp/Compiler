# Alter — 택일(대안들의 집합)

> 🎓 **심화 과정** 이에요. 드디어 [NonTerminal](deep-nonterminal.md)이 품고 있던 그 **`alters` 상자**
> 를 열어볼 차례예요. 앞 [Single](deep-single.md) 장까지 오느라 고생하셨어요 — 여기서 조각들이
> 하나로 맞물려요.

[NonTerminal](deep-nonterminal.md) 장에서 우리는 줄곧 *"규칙은 `alters`(대안들의 묶음)를 품는다"*
고 했죠.

그 **`alters` 의 정체** 가 바로 `NonTerminalAlter` 예요.

## 저자의 고민 — "대안 여러 개를 어떻게 담지?"

`Expr` 은 만드는 방법이 둘이었어요.\
`Expr '+' Term` 과 `Term` 이죠.

각 방법은 이미 우리가 아는 `Concat`(순서 담은 리스트)이죠.\
그러면 *"그 Concat들을 여러 개
담는 그릇"* 이 필요해요.

저자의 답은 — **집합(Set)** 이었어요.

```csharp
public class NonTerminalAlter : ISet<NonTerminalConcat>   // ← Concat들의 '집합'
{
    private HashSet<NonTerminalConcat> concatSymbols = new();
}
```

왜 리스트(List)가 아니라 **집합(HashSet)** 일까요? 저자의 생각을 따라가 보면:

> *"대안은 '이 규칙을 만드는 방법들의 모음' 이야. 똑같은 방법이 두 번 들어 있어 봐야 의미가
> 없잖아 — 중복은 하나로 쳐야지. 그러면 List보다 **Set** 이 맞아."*

그래서 `Expr` 의 대안들은 이렇게 담겨요.

```
   Expr : Expr '+' Term | Term ;
                  │
                  ▼
   NonTerminalAlter   (= alters,  Concat들의 집합)
     ├ NonTerminalConcat  [ Expr ] · [ '+' ] · [ Term ]     ← 대안 1
     └ NonTerminalConcat  [ Term ]                           ← 대안 2
```

[NonTerminal](deep-nonterminal.md) 장에서 그렸던 `alters` 그림, 기억나시죠?\
그 그림의 *진짜
타입* 이 이거예요.

## 두 개의 문 — AddAsConcat vs AddAsAlter (★ 연접과 택일이 여기서 갈려요)

이제 앞에서 [Symbol](deep-symbols.md) 장에 잠깐 나왔던 `+`(연접)와 `|`(택일) — 그 둘이 **실제로
무슨 차이** 였는지, 바로 여기서 코드로 드러나요.

`NonTerminalAlter` 에는 대안을 넣는 문이 *두 개* 있어요.

```csharp
public void AddAsConcat(params Symbol[] symbols)
{
    this.Add(new NonTerminalConcat(symbols));               // 통째로 '한 개' 의 대안
}

public void AddAsAlter(params Symbol[] symbols)
{
    foreach (var symbol in symbols)
        this.Add(new NonTerminalConcat(symbol));            // 각각을 '따로따로' 대안으로
}
```

차이가 보이시나요? **같은 `[a, b]` 를 넣어도 결과가 완전히 달라요.**

```
   AddAsConcat(a, b)   →   Alter { [ a · b ] }        ← 대안 1개 (안에 a, b 가 순서대로)

   AddAsAlter(a, b)    →   Alter { [ a ] , [ b ] }    ← 대안 2개 (a 하나, b 하나)
```

그리고 이게 바로 우리가 코드로 문법을 쓸 때의 `+` 와 `|` 예요.

```csharp
a + b   // operator+ →  AddAsConcat(a, b)  →  "a 다음 b"   (한 대안, 순서)
a | b   // operator| →  AddAsAlter(a, b)   →  "a 또는 b"   (두 대안, 택일)
```

> 💡 [Symbol](deep-symbols.md) 장에서 *"`|` 는 '병합' 이 아니라 '택일' 이다"* 라고 했던 게 여기서
> 증명돼요. `AddAsAlter` 는 a 와 b 를 **하나로 합치지 않아요** — `[a]` 와 `[b]` 라는 **별개의
> 대안 두 개로 따로 둬요.** 읽는 쪽은 그중 *하나를 고르는* 거고요. 그래서 *병합(merge)* 이 아니라
> *택일(choose-one)* 이에요.

## 모든 대안에 한꺼번에 — AddSymbols / InsertSymbol

가끔은 *이미 들어 있는 모든 대안* 에 기호를 똑같이 끼워 넣어야 할 때가 있어요.\
그걸 위한
메서드도 있어요.

```csharp
public void AddSymbols(params Symbol[] symbols)    // 모든 대안 '뒤' 에 추가
public void InsertSymbol(int index, params Symbol[] symbols)   // 모든 대안의 특정 위치에 삽입
```

이름이 단수형 `Symbol` 이 아니라 동작이 *"집합 안 모든 Concat 에 broadcast"* 라는 점만 기억해
두세요. (문법을 자동으로 변형할 때 요긴하게 쓰여요.)

## 집합이라서 — 합치고, 빼고, 겹치는지 보고

`NonTerminalAlter` 가 `ISet<NonTerminalConcat>` 이라고 했죠.\
그래서 **집합 연산** 을 통째로 갖고
있어요.

```csharp
public void UnionWith(...);          // 합집합 — 다른 대안들을 끌어와 합치기
public void IntersectWith(...);      // 교집합
public void ExceptWith(...);         // 차집합
public bool SetEquals(...);          // 대안 구성이 똑같은가
// … IsSubsetOf / Overlaps / … ISet 의 전부 …
```

지금 당장은 안 쓰지만 — 나중에 **문법을 정규화하거나 변형** 할 때(예: 자동 생성된 규칙을
기존 규칙에 녹일 때), "대안 집합을 합치고 빼는" 이 연산들이 그대로 동원돼요.\
저자가 굳이 Set을
고른 보람이 여기서 나오죠.

## 작은 편의 하나 — IsInduceEpsilon

대안 중에 **빈 것(ε)** 이 하나라도 있으면 `true` 를 주는 속성도 있어요.

```csharp
public bool IsInduceEpsilon { get; }   // 대안 중 ε(빈 대안)이 있나
```

[FIRST/FOLLOW](first-follow.md) 를 계산할 때 "이 규칙이 빈 문자열을 만들 수 있나?" 가 중요했죠.\
그 판단을 여기서 바로 답해줘요. (조각들이 어떻게 이어지는지 슬슬 보이시죠?)

## 한눈에 — Alter의 전체 모습

`NonTerminalAlter` 의 **전체 골격** 이에요.\
로직은 비우고 *무엇이 있는지* 만 보여줘요.

```csharp
public class NonTerminalAlter : ISet<NonTerminalConcat>
{
    private HashSet<NonTerminalConcat> concatSymbols;   // 대안(Concat)들의 집합

    public int  Count { get; }
    public bool IsInduceEpsilon { get; }                // 대안 중 ε 이 있나

    // ── 대안 넣기 (★ 연접 vs 택일) ───────────
    public void AddAsConcat(params Symbol[] symbols);   // → 대안 1개 (순서대로)   = '+'
    public void AddAsAlter(params Symbol[] symbols);    // → 대안 N개 (따로따로)   = '|'

    // ── 모든 대안에 broadcast ────────────────
    public void AddSymbols(params Symbol[] symbols);
    public void InsertSymbol(int index, params Symbol[] symbols);

    // ── 집합 연산 (ISet) ─────────────────────
    public void UnionWith(...);  public void IntersectWith(...);  public void ExceptWith(...);
    public bool SetEquals(...);  public bool IsSubsetOf(...);     public bool Overlaps(...);
    // … Add / Remove / Contains / GetEnumerator …

    // ── 변환 ────────────────────────────────
    public HashSet<NonTerminal> ToNonTerminalSet();
}
```

한 줄로 — **`Alter` = Concat(대안)들의 *집합*. 그리고 `+`/`|` 가 이 집합에 대안을 *어떻게* 넣을지를
가른다.**

## 조각이 다 맞물렸어요 — 전체 그림

여기까지가 Janglim 문법 구조의 **뼈대 전부** 예요.\
한 장에 모아 볼게요.

```
   Symbol  (추상)
    ├ Terminal              ← 잎(토큰):  더 안 쪼개짐
    └ NonTerminal "Expr"    ← 가지(규칙)
         └ alters : NonTerminalAlter            ← 대안들의 '집합'   (Alter)
              ├ NonTerminalConcat [Expr · '+' · Term]   ┐  각 원소(Concat)를
              └ NonTerminalConcat [Term]                ┘  "Expr의 N번째" 로 보면
                                                            → NonTerminalSingle (생성규칙)
```

- **Symbol** — 모든 기호의 추상 뿌리 (정체성 = `UniqueKey`)
- **Terminal / NonTerminal** — 잎 / 가지
- **Concat** — 한 대안의 *순서(RHS)*
- **Alter** — 대안들의 *집합* (= `alters`)
- **Single** — 집합 속 한 대안을 *"어느 규칙의 몇 번째"* 로 본 *생성규칙*

`+`(연접)은 `Concat` 을, `|`(택일)은 `Alter` 의 대안을 만든다 — 이 한 문장이 전체를 꿰어요.

## 다음 장

문법을 *담는 그릇* 은 다 봤어요.\
이제 이 구조 **위에서** 파서가 실제로 하는 계산으로 넘어가요.

가장 먼저, 규칙이 "어떤 토큰으로 시작할 수 있고(FIRST), 그 뒤엔 무엇이 올 수 있는지(FOLLOW)" 를
구하는 단계예요.\
기본 과정에서 개념으로 만났던 그것을, 이번엔 **공식과 코드** 로요.

👉 **FIRST / FOLLOW — 공식과 구현** *(작성 예정)*
