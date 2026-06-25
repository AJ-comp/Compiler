# 閉包 · 実装 (コード)

> 🎓 **応用編** です。\
> 先ほどは閉包の[定義](closure-def.md)(もっとも小さい閉じた集合)と[計算法](closure-calc.md)(閉じるまで一
> 段階ずつ)を見ましたね。\
> 今度はそれが `Analyzer.Closure` のコードに **ほぼ一行ずつ** どう落とし込まれているかを見ましょう。

## コード — `Analyzer.Closure`

```csharp
public static CanonicalState Closure(CanonicalState iStatus, HashSet<NonTerminal> exploredSet = null)
{
    if (exploredSet == null) exploredSet = new HashSet<NonTerminal>();
    var result = new CanonicalState();

    foreach (var item in iStatus)
    {
        result.Add(item);                               // ① I のアイテムはそのまま抱える
        
        if (item.MarkSymbol == null)        continue;   // 点が終わり   → 展開するものなし
        if (item.MarkSymbol is Terminal)    continue;   // 点の後ろが終端 → 展開するものなし

        NonTerminal B = item.MarkSymbol as NonTerminal; // 点の後ろが非終端 B
        if (exploredSet.Contains(B)) continue;          // すでに展開した B → スキップ (再投入しない)
        exploredSet.Add(B);

        CanonicalState param = new CanonicalState();
        foreach (NonTerminalSingle single in B)         // ② B のすべての生成規則を
            param.Add(new LRItem(single));              //    点を先頭(B → •γ) に

        result.UnionWith(Analyzer.Closure(param, exploredSet));   // さらに展開するものがあれば再帰で
    }
    return result;
}
```

## 定義・計算法 ↔ コード一行ずつ

この短い関数の中に、前の二つのページがそのまま入っています。

- **`result.Add(item)`** → 定義の **①**(「`I` のアイテムをそのまま抱える」)です。
- **`item.MarkSymbol == null` / `is Terminal` なら `continue`** → *点の後ろが終端記号か終わりなら展開するものは
  ない。*([計算法](closure-calc.md)の **3段階目** — `(`・`id` のような終端記号で止まっていた、あの部分です。)
- **点の後ろが非終端 `B` → `foreach (single in B) param.Add(new LRItem(single))`** → 定義の **②**
  (「`B` のすべての生成規則を `B → •γ` として追加」)。`new LRItem(single)` が *点を先頭* に置いたアイテム([LR
  項目](lr-item.md)の既定コンストラクタ、`markIndex = 0`)です。
- **`exploredSet`** → 計算法の *「すでに展開した非終端記号は再びやらない」*。`Expr` を一度展開したあとは、また
  展開しないように止めて、**無限ループ** を断ち切ります。
- **`result.UnionWith(Closure(param, exploredSet))`** → *「閉じるまで」* を **再帰**(関数が自分自身をもう一度呼ぶこと)で成し遂げます。\
  新しく入れた `B → •γ` たちの点の後ろが *また* 非終端記号なら、その再帰が続けて展開してくれるからです。(計算法の1段階目 →
  2段階目へ進んでいた、あの流れです。)

> 💡 [計算法](closure-calc.md)では *「一段階ずつ繰り返す」* と言いましたが、コードは `do…while` ループではなく
> **再帰** ですよね?\
> 二つは同じことです — 再帰が「点の後ろの非終端記号をたどって入り込みながら」閉じるまで展開し、`exploredSet` が
> *一つの非終端記号を一度だけ* 展開するよう保証するので、結局 *もっとも小さい閉じた集合* 一つに落ち着きます。

## 📐 著者の閉包設計ダイアグラム

- 閉包アルゴリズム — <https://www.lucidchart.com/documents/edit/515ff26b-2649-4150-86ec-80288ef51570/0>

> (著者本人のノートなので、閲覧権限が必要な場合があります。)

## 次の章

閉包で **一つの状態を漏れなく埋める方法** を — 定義・計算法・実装まで — すべて見ました。

これからはその状態から **記号を一つ読むと** どの状態へ行くのか、それを決める **GOTO** の番です。\
(そして GOTO も *最後にはこの閉包をもう一度* 使います — だから新しく移った状態も、空きなく完成します。もうすぐ
見ることになります。)

👉 **[GOTO — 記号を一つ読んで次の状態へ](goto.md)**

---

👈 前へ: [閉包 · 計算法](closure-calc.md)
