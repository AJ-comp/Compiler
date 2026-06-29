# 파싱 테이블 · LALR — 구현

[원리](parse-table-lalr.md) 에서 LALR = *CLR 을 합친 것* 이고, 엔진은 CLR 을 통째로 만들지 않고 **LR(0) 상태에 lookahead 를 직접 전파** 해 같은 결과를 얻는다고 했죠. 그 코드를 봅시다.

---

## 먼저 — 세 갈래로 갈리는 상태 만들기

엔진에서 상태 모음을 만드는 코드(`CanonicalRelation`)는 *방식에 따라 세 갈래* 로 갈려요.

```csharp
if (canonicalType == CanonicalType.C0)            // SLR
    ConstructC0(...);                             //   LR(0) 상태, reduce 는 FOLLOW
else if (canonicalType == CanonicalType.LALRC1)   // LALR   ← 실제로 쓰는 길
    if (ConstructC0(...)) ConvertC0ToC1(...);     //   LR(0) 상태 + lookahead 전파
else if (canonicalType == CanonicalType.C1)       // CLR
    ConstructC1(...);                             //   ← 본문이 비어 있음 ([CLR 구현](parse-table-clr-impl.md))
```

[SLR 구현](parse-table-slr-impl.md) 에서 봤듯, SLR 은 `ConstructC0` *하나* 로 끝났어요.\
LALR(`LALRC1`)도 **출발은 똑같아요** — `ConstructC0` 로 *같은 LR(0) 상태* 를 만들죠. (그래서 [원리](parse-table-lalr.md) 에서 말한 대로 *상태 수가 LR(0) 그대로* 인 거예요.)

차이는 딱 한 줄 — **`ConvertC0ToC1`.** 바로 여기가 LALR 의 심장이에요.

---

## LALR 의 심장 — `ConvertC0ToC1`

[원리](parse-table-lalr.md) 에서 LALR 을 *"이 상태에 실제로 도달하는 lookahead 만 쓴다"* 고 했고, 그걸 **lookahead 전파** 로 계산한다고 했죠. 그 전파가 *실제로 일어나는 곳* 이 이 메서드예요.

```csharp
private void ConvertC0ToC1(RelationData relationData)
{
    CalculateLookAhead();                       // ① lookahead 가 상태 사이로 어떻게 흘러드는지 계산 (전파)

    foreach (var state in IndexStateDic)        // ② 상태마다 FOLLOW 를 구해 두고
        state.Value.CalculateFollow(relationData);

    foreach (var entry in LookAheadTable)       // ③ 계산된 '정밀 lookahead' 를
    {                                           //    바로 그 상태의 · 그 아이템에 붙인다
        var state = IndexStateDic[entry.Key.Item1];
        state.GetItem(entry.Key.Item2).LookAhead = entry.Value;
    }
}
```

세 단계가 [원리](parse-table-lalr.md) 의 말을 *그대로* 실행해요.

1. **`CalculateLookAhead()`** — 어떤 글자가 *어느 상태의 어느 아이템* 으로 흘러드는지 따라가며 계산해요. 이게 바로 *lookahead 전파* 예요.
2. 상태마다 `CalculateFollow` 로 밑준비를 하고,
3. **그 결과를 — FOLLOW *전체* 가 아니라 — 상태마다·아이템마다 `LookAhead` 로 *콕 집어* 붙여요.**

이렇게 각 완료 아이템이 *자기만의 정밀한 lookahead* 를 갖게 돼요.\
그래서 reduce 칸을 정할 때, SLR 은 `FOLLOW` 를 봤지만 **LALR 은 이 `LookAhead` 를** 봐요. 분기에도 그렇게 적혀 있죠.

```csharp
ReduceParameter = ReduceParameter.LalrLookAhead;   // SLR 은 .Follow 였던 그 자리
```

바로 이 *한 끗* — **`FOLLOW`(문법 전체) 대신, 상태별 정밀 `LookAhead`(이 자리만)** — 이 [SLR 의 헛충돌](parse-table-slr.md) 을 없애는 전부예요. 상태는 하나도 안 늘리고요.

---

## 마무리 — LR 이야기가 한 바퀴 돌았어요

우리가 걸어온 길을 되짚어보면:

> 문법 읽기 → FIRST / FOLLOW → LR 아이템 → 상태 → 클로저 → GOTO → 정준 집합 → 파싱 테이블 → 충돌 → **정밀도(SLR · CLR · LALR)**

이걸로 **"LR 파서를 *어떻게* 만드는가"** 의 이야기가 한 바퀴 완결됐어요.\
이제 당신은 — 문법 한 줄이 어떻게 상태가 되고, 상태가 어떻게 표가 되고, 그 표가 왜 *결정적으로* 굴러가는지, 그리고 *충돌* 이 왜 생기고 어떻게 다스리는지를 — 처음부터 끝까지 따라온 거예요. 🎉

> 그 표가 *실제로 돌아가는 모습* (한 글자씩 shift·reduce 하며 트리가 자라는 장면)은 기본 트랙의 **[표로 실제 파싱](parsing-in-action.md)** 에서 손으로 만져볼 수 있어요. 이론으로 *만든* 표를, 거기서 *돌려* 보면 한 바퀴가 완전히 닫혀요.
