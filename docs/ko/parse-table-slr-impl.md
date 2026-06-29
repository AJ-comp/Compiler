# 파싱 테이블 · SLR — 구현

[원리](parse-table-slr.md) 에서 SLR 의 정의를 봤어요 — **LR(0) 상태 + reduce 는 FOLLOW(A) 에서만.**\
그럼 이게 *엔진 코드* 에선 어떻게 생겼을까요? 말로 한 정의가 *놀랄 만큼 그대로* 드러나요.

---

## `SLRParser` — 핵심은 생성자 한 줄

엔진에서 SLR 은 `SLRParser` 인데 — 핵심은 생성자 *한 줄* 이에요.

```csharp
public SLRParser(Grammar grammar, bool bLogging)
    : base(grammar, CanonicalType.C0, bLogging) { }   // ← C0 = LR(0) 상태로 만들어라
```

이 `CanonicalType.C0` 를 넘기면, 상태를 만드는 `CanonicalRelation` 이 *이 갈래* 로 가요.

```csharp
if (canonicalType == CanonicalType.C0)            // ← SLR
{
    ConstructC0(...);                             //  LR(0) 상태를 만든다 (정준 집합 장의 그 자동기계)
    ReduceParameter = ReduceParameter.Follow;     //  reduce 칸은 FOLLOW 로 정한다
}
```

---

## 두 줄이 SLR 정의 전부

딱 이 **두 줄** 이 [원리](parse-table-slr.md) 에서 말한 SLR 의 정의 *전부* 예요:

> **LR(0) 상태(`ConstructC0`) + reduce 는 FOLLOW(`ReduceParameter.Follow`).**

즉 [정준 집합](canonical-set.md) 에서 만들어 둔 그 LR(0) 상태들을 *그대로 쓰고*, reduce 칸만 FOLLOW 로 채우는 거예요.\
충돌 검사(`SLRParser.CheckAmbiguity`)도 똑같이 `ReduceParameter.Follow` 기준으로 상태마다 reduce 칸을 따져, 한 칸에 둘이면 충돌로 보고하고요.

말로 풀었던 *원리* 가 코드에선 *딱 두 줄* — 이게 SLR 이 "Simple" 이라 불리는 이유이기도 해요.

---

## 다음

SLR 은 *LR(0) 상태 + FOLLOW*, 이렇게 간단해요. 그런데 [원리](parse-table-slr.md) 에서 봤듯 — 바로 그 **FOLLOW** 가 헛충돌을 만들죠. (그래서 엔진도 SLR 은 *보조* 로만 두고, 권장 파서는 **LALR** 이에요.)

이제 사다리 윗 칸으로 올라가요. SLR 의 헛충돌을 없애는 열쇠는 — *글로벌 FOLLOW 말고, 아이템마다 붙는 정밀한 lookahead.* 그게 *무엇인지* 부터 보고(다음 장), 그걸 쓰는 **CLR**, 이어서 **LALR** 로 갑니다.

👉 **[lookahead (LR(1) 아이템)](parse-table-lookahead.md)**
