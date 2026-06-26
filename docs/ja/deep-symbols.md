# Symbol — 文法の最も小さな単位

> 🎓 ここは **深化コース** です。基本コースが *概念* だったとすれば、深化コースは **その概念を Janglim が
> どのようにコードへ積み上げたのか** を — それも **作った順番のまま、ゆっくりと** — たどっていきます。
>
> 一つだけ約束しておきますね。この深化コースで **「著者」** と言うときは、Janglim を自ら設計しコードを
> 書いた **人**(このプロジェクトの主)を指します。*いま、この文章を整理している AI ではありません。*
>
> そして正直に言うと、ここに込めた「著者の考え」は **二つのものが混ざって** います:
>
> - **一部は著者に直接たずねて** その意図をそのまま込めました。(ただし *すべての* 決定を伺ったわけではありません。)
> - **残りの多くの部分はコードを見て「おそらくこういう判断をしただろう」と推論** したものです。
>
> なので *「これは著者本人の言葉、あれはコードから読み取った推測」* とはっきり線を引いて分けられはしません — 二つが
> 自然に混ざり合っていると思ってください。それでも心配いりません、**コードは思った以上に多くを語ってくれますから。** 🙂

## 著者の設計の出発点 — 「具体が見えたら、抽象から押さえる」

著者には、設計するときの一貫した習慣が一つあります。

**互いに異なる具体的なものが見えたら、それらを一つにまとめる *抽象クラス* から押さえる。**

なぜなら — **そうすれば、その具体たちの *共通部分* を一か所に集めてまとめられるからです。** (共通の動作・共通の
データを抽象ベースに一度だけ置けば、具体クラスたちはそれをただ受け継げばよいわけです。)

`Symbol` はまさにそうやって生まれました。

文法には、性格がはっきり異なる二つがありますよね — **Terminal(終端記号、トークン)** と **NonTerminal(非終端記号、
規則)**。\
著者はこの二つの *具体クラス* を見て、こう判断したはずです:

> *「この二つは違うけれど、結局『文法に登場する記号』という点では一つの家族だ。それなら、この二つを **抽象化した
> 共通クラス** をまず押さえよう。」*

そうやって **最初から抽象クラス(`abstract`)として設計された** のが `Symbol` です。\
だから `Symbol` は単独では
生まれられません (`new Symbol()` は不可能) — 必ず Terminal か NonTerminal のいずれかへ **具体化されなければ**
なりません。

> 📍 **`Symbol`** · モジュール `Parse.FrontEnd` (Layer 2) · `src/FrontEnd/Parse.FrontEnd/RegularGrammar/Symbol.cs`

```csharp
public abstract class Symbol : IShowable, IQuantifiable, IConvertableEbnfString
{
    // 二つの具体(Terminal · NonTerminal)を抽象化した共通ベース
}
```

```
        Symbol  (抽象 — 二つの具体の共通の抽象)
        ├── Terminal      ← 葉: これ以上分かれないトークン   (次の章)
        └── NonTerminal   ← 枝: さらに分かれる規則           (その次の章)
```

> 💡 **この習慣はマニュアルの随所でまた出てきます。** これから *具体クラスが複数見えたら、その上にそれらを
> 抽象化したクラスがあるだろうな* と思って見れば、著者の頭の中をたどるのがぐっと楽になります。

## 一つめの共通部分 — アイデンティティ (UniqueKey)

抽象ベースを押さえたので、いよいよそこに *まとめ上げる共通部分* を一つずつ埋めていく番です。\
一つめは —
**「二つの記号が同じか違うかを、何で判断するのか?」**、つまり *アイデンティティ* です。(Terminal であろうと
NonTerminal であろうと同じように必要なものですよね — だから共通。)パーサは絶えず「この記号 = あの記号?」を
問い続けなければならないのです。

いちばん簡単な答えは *名前で比較する* ことです。\
ところが — 著者はここでもう一歩踏み込んだはずです:

> *「名前(画面に見える表示)が後で変わったら? たとえば `+` の表示を 'plus' に変えたら? そうすると
> 同じ記号なのに急に別物に見えてしまうじゃないか。そうなったら構文解析がまるごと揺らいでしまう… だめだ。**『見える
> 名前』と『本当のアイデンティティ』を分離しよう。』*

そうして入ったのが **`UniqueKey`** です。\
表示名とは *まったく別個* の、数字でできた固有の識別子です。

```csharp
public UInt32 UniqueKey { get; internal set; } = UInt32.MaxValue;

public override int GetHashCode() => (int)this.UniqueKey;   // ハッシュも
// == も、Equals も — すべて UniqueKey でのみ比較
```

同じかどうかの判定(`==`)も、ハッシュも、**ただ `UniqueKey` だけで** 行います。

おかげで表示名をいくら変えても、パーサの立場からすれば **同じ記号は永遠に同じ記号** です。

小さく見えますが — *コンパイラのように小さなミスが大きく波及するプロジェクト* では、こういう分離は大きな安全装置です。

> (この「アイデンティティ ↔ 表示」の分離は、次の [Terminal](deep-terminal.md) の章で `Value`/`Caption` として、また
> 具体化されます。)

## もう一つの共通部分 — 演算子と数量子

最後に、私たちが C# で文法を書くとき `Expr + plus + Term` や `... | Term` のように書きますよね?\
このとき使う **`+`(つなぐ)・`|`(選ぶ)演算子**、そして `?`・`*`・`+`(数量子)は — どこにあるべき
でしょうか?

> 著者の判断: *「これは Terminal であろうと NonTerminal であろうと **どんな記号にでも** 使えるべきだ。それなら
> 二つの共通の抽象である `Symbol` に置くのが正しいよね。」*

なので、この演算子・数量子もすべて `Symbol` にあります。(抽象ベースに共通の動作を集めておくわけです。)

```csharp
public static NonTerminal operator +(Symbol left, Symbol right);   // つなぐ(連接)
public static NonTerminal operator |(Symbol left, Symbol right);   // 選ぶ(選択)
// ?(Optional) · *(ZeroOrMore) · +(OneOrMore) も Symbol に (IQuantifiable)
```

いまは *「ここにあるんだな」* だけ — これが実際に **どんな構造を作り出すのか** は、後で一章ずつゆっくり
掘り下げていきます。

## 📐 著者の設計ダイアグラム

著者がこの部分を設計するときに描いたダイアグラムです(コードのコメントにも同じリンクが埋め込まれています)。\
いっしょに
見ると、頭の中の絵がくっきりします。

- Symbol と演算子の設計 — <https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/0>

> (著者本人の設計ノートなので、閲覧権限が必要なことがあります。)

## ひと目で — Symbol の全体像

部分部分を見てきたので、いよいよ `Symbol` クラスの **全体の骨格** を一度に見ましょう。

ロジックは空にして、*何が入っているのか* だけを。(「ああ、こういう形なんだ」と感じてもらえるように。)

```csharp
public abstract class Symbol : IShowable, IQuantifiable, IConvertableEbnfString
{
    // ── アイデンティティ ────────────────────
    public UInt32 UniqueKey { get; internal set; }
    protected string EbnfString { get; set; }

    // ── 表現 (子が埋める) ───────────────────
    public abstract string ToEbnfString(bool bContainLHS = false);
    public abstract string ToGrammarString();
    public abstract string ToTreeString(ushort depth = 1);

    // ── 等価 (すべて UniqueKey 基準) ─────────
    public bool Equals(Symbol other);
    public override int GetHashCode();
    public static bool operator ==(Symbol left, Symbol right);
    public static bool operator !=(Symbol left, Symbol right);

    // ── つなぐ / 選ぶ ────────────────────────
    public static NonTerminal operator +(Symbol left, Symbol right);   // 連接: a の次に b
    public static NonTerminal operator |(Symbol left, Symbol right);   // 選択: a または b

    // ── 数量子 (IQuantifiable) ───────────────
    public NonTerminal ZeroOrMore();   // *
    public NonTerminal OneOrMore();    // +
    public NonTerminal Optional();     // ?
}
```

大きくないですよね? **アイデンティティ · 表現 · 同じか · つなぐ/選ぶ · 数量子** — ちょうどこの五つのまとまりです。

## 次の章

`Symbol` を見ました — *なぜ* 最初から抽象として押さえたのか(二つの具体の抽象化)、*なぜ* アイデンティティを名前ではなく
キーに置いたのかまで。\
さあ、その二つの枝のうち、より単純なほう — **葉(トークン)である Terminal** からのぞいてみましょう。

👉 **[Terminal — これ以上は分かれない葉](deep-terminal.md)**
