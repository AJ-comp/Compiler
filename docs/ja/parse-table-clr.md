# 構文解析表 · CLR — しくみ

[SLR の章](parse-table-slr.md) で、SLR の弱点 — **見かけの衝突** — を見ました。その根っこは、SLR が reduce を決めるときに `FOLLOW` を *文法全体* から引っ張ってきたことにありましたね。だから *今この状態* とは無関係な文字まで一緒に紛れ込んでしまったのです。

答えははっきりしています — **グローバルな `FOLLOW` ではなく、*この状態にぴったり合った* 精密な先読みを使おう。** その先読みが *何であり、どうやって求めるのか* は [前章 — 先読み](parse-table-lookahead.md) で見ましたね。*項目ごと* に付く次の文字（LR(1) 項目）であり、その値は `FIRST(β t)` で求める、というものでした。

この章の **CLR** は — その先読みを *いちばん徹底的に*、状態を作る **その時から全面的に** 使う方式です。

---

## いちばん徹底した答え — 先読みを最初から

[前章](parse-table-lookahead.md) の LR(1) 項目（規則 + ドット + 先読み）を — 状態を作る *その時から* もれなく付けて回ります。

すると *見かけ（ドットの位置）は同じでも、先読みが違えば* 別々の状態に分かれます。状態ごとに *その文脈にぴったり合った* 先読みだけを持つので — **見かけの衝突がそもそも起きません。**

> 🔖 **CLR (Canonical LR) = LR(1)** — 先読みを状態に最初から埋め込み、文脈ごとに精密に分ける方式。

---

## 例題 — SLR の見かけの衝突を CLR で

[SLR の章](parse-table-slr.md) で見かけの衝突が起きた *その文法、その状態* を、CLR でもう一度見てみましょう。

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">c</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">e</span> <span class="nt">A</span> <span class="setm">d</span>
<span class="nt">A</span> → <span class="setm">b</span>
<span class="nt">B</span> → <span class="setm">b</span>
</pre>

SLR は `a b` 状態で `A → b •` の reduce を <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> *全体* に書き込むので — `d` で `B → b •` とぶつかってしまいました。（その `d` は *遠くの `e A d`* から紛れ込んだものです。）

**CLR は先読みを *文脈ごと* に持って回ります。** どういうことかというと — 状態を作るときに、各項目の横に *「この道で自分のうしろに来うる文字」* を一緒に書いておく、という意味です。ゆっくり追っていきましょう。

**① `a` を読んだなら — 私たちは今どの道の上にいるのか？**\
文法で `a` から始まる規則は *二つだけ* です。

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">c</span>      <span style="opacity:.65">← a の次に A、その次に c</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">d</span>      <span style="opacity:.65">← a の次に B、その次に d</span>
</pre>

`S → e A d` は `e` から始まるので、`a` を読んだ *今の私たち* とは関係のない道です。\
ですから `a` を読んだ瞬間 — 行ける道は **この二つだけ** です。

**② ではこの道で `A`・`B` のうしろには何が来るのか？**\
ここに *核心* があります。**`A` は文法に *二か所* 出てきますが — 出てくる場所ごとにうしろの文字が違うのです。**

- `a A c` の A → うしろは **`c`**
- `e A d` の A → うしろは **`d`**

この二つを *まるごと合わせた* ものが <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> です — SLR が使う *ひとまとめにした* 集合がまさにこれですね。

ところが私たちはたった今 **`a`** を読みました。すると *二つのうち前のほう* — `a A c` の A — の道に入ったわけです。（うしろの `e A d` は `e` から始まるので *すでに脱落* しています。）\
→ ですから *今この場所* の `A` のうしろには **`c` だけ。** `e A d` の `d` は *別の場所の A* の事情なので — ここには来られません。

（`B` は `a B d` の一か所にしか出てこないので、うしろはいつも `d` です。）

**③ CLR はそれをそのまま項目に書いておきます。**\
だから `a b` 状態の二つの完了項目は *それぞれ自分の文字* を付けています。

<pre class="lrbox">
<span class="nt">A</span> → <span class="setm">b</span> <span class="lrdot">•</span>   <span style="opacity:.65">うしろに来うる文字: c</span>
<span class="nt">B</span> → <span class="setm">b</span> <span class="lrdot">•</span>   <span style="opacity:.65">うしろに来うる文字: d</span>
</pre>

**まさにこれが SLR と分かれる地点です。** SLR は `A → b •` に *グローバルな* `FOLLOW(A)` をまるごと書き込んで `d` まで引きずってきましたが — CLR は *この道で本当に来うる* 文字だけを書きます。

| `a b` 状態 | `A → b •` が reduce する文字 | `B → b •` が reduce する文字 |
|:--|:--:|:--:|
| SLR（グローバル FOLLOW） | <code><span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> |
| **CLR（文脈ごと）** | <code><span class="setb">{</span><span class="setm"> c </span><span class="setb">}</span></code> ← `d` が抜けましたね！ | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> |

これで **`d` が入ってくると** — `A → b •` の文字（`c`）には `d` がないので *reduce せず*、`B → b •` の文字（`d`）にだけ合うので *`B` へ reduce します。* → 一つのマスでやることが *一つだけ* → **見かけの衝突が消えました。** ✅

CLR はこうやって *「この道で本当に来うる文字」* だけを見るので、`e A d` のような *別の道* の事情が割り込む余地がありません。だから *完璧に* 精密なのです。

---

## ただではありません — 状態の爆発

その代わり代償があります。見かけ（コア）が同じ状態でも先読みが違えばどんどん分かれてしまうので — **状態数が一気に膨らみます（爆発）。** 小さな文法では問題ありませんが、大きな文法ではこの爆発がかなりの負担になります。

---

## 次へ

CLR は *完璧ですが高くつきます。* この爆発をなくしながら精密度はほぼそのまま受け継ぐ実用形が — *すぐ次に続く* **LALR** です。

その前に — *私たちのエンジン* は CLR をどう実装したのでしょうか？（正直に言うと、ちょっと短いです。）

👉 **[構文解析表 · CLR — 実装](parse-table-clr-impl.md)**
