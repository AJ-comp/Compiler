# LR項目 — 「どこまで読んだか」 をドットで

> 🎓 **発展コース** です。\
> ここまで **文法構造**([Symbol](deep-symbols.md) 〜 [Alter](deep-alter.md))と **FIRST/FOLLOW** を一通り
> 見てきましたね。これからはそれを *材料* にして — いよいよ **LRパーサを作り** 始めます。\
> その最初のレンガが **LR項目** です。

## 何を表そうとしているのか

LRパーサは入力を **左から1トークンずつ** 読み進めていきます。\
読んでいくうちに *「今どの生成規則を、どこまで読んだのか」* をずっと覚えておかなければなりません。

その「どこまで」を表す最も単純な方法 — それは生成規則の真ん中に **ドット(`•`)** を一つ打つことです。

## 定義 — LR項目とは

> **LR項目** = 生成規則一つに **ドット(`•`) を一か所打ったもの。**\
> `A → α • β` の形です。**ドットの前(α)** は *すでに読んだ部分*、**ドットの後ろ(β)** は *まだ読む部分* ですね。

例えば生成規則 `Expr → Expr '+' Term` 一つに、ドットを打てる場所はこうなります。

<pre class="lrbox">   Expr → <span class="lrdot">•</span> Expr '+' Term      まだ何も読んでいない
   Expr → Expr <span class="lrdot">•</span> '+' Term      Expr まで読んだ
   Expr → Expr '+' <span class="lrdot">•</span> Term      '+' まで読んだ
   Expr → Expr '+' Term <span class="lrdot">•</span>      全部読んだ — いよいよこの規則でまとめる(reduce)番!</pre>

**同じ生成規則でもドットの位置が違えば別の項目** です。ドットが「進み具合」だからですね。

## コード — `LRItem` · 著者はどう表現したか

ここからはまた **著者の設計** が出てきます。\
LR項目という *概念* — 生成規則にドットを打って「どこまで読んだか」を表すこと — はコンパイラ教科書の **標準** です。\
(概念も名前も教科書から来ます。英語では *item*、韓国の教科書ではよく **「항목(項目)」** と呼びます。)\
ただしそれを **コードでどう収めるか** — 生成規則を再利用して `markIndex` だけ乗せるその方式 — は著者の選択なのです。

著者はおそらくこう判断したのでしょう。

> *「LR項目は結局『生成規則一つ + ドット位置』だよね。生成規則はすでに [Single](deep-single.md) で作って
> あるから — **新しく作る必要はなくそれを指して**、ドット位置(`markIndex`)一つだけ乗せればいいな。」*

そういうわけで `LRItem` はちょうど **二つ** だけです — *どの生成規則* なのか、そして *ドットがどこ* なのか。

> 📍 **`LRItem : ICloneable`** · `…/Parsers/Datas/LR/LRItem.cs`

```csharp
public class LRItem : ICloneable
{
    public NonTerminalSingle SingleNT { get; }   // どの生成規則か  (A → α β)
    private sbyte markIndex = 0;                  // ドットが何番目の記号の前にあるか (0 = 先頭)
}
```

- `SingleNT` — まさに [Single](deep-single.md) 章で見たあの **生成規則(`NonTerminalSingle`)** そのものです。\
  (LR項目は生成規則を *新しく作らず* そのまま指します。)
- `markIndex` — ドットの位置。`markIndex` 個の記号がドットの前(α)にあるということです。

### ドットの前 / ドットの後ろ — 著者が前もって植えておいた種

[Concat](deep-concat.md) 章で `PrevSymbolListFrom`/`PostSymbolListFrom` を見ながら *「これは後で LR
パーシングのドットの前/後ろで使われます」* と言ったの、覚えていますか?\
実は著者は — **LR項目でドットの前後(α/β)を切り分けることを前もって見越して** [Concat](deep-concat.md) にその
メソッドを植えておいたのです。\
**ここでその種が育ちます。**

```csharp
public NonTerminalConcat SymbolListBeforeMarkSymbol => SingleNT.PrevSymbolListFrom(markIndex);  // α (ドットの前)
public NonTerminalConcat SymbolListAfterMarkSymbol  => SingleNT.PostSymbolListFrom(markIndex);  // β (ドットの後ろ)
public Symbol            MarkSymbol                  // ドットのすぐ後ろの記号一個 (SingleNT[markIndex])
```

`A → α • β` の `α` と `β` が、あのとき作った `Prev/PostSymbolListFrom` でぴったり切り離されます。\
そして `MarkSymbol` はドットのすぐ後ろの記号一個 — *次に読む記号* です。

### ドットが終端に届いたら = まとめる番 (`IsReachedHandle`)

ドットが生成規則の **一番終わり** まで行ったら、その規則を *全部読んだ* ということです。\
このときがまさに **まとめる(reduce)番** なのです。

```csharp
public bool IsReachedHandle => markIndex >= SingleNT.Count;   // ドットが終わり = 完了(reduce)項目
```

このような *完了項目* をコードでは **handle に届いた(reached handle)** と呼びます。(これから頻繁に出てきます —
*「この状態に完了項目があれば reduce」* のように。)

### ドットを動かす — `MoveMarkSymbol`

記号一つを読むと、ドットを **一つ前へ** 動かします。(`A → α • X β` → `A → α X • β`)

```csharp
public void MoveMarkSymbol() { if (this.MarkSymbol != null) this.markIndex++; }
```

(ドットがすでに終わりなら — `MarkSymbol` が `null` なので — 何もしません。)

### 文字で描くと — `ToString`

`ToString()` がドットを打った姿を *そのまま* 描いてくれます。私たちが上で手で描いたあの形です。

```csharp
//  例) markIndex = 2 の LRItem  →
//      "Expr -> Expr '+'•Term"
```

### アイデンティティ — 「どの生成規則の、ドットがどこか」

二つのLR項目が *同じかどうか* は — **生成規則 + ドット位置** だけで判断します。

```csharp
public override int GetHashCode()
    => Convert.ToInt32(this.SingleNT.GetHashCode().ToString() + this.markIndex.ToString());
```

[Single](deep-single.md) のアイデンティティ(`UniqueKey + alterIndex`)に **ドット位置(`markIndex`)** をもう一枚乗せた
ものです。\
*「どの規則の何番目の選択肢の、ドットがどこ」* がそのままLR項目のアイデンティティですね。(これがまさに **正準集合** を
*重複なく* 作る鍵になります — 次の次の章で。)

## 一目で — LRItemの全体像

```csharp
public class LRItem : ICloneable
{
    public NonTerminalSingle SingleNT { get; }   // 生成規則 (どの規則)
    private sbyte markIndex;                       // ドット位置

    // ── ドット周辺 ──────────────────────────────
    public Symbol MarkSymbol      { get; }         // ドットのすぐ後ろの記号 (次に読むもの)
    public Symbol PrevMarkSymbol  { get; }         // ドットのすぐ前の記号
    public NonTerminalConcat SymbolListBeforeMarkSymbol { get; }   // α (ドットの前すべて)
    public NonTerminalConcat SymbolListAfterMarkSymbol  { get; }   // β (ドットの後ろすべて)

    // ── 状態 ─────────────────────────────────
    public bool IsFirst         { get; }           // ドットが先頭か (markIndex == 0)
    public bool IsReachedHandle { get; }           // ドットが終わりか = 完了(reduce)項目

    // ── 先読み (後で) ────────────────────
    public TerminalSet Follow    { get; }          // FOLLOW (SLR 用)
    public TerminalSet LookAhead { get; }          // 先読み (LALR/LR(1) 用)

    // ── 操作 ─────────────────────────────────
    public void   MoveMarkSymbol();                // ドットを一つ前進
    public LRItem FirstLRItem();                   // ドットを先頭(0)へ
    public LRItem PrevLRItem();                    // ドットを一つ後ろへ
    public object Clone();

    // ── アイデンティティ / 表現 ────────────────────────
    public override int    GetHashCode();          // SingleNT ハッシュ + markIndex
    public override string ToString();             // "Expr -> Expr '+'•Term"
}
```

一行で — **LR項目 = 生成規則([Single](deep-single.md)) + ドット位置(`markIndex`)。** それで全部です。

## 次の章

LR項目一つは *「この規則をどこまで読んだか」* を表します — ちょうど一つの規則の進み具合だけを。

ところがパーサが実際に立つ一つの場所では — *複数の* 項目が **同時に** 可能になりうるのです。\
(例えば「`Term` を一つ読んだ」場所は — `Expr → Term •` かもしれないし、`Term → Term • '*' Factor` かもしれ
ません。)\
その *同時に可能な項目たちの束* がまさに **状態(state)** です。

👉 **[状態 — LR項目たちの集合](lr-state.md)**

---

👈 前へ: [FOLLOW · 実装](follow-impl.md)
