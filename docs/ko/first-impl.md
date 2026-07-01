# FIRST · 구현 (코드)

> 🎓 **심화 과정 · 구현** 이에요.\
> 앞 [FIRST · 계산 규칙](first-rules.md)에서 세 경우(단말 / 비단말 / ε)와 반복을 *규칙* 으로 봤죠.\
> 이번엔 그 규칙이 `FirstFollowAnalyzer` 코드에 **거의 한 줄씩** 어떻게 들어갔는지 따라가요.\
> ([정의와 유도](first-formula.md)부터 안 보셨다면 거기부터 추천해요.)

## 써보기 — 공개 API (한 줄)

세부로 들어가기 전에, *실제로 쓰는 법* 부터요.\
파서에서 **한 줄** 이면 모든 심볼의 FIRST/FOLLOW 가 나와요. (이 입구는 FOLLOW 와 공유해요 — 호출 한 번에 둘 다 나오거든요.)

```csharp
var parser = new LALRParser(grammar);

foreach (var item in parser.GetFirstAndFollow())   // FirstAndFollowCollection
{
    Console.WriteLine($"{item.Symbol}");
    Console.WriteLine($"   FIRST  = {item.First}");
    Console.WriteLine($"   FOLLOW = {item.Follow}");
}
```

[계산 규칙](first-rules.md)에서 손으로 구한 그 집합이 그대로 찍혀요.\
그럼 이 FIRST 가 *내부에서* 어떻게 만들어지는지 들어가 봐요.

## 먼저 — 코드는 두 부류예요: **계산기** 와 **꺼내기**

코드를 처음 열면 `First…` 라는 메서드가 여러 개라 헷갈릴 수 있어요.\
그런데 사실 딱 **두 부류** 로 나뉩니다.

- **`FirstSet(...)` = 계산기.**\
  재귀로 FIRST 를 *직접 구해서* 캐시(`_cache`)에 채워 넣어요. 진짜 알고리즘은 전부 여기 있어요.
- **`First(...)` = 꺼내기.**\
  계산이 끝난 캐시에서 결과를 *조회하거나 조립* 할 뿐이에요.

그리고 둘 다 **인자 타입별 오버로드** 가 있어요 — FIRST 가 *기호 한 개(`Symbol`)*, *기호들의 열(`Concat`)*, *특정 생성규칙(`Single`)* 셋 모두에 정의되니까요.

그러니 우리는 **계산기 `FirstSet`** 만 따라가면 돼요.\
앞에서 본 *세 경우 + ⊕ + 반복* 이 전부 거기 들어 있거든요.

## 담아 둘 곳 — `_cache` 와 `_bChanged`

```csharp
private bool _bChanged = false;                                    // "이번 바퀴에 늘었나?" (반복 종료용)
private Dictionary<NonTerminalSingle, TerminalSet> _cache = new(); // 생성규칙별 FIRST 저장 (점점 채워나감)
```

`_cache` 는 *생성규칙([Single](deep-single.md)) 하나하나* 의 FIRST 를 모아 두는 장부예요.\
`_bChanged` 는 *"이번 한 바퀴에 뭐라도 늘었나"* 를 기억하는 깃발이고요. (반복을 언제 멈출지 정해요.)

## 계산기 (1) — 기호 하나의 FIRST: `FirstSet(Symbol)`

여기에 **경우 ①(단말) · 경우 ②(비단말)** 가 그대로 들어 있어요.

```csharp
public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null)
{
    if (symbol is Terminal)                                       // ── 경우 ① : 단말이면
        return new TerminalSet(symbol as Terminal);               //    자기 자신만 담아 돌려줌

    TerminalSet result = new TerminalSet();                       // ── 경우 ② : 비단말이면
    foreach (NonTerminalSingle singleNT in symbol as NonTerminal) //    모든 생성규칙을 돌며
    {
        … (좌재귀 가드 — 아래에서) …
        result.UnionWith(FirstSet(singleNT, seenNT));             //    각 생성규칙의 FIRST 를
        result.UnionWith(_cache[singleNT]);                       //    전부 합집합
    }
    return result;
}
```

- **기호가 단말이면**, 자기 자신만 담은 집합을 그대로 돌려줍니다 → **경우 ①** 그대로예요.
- **기호가 비단말이면**, 그 비단말의 **모든 생성규칙을 `foreach` 로 돌며** 각 FIRST 를 **합집합** 합니다.\
  → **경우 ②** + *"비단말의 FIRST 는 모든 생성규칙의 FIRST 를 합집합한 것"* 이 그대로예요.\
  (`NonTerminal` 을 `foreach` 하면 생성규칙([Single](deep-single.md))이 하나씩 나오던 거, 기억나시죠?)

## 계산기 (2) — 생성규칙(열)의 FIRST: `FirstSet(Concat)` = ⊕

한 생성규칙은 결국 기호들의 *열* 이죠 (`Term '*' Factor`).\
그 FIRST 는 — 앞 장 결론대로 — 기호들의 FIRST 를 **⊕(링섬)** 한 거예요.

```csharp
public TerminalSet FirstSet(NonTerminalConcat singleNT, ...)
{
    TerminalSet result = new TerminalSet();
    foreach (var symbol in singleNT)                        // 기호를 순서대로
    {
        result = result.RingSum(FirstSet(symbol, seenNT));  // ⊕ 한 칸
        if (!result.IsNullAble) break;                      // 더 볼 ε 없으면 멈춤
    }
    return result;
}
```

`RingSum` 이 바로 ⊕ 예요. 정의가 1:1 이라, **세 경우가 이 안에 다 들어 있어요.**

```csharp
// TerminalSet.RingSum
if (result.IsNull)            result.UnionWith(param);   //  ∅ ⊕ B = B
else if (result.IsNullAble) { result.Remove(ε);          //  ε 있으면(=사라질 수 있으면) ε 빼고
                              result.UnionWith(param); }  //          다음 칸 B 도 더함     → 경우 ③
// 둘 다 아니면 그대로 = A   (B 안 봄)                       → 경우 ①·②
```

- `IsNullAble` = *"ε 을 품고 있나"* (`Contains(Epsilon)`), 즉 **nullable 판정** 이에요.
- `if (!result.IsNullAble) break;` = *"맨 앞이 사라질 수 없으면 거기서 끝"* → **경우 ①·②** 가 여기서 멈춰요.
- ε 이 있으면 `break` 를 안 하고 다음 칸으로 넘어가요 → **경우 ③**.

즉 **세 경우는 이 한 루프 안에 다 들어 있고, ⊕ 가 어디서 멈추느냐일 뿐** 이에요.

## 좌재귀를 안전하게 처리하는 가드

**경우 ②** 에서 `Term → Term '*' …` 같은 좌재귀를 봤죠.\
순진하게 재귀하면 `FirstSet(Term)` → `FirstSet(Term)` → … 무한 루프예요.\
그래서 `FirstSet(Symbol)` 안에 가드 두 줄이 있어요.

```csharp
if (seenNT.Contains(singleNT)) { result.UnionWith(_cache[singleNT]); … }  // 이미 본 생성규칙?
if (singleNT[0] == symbol)     { result.UnionWith(_cache[singleNT]); … }  // 맨 앞이 자기 자신? (좌재귀)
```

*이미 방문한* 생성규칙이거나 *맨 앞이 자기 자신* 이면, 더 재귀하지 않고 **지금까지 쌓인 캐시값만** 가져와요.\
무한 루프는 끊되, 반복(다음 절)이 한 바퀴 더 돌면서 마저 채워줘요.\
앞 장의 *"맨 앞 `Term` 의 지금까지 값을 더함"* 이 바로 이 두 줄입니다.

## 전체를 — 안 바뀔 때까지 반복: `CalculateAllFirst`

```csharp
public void CalculateAllFirst(HashSet<NonTerminal> nonTerminals)
{
    do
    {
        _bChanged = false;
        foreach (var nonTerminal in nonTerminals) FirstSet(nonTerminal);
    }
    while (_bChanged);     // 한 바퀴 돌아도 안 늘면 → 정답
}
```

`do { _bChanged = false; … } while(_bChanged)` — 앞 장의 **고정점 반복** 그 자체예요.\
`FirstSet` 안에서 캐시가 조금이라도 커지면 `_bChanged` 를 켜 두고, 한 바퀴 동안 변화가 없으면 멈춰요.

## 결과 꺼내기 — `First(...)`

계산(`CalculateAllFirst`)이 끝나면, 이제 결과는 **꺼내기** 쪽인 `First(...)` 로 읽어요.

```csharp
public TerminalSet First(NonTerminalSingle key) => _cache[key];   // 캐시에서 바로 조회
public TerminalSet First(NonTerminalConcat concat);               // 아는 FIRST 들을 ⊕ 로 조립
public TerminalSet First(Symbol key);                             // 단말이면 {자기}, 비단말이면 캐시에서 모음
```

맨 처음 본 공개 API `GetFirstAndFollow()` 가, 안에서 바로 이 `First(...)` 들을 불러
`FirstAndFollowCollection` 을 만들어 돌려주는 거예요.

## 공식 ↔ 코드 한눈에

| 계산 규칙 | 코드 |
|---|---|
| 경우 ① — 단말로 시작 | `if (symbol is Terminal) return new TerminalSet(...)` |
| 경우 ② — 비단말로 시작 (+ 합집합) | `foreach (singleNT in NonTerminal) result.UnionWith(FirstSet(singleNT))` |
| 경우 ③ — ε / ⊕ | `result.RingSum(...)` + `if (!IsNullAble) break;` |
| 좌재귀 가드 | `if (singleNT[0] == symbol) …` |
| 고정점 반복 | `do { _bChanged=false; … } while(_bChanged)` |
| 계산 vs 조회 | `FirstSet(...)` 계산 / `First(...)` 꺼내기 |

> 📌 심화 과정은 늘 이렇게 **"규칙 ↔ 우리 코드"** 를 짝지어 봐요.

## 예제로 따라가기

`FIRST(Factor)` 부터요. `Factor : '(' Expr ')' | id`.

- 생성규칙 1 `'(' Expr ')'` → `FirstSet(Concat)`: 첫 기호 `'('` 는 단말 → `RingSum` 결과 `{ '(' }`, ε 없음 → **즉시 break.** → `{ '(' }`
- 생성규칙 2 `id` → `{ id }`
- 둘을 합쳐 → **`FIRST(Factor) = { '(', id }`** ✓

`FIRST(Term)` 은 `Term : Term '*' Factor | Factor`.

- 생성규칙 `Factor` → `{ '(', id }`
- 생성규칙 `Term '*' Factor` → 첫 기호가 `Term`(자기 자신, 좌재귀) → 가드가 캐시값을 가져와요.\
  반복이 한 바퀴 더 돌며 `Term` 의 캐시가 `{ '(', id }` 로 채워지면, 그게 흘러들어와요.
- 수렴 → **`FIRST(Term) = { '(', id }`** ✓

`Expr` 도 같은 흐름으로 **`{ '(', id }`**.\
[계산 규칙 페이지](first-rules.md)·[정의와 유도 페이지](first-formula.md)에서 구한 답과 정확히 같아요. ✓

**ε(nullable)이 코드에서 실제로 도는 모습도 한 번 봐요.** expr 문법엔 nullable 이 없어서, 위 트레이스는 전부 "ε 없음 → 즉시 `break`" 로만 흘렀거든요. 이 장에서 예시로 쓸 문법은 아래와 같아요:

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

`FIRST(S)` 를 `FirstSet(Concat)` 으로 돌려봐요. `S → A B` 의 기호를 순서대로:

- 첫 칸 `A` → `FirstSet(A) = { a, ε }`. `RingSum` 결과에 ε 이 있어요(`IsNullAble = true`) → **`break` 안 하고 다음 칸으로** (← 바로 이게 경우 ③ 경로!)
- 둘째 칸 `B` → `FirstSet(B) = { b, ε }`.
- ε 을 빼고(`Remove(ε)`) 앞 칸과 합쳐요: `{ a } ∪ { b, ε }` = `{ a, b, ε }`
- → **`FIRST(S) = { a, b, ε }`**

위 expr 트레이스에선 한 번도 안 돌던 `if (!result.IsNullAble) break;` 의 *"안 멈추고 다음 칸"* 가지가, 여기선 첫 칸 `A` 에서 실제로 발동해요.

## 한눈에 — FIRST 관련 전체 명세

`FirstFollowAnalyzer` 의 FIRST 쪽 골격이에요.\
로직은 비우고 *무엇이 있는지* 만 보여줘요. (계산기 `FirstSet` / 꺼내기 `First` 로 나뉜 게 보이죠.)

```csharp
public partial class FirstFollowAnalyzer
{
    private bool _bChanged;
    private Dictionary<NonTerminalSingle, TerminalSet> _cache;

    // ── 꺼내기 (계산 끝난 결과 조회/조립) ─────
    public TerminalSet First(NonTerminalSingle key);     // 캐시에서 꺼내기
    public TerminalSet First(NonTerminalConcat concat);  // 열의 FIRST (⊕로 조립)
    public TerminalSet First(Symbol key);

    // ── 계산기 (재귀 + ⊕ + 좌재귀 가드) ───────
    public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null);
    public TerminalSet FirstSet(NonTerminalConcat singleNT, HashSet<NonTerminalSingle> seenNT = null);

    // ── 전체 고정점 반복 ─────────────────────
    public void CalculateAllFirst(HashSet<NonTerminal> nonTerminals);
}
```

보조 타입 `TerminalSet : HashSet<Terminal>` 에 `IsNull`(빈 집합) · `IsNullAble`(ε 포함) · `RingSum`(⊕) 이 들어 있고요.

## 다음 장

FIRST 를 **정의 · 유도 · 계산 규칙 · 코드** 까지 한 바퀴 끝냈어요.\
**"어떤 토큰으로 *시작* 하나"** 의 답이죠.

이제 그 짝꿍 — **"이것 *다음에* 어떤 토큰이 오나"**, 즉 **FOLLOW** 예요.\
FOLLOW 는 FIRST 를 *재료로* 쓰기 때문에 (`CalculateAllFollow` 첫 줄이 `CalculateAllFirst`), 방금 만든 게 그대로 이어져요.

👉 **[FOLLOW · 정의와 유도](follow-formula.md)**
