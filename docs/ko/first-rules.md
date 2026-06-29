# FIRST · 계산 규칙

> 🎓 **심화 과정 · 이론** 이에요.\
> 앞 [FIRST · 정의와 유도](first-formula.md)에서 — *정의* 를 잡고, *정의대로 유도* 도 해봤죠.\
> 그런데 **재귀에서 유도가 무한히 길어지는 벽** 을 만났어요.
>
> 그래서 이 페이지는 — **유도를 직접 펼치지 않고도** 같은 FIRST 를 뽑아내는 **계산 규칙** 이에요.\
> (재귀도 얌전히 처리돼요.)\
> 그 규칙이 코드로 어떻게 구현됐는지는 → **[FIRST · 구현](first-impl.md)**.

> 한 번에 다 받아들이려 하지 마세요.\
> **쉬운 것부터 한 걸음씩** 갈게요.

## 먼저 — 등장인물을 전부 늘어놓고 시작해요

FIRST 를 구하기 전에, 이 문법에 *무엇이 있는지* 부터 한눈에 봐요.\
문법의 기호는 딱 **두 종류** 예요.

- **단말(terminal)** — 입력에 진짜 나오는 토큰
- **비단말(nonterminal)** — 규칙 이름

우리 예제 문법에서 둘을 갈라 적으면 이래요.

<pre class="lrbox">
   단말 리스트   :   <span class="setm">(</span>    <span class="setm">)</span>    <span class="setm">+</span>    <span class="setm">*</span>    <span class="setm">id</span>        <span style="opacity:.6">← 5개</span>
   비단말 리스트 :   <span class="nt">Expr</span>    <span class="nt">Term</span>    <span class="nt">Factor</span>        <span style="opacity:.6">← 3개</span>
</pre>

FIRST 는 *이 기호 하나하나에 대해* 구해요.\
그러니 우리가 할 일은 분명해요 — **단말 5개 + 비단말 3개, 총 8개의 FIRST 를 채우는 것.**\
그리고 좋은 소식 — **단말 쪽은 거의 공짜** 예요. 거기부터 가요.

## 단말의 FIRST — 전부 자기 자신 (한 방에 끝)

단말은 *자기 자신* 으로 시작해요.\
당연하죠, `+` 는 `+` 로 시작하니까요.

**읽는 법부터.** `FIRST( '(' ) = { '(' }` 는 — *"단말 `(` 의 FIRST 집합은 `(` 하나"* 라고 읽어요. (`{ }` 는 *집합*, 안에 든 게 원소예요.)

그럼 단말 5개를 쭉 —

<pre class="lrbox">
<span class="setf">FIRST(</span> <span class="setm">'('</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span> <span class="setb">}</span>   <span style="opacity:.6">← 단말 '(' 의 FIRST 는 자기 자신</span>
<span class="setf">FIRST(</span> <span class="setm">'+'</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'+'</span> <span class="setb">}</span>   <span style="opacity:.6">← 단말 '+' 의 FIRST 는 자기 자신</span>
<span class="setf">FIRST(</span> <span class="setm">')'</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">')'</span> <span class="setb">}</span>   <span style="opacity:.6">← 단말 ')' 의 FIRST 는 자기 자신</span>
<span class="setf">FIRST(</span> <span class="setm">'*'</span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'*'</span> <span class="setb">}</span>   <span style="opacity:.6">← 단말 '*' 의 FIRST 는 자기 자신</span>
<span class="setf">FIRST(</span> <span class="setm">id </span> <span class="setf">)</span> = <span class="setb">{</span> <span class="setm">id </span> <span class="setb">}</span>   <span style="opacity:.6">← 단말 id 의 FIRST 는 자기 자신</span>
</pre>

한 줄로 요약하면 — **`FIRST(단말 a) = { a }`** (*단말의 FIRST 는 늘 자기 자신*).\
단말 5개, 이걸로 **끝.** 8개 중 5개를 공짜로 채웠어요. 🙂

## 비단말의 FIRST — 큰 그림부터

이제 진짜 본론, 비단말 셋(`Expr` `Term` `Factor`)이에요.\
그 전에 큰 그림을 먼저 잡고 갈게요.

> 📎 잠깐, 용어 하나. `Factor : '(' Expr ')' | id` 에서 `|` 로 나뉜 **하나하나** 를 **생성규칙
> (production)** 이라고 불러요.\
> `Factor → id` 처럼 *"비단말을 만드는 규칙 한 줄"* 이에요. (자세힌 [Single](deep-single.md).)\
> 앞으론 이 말을 써요.

비단말 하나의 FIRST 는 이렇게 구해요 — **그 비단말의 모든 생성규칙 각각의 FIRST 를 구해서, 전부
합치면(합집합)** 됩니다.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Factor</span> 의 생성규칙 1<span class="setf">)</span> ∪ <span class="setf">FIRST(</span><span class="nt">Factor</span> 의 생성규칙 2<span class="setf">)</span> ∪ …
</pre>

그러니 진짜 풀어야 할 질문은 하나로 좁혀져요 — **생성규칙 하나의 FIRST 는 어떻게 구하지?**

답은 의외로 단순해요.\
그 생성규칙이 **무엇으로 시작하느냐** 에 달렸고, 경우는 딱 **셋** 뿐입니다.

1. **경우 ①** — 단말로 시작할 때
2. **경우 ②** — 비단말로 시작할 때
3. **경우 ③** — 맨 앞이 사라질(ε) 수 있을 때

쉬운 것부터 하나씩 볼게요.

## 경우 ① — 생성규칙이 **단말로 시작** 할 때

가장 쉬운 경우예요. 사실 **앞에서 본 걸 그대로 쓰는 것** 뿐이에요.

맨 앞이 단말이면, 그 생성규칙의 FIRST 는 곧 **그 단말** 이에요.\
바로 위에서 *"단말의 FIRST 는 자기 자신"* 이라고 했죠? **딱 그거랑 똑같은 이야기** 예요 — 맨 앞 단말이 곧 답이에요.\
(생성규칙 뒤에 기호가 더 붙어 있어도 상관없어요. 맨 앞이 단말이면 그게 첫 단말이라, 거기서 끝나니까요.)

예를 들어 `Factor` 의 두 생성규칙이 각각 이래요.

<pre class="lrbox">
   <span class="nt">Factor</span> → <span class="setm">id</span>            <span style="opacity:.6">맨 앞이 단말 id   →   FIRST = { id }</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>  <span style="opacity:.6">맨 앞이 단말 (    →   FIRST = { '(' }</span>
</pre>

`'(' Expr ')'` 는 뒤에 `Expr ')'` 가 더 붙어도, 맨 앞 `(` 에서 바로 끝나죠.

`Factor` 는 이 두 생성규칙뿐이니, 합치면 바로 완성이에요.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span> <span class="setb">}</span> ∪ <span class="setb">{</span> <span class="setm">id</span> <span class="setb">}</span> = <span class="setb">{</span> <span class="setm">'('</span>, <span class="setm">id</span> <span class="setb">}</span>
</pre>

첫 비단말 끝! 🙂

## 경우 ② — 생성규칙이 **비단말로 시작** 할 때

이번엔 맨 앞이 단말이 아니라, *또 다른 비단말* 인 경우예요. 우리 문법의 `Expr → Term` 이 딱 그래요 — 맨 앞이 비단말 `Term` 이죠.

그럼 이 생성규칙의 FIRST 는? — **맨 앞 `Term` 의 FIRST 를 그대로 가져와요.** 즉 **`FIRST(Expr) = FIRST(Term)`**.

**왜 그럴까요?**\
정의로 돌아가요 — FIRST 는 *"유도해서 가장 처음 나타나는 단말"* 이죠. `Expr → Term` 을 유도하면 맨 앞자리를 `Term` 이 차지하니, 끝까지 펼쳐 나오는 맨 앞 단말도 결국 *`Term` 이 정해요.*

**유도로 직접 봐요.** `Expr → Term` 을 끝까지 펼치면 —

<pre class="lrbox">
<span class="nt">Expr</span>  ⇒  <span class="nt">Term</span>  ⇒  <span class="nt">Factor</span>  ⇒  <span class="setm">id</span>            <span style="opacity:.6">맨 앞 단말 = id</span>
<span class="nt">Expr</span>  ⇒  <span class="nt">Term</span>  ⇒  <span class="nt">Factor</span>  ⇒  <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span>      <span style="opacity:.6">맨 앞 단말 = (</span>
</pre>

> 🎨 *보라 = 비단말(`Expr`·`Term`·`Factor`), 청록 = 단말(`id`·`(`·`)`).*

맨 앞자리를 처음부터 끝까지 `Term`(→ `Factor` → …) 이 쥐고 있죠?\
그러니 맨 앞에 나온 단말 `id` · `(` 는 곧 **`Term` 이 내놓는 첫 단말** — 즉 `FIRST(Term)` 그 자체예요.\
그래서 **`FIRST(Expr) = FIRST(Term)`**.\
(`Expr` 의 나머지 생성규칙 `Expr '+' Term` 은 곧 볼 *좌재귀* 라 새로 보태는 게 없어서, 이 등호가 딱 성립해요.)

> 🔖 **한 줄로 일반화:** *생성규칙의 맨 앞이 비단말이면 — 그 비단말의 FIRST 를 그대로 가져온다.* (단, 그 비단말이 *자기 자신* 이면 한 번 걸려요 — 바로 아래.)

### 그런데 — 그 비단말이 *자기 자신* 이면? (좌재귀)

여기서 한 번 걸려요.\
`Term : Term '*' Factor | Factor` 의 첫 생성규칙을 봐요.

<pre class="lrbox">
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>     <span style="opacity:.6">맨 앞이 또 Term — 자기 자신이네?!</span>
</pre>

`FIRST(Term)` 을 구하려고 보니, 맨 앞 비단말이 또 `Term` 이에요.\
즉 `FIRST(Term)` 을 구하는 데 `FIRST(Term)` 이 필요한, **닭이 먼저냐 달걀이 먼저냐** 상황이죠.\
이대로는 한 번에 안 풀려요.

그래서 이렇게 풀어요 — **빈 집합에서 시작해서, 더 안 늘어날 때까지 한 바퀴씩 반복.**\
`FIRST(Term)` 을 직접 돌려볼게요. (`FIRST(Factor) = { '(', id }` 는 경우 ①에서 이미 구했죠.)

빈 집합에서 시작해, `Term` 의 두 생성규칙(①②)을 훑는 걸 — **더 안 늘어날 때까지** 반복해요.\
한 바퀴 = 한 줄로 보면 이래요.

| 바퀴 | ① `Term → Factor` | ② `Term → Term '*' Factor` | 바퀴 후 `FIRST(Term)` |
|:--:|:--|:--|:--:|
| **시작** | — | — | `{ }` |
| **1바퀴** | `FIRST(Factor)` = `{ '(', id }` 추가 | 맨 앞 `Term` 의 *현재값* → 새것 없음 | **`{ '(', id }`** &nbsp;*(늘어남)* |
| **2바퀴** | 이미 있음 → 없음 | 현재값 `{ '(', id }` → 새것 없음 | `{ '(', id }` &nbsp;*(그대로)* |

→ **1바퀴** 에 빈 집합에서 `{ '(', id }` 로 *늘어서* 한 바퀴 더 돌고, **2바퀴** 엔 아무것도 안 늘어 — **거기서 멈춰요.**

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span>, <span class="setm">id</span> <span class="setb">}</span>
</pre>

핵심은 생성규칙 ② 예요.\
자기 자신을 가리키지만, "맨 앞 `Term` 의 *지금까지* 값" 을 끌어올 뿐이라, 그 값이 다른 생성규칙
①(`Factor`)으로 **먼저 채워지고 나면** 더 보탤 게 없어져요.\
그래서 무한히 펼치지 않고도, 두 바퀴 만에 답이 굳습니다.

> 💡 앞 페이지([정의와 유도](first-formula.md))에서 유도가 *무한히 길어지던* 그 재귀의 벽 — **그 벽을
> 넘는 게 바로 이 "반복"** 이에요.\
> 끝까지 펼치는 대신, 집합을 조금씩 키우다 안 바뀌면 멈추니까요.

`Expr`(`Expr : Expr '+' Term | Term`) 도 똑같은 모양이라, 같은 식으로 두 바퀴면 `{ '(', id }` 로
굳어요.

## 경우 ③ — 맨 앞 비단말이 **사라질 수 있을 때** (ε)

마지막 경우예요.\
경우 ②에서 *"맨 앞 비단말 `Y` 의 FIRST 를 가져온다"* 고 했죠?\
그런데 만약 그 `Y` 가 **빈 것(ε)으로 사라질 수도** 있다면, 한 가지를 더 챙겨야 해요.\
(어떤 비단말이 빈 문자열까지 유도할 수 있는 걸 *nullable* 이라고 불러요.)

**왜 다음 기호까지 봐야 할까요?**\
역시 정의예요 — FIRST 는 *"유도해서 가장 처음 나타나는 단말"* 이죠.\
그런데 맨 앞 `Y` 가 ε 으로 사라지면, 유도 결과의 맨 앞을 차지하는 건 `Y` 가 아니라 **바로 다음 기호** 예요.\
그러면 그 다음 기호가 유도하는 첫 단말도 맨 앞에 올 수 있죠.\
그래서 `Y` 의 FIRST 에 **그 다음 기호의 FIRST 까지** 더해야 맞아요.\
이 *"맨 앞이 사라질 수 있으면 다음 기호로 넘어가며 합치는"* 규칙을 **⊕(링섬)** 이라고 불러요.

```
   A ⊕ B =  A              (A 가 사라질 수 없으면 → 거기서 끝)
            (A-ε) ∪ B      (A 가 사라질 수 있으면 → ε 를 빼고, B 도 더한다)
```

> 다행히 우리 예제 문법엔 *사라지는(nullable) 비단말* 이 하나도 없어요.\
> 그래서 경우 ③ 은 실제로 일어나지 않고, ⊕ 도 거의 맨 앞 기호에서 바로 끝나요.\
> 지금은 **"이런 안전장치가 있구나"** 만 알면 충분합니다. ⊕ 의 진가는 ε 이 있는 문법에서 드러나요.

## 정리 — 세 경우는 결국 한 식

앞에서 **경우 ①②③** 으로 나눠 봤지만, 사실 이 셋은 **하나의 식** 으로 합쳐져요.

생성규칙은 결국 *기호들의 열* 이죠 (`Term '*' Factor` 처럼).\
그 FIRST 는 — **구성 기호들의 FIRST 를 순서대로 ⊕(링섬) 한 것**, 그게 전부예요.

```
   FIRST(X₁ X₂ … Xₙ) = FIRST(X₁) ⊕ FIRST(X₂) ⊕ … ⊕ FIRST(Xₙ)
```

그럼 앞의 세 경우는 — 이 **⊕ 가 *어디서 멈추느냐*** 의 차이일 뿐이에요.

- **경우 ①** : 첫 기호가 단말이에요. 단말은 ε 이 안 되니, ⊕ 가 **첫 칸에서 멈춰서** `{ 그 단말 }` 이 돼요.
- **경우 ②** : 첫 기호가 (ε 이 안 되는) 비단말이에요. ⊕ 가 그 **FIRST 에서 멈춰서** `FIRST(그 비단말)` 이 돼요.
- **경우 ③** : 첫 기호가 ε 이 될 수 있어요. 그래서 ⊕ 가 **다음 칸으로 넘어가며** 계속 합쳐요.

예로 `Term '*' Factor` 를 그대로 ⊕ 해보면 이래요.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span> ⊕ <span class="setf">FIRST(</span><span class="setm">'*'</span><span class="setf">)</span> ⊕ <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span>
                          = <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span>              <span style="opacity:.6">← Term 은 ε 이 안 돼서 첫 칸에서 멈춤</span>
</pre>

코드도 정확히 이거예요 — 생성규칙([Concat](deep-concat.md))의 기호들을 차례로 `RingSum(⊕)` 하다가, 더 볼 ε 이 없으면 멈춰요.

```csharp
// FirstFollowAnalyzer [First].cs
public TerminalSet FirstSet(NonTerminalConcat singleNT, ...)
{
    TerminalSet result = new TerminalSet();

    foreach (var symbol in singleNT)                        // 생성규칙의 기호를 순서대로
    {
        result = result.RingSum(FirstSet(symbol, seenNT));  // ⊕ 한 칸
        if (!result.IsNullAble) break;                      // 더 볼 ε 없으면 멈춤
    }

    return result;
}
```

**검산** — 이 규칙을 우리 문법에 돌리면, 셋 다 같은 답이 나와요.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">Factor</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Term</span><span class="setf">)</span> = <span class="setf">FIRST(</span><span class="nt">Expr</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">'('</span>, <span class="setm">id</span> <span class="setb">}</span>
</pre>

[정의와 유도 페이지](first-formula.md)에서 손으로 구한 답과 정확히 같습니다. ✓

---

마지막으로 — 지금까지 본 **⊕**(한 생성규칙)에, *갈래(`|`) 합치기* **`∪`** 한 겹만 더 두르면, **이게 FIRST 의 *전부* 예요:**

<pre class="lrbox">
   FIRST(<span class="nt">A</span>) = ⋃ ( FIRST(X₁) ⊕ FIRST(X₂) ⊕ … )
</pre>

- 바깥 **`⋃`** : `A` 의 여러 생성규칙(`|` 갈래)을 *합침*. &nbsp;*(= 비단말의 FIRST = 모든 생성규칙 FIRST 의 합집합)*
- 안쪽 **`⊕`** : 한 생성규칙(기호 열)을 *잇되, 앞이 사라지면 다음 칸까지*. &nbsp;*(= 경우 ①②③)*
- 밑바닥 **`FIRST(단말 a) = { a }`**.

이 한 줄에 — *경우 ①②③* 과 *갈래 합치기* 가 **전부** 들어 있어요. FIRST 는 결국 이게 다예요. 🎯

## 다음 — 이 규칙이 코드로

방금 본 세 경우 — 단말 시작 · 비단말 시작 · ε — 와 "안 바뀔 때까지 반복" 이, `FirstFollowAnalyzer`
코드에 **거의 한 줄씩** 그대로 박혀 있어요.\
이어서 봐요.

👉 **[FIRST · 구현 (코드)](first-impl.md)**

---

👈 앞으로: [FIRST · 정의와 유도](first-formula.md)
