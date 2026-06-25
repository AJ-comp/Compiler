# FIRST · 実装 (コード)

> 🎓 **応用編 · 実装** です。\
> 前の [FIRST · 計算ルール](first-rules.md) では、三つの場合(終端記号 / 非終端記号 / ε)と反復を *ルール* として見ましたね。\
> 今回は、そのルールが `FirstFollowAnalyzer` のコードに **ほぼ一行ずつ** どう落とし込まれているのかを追っていきます。\
> ([定義と導出](first-formula.md)からまだ見ていない方は、そちらから始めるのをおすすめします。)

## 使ってみる — 公開 API (一行)

細部に入る前に、まずは *実際の使い方* からです。\
パーサで **一行** 書けば、すべての記号の FIRST/FOLLOW が出てきます。(この入口は FOLLOW と共有していて、一度の呼び出しで両方が出てくるんです。)

```csharp
var parser = new LALRParser(grammar);

foreach (var item in parser.GetFirstAndFollow())   // FirstAndFollowCollection
{
    Console.WriteLine($"{item.Symbol}");
    Console.WriteLine($"   FIRST  = {item.First}");
    Console.WriteLine($"   FOLLOW = {item.Follow}");
}
```

[計算ルール](first-rules.md)で手で求めた、あの集合がそのまま表示されます。\
では、この FIRST が *内部で* どうやって作られるのか、中に入っていきましょう。

## まず — コードは二種類です: **計算機** と **取り出し**

コードを最初に開くと、`First…` というメソッドがいくつもあって戸惑うかもしれません。\
でも実は、ちょうど **二種類** に分かれています。

- **`FirstSet(...)` = 計算機。**\
  再帰で FIRST を *直接求めて* キャッシュ(`_cache`)に詰めていきます。本当のアルゴリズムは全部ここにあります。
- **`First(...)` = 取り出し。**\
  計算が終わったキャッシュから結果を *参照したり組み立てたり* するだけです。

そして、どちらにも **引数の型ごとのオーバーロード** があります — FIRST が *記号一個(`Symbol`)*、*記号の列(`Concat`)*、*特定の生成規則(`Single`)* の三つすべてに定義されているからです。

なので、私たちは **計算機 `FirstSet`** だけを追えば大丈夫です。\
さきほど見た *三つの場合 + ⊕ + 反復* が全部そこに入っているからです。

## 入れておく場所 — `_cache` と `_bChanged`

```csharp
private bool _bChanged = false;                                    // "今回の周回で増えたか?" (反復終了用)
private Dictionary<NonTerminalSingle, TerminalSet> _cache = new(); // 生成規則ごとの FIRST を保存 (少しずつ埋めていく)
```

`_cache` は *生成規則([Single](deep-single.md))一つひとつ* の FIRST をためておく台帳です。\
`_bChanged` は *"今回の一周で何か増えたか"* を覚えておく旗です。(反復をいつ止めるかを決めます。)

## 計算機 (1) — 記号一個の FIRST: `FirstSet(Symbol)`

ここに **場合 ①(終端記号) · 場合 ②(非終端記号)** がそのまま入っています。

```csharp
public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null)
{
    if (symbol is Terminal)                                       // ── 場合 ① : 終端記号なら
        return new TerminalSet(symbol as Terminal);               //    自分自身だけを入れて返す

    TerminalSet result = new TerminalSet();                       // ── 場合 ② : 非終端記号なら
    foreach (NonTerminalSingle singleNT in symbol as NonTerminal) //    すべての生成規則を回って
    {
        … (左再帰ガード — 下で) …
        result.UnionWith(FirstSet(singleNT, seenNT));             //    各生成規則の FIRST を
        result.UnionWith(_cache[singleNT]);                       //    全部和集合
    }
    return result;
}
```

- **記号が終端記号なら**、自分自身だけを入れた集合をそのまま返します → **場合 ①** そのままです。
- **記号が非終端記号なら**、その非終端記号の **すべての生成規則を `foreach` で回って** 各 FIRST を **和集合** します。\
  → **場合 ②** + *"非終端記号の FIRST は、すべての生成規則の FIRST を和集合したもの"* がそのままです。\
  (`NonTerminal` を `foreach` すると生成規則([Single](deep-single.md))が一つずつ出てきたの、覚えていますよね?)

## 計算機 (2) — 生成規則(列)の FIRST: `FirstSet(Concat)` = ⊕

一つの生成規則は結局、記号たちの *列* ですよね (`Term '*' Factor`)。\
その FIRST は — 前章の結論どおり — 記号たちの FIRST を **⊕(リングサム)** したものです。

```csharp
public TerminalSet FirstSet(NonTerminalConcat singleNT, ...)
{
    TerminalSet result = new TerminalSet();
    foreach (var symbol in singleNT)                        // 記号を順番に
    {
        result = result.RingSum(FirstSet(symbol, seenNT));  // ⊕ 一マス
        if (!result.IsNullAble) break;                      // これ以上見る ε がなければ止める
    }
    return result;
}
```

`RingSum` がまさに ⊕ です。定義が 1:1 なので、**三つの場合がこの中に全部入っています。**

```csharp
// TerminalSet.RingSum
if (result.IsNull)            result.UnionWith(param);   //  ∅ ⊕ B = B
else if (result.IsNullAble) { result.Remove(ε);          //  ε があれば(=消える可能性があれば) ε を抜いて
                              result.UnionWith(param); }  //          次のマス B も足す     → 場合 ③
// どちらでもなければそのまま = A   (B は見ない)                → 場合 ①·②
```

- `IsNullAble` = *"ε を含んでいるか"* (`Contains(Epsilon)`)、つまり **nullable 判定** です。
- `if (!result.IsNullAble) break;` = *"先頭が消えられないなら、そこで終わり"* → **場合 ①·②** がここで止まります。
- ε があれば `break` せず、次のマスへ進みます → **場合 ③**。

つまり **三つの場合は、この一つのループの中に全部入っていて、⊕ がどこで止まるかの違いだけ** なんです。

## 左再帰で爆発しないように — ガード

**場合 ②** で `Term → Term '*' …` のような左再帰を見ましたね。\
素直に再帰すると `FirstSet(Term)` → `FirstSet(Term)` → … と無限ループです。\
そこで `FirstSet(Symbol)` の中にガードが二行あります。

```csharp
if (seenNT.Contains(singleNT)) { result.UnionWith(_cache[singleNT]); … }  // もう見た生成規則?
if (singleNT[0] == symbol)     { result.UnionWith(_cache[singleNT]); … }  // 先頭が自分自身? (左再帰)
```

*すでに訪れた* 生成規則であるか、*先頭が自分自身* であれば、それ以上再帰せず **今まで積み上がったキャッシュ値だけ** を取ってきます。\
無限ループは断ち切りつつ、反復(次節)がもう一周回りながら残りを埋めてくれます。\
前章の *"先頭の `Term` の今までの値を足す"* が、まさにこの二行です。

## 全体を — 変わらなくなるまで反復: `CalculateAllFirst`

```csharp
public void CalculateAllFirst(HashSet<NonTerminal> nonTerminals)
{
    do
    {
        _bChanged = false;
        foreach (var nonTerminal in nonTerminals) FirstSet(nonTerminal);
    }
    while (_bChanged);     // 一周回っても増えなければ → 正解
}
```

`do { _bChanged = false; … } while(_bChanged)` — 前章の **不動点反復** そのものです。\
`FirstSet` の中でキャッシュが少しでも大きくなれば `_bChanged` を立てておき、一周のあいだ変化がなければ止まります。

## 結果を取り出す — `First(...)`

計算(`CalculateAllFirst`)が終わったら、これからは結果を **取り出し** 側の `First(...)` で読みます。

```csharp
public TerminalSet First(NonTerminalSingle key) => _cache[key];   // キャッシュからそのまま参照
public TerminalSet First(NonTerminalConcat concat);               // 知っている FIRST たちを ⊕ で組み立て
public TerminalSet First(Symbol key);                             // 終端記号なら {自分}、非終端記号ならキャッシュから集める
```

一番最初に見た公開 API `GetFirstAndFollow()` が、内部でまさにこの `First(...)` たちを呼んで
`FirstAndFollowCollection` を作って返してくれているんです。

## 公式 ↔ コード ひと目で

| 計算ルール | コード |
|---|---|
| 場合 ① — 終端記号で始まる | `if (symbol is Terminal) return new TerminalSet(...)` |
| 場合 ② — 非終端記号で始まる (+ 和集合) | `foreach (singleNT in NonTerminal) result.UnionWith(FirstSet(singleNT))` |
| 場合 ③ — ε / ⊕ | `result.RingSum(...)` + `if (!IsNullAble) break;` |
| 左再帰ガード | `if (singleNT[0] == symbol) …` |
| 不動点反復 | `do { _bChanged=false; … } while(_bChanged)` |
| 計算 vs 参照 | `FirstSet(...)` 計算 / `First(...)` 取り出し |

> 📌 応用編ではいつもこうやって **"ルール ↔ 私たちのコード"** を対にして見ていきます。

## 例で追ってみる

`FIRST(Factor)` からです。`Factor : '(' Expr ')' | id`。

- 生成規則 1 `'(' Expr ')'` → `FirstSet(Concat)`: 最初の記号 `'('` は終端記号 → `RingSum` の結果 `{ '(' }`、ε なし → **即 break。** → `{ '(' }`
- 生成規則 2 `id` → `{ id }`
- 二つを合わせて → **`FIRST(Factor) = { '(', id }`** ✓

`FIRST(Term)` は `Term : Term '*' Factor | Factor`。

- 生成規則 `Factor` → `{ '(', id }`
- 生成規則 `Term '*' Factor` → 最初の記号が `Term`(自分自身、左再帰) → ガードがキャッシュ値を取ってきます。\
  反復がもう一周回って `Term` のキャッシュが `{ '(', id }` で埋まると、それが流れ込んできます。
- 収束 → **`FIRST(Term) = { '(', id }`** ✓

`Expr` も同じ流れで **`{ '(', id }`**。\
[計算ルールのページ](first-rules.md)・[定義と導出のページ](first-formula.md)で求めた答えと、ぴったり同じです。 ✓

## ひと目で — FIRST 関連の全仕様

`FirstFollowAnalyzer` の FIRST 側の骨格です。\
ロジックは空にして *何があるか* だけを見せます。(計算機 `FirstSet` / 取り出し `First` に分かれているのが見えますね。)

```csharp
public partial class FirstFollowAnalyzer
{
    private bool _bChanged;
    private Dictionary<NonTerminalSingle, TerminalSet> _cache;

    // ── 取り出し (計算済みの結果を参照/組み立て) ─────
    public TerminalSet First(NonTerminalSingle key);     // キャッシュから取り出し
    public TerminalSet First(NonTerminalConcat concat);  // 列の FIRST (⊕で組み立て)
    public TerminalSet First(Symbol key);

    // ── 計算機 (再帰 + ⊕ + 左再帰ガード) ───────
    public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null);
    public TerminalSet FirstSet(NonTerminalConcat singleNT, HashSet<NonTerminalSingle> seenNT = null);

    // ── 全体の不動点反復 ─────────────────────
    public void CalculateAllFirst(HashSet<NonTerminal> nonTerminals);
}
```

補助の型 `TerminalSet : HashSet<Terminal>` に `IsNull`(空集合) · `IsNullAble`(ε を含む) · `RingSum`(⊕) が入っています。

## 次の章

FIRST を **定義 · 導出 · 計算ルール · コード** まで、ひと回り終えました。\
**"どのトークンで *始まる* か"** の答えですね。

次はその相棒 — **"これの *次に* どのトークンが来るか"**、つまり **FOLLOW** です。\
FOLLOW は FIRST を *材料として* 使うので (`CalculateAllFollow` の最初の一行が `CalculateAllFirst`)、さっき作ったものがそのままつながります。

👉 **[FOLLOW · 定義と導出](follow-formula.md)**
