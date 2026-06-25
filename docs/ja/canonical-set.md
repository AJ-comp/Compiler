# 正準集合 — すべての状態 (I₀ ~ I₁₁)

> 🎓 **発展コース** です。\
> [閉包](closure-def.md)で一つの状態を *漏れなく満たし*、[GOTO](goto.md)で *記号を一つ読んで次の
> 状態へ* 進みましたね。\
> この二つを — 開始状態 `I₀` から出発して **新しい状態がもう出てこなくなるまで** — 繰り返すと、*到達可能な
> すべての状態* が集まります。それが **正準集合(canonical collection)** です。

> 📍 **在りか** · `CanonicalRelation.Calculate` · `…/Parsers/Collections/CanonicalRelation.cs`

私たちの例文法(拡大したもの)で最後まで回すと、状態は **ちょうど12個 — `I₀` ~ `I₁₁`** になります。\
以下にすべて書き留めておきますね。各状態ごとに *項目たち* と、そこから記号を読んで進む *遷移(GOTO)* を一緒に載せました。\
(ドットが一番後ろまで進んだ **完了(reduce)項目** は `← 完了` と表示しています。)

拡大文法はこれです。

```
   Accept → Expr
   Expr   → Expr '+' Term   |  Term
   Term   → Term '*' Factor  |  Factor
   Factor → '(' Expr ')'     |  id
```

---

## `I₀` — 開始状態

`Accept → • Expr` から閉包した状態です。([計算法](closure-calc.md)で作ったあの7個。)

<pre class="lrbox">   Accept → <span class="lrdot">•</span> Expr
   Expr   → <span class="lrdot">•</span> Expr '+' Term
   Expr   → <span class="lrdot">•</span> Term
   Term   → <span class="lrdot">•</span> Term '*' Factor
   Term   → <span class="lrdot">•</span> Factor
   Factor → <span class="lrdot">•</span> '(' Expr ')'
   Factor → <span class="lrdot">•</span> id</pre>

**遷移:**

- `Expr` を読むと → `I₁`
- `Term` を読むと → `I₂`
- `Factor` を読むと → `I₃`
- `'('` を読むと → `I₄`
- `id` を読むと → `I₅`

## `I₁` — `GOTO(I₀, Expr)`

<pre class="lrbox">   Accept → Expr <span class="lrdot">•</span>              <span style="opacity:.65">← 完了 (入力の終わり $ で accept)</span>
   Expr   → Expr <span class="lrdot">•</span> '+' Term</pre>

**遷移:** `'+'` を読むと → `I₆`

## `I₂` — `GOTO(I₀, Term)`

<pre class="lrbox">   Expr → Term <span class="lrdot">•</span>               <span style="opacity:.65">← 完了 (reduce: Expr → Term)</span>
   Term → Term <span class="lrdot">•</span> '*' Factor</pre>

**遷移:** `'*'` を読むと → `I₇`

## `I₃` — `GOTO(I₀, Factor)`

<pre class="lrbox">   Term → Factor <span class="lrdot">•</span>             <span style="opacity:.65">← 完了 (reduce: Term → Factor)</span></pre>

**遷移:** なし (完了項目だけがある状態)

## `I₄` — `GOTO(I₀, '(')`

`'('` を読んでドットを動かした `Factor → '(' • Expr ')'` に、`Expr` がドットの後ろなので再び閉包が付いた7個です。

<pre class="lrbox">   Factor → '(' <span class="lrdot">•</span> Expr ')'
   Expr   → <span class="lrdot">•</span> Expr '+' Term
   Expr   → <span class="lrdot">•</span> Term
   Term   → <span class="lrdot">•</span> Term '*' Factor
   Term   → <span class="lrdot">•</span> Factor
   Factor → <span class="lrdot">•</span> '(' Expr ')'
   Factor → <span class="lrdot">•</span> id</pre>

**遷移:**

- `Expr` を読むと → `I₈`
- `Term` を読むと → `I₂`
- `Factor` を読むと → `I₃`
- `'('` を読むと → `I₄`
- `id` を読むと → `I₅`

## `I₅` — `GOTO(I₀, id)`

<pre class="lrbox">   Factor → id <span class="lrdot">•</span>               <span style="opacity:.65">← 完了 (reduce: Factor → id)</span></pre>

**遷移:** なし (完了項目だけがある状態)

## `I₆` — `GOTO(I₁, '+')`

<pre class="lrbox">   Expr   → Expr '+' <span class="lrdot">•</span> Term
   Term   → <span class="lrdot">•</span> Term '*' Factor
   Term   → <span class="lrdot">•</span> Factor
   Factor → <span class="lrdot">•</span> '(' Expr ')'
   Factor → <span class="lrdot">•</span> id</pre>

**遷移:**

- `Term` を読むと → `I₉`
- `Factor` を読むと → `I₃`
- `'('` を読むと → `I₄`
- `id` を読むと → `I₅`

## `I₇` — `GOTO(I₂, '*')`

<pre class="lrbox">   Term   → Term '*' <span class="lrdot">•</span> Factor
   Factor → <span class="lrdot">•</span> '(' Expr ')'
   Factor → <span class="lrdot">•</span> id</pre>

**遷移:**

- `Factor` を読むと → `I₁₀`
- `'('` を読むと → `I₄`
- `id` を読むと → `I₅`

## `I₈` — `GOTO(I₄, Expr)`

<pre class="lrbox">   Factor → '(' Expr <span class="lrdot">•</span> ')'
   Expr   → Expr <span class="lrdot">•</span> '+' Term</pre>

**遷移:**

- `')'` を読むと → `I₁₁`
- `'+'` を読むと → `I₆`

## `I₉` — `GOTO(I₆, Term)`

<pre class="lrbox">   Expr → Expr '+' Term <span class="lrdot">•</span>      <span style="opacity:.65">← 完了 (reduce: Expr → Expr '+' Term)</span>
   Term → Term <span class="lrdot">•</span> '*' Factor</pre>

**遷移:** `'*'` を読むと → `I₇`

## `I₁₀` — `GOTO(I₇, Factor)`

<pre class="lrbox">   Term → Term '*' Factor <span class="lrdot">•</span>    <span style="opacity:.65">← 完了 (reduce: Term → Term '*' Factor)</span></pre>

**遷移:** なし (完了項目だけがある状態)

## `I₁₁` — `GOTO(I₈, ')')`

<pre class="lrbox">   Factor → '(' Expr ')' <span class="lrdot">•</span>     <span style="opacity:.65">← 完了 (reduce: Factor → '(' Expr ')')</span></pre>

**遷移:** なし (完了項目だけがある状態)

---

## 遷移を一目で

上の遷移たちを一つの表にまとめると、こうなります。空欄は *その記号では行ける場所がない* という意味です。

| 状態 | `Expr` | `Term` | `Factor` | `'+'` | `'*'` | `'('` | `')'` | `id` |
|:--|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| `I₀`  | I₁ | I₂ | I₃ |    |    | I₄ |    | I₅ |
| `I₁`  |    |    |    | I₆ |    |    |    |    |
| `I₂`  |    |    |    |    | I₇ |    |    |    |
| `I₃`  |    |    |    |    |    |    |    |    |
| `I₄`  | I₈ | I₂ | I₃ |    |    | I₄ |    | I₅ |
| `I₅`  |    |    |    |    |    |    |    |    |
| `I₆`  |    | I₉ | I₃ |    |    | I₄ |    | I₅ |
| `I₇`  |    |    | I₁₀|    |    | I₄ |    | I₅ |
| `I₈`  |    |    |    | I₆ |    |    | I₁₁|    |
| `I₉`  |    |    |    |    | I₇ |    |    |    |
| `I₁₀` |    |    |    |    |    |    |    |    |
| `I₁₁` |    |    |    |    |    |    |    |    |

## 次の章

状態も、状態どうしの遷移もすべて集めました — 正準集合の完成です。

あとに残ったのは、これを **一枚の表** に変える仕事だけです。上の *遷移表* はそのままパーサの **shift / goto** に
なり、各状態の **完了(reduce)項目** は *「いつまとめるか」* になります。この二つを合わせると — 次の章の **構文解析表**
です。

👉 **[構文解析表 · 作り方](parse-table-build.md)**

---

👈 前へ: [GOTO](goto.md)
