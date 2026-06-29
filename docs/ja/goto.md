# GOTO — 記号を一つ読んで次の状態へ

> 🎓 **発展コース** です。\
> 前の [閉包 · 計算法](closure-calc.md) で開始状態 `I₀` を作りましたね。\
> 今度はその状態から **記号を一つ読むと** どの状態へ行くのか — それを決めるのが **GOTO** です。

> 📍 **ある場所** · `Analyzer.Goto` · `…/Parsers/Analyzer.cs`

## 定義

> **GOTO(I, X)** = 状態 `I` の中で **ドットのすぐ後ろが `X` の項目たち** を選び、\
> そのドットを **`X` の後ろへ一つ動かし**（`A → α • X β` → `A → α X • β`）、\
> そうやって集めた項目たちを再び **[閉包](closure-def.md)** したもの — それが `X` を読んだ次の状態です。

GOTO は *一度で終わる* 演算です。\
（閉包のように「閉じるまで繰り返す」のではなく、ドットを動かして閉包を一度すれば終わり — だから *定義がそのまま
計算法* なのです。）

## 自分でやってみる — `I₀` から記号を一つずつ

前の [計算法](closure-calc.md) で作った開始状態 `I₀`（7個）をもう一度持ってきますね。

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

この状態で *ドットのすぐ後ろ* に来られる記号は — `Expr`、`Term`、`Factor`、`'('`、`id` です。\
（これが [状態](lr-state.md) 章で見た `MarkSymbolSet` ですね。）この記号ごとに GOTO を一回ずつやってみましょう。

### `id` を読むと — `GOTO(I₀, id)`

ドットの後ろが `id` の項目は `Factor → • id` 一つだけですね。ドットを `id` の後ろへ動かすと:

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span>        ──( id を読む )──▶        <span class="nt">Factor</span> → <span class="setm">id</span> <span class="lrdot">•</span></pre>

`Factor → id •` はドットが末尾に届きました — *完了項目（reduce）* ですね。（この状態に来ると *「`id` を `Factor`
にまとめよ」* になります。）ドットの後ろに非終端記号がないので閉包で追加されるものもありません。→ **項目1個だけ** の次の
状態です。

### `Term` を読むと — `GOTO(I₀, Term)`

ドットの後ろが `Term` の項目は *二つ* です。二つのドットを `Term` の後ろへ動かすと:

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="lrdot">•</span> <span class="nt">Term</span>              ──( Term )──▶   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>
   <span class="nt">Term</span> → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>   ──( Term )──▶   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

この二つを集めると（ドットの後ろに新しい非終端記号がないので閉包でこれ以上は追加されません）:

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

> 💡 この状態、どこかで見ましたよね？ まさに **[状態](lr-state.md) 章のあの `id * id` 状態** です！\
> *「`id` を `Term` まで読んだとき」* のあの状態が — 実は **`I₀` から `Term` を読んで到達する状態**
> だったのです。バラバラに見えていた二つの章がここで出会います。

### `Expr` を読むと — `GOTO(I₀, Expr)`

ドットの後ろが `Expr` の項目も二つです（`Accept → •Expr`、`Expr → •Expr '+' Term`）。動かすと:

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>          ──( Expr )──▶   <span class="nt">Accept</span> → <span class="nt">Expr</span> <span class="lrdot">•</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> ──( Expr )──▶   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">'+'</span> <span class="nt">Term</span></pre>

ここの `Accept → Expr •` が特別です — *仮想の開始規則が最後まで行った* というのは、**入力がここで終わり（`$`）なら
構文解析を受け入れる（accept）** という意味です！\
一緒にある `Expr → Expr • '+' Term` は — `'+'` がさらに来れば式を続けます。\
（だからこの状態がつまり *「終わって受け入れるか、`+` でさらに続けるか」* です。私たちが作る自動機械の **目標
地点** ですね。）

### `'('` を読むと — `GOTO(I₀, '(')` · ここで閉包が本当に働きます

ドットの後ろが `'('` の項目は `Factor → • '(' Expr ')'` 一つですね。ドットを `'('` の後ろへ動かすと:

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>    ──( '(' を読む )──▶    <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">')'</span></pre>

ところが今回は違います — 動かした先の **ドットの後ろが非終端記号 `Expr`** なのです！\
さっきの `id`・`Term`・`Expr` のときはドットを動かすだけで終わりでしたが、この一項目だけでは *`Expr` をどう始めるのか* が
抜けた **不完全な状態** ですね。だから **閉包がまた働きます。**

ドットの後ろの `Expr` をたどって — `Expr` の規則、そこから `Term` の規則、さらに `Factor` の規則まで — 次々に引き
込まれて **7個に満たされます。**

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">')'</span>        <span style="opacity:.65">← ドットを動かしたもの</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>         <span style="opacity:.65">← 閉包で満たされた</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

これがまさに **`I₄`** です（[正準集合](canonical-set.md) にそのまま出てきます）。

> 💡 見てください — `id`・`Term`・`Expr` のときは *ドットを動かすだけで終わり* でしたが、`'('` はドットの後ろが非終端記号なので **閉包が
> 本当に仕事をしました**（1個 → 7個）。これが GOTO 定義の最後の *「再び閉包」* の部分です。\
> ですから **閉包は `I₀` を作るときだけ使うのではなく、GOTO ごとに再び使われます。** おかげで *どの GOTO の
> 結果も常に空きのない完全な状態* になります。\
> 一行で — **閉包 = 状態を一つ完成、GOTO = ドットを動かして → 再び閉包。**

### まとめ — GOTO の結果ごとに番号（`Iₙ`）が付きます

今 `I₀` から GOTO で行った次の状態たち — 一つ一つが **番号を受け取る新しい状態** です。\
`I₀` の `MarkSymbolSet`（読める記号）が `{ Expr, Term, Factor, '(', id }` の五つだったので、次の状態も
五つです。正準集合を作るとき *状態が発見される順に* `I₁`、`I₂`、… が付きます。

| `I₀` で読む記号 | GOTO 結果 |
|:--|:--|
| `Expr`   | `I₁` |
| `Term`   | `I₂` |
| `Factor` | `I₃` |
| `'('`    | `I₄` |
| `id`     | `I₅` |

（だから上の `'('` の結果が `I₄` でしたね。`Factor` は例として見ませんでしたが同じやり方です — `Term → • Factor` の
ドットを動かして `Term → Factor •` が `I₃`。）\
注意点が一つ — 上では *説明しやすい順* で `id`・`Term`・`Expr`・`'('` を見ましたが、**番号自体は
「発見順」で** 付けられます。だから見せた順序（`id` が先）と番号（`id` = `I₅`）が必ずしも同じとは限りません。

## 実装 — `Analyzer.Goto`

```csharp
public static CanonicalState Goto(CanonicalState iStatus, Symbol toSeeSymbol)
{
    if (toSeeSymbol == null) return null;
    var param = new CanonicalState();

    foreach (var item in iStatus)
    {
        if (item.MarkSymbol == toSeeSymbol)   // ドットの後ろが読む記号と同じ項目だけ
        {
            var clone = item.Clone() as LRItem;
            clone.MoveMarkSymbol();            // ドットを一つ前へ
            param.Add(clone);
        }
    }

    return Analyzer.Closure(param);            // 動かした項目たちを再び閉包
}
```

- `item.MarkSymbol == toSeeSymbol` — *ドットのすぐ後ろが読む記号 `X` の* 項目だけを選ぶのです。
- `clone.MoveMarkSymbol()` — [LR項目](lr-item.md) 章で見た *「ドットを一つ前進」* そのままです。（元を
  触らないよう `Clone` から行います — `I₀` はそのまま残しておかなければならないので。）
- `return Closure(param)` — 動かした項目たちを *再び閉包* します。GOTO の結果も **完全な状態** で
  なければならないからです。（さっきの `'('` の例のようにドットの後ろが非終端記号なら閉包が満たしてくれて、`id`・`Term` の例のようにそうでなければそのまま
  返します。）— 閉包が `I₀` だけでなく *ここでも* 使われるということが、この一行に入っています。

## 次の章

`I₀` 一つから `id`・`Term`・`Expr`・`'('`… へ GOTO をやってみると、*次の状態たち* が次々に出てきました。

これを **すべての状態に対して、これ以上新しい状態が出てこなくなるまで繰り返す** と — 開始状態から *到達可能なすべての
状態* が集まります。\
その状態たちの集まりがまさに **正準集合（canonical collection）** です。

👉 **[正準集合 — すべての状態を作る](canonical-set.md)**

---

👈 これまで: [閉包 · 実装](closure-impl.md)
