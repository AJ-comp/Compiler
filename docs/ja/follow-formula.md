# FOLLOW · 定義と導出

> 🎓 **発展コース · 理論** です。\
> [FIRST トラック](first-formula.md)を終えたので、いよいよその相棒の **FOLLOW** です。\
> FIRST が **「何で *始まる* か」** だったとすれば、FOLLOW は **「その *次に* 何が来るか」** です。\
> このページでは **定義** と **定義どおりに直接導出** するところまで見ます。続いて → **計算規則** → **実装**。

> 📍 **ある場所** · エンジン `FirstFollowAnalyzer` · モジュール `Janglim.FrontEnd` — **Layer 2**（構文解析表よりも
> *下* の段である土台側）

ずっと使っている例の文法です。\
今回は **開始記号** が大事なので、印を付けておきますね — 一番上の `Expr` が開始記号です。

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;      ← 開始記号
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
</pre>

[基本コース](first-follow.md)で手で求めた答えはこうでした。

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
   FOLLOW(<span class="nt">Term</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
   FOLLOW(<span class="nt">Factor</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

これを *定義* からあらためて一つずつ見ていきます。

---

## 定義 — FOLLOW とは何か

まず一行で言い切っておきますね。

> **FOLLOW(B)** = 正しい文のどこかで、非終端記号 `B` の **すぐ次に** 現れうる **終端記号** の
> 集合。\
> （そして `B` が文の *一番後ろ* に来られるなら、入力の終わりを表す `$` も入れます。）

FIRST と対にして見ると、FIRST はその記号を *導出して一番最初に* 現れうる終端記号、FOLLOW はその記号の *すぐ後ろ* に現れうる終端記号です。

### 「すぐ後ろ」を導出で見る

[FIRST の定義](first-formula.md)で *導出（⇒）* を定義しましたね — 非終端記号を生成規則の右辺に置き換える
ことでした。\
FOLLOW は、開始記号 `Expr` から導出を進めていって **`B` のすぐ後ろにどんな終端記号が付くか** を見る
ものです。

<pre class="lrbox">
   <span class="nt">Expr</span>  ⇒*  …  <span class="nt">B</span>  <span class="setm">a</span>  …       →  <span class="setm">a</span> が <span class="nt">B</span> のすぐ後ろに来た   ⇒   <span class="setm">a</span> ∈ FOLLOW(<span class="nt">B</span>)
</pre>

記号できっちり書くとこうです（`S` = 開始記号、`T` = 終端記号の集合）。

<pre class="lrbox">
   FOLLOW(<span class="nt">B</span>) = { <span class="setm">a</span> ∈ T | <span class="nt">S</span> ⇒* … <span class="nt">B</span> <span class="setm">a</span> … }   ∪   ( { $ }  if  <span class="nt">S</span> ⇒* … <span class="nt">B</span> )
</pre>

> 📎 FIRST と決定的に違う点が一つ。\
> FIRST はその記号 **一つだけ** を見れば済みましたが、FOLLOW は **`B` が文法全体で *どこどこで使われているか* を
> すべて見渡す** 必要があります。`B` の後ろに何が来るかは、`B` を使った場所ごとに違うからです。

---

## 定義どおりに — 導出して直接求めてみる

「`B` のすぐ後ろに来る終端記号を集める」を、定義を *そのまま適用* して手で求めてみますね。

### FOLLOW(Expr) — きれいに出ます

`Expr` は **開始記号** です — つまり *入力全体* が一つの `Expr` なんですね。\
では、その `Expr` を最後まで全部読み終えたら、すぐ後ろには何が来るでしょう？\
もう読むものがない **入力の終わり** です。\
その入力の終わりを表す仮想のトークンが `$` でしたね。\
ですから `Expr` のすぐ後ろには `$` が来ることになります — **`$ ∈ FOLLOW(Expr)`。**

では文法全体で `Expr` の *すぐ後ろ* に来るものを探してみます。

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      前の <span class="nt">Expr</span> のすぐ後ろ :  <span class="setm">'+'</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     <span class="nt">Expr</span> のすぐ後ろ :       <span class="setm">')'</span>
</pre>

`Expr` の後ろに来られるのは `'+'` と `')'`、そして一番後ろの `$`。全部集めると：

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
</pre>

### FOLLOW(Term) — 一番後ろに来ると受け継ぎます

`Term` の *すぐ後ろ* を文法から探してみます。

<pre class="lrbox">
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    前の <span class="nt">Term</span> のすぐ後ろ :  <span class="setm">'*'</span>           →  <span class="setm">'*'</span> を追加
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      <span class="nt">Term</span> が生成規則の一番後ろ …          →  後ろに何もないね？
   <span class="nt">Expr</span> → <span class="nt">Term</span>               <span class="nt">Term</span> が生成規則の一番後ろ …          →  やはり後ろに何もない
</pre>

`'*'` はそのまま入ります。\
ところが後ろの二行が不思議です — `Term` が生成規則の **一番後ろ** にあるので、すぐ後ろに終端記号がありません。

では `Term` の次には何が来るのでしょう？\
`Expr → Term` を見ると、`Term` が `Expr` の *全部* です — つまり **`Expr` と `Term` が同じ場所で終わるんですね。**

言葉だと抽象的なので、一つの場面で見てみますね。\
`( Expr )` という断片を思い浮かべてください（`Factor → '(' Expr ')'`）。ここでは `Expr` のすぐ後ろに `)` が来ますね。\
ところがその `Expr` がただの `Term` 一つだったら（`Expr → Term`）、同じ場所が `( Term )` になります。

<pre class="lrbox">
   <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span>        →   <span class="nt">Expr</span> の後ろに <span class="setm">)</span>
   <span class="setm">(</span> <span class="nt">Term</span> <span class="setm">)</span>        →   今度は <span class="nt">Term</span> の後ろに <span class="setm">)</span>   （同じ場所！）
</pre>

見てください — さっき `Expr` の後ろにあった `)` が、今度は **`Term` のすぐ後ろ** に来ています。\
`Term` が `Expr` の末尾の場所をそのまま受け継いだからです。

> 💡 **これが FOLLOW の核心の規則です。**\
> ある非終端記号が生成規則の **一番後ろ** に来ると、その生成規則の **LHS（左側の非終端記号）の FOLLOW を
> まるごと受け継ぎます。**\
> ここでは `Expr → Term` なので — **`FOLLOW(Expr)` がそのまま `FOLLOW(Term)` に流れ込んできます。**

<pre class="lrbox">
   FOLLOW(<span class="nt">Term</span>) = { <span class="setm">'*'</span> }  ∪  FOLLOW(<span class="nt">Expr</span>)
                = { <span class="setm">'*'</span> }  ∪  { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }   =   { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

### ε が含まれる場合 — 小さな文法で

ここまで `B` の後ろの `β` はいつも終端記号（`'+'`、`')'`、`'*'`）で単純でした。ところが `β` が *消えうる* 非終端記号だと、もう一段増えます。私たちの expr 文法にはそういう非終端記号がないので、この節で例に使う文法は以下のとおりです：

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

`A` と `B` がそれぞれ ε に消えうります。`FOLLOW(A)` を定義どおりに見ると、`S → A B` で `A` のすぐ後ろには `B` が来ます。

- `B` が *消えないなら*：`A` の後ろの最初の終端記号は `B` の先頭 `b` です。→ `b ∈ FOLLOW(A)`。（`FIRST(B)` の `ε` は終端記号ではないので抜いて、`FIRST(B) − ε` だけ入れます。）
- `B` が *消えるなら*：`S ⇒ A B ⇒ A`（B が ε）なので `A` が `S` の末尾になり、`A` の後ろは `S` の後ろ、つまり `FOLLOW(S)` をそのまま受け継ぎます。`FOLLOW(S) = { $ }`（開始記号）なので `$ ∈ FOLLOW(A)`。

<pre class="lrbox">
   FOLLOW(<span class="nt">A</span>) = ( FIRST(<span class="nt">B</span>) − ε )  ∪  FOLLOW(<span class="nt">S</span>)  =  <span class="setb">{</span> <span class="setm">b</span> <span class="setb">}</span> ∪ <span class="setb">{</span> <span class="setm">$</span> <span class="setb">}</span> = <span class="setb">{</span> <span class="setm">b</span>, <span class="setm">$</span> <span class="setb">}</span>
</pre>

`β`（ここでは `B`）が消えうるので `$` まで流れ込んだわけです。いま見た二つ、すなわち *`FIRST(β)` から `ε` だけ抜いて入れること* と *`β` が消えたら LHS の FOLLOW を受け継ぐこと* が、下のまとめの規則 2・3 です。

### なぜ一度で終わらないのか — 受け継ぐべき FOLLOW がまだ決まっていないことがあります

たった今見たように、`B` が生成規則の **一番後ろ** に来ると、その生成規則の **左側の非終端記号（LHS）の FOLLOW** が
必要です。\
ところがその LHS の FOLLOW も、まだ埋まっている途中かもしれません。（FIRST のときに再帰で行き詰まったのと
まったく同じ状況ですね。）\
ですから定義を *一つずつたどる* だけでは、一度で終わりません。

そこで次のページでは — この過程を **いくつかの規則に整理** して、FIRST のときと同じように **変わらなくなるまで反復**
で解きます。

---

## まとめ

定義どおりにたどってみると、FOLLOW は結局、三つで埋まりました。

1. **開始記号** の FOLLOW には `$` が入る。（文の一番後ろに来られるから。）\
   `$ ∈ FOLLOW(開始記号)`
2. `B` の **すぐ後ろに何かが来れば**、その後ろのものの *先頭の終端記号* が FOLLOW(B) に入る。\
   `A → α B β` なら、`FOLLOW(B)` に `FIRST(β) − ε`
3. `B` が生成規則の **一番後ろ** に来れば、その生成規則の **LHS の FOLLOW** を受け継ぐ。\
   `A → α B` なら、`FOLLOW(B)` に `FOLLOW(A)` を継承

## 次へ — この過程を規則に

この三つを *どんな文法にも* 通じる計算規則に整理して、反復で解くのが次の話です。

👉 **[FOLLOW · 計算規則](follow-rules.md)**

---

👈 基本概念に戻る: [FIRST / FOLLOW](first-follow.md)
