# 클로저 · 구현 (코드)

> 🎓 **심화 과정** 이에요.\
> 앞에서 클로저의 [정의](closure-def.md)(가장 작은 닫힌 집합)와 [계산법](closure-calc.md)(닫힐 때까지 한
> 단계씩)을 봤죠.\
> 이번엔 그게 `Analyzer.Closure` 코드에 **거의 한 줄씩** 어떻게 들어갔는지 봐요.

## 코드 — `Analyzer.Closure`

```csharp
public static CanonicalState Closure(CanonicalState iStatus, HashSet<NonTerminal> exploredSet = null)
{
    if (exploredSet == null) exploredSet = new HashSet<NonTerminal>();
    var result = new CanonicalState();

    foreach (var item in iStatus)
    {
        result.Add(item);                               // ① I 의 아이템은 그대로 품는다

        if (item.MarkSymbol == null)        continue;   // 점이 끝   → 펼칠 것 없음
        if (item.MarkSymbol is Terminal)    continue;   // 점 뒤 단말 → 펼칠 것 없음

        NonTerminal B = item.MarkSymbol as NonTerminal; // 점 뒤가 비단말 B
        if (exploredSet.Contains(B)) continue;          // 이미 펼친 B → 건너뜀 (다시 안 넣음)
        exploredSet.Add(B);

        CanonicalState param = new CanonicalState();
        foreach (NonTerminalSingle single in B)         // ② B 의 모든 생성규칙을
            param.Add(new LRItem(single));              //    점 맨 앞(B → •γ) 으로

        result.UnionWith(Analyzer.Closure(param, exploredSet));   // 더 펼칠 게 있으면 재귀로
    }
    return result;
}
```

## 정의·계산법 ↔ 코드 한 줄씩

이 짧은 함수 안에, 앞 두 페이지가 그대로 들어 있어요.

- **`result.Add(item)`** → 정의의 **①** ("`I` 의 아이템을 그대로 품는다") 예요.
- **`item.MarkSymbol == null` / `is Terminal` 이면 `continue`** → *점 뒤가 단말이거나 끝이면 펼칠 게
  없다.* ([계산법](closure-calc.md)의 **3단계** — `(`·`id` 같은 단말에서 멈추던 그 부분이에요.)
- **점 뒤가 비단말 `B` → `foreach (single in B) param.Add(new LRItem(single))`** → 정의의 **②**
  ("`B` 의 모든 생성규칙을 `B → •γ` 로 추가"). `new LRItem(single)` 이 *점 맨 앞* 인 아이템([LR
  아이템](lr-item.md)의 기본 생성자, `markIndex = 0`)이에요.
- **`exploredSet`** → 계산법의 *"이미 펼친 비단말은 다시 안 함"*. `Expr` 를 한 번 펼친 뒤엔 또 안
  펼치게 막아, **무한 루프** 를 끊어요.
- **`result.UnionWith(Closure(param, exploredSet))`** → *"닫힐 때까지"* 를 **재귀**(함수가 자기 자신을 다시 부르는 것)로 해내요.\
  새로 넣은 `B → •γ` 들의 점 뒤가 *또* 비단말이면, 그 재귀가 이어서 펼쳐 주거든요. (계산법의 1단계 →
  2단계로 넘어가던 그 흐름이에요.)

> 💡 [계산법](closure-calc.md)에선 *"한 단계씩 반복"* 이라고 했는데, 코드는 `do…while` 루프가 아니라
> **재귀** 죠?\
> 둘은 같은 일이에요 — 재귀가 "점 뒤 비단말을 따라 들어가며" 닫힐 때까지 펼치고, `exploredSet` 이
> *한 비단말을 한 번만* 펼치게 보장하니, 결국 *가장 작은 닫힌 집합* 하나로 떨어져요.

## 📐 저자의 클로저 설계 다이어그램

- 클로저 알고리즘 — <https://www.lucidchart.com/documents/edit/515ff26b-2649-4150-86ec-80288ef51570/0>

> (저자 본인의 노트라 열람 권한이 필요할 수 있어요.)

## 다음 장

클로저로 **한 상태를 빠짐없이 채우는 법** 을 — 정의·계산법·구현까지 — 다 봤어요.

이제 그 상태에서 **기호 하나를 읽으면** 어떤 상태로 가는지, 그걸 정하는 **GOTO** 차례예요.\
(그리고 GOTO 도 *마지막엔 이 클로저를 다시* 써요 — 그래서 새로 간 상태도 빈 곳 없이 완성돼요. 곧
보게 돼요.)

👉 **[GOTO — 한 기호를 읽고 다음 상태로](goto.md)**

---

👈 앞으로: [클로저 · 계산법](closure-calc.md)
