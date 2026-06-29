# 閉包 · 計算法

> 🎓 **発展コース** です。\
> 前の [閉包 · 定義](closure-def.md)では — `CLOSURE(I)` は *「②について閉じている最小の集合」* だとお話ししましたね。\
> 定義は *「どんな集合か」* だけを教えてくれます。このページでは、それを **実際にどうやって求めるのか** を — しかも私たちの
> 例題文法の **本当の開始状態 `I₀`** を自分で作ってみながら — 一段ずつ見ていきます。

## 方法

方法そのものは単純です。

> あるアイテム集合 `I` から始めて、**規則②を *もう追加するものがなくなるまで* 一段ずつ適用** します。

肝心なのは *「もう追加するものがなくなるまで」* です。集合が一段ずつ **育っていって**、ある瞬間に *何も
増えなくなったら* — それで閉じたことになり、止まります。\
[FIRST/FOLLOW](first-rules.md)で見た **不動点（変わらなくなるまで繰り返す）** とまったく同じ感じです。

## その前に — 出発点はどこ？（拡大文法）

閉包は *開始アイテム1個* から出発します。その1個はどこから来るのでしょうか？

ここで小さな仕掛けが1つ登場します。LRパーサーはもともとの文法の一番上に **開始規則をたった1つだけ付け足します** —
`Accept → Expr` です。（`Expr` は私たちの文法のもともとの開始記号ですね。）\
このように *開始規則を1つ上乗せした* 文法を **拡大文法（augmented grammar）** と呼びます。

なぜこんなものを付け足すのでしょうか？ — それは **パーサーが「これで入力全体が終わった」をすっきり知るため** です。\
もともとの開始記号 `Expr` は式の *中* のあちこちに登場します（`Expr '+' Term` のその `Expr`、`'(' Expr ')'` のその
`Expr` のようにです）。だから `Expr` を1つ完成させたからといって *入力全体* が終わったわけではありません — もっと大きな式の
一部かもしれませんからね。\
でも *どこにも使われない* `Accept` を1つ上乗せしておけば — `Accept → Expr` を完成させる **その瞬間こそが
「全体の終わり（accept）」** だと一発で分かります。（この「終わりの合図」が実際にどう使われるのかは *構文解析表* の章で
締めくくります。今は *「出発点」* としてだけ使います。）

簡単に言えば — **宅配の箱** を思い浮かべてみてください。箱の中にまた小さな箱が入っていることがありますよね（私たちの `Expr` の中に
`Expr` がまた入るように — `'(' Expr ')'` のようにです）。すると *「どれが一番外側の箱なのか」* が紛らわしくなります。\
だから **全体をもう一度包む外箱** をかぶせて、*「これが最後の外装だよ」* と書いておくのです。\
その外箱がまさに `Accept → Expr` — 外箱まですべて閉じれば（完成させれば）*「全部終わり（accept）！」* と一発で
分かります。

そういうわけで出発アイテムはたった1つ — `Accept → • Expr` です。\
これに閉包をかけた結果が、すなわち **開始状態 `I₀`** なのです。実際に育っていく様子を見てみましょう。

## 一段ずつ — `I₀` が育つ様子を見ながら

たどっていく前に **拡大文法** を広げておきますね。（各段で *どの生成規則が* 入ってくるのか、ここで
すぐさま確認できるようにです。）

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>   |  <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>  |  <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     |  <span class="setm">id</span></pre>

**スタート — 1個。** 仮想の開始アイテム1つから出発します。

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span></pre>

**ステップ1。** `Accept → • Expr` のドットの後ろは `Expr` ですね。\
文法で `Expr` の行を見ると、生成規則は **`Expr → Expr '+' Term`** と **`Expr → Term`** の2つです。\
この2つを — まだ何も読んでいないのでドットを一番前に打って — 集合に追加します。

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>       ← 新規
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>                ← 新規</pre>

→ **3個。**

**ステップ2。** たった今入ってきた2つのドットの後ろを見ます。

- `Expr → • Term` のドットの後ろは `Term` です。文法で `Term` の生成規則は **`Term → Term '*' Factor`**
  と **`Term → Factor`** の2つですね。この2つを（ドットを一番前に打って）追加します。
- `Expr → • Expr '+' Term` のドットの後ろも `Expr` ですが — `Expr` は **ステップ1ですでに展開しました**（その2つの規則は
  すでに集合にありますね）。だから新しく入れるものはありません。

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>     ← 新規
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>              ← 新規</pre>

→ **5個。**

**ステップ3。** またたった今入ってきた2つのドットの後ろを見ます。

- `Term → • Factor` のドットの後ろは `Factor` です。文法で `Factor` の生成規則は **`Factor → '(' Expr ')'`**
  と **`Factor → id`** の2つですね。この2つを追加します。
- `Term → • Term '*' Factor` のドットの後ろは `Term` → **すでに展開したので** 新しいものはなし。

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>        ← 新規
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span>                  ← 新規</pre>

→ **7個。**

**ステップ4。** 残ったドットの後ろを見ます。`Factor → • '(' Expr ')'` のドットの後ろは `'('`、`Factor → • id` のドットの後ろは
`id` — どちらも **終端記号** です。\
終端記号は *始めるべき生成規則* がないので展開するものがありません。もう追加するものもありません → **閉じました。終わり！**

## まとめ — これが `I₀` です

集合が **1個 → 3個 → 5個 → 7個** と *育っていって、もう増えるものがなくなって止まりました。*\
この最後の **7個からなる閉じた集合** こそ、私たちの文法の **開始状態 `I₀`** です。

<pre class="lrbox">   I₀ = CLOSURE( { <span class="nt">Accept</span> → •<span class="nt">Expr</span> } )
      = <span class="nt">Accept</span>·<span class="nt">Expr</span>·<span class="nt">Term</span>·<span class="nt">Factor</span> の生成規則がすべてドットを一番前に並べた7個のアイテム</pre>

*「一番前に来うるものすべて」* が `I₀` に詰め込まれているのです — `Factor → •'(' Expr ')'` と `Factor → •id`
のドットの後ろの `(`·`id` が、すなわち *一番最初に読みうる終端記号* ですね。（おや、それって [FIRST(Expr)](first-rules.md)
`= { '(', id }` と同じですね！ 偶然ではありません。）

## もう一歩 — 著者はこの過程を「再帰」で書きます

今しがたは集合が **横に一段ずつ育っていく** 様子を見ましたね。同じ計算を、著者の設計ノートはもう少し圧縮された
かたちで書きます — **`Closure` の中にまた `Closure`** が入る *再帰* の形です。

読み方さえ分かれば難しくありません。

- `Closure({ … })` は *「このアイテムたちはまだもっと展開しなければならない」* という印です。
- <span class="lrmark">赤い記号</span> は *今展開している最中のドットの後ろの記号*（マーカー）です。
- 1行下がるたびに、その赤い非終端記号の生成規則が新しい `Closure({ … })` として引き込まれてきます。（前の行で
  すでに整理されたアイテムは *省略* して、一番最後の行に全部まとめます。）

<pre class="lrbox">Closure({ <span class="nt">Accept</span> → • <span class="lrmark">Expr</span> })
 = { <span class="nt">Accept</span> → • <span class="nt">Expr</span>,   Closure({ <span class="nt">Expr</span> → • <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>,  <span class="nt">Expr</span> → • <span class="nt">Term</span> }) }
 = { <span class="nt">Expr</span> → • <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>,  <span class="nt">Expr</span> → • <span class="lrmark">Term</span>,   Closure({ <span class="nt">Term</span> → • <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>,  <span class="nt">Term</span> → • <span class="nt">Factor</span> }) }
 = { <span class="nt">Term</span> → • <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>,  <span class="nt">Term</span> → • <span class="lrmark">Factor</span>,   Closure({ <span class="nt">Factor</span> → • <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>,  <span class="nt">Factor</span> → • <span class="setm">id</span> }) }
 = { <span class="nt">Factor</span> → • <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>,  <span class="nt">Factor</span> → • <span class="setm">id</span> }      <span style="opacity:.6">（ドットの後ろが '(' · id — 終端記号なので停止）</span>
 = { <span class="nt">Accept</span>→•<span class="nt">Expr</span>, <span class="nt">Expr</span>→•<span class="nt">Expr</span><span class="setm">'+'</span><span class="nt">Term</span>, <span class="nt">Expr</span>→•<span class="nt">Term</span>, <span class="nt">Term</span>→•<span class="nt">Term</span><span class="setm">'*'</span><span class="nt">Factor</span>, <span class="nt">Term</span>→•<span class="nt">Factor</span>, <span class="nt">Factor</span>→•<span class="setm">'('</span><span class="nt">Expr</span><span class="setm">')'</span>, <span class="nt">Factor</span>→•<span class="setm">id</span> }   <span style="opacity:.6">= I₀</span></pre>

一番最後で `Closure({ … })` が *消えましたね？* もう展開するものがないという意味 — つまり **閉じた** のです。それが `I₀` です。\
（*「すでに展開した非終端記号は二度と展開しない」* という約束もそのままです — だから `Expr → •Expr '+' Term` のその
`Expr`、`Term → •Term '*' Factor` のその `Term` はマーカーとして拾われません。そうしないと永遠に回り続けてしまいますからね。）

そしてこの **`Closure` が `Closure` を呼ぶ再帰の形** こそ — まさに次の [実装](closure-impl.md) のコード
`result.UnionWith(Closure( … ))` と *瓜二つ* なのです。手で書いていたものをそのままコードに移したわけですね。\
（著者の元のノートは自分のテスト文法 `S' → G`、`G → E = E | f`、… で描かれていますが、展開する原理は
まったく同じです。）

## 次へ

手で回してみたこの「閉じるまで一段ずつ」を、コードはどうやったのでしょうか？

👉 **[閉包 · 実装](closure-impl.md)**

---

👈 これまで: [閉包 · 定義](closure-def.md)
