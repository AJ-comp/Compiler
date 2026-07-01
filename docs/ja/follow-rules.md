# FOLLOW · 計算規則

> 🎓 **発展コース · 理論** です。\
> 前の [FOLLOW · 定義と導出](follow-formula.md)では — 定義を立て、定義どおりに導出していくうちに *「一番後ろに来たら
> LHS の FOLLOW を継承する」* という壁にぶつかりましたね。\
> このページではその過程を **規則3つ** に整理して、FIRST のときと同じように **反復** で解きます。（実装は → **[FOLLOW · 実装](follow-impl.md)**。）

> 一度に全部を受け入れようとしないでくださいね。\
> **やさしいものから一歩ずつ** 進みます。

## まず — 何を埋めるのか

FOLLOW は FIRST と *埋める対象* からして違います。\
FIRST は終端記号まで含めて8個を求めましたが、**FOLLOW は非終端記号だけ** を求めます。\
（終端記号の後ろに何が来るかは別に考える必要がないんです — 「どの規則が終わったか」を判断するのは非終端記号ですからね。）

ですから私たちの文法では `Expr` · `Term` · `Factor` **3つの FOLLOW** だけを埋めればいいんです。\
埋めるための道具は規則3つ — さきほど導出しながら出会った、まさにその3つです。

- **規則 ①** — 開始記号には `$`
- **規則 ②** — `B` のすぐ後ろに来るものの FIRST
- **規則 ③** — `B` が一番後ろに来たら LHS の FOLLOW を継承

やさしいものから一つずつ。

## 規則 ① — 開始記号には `$`

**開始記号の FOLLOW には `$`（入力の終わり）を入れます。**

なぜでしょう？\
[定義と導出](follow-formula.md)で見たように、開始記号は *入力全体* なので、それを最後まで読み切ったら、すぐ後ろは **入力の終わり** だからです。

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>) ⊇ { $ }
</pre>

## 規則 ② — `B` のすぐ後ろに来るものの FIRST

生成規則に `A → α B β` のように（`α` は `B` の *前* に来る何でも、`β` は `B` の *後ろ* に来る何でも）`B` の次に **何か（β）** が来る場合です。

**すると、その `β` が作り出す最初の終端記号が `B` のすぐ後ろに来られます。** → `FOLLOW(B)` に `FIRST(β)` を入れます。（ただし、`ε` は除いて。）

なぜでしょう？\
`B` の次に `β` が付くので、`β` が導出する *先頭の終端記号* がそのまま `B` のすぐ後ろに来る終端記号ですよね。\
その「先頭の終端記号」がまさに [FIRST(β)](first-rules.md) です。（**FOLLOW が FIRST を材料として使う地点** です。）

> 📎 `ε` はなぜ除くの？\
> `FIRST(β)` に `ε` があるということは *「β がまるごと消える可能性もある」* という意味です。\
> `ε` は終端記号ではなく「消失」なので、終端記号の集合である FOLLOW には入れません。\
> その代わり β が消えると `B` が実質的に一番後ろになるので — それは **規則 ③** が引き受けてくれます。

私たちの文法では `β` がいつも終端記号で始まるので簡単です。

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      前の <span class="nt">Expr</span> の後ろ β = "<span class="setm">'+'</span> <span class="nt">Term</span>"  →  FIRST = <span class="setm">'+'</span>   →  FOLLOW(<span class="nt">Expr</span>) ⊇ { <span class="setm">'+'</span> }
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     <span class="nt">Expr</span> の後ろ β = "<span class="setm">')'</span>"          →  FIRST = <span class="setm">')'</span>   →  FOLLOW(<span class="nt">Expr</span>) ⊇ { <span class="setm">')'</span> }
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    前の <span class="nt">Term</span> の後ろ β = "<span class="setm">'*'</span> <span class="nt">Factor</span>" →  FIRST = <span class="setm">'*'</span>   →  FOLLOW(<span class="nt">Term</span>) ⊇ { <span class="setm">'*'</span> }
</pre>

## 規則 ③ — `B` が一番後ろに来たら LHS の FOLLOW を継承  ★

これが [定義と導出](follow-formula.md)でコールアウトとして指摘した **あの核心の規則** です。

生成規則に `A → α B` のように `B` が **一番後ろ** に来たら：

> **`FOLLOW(B)` はその生成規則の LHS である `A` の FOLLOW をまるごと受け継ぎます。** → `FOLLOW(B) ⊇ FOLLOW(A)`。

なぜでしょう？\
`B` が `A` の末尾の場所を占めるので、*`A` の次に来られるもの* がそのまま *`B` の次に来られるもの* なんです。\
（`( Expr )` → `( Term )` で `)` が `Term` の後ろへ移ってきた、あの場面です。）

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      <span class="nt">Term</span> が一番後ろ   →  FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)
   <span class="nt">Expr</span> → <span class="nt">Term</span>               <span class="nt">Term</span> が一番後ろ   →  FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    <span class="nt">Factor</span> が一番後ろ  →  FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)
   <span class="nt">Term</span> → <span class="nt">Factor</span>             <span class="nt">Factor</span> が一番後ろ  →  FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)
</pre>

## なぜ一度では終わらないのか — 反復

規則 ③ がやっかいです。\
`FOLLOW(Term)` に `FOLLOW(Expr)` を注がなければならないのに、その `FOLLOW(Expr)` もまだ埋まっている途中かもしれません。\
FIRST のとき再帰で行き詰まったのとまったく同じですね — **互いに依存** していて一度では解けません。

ですから処方も同じです — **規則 ①·② で初期値を埋めたあと、規則 ③ を *変わらなくなるまで* 反復。**

## 定義どおりに — 私たちの文法で回してみる

<div class="ex-card">

**① `初期値` — 規則 ①·② でまず埋めます**

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }      ← ① の $ , ② の <span class="setm">'+'</span> <span class="setm">')'</span>
   FOLLOW(<span class="nt">Term</span>)   = { <span class="setm">'*'</span> }              ← ② の <span class="setm">'*'</span>
   FOLLOW(<span class="nt">Factor</span>) = { }                  ← まだ空
</pre>

</div>

<div class="ex-card">

**② `1周目` — 規則 ③ で継承したら増えました**

<pre class="lrbox">
   FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)  →  { <span class="setm">'*'</span> } ∪ { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }  =  { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }   （増えた）
   FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)  →  { }    ∪ { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> } = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> } （増えた）
</pre>

→ 何かが増えたので、もう一周回します。

</div>

<div class="ex-card">

**③ `2周目` — もう増えなければ止まります**

<pre class="lrbox">
   FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)  →  変化なし
   FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)  →  変化なし
</pre>

→ 今回の周では何も増えませんでした。**停止！**

</div>

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
   FOLLOW(<span class="nt">Term</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
   FOLLOW(<span class="nt">Factor</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

[定義と導出ページ](follow-formula.md)·[基本コース](first-follow.md)で求めた答えと正確に同じです。 ✓

## ε が含まれる場合 — 小さな文法で一度

上の expr 文法には消える（nullable）非終端記号がないので、規則 ② の *`− ε`* と規則 ③ の *「β が消えたら」* の枝が一度も回りませんでした。この二つを目で見るために、この節で例に使う文法は以下のとおりです：

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

`A` と `B` がそれぞれ ε に消えうります。

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">A</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">a</span>, ε <span class="setb">}</span>
   <span class="setf">FIRST(</span><span class="nt">B</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">b</span>, ε <span class="setb">}</span>
</pre>

では `FOLLOW(A)` を見ます。`S → A B` で `A` の後ろに `β = B` が付いています。

- **規則 ②** — `FOLLOW(A)` に `FIRST(B) − ε` を入れます：`{ b, ε } − ε` = `{ b }`。← *ここで `− ε` が実際に働きます。*
- **規則 ③** — ところが `B` が *消えうる* ので、`A` も実質的に末尾になりえます。だから LHS の `S` の FOLLOW も受け継ぎます：`FOLLOW(A) ⊇ FOLLOW(S)`。

`FOLLOW(S)` は開始記号なので規則 ① で `{ $ }` です。そして `B` は `S → A B` の末尾なので、規則 ③ でその `FOLLOW(S)` をそのまま受け継ぎます。

<pre class="lrbox">
   FOLLOW(<span class="nt">S</span>) = <span class="setb">{</span> <span class="setm">$</span> <span class="setb">}</span>
   FOLLOW(<span class="nt">B</span>) = <span class="setb">{</span> <span class="setm">$</span> <span class="setb">}</span>              <span style="opacity:.6">B が末尾 → FOLLOW(S) を継承</span>
   FOLLOW(<span class="nt">A</span>) = <span class="setb">{</span> <span class="setm">b</span>, <span class="setm">$</span> <span class="setb">}</span>          <span style="opacity:.6">FIRST(B)−ε = {b}、さらに B が消えうるので FOLLOW(S) を継承</span>
</pre>

`B` が *消えられなかったら* `FOLLOW(A) = { b }` で終わるところですが、`B` が ε になりうるので `$` まで受け継ぎました。規則 ② の `− ε` と規則 ③ の「消えたら継承」が両方ともここで実際に回ります。

## まとめ

規則に使う記号は `A → α B β` がすべてです。

<pre class="lrbox">
   <span class="nt">A</span> → α <span class="nt">B</span> β
   <span class="nt">A</span>, <span class="nt">B</span> = 非終端記号
   α, β = 記号の並び <span style="opacity:.6">(B の前・後ろの部分 — 終端記号・非終端記号が混じっても、無くてもよい)</span>
</pre>

1. **開始記号**（`Expr`）の FOLLOW に `$`。
2. `B` のすぐ後ろの `β` の **`FIRST(β) − ε`** を `FOLLOW(B)` に。（FIRST を材料に！）
3. `B` が **一番後ろ**（または `B` の後ろが *全部消える可能性があれば*）なら **LHS（`A`）の FOLLOW** を継承 — *変わらなくなるまで反復*。

FIRST を材料に使い、反復で継承を解く — これが FOLLOW 計算のすべてです。 🎯

## 次 — この規則がコードに

この3つの規則と反復が `FirstFollowAnalyzer` のコードにどう入っているかを見ます。\
（`CalculateAllFollow` の最初の行が `CalculateAllFirst` であることからして、FIRST を材料として使うのがそのまま見てとれます。）

👉 **[FOLLOW · 実装](follow-impl.md)**

---

👈 前へ: [FOLLOW · 定義と導出](follow-formula.md)
