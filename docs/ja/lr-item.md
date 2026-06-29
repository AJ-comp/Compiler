# LR項目 — 「どこまで読んだか」をドットで

> 🎓 **発展コース** です。\
> ここまでで **文法の構造**（[Symbol](deep-symbols.md) 〜 [Alter](deep-alter.md)）と **FIRST/FOLLOW** をひと通り
> 見てきましたね。これからはそれを *材料* にして — いよいよ **LRパーサーを作り** 始めます。\
> その最初のれんがが **LR項目** です。

## 何を表そうとしているのか

LRパーサーは入力を **左から1トークンずつ** 読み進めていきます。\
読んでいくうちに *「いま、どの生成規則を、どこまで読んだのか」* をずっと覚えておかなければなりません。

その「どこまで」を表す、いちばん単純な方法 — それは生成規則の真ん中に **ドット（`•`）** を1つ打つことです。

## 定義 — LR項目とは

> **LR項目** = 生成規則1つに **ドット（`•`）を1か所打ったもの。**\
> `A → α • β` の形です。**ドットの前（α）** は *すでに読んだ部分*、**ドットの後ろ（β）** は *まだ読む部分* ですね。

たとえば生成規則 `Expr → Expr '+' Term` 1つに、ドットを打てる場所はこうなります。

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      まだ何も読んでいない
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">'+'</span> <span class="nt">Term</span>      Expr まで読んだ
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="lrdot">•</span> <span class="nt">Term</span>      '+' まで読んだ
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> <span class="lrdot">•</span>      全部読んだ — いよいよこの規則で還元（reduce）する番！</pre>

**同じ生成規則でも、ドットの位置が違えば別の項目** です。ドットが「進み具合」だからですね。

## コード — `LRItem` · 作者はどう表現したか

ここからはまた **作者の設計** が出てきます。\
LR項目という *概念* — 生成規則にドットを打って「どこまで読んだか」を表すこと — はコンパイラの教科書の **標準** です。\
（概念も名前も教科書から来ます。英語では *item*、日本の教科書ではよく **「項目」** と呼びます。）\
ただし、それを **コードでどう収めるか** — 生成規則を再利用して `markIndex` だけ載せる、その方式 — は作者の選択なのです。

作者はおそらく、こう判断したのでしょう。

> *「LR項目は結局『生成規則1つ + ドットの位置』だよね。生成規則はすでに [Single](deep-single.md) で作って
> あるから — **新しく作る必要はなく、それを指して**、ドットの位置（`markIndex`）1つだけ載せればいいな。」*

そういうわけで `LRItem` は、ちょうど **2つ** だけです — *どの生成規則* なのか、そして *ドットがどこ* なのか。

> 📍 **`LRItem : ICloneable`** · `…/Parsers/Datas/LR/LRItem.cs`

```csharp
public class LRItem : ICloneable
{
    public NonTerminalSingle SingleNT { get; }   // どの生成規則か  (A → α β)
    private sbyte markIndex = 0;                  // ドットが何番目の記号の前にあるか (0 = 先頭)
}
```

- `SingleNT` — まさに [Single](deep-single.md) の章で見た、あの **生成規則（`NonTerminalSingle`）** そのものです。\
  （LR項目は生成規則を *新しく作らず*、そのまま指します。）
- `markIndex` — ドットの位置。`markIndex` 個の記号がドットの前（α）にある、ということです。

### ドットの前 / ドットの後ろ — 作者が前もって植えておいた種

[Concat](deep-concat.md) の章で `PrevSymbolListFrom`/`PostSymbolListFrom` を見ながら *「これは後でLR
解析のドットの前/後ろで使われます」* と言ったの、覚えていますか？\
じつは作者は — **LR項目でドットの前後（α/β）を切り分けることを前もって見越して** [Concat](deep-concat.md) にその
メソッドを植えておいたのです。\
**ここで、その種が育ちます。**

```csharp
public NonTerminalConcat SymbolListBeforeMarkSymbol => SingleNT.PrevSymbolListFrom(markIndex);  // α (ドットの前)
public NonTerminalConcat SymbolListAfterMarkSymbol  => SingleNT.PostSymbolListFrom(markIndex);  // β (ドットの後ろ)
public Symbol            MarkSymbol                  // ドットのすぐ後ろの記号1個 (SingleNT[markIndex])
```

`A → α • β` の `α` と `β` が、あのとき作った `Prev/PostSymbolListFrom` でぴったり切り離されてきます。\
そして `MarkSymbol` は、ドットのすぐ後ろの記号1個 — *次に読む記号* です。

### ドットが末尾に達したら = 還元する番 (`IsReachedHandle`)

ドットが生成規則の **いちばん末尾** まで行ったら、その規則を *全部読んだ* ということです。\
このときがまさに **還元（reduce）する番** なのです。

```csharp
public bool IsReachedHandle => markIndex >= SingleNT.Count;   // ドットが末尾 = 完了(reduce)項目
```

このような *完了項目* を、コードでは **ハンドルに達した（reached handle）** と呼びます。（これから頻繁に出てきます —
*「この状態に完了項目があれば reduce」* のように。）

### ドットを動かす — `MoveMarkSymbol`

記号を1つ読むと、ドットを **1つ前へ** 動かします。（`A → α • X β` → `A → α X • β`）

```csharp
public void MoveMarkSymbol() { if (this.MarkSymbol != null) this.markIndex++; }
```

（ドットがすでに末尾なら — `MarkSymbol` が `null` なので — 何もしません。）

### 文字で描くと — `ToString`

`ToString()` が、ドットを打った姿を *そのまま* 描いてくれます。私たちが上で手で描いた、あの形です。

```csharp
//  例) markIndex = 2 の LRItem  →
//      "Expr -> Expr '+'•Term"
```

### 同一性 — 「どの生成規則の、ドットがどこか」

2つのLR項目が *同じかどうか* は — **生成規則 + ドットの位置** だけで判断します。

```csharp
public override int GetHashCode()
    => Convert.ToInt32(this.SingleNT.GetHashCode().ToString() + this.markIndex.ToString());
```

[Single](deep-single.md) の同一性（`UniqueKey + alterIndex`）に、**ドットの位置（`markIndex`）** をもう1枚
重ねたものです。\
*「どの規則の何番目の選択肢の、ドットがどこ」* が、そのままLR項目の同一性ですね。（これがまさに [正準集合](canonical-set.md) を
*重複なく* 作る鍵になります。）

## ひと目で — LRItem の全体像

```csharp
public class LRItem : ICloneable
{
    public NonTerminalSingle SingleNT { get; }   // 生成規則 (どの規則)
    private sbyte markIndex;                       // ドットの位置

    // ── ドットの周り ──────────────────────────────
    public Symbol MarkSymbol      { get; }         // ドットのすぐ後ろの記号 (次に読むもの)
    public Symbol PrevMarkSymbol  { get; }         // ドットのすぐ前の記号
    public NonTerminalConcat SymbolListBeforeMarkSymbol { get; }   // α (ドットの前すべて)
    public NonTerminalConcat SymbolListAfterMarkSymbol  { get; }   // β (ドットの後ろすべて)

    // ── 状態 ─────────────────────────────────
    public bool IsFirst         { get; }           // ドットが先頭か (markIndex == 0)
    public bool IsReachedHandle { get; }           // ドットが末尾か = 完了(reduce)項目

    // ── 先読み (後で) ────────────────────
    public TerminalSet Follow    { get; }          // FOLLOW (SLR 用)
    public TerminalSet LookAhead { get; }          // 先読み (LALR/LR(1) 用)

    // ── 操作 ─────────────────────────────────
    public void   MoveMarkSymbol();                // ドットを1つ前進
    public LRItem FirstLRItem();                   // ドットを先頭(0)へ
    public LRItem PrevLRItem();                    // ドットを1つ後ろへ
    public object Clone();

    // ── 同一性 / 表現 ────────────────────────
    public override int    GetHashCode();          // SingleNT のハッシュ + markIndex
    public override string ToString();             // "Expr -> Expr '+'•Term"
}
```

ひと言で — **LR項目 = 生成規則（[Single](deep-single.md)）+ ドットの位置（`markIndex`）。** それで全部です。

## 次の章

LR項目1つは *「この規則をどこまで読んだか」* を表します — ちょうど1つの規則の進み具合だけを。

ところが、パーサーが実際に立つ1つの場所では — *複数の* 項目が **同時に** 可能になり得るのです。\
（たとえば「`Term` を1つ読んだ」場所は — `Expr → Term •` かもしれないし、`Term → Term • '*' Factor` かも
しれません。）\
その *同時に可能な項目たちの束* が、まさに **状態（state）** です。

👉 **[状態 — LR項目たちの集合](lr-state.md)**

---

👈 前へ: [FOLLOW · 実装](follow-impl.md)
