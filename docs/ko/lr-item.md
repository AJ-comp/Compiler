# LR 아이템 — "어디까지 읽었나" 를 점으로

> 🎓 **심화 과정** 이에요.\
> 지금까지 **문법 구조**([Symbol](deep-symbols.md) ~ [Alter](deep-alter.md))와 **FIRST/FOLLOW** 를 다
> 봤죠. 이제 그걸 *재료* 로 — 드디어 **LR 파서를 만들기** 시작해요.\
> 그 첫 벽돌이 **LR 아이템** 입니다.

## 무엇을 표시하려는 걸까

LR 파서는 입력을 **왼쪽부터 한 토큰씩** 읽어 나가요.\
읽다 보면 *"지금 어떤 생성규칙을, 어디까지 읽었나"* 를 계속 기억해야 해요.

그 "어디까지" 를 표시하는 가장 단순한 방법 — 생성규칙 한가운데에 **점(`•`)** 을 하나 찍는 거예요.

## 정의 — LR 아이템이란

> **LR 아이템** = 생성규칙 하나에 **점(`•`) 을 한 군데 찍은 것.**\
> `A → α • β` 꼴이에요. **점 앞(α)** 은 *이미 읽은 부분*, **점 뒤(β)** 는 *아직 읽을 부분* 이죠.

예를 들어 생성규칙 `Expr → Expr '+' Term` 하나에, 점을 찍을 수 있는 자리는 이래요.

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      아직 아무것도 안 읽음
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">'+'</span> <span class="nt">Term</span>      Expr 까지 읽음
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="lrdot">•</span> <span class="nt">Term</span>      '+' 까지 읽음
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> <span class="lrdot">•</span>      다 읽음 — 이제 이 규칙으로 묶을(reduce) 차례!</pre>

**같은 생성규칙이라도 점 위치가 다르면 다른 아이템** 이에요. 점이 "진행 상황" 이니까요.

## 코드 — `LRItem` · 저자는 어떻게 표현했나

여기서부턴 다시 **저자의 설계** 가 나와요.\
LR 아이템이라는 *개념* — 생성규칙에 점을 찍어 "어디까지 읽었나" 를 나타내는 것 — 은 컴파일러 교과서의 **표준** 이에요.\
(개념도 이름도 교과서에서 와요. 영어로는 *item*, 한국 교과서에선 흔히 **"항목"** 이라고 불러요.)\
다만 그걸 **코드로 어떻게 담을지** — 생성규칙을 재사용하고 `markIndex` 만 얹는 그 방식 — 은 저자의 선택이거든요.

저자는 아마 이렇게 판단했을 거예요.

> *"LR 아이템은 결국 '생성규칙 하나 + 점 위치' 잖아. 생성규칙은 이미 [Single](deep-single.md) 로 만들어
> 뒀으니 — **새로 만들 것 없이 그걸 가리키고**, 점 위치(`markIndex`) 하나만 얹으면 되겠네."*

그래서 `LRItem` 은 딱 **두 가지** 뿐이에요 — *어느 생성규칙* 인가, 그리고 *점이 어디* 인가.

> 📍 **`LRItem : ICloneable`** · `…/Parsers/Datas/LR/LRItem.cs`

```csharp
public class LRItem : ICloneable
{
    public NonTerminalSingle SingleNT { get; }   // 어느 생성규칙인가  (A → α β)
    private sbyte markIndex = 0;                  // 점이 몇 번째 기호 앞에 있나 (0 = 맨 앞)
}
```

- `SingleNT` — 바로 [Single](deep-single.md) 장에서 본 그 **생성규칙(`NonTerminalSingle`)** 그대로예요.\
  (LR 아이템은 생성규칙을 *새로 만들지 않고* 그대로 가리켜요.)
- `markIndex` — 점의 위치. `markIndex` 개의 기호가 점 앞(α)에 있는 거예요.

### 점 앞 / 점 뒤 — 저자가 미리 심어둔 씨앗

[Concat](deep-concat.md) 장에서 `PrevSymbolListFrom`/`PostSymbolListFrom` 를 보며 *"이건 나중에 LR
파싱의 점 앞/뒤에서 쓰여요"* 라고 했던 거, 기억나세요?\
사실 저자는 — **LR 아이템에서 점 앞뒤(α/β)를 쪼갤 걸 미리 내다보고** [Concat](deep-concat.md) 에 그
메서드를 심어둔 거예요.\
**여기서 그 씨앗이 자라요.**

```csharp
public NonTerminalConcat SymbolListBeforeMarkSymbol => SingleNT.PrevSymbolListFrom(markIndex);  // α (점 앞)
public NonTerminalConcat SymbolListAfterMarkSymbol  => SingleNT.PostSymbolListFrom(markIndex);  // β (점 뒤)
public Symbol            MarkSymbol                  // 점 바로 뒤의 기호 한 개 (SingleNT[markIndex])
```

`A → α • β` 의 `α` 와 `β` 가, 그때 만든 `Prev/PostSymbolListFrom` 으로 정확히 떨어져 나와요.\
그리고 `MarkSymbol` 은 점 바로 뒤의 기호 한 개 — *다음에 읽을 기호* 예요.

### 점이 끝에 닿으면 = 묶을 차례 (`IsReachedHandle`)

점이 생성규칙의 **맨 끝** 까지 가면, 그 규칙을 *다 읽은* 거예요.\
이때가 바로 **묶을(reduce) 차례** 고요.

```csharp
public bool IsReachedHandle => markIndex >= SingleNT.Count;   // 점이 끝 = 완료(reduce) 아이템
```

이런 *완료 아이템* 을 코드에선 **handle 에 닿았다(reached handle)** 고 불러요. (앞으로 자주 나와요 —
*"이 상태에 완료 아이템이 있으면 reduce"* 처럼요.)

### 점 옮기기 — `MoveMarkSymbol`

기호 하나를 읽으면, 점을 **한 칸 앞으로** 옮겨요. (`A → α • X β` → `A → α X • β`)

```csharp
public void MoveMarkSymbol() { if (this.MarkSymbol != null) this.markIndex++; }
```

(점이 이미 끝이면 — `MarkSymbol` 이 `null` 이라 — 아무것도 안 해요.)

### 글자로 그리면 — `ToString`

`ToString()` 이 점 찍힌 모습을 *그대로* 그려줘요. 우리가 위에서 손으로 그린 그 모양이에요.

```csharp
//  예) markIndex = 2 인 LRItem  →
//      "Expr -> Expr '+'•Term"
```

### 정체성 — "어느 생성규칙의, 점이 어디인가"

두 LR 아이템이 *같은지* 는 — **생성규칙 + 점 위치** 로만 따져요.

```csharp
public override int GetHashCode()
    => Convert.ToInt32(this.SingleNT.GetHashCode().ToString() + this.markIndex.ToString());
```

[Single](deep-single.md) 의 정체성(`UniqueKey + alterIndex`)에 **점 위치(`markIndex`)** 를 한 겹 더 얹은
거예요.\
*"어느 규칙의 몇 번째 대안의, 점이 어디"* 가 곧 LR 아이템의 정체성이죠. (이게 바로 [정준 집합](canonical-set.md) 을
*중복 없이* 만드는 열쇠가 돼요.)

## 한눈에 — LRItem의 전체 모습

```csharp
public class LRItem : ICloneable
{
    public NonTerminalSingle SingleNT { get; }   // 생성규칙 (어느 규칙)
    private sbyte markIndex;                       // 점 위치

    // ── 점 주변 ──────────────────────────────
    public Symbol MarkSymbol      { get; }         // 점 바로 뒤 기호 (다음에 읽을 것)
    public Symbol PrevMarkSymbol  { get; }         // 점 바로 앞 기호
    public NonTerminalConcat SymbolListBeforeMarkSymbol { get; }   // α (점 앞 전부)
    public NonTerminalConcat SymbolListAfterMarkSymbol  { get; }   // β (점 뒤 전부)

    // ── 상태 ─────────────────────────────────
    public bool IsFirst         { get; }           // 점이 맨 앞인가 (markIndex == 0)
    public bool IsReachedHandle { get; }           // 점이 끝인가 = 완료(reduce) 아이템

    // ── 룩어헤드 (뒤에서) ────────────────────
    public TerminalSet Follow    { get; }          // FOLLOW (SLR 용)
    public TerminalSet LookAhead { get; }          // 룩어헤드 (LALR/LR(1) 용)

    // ── 조작 ─────────────────────────────────
    public void   MoveMarkSymbol();                // 점을 한 칸 전진
    public LRItem FirstLRItem();                   // 점을 맨 앞(0)으로
    public LRItem PrevLRItem();                    // 점을 한 칸 뒤로
    public object Clone();

    // ── 정체성 / 표현 ────────────────────────
    public override int    GetHashCode();          // SingleNT 해시 + markIndex
    public override string ToString();             // "Expr -> Expr '+'•Term"
}
```

한 줄로 — **LR 아이템 = 생성규칙([Single](deep-single.md)) + 점 위치(`markIndex`).** 그게 전부예요.

## 다음 장

LR 아이템 하나는 *"이 규칙을 어디까지 읽었나"* 를 표현해요 — 딱 한 규칙의 진행만요.

그런데 파서가 실제로 서는 한 자리에선 — *여러* 아이템이 **동시에** 가능할 수 있어요.\
(예를 들어 "`Term` 을 하나 읽은" 자리는 — `Expr → Term •` 일 수도, `Term → Term • '*' Factor` 일 수도
있죠.)\
그 *동시에 가능한 아이템들의 묶음* 이 바로 **상태(state)** 예요.

👉 **[상태 — LR 아이템들의 집합](lr-state.md)**

---

👈 이전 장: [FOLLOW · 구현](follow-impl.md)
