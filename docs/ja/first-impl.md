# FIRST · 実装 (コード)

> 🎓 **発展コース · 実装** です。\
> 前の [FIRST · 計算規則](first-rules.md) で三つの場合（終端記号 / 非終端記号 / ε）と繰り返しを *規則* として見ましたね。\
> 今回はその規則が `FirstFollowAnalyzer` のコードに **ほぼ一行ずつ** どう入っているかを追っていきます。\
> （[定義と導出](first-formula.md) からまだ見ていないなら、そこからをおすすめします。）

## 使ってみる — 公開 API（一行）

詳細に入る前に、*実際の使い方* から。\
パーサーから **一行** あれば、すべての記号の FIRST/FOLLOW が出てきます。（この入口は FOLLOW と共有しています — 呼び出し一回で両方出てくるからです。）

```csharp
var parser = new LALRParser(grammar);

foreach (var item in parser.GetFirstAndFollow())   // FirstAndFollowCollection
{
    Console.WriteLine($"{item.Symbol}");
    Console.WriteLine($"   FIRST  = {item.First}");
    Console.WriteLine($"   FOLLOW = {item.Follow}");
}
```

[計算規則](first-rules.md) で手で求めたあの集合が、そのまま出力されます。\
ではこの FIRST が *内部で* どう作られるのか、入っていきましょう。

## まず — コードは二系統です：**計算機** と **取り出し**

コードを最初に開くと、`First…` というメソッドが複数あって混乱するかもしれません。\
でも実はちょうど **二系統** に分かれます。

- **`FirstSet(...)` = 計算機。**\
  再帰で FIRST を *直接求めて* キャッシュ（`_cache`）に詰め込みます。本物のアルゴリズムはすべてここにあります。
- **`First(...)` = 取り出し。**\
  計算が終わったキャッシュから結果を *照会したり組み立てたり* するだけです。

そして両方とも **引数の型ごとのオーバーロード** があります — FIRST が *記号一つ（`Symbol`）*、*記号の並び（`Concat`）*、*特定の生成規則（`Single`）* の三つすべてに定義されているからです。

ですから私たちは **計算機 `FirstSet`** だけを追えばよいのです。\
前で見た *三つの場合 + ⊕ + 繰り返し* が全部そこに入っていますから。

## しまっておく場所 — `_cache` と `_bChanged`

```csharp
private bool _bChanged = false;                                    // "この周で増えたか?" (繰り返し終了用)
private Dictionary<NonTerminalSingle, TerminalSet> _cache = new(); // 生成規則ごとの FIRST を保存 (少しずつ埋めていく)
```

`_cache` は *生成規則（[Single](deep-single.md)）一つひとつ* の FIRST を集めておく帳簿です。\
`_bChanged` は *「この一周で何か増えたか」* を覚えておく旗です。（繰り返しをいつ止めるかを決めます。）

## 計算機 (1) — 記号一つの FIRST： `FirstSet(Symbol)`

ここに **場合 ①（終端記号） · 場合 ②（非終端記号）** がそのまま入っています。

```csharp
public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null)
{
    if (symbol is Terminal)                                       // ── 場合 ① : 終端記号なら
        return new TerminalSet(symbol as Terminal);               //    自分自身だけ入れて返す

    TerminalSet result = new TerminalSet();                       // ── 場合 ② : 非終端記号なら
    foreach (NonTerminalSingle singleNT in symbol as NonTerminal) //    すべての生成規則を回りながら
    {
        … (左再帰ガード — 下で) …
        result.UnionWith(FirstSet(singleNT, seenNT));             //    各生成規則の FIRST を
        result.UnionWith(_cache[singleNT]);                       //    全部和集合
    }
    return result;
}
```

- **記号が終端記号なら**、自分自身だけ入れた集合をそのまま返します → **場合 ①** そのものです。
- **記号が非終端記号なら**、その非終端記号の **すべての生成規則を `foreach` で回りながら** 各 FIRST を **和集合** します。\
  → **場合 ②** + *「非終端記号の FIRST はすべての生成規則の FIRST を和集合したもの」* がそのままです。\
  （`NonTerminal` を `foreach` すると生成規則（[Single](deep-single.md)）が一つずつ出てきていたの、覚えていますよね？）

## 計算機 (2) — 生成規則（並び）の FIRST： `FirstSet(Concat)` = ⊕

一つの生成規則は結局、記号たちの *並び* ですよね（`Term '*' Factor`）。\
その FIRST は — 前章の結論どおり — 記号たちの FIRST を **⊕（リングサム）** したものです。

```csharp
public TerminalSet FirstSet(NonTerminalConcat singleNT, ...)
{
    TerminalSet result = new TerminalSet();
    foreach (var symbol in singleNT)                        // 記号を順番に
    {
        result = result.RingSum(FirstSet(symbol, seenNT));  // ⊕ 一マス
        if (!result.IsNullAble) break;                      // もう見る ε がなければ止まる
    }
    return result;
}
```

`RingSum` がまさに ⊕ です。定義が 1:1 なので、**三つの場合がこの中に全部入っています。**

```csharp
// TerminalSet.RingSum
if (result.IsNull)            result.UnionWith(param);   //  ∅ ⊕ B = B
else if (result.IsNullAble) { result.Remove(ε);          //  ε があれば(=消えうるなら) ε を抜いて
                              result.UnionWith(param); }  //          次のマス B も加える     → 場合 ③
// どちらでもなければそのまま = A   (B は見ない)              → 場合 ①·②
```

- `IsNullAble` = *「ε を抱えているか」*（`Contains(Epsilon)`）、つまり **nullable 判定** です。
- `if (!result.IsNullAble) break;` = *「先頭が消えられないならそこで終わり」* → **場合 ①·②** がここで止まります。
- ε があれば `break` せず次のマスへ進みます → **場合 ③**。

つまり **三つの場合はこの一つのループの中に全部入っていて、⊕ がどこで止まるかの違いだけ** です。

## 左再帰で破綻しないように — ガード

**場合 ②** で `Term → Term '*' …` のような左再帰を見ましたね。\
素直に再帰すると `FirstSet(Term)` → `FirstSet(Term)` → … 無限ループです。\
そこで `FirstSet(Symbol)` の中にガードが二行あります。

```csharp
if (seenNT.Contains(singleNT)) { result.UnionWith(_cache[singleNT]); … }  // すでに見た生成規則?
if (singleNT[0] == symbol)     { result.UnionWith(_cache[singleNT]); … }  // 先頭が自分自身? (左再帰)
```

*すでに訪問した* 生成規則であるか、*先頭が自分自身* なら、それ以上再帰せず **これまで積まれたキャッシュ値だけ** を持ってきます。\
無限ループは断ち切りつつ、繰り返し（次の節）がもう一周回って残りを埋めてくれます。\
前章の *「先頭 `Term` のこれまでの値を加える」* がまさにこの二行です。

## 全体を — 変わらなくなるまで繰り返す： `CalculateAllFirst`

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

計算（`CalculateAllFirst`）が終わったら、これからは結果を **取り出し** 側の `First(...)` で読みます。

```csharp
public TerminalSet First(NonTerminalSingle key) => _cache[key];   // キャッシュからすぐ照会
public TerminalSet First(NonTerminalConcat concat);               // 知っている FIRST たちを ⊕ で組み立て
public TerminalSet First(Symbol key);                             // 終端記号なら{自分}、非終端記号ならキャッシュから集める
```

一番最初に見た公開 API `GetFirstAndFollow()` が、内部でまさにこの `First(...)` たちを呼んで
`FirstAndFollowCollection` を作って返してくれるのです。

## 公式 ↔ コードを一目で

| 計算規則 | コード |
|---|---|
| 場合 ① — 終端記号で始まる | `if (symbol is Terminal) return new TerminalSet(...)` |
| 場合 ② — 非終端記号で始まる (+ 和集合) | `foreach (singleNT in NonTerminal) result.UnionWith(FirstSet(singleNT))` |
| 場合 ③ — ε / ⊕ | `result.RingSum(...)` + `if (!IsNullAble) break;` |
| 左再帰ガード | `if (singleNT[0] == symbol) …` |
| 不動点反復 | `do { _bChanged=false; … } while(_bChanged)` |
| 計算 vs 照会 | `FirstSet(...)` 計算 / `First(...)` 取り出し |

> 📌 発展コースでは、いつもこうやって **「規則 ↔ 私たちのコード」** を対にして見ます。

## 例で追ってみる

`FIRST(Factor)` から。`Factor : '(' Expr ')' | id`。

- 生成規則 1 `'(' Expr ')'` → `FirstSet(Concat)`：最初の記号 `'('` は終端記号 → `RingSum` の結果 `{ '(' }`、ε なし → **即 break。** → `{ '(' }`
- 生成規則 2 `id` → `{ id }`
- 二つを合わせて → **`FIRST(Factor) = { '(', id }`** ✓

`FIRST(Term)` は `Term : Term '*' Factor | Factor`。

- 生成規則 `Factor` → `{ '(', id }`
- 生成規則 `Term '*' Factor` → 最初の記号が `Term`（自分自身、左再帰）→ ガードがキャッシュ値を持ってきます。\
  繰り返しがもう一周回って `Term` のキャッシュが `{ '(', id }` に埋まると、それが流れ込んできます。
- 収束 → **`FIRST(Term) = { '(', id }`** ✓

`Expr` も同じ流れで **`{ '(', id }`**。\
[計算規則のページ](first-rules.md)・[定義と導出のページ](first-formula.md) で求めた答えと正確に同じです。✓

## 一目で — FIRST 関連の全体仕様

`FirstFollowAnalyzer` の FIRST 側の骨格です。\
ロジックは空にして *何があるか* だけを見せます。（計算機 `FirstSet` / 取り出し `First` に分かれているのが見えますね。）

```csharp
public partial class FirstFollowAnalyzer
{
    private bool _bChanged;
    private Dictionary<NonTerminalSingle, TerminalSet> _cache;

    // ── 取り出し (計算済みの結果を照会/組み立て) ─────
    public TerminalSet First(NonTerminalSingle key);     // キャッシュから取り出し
    public TerminalSet First(NonTerminalConcat concat);  // 並びの FIRST (⊕で組み立て)
    public TerminalSet First(Symbol key);

    // ── 計算機 (再帰 + ⊕ + 左再帰ガード) ───────
    public TerminalSet FirstSet(Symbol symbol, HashSet<NonTerminalSingle> seenNT = null);
    public TerminalSet FirstSet(NonTerminalConcat singleNT, HashSet<NonTerminalSingle> seenNT = null);

    // ── 全体の不動点反復 ─────────────────────
    public void CalculateAllFirst(HashSet<NonTerminal> nonTerminals);
}
```

補助型 `TerminalSet : HashSet<Terminal>` に `IsNull`（空集合） · `IsNullAble`（ε 含む） · `RingSum`（⊕） が入っています。

## 次の章

FIRST を **定義 · 導出 · 計算規則 · コード** まで一巡し終えました。\
**「どんなトークンで *始まる* か」** の答えですね。

ではその相棒 — **「これの *次に* どんなトークンが来るか」**、つまり **FOLLOW** です。\
FOLLOW は FIRST を *材料として* 使うので（`CalculateAllFollow` の最初の行が `CalculateAllFirst`）、いま作ったものがそのままつながります。

👉 **[FOLLOW · 定義と導出](follow-formula.md)**
