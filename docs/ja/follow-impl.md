# FOLLOW · 実装 (コード)

> 🎓 **発展コース · 実装** です。\
> 前の [FOLLOW · 計算規則](follow-rules.md)では、三つの規則(① 開始記号 `$` / ② 後ろの `FIRST` / ③ 末尾なら LHS を継承)と反復を見ましたね。\
> 今回は、それが `FirstFollowAnalyzer` のコードに **ほぼ一行ずつ** どう入っているのかを追っていきます。

## 使ってみる — 公開 API (一行)

FIRST と *同じ入口* です。\
一度の呼び出しで FIRST と FOLLOW がいっしょに出てきます。

```csharp
var parser = new LALRParser(grammar);

foreach (var item in parser.GetFirstAndFollow())
{
    Console.WriteLine($"{item.Symbol}");
    Console.WriteLine($"   FOLLOW = {item.Follow}");
}
```

## しまっておく場所 — `Datas`

```csharp
public RelationData Datas { get; private set; } = new();   // 非終端記号ごとの FOLLOW を保存
```

FIRST の `_cache` に対応する、**非終端記号ごとの FOLLOW 台帳** です。\
計算が終わったら、ここから結果を取り出します。

## ① · ② 初期値 — `InitFollowSet`

規則 ①(開始記号 `$`)と規則 ②(すぐ後ろの `FIRST − ε`)が、この一つのメソッドに入っています。

```csharp
public TerminalSet InitFollowSet(NonTerminal nonTerminal, HashSet<NonTerminal> nonTerminalSet)
{
    TerminalSet result = new TerminalSet();
    if (nonTerminal.IsStartSymbol) result.Add(new EndMarker());   // ── 規則 ① : 開始記号なら $

    foreach (var symbol in GetFollowSymbols(nonTerminalSet, nonTerminal))  // ── 規則 ② : すぐ後ろの記号たち
    {
        var firstSet = FirstSet(symbol);                          //    その後ろのものの FIRST
        firstSet.ExceptWith(new TerminalSet(new Epsilon()));      //    ε を除いて
        result.UnionWith(firstSet);                               //    FOLLOW に加える
    }
    return result;
}
```

- `if (nonTerminal.IsStartSymbol) result.Add(new EndMarker())` → **規則 ①** です。(`$` はコードでは `EndMarker`。)
- `GetFollowSymbols(...)` が *`B` のすぐ後ろに来る記号たち* を集めてくれたら、それぞれの `FirstSet` から `ε` を除いて合わせます → **規則 ②**。\
  (ここで `FirstSet` は [FIRST 計算機](first-impl.md)をそのまま呼びます。だから FOLLOW の前に FIRST が全部終わっていないといけないんですね。)

### すぐ後ろの記号を探す — `FindNextSymbolSet`

`GetFollowSymbols` は文法全体を見渡しながら `FindNextSymbolSet` を集めたものです。\
これが *「`B` のすぐ後ろ」* を探すコードです。

```csharp
foreach (var symbol in singleNT)
{
    if (bFind)                                       // B をすでに通り過ぎていたら
    {
        result.Add(symbol);                          //    その後ろの記号を入れて
        if (!FirstSet(symbol).IsNullAble) break;     //    ε になれないなら止める
    }
    else if (symbol == findSymbol) bFind = true;     // ここで B を見つける
}
```

`B`(`findSymbol`)に出会った後から記号を入れていき、*消えられない(ε でない)* 記号に出会ったら止めます。\
計算規則 ②の *「β の FIRST」* を集めるのが、まさにこれです。

## ③ 反復継承 — `ConCatExprUpdateFollow`

規則 ③(*「末尾なら LHS の FOLLOW を継承」*)がここです。

```csharp
private bool ConCatExprUpdateFollow(NonTerminalSingle contents, TerminalSet followSet)
{
    for (int i = contents.Count - 1; i >= 0; i--)            // ← 末尾(右)から左へ
    {
        var symbol = contents[i];
        if (symbol is Terminal) break;                       // 終端記号なら止める (終端記号には FOLLOW がない)

        this.Datas[symbol as NonTerminal].UnionWith(followSet);  // LHS の FOLLOW を注ぎ込む
        …
        if (!FirstSet(symbol).IsNullAble) break;             // この非終端記号が ε になれないなら止める
    }
    …
}
```

`followSet` がそのまま LHS(`A`)の FOLLOW です。\
生成規則を **末尾(右)から** 見ながら、末尾にある非終端記号に `FOLLOW(A)` を注ぎ込みます → **規則 ③**。\
その非終端記号が *消えられる(ε)なら* その前の非終端記号まで注ぎ続け、消えられないなら止めます — 計算規則の
*「β が消えるならその前まで」* が、まさにこの `if (!IsNullAble) break;` の一行です。

(`UpdateFollow` は、一つの非終端記号の *すべての生成規則* に対して上を回してくれるラッパーです。)

## 全体ドライバー — `CalculateAllFollow`

```csharp
public void CalculateAllFollow(HashSet<NonTerminal> nonTerminals)
{
    CalculateAllFirst(nonTerminals);                          // ← FIRST が先 (規則 ②が FIRST を使うので)

    foreach (var nt in nonTerminals)
        Datas.Add(nt, InitFollowSet(nt, nonTerminals));       // ← 規則 ①·② で初期値

    do
    {
        bChange = false;
        foreach (var d in Datas)
            if (UpdateFollow(d.Key, d.Value)) bChange = true;  // ← 規則 ③ 継承
    }
    while (bChange);                                          //    変わらなくなるまで (不動点)
}
```

計算規則が **順序までそのまま** です — **FIRST が先 → 規則 ①·② で初期値 → 規則 ③ を変わらなくなるまで反復。**\
最初の行 `CalculateAllFirst` が、*「FOLLOW は FIRST を材料に使う」* をコードで釘付けにしているんですね。

## 結果を取り出す — `Follow`

```csharp
public TerminalSet Follow(NonTerminal nonTerminal) => this.Datas[nonTerminal];   // ただの照会
```

計算が終わった `Datas` から **取り出すだけ** です。\
公開 API `GetFirstAndFollow()` がこれを呼んで `item.Follow` に詰めてくれます。

## 公式 ↔ コード ひと目で

| 計算規則 | コード |
|---|---|
| 規則 ① — 開始記号 `$` | `if (IsStartSymbol) result.Add(new EndMarker())` |
| 規則 ② — 後ろの `FIRST − ε` | `GetFollowSymbols` + `FirstSet(symbol).ExceptWith(ε)` |
| 規則 ③ — 末尾なら LHS 継承 | `ConCatExprUpdateFollow` (右から `UnionWith(followSet)`) |
| FIRST を材料に (先に) | `CalculateAllFollow` 最初の行 `CalculateAllFirst` |
| 不動点反復 | `do { … } while(bChange)` |

## 例で追っていく

`CalculateAllFollow` を私たちの文法に回すと — [計算規則](follow-rules.md)で手で回したのとまったく同じように流れます。

- **`CalculateAllFirst`** が先 → `FIRST` たちが埋まる。
- **`InitFollowSet`**(①②) → `FOLLOW(Expr)={$,'+',')'}`、`FOLLOW(Term)={'*'}`、`FOLLOW(Factor)={}`。
- **`do…while`**(③) → `Term ⊇ FOLLOW(Expr)`、`Factor ⊇ FOLLOW(Term)` に伝播して、両方とも `{$,'+',')','*'}`。
- もっと回しても変化がなければ `bChange = false` → 止まる。 ✓\
  (伝播が *何周* かかるかは `Datas` の処理順序によって一周か二周かに分かれますが、止まった後の値はどの順序でも同じです。)

## ひと目で — FOLLOW 関連の全仕様

`FirstFollowAnalyzer` の FOLLOW 側の骨格です。\
ロジックは空にして *何があるか* だけを見せます。

```csharp
public partial class FirstFollowAnalyzer   // (FOLLOW 側)
{
    public RelationData Datas { get; }

    // ── 取り出し ───────────────────────────────
    public TerminalSet Follow(NonTerminal nonTerminal);   // Datas から照会

    // ── 計算 ─────────────────────────────────
    public TerminalSet InitFollowSet(NonTerminal nt, HashSet<NonTerminal> all);     // 規則 ①·②
    public SymbolSet   GetFollowSymbols(HashSet<NonTerminal> all, NonTerminal nt);  // B のすぐ後ろの記号たち
    public void        CalculateAllFollow(HashSet<NonTerminal> nonTerminals);       // FIRSTが先 + 初期値 + 反復
    // (private) UpdateFollow · ConCatExprUpdateFollow · FindNextSymbolSet — 規則 ③ + 「後ろ探し」
}
```

## 次の章

FIRST と FOLLOW を **定義 · 導出 · 計算規則 · コード** まで全部終えました。\
この二つこそが **構文解析表** を作る核心の材料です。

次は、LR パーサが *「今このルールをどこまで読んだのか」* を表現する **LR項目** と **正準集合(状態たち)** —
それが集まって、いよいよあの有名な **構文解析表** になります。

👉 **[LR項目](lr-item.md)**

---

👈 前へ: [FOLLOW · 計算規則](follow-rules.md)
