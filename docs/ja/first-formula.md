# FIRST · 定義と導出

> 🎓 **発展コース · 理論** です。[基本コースの FIRST/FOLLOW](first-follow.md) で *概念* を先に
> 掴んでくると良いです。このページでは **FIRST が正確には何なのか（定義）** と、その **定義どおりに自分で
> 導出して求めていく過程** までを見ます。
>
> 気負わないでください — ゆっくり進みます。

> 📍 **ある場所** · エンジン `FirstFollowAnalyzer` · モジュール `Janglim.FrontEnd` — **Layer 2**（構文解析表より
> *下* の段にあたる土台）

ずっと使う例文法です。

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
</pre>

基本で手で求めた答えは `FIRST(Expr) = FIRST(Term) = FIRST(Factor) = { '(', id }` でした。\
ですがその前に — **FIRST が *正確に* 何なのか** から、はっきりと定義しておきます。\
（これが抜けると、後の計算規則がただ暗記する呪文のようになってしまいます。）

---

## 定義 — FIRST とは何か

まず一行で釘を刺しておきます。

> **FIRST(X)** = 記号 `X` を **導出して一番最初に現れうる終端記号** の集合。

核心となる言葉が二つあります — **導出** と **一番最初に現れる終端記号**（つまり *先頭* に来るその終端記号）。\
一つずつ見ていきましょう。

### 「導出（derivation）」 — 規則で展開すること

非終端記号をその生成規則の右辺で **置き換える** 一歩を *導出* と呼び、矢印 `⇒` で書きます。

`Expr` を一歩ずつ展開してみます。

<pre class="lrbox">
   <span class="nt">Expr</span>  ⇒  <span class="nt">Term</span>  ⇒  <span class="nt">Factor</span>  ⇒  <span class="setm">id</span>
        (Expr:Term)  (Term:Factor)  (Factor:id)
</pre>

規則を三回適用して、結局 **終端記号だけからなる文字列** `id` に到達しましたね。\
このように *何歩も* 展開することを `⇒*` と書きます → `Expr ⇒* id`（「Expr は id を導出する」）。

### だから FIRST は

`X` を *あらゆる方法で* 最後まで展開して、そこで **一番最初に現れうる終端記号**（= 先頭に来る終端記号）を全部集めたものです。

`Expr` で実際に見てみると：

<pre class="lrbox">
   <span class="nt">Expr</span> ⇒* <span class="setm">id</span> …             →  先頭が <span class="setm">id</span>   ⇒   <span class="setm">id</span> ∈ FIRST(<span class="nt">Expr</span>)
   <span class="nt">Expr</span> ⇒* <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span> …       →  先頭が <span class="setm">(</span>    ⇒   <span class="setm">(</span> ∈ FIRST(<span class="nt">Expr</span>)
</pre>

`Expr` が作れる文字列は無数にありますが（`id`、`id + id`、`( id ) * id`、…）、**先頭に来うる
終端記号** は結局 `id` か `(` の二つだけです。だから：

<pre class="lrbox">
   FIRST(<span class="nt">Expr</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

記号でもっとかっちり書くとこうです（`T` = 終端記号の集合）：

<pre class="lrbox">
   FIRST(<span class="nt">X</span>) = { <span class="setm">a</span> ∈ T | <span class="nt">X</span> ⇒* <span class="setm">a</span> … (<span class="setm">a</span> で始まる文字列を導出) }
</pre>

> 📎 **ε（空文字列）を一つだけ追加で。** もし `X` が *何でもないもの* まで導出できるなら
> （`X ⇒* ε`）、**ε も FIRST(X) に入れます。** 「X はまるごと消えることもある」という表示です。（私たちの
> 例にはそういう非終端記号がないので ε は出てきません — でも定義には必ず含まれます。）

整理すると — 終端記号でも、非終端記号でも、*複数の記号からなる並び* でも、**「導出して一番最初に現れうる終端記号（＋必要なら ε）の集合」** が FIRST です。\
これが定義のすべてです。

---

## 定義どおりに — 導出して自分で求めてみる

ではこの「導出して先頭の終端記号を集める」を、その定義を *直接適用* して手で求めてみます。\
簡単な `Factor` から。

### Factor — 詰まることなく最後まで展開されます

`Factor` の二つの **生成規則**（`|` で区切られた一行ずつ — `A → α` の形、ここで `α` は *右辺の記号の並び*。詳しくは [Single](deep-single.md)）を最後まで導出してみましょう。

<pre class="lrbox">
   <span class="nt">Factor</span> ⇒ <span class="setm">id</span>              →  先頭の終端記号 :  <span class="setm">id</span>
   <span class="nt">Factor</span> ⇒ <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span>        →  先頭の終端記号 :  <span class="setm">(</span>
</pre>

`Factor` が生み出す文字列の先頭は `id` か `(` — 二つだけです。\
定義どおりに集めると：

<pre class="lrbox">
   FIRST(<span class="nt">Factor</span>) = { <span class="setm">id</span>, <span class="setm">'('</span> }
</pre>

簡単ですよね？\
`Factor` の中には自分自身が出てこないので、導出がきれいに終わります。

### Term — 展開していくと自分自身がまた出てきます

`Term : Term '*' Factor | Factor`。\
二つの生成規則を導出してみましょう。

<pre class="lrbox">
   ① <span class="nt">Term</span> ⇒ <span class="nt">Factor</span> ⇒ … ⇒ <span class="setm">id</span> または <span class="setm">(</span>          →  先頭 :  <span class="setm">id</span>, <span class="setm">(</span>
   ② <span class="nt">Term</span> ⇒ <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>                  →  先頭がまた <span class="nt">Term</span> ?!
</pre>

②番が妙です。\
先頭が *自分自身* `Term` なので、先頭の終端記号を知るには、その `Term` を **また** 展開しなければなりません。

<pre class="lrbox">
   <span class="nt">Term</span> ⇒ <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
        ⇒ <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
        ⇒ …                              ←  展開しても展開しても先頭はずっと <span class="nt">Term</span>、終わりが見えない
</pre>

でも結局、先頭の `Term` もいつかは `Factor` の生成規則に降りていき、そうすると先頭の終端記号はまた `id` か `(` になります。
だから：

<pre class="lrbox">
   FIRST(<span class="nt">Term</span>) = { <span class="setm">id</span>, <span class="setm">'('</span> }
</pre>

`Expr`（`Expr : Expr '+' Term | Term`）も同じかたちなので、`FIRST(Expr) = { id, '(' }` になります。

### ここで引っかかります — 「最後まで」導出できません

`Term` で見たように、**自分自身を抱えていると（再帰）、導出が無限に長くなりうるのです。**\
定義は明確なのに（「導出して一番最初に現れる終端記号」）、その導出を *一つひとつ最後まで* 展開するのは、手でもコンピューターでも厄介です。

## 次へ — 計算規則へ

そこで次のページでは — **導出を直接展開しなくても** 同じ FIRST を取り出せる **計算規則** へ
進みます。\
（再帰もおとなしく処理されます。）

👉 **[FIRST · 計算規則](first-rules.md)**

---

👈 基本概念へ戻る： [FIRST / FOLLOW](first-follow.md)
