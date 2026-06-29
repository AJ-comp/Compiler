# クイックスタート

この章の目標はたった一つです。\
**5行のコードで、自分で定義した文法を使って最初の構文解析を動かしてみること。**
詳しい説明は次の章からゆっくり進めていきますので、ここではただ一つ — *構文解析とは何か* だけを軽く押さえて、
すぐに動かしてみましょう。

## その前に — 構文解析とは何でしょう？

一文で済みます。\
私たちが `a + a * a` のような **文字の並び（テキスト）** をコンピュータに渡すと、コンピュータの
目には最初はただの *文字の山* でしかありません。\
何の意味もないのです。

**構文解析（parsing）** とは、この文字の山が **文法に合っているかを確認し、その中の構造を木の形で
明らかにする** 作業です。\
国語の時間に文を「主語 + 述語」に分けていたのとまったく同じです。

```
a + a * a   →   "ああ、これは (a) 足す (a 掛ける a) という構造なんだな"
```

この構造が分かって初めて、その次（計算・翻訳・コンパイル）に進めます。\
構文解析はほとんどすべての言語処理の
**最初の関門** なのです。\
さあ — この「構文解析」を、これから Janglim で実際にやってみましょう。

## インストール

まずはパッケージを入れます。\
.NET プロジェクトで:

```bash
dotnet add package Janglim --prerelease
```

> `--prerelease` は、まだ初期プレビューなので付けています。

> 🌿 **インストールが面倒なら、少し後回しにしても大丈夫です。** すぐ後で書くコードと *まったく同じもの* を
> **ブラウザでクリックするだけ** で試せる [ライブプレイグラウンド](https://polite-island-0b2142200.7.azurestaticapps.net)が
> あるんです。そこでは構文解析表・構文木まで図で見せてくれます。まずは目で先に見てきても良いですよ。

## 最初の構文解析（5行）

インストールできたら、コンソールプロジェクトの `Program.cs` に下記をそのまま貼り付けてみてください。

```csharp
using Janglim.FrontEnd.Grammars.Ebnf;   // 文法をテキストとして読み込むリーダー
using Janglim.FrontEnd.Parsers.LR;       // LALR パーサー
using Janglim.FrontEnd.Tokenize;         // レキサー（トークナイザー）

// ① 文法を EBNF テキストで定義する
var read = EbnfGrammarReader.Read(@"
    Expr   : Expr '+' Term | Term ;
    Term   : Term '*' Factor | Factor ;
    Factor : '(' Expr ')' | id ;
    id     := ""[a-zA-Z]+"" ;");

var grammar = read.Grammar;

// ② レキサーを作り、文法が使うトークン規則を登録する
var lexer = new Lexer();
foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);

// ③ パーサーを作り
var parser = new LALRParser(grammar);

// ④ 入力をトークンに分割して（lexing）構文解析する
var result = parser.Parsing(lexer.Lexing("a + a * a").TokensForParsing);

// ⑤ 結果の確認
Console.WriteLine(result.Success);   // True
```

`dotnet run` すると **`True`** と表示されます。\
たった今あなたは *自分で定義した文法* で文字列を構文解析した
のです。🎉

## たった今、何が起きたのでしょうか

コードの五つのかたまりが、それぞれ一つずつ仕事をしました。\
今は **名前だけ目に慣らせば十分です** — 一つひとつは
これから一章ずつゆっくりと解きほぐしていきますから。

| ステップ | やったこと |
|---|---|
| ① `EbnfGrammarReader.Read` | テキストで書いた文法 → `Grammar` オブジェクトへ |
| ② `lexer.AddTokenRule` | 文字を *トークン* に分割する規則を登録 |
| ③ `new LALRParser(grammar)` | 文法から **構文解析表** を前もって計算 |
| ④ `parser.Parsing(...)` | トークンたちを実際に構文解析（shift / reduce） |
| ⑤ `result.Success` | 入力が文法に合っていたか（`True` / `False`） |

一つだけ押さえておきましょう。\
③でさらっと **「構文解析表を *前もって* 計算する」** と言いましたよね？\
実はこれが LR
パーサーの最大の特徴であり、このマニュアルの核心となる旅路なのです。\
そして — **すぐ次の章がまさにその話** です。

## 次の章

たった今は「動くもの」を手で触ってみました。\
ここで一歩下がって **全体像** を見てみましょう — テキストが
結果になるまで、内部でどんなステップが流れていくのか、そして今言ったあの「前もって作る表」とは何なのかを。

👉 **[ひと目で見るパイプライン](the-big-picture.md)**
