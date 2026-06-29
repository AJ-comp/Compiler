# 構文解析表 · SLR — 実装

[しくみ](parse-table-slr.md)で SLR の定義を見ました — **LR(0) 状態 + reduce は FOLLOW(A) でだけ。**\
では、これが*エンジンのコード*ではどんな姿でしょう？言葉で説明した定義が*驚くほどそのまま*現れます。

---

## `SLRParser` — 核心はコンストラクタ一行

エンジンで SLR は `SLRParser` ですが — 核心はコンストラクタの*一行*です。

```csharp
public SLRParser(Grammar grammar, bool bLogging)
    : base(grammar, CanonicalType.C0, bLogging) { }   // ← C0 = LR(0) 状態で作れ
```

この `CanonicalType.C0` を渡すと、状態を作る `CanonicalRelation` が*この分岐*へ行きます。

```csharp
if (canonicalType == CanonicalType.C0)            // ← SLR
{
    ConstructC0(...);                             //  LR(0) 状態を作る（正準集合の章のあの自動機械）
    ReduceParameter = ReduceParameter.Follow;     //  reduce のマスは FOLLOW で決める
}
```

---

## この二行が SLR の定義のすべて

ちょうどこの**二行**が[しくみ](parse-table-slr.md)で言った SLR の定義の*すべて*です:

> **LR(0) 状態（`ConstructC0`）+ reduce は FOLLOW（`ReduceParameter.Follow`）。**

つまり[正準集合](canonical-set.md)で作っておいたあの LR(0) 状態たちを*そのまま使い*、reduce のマスだけ FOLLOW で埋めるのです。\
衝突検査（`SLRParser.CheckAmbiguity`）も同じく `ReduceParameter.Follow` を基準に状態ごとに reduce のマスを調べ、一つのマスに二つあれば衝突として報告します。

言葉で説いた*しくみ*がコードでは*たった二行* — これが SLR が "Simple" と呼ばれる理由でもあります。

---

## 次へ

SLR は *LR(0) 状態 + FOLLOW*、こんなに簡単です。ところが[しくみ](parse-table-slr.md)で見たように — まさにその **FOLLOW** が見かけの衝突を生みます。（だからエンジンも SLR は*補助*としてだけ置き、推奨パーサーは **LALR** です。）

さあ、はしごの上の段へ上ります。SLR の見かけの衝突をなくす鍵は — *グローバルな FOLLOW ではなく、項目ごとにつく精密な lookahead。* それが*何なのか*からまず見て（次の章）、それを使う **CLR**、続いて **LALR** へ行きます。

👉 **[lookahead（LR(1) 項目）](parse-table-lookahead.md)**
