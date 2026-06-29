# FIRST / FOLLOW

この章からはいよいよ LR 構文解析の「心臓部」に入っていきます。\
最初の概念は **FIRST 集合** と **FOLLOW 集合** です。

正直に先にお伝えしておきます — 名前からして耳慣れませんし、初めて見ると「これって一体何だろう？」と思うかもしれません。\
**でも大丈夫です。みんな必ず一度はここで立ち止まるものなんです。**\
コンパイラの授業で一番つまずきやすいのが、まさにここなんですよ。\
ですから、一度で腑に落ちなくても、まったくおかしなことではありません。\
本当にゆっくり、手を取って一緒に進んでいきます。\
ゆっくりついてくれば、この二つの集合がなぜ必要なのか、自然と見えてくるはずです。🙂

---

> 📖 **始める前に：** この章では、例文法を *読める* ことを前提にしています。`:` `|` `;` のような表記が
> 不慣れなら、まず [文法の読み方](grammar-reading.md) を見てきてください — 5分もあれば十分です。

この章でずっと使う例文法です（参考用）。

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
<span class="setm">id</span>     := "[a-zA-Z]+" ;
</pre>

---

## ① なぜ必要なのか

パーサーがトークンを左から一つずつ読み進めていくと、**絶え間なく決定を下さなければ** なりません。\
その中でもっとも重要な決定が、次の二つです。

1. **「いま、どの規則を始めてもいいのか？」** — 次のトークンを見て、どこへ入るかを選ばなければならない
2. **「いま読んでいた規則は終わったのか？」** — どこまでが一つのかたまりなのか、いつまとめるかを決めなければならない

言葉だけだと抽象的ですよね。\
例を挙げてみましょう。\
`a + a * a` を読んでいて、**最初の `a` をちょうど見たところ** だとしましょう。\
パーサーはこんな悩みに陥ります。

> 「この `a` 一つで一つのかたまり（`Term`）を終わらせたことにしようか？ それとも、後ろに `* 何か` がさらに付く、
> まだ終わっていないかたまりだろうか？」

どう判断するのでしょう？\
意外と簡単です。\
**次のトークンをちらっと見ます。**

- 次が `*` → まだ終わっていない（掛け算がさらに付く）
- 次が `+` か、入力の終わり → ここで終わり（まとめてよい）

つまりパーサーは、**「どんなトークンがこのかたまりの *次に* 来られるか」** をあらかじめ知っておかなければなりません。\
それがまさに **FOLLOW** です。\
そして **「どんなトークンがこのかたまりを *始める* ことができるか」** が **FIRST** です。

> 💡 あまり深く考えないでください。一行でまとめるとこうです。
> **FIRST/FOLLOW = パーサーが「始まり／終わり」を判断するために、あらかじめ用意しておく *チートシート*。**
> これがあって初めて、後で出てくる [構文解析表](parse-table-build.md) を作れるようになります。

## ② 何をするのか

> ここからは集合を自分の手で求めていきます。**計算がちょっと多く見えても、気負わないでください。**
> パターンは単純で、ゆっくり一行ずつ一緒に埋めていきますから。

### FIRST — 「これはどんなトークンで *始まる* ことができるか」

**FIRST(X)** = X を導出して **一番最初に現れうる終端記号（トークン）の集まり**。（= その文字列の *先頭* に来る終端記号）

非終端記号の三つ — `Factor` · `Term` · `Expr` — の FIRST を一つずつ求めてみましょう。一番簡単なものから。

<div class="ex-card">

**① `Factor` — 詰まることなく終わります**

<pre class="lrbox">
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
</pre>

`Factor` は `(` で始まるか `id` で始まりますよね？ ですから：

<pre class="lrbox">
FIRST(<span class="nt">Factor</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

> `{ }` は *集合* を表す記号です — 中括弧の中に入っているのがその候補たちです。

</div>

<div class="ex-card">

**② `Term` — 自分自身がまた出てきます**

<pre class="lrbox">
<span class="nt">Term</span> : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
</pre>

`Term` は `Term` で始まるか（自分自身！ — さっきのあの再帰ですね）、`Factor` で始まります。\
自分自身で始まるものをずっと展開していくと、結局のところ先頭は `Factor` になります。\
ですから：

<pre class="lrbox">
FIRST(<span class="nt">Term</span>) = FIRST(<span class="nt">Factor</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

</div>

<div class="ex-card">

**③ `Expr` — 同じかたち**

同じ理屈で `Expr` も：

<pre class="lrbox">
FIRST(<span class="nt">Expr</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

</div>

三つとも `(` か `id` で始まりますね。\
筋が通っていますよね？\
どんな数式も、結局のところ先頭は **名前（`id`）** か **開き括弧（`(`）** ですから。\
**ここまでついてこられたら、FIRST は終わりです！**\
思ったほど大したことなかったでしょう？

### FOLLOW — 「これの *次に* どんなトークンが来られるか」

**FOLLOW(X)** = 正しい文のどこかで X の **すぐ次に現れうる終端記号の集まり**。
ここには特別な記号 **`$`**（入力の終わりを表す仮想のトークン）も入ることがあります。

今回も三つ — `Expr` · `Term` · `Factor` — を一つずつ見ていきます。

<div class="ex-card">

**① `Expr` — 開始記号から**

「`Expr` の次には何が来られるか？」を文法全体から探してみると：

- `Expr` は **開始記号** です（文全体が `Expr`）→ ですから `Expr` の次には **入力の終わり `$`** が来られる
- `Expr : Expr '+' Term` から → 最初の `Expr` の次には `+`
- `Factor : '(' Expr ')'` から → `Expr` の次には `)`

全部集めると：

<pre class="lrbox">
FOLLOW(<span class="nt">Expr</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
</pre>

</div>

<div class="ex-card">

**② `Term` — `Expr` の FOLLOW を受け継ぎます**

`Term` も同じように「Term の次に来るもの」を洗い出してみると：

- `Expr` の一番末尾が `Term` である場合がありますよね（`Expr : ... Term`、`Expr : Term`）→ そうすると **Expr の次に
  来られるものは Term の次にも来られます** → FOLLOW(Expr) がそのまま入ってくる
- `Term : Term '*' Factor` → 最初の `Term` の次には `*`

<pre class="lrbox">
FOLLOW(<span class="nt">Term</span>) = FOLLOW(<span class="nt">Expr</span>) ∪ { <span class="setm">'*'</span> } = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

> `∪` は *和集合* の記号です — 二つの集まりをただ合わせるという意味です。

</div>

<div class="ex-card">

**③ `Factor` — `Term` とまったく同じように**

`Factor` も同じやり方で：

<pre class="lrbox">
FOLLOW(<span class="nt">Factor</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

</div>

### さあ、もう一度、最初のあの悩みに戻りましょう

覚えていますよね？\
`a`（つまり `Factor` → `Term`）を読んで、次のトークンを見たときのあの悩み。\
もう答えが見えています。

- 次が `*` → 「もっと進め」という合図（`Term '*' Factor` へ）
- 次が `+` または `$` → この二つは **FOLLOW(Term)** にあります → 「Term が終わったから **まとめろ（reduce）**」

**まさにこうやって FOLLOW が「いつまとめるか」を決めるのです。**\
FIRST/FOLLOW がなぜ構文解析表の材料なのか、ここで明らかになりますね。\
次の章でその表を自分で作ってみると、もっとはっきりします。

### 特殊な状況が二つ（今は軽くだけ）

あまり深く入らなくて大丈夫です。\
「こういうのがあるんだな」くらいで。

- **ε（イプシロン、空文字列）：** ある非終端記号が「何でもないもの」になりうるときに使う表示です。
  私たちの例には出てこないので、今は気にしなくて大丈夫です。
- **`$`（終わりの印）：** さっき見たとおり、入力の終わりを表す仮想のトークンです。開始記号の FOLLOW には必ず入ります。

## ③ プレイグラウンドで見る

FIRST/FOLLOW そのものは画面に直接は見えませんが、**その産物である構文解析表の reduce マス** で
その効果を目で見ることができます。\
プレイグラウンドで：

1. 既定の文法と入力 `a + a * a` で **Run**
2. **Parse table** で、次のトークンが `+`/`)`/`$`（つまり FOLLOW(Term)）のときに緑色の **reduce** バッジが
   現れる位置を確認 — まさに FOLLOW が「ここでまとめろ」と命じた場所です。
3. **Step through** で一マスずつ進んでみると、`a` の後ろのトークンが `*` か `+` かによって動作が分かれるのが見えます。

👉 **[ライブプレイグラウンド](https://polite-island-0b2142200.7.azurestaticapps.net)**

> （FIRST/FOLLOW 集合を *直接* 表で見せるパネルは追加予定です。）

---

## もう一歩 — 発展コース（任意）

ここまでが **概念** です。\
そして — 正直なところ、基本コースは **ここまでで十分** です。\
次の基本の章へそのまま進んでもらってかまいません。\
ただ、もっと深く掘り下げたい方のために、道を一つ開けておきます。

> 🎓 いま私たちは *この例文法* の FIRST/FOLLOW を手で求めましたね。これを **どんな文法にも通じる
> 公式（アルゴリズム）** として整理し、それが **Janglim のコードでどう実装されているか** まで見るのは —
> **発展コース** の [FIRST — 定義と導出](first-formula.md)（→ 計算規則 → 実装）で別に扱います。
>
> **見なくてもまったく問題ありません。** 概念はもうすべて掴めているのですから。🙂

---

## 次の章

FIRST/FOLLOW という材料が用意できました。\
**ここまで本当によくついてきてくれました 🎉**\
次は — パーサーが *「いまどこまで読んだのか」* をどうやって覚えているか（**ドット**）、そして *「いま可能なこと」* を
集めた **状態** です。\
それが集まると、いよいよあの有名な **構文解析表** になります。

👉 **[ドットと状態](dot-and-state.md)**
