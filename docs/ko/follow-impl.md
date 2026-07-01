# FOLLOW · 구현 (코드)

> 🎓 **심화 과정 · 구현** 이에요.\
> 앞 [FOLLOW · 계산 규칙](follow-rules.md)에서 세 규칙(① 시작기호 `$` / ② 뒤의 `FIRST` / ③ 끝이면 LHS 상속)과 반복을 봤죠.\
> 이번엔 그게 `FirstFollowAnalyzer` 코드에 **거의 한 줄씩** 어떻게 들어갔는지 따라가요.

## 써보기 — 공개 API (한 줄)

FIRST 와 *같은 입구* 예요.\
호출 한 번에 FIRST 와 FOLLOW 가 같이 나와요.

```csharp
var parser = new LALRParser(grammar);

foreach (var item in parser.GetFirstAndFollow())
{
    Console.WriteLine($"{item.Symbol}");
    Console.WriteLine($"   FOLLOW = {item.Follow}");
}
```

## 담아 둘 곳 — `Datas`

```csharp
public RelationData Datas { get; private set; } = new();   // 비단말별 FOLLOW 저장
```

FIRST 의 `_cache` 에 대응하는, **비단말별 FOLLOW 장부** 예요.\
계산이 끝나면 여기서 결과를 꺼내요.

## ① · ② 초깃값 — `InitFollowSet`

규칙 ①(시작기호 `$`)과 규칙 ②(바로 뒤의 `FIRST − ε`)가 이 한 메서드에 들어 있어요.

```csharp
public TerminalSet InitFollowSet(NonTerminal nonTerminal, HashSet<NonTerminal> nonTerminalSet)
{
    TerminalSet result = new TerminalSet();
    if (nonTerminal.IsStartSymbol) result.Add(new EndMarker());   // ── 규칙 ① : 시작기호면 $

    foreach (var symbol in GetFollowSymbols(nonTerminalSet, nonTerminal))  // ── 규칙 ② : 바로 뒤 기호들
    {
        var firstSet = FirstSet(symbol);                          //    그 뒤엣것의 FIRST
        firstSet.ExceptWith(new TerminalSet(new Epsilon()));      //    ε 을 빼고
        result.UnionWith(firstSet);                               //    FOLLOW 에 더함
    }
    return result;
}
```

- `if (nonTerminal.IsStartSymbol) result.Add(new EndMarker())` → **규칙 ①** 이에요. (`$` 가 코드에선 `EndMarker`.)
- `GetFollowSymbols(...)` 가 *`B` 바로 뒤에 오는 기호들* 을 모아 주면, 각각의 `FirstSet` 에서 `ε` 을 빼고 합쳐요 → **규칙 ②**.\
  (여기서 `FirstSet` 은 [FIRST 계산기](first-impl.md)를 그대로 불러요. 그래서 FOLLOW 전에 FIRST 가 다 끝나 있어야 하죠.)

### 바로 뒤 기호 찾기 — `FindNextSymbolSet`

`GetFollowSymbols` 는 문법 전체를 훑으며 `FindNextSymbolSet` 을 모은 거예요.\
이게 *"`B` 바로 뒤"* 를 찾는 코드입니다.

```csharp
foreach (var symbol in singleNT)
{
    if (bFind)                                       // B 를 이미 지났으면
    {
        result.Add(symbol);                          //    그 뒤 기호를 담고
        if (!FirstSet(symbol).IsNullAble) break;     //    ε 못 되면 멈춤
    }
    else if (symbol == findSymbol) bFind = true;     // 여기서 B 를 발견
}
```

`B`(`findSymbol`)를 만난 다음부터 기호를 담되, *사라질 수 없는(ε 아닌)* 기호를 만나면 멈춰요.\
계산 규칙 ②의 *"β 의 FIRST"* 를 모으는 게 바로 이거예요.

## ③ 반복 상속 — `ConCatExprUpdateFollow`

규칙 ③(*"맨 끝이면 LHS 의 FOLLOW 를 상속"*)이 여기예요.

```csharp
private bool ConCatExprUpdateFollow(NonTerminalSingle contents, TerminalSet followSet)
{
    for (int i = contents.Count - 1; i >= 0; i--)            // ← 맨 끝(오른쪽)부터 왼쪽으로
    {
        var symbol = contents[i];
        if (symbol is Terminal) break;                       // 단말이면 멈춤 (단말은 FOLLOW 가 없음)

        this.Datas[symbol as NonTerminal].UnionWith(followSet);  // LHS 의 FOLLOW 를 부어줌
        …
        if (!FirstSet(symbol).IsNullAble) break;             // 이 비단말이 ε 못 되면 멈춤
    }
    …
}
```

`followSet` 이 곧 LHS(`A`)의 FOLLOW 예요.\
생성규칙을 **맨 끝(오른쪽)부터** 보면서, 끝에 있는 비단말에 `FOLLOW(A)` 를 부어줘요 → **규칙 ③**.\
그 비단말이 *사라질 수 있으면(ε)* 그 앞 비단말까지 계속 부어주고, 못 사라지면 멈춰요 — 계산 규칙의
*"β 가 사라지면 그 앞까지"* 가 바로 이 `if (!IsNullAble) break;` 한 줄이에요.

(`UpdateFollow` 는 한 비단말의 *모든 생성규칙* 에 대해 위를 돌려주는 래퍼고요.)

## 전체 드라이버 — `CalculateAllFollow`

```csharp
public void CalculateAllFollow(HashSet<NonTerminal> nonTerminals)
{
    CalculateAllFirst(nonTerminals);                          // ← FIRST 먼저 (규칙 ②가 FIRST 를 쓰니까)

    foreach (var nt in nonTerminals)
        Datas.Add(nt, InitFollowSet(nt, nonTerminals));       // ← 규칙 ①·② 로 초깃값

    do
    {
        bChange = false;
        foreach (var d in Datas)
            if (UpdateFollow(d.Key, d.Value)) bChange = true;  // ← 규칙 ③ 상속
    }
    while (bChange);                                          //    안 바뀔 때까지 (고정점)
}
```

계산 규칙이 **순서까지 그대로** 예요 — **FIRST 먼저 → 규칙 ①·② 초깃값 → 규칙 ③ 을 안 바뀔 때까지 반복.**\
첫 줄 `CalculateAllFirst` 가, *"FOLLOW 는 FIRST 를 재료로 쓴다"* 를 코드로 분명히 보여주죠.

## 결과 꺼내기 — `Follow`

```csharp
public TerminalSet Follow(NonTerminal nonTerminal) => this.Datas[nonTerminal];   // 그냥 조회
```

계산이 끝난 `Datas` 에서 **꺼내기만** 해요.\
공개 API `GetFirstAndFollow()` 가 이걸 불러 `item.Follow` 로 채워 주고요.

## 공식 ↔ 코드 한눈에

| 계산 규칙 | 코드 |
|---|---|
| 규칙 ① — 시작기호 `$` | `if (IsStartSymbol) result.Add(new EndMarker())` |
| 규칙 ② — 뒤의 `FIRST − ε` | `GetFollowSymbols` + `FirstSet(symbol).ExceptWith(ε)` |
| 규칙 ③ — 끝이면 LHS 상속 | `ConCatExprUpdateFollow` (오른쪽부터 `UnionWith(followSet)`) |
| FIRST 를 재료로 (먼저) | `CalculateAllFollow` 첫 줄 `CalculateAllFirst` |
| 고정점 반복 | `do { … } while(bChange)` |

## 예제로 따라가기

`CalculateAllFollow` 를 우리 문법에 돌리면 — [계산 규칙](follow-rules.md)에서 손으로 돌린 것과 똑같이 흘러요.

- **`CalculateAllFirst`** 먼저 → `FIRST` 들이 채워짐.
- **`InitFollowSet`**(①②) → `FOLLOW(Expr)={$,'+',')'}`, `FOLLOW(Term)={'*'}`, `FOLLOW(Factor)={}`.
- **`do…while`**(③) → `Term ⊇ FOLLOW(Expr)`, `Factor ⊇ FOLLOW(Term)` 로 전파돼, 둘 다 `{$,'+',')','*'}`.
- 더 돌려도 변화가 없으면 `bChange = false` → 멈춤. ✓\
  (전파가 *몇 바퀴* 걸리는지는 `Datas` 의 처리 순서에 따라 한두 바퀴로 갈리지만, 멈춘 뒤의 값은 어느 순서든 똑같아요.)

**ε(nullable)이 코드에서 도는 모습도 한 번.** expr 문법엔 nullable 이 없어서 위 트레이스는 규칙 ② 의 `ExceptWith(ε)` 도, 규칙 ③ 의 "사라지면 앞 비단말까지 전파" 도 안 건드려요. 이 절에서 예시로 쓸 문법은 아래와 같아요:

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

`FOLLOW(A)` 를 코드대로 따라가요 (`A` 뒤 기호는 `B`):

- `GetFollowSymbols` 가 `A` 뒤의 `B` 를 집어요. `FirstSet(B) = { b, ε }` 인데 **`ExceptWith(ε)`** 가 ε 을 떼니 `{ b }` 만 들어가요. (규칙 ② 의 `− ε`)
- `B` 가 nullable 이라 `ConCatExprUpdateFollow` 가 거기서 안 멈추고, 그 앞 `A` 까지 LHS(`S`)의 FOLLOW 를 부어줘요. (규칙 ③ 의 "사라지면 전파")

<pre class="lrbox">
   FOLLOW(<span class="nt">A</span>) = <span class="setb">{</span> <span class="setm">b</span> <span class="setb">}</span>      <span style="opacity:.6">규칙 ② : FIRST(B) − ε</span>
            ∪ <span class="setb">{</span> <span class="setm">$</span> <span class="setb">}</span>      <span style="opacity:.6">규칙 ③ : B 가 사라져 FOLLOW(S) 상속</span>
            = <span class="setb">{</span> <span class="setm">b</span>, <span class="setm">$</span> <span class="setb">}</span>
</pre>

expr 트레이스에선 한 번도 안 돌던 `ExceptWith(ε)` 와 nullable 전파가, 여기선 실제로 발동해요.

## 한눈에 — FOLLOW 관련 전체 명세

`FirstFollowAnalyzer` 의 FOLLOW 쪽 골격이에요.\
로직은 비우고 *무엇이 있는지* 만 보여줘요.

```csharp
public partial class FirstFollowAnalyzer   // (FOLLOW 쪽)
{
    public RelationData Datas { get; }

    // ── 꺼내기 ───────────────────────────────
    public TerminalSet Follow(NonTerminal nonTerminal);   // Datas 에서 조회

    // ── 계산 ─────────────────────────────────
    public TerminalSet InitFollowSet(NonTerminal nt, HashSet<NonTerminal> all);     // 규칙 ①·②
    public SymbolSet   GetFollowSymbols(HashSet<NonTerminal> all, NonTerminal nt);  // B 바로 뒤 기호들
    public void        CalculateAllFollow(HashSet<NonTerminal> nonTerminals);       // FIRST먼저 + 초깃값 + 반복
    // (private) UpdateFollow · ConCatExprUpdateFollow · FindNextSymbolSet — 규칙 ③ + "뒤 찾기"
}
```

## 다음 장

FIRST 와 FOLLOW 를 **정의 · 유도 · 계산 규칙 · 코드** 까지 전부 끝냈어요.\
이 둘이 바로 **파싱 테이블** 을 만드는 핵심 재료예요.

다음은 LR 파서가 *"지금 이 규칙을 어디까지 읽었나"* 를 표현하는 **LR 아이템** 과 **정준 집합(상태들)** —
그게 모여 드디어 그 유명한 **파싱 테이블** 이 됩니다.

👉 **[LR 아이템](lr-item.md)**

---

👈 이전 장: [FOLLOW · 계산 규칙](follow-rules.md)
