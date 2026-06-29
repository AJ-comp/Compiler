# FIRST · 정의와 유도

> 🎓 **심화 과정 · 이론** 이에요. [기본 과정의 FIRST/FOLLOW](first-follow.md)에서 *개념* 을 먼저
> 잡고 오시면 좋아요. 이 페이지에선 **FIRST 가 정확히 무엇인지(정의)** 와, 그 **정의대로 직접
> 유도해서 구하는 과정** 까지 봐요.
>
> 부담 갖지 마세요 — 천천히 갈게요.

> 📍 **사는 곳** · 엔진 `FirstFollowAnalyzer` · 모듈 `Janglim.FrontEnd` — **Layer 2** (파싱 테이블보다
> *아래* 단인 밑단)

계속 쓰는 예제 문법이에요.

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
</pre>

기본에서 손으로 구한 답은 `FIRST(Expr) = FIRST(Term) = FIRST(Factor) = { '(', id }` 였어요.\
그런데 그 전에 — **FIRST 가 *정확히* 무엇인지** 부터 또박또박 정의하고 갈게요.\
(이게 빠지면 뒤의 계산 규칙이 그냥 외우는 주문처럼 돼버려요.)

---

## 정의 — FIRST 란 무엇인가

한 줄로 먼저 못박을게요.

> **FIRST(X)** = 기호 `X` 를 **유도해서 가장 처음 나타날 수 있는 단말**들의 집합.

핵심 단어가 둘이에요 — **유도** 와 **가장 처음 나타나는 단말**(곧 *맨 앞* 에 오는 그 단말).\
하나씩 봐요.

### "유도(derivation)" — 규칙으로 펼치는 것

비단말을 그 생성규칙의 우변으로 **바꿔치기** 하는 한 걸음을 *유도* 라 하고, 화살표 `⇒` 로 써요.

`Expr` 를 한 걸음씩 펼쳐볼게요.

<pre class="lrbox">
   <span class="nt">Expr</span>  ⇒  <span class="nt">Term</span>  ⇒  <span class="nt">Factor</span>  ⇒  <span class="setm">id</span>
        (Expr:Term)  (Term:Factor)  (Factor:id)
</pre>

규칙을 세 번 적용해서, 결국 **단말로만 된 문자열** `id` 에 도달했죠.\
이렇게 *여러 걸음* 펼치는 걸 `⇒*` 로 적어요 → `Expr ⇒* id` ("Expr 는 id 를 유도한다").

### 그래서 FIRST 는

`X` 를 *온갖 방법으로* 끝까지 펼쳐서, 거기서 **가장 처음 나타날 수 있는 단말**(= 맨 앞에 오는 단말) 을 다 모은 거예요.

`Expr` 로 직접 보면:

<pre class="lrbox">
   <span class="nt">Expr</span> ⇒* <span class="setm">id</span> …             →  맨 앞이 <span class="setm">id</span>   ⇒   <span class="setm">id</span> ∈ FIRST(<span class="nt">Expr</span>)
   <span class="nt">Expr</span> ⇒* <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span> …       →  맨 앞이 <span class="setm">(</span>    ⇒   <span class="setm">(</span> ∈ FIRST(<span class="nt">Expr</span>)
</pre>

`Expr` 가 만들 수 있는 문자열은 무수히 많지만(`id`, `id + id`, `( id ) * id`, …), **맨 앞에 올 수
있는 단말** 은 결국 `id` 아니면 `(`, 둘뿐이에요. 그래서:

<pre class="lrbox">
   FIRST(<span class="nt">Expr</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

기호로 더 단단히 적으면 이래요 (`T` = 단말 집합):

<pre class="lrbox">
   FIRST(<span class="nt">X</span>) = { <span class="setm">a</span> ∈ T | <span class="nt">X</span> ⇒* <span class="setm">a</span> … (<span class="setm">a</span> 로 시작하는 문자열을 유도) }
</pre>

> 📎 **ε(빈 문자열) 한 가지만 더.** 만약 `X` 가 *아무것도 아닌 것* 까지 유도할 수 있으면
> (`X ⇒* ε`), **ε 도 FIRST(X) 에 넣어요.** "X 는 통째로 사라질 수도 있다" 는 표시예요. (우리
> 예제엔 그런 비단말이 없어서 ε 은 안 나와요 — 하지만 정의엔 꼭 포함돼요.)

정리하면 — 단말이든, 비단말이든, *기호 여러 개로 된 열* 이든, **"유도해서 가장 처음 나타날 수 있는 단말(+필요하면 ε) 집합"** 이 FIRST 예요.\
이게 정의의 전부예요.

---

## 정의대로 — 유도해서 직접 구해보기

이제 "유도해서 맨 앞 단말 모으기" 를 그 정의를 *직접 적용* 해서 손으로 구해볼게요.\
쉬운 `Factor` 부터.

### Factor — 막힘 없이 끝까지 펼쳐져요

`Factor` 의 두 **생성규칙**(`|` 로 갈린 한 줄씩 — `A → α` 꼴, 여기서 `α` 는 *우변의 기호 열*. 자세힌 [Single](deep-single.md))을 끝까지 유도해봐요.

<pre class="lrbox">
   <span class="nt">Factor</span> ⇒ <span class="setm">id</span>              →  맨 앞 단말 :  <span class="setm">id</span>
   <span class="nt">Factor</span> ⇒ <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span>        →  맨 앞 단말 :  <span class="setm">(</span>
</pre>

`Factor` 가 내놓는 문자열의 맨 앞은 `id` 아니면 `(` — 둘뿐이에요.\
정의 그대로 모으면:

<pre class="lrbox">
   FIRST(<span class="nt">Factor</span>) = { <span class="setm">id</span>, <span class="setm">'('</span> }
</pre>

쉽죠?\
`Factor` 안엔 자기 자신이 안 나와서, 유도가 깔끔하게 끝나요.

### Term — 펼치다 보면 자기 자신이 또 나와요

`Term : Term '*' Factor | Factor`.\
두 생성규칙을 유도해볼게요.

<pre class="lrbox">
   ① <span class="nt">Term</span> ⇒ <span class="nt">Factor</span> ⇒ … ⇒ <span class="setm">id</span> 또는 <span class="setm">(</span>          →  맨 앞 :  <span class="setm">id</span>, <span class="setm">(</span>
   ② <span class="nt">Term</span> ⇒ <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>                  →  맨 앞이 또 <span class="nt">Term</span> ?!
</pre>

②번이 묘해요.\
맨 앞이 *자기 자신* `Term` 이라, 맨 앞 단말을 알려면 그 `Term` 을 **또** 펼쳐야 해요.

<pre class="lrbox">
   <span class="nt">Term</span> ⇒ <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
        ⇒ <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
        ⇒ …                              ←  펼쳐도 펼쳐도 맨 앞은 계속 <span class="nt">Term</span>, 끝이 안 보임
</pre>

하지만 결국 맨 앞 `Term` 도 언젠가 `Factor` 생성규칙으로 내려앉고, 그럼 맨 앞 단말은 또 `id` 나 `(` 가 돼요.
그래서:

<pre class="lrbox">
   FIRST(<span class="nt">Term</span>) = { <span class="setm">id</span>, <span class="setm">'('</span> }
</pre>

`Expr`(`Expr : Expr '+' Term | Term`) 도 똑같은 모양이라, `FIRST(Expr) = { id, '(' }` 가 돼요.

### 여기서 걸려요 — "끝까지" 유도를 못 해요

`Term` 에서 봤듯, **자기 자신을 물고 있으면(재귀) 유도가 무한히 길어질 수 있어요.**\
정의는 명확한데 ("유도해서 가장 처음 나타나는 단말"), 그 유도를 *일일이 끝까지* 펼치는 건 손으로도 컴퓨터로도 곤란해요.

## 다음 — 계산 규칙으로

그래서 다음 페이지에선 — **유도를 직접 펼치지 않고도** 같은 FIRST 를 뽑아내는 **계산 규칙** 으로
가요.\
(재귀도 얌전히 처리되고요.)

👉 **[FIRST · 계산 규칙](first-rules.md)**

---

👈 기본 개념으로 돌아가기: [FIRST / FOLLOW](first-follow.md)
