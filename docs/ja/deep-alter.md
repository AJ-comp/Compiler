# Alter — 択一(代替案の集合)

> 🎓 **発展コース** です。ついに [NonTerminal](deep-nonterminal.md) が抱えていたあの **`alters` 箱**
> を開ける番です。前の [Single](deep-single.md) 章までたどり着くの、お疲れさまでした — ここで断片たちが
> 一つにかみ合います。

[NonTerminal](deep-nonterminal.md) 章で私たちはずっと *「規則は `alters`(代替案の束)を抱える」*
と言ってきましたよね。

その **`alters` の正体** こそが `NonTerminalAlter` です。

## 著者の悩み — 「複数の代替案をどう入れる?」

`Expr` は作る方法が二つありました。\
`Expr '+' Term` と `Term` です。

各方法はすでに私たちが知っている `Concat`(順序を入れたリスト)ですよね。\
すると *「その Concat たちを複数
入れる器」* が必要になります。

著者の答えは — **集合(Set)** でした。

```csharp
public class NonTerminalAlter : ISet<NonTerminalConcat>   // ← Concat たちの『集合』
{
    private HashSet<NonTerminalConcat> concatSymbols = new();
}
```

なぜリスト(List)ではなく **集合(HashSet)** なのでしょうか? 著者の考えをたどってみると:

> *「代替案は『この規則を作る方法たちの集まり』だ。同じ方法が二度入っていたって意味が
> ないだろう — 重複は一つに数えるべきだ。だったら List より **Set** が正しい。」*

そこで `Expr` の代替案たちはこう入ります。

<pre class="lrbox">
   <span class="nt">Expr</span> : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
                  │
                  ▼
   NonTerminalAlter   (= alters,  Concat たちの集合)
     ├ NonTerminalConcat  [ <span class="nt">Expr</span> ] · [ <span class="setm">'+'</span> ] · [ <span class="nt">Term</span> ]     ← 代替案 1
     └ NonTerminalConcat  [ <span class="nt">Term</span> ]                           ← 代替案 2
</pre>

[NonTerminal](deep-nonterminal.md) 章で描いた `alters` の図、覚えていますよね?\
あの図の *本当の
型* がこれです。

## 二つの扉 — AddAsConcat vs AddAsAlter (★ 連接と択一がここで分かれます)

さて、前に [Symbol](deep-symbols.md) 章でちらっと出てきた `+`(連接)と `|`(択一) — その二つが **実際に
どんな違い** だったのか、まさにここでコードとして現れます。

`NonTerminalAlter` には代替案を入れる扉が *二つ* あります。

```csharp
public void AddAsConcat(params Symbol[] symbols)
{
    this.Add(new NonTerminalConcat(symbols));               // まるごと『一個』の代替案
}

public void AddAsAlter(params Symbol[] symbols)
{
    foreach (var symbol in symbols)
        this.Add(new NonTerminalConcat(symbol));            // それぞれを『別々に』代替案として
}
```

違いが見えますか? **同じ `[a, b]` を入れても結果がまったく変わります。**

<pre class="lrbox">
   AddAsConcat(a, b)   →   Alter { [ <span class="setm">a</span> · <span class="setm">b</span> ] }        ← 代替案 1個 (中に a, b が順番に)

   AddAsAlter(a, b)    →   Alter { [ <span class="setm">a</span> ] , [ <span class="setm">b</span> ] }    ← 代替案 2個 (a 一つ, b 一つ)
</pre>

そしてこれこそが、私たちがコードで文法を書くときの `+` と `|` です。

```csharp
a + b   // operator+ →  AddAsConcat(a, b)  →  "a の次に b"   (一つの代替案, 順序)
a | b   // operator| →  AddAsAlter(a, b)   →  "a または b"   (二つの代替案, 択一)
```

> 💡 [Symbol](deep-symbols.md) 章で *「`|` は『併合』ではなく『択一』だ」* と言っていたのが、ここで
> 証明されます。`AddAsAlter` は a と b を **一つに合わせません** — `[a]` と `[b]` という **別個の
> 代替案二つとして別々に置きます。** 読む側はその中から *一つを選ぶ* わけです。だから *併合(merge)* ではなく
> *択一(choose-one)* なのです。

## すべての代替案にまとめて — AddSymbols / InsertSymbol

ときには *すでに入っているすべての代替案* に記号を同じように差し込まなければならないときがあります。\
そのための
メソッドもあります。

```csharp
public void AddSymbols(params Symbol[] symbols)    // すべての代替案の『末尾』に追加
public void InsertSymbol(int index, params Symbol[] symbols)   // すべての代替案の特定の位置に挿入
```

名前が単数形の `Symbol` ではなく、動作が *「集合内のすべての Concat に broadcast」* だという点だけ覚えて
おいてください。(文法を自動的に変形するときに重宝します。)

## 集合だから — 合わせて、引いて、重なるか見て

`NonTerminalAlter` が `ISet<NonTerminalConcat>` だと言いましたよね。\
だから **集合演算** をまるごと持って
います。

```csharp
public void UnionWith(...);          // 和集合 — 他の代替案たちを引き寄せて合わせる
public void IntersectWith(...);      // 積集合
public void ExceptWith(...);         // 差集合
public bool SetEquals(...);          // 代替案の構成が同じか
// … IsSubsetOf / Overlaps / … ISet のすべて …
```

今すぐは使いませんが — のちほど **文法を正規化したり変形** するとき(例: 自動生成された規則を
既存の規則に溶け込ませるとき)、「代替案の集合を合わせて引く」これらの演算がそのまま動員されます。\
著者がわざわざ Set を
選んだ甲斐がここで出てきます。

## 小さな便利さ一つ — IsInduceEpsilon

代替案の中に **空のもの(ε)** が一つでもあれば `true` を返すプロパティもあります。

```csharp
public bool IsInduceEpsilon { get; }   // 代替案の中に ε(空の代替案)があるか
```

ただし、これは *直接* ε の代替案があるかどうかだけを見ます(`A → ε` のような場合)。`A → B`、`B → ε` のように *別の規則を経て間接的に* ε になる **本物の nullable** は捕まえられません — それは [FIRST/FOLLOW](first-follow.md) で `FIRST` に ε が入ったかどうかで別途判定します。(断片たちがどうつながるのか、だんだん見えてきましたよね?)

## ひと目で — Alter の全体像

`NonTerminalAlter` の **全体の骨格** です。\
ロジックは空にして *何があるか* だけを見せます。

```csharp
public class NonTerminalAlter : ISet<NonTerminalConcat>
{
    private HashSet<NonTerminalConcat> concatSymbols;   // 代替案(Concat)たちの集合

    public int  Count { get; }
    public bool IsInduceEpsilon { get; }                // 代替案の中に ε があるか

    // ── 代替案を入れる (★ 連接 vs 択一) ───────────
    public void AddAsConcat(params Symbol[] symbols);   // → 代替案 1個 (順番どおり)   = '+'
    public void AddAsAlter(params Symbol[] symbols);    // → 代替案 N個 (別々に)        = '|'

    // ── すべての代替案に broadcast ────────────────
    public void AddSymbols(params Symbol[] symbols);
    public void InsertSymbol(int index, params Symbol[] symbols);

    // ── 集合演算 (ISet) ─────────────────────
    public void UnionWith(...);  public void IntersectWith(...);  public void ExceptWith(...);
    public bool SetEquals(...);  public bool IsSubsetOf(...);     public bool Overlaps(...);
    // … Add / Remove / Contains / GetEnumerator …

    // ── 変換 ────────────────────────────────
    public HashSet<NonTerminal> ToNonTerminalSet();
}
```

ひと言で — **`Alter` = Concat(代替案)たちの *集合*。そして `+`/`|` がこの集合に代替案を *どう* 入れるかを
分ける。**

## 断片が全部かみ合いました — 全体図

ここまでが Janglim 文法構造の **骨組みのすべて** です。\
一枚にまとめてみます。

<pre class="lrbox">
   Symbol  (抽象)
    ├ Terminal              ← 葉(トークン):  これ以上分割されない
    └ NonTerminal "<span class="nt">Expr</span>"    ← 枝(規則)
         └ alters : NonTerminalAlter            ← 代替案たちの『集合』   (Alter)
              ├ NonTerminalConcat [<span class="nt">Expr</span> · <span class="setm">'+'</span> · <span class="nt">Term</span>]   ┐  各要素(Concat)を
              └ NonTerminalConcat [<span class="nt">Term</span>]                ┘  "Expr の何番目" として見れば
                                                            → NonTerminalSingle (生成規則)
</pre>

- **Symbol** — すべての記号の抽象的な根 (アイデンティティ = `UniqueKey`)
- **Terminal / NonTerminal** — 葉 / 枝
- **Concat** — 一つの代替案の *順序(RHS)*
- **Alter** — 代替案たちの *集合* (= `alters`)
- **Single** — 集合の中の一つの代替案を *「どの規則の何番目か」* として見た *生成規則*

`+`(連接)は `Concat` を、`|`(択一)は `Alter` の代替案を作る — このひと文が全体を貫きます。

## 次の章

文法を *入れる器* は全部見ました。\
これからはこの構造 **の上で** パーサが実際に行う計算へと移ります。

まず最初は、規則が「どのトークンで始まれるか(FIRST)、その後には何が来られるか(FOLLOW)」を
求める段階です。\
基本コースで概念として出会ったそれを、今度は **公式とコード** で。

👉 **[FIRST · FOLLOW — 公式と実装](first-formula.md)**
