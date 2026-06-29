# FIRST · 計算規則

> 🎓 **発展コース · 理論** です。\
> 前の [FIRST · 定義と導出](first-formula.md) で — *定義* を掴み、*定義どおりの導出* もやってみましたね。\
> ところが **再帰で導出が無限に長くなる壁** にぶつかりました。
>
> そこでこのページは — **導出を直接展開しなくても** 同じ FIRST を取り出せる **計算規則** です。\
> （再帰もおとなしく処理されます。）\
> その規則がコードでどう実装されているかは → **[FIRST · 実装](first-impl.md)**。

> 一度ですべてを受け入れようとしないでください。\
> **簡単なものから一歩ずつ** 進みます。

## まず — 登場人物を全部並べてから始めます

FIRST を求める前に、この文法に *何があるのか* から一目で見ましょう。\
文法の記号はちょうど **二種類** です。

- **終端記号（terminal）** — 入力に本当に現れるトークン
- **非終端記号（nonterminal）** — 規則の名前

私たちの例文法で、この二つを分けて書くとこうです。

<pre class="lrbox">
   終端記号リスト   :   <span class="setm">(</span>    <span class="setm">)</span>    <span class="setm">+</span>    <span class="setm">*</span>    <span class="setm">id</span>        <span style="opacity:.6">← 5個</span>
   非終端記号リスト :   <span class="nt">Expr</span>    <span class="nt">Term</span>    <span class="nt">Factor</span>        <span style="opacity:.6">← 3個</span>
</pre>

FIRST は *この記号一つひとつに対して* 求めます。\
ですから私たちがやることは明確です — **終端記号5個 + 非終端記号3個、合計8個の FIRST を埋めること。**\
そして良い知らせ — **終端記号のほうはほぼタダ** です。そこから行きましょう。

## 終端記号の FIRST — すべて自分自身（一発で終わり）

終端記号は *自分自身* で始まります。\
当然ですよね、`+` は `+` で始まりますから。

**読み方から。** `FIRST( '(' ) = { '(' }` は — *「終端記号 `(` の FIRST 集合は `(` 一つ」* と読みます。（`{ }` は *集合*、中に入っているのが要素です。）

では終端記号5個をずらっと —

<pre class="lrbox">
<span class="setf">FIRST(</span> <span class="setm">'('</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span> <span class="setb">}</span>   <span style="opacity:.6">← 終端記号 '(' の FIRST は自分自身</span>
<span class="setf">FIRST(</span> <span class="setm">'+'</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'+'</span> <span class="setb">}</span>   <span style="opacity:.6">← 終端記号 '+' の FIRST は自分自身</span>
<span class="setf">FIRST(</span> <span class="setm">')'</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">')'</span> <span class="setb">}</span>   <span style="opacity:.6">← 終端記号 ')' の FIRST は自分自身</span>
<span class="setf">FIRST(</span> <span class="setm">'*'</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'*'</span> <span class="setb">}</span>   <span style="opacity:.6">← 終端記号 '*' の FIRST は自分自身</span>
<span class="setf">FIRST(</span> <span class="setm">id </span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">id </span> <span class="setb">}</span>   <span style="opacity:.6">← 終端記号 id の FIRST は自分自身</span>
</pre>

一行でまとめると — **`FIRST(終端記号 a) = { a }`**（*終端記号の FIRST は常に自分自身*）。\
終端記号5個、これで **終わり。** 8個のうち5個をタダで埋めました。🙂

## 非終端記号の FIRST — 大きな絵から

ではいよいよ本題、非終端記号の三つ（`Expr` `Term` `Factor`）です。\
その前に、大きな絵を先に掴んでおきましょう。

> 📎 ちょっと用語を一つ。`Factor : '(' Expr ')' | id` で `|` で区切られた **一つひとつ** を **生成規則
> （production）** と呼びます。\
> `Factor → id` のように *「非終端記号を作る規則の一行」* です。（詳しくは [Single](deep-single.md)。）\
> これからはこの言葉を使います。

一つの非終端記号の FIRST はこう求めます — **その非終端記号のすべての生成規則それぞれの FIRST を求めて、全部
合わせる（和集合）** のです。

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Factor</span> の生成規則 1<span class="setf">)</span> ∪ <span class="setf">FIRST(</span><span class="nt">Factor</span> の生成規則 2<span class="setf">)</span> ∪ …
</pre>

ですから本当に解くべき問いは一つに絞られます — **生成規則一つの FIRST はどう求めるのか？**

答えは意外と単純です。\
その生成規則が **何で始まるか** にかかっていて、場合はちょうど **三つ** だけです。

1. **場合 ①** — 終端記号で始まるとき
2. **場合 ②** — 非終端記号で始まるとき
3. **場合 ③** — 先頭が消えうる（ε）とき

簡単なものから一つずつ見ていきましょう。

## 場合 ① — 生成規則が **終端記号で始まる** とき

もっとも簡単な場合です。実は **前に見たものをそのまま使う** だけです。

先頭が終端記号なら、その生成規則の FIRST はすなわち **その終端記号** です。\
すぐ上で *「終端記号の FIRST は自分自身」* と言いましたよね？ **まさにそれとまったく同じ話** です — 先頭の終端記号がすなわち答えです。\
（生成規則の後ろに記号がさらに付いていても関係ありません。先頭が終端記号なら、それが最初の終端記号なので、そこで終わるからです。）

例えば `Factor` の二つの生成規則はそれぞれこうです。

<pre class="lrbox">
   <span class="nt">Factor</span> → <span class="setm">id</span>            <span style="opacity:.6">先頭が終端記号 id   →   FIRST = { id }</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>  <span style="opacity:.6">先頭が終端記号 (    →   FIRST = { '(' }</span>
</pre>

`'(' Expr ')'` は後ろに `Expr ')'` がさらに付いても、先頭の `(` ですぐ終わります。

`Factor` はこの二つの生成規則だけなので、合わせればすぐ完成です。

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span> <span class="setb">}</span> ∪ <span class="setb">{</span> <span class="setm">id</span> <span class="setb">}</span> = <span class="setb">{</span> <span class="setm">'('</span>, <span class="setm">id</span> <span class="setb">}</span>
</pre>

最初の非終端記号、終わり！ 🙂

## 場合 ② — 生成規則が **非終端記号で始まる** とき

今度は先頭が終端記号ではなく、*また別の非終端記号* である場合です。私たちの文法の `Expr → Term` がまさにそうです — 先頭が非終端記号 `Term` ですね。

ではこの生成規則の FIRST は？ — **先頭の `Term` の FIRST をそのまま持ってきます。** つまり **`FIRST(Expr) = FIRST(Term)`**。

**なぜそうなるのでしょう？**\
定義に戻りましょう — FIRST は *「導出して一番最初に現れる終端記号」* ですよね。`Expr → Term` を導出すると先頭の位置を `Term` が占めるので、最後まで展開して出てくる先頭の終端記号も結局 *`Term` が決めます。*

**導出で直接見てみましょう。** `Expr → Term` を最後まで展開すると —

<pre class="lrbox">
<span class="nt">Expr</span>  ⇒  <span class="nt">Term</span>  ⇒  <span class="nt">Factor</span>  ⇒  <span class="setm">id</span>            <span style="opacity:.6">先頭の終端記号 = id</span>
<span class="nt">Expr</span>  ⇒  <span class="nt">Term</span>  ⇒  <span class="nt">Factor</span>  ⇒  <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span>      <span style="opacity:.6">先頭の終端記号 = (</span>
</pre>

> 🎨 *紫 = 非終端記号（`Expr`·`Term`·`Factor`）、青緑 = 終端記号（`id`·`(`·`)`）。*

先頭の位置を最初から最後まで `Term`（→ `Factor` → …）が握っていますよね？\
ですから先頭に現れた終端記号 `id` · `(` はすなわち **`Term` が出す最初の終端記号** — つまり `FIRST(Term)` そのものです。\
だから **`FIRST(Expr) = FIRST(Term)`**。\
（`Expr` の残りの生成規則 `Expr '+' Term` は、すぐ見る *左再帰* なので新しく加わるものがなく、この等号がぴたりと成り立ちます。）

> 🔖 **一行で一般化：** *生成規則の先頭が非終端記号なら — その非終端記号の FIRST をそのまま持ってくる。*（ただし、その非終端記号が *自分自身* なら一度引っかかります — すぐ下。）

### ところで — その非終端記号が *自分自身* なら？（左再帰）

ここで一度引っかかります。\
`Term : Term '*' Factor | Factor` の最初の生成規則を見てください。

<pre class="lrbox">
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>     <span style="opacity:.6">先頭がまた Term — 自分自身じゃない？!</span>
</pre>

`FIRST(Term)` を求めようとして見たら、先頭の非終端記号がまた `Term` です。\
つまり `FIRST(Term)` を求めるのに `FIRST(Term)` が必要な、**鶏が先か卵が先か** の状況ですね。\
このままでは一度には解けません。

そこでこう解きます — **空集合から始めて、もう増えなくなるまで一周ずつ繰り返す。**\
`FIRST(Term)` を実際に回してみます。（`FIRST(Factor) = { '(', id }` は場合 ① で既に求めましたね。）

空集合から始めて、`Term` の二つの生成規則（①②）を洗い出すのを — **もう増えなくなるまで** 繰り返します。\
一周 = 一行で見るとこうです。

| 周 | ① `Term → Factor` | ② `Term → Term '*' Factor` | 周の後の `FIRST(Term)` |
|:--:|:--|:--|:--:|
| **開始** | — | — | `{ }` |
| **1周目** | `FIRST(Factor)` = `{ '(', id }` 追加 | 先頭 `Term` の *現在値* → 新しいものなし | **`{ '(', id }`** &nbsp;*(増えた)* |
| **2周目** | すでにある → なし | 現在値 `{ '(', id }` → 新しいものなし | `{ '(', id }` &nbsp;*(そのまま)* |

→ **1周目** で空集合から `{ '(', id }` に *増えた* のでもう一周回り、**2周目** では何も増えない — **そこで止まります。**

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span>, <span class="setm">id</span> <span class="setb">}</span>
</pre>

核心は生成規則 ② です。\
自分自身を指していますが、「先頭 `Term` の *これまでの* 値」を引っぱってくるだけなので、その値が別の生成規則
①（`Factor`）で **先に埋まってしまえば** もう加えるものがなくなります。\
だから無限に展開しなくても、二周で答えが固まります。

> 💡 前のページ（[定義と導出](first-formula.md)）で導出が *無限に長くなっていた* あの再帰の壁 — **その壁を
> 越えるのがまさにこの「繰り返し」** です。\
> 最後まで展開する代わりに、集合を少しずつ大きくしていって変わらなくなったら止まるからです。

`Expr`（`Expr : Expr '+' Term | Term`）も同じかたちなので、同じやり方で二周あれば `{ '(', id }` に
固まります。

## 場合 ③ — 先頭の非終端記号が **消えうる** とき（ε）

最後の場合です。\
場合 ② で *「先頭の非終端記号 `Y` の FIRST を持ってくる」* と言いましたね？\
ところがもしその `Y` が **空（ε）に消える** こともありうるなら、もう一つ拾っておかなければなりません。\
（ある非終端記号が空文字列まで導出できることを *nullable* と呼びます。）

**なぜ次の記号まで見なければならないのでしょう？**\
やはり定義です — FIRST は *「導出して一番最初に現れる終端記号」* ですよね。\
ところが先頭の `Y` が ε に消えると、導出結果の先頭を占めるのは `Y` ではなく **すぐ次の記号** です。\
そうするとその次の記号が導出する最初の終端記号も先頭に来られますよね。\
だから `Y` の FIRST に **その次の記号の FIRST まで** 加えてようやく正しくなります。\
この *「先頭が消えうるなら次の記号へ進みながら合わせる」* 規則を **⊕（リングサム）** と呼びます。

```
   A ⊕ B =  A              (A が消えられないなら → そこで終わり)
            (A-ε) ∪ B      (A が消えうるなら → ε を抜いて、B も加える)
```

> 幸い、私たちの例文法には *消える（nullable）非終端記号* が一つもありません。\
> ですから場合 ③ は実際には起こらず、⊕ もほぼ先頭の記号ですぐ終わります。\
> 今は **「こういう安全装置があるんだな」** とだけ知っていれば十分です。⊕ の真価は ε のある文法で発揮されます。

## まとめ — 三つの場合は結局一つの式

前で **場合 ①②③** に分けて見ましたが、実はこの三つは **一つの式** にまとまります。

生成規則は結局 *記号の並び* ですよね（`Term '*' Factor` のように）。\
その FIRST は — **構成する記号たちの FIRST を順番に ⊕（リングサム）したもの**、それがすべてです。

```
   FIRST(X₁ X₂ … Xₙ) = FIRST(X₁) ⊕ FIRST(X₂) ⊕ … ⊕ FIRST(Xₙ)
```

すると前の三つの場合は — この **⊕ が *どこで止まるか*** の違いにすぎません。

- **場合 ①** : 最初の記号が終端記号です。終端記号は ε にならないので、⊕ が **最初のマスで止まって** `{ その終端記号 }` になります。
- **場合 ②** : 最初の記号が（ε にならない）非終端記号です。⊕ がその **FIRST で止まって** `FIRST(その非終端記号)` になります。
- **場合 ③** : 最初の記号が ε になりえます。だから ⊕ が **次のマスへ進みながら** ずっと合わせていきます。

例として `Term '*' Factor` をそのまま ⊕ してみるとこうです。

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span> ⊕ <span class="setf">FIRST(</span><span class="setm">'*'</span><span class="setf">)</span> ⊕ <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span>
                          = <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span>              <span style="opacity:.6">← Term は ε にならないので最初のマスで止まる</span>
</pre>

コードもまさにこれです — 生成規則（[Concat](deep-concat.md)）の記号たちを順に `RingSum(⊕)` していって、もう見る ε がなければ止まります。

```csharp
// FirstFollowAnalyzer [First].cs
public TerminalSet FirstSet(NonTerminalConcat singleNT, ...)
{
    TerminalSet result = new TerminalSet();

    foreach (var symbol in singleNT)                        // 生成規則の記号を順番に
    {
        result = result.RingSum(FirstSet(symbol, seenNT));  // ⊕ 一マス
        if (!result.IsNullAble) break;                      // もう見る ε がなければ止まる
    }

    return result;
}
```

**検算** — この規則を私たちの文法で回すと、三つとも同じ答えが出ます。

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Expr</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span>, <span class="setm">id</span> <span class="setb">}</span>
</pre>

[定義と導出のページ](first-formula.md) で手で求めた答えと正確に同じです。✓

---

最後に — ここまで見た **⊕**（一つの生成規則）に、*枝（`|`）合わせ* **`∪`** を一枚だけ重ねれば、**これが FIRST の *すべて* です：**

<pre class="lrbox">
   FIRST(<span class="nt">A</span>) = ⋃ ( FIRST(X₁) ⊕ FIRST(X₂) ⊕ … )
</pre>

- 外側の **`⋃`** : `A` の複数の生成規則（`|` の枝）を *合わせる*。 &nbsp;*(= 非終端記号の FIRST = すべての生成規則の FIRST の和集合)*
- 内側の **`⊕`** : 一つの生成規則（記号の並び）を *つなぐ、ただし先頭が消えるなら次のマスまで*。 &nbsp;*(= 場合 ①②③)*
- 土台の **`FIRST(終端記号 a) = { a }`**。

この一行に — *場合 ①②③* と *枝合わせ* が **すべて** 入っています。FIRST は結局これがすべてです。🎯

## 次へ — この規則がコードへ

いま見た三つの場合 — 終端記号始まり · 非終端記号始まり · ε — と「変わらなくなるまで繰り返す」が、`FirstFollowAnalyzer`
のコードに **ほぼ一行ずつ** そのまま刻まれています。\
続けて見ていきましょう。

👉 **[FIRST · 実装（コード）](first-impl.md)**

---

👈 前へ： [FIRST · 定義と導出](first-formula.md)
