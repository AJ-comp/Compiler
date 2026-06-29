# 構文解析表 · LALR — 実装

[しくみ](parse-table-lalr.md) で、LALR = *CLR をマージしたもの* であり、エンジンは CLR をまるごと作らず **LR(0) 状態に先読みを直接伝播** して同じ結果を得る、と言いましたね。そのコードを見てみましょう。

---

## まず — 三つに分かれる状態づくり

エンジンで状態のまとまりを作るコード（`CanonicalRelation`）は *方式に応じて三つ* に分かれます。

```csharp
if (canonicalType == CanonicalType.C0)            // SLR
    ConstructC0(...);                             //   LR(0) 状態、reduce は FOLLOW
else if (canonicalType == CanonicalType.LALRC1)   // LALR   ← 実際に使う道
    if (ConstructC0(...)) ConvertC0ToC1(...);     //   LR(0) 状態 + 先読み伝播
else if (canonicalType == CanonicalType.C1)       // CLR
    ConstructC1(...);                             //   ← 中身が空 ([CLR 実装](parse-table-clr-impl.md))
```

[SLR 実装](parse-table-slr-impl.md) で見たように、SLR は `ConstructC0` *一つ* で終わりました。\
LALR（`LALRC1`）も **出発はまったく同じです** — `ConstructC0` で *同じ LR(0) 状態* を作ります。（だから [しくみ](parse-table-lalr.md) で言ったとおり *状態数が LR(0) のまま* なのです。）

違いはたった一行 — **`ConvertC0ToC1`。** まさにここが LALR の心臓です。

---

## LALR の心臓 — `ConvertC0ToC1`

[しくみ](parse-table-lalr.md) で LALR を *「この状態に実際にたどり着く先読みだけを使う」* と言い、それを **先読み伝播** で計算する、と言いましたね。その伝播が *実際に起こる場所* がこのメソッドです。

```csharp
private void ConvertC0ToC1(RelationData relationData)
{
    CalculateLookAhead();                       // ① lookahead が状態のあいだをどう流れ込むかを計算（伝播）

    foreach (var state in IndexStateDic)        // ② 状態ごとに FOLLOW を求めておき
        state.Value.CalculateFollow(relationData);

    foreach (var entry in LookAheadTable)       // ③ 計算した「精密な lookahead」を
    {                                           //    まさにその状態の · その項目に付ける
        var state = IndexStateDic[entry.Key.Item1];
        state.GetItem(entry.Key.Item2).LookAhead = entry.Value;
    }
}
```

三つのステップが [しくみ](parse-table-lalr.md) の言葉を *そのまま* 実行します。

1. **`CalculateLookAhead()`** — どの文字が *どの状態のどの項目* へ流れ込むかを追いながら計算します。これがまさに *先読み伝播* です。
2. 状態ごとに `CalculateFollow` で下ごしらえをし、
3. **その結果を — FOLLOW *全体* ではなく — 状態ごと・項目ごとに `LookAhead` として *ピンポイントで* 付けます。**

こうして各完了項目が *自分だけの精密な先読み* を持つようになります。\
だから reduce のマスを決めるとき、SLR は `FOLLOW` を見ましたが **LALR はこの `LookAhead` を** 見ます。分岐にもそう書かれていますね。

```csharp
ReduceParameter = ReduceParameter.LalrLookAhead;   // SLR は .Follow だったその場所
```

まさにこの *わずかな違い* — **`FOLLOW`（文法全体）の代わりに、状態ごとの精密な `LookAhead`（この場所だけ）** — が [SLR の見かけの衝突](parse-table-slr.md) をなくすすべてです。状態は一つも増やさずに。

---

## おわりに — LR の物語が一周しました

私たちが歩んできた道をふり返ると：

> 文法読み → FIRST / FOLLOW → LR 項目 → 状態 → 閉包 → GOTO → 正準集合 → 構文解析表 → 衝突 → **精密度（SLR · CLR · LALR）**

これで **「LR パーサを *どうやって* 作るのか」** の物語が一周、完結しました。\
いまや あなたは — 文法の一行がどうやって状態になり、状態がどうやって表になり、その表がなぜ *決定的に* 回るのか、そして *衝突* がなぜ起き、どう手なずけるのかを — 最初から最後まで追ってきたのです。🎉

> その表が *実際に動く姿*（一文字ずつ shift・reduce しながら木が育つ場面）は、基本トラックの **[表で実際に構文解析](parsing-in-action.md)** で手で触ってみることができます。理論で *作った* 表を、そこで *回して* みると一周が完全に閉じます。
