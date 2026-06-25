# Terminal — これ以上分けられない葉

> 🎓 **発展コース** です。\
> 前章 [Symbol](deep-symbols.md) では、すべての記号に共通する根っこを見ましたね。\
> いよいよ、その二つの枝のうち、よりシンプルなほう — **葉(トークン)である Terminal** から見ていきます。

`Terminal` は、入力に本当に登場する、これ以上分けられない断片です。\
`+`、`*`、`(`、`id` のような
ものですね。\
これをコードで表そうと机に向かった著者は — **何から決めたのでしょう?**

## 最初の悩み — 「字句解析器が照らし合わせる *値* がなくちゃ」

トークンの本質は「入力のこの断片」です。\
だから、字句解析器が入力と **照らし合わせる実際の値** が、まず最初に
必要になりますね。\
それが `Value` です。

```csharp
public string Value { get; } = string.Empty;
```

> 著者にとって `Value` は *「あればうれしいもの」* ではなく ***「必ず与えなければならないもの」*** です。\
> 照らし合わせる値の
> ないトークンは、存在する意味がありませんから。\
> だからコードでも `Value` は、ユーザーが与えた値を **そのまま**
> 保管し、決して空のままにしません。

## 二つ目の悩み — 「でも *見せる名前* は値と違うこともあるよね」

ここで [Symbol の章](deep-symbols.md) の **「アイデンティティ ↔ 表示の分離」** という哲学が具体化されます。\
著者はこんな
ケースを思い浮かべたはずです:

> *「`id` トークンの実際の値(Value)は正規表現 `[a-zA-Z]+` だけど、画面にはただ 'id' と見せたい。\
> 値と表示が違うわけだ。\
> なら **表示用の名前を別に** 用意しよう。」*

そうして `Caption` が入りました(`ToString()` もこれを使います)。

```csharp
public string Caption { get; } = string.Empty;
public override string ToString() => Caption;
```

そして — この部分は *推測ではなく、著者がコードのコメントに直接書き残した* 内容です。

`Caption` は、表・診断・FIRST/FOLLOW 出力など **表示に使われます。**\
だから null だとテキストレンダラーが
壊れてしまいます。

そこで caption がなければ value で埋めます。\
**ただし `Value` には手をつけません** — 字句解析器のマッチング値であって
表示文字列ではないからです。

```csharp
// caption이 null이면 value로 fallback (표시가 null이면 텍스트 렌더러가 깨지므로).
// 단, Value 자체는 준 그대로 — 렉서의 매칭 값/패턴이라 절대 바꾸지 않는다.
this.Caption = caption ?? value ?? string.Empty;
```

表示が壊れるのは fallback で防ぎつつ、**アイデンティティである値はそのまま。**\
Symbol のあの哲学が、たった一行に込められて
いますね。

## 助けてくれる情報たち — 種類、意味、正規表現

トークン一つをきちんと扱うには、もういくつか必要でした。\
著者が一つずつ付け足していったものです。

- **`TokenType`** — これが演算子なのか、区切り記号なのか、数値なのか、キーワードなのか。(字句解析器がどう扱うかが分かれます。)
- **`Meaning`** — このトークンが *意味のある* トークンなのか。(あとで AST を作るとき、残すか捨てるかの基準。)
- **`RegexExpression`** — `Value` を、実際に字句解析器が使う **正規表現** に変換します(演算子/数値/単語ごとに異なるように)。

```csharp
public TokenType TokenType { get; }
public bool Meaning { get; } = true;
public string RegexExpression => (IsOper) ? ... : (IsNumber) ? Value : ... ;
```

## そして — 葉だから *中が空っぽです*

`Terminal` が `NonTerminal` と決定的に違う点が一つ。\
**Terminal は中に「代替(alters)」が
ありません。**\
これ以上ほどくものがありませんから。\
文字どおり **葉** なんです。\
(次章の NonTerminal は正反対で、
中に代替をたっぷり抱え込みます。)

## 特別な葉をいくつか — ε と $

最後に、著者は *本物の入力にはないけれど、構文解析には必ず必要な* 偽のトークンをいくつか `Terminal` の子として
作っておきました。\
[FIRST/FOLLOW](first-follow.md) で出会った、まさにそれらです。

```csharp
public class Epsilon   : Terminal { ... }   // ε — "빈 것"
public class EndMarker : Terminal { ... }   // $ — "입력의 끝"
```

これらは **固定された UniqueKey** を持ちます(`KeyManager.EpsilonKey` など)。\
どこで作っても *いつも同じ
一つ* として扱われるようにです。\
(これもまた Symbol の「アイデンティティはキー」という哲学ですね。)

## ひと目で — Terminal の全体像

`Terminal` クラスの **全体の骨組み** です。\
ロジックは省いて *何があるのか* だけを見せます。

```csharp
public class Terminal : Symbol
{
    // ── 무엇인가 ────────────────────────────
    public TokenType TokenType { get; }
    public string Value { get; }        // 렉서가 맞춰볼 값 (필수 · 건드리지 않음)
    public string Caption { get; }       // 표시 이름 (ToString 이 이걸 씀)
    public bool Meaning { get; }         // 의미 있는 토큰인가 (AST 용)
    public bool IsWordPattern { get; }

    // ── 파생 정보 ───────────────────────────
    public bool IsOper { get; }          // 연산자/구분자류인가
    public bool IsNumber { get; }
    public string RegexExpression { get; }   // Value → 실제 렉서용 정규식

    // ── 생성 ────────────────────────────────
    public Terminal(TokenType type, string value, bool meaning = true, bool bWord = false);
    public Terminal(TokenType type, string value, string caption, ...);

    // ── 표현 ────────────────────────────────
    public override string ToString();   // → Caption
    public override string ToEbnfString(bool bContainLHS = false);
    public override string ToGrammarString();
    public override string ToTreeString(ushort depth = 1);
}

// 특수한 잎들 — 전부 Terminal 의 자식, 고정 UniqueKey
public class Epsilon        : Terminal { }   // ε — 빈 것
public class EndMarker      : Terminal { }   // $ — 입력의 끝
public class InputTerminal  : Terminal { }
public class NotDefined     : Terminal { }
public class CustomTerminal : Terminal { }
```

## 次の章

`Terminal` — 葉一つを表すために、著者が **何を、なぜ** 入れたのかを見ました(値 vs 表示、種類、そして
ε・$ のような特殊な葉まで)。\
いよいよ反対側 — 中に代替を抱え込む **枝、NonTerminal** へ進みます。

👉 **[NonTerminal — 中に規則を抱える枝](deep-nonterminal.md)**
