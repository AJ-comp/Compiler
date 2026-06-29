# 構文解析表 · LALR — しくみ

[CLR の章](parse-table-clr.md) で見たように — CLR は *完璧に精密* ですが **状態の爆発** が高くつきます。\
**LALR** はその二つを和解させた *実用形* です — 精密度は CLR 級、状態数は LR(0) 級。

---

## LALR = CLR をマージする

アイデアは一行です。**CLR の精密な先読みは活かしつつ、*見かけ（コア）が同じ状態どうしはまた一つにマージして* 状態数を LR(0) と同じだけ減らそう。**

（じつは私たちが [正準集合](canonical-set.md) で作った LR(0) 状態こそ、その「マージされた」コアです。だからエンジンは CLR をまるごと作らず、LR(0) 状態に先読みを *直接伝播* させて同じ結果を効率よく得ます — そのコードは [実装](parse-table-lalr-impl.md) で。）

> 🔖 **LALR (Look-Ahead LR)** — CLR の状態のうち *見かけ（コア）が同じものをマージして*、LR(1) 級の精密度を LR(0) の状態数で出す方式。

### マージを目で見る — 実際の状態と表

小さな文法で *状態を実際に作って* みましょう。

<pre class="lrbox">
1:  <span class="nt">S</span> → <span class="setm">b</span> <span class="nt">A</span> <span class="setm">x</span>
2:  <span class="nt">S</span> → <span class="setm">d</span> <span class="nt">A</span> <span class="setm">y</span>
3:  <span class="nt">A</span> → <span class="setm">c</span>
</pre>

`A → c` は `c` を読むと終わる（完了）規則です。ところがこの規則にたどり着く道が *二つ* あります — `b` を経た道と `d` を経た道です。

<div class="ex-card">

**① `CLR` — 状態が二つに分かれます**

**CLR で作ると — `A → c •` 状態が *二つ* に分かれます。**（先読みが道ごとに違うからです。）

<pre class="lrbox">
状態 5a :  <span class="nt">A</span> → <span class="setm">c</span> <span class="lrdot">•</span>   lookahead { <span class="setm">x</span> }    <span style="opacity:.65">b c で到着 — b A x の A のうしろは x</span>
状態 5b :  <span class="nt">A</span> → <span class="setm">c</span> <span class="lrdot">•</span>   lookahead { <span class="setm">y</span> }    <span style="opacity:.65">d c で到着 — d A y の A のうしろは y</span>
</pre>

だから CLR 状態は全部で **10個** — `0, 1, 2, 3, 4,` **`5a, 5b`** `, 6, 7, 8`。

</div>

<div class="ex-card">

**② `LALR` — コアが同じだからマージします**

**LALR — `5a` と `5b` は項目が `A → c •` でまったく同じです（コアが同じ）。** だから *一つの状態にマージし*、先読みは和集合でまとめます。

<pre class="lrbox">
状態 5 :  <span class="nt">A</span> → <span class="setm">c</span> <span class="lrdot">•</span>   lookahead { <span class="setm">x</span> , <span class="setm">y</span> }
</pre>

`5a` と `5b` が消えて、その場所に **状態 5 一つ。** 全体 **10個 → 9個** になりました。

</div>

<div class="ex-card">

**③ `構文解析表` — マージが表に刻まれた姿**

**その結果、実際の LALR 構文解析表はこう出ます。**\
（`sN` = 状態 N へシフト、`rN` = 規則 N で reduce、`acc` = 受理、空欄 = エラー。）

| 状態 | `b` | `d` | `c` | `x` | `y` | `$` | **S** | **A** |
|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| **0** | s2 | s3 | | | | | 1 | |
| **1** | | | | | | acc | | |
| **2** | | | s**5** | | | | | 4 |
| **3** | | | s**5** | | | | | 6 |
| **4** | | | | s7 | | | | |
| **5** | | | | **r3** | **r3** | | | |
| **6** | | | | | s8 | | | |
| **7** | | | | | | r1 | | |
| **8** | | | | | | r2 | | |

マージが表に *実際に刻まれた場所* は二つあります。

1. **状態 2 と状態 3 が `c` のマスで *どちらも状態 5* へ** 行きます（`s5`）。CLR なら 2 は `5a` へ、3 は `5b` へと *別々の* 状態に分かれていた場所です。
2. **状態 5 の一行** が `x`・`y` どちらも `r3`（= `A→c` で reduce）。CLR なら `5a` は `x` のマスだけ、`5b` は `y` のマスだけを埋めた *二行* だったものが — 一行に重なりました。

二つのマスとも動作が一つずつ（`r3` 一つ）なので — **衝突はありません。**

> *間違った入力は？* `b c y` を入れてみると：`b` → 状態 2、`c` → 状態 5。そこで `y` を見て `r3` で `A` へ reduce します（`y` が lookahead `{ x , y }` に入っているので）。そしてすぐ次の状態 4（`S → b A • x`）で `x` を待っているのに `y` が来たので → **エラー。** 間違った入力は *一マス遅れてでも* 同じように弾き出します。

</div>

---

（参考：[CLR の章](parse-table-clr.md) の a/b 文法では、`A → b •` を入れた二つの状態（`a b`、`e b`）の *コアが互いに違うので* — マージされる相手がいません。LALR はそこでは CLR の `{ c }` をそのまま使うので、同じく見かけの衝突は起きません。）

→ **精密度は CLR 級、状態数は LR(0) 級。** yacc・bison、そして **私たちのエンジンの実働パーサがすべて LALR** である理由です。

---

## ただし — マージが *まれに* 衝突をよみがえらせます

ほとんどいつもマージは無害です。ところが **ごくまれに**、マージした瞬間 *なかった衝突が生き返る* こともあります。（少し込み入っていますが、一度だけ付いてくれば大丈夫です。）

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">b</span> <span class="nt">B</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">e</span>
<span class="nt">S</span> → <span class="setm">b</span> <span class="nt">A</span> <span class="setm">e</span>
<span class="nt">A</span> → <span class="setm">c</span>
<span class="nt">B</span> → <span class="setm">c</span>
</pre>

`c` を読んだばかりの状態が *二か所* 出てきます。項目は `{ A→c•, B→c• }` で同じですが、*どこから来たか* によって先読みが食い違います。

| `c` を読んで到着した状態 | `A → c •` | `B → c •` |
|:--|:--:|:--:|
| `a c` のあと | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> | <code><span class="setb">{</span><span class="setm"> e </span><span class="setb">}</span></code> |
| `b c` のあと | <code><span class="setb">{</span><span class="setm"> e </span><span class="setb">}</span></code> | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> |

- **CLR** は — 二つを *別々* に置きます。各状態の中で重なりません。→ 衝突なし。
- **LALR** は — コアが同じなので *マージします。* すると先読みが和集合になって両方とも <code><span class="setb">{</span><span class="setm"> d e </span><span class="setb">}</span></code> → `d`（と `e`）で A・B 両方が reduce → **reduce/reduce 衝突！** *マージ前にはなかった* ものが生き返ったのです。

> つまりこの文法は *LR(1)(CLR) では解けるのに LALR では衝突* する、まれな場合です。でも実務の文法にはこういう食い違いがほとんどないので、LALR のマージは大半が *ただで状態だけ節約してくれます。*

> 📎 **マージで新たに生じる衝突は *reduce/reduce だけ* です** — *shift/reduce 衝突* はマージでは絶対に生じません。（CLR が衝突なしなら、LALR が付け加えうるのは r/r 衝突だけだと証明されています。だから上の例も r/r でしたね。）

> 💡 **ここでもう一歩** — *「では、マージしても衝突しないものだけ選んでマージすれば？ LR(1) の精密度を保ちながら、状態数はほぼ LALR と同じくらいになるのでは？」* — その通りです。それが **minimal LR(1)**（Pager, 1977）と、その現代版 **IELR(1)** です。（GNU Bison も `%define lr.type ielr` で対応。）私たちのエンジンはまだ LALR までなので、これは *将来の改善候補* です。

---

## 精密度のはしご — ひと目で

| 方式 | reduce を書く文字 | 精密度 | 状態数 | 見かけの衝突 |
|:--|:--|:--:|:--:|:--:|
| **LR(0)** | すべての終端記号 | 最低 | 少ない | わんさか |
| **SLR** | FOLLOW(A) | 中 | 少ない | ときどき |
| **LALR** | コアごとの精密な先読み *(CLR をマージしたもの)* | 高い | 少ない (= LR(0)) | ほぼなし |
| **CLR / LR(1)** | 文脈ごとの先読み *(状態を分割する)* | 最高 | **爆発** | なし |

上に行くほど精密になりますが、最後の一マス（CLR）で *状態数* という高い代償を払います。\
だから **LALR が — 精密度と状態数、二兎を同時に追える場所** なのです。

---

## 次へ

しくみはここまでです。では *私たちのエンジン* は、この「マージ（= 伝播）」をコードでどうやるのでしょうか？

👉 **[構文解析表 · LALR — 実装](parse-table-lalr-impl.md)**
