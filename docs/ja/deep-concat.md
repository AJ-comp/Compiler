# Concat — 順序(連接)

> 🎓 **発展コース** です。前の [NonTerminal](deep-nonterminal.md) 章で、規則は `alters`(選択肢たち)を
> 抱えていると話しましたね。さあ今度は、その **選択肢ひとつの中身** へと入っていきます。

選択肢ひとつは、どんな形をしているのでしょう?

`Expr '+' Term` を見ると — 記号たちが **順番に** 並んでいますね。\
`Expr` の次に `+`、その次に `Term`。

## 著者の悩み — 「この *順序* をどうやって入れる?」

著者が思いついた答えはシンプルでした。

> *「順番に並んでいるもの? それってただの **リスト** じゃないか。」*

だから `NonTerminalConcat` は **Symbol たちのリスト** です。\
それが全部です。

> 📍 **`NonTerminalConcat : IList<Symbol>`** · `…/RegularGrammar/NonTerminalConcat.cs`

```csharp
public class NonTerminalConcat : IList<Symbol>
{
    protected List<Symbol> _symbols = new();   // [Expr, '+', Term] のように順番に
}
```

図にすると、こうなります。

```
   Expr '+' Term
        │
        ▼
   NonTerminalConcat
     [ Expr ] → [ '+' ] → [ Term ]
       0        1         2
```

文字どおり、マスが順番に並んだリストですね。\
難しくありません。

## 一行に付く荷札 — Priority, MeaningUnit

順序だけ入れれば終わりでしょうか?\
著者はもう二つ付け加えました。

**`Priority`** — 優先順位です。\
あとで文法に *衝突* が起きたとき、「どちらを先に取るか」を決める
基準として使います。\
(衝突の話はずっとあとで出てきます。)

**`MeaningUnit`** — 「この行が AST でどんな *意味の単位* になるのか」です。\
(これもあとで別に扱います。)

```csharp
public uint Priority { get; internal set; }
public MeaningUnit MeaningUnit { get; internal set; }
```

今は *「順序(リスト)のほかにも、優先順位・意味の荷札が付くんだな」* くらいで十分です。

## 小さいけれど大事な二つ — 前側 / 後ろ側を切り取る

`NonTerminalConcat` には、メソッドがもう二つあります。\
今は些細に見えますが、**あとで LR 構文解析の
心臓部でそのまま使われます。**

```csharp
public NonTerminalConcat PrevSymbolListFrom(int index);   // ある位置の *前側* の記号たち
public NonTerminalConcat PostSymbolListFrom(int index);   // ある位置の *後ろ側* の記号たち
```

なぜこれが大事かというと — LR パーサは「今この規則を *どこまで* 読んだか」を **ドット(`•`)** で示します。\
ドットの **すぐ後ろ** の記号を `X` とすると、生成規則はこんな形になります。

```
   A → α • X β       (α = すでに読んだ,  X = 今見る記号,  β = 残り)
```

このとき `X` の位置を `index` として渡すと、二つのメソッドが `α` と `β` を **区間ごと** 切り出してくれます。\
(コードを見ると
`Prev = _symbols.Take(index)`, `Post = _symbols.Skip(index + 1)` です。)

```
   A → Expr '+' Term          (インデックス   0     1     2)

   X = '+' (インデックス 1) で見ると   →   A → Expr • '+' Term

   PrevSymbolListFrom(1) = [ Expr ]        ← α : ドットの前 '全部'  (Take(1))
   PostSymbolListFrom(1) = [ Term ]        ← β : X の後ろ '全部'   (Skip(2))
   ( インデックス 1 の '+' 自身は、どちらにも入りません — 今 '見ている' 記号なので )
```

ポイントは — この二つが *一つの記号* ではなく **前/後ろの区間まるごと** を返す、ということです。\
今は「こういうものが前もって用意されているんだな」くらいに知っておけば大丈夫です。\
これが活きてくるのは *LR項目* の章です。

## ひと目で — Concat の全体像

`NonTerminalConcat` の **全体の骨格** です。\
ロジックは省いて *何があるか* だけを見せます。

```csharp
public class NonTerminalConcat : IList<Symbol>, ...
{
    protected List<Symbol> _symbols;     // 順番に並んだ記号たち

    // ── 荷札 ───────────────────────────────
    public uint Priority { get; internal set; }
    public MeaningUnit MeaningUnit { get; internal set; }

    // ── 判定 ─────────────────────────────────
    public bool IsNull { get; }          // 空かどうか
    public bool IsEpsilon { get; }       // 空のもの(ε)一つだけかどうか
    public bool IsAllTerminal { get; }   // 全部が終端記号かどうか

    // ── 前/後ろを切り取る (LR のドットの前/後ろ) ──────
    public NonTerminalConcat PrevSymbolListFrom(int index);
    public NonTerminalConcat PostSymbolListFrom(int index);

    // ── 編集 ─────────────────────────────────
    public void Replace(int index, Symbol item);
    public void AddRange(params Symbol[] symbols);
    public NonTerminalConcat ToReverse();
    // … IList<Symbol> の Add / Insert / RemoveAt / this[i] …

    // ── 変換 ─────────────────────────────────
    public HashSet<NonTerminal> ToNonTerminalSet();
    public TerminalSet ToTerminalSet();
}
```

核心はたった一つです。\
**`NonTerminalConcat` = 順序を入れた記号のリスト + 荷札いくつか。**

## 次の章

`Concat` で *順序(連接)* を入れる方法を見ました。

ところで — `Expr` の「0番目の選択肢」「1番目の選択肢」のように、**この順序が *どの規則の何番目の選択肢なのか***
まで知らなければならないときがあります。\
そのために `Concat` に荷札をもう一つ付けたのが、次の主役です。

👉 **[Single — 一つの生成規則](deep-single.md)**
