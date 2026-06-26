# クイックスタート

この章の目標はたった一つです。\
**5行のコードで、自分で定義した文法を使って最初の構文解析を動かしてみること。**
詳しい説明は次の章からゆっくりやっていきますし、ここではただ一つ — *構文解析とは何か* だけを軽く押さえて
すぐに動かしてみましょう。

## その前に — 構文解析って何ですか?

一文で済みます。\
私たちが `a + a * a` のような **文字の並び(テキスト)** をコンピュータに渡すと、コンピュータの
目には最初はただの *文字の山* でしかありません。\
何の意味もありませんよね。

**構文解析(parsing)** とは、この文字の山が **文法に合っているか確認し、その中の構造を木の形で
明らかにする** 仕事です。\
国語の時間に文を「主語 + 述語」に分けていたのとまったく同じです。

```
a + a * a   →   "アア、これは (a) 足す (a 掛ける a) 構造なんだな"
```

この構造が分かって初めて、その次(計算・翻訳・コンパイル)に進めます。\
構文解析はほぼすべての言語処理の
**最初の関門**です。\
さあ — この「構文解析」を、これから Janglim で実際にやってみましょう。

## インストール

まずパッケージを入れます。\
.NET プロジェクトで:

```bash
dotnet add package Janglim --prerelease
```

> `--prerelease` はまだ初期プレビューなので付けます。

> 🌿 **インストールが面倒なら少し後回しにしても大丈夫です。** すぐ後で書くコードと *まったく同じもの* を **ブラウザで
> クリックするだけ** で試せる [ライブプレイグラウンド](https://polite-island-0b2142200.7.azurestaticapps.net)が
> あるんです。そこでは構文解析表・構文木まで図で見せてくれます。まずは目で先に見てきてもいいですよ。

## 最初の構文解析 (5行)

インストールしたら、コンソールプロジェクトの `Program.cs` に下記をそのまま貼り付けてみてください。

```csharp
using Parse.FrontEnd.Grammars.Ebnf;   // 文法をテキストとして読むリーダー
using Parse.FrontEnd.Parsers.LR;       // LALR パーサ
using Parse.FrontEnd.Tokenize;         // レクサ(トークナイザ)

// ① 文法を EBNF テキストで定義する
var read = EbnfGrammarReader.Read(@"
    Expr   : Expr '+' Term | Term ;
    Term   : Term '*' Factor | Factor ;
    Factor : '(' Expr ')' | id ;
    id     := ""[a-zA-Z]+"" ;");

var grammar = read.Grammar;

// ② レクサを作り、文法が使うトークン規則を登録する
var lexer = new Lexer();
foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);

// ③ パーサを作り
var parser = new LALRParser(grammar);

// ④ 入力をトークンに分割して(lexing)構文解析する
var result = parser.Parsing(lexer.Lexing("a + a * a").TokensForParsing);

// ⑤ 結果の確認
Console.WriteLine(result.Success);   // True
```

`dotnet run` すると **`True`** が表示されます。\
たった今、あなたは *自分で定義した文法* で文字列を構文解析した
わけです。 🎉

## たった今、何が起きたのでしょうか

コードの五つのかたまりが、それぞれ一つずつ仕事をしました。\
今は **名前だけ目に慣れれば十分です** — 一つひとつは
これから一章ずつゆっくりと解きほぐしていきますから。

| ステップ | やったこと |
|---|---|
| ① `EbnfGrammarReader.Read` | テキストで書いた文法 → `Grammar` オブジェクトへ |
| ② `lexer.AddTokenRule` | 文字を *トークン* に分割する規則を登録 |
| ③ `new LALRParser(grammar)` | 文法から **構文解析表**を前もって計算 |
| ④ `parser.Parsing(...)` | トークンたちを実際に構文解析 (shift / reduce) |
| ⑤ `result.Success` | 入力が文法に合っていたか (`True` / `False`) |

一つだけ押さえておきましょう。\
③でさらっと **「構文解析表を *前もって* 計算する」** と言いましたよね?\
実はこれが LR
パーサの最大の特徴であり、このマニュアルの核心となる旅路なのです。\
そして — **すぐ次の章がまさにその話**です。

## 次の章

たった今は「動くもの」を手で触ってみました。\
ここで一歩下がって **全体像** を見てみましょう — テキストが
結果になるまで、内部でどんなステップが流れているのか、そしてさっき言ったあの「前もって作る表」とは何なのかを。

👉 **[ひと目で分かるパイプライン](the-big-picture.md)**
