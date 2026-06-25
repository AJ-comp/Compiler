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
    protected List<Symbol> _symbols = new();   // [Expr, '+', Term] 처럼 순서대로
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
public NonTerminalConcat PrevSymbolListFrom(int index);   // 어떤 위치 *앞쪽* 기호들
public NonTerminalConcat PostSymbolListFrom(int index);   // 어떤 위치 *뒤쪽* 기호들
```

なぜこれが大事かというと — LR パーサは「今この規則を *どこまで* 読んだか」を **ドット(`•`)** で示します。\
ドットの **すぐ後ろ** の記号を `X` とすると、生成規則はこんな形になります。

```
   A → α • X β       (α = 이미 읽음,  X = 지금 볼 기호,  β = 남음)
```

このとき `X` の位置を `index` として渡すと、二つのメソッドが `α` と `β` を **区間ごと** 切り出してくれます。\
(コードを見ると
`Prev = _symbols.Take(index)`, `Post = _symbols.Skip(index + 1)` です。)

```
   A → Expr '+' Term          (인덱스   0     1     2)

   X = '+' (인덱스 1) 로 보면   →   A → Expr • '+' Term

   PrevSymbolListFrom(1) = [ Expr ]        ← α : 점 앞 '전부'  (Take(1))
   PostSymbolListFrom(1) = [ Term ]        ← β : X 뒤 '전부'   (Skip(2))
   ( 인덱스 1의 '+' 자신은 어느 쪽에도 안 들어가요 — 지금 '보는' 기호니까 )
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
    protected List<Symbol> _symbols;     // 순서대로 늘어선 기호들

    // ── 꼬리표 ───────────────────────────────
    public uint Priority { get; internal set; }
    public MeaningUnit MeaningUnit { get; internal set; }

    // ── 판단 ─────────────────────────────────
    public bool IsNull { get; }          // 비어 있나
    public bool IsEpsilon { get; }       // 빈 것(ε) 하나뿐인가
    public bool IsAllTerminal { get; }   // 전부 단말인가

    // ── 앞/뒤 떼어내기 (LR 의 점 앞/뒤) ──────
    public NonTerminalConcat PrevSymbolListFrom(int index);
    public NonTerminalConcat PostSymbolListFrom(int index);

    // ── 편집 ─────────────────────────────────
    public void Replace(int index, Symbol item);
    public void AddRange(params Symbol[] symbols);
    public NonTerminalConcat ToReverse();
    // … IList<Symbol> 의 Add / Insert / RemoveAt / this[i] …

    // ── 변환 ─────────────────────────────────
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
