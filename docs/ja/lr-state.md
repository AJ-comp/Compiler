# 状態 — LR項目たちの集合 (I₀, I₁, …)

> 🎓 **発展コース** です。\
> 前の [LR項目](lr-item.md) で *ドットの付いた生成規則* を1つ見ましたね。\
> ところが、パーサーが入力を読みながらある地点に立つと — *可能性のある項目* が普通は **複数** あります。\
> その複数をひと束にまとめたものが **状態（state）** です。

> 📍 **ある場所** · `CanonicalState` · `…/Parsers/Collections/CanonicalState.cs`

## ちょっと待って — まず例題文法をもう一度開いておきます

本格的に入る前に、ちょっと一息ついていきましょう。\
これから状態の話には、私たちの **例題文法** がずっと登場します。[FIRST / FOLLOW](first-follow.md) のときから
ずっと一緒に使ってきた、まさにその文法です。離れてしまわないように、目の前にもう一度開いておきます。

<pre class="lrbox">   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>   |  <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>  |  <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     |  <span class="setm">id</span>
</pre>

- 式 `Expr` は — 項 `Term` たちを `'+'` でつないだもの、
- 項 `Term` は — 因子 `Factor` たちを `'*'` でつないだもの、
- 因子 `Factor` は — 括弧式 `'(' Expr ')'` か、名前1つ `id` です。

掛け算 `'*'` が足し算 `'+'` よりも *より内側* でまとまる（掛け算が先になる）構造ですね。\
この小さな文法1つで十分です — **これから出てくるすべての例がここから** 出てきます。この3行だけ手元に置いて
いけば大丈夫です。

## 状態とは — 項目たちの集合、`Iₓ`

パーサーがトークンを読み進めて、ある位置に立っているとしましょう。\
その位置では「いま進行中かもしれない規則」が1つではなく **複数** あり得ます。\
その *同時に可能なLR項目たち* をまとめたものが1つの **状態** です。

教科書では状態ごとに番号を付けて **`I₀`, `I₁`, `I₂` …** と書きます。（`I` は *item set* の I です。）

言葉だけでは漠然としているので、**なぜ項目1つでは足りないのか** から押さえて — 本物の状態を1つ分解してみます。

## なぜ「項目1つ」ではなく「集合」なのか

項目を1つ見てみます — `Expr → Term •` （*「Term まで読んで Expr が終わった」*）。

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span></pre>

この項目1つだけ見ると *「Term を読み終えたから Expr にまとめよう」* に思えますね。\
でも — 本当にそう言い切ってよいのでしょうか？ 私たちの文法には、この規則もあります。

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
</pre>

つまり、いまその `Term` の後ろに `'*' Factor` がさらに付いて **もっと大きな `Term`** になることもあり得ます。\
その可能性は、この項目で表されます。

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

ドットを見てください — *どちらも `Term` のすぐ後ろ* です。**「ここまで `Term` を読んだ」** という同じ状況を、2つの規則が
それぞれの立場から見ているのです。\
ですから、この位置を正直に書こうとすると — **2つの項目を同時に** 抱えていなければなりません。\
このように *1つの位置で可能な項目をすべて集めた* ものが、まさに **状態** です。

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>                <span style="opacity:.65"># この Term で Expr が終わったのかもしれないし</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>     <span style="opacity:.65"># あるいは '*' が付くもっと大きな Term の前半かもしれない</span></pre>

> このような状態が *いつ、どのように* 正確に作られるのかは — すぐ次の **[閉包](closure-def.md)** と
> **[GOTO](goto.md)** で扱います。いまは *「状態 = 1つの位置で可能な項目をすべて集めた集合」* ということ
> だけつかんでいきましょう。

## それで、この状態では何をするの？

2つの項目は、互いに違うことを言っています。

- **`Expr → Term •`** — ドットが *終わり* に届きました。*「Term 1つがそのまま Expr」* — もう全部見たので **まとめる（reduce）** ことができます。
- **`Term → Term • '*' Factor`** — ドットの後ろに `'*'` が残っています。*「後ろに `* Factor` がさらに来るかも」* — それなら **もっと読まなければ（shift）** なりません。

**2つのうちどちらをするかは — 次のトークン** が決めます。

- 次のトークンが **`*`** なら → `Term → Term • '*' Factor` の側。**`'*'` をさらに読みます（shift）。**
- 次のトークンが **`+`・`)`・入力の終わり（`$`）** なら → これ以上付くものがないので `Expr → Term •` の側。**Term を Expr にまとめます（reduce）。**

> 💡 *「次が `+`・`)`・`$` ならまとめる」* — どこかで見た集合ですね？ まさに **[FOLLOW(Expr)](follow-formula.md)** `= { $, '+', ')' }` です。\
> FIRST/FOLLOW が *ここで* 使われます — *「まとめてよい次のトークン」* を FOLLOW が教えてくれるからです。（このつながりは *構文解析表* の章でしっかり締めくくります。）

## 🌱 種 — 2つのアクションが重なったら？「衝突」

いまその状態には、2つの項目が一緒にありましたね — まとめようとする（reduce） `Expr → Term •` と、もっと読もうとする（shift）
`Term → Term • '*' Factor`。**1つの状態に2つのアクションが共存** したわけです。

それでもパーサーが迷わなかったのは — *各アクションが反応する次のトークンが、互いに重ならなかった* からです。

| 次のトークン | この状態のアクション |
|:--|:--|
| `'*'` | **shift** — `'*'` を読んで `Term → Term • '*' Factor` を進める |
| `$` · `'+'` · `')'`  (`= FOLLOW(Expr)`) | **reduce** — `Expr → Term •` にまとめる |

表を見てください — **トークンごとにアクションがきっちり1つずつ** ですね。\
`'*'` は reduce 側のトークン `{ $, '+', ')' }` にないので、*どのトークンが来てもやることが1つに決まります* —
だからすっきり分かれました。

### では、もし重なったら？

想像だけしてみましょう — もし `'*'` が `FOLLOW(Expr)` にも入っていたら？ 次のトークンが `'*'` のとき、2つの項目が
*同時に* 手を挙げます。

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    →  "'*' を読もう!"   (shift)
   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>               →  "Expr にまとめよう!"  (reduce — '*' が FOLLOW(Expr) にあると仮定)</pre>

表でいうと — `'*'` のマスがこうなります。

| 次のトークン | この状態のアクション |
|:--|:--|
| `'*'` | **shift** _そして_ **reduce**  ⚠️ |
| `$` · `'+'` · `')'` | reduce |

さっきのすっきりした表では、`'*'` のマスに **shift 1つ** だけでしたね。いまそこに **reduce まで** 入ってきて — *1つのマスに
アクションが2つ* です。\
同じトークン1つに対して **shift と reduce が両方** 可能になります。パーサーは *「まとめようか、もっと読もうか」* を決め
られません。\
この *「1つの位置でアクションが分かれる」* のが、まさに **衝突（conflict）** — その中でも **shift/reduce 衝突** です。

> 私たちの例題文法は幸い、こういう重なりがなく、どの状態でも衝突が起きません。\
> このように *衝突が1つもない* 文法には名前があります — その構文解析方式の名前そのまま **LR文法** と
> 呼びます。私たちのものは、最も単純な **SLR(1)** だけで衝突がない、教科書の代表的な **SLR(1) 文法** です。
> （衝突が消える先読みの精度順に **SLR(1) ⊂ LALR(1) ⊂ LR(1)** とさらに分かれますが、これは *構文解析
> 表* の章で。— よく混同される *「文脈自由文法（CFG）」* は衝突とは *無関係な* ずっと広い分類で、ほとんど
> すべての言語文法が CFG です。そのうち、衝突なく LR 構文解析ができる一部が **LR文法** です。）\
> （一方、衝突がよく起きる最も有名な例が `if-then-else` の *「`else` をどの `if` に付けるか」* ですね。）\
> どう **検出して見分けるか** は、ずっと後の *構文解析表* の章できちんと扱います。いまは *「1つの状態に2つの
> アクションが重なれば衝突」* という種だけ植えておいていきましょう。

## コード — `CanonicalState`

状態が *「項目たちの集合」* だと言いましたね。コードも文字どおりです。

```csharp
public class CanonicalState : HashSet<LRItem>   // 1つの状態 = LR項目たちの集合
{
    public int StateNumber { get; }   // その状態の番号 — Iₓ の x (I0 なら 0)
}
```

> 💡 集合（`HashSet`）であるのが自然です — 1つの状態に *同じ項目* が二度ある理由がないからです。\
> [LR項目](lr-item.md) の正体は *「生成規則 + ドット位置」* なので、まったく同じ項目は集合の中で自動的に
> 1つにまとめられます。

## コードで — 2つの種類を見分ける

前に見たその2つのアクション — *まとめる（reduce）* と *もっと読む（shift）* — は、結局のところ項目が **ドットが終わったか** で
分かれるのでした。コードも、その2つをこのように見分けます。

- **shift 項目** — ドットがまだ *終わって* いないもの（`A → α • X β`）。\
  まだ読むものが残っている → `X` を *読み進める*。（コード: `ShiftItemList`）
- **完了（reduce）項目** — ドットが *終わり* に届いたもの（`A → α •`）。\
  全部読んだ → この規則で *まとめる（reduce）*。（コード: `IsReachedHandle`, `ReachedHandleSet`）

```csharp
public bool IsReachedHandle { get; }                 // この状態に完了項目があるか
public HashSet<NonTerminalSingle> ReachedHandleSet;  // 完了した生成規則たち (reduce 候補)
public HashSet<NonTerminalSingle> ShiftItemList;     // まだ進行中の生成規則たち
```

言葉だけ見るとぼんやりするので、**例題の状態をそのまま代入** してみましょう。各変数に何が入るかを見れば、一度に
はっきりします。

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← 完了項目</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>   <span style="opacity:.65">← 進行中(shift) 項目</span></pre>

| 変数 | この状態での値 | なぜ |
|:--|:--|:--|
| `IsReachedHandle` | `true` | 完了項目 `Expr → Term •` が *1つでも* あるから |
| `ReachedHandleSet` | `{ Expr → Term }` | その完了項目の **生成規則** |
| `ShiftItemList` | `{ Term → Term '*' Factor }` | 進行中項目の **生成規則** |

見てください — 1つの状態の2つの項目が、`Expr → Term •` は *完了* のマス（`ReachedHandleSet`）へ、
`Term → Term • '*' Factor` は *進行中* のマス（`ShiftItemList`）へ、きれいに分かれて入っていきますね。\
（どちらも入っているので `IsReachedHandle` は `true` — *「この状態にはまとめるものもある」* という合図です。）

そして `ShiftItemList` も `ReachedHandleSet` も **集合** なので、項目が多ければ複数入ります。\
*完了も進行中も2つずつ* の状態を描いてみると、一目で分かります。（私たちの例題文法では、このようには集まりません —
割り算 `Term → Term '/' Factor` と、「文は式1つ」規則 `Stmt → Term` がさらにあったと *仮定* した
ものです。）

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← 完了</span>
   <span class="nt">Stmt</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← 完了   (Stmt → Term があったなら)</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>   <span style="opacity:.65">← 進行中</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'/'</span> <span class="nt">Factor</span>   <span style="opacity:.65">← 進行中 (Term → Term '/' Factor があったなら)</span></pre>

| 変数 | 値 (仮定した状態で) |
|:--|:--|
| `IsReachedHandle` | `true` |
| `ReachedHandleSet` | `{ Expr → Term, `**`Stmt → Term`**` }` |
| `ShiftItemList` | `{ Term → Term '*' Factor, `**`Term → Term '/' Factor`**` }` |

完了が2つ、進行中が2つ — 変数ごとに複数の項目が入りましたね。\
実際の私たちの例題文法では、この状態は完了1つ・進行中1つでシンプルですが、構造上 **いくらでも増やせる
集合** だということを見せようとしているのです。

## この状態で読める記号 — `MarkSymbolSet`

shift 項目たちの **ドットのすぐ後ろの記号** をすべて集めたものが `MarkSymbolSet` です。\
*「この状態でいま読める記号」のリスト* ですね。（次の [GOTO](goto.md) で、これらの記号で次の状態を
たどっていきます。）

```csharp
public SymbolSet MarkSymbolSet { get; }   // 状態の中の項目たちの 'ドット後ろの記号' 全部
```

上の例題の状態なら `MarkSymbolSet = { '*' }` です。（`Term → Term • '*' Factor` のドットの後ろの `'*'` 1つだけ。）

## ひと目で — `CanonicalState` の全体像

```csharp
public class CanonicalState : HashSet<LRItem>   // 状態 = LR項目の集合
{
    public int StateNumber { get; }                      // 状態番号 (Iₓ の x)

    // ── この状態で読める記号 ───────
    public SymbolSet MarkSymbolSet { get; }              // ドット後ろの記号全部

    // ── 2つの種類に分ける ──────────────────────
    public bool IsReachedHandle { get; }                 // 完了項目があるか
    public HashSet<NonTerminalSingle> ReachedHandleSet;  // 完了(reduce) 生成規則たち
    public CanonicalState ReachedHandleItem { get; }     // 完了項目だけ集めた状態
    public HashSet<NonTerminalSingle> ShiftItemList;     // 進行中(shift) 生成規則たち

    // ── 照会 ────────────────────────────────
    public bool   HasItem(LRItem item);
    public LRItem GetItem(LRItem item);
}
```

ひと言で — **状態 `Iₓ` = LR項目たちの集合。その中には *もっと読む* shift 項目と *まとめる* 完了項目が
混ざっていて、次のトークンが道を選ぶ。**

## 次の章

状態が *何であるか* を見ました — 項目たちの集合 `Iₓ`。

ところが状態を *作る* ときは、項目を適当に集めるのではなく — ドットの後ろの非終端記号の生成規則まで
漏れなく埋めて、はじめて *完全な* 状態になります。\
その「漏れなく埋める」が、まさに **閉包** です。

👉 **[閉包 · 定義](closure-def.md)**

---

👈 前へ: [LR項目](lr-item.md)
