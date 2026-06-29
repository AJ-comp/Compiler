# FIRST · FOLLOW — 例題集 (自分で解いてみる)

> 🎓 **発展コース · 理論** です。\
> [FIRST 計算規則](first-rules.md) · [FOLLOW 計算規則](follow-rules.md) を *いろいろな文法に直接適用* してみる **練習帳** です。

本文でずっと使った `Expr` 文法は ε（消える非終端記号）がないので、規則の一部は *実際に動く様子* を見られませんでした。そこでここでは **ε が入った文法** たちで、*簡単なものからだんだん手応えのあるものへ* 解いていきます。手で一緒についてくるのが一番です。

---

## 使う規則を一目で (チートシート)

解く前に — 使う公式は下に整理してあります。

- **[FIRST 公式まとめ](first-rules.md)**
- **[FOLLOW 公式まとめ](follow-rules.md)**

> 📎 下の例文法では — *小文字* `a b c d x y …` = **終端記号**、*大文字* `A B S E T …` = **非終端記号**、`ε` = *消える（空のもの）*。

---

## 例題 1 — ε が初登場 (あってもなくてもいい記号)

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="setm">b</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
</pre>

`A` が `a` にもなれるし、*何でもないもの（ε）* にもなれます — 「あってもなくてもいい」記号ですね。

### FIRST

`S → A b` は記号の並びなので、⊕ 公式から始めます。

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = FIRST(<span class="nt">A</span>) ⊕ FIRST(<span class="setm">b</span>)
</pre>

部品を埋めると —

<pre class="lrbox">
   FIRST(<span class="nt">A</span>) = { <span class="setm">a</span>, ε }     ← A → a | ε なので nullable
   FIRST(<span class="setm">b</span>) = { <span class="setm">b</span> }
</pre>

`FIRST(A)` に ε があるので — ε を抜いて次のマス `b` まで ⊕：

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = { <span class="setm">a</span>, ε } ⊕ { <span class="setm">b</span> }
            = ( { <span class="setm">a</span>, ε } − ε ) ∪ { <span class="setm">b</span> }
            = { <span class="setm">a</span>, <span class="setm">b</span> }
</pre>

→ **`FIRST(S) = { a, b }`**（S は ε を出せないので nullable ではない）。

### FOLLOW

<pre class="lrbox">
   ① 開始記号 <span class="nt">S</span>   →  FOLLOW(<span class="nt">S</span>) = { $ }
   ② <span class="nt">S</span> → <span class="nt">A</span> <span class="setm">b</span> で A の後ろ β = "<span class="setm">b</span>"  →  FIRST(<span class="setm">b</span>) − ε = { <span class="setm">b</span> }  →  FOLLOW(<span class="nt">A</span>) ⊇ { <span class="setm">b</span> }
</pre>

`A` はどの生成規則でも *末尾* ではありません（`b` が常に後ろにある）→ 規則 ③ は該当なし。

<pre class="lrbox">
   FOLLOW(<span class="nt">S</span>) = { $ }
   FOLLOW(<span class="nt">A</span>) = { <span class="setm">b</span> }
</pre>

> 🔖 **この例題で新しく見たもの** — ε が `FIRST(A)` に入り、`FIRST(S)` を求めるとき ⊕ が *「先頭が消えうるので次のマスまで」* 進んでいく場面。（Expr 文法では見られなかったものです。）

---

## 例題 2 — nullable が連続 (ε が二つ続けて)

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span> <span class="setm">c</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

今度は `A` も `B` も消えられます。`S` の FIRST を求めるとき ⊕ が *二マス* 飛び越えるか見てみましょう。

### FIRST

`S → A B c` は記号の並びなので、⊕ 公式から始めます。

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = FIRST(<span class="nt">A</span>) ⊕ FIRST(<span class="nt">B</span>) ⊕ FIRST(<span class="setm">c</span>)
</pre>

部品を埋めると —

<pre class="lrbox">
   FIRST(<span class="nt">A</span>) = { <span class="setm">a</span>, ε }     ← nullable
   FIRST(<span class="nt">B</span>) = { <span class="setm">b</span>, ε }     ← nullable
   FIRST(<span class="setm">c</span>) = { <span class="setm">c</span> }
</pre>

左から ⊕ — ε があればそのマスを抜いて次のマスまで：

- `FIRST(A)` に ε あり → `a` を拾って **次のマスへ**
- `FIRST(B)` に ε あり → `b` を拾って **さらに次のマスへ**
- `FIRST(c)` には ε なし → `c` を拾って **止まる**

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = { <span class="setm">a</span>, ε } ⊕ { <span class="setm">b</span>, ε } ⊕ { <span class="setm">c</span> }
            = { <span class="setm">a</span> } ∪ { <span class="setm">b</span> } ∪ { <span class="setm">c</span> } = { <span class="setm">a</span>, <span class="setm">b</span>, <span class="setm">c</span> }
</pre>

→ **`FIRST(S) = { a, b, c }`**（末尾の `c` が終端記号なので S は nullable ではない）。

### FOLLOW

<pre class="lrbox">
   ① 開始記号 <span class="nt">S</span>   →  FOLLOW(<span class="nt">S</span>) = { $ }
</pre>

**② — `A` の後ろ、`B` の後ろを見ます。**

<pre class="lrbox">
   <span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span> <span class="setm">c</span>   :  A の後ろ β = "<span class="nt">B</span> <span class="setm">c</span>"   →  FIRST(<span class="nt">B</span> <span class="setm">c</span>) − ε
   <span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span> <span class="setm">c</span>   :  B の後ろ β = "<span class="setm">c</span>"     →  FIRST(<span class="setm">c</span>)   − ε = { <span class="setm">c</span> }
</pre>

`FIRST(B c)` を ⊕ で：`FIRST(B)={b,ε}` に ε あり → `b` + 次のマス `FIRST(c)={c}` → `{ b, c }`（ε なし、止まる）。ε が混ざっていないのでそのまま：

<pre class="lrbox">
   FOLLOW(<span class="nt">A</span>) ⊇ { <span class="setm">b</span>, <span class="setm">c</span> }
   FOLLOW(<span class="nt">B</span>) ⊇ { <span class="setm">c</span> }
</pre>

`A`·`B` どちらも *末尾* ではなく、後ろ（`B c`、`c`）が ε で全部消えることもありません（末尾に終端記号 `c`）→ 規則 ③ は該当なし。

<pre class="lrbox">
   FOLLOW(<span class="nt">S</span>) = { $ }
   FOLLOW(<span class="nt">A</span>) = { <span class="setm">b</span>, <span class="setm">c</span> }
   FOLLOW(<span class="nt">B</span>) = { <span class="setm">c</span> }
</pre>

> 🔖 **この例題で新しく見たもの** — ⊕ が *nullable 二マス* を連続して飛び越えること（`FIRST(S)`）、そして FOLLOW ② が *終端記号一つではなく `FIRST(β)`（複数の記号の並び）* を材料に使う場面。

---

## 例題 3 — 本当に手応えのあるもの (再帰 + ε + 継承) ★

本文の `Expr` 文法を、左再帰の代わりに **ε で解いた** いとこです。コンパイラ教科書の定番文法で — 規則が *全部* 一度ずつ回ります。

<pre class="lrbox">
<span class="nt">E</span>  → <span class="nt">T</span> <span class="nt">E'</span>
<span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> | ε
<span class="nt">T</span>  → <span class="nt">F</span> <span class="nt">T'</span>
<span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span> | ε
<span class="nt">F</span>  → <span class="setm">'('</span> <span class="nt">E</span> <span class="setm">')'</span> | <span class="setm">id</span>
</pre>

`E'` と `T'` が *末尾* です — さらに付くことも（`+ T E'`）、終わることも（ε）あります。だから両方とも nullable。

### FIRST

簡単なもの（末尾のほう）から上がっていきます。

<pre class="lrbox">
   FIRST(<span class="nt">F</span>)  : <span class="nt">F</span> → <span class="setm">'('</span> <span class="nt">E</span> <span class="setm">')'</span>  → { <span class="setm">(</span> }
               <span class="nt">F</span> → <span class="setm">id</span>         → { <span class="setm">id</span> }              →  FIRST(<span class="nt">F</span>)  = { <span class="setm">(</span>, <span class="setm">id</span> }

   FIRST(<span class="nt">T'</span>) : <span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span>  → { <span class="setm">*</span> }
               <span class="nt">T'</span> → ε         → { ε }               →  FIRST(<span class="nt">T'</span>) = { <span class="setm">*</span>, ε }

   FIRST(<span class="nt">T</span>)  : <span class="nt">T</span> → <span class="nt">F</span> <span class="nt">T'</span>  =  FIRST(<span class="nt">F</span>) ⊕ FIRST(<span class="nt">T'</span>)
                         =  { <span class="setm">(</span>, <span class="setm">id</span> } (ε なし, 止まる)  →  FIRST(<span class="nt">T</span>)  = { <span class="setm">(</span>, <span class="setm">id</span> }

   FIRST(<span class="nt">E'</span>) : <span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> → { <span class="setm">+</span> }
               <span class="nt">E'</span> → ε        → { ε }                →  FIRST(<span class="nt">E'</span>) = { <span class="setm">+</span>, ε }

   FIRST(<span class="nt">E</span>)  : <span class="nt">E</span> → <span class="nt">T</span> <span class="nt">E'</span>  =  FIRST(<span class="nt">T</span>) ⊕ FIRST(<span class="nt">E'</span>)
                         =  { <span class="setm">(</span>, <span class="setm">id</span> } (ε なし, 止まる)  →  FIRST(<span class="nt">E</span>)  = { <span class="setm">(</span>, <span class="setm">id</span> }
</pre>

> `E` · `T` · `F` の FIRST が `{ (, id }` で同じです — 本文 `Expr` 文法とまったく同じ答えですね。かたちを変えただけで *同じ言語* ですから。

### FOLLOW

**初期値 — ①② から。**

<pre class="lrbox">
   ① 開始記号 <span class="nt">E</span>                →  FOLLOW(<span class="nt">E</span>) ⊇ { $ }
   ② <span class="nt">F</span> → <span class="setm">'('</span> <span class="nt">E</span> <span class="setm">')'</span> : E の後ろ ")"   →  FOLLOW(<span class="nt">E</span>) ⊇ FIRST("<span class="setm">)</span>") = { <span class="setm">)</span> }
</pre>

では **継承（③）** が必要な箇所を探します。*nullable な末尾* のせいで ③ があちこちで引っかかります。

<pre class="lrbox">
   <span class="nt">E</span>  → <span class="nt">T</span> <span class="nt">E'</span>   : E' が末尾          →  FOLLOW(<span class="nt">E'</span>) ⊇ FOLLOW(<span class="nt">E</span>)
   <span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> : E' が末尾         →  FOLLOW(<span class="nt">E'</span>) ⊇ FOLLOW(<span class="nt">E'</span>)   (自分自身, 新しいものなし)
   <span class="nt">E</span>  → <span class="nt">T</span> <span class="nt">E'</span>   : T の後ろ β = "<span class="nt">E'</span>", でも E' は nullable!
                   → ② FIRST(<span class="nt">E'</span>) − ε = { <span class="setm">+</span> }   そして  ③ FOLLOW(<span class="nt">T</span>) ⊇ FOLLOW(<span class="nt">E</span>)
   <span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> : T の後ろ "<span class="nt">E'</span>" — 上と同じ  →  { <span class="setm">+</span> },  FOLLOW(<span class="nt">T</span>) ⊇ FOLLOW(<span class="nt">E'</span>)
   <span class="nt">T</span>  → <span class="nt">F</span> <span class="nt">T'</span>   : T' が末尾          →  FOLLOW(<span class="nt">T'</span>) ⊇ FOLLOW(<span class="nt">T</span>)
   <span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span> : T' が末尾         →  FOLLOW(<span class="nt">T'</span>) ⊇ FOLLOW(<span class="nt">T'</span>)   (自分自身)
   <span class="nt">T</span>  → <span class="nt">F</span> <span class="nt">T'</span>   : F の後ろ β = "<span class="nt">T'</span>", T' も nullable!
                   → ② FIRST(<span class="nt">T'</span>) − ε = { <span class="setm">*</span> }   そして  ③ FOLLOW(<span class="nt">F</span>) ⊇ FOLLOW(<span class="nt">T</span>)
   <span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span> : F の後ろ "<span class="nt">T'</span>" — 上と同じ  →  { <span class="setm">*</span> },  FOLLOW(<span class="nt">F</span>) ⊇ FOLLOW(<span class="nt">T'</span>)
</pre>

**繰り返して固めると**（`E'` は `E` を、`T` は `E` を、`T'`·`F` は `T` を受け継ぎます）：

<pre class="lrbox">
   FOLLOW(<span class="nt">E</span>)  = { $, <span class="setm">)</span> }
   FOLLOW(<span class="nt">E'</span>) = { $, <span class="setm">)</span> }                  ← E から継承
   FOLLOW(<span class="nt">T</span>)  = { <span class="setm">+</span> } ∪ FOLLOW(<span class="nt">E</span>)  = { <span class="setm">+</span>, $, <span class="setm">)</span> }
   FOLLOW(<span class="nt">T'</span>) = FOLLOW(<span class="nt">T</span>)          = { <span class="setm">+</span>, $, <span class="setm">)</span> }   ← T から継承
   FOLLOW(<span class="nt">F</span>)  = { <span class="setm">*</span> } ∪ FOLLOW(<span class="nt">T</span>)  = { <span class="setm">*</span>, <span class="setm">+</span>, $, <span class="setm">)</span> }
</pre>

> 🔖 **この例題で新しく見たもの** — ③ 継承が *本当に* 回る様子。とくに **`T` の後ろの `E'` が nullable なので、② の `{+}` だけでなく ③ で `FOLLOW(E)` まで一緒に受け取る** 場面が核心です。（「後ろが消えうるなら事実上の末尾」→ ③。）

---

## 自分で解いてみる

規則が手になじんだか — 自分で一度やってみてください。（答えは下に折りたたんであります。）

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">B</span> <span class="setm">a</span>
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

`FIRST(S)` · `FIRST(B)` · `FOLLOW(S)` · `FOLLOW(B)` を求めてみてください。

<details>
<summary>答えを見る</summary>

<pre class="lrbox">
   FIRST(<span class="nt">B</span>) = { <span class="setm">b</span>, ε }
   FIRST(<span class="nt">S</span>) = FIRST(<span class="nt">B</span>) ⊕ FIRST(<span class="setm">a</span>) = { <span class="setm">b</span> } ∪ { <span class="setm">a</span> } = { <span class="setm">a</span>, <span class="setm">b</span> }
              (B が nullable なので a まで進む)

   FOLLOW(<span class="nt">S</span>) = { $ }                          ① 開始記号
   FOLLOW(<span class="nt">B</span>) = FIRST(<span class="setm">a</span>) − ε = { <span class="setm">a</span> }           ② B の後ろ "<span class="setm">a</span>"
              (B は末尾ではなく後ろが終端記号 a なので ③ なし)
</pre>

</details>

---

## 次へ

この練習で、FIRST/FOLLOW が *どんな文法でも* どう回るのか、感覚が掴めたはずです。\
ではこの二つを *材料* として使う — **LR パーサーの構文解析表** へ進みます。

👉 **[LR パーサー — 構文解析表を作る](lr-item.md)**

---

👈 前へ： [FOLLOW · 実装](follow-impl.md)
