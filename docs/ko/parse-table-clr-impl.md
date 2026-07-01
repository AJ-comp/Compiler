# 파싱 테이블 · CLR — 구현

솔직하게 — **엔진은 CLR 을 아직 구현하지 않았어요.** 그래서 이 페이지는 짧아요.\
하지만 *되는 것과 안 되는 것* 을 분명히 적어두는 게 이 매뉴얼의 약속이니, *있는 그대로* 보여줄게요.

---

## 상태 생성 분기에서 CLR 의 자리

엔진에서 상태 모음을 만드는 코드(`CanonicalRelation`)는 방식에 따라 세 갈래로 갈리는데, CLR 은 이 길이에요.

```csharp
else if (canonicalType == CanonicalType.C1)   // CLR
    ConstructC1(...);
```

그런데 그 `ConstructC1` 의 본문이 — **텅 비어 있어요.**

```csharp
private void ConstructC1(NonTerminal virtualStartSymbol, RelationData relationData)
{
    // (비어 있음 — LR(1) 상태 생성 미구현)
}
```

그래서 `CLRParser` 의 충돌 검사도 이렇게 돼 있고요.

```csharp
public override AmbiguityCheckResult CheckAmbiguity()
    => throw new NotImplementedException();   // CLR — 미구현
```

---

## 왜 안 만들었나

[원리](parse-table-clr.md) 에서 봤듯, CLR 은 **상태 폭증** 이라는 비용이 커요. 그리고 다음 장에서 볼 **LALR** 이 — 그 폭증 없이 *거의 같은 정밀도(LR(1) 급)* 를 내주거든요.

그래서 엔진은 *진짜 CLR 을 만들지 않고, **LALR 로 LR(1) 에 거의 맞먹는 힘을 내는*** 쪽을 택했어요. (실무 파서 생성기들이 흔히 하는 선택이에요.) 겉으로 "LR(1) 지원!" 이라고 적는 대신, *실제로 되는 건 LALR* 이라고 솔직히 적어두는 거고요.

---

## 다음

그럼 그 LALR 은 — *완벽한 CLR 을 어떻게 "합쳐서"* 폭증 없이 정밀도를 낼까요?

👉 **[파싱 테이블 · LALR — 원리](parse-table-lalr.md)**
