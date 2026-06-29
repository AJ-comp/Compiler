# 構文解析表 · CLR — 実装

正直に言うと — **エンジンは CLR をまだ実装していません。** だからこのページは短いです。\
でも *できることとできないこと* をはっきり書いておくのがこのマニュアルの約束なので、*あるがまま* お見せします。

---

## 状態生成の分岐における CLR の居場所

エンジンで状態のまとまりを作るコード（`CanonicalRelation`）は方式に応じて三つに分かれますが、CLR はこの道です。

```csharp
else if (canonicalType == CanonicalType.C1)   // CLR
    ConstructC1(...);
```

ところがその `ConstructC1` の中身が — **空っぽです。**

```csharp
private void ConstructC1(NonTerminal virtualStartSymbol, RelationData relationData)
{
    // (空 — LR(1) 状態生成は未実装)
}
```

だから `CLRParser` の衝突チェックもこうなっています。

```csharp
public override AmbiguityCheckResult CheckAmbiguity()
    => throw new NotImplementedException();   // CLR — 未実装
```

---

## なぜ作らなかったのか

[しくみ](parse-table-clr.md) で見たように、CLR は **状態の爆発** という代償が高くつきます。そして次章で見る **LALR** が — その爆発なしに *ほぼ同じ精密度（LR(1) 級）* を出してくれるのです。

だからエンジンは *本物の CLR を作らず、**LALR で LR(1) にほぼ匹敵する力を出す*** ほうを選びました。（実務の構文解析器生成器がよくする選択です。）表向きに「LR(1) 対応！」と書く代わりに、*実際にできるのは LALR* だと正直に書いておく、というわけです。

---

## 次へ

では、その LALR は — *完璧な CLR をどう「マージして」* 爆発なしに精密度を出すのでしょうか？

👉 **[構文解析表 · LALR — しくみ](parse-table-lalr.md)**
