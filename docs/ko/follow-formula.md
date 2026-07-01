# FOLLOW · 정의와 유도

> 🎓 **심화 과정 · 이론** 이에요.\
> [FIRST 트랙](first-formula.md)을 끝냈으니, 이제 그 짝꿍 **FOLLOW** 예요.\
> FIRST 가 **"무엇으로 *시작* 하나"** 였다면, FOLLOW 는 **"무엇이 그 *다음에* 오나"** 예요.\
> 이 페이지에선 **정의** 와 **정의대로 직접 유도** 까지 봐요. 이어서 → **계산 규칙** → **구현**.

> 📍 **사는 곳** · 엔진 `FirstFollowAnalyzer` · 모듈 `Janglim.FrontEnd` — **Layer 2** (파싱 테이블보다
> *아래* 단인 밑단)

계속 쓰는 예제 문법이에요.\
이번엔 **시작 기호** 가 중요하니 표시해 둘게요 — 맨 위 `Expr` 가 시작 기호예요.

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;      ← 시작 기호
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
</pre>

[기본 과정](first-follow.md)에서 손으로 구한 답은 이랬어요.

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
   FOLLOW(<span class="nt">Term</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
   FOLLOW(<span class="nt">Factor</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

이걸 *정의* 부터 다시 또박또박 봅니다.

---

## 정의 — FOLLOW 란 무엇인가

한 줄로 먼저 분명히 할게요.

> **FOLLOW(B)** = 올바른 문장 어딘가에서 비단말 `B` **바로 다음에** 나타날 수 있는 **단말** 들의
> 집합.\
> (그리고 `B` 가 문장의 *맨 끝* 에 올 수 있으면, 입력의 끝을 뜻하는 `$` 도 넣어요.)

FIRST 와 짝을 맞춰 보면, FIRST 는 그 기호가 *유도해서 가장 처음* 올 수 있는 단말이고, FOLLOW 는 그 기호 *바로 뒤* 에 올 수 있는 단말이에요.

### "바로 뒤" 를 유도로 보기

[FIRST 정의](first-formula.md)에서 *유도(⇒)* 를 정의했죠 — 비단말을 생성규칙 우변으로 바꿔치기하는
거요.\
FOLLOW 는, 시작 기호 `Expr` 에서 유도해 나가다가 **`B` 바로 뒤에 어떤 단말이 붙는지** 를 보는
거예요.

<pre class="lrbox">
   <span class="nt">Expr</span>  ⇒*  …  <span class="nt">B</span>  <span class="setm">a</span>  …       →  <span class="setm">a</span> 가 <span class="nt">B</span> 바로 뒤에 왔다   ⇒   <span class="setm">a</span> ∈ FOLLOW(<span class="nt">B</span>)
</pre>

기호로 단단히 적으면 이래요 (`S` = 시작 기호, `T` = 단말 집합).

<pre class="lrbox">
   FOLLOW(<span class="nt">B</span>) = { <span class="setm">a</span> ∈ T | <span class="nt">S</span> ⇒* … <span class="nt">B</span> <span class="setm">a</span> … }   ∪   ( { $ }  if  <span class="nt">S</span> ⇒* … <span class="nt">B</span> )
</pre>

> 📎 FIRST 와 결정적으로 다른 점 하나.\
> FIRST 는 그 기호 **하나만** 보면 됐지만, FOLLOW 는 **`B` 가 문법 전체에서 *어디어디 쓰이는지* 를
> 다 훑어야** 해요. `B` 뒤에 무엇이 오는지는, `B` 를 쓴 자리마다 다르니까요.

---

## 정의대로 — 유도해서 직접 구해보기

"`B` 바로 뒤에 오는 단말 모으기" 를 정의를 *직접 적용* 해서 손으로 구해볼게요.

### FOLLOW(Expr) — 깔끔하게 나와요

`Expr` 은 **시작 기호** 예요 — 즉 *입력 전체* 가 하나의 `Expr` 이죠.\
그럼 그 `Expr` 을 끝까지 다 읽고 나면, 바로 뒤엔 뭐가 올까요?\
더 읽을 것이 없는 **입력의 끝** 이에요.\
그 입력의 끝을 표시하는 가상의 토큰이 `$` 였죠.\
그러니 `Expr` 바로 뒤엔 `$` 가 오는 셈이에요 — **`$ ∈ FOLLOW(Expr)`.**

이제 문법 전체에서 `Expr` *바로 뒤* 에 오는 걸 찾아봐요.

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      <span class="nt">Expr</span> 바로 뒤 :  <span class="setm">'+'</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     <span class="nt">Expr</span> 바로 뒤 :  <span class="setm">')'</span>
</pre>

`Expr` 뒤에 올 수 있는 건 `'+'` 와 `')'`, 그리고 맨 끝의 `$`. 다 모으면:

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
</pre>

### FOLLOW(Term) — 맨 끝에 오면 물려받아요

`Term` *바로 뒤* 를 문법에서 찾아봐요.

<pre class="lrbox">
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    <span class="nt">Term</span> 바로 뒤 :  <span class="setm">'*'</span>                →  <span class="setm">'*'</span> 추가
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      <span class="nt">Term</span> 이 생성규칙의 맨 끝 …          →  뒤에 아무것도 없네?
   <span class="nt">Expr</span> → <span class="nt">Term</span>               <span class="nt">Term</span> 이 생성규칙의 맨 끝 …          →  역시 뒤에 없음
</pre>

`'*'` 는 바로 들어가요.\
그런데 뒤 두 줄이 묘해요 — `Term` 이 생성규칙의 **맨 끝** 에 있어서, 바로 뒤에 단말이 없어요.

그럼 `Term` 다음엔 뭐가 올까요?\
`Expr → Term` 을 보면, `Term` 이 `Expr` 의 *전부* 예요 — 즉 **`Expr` 과 `Term` 이 똑같은 자리에서 끝나죠.**

말로는 추상적이니 한 장면으로 볼게요.\
`( Expr )` 조각을 떠올려요 (`Factor → '(' Expr ')'`). 여기선 `Expr` 바로 뒤에 `)` 가 오죠.\
그런데 그 `Expr` 이 그냥 `Term` 하나라면 (`Expr → Term`), 똑같은 자리가 `( Term )` 가 돼요.

<pre class="lrbox">
   <span class="setm">(</span> <span class="nt">Expr</span> <span class="setm">)</span>        →   <span class="nt">Expr</span> 뒤에 <span class="setm">)</span>
   <span class="setm">(</span> <span class="nt">Term</span> <span class="setm">)</span>        →   이제 <span class="nt">Term</span> 뒤에 <span class="setm">)</span>   (같은 자리!)
</pre>

보세요 — 아까 `Expr` 뒤에 있던 `)` 가, 이제 **`Term` 바로 뒤** 에 와 있어요.\
`Term` 이 `Expr` 의 끝자리를 그대로 물려받았으니까요.

> 💡 **이게 FOLLOW 의 핵심 규칙이에요.**\
> 어떤 비단말이 생성규칙의 **맨 끝** 에 오면, 그 생성규칙 **LHS(왼쪽 비단말)의 FOLLOW 를 통째로
> 물려받아요.**\
> 여기선 `Expr → Term` 이라 — **`FOLLOW(Expr)` 이 그대로 `FOLLOW(Term)` 으로 흘러들어옵니다.**

<pre class="lrbox">
   FOLLOW(<span class="nt">Term</span>) = { <span class="setm">'*'</span> }  ∪  FOLLOW(<span class="nt">Expr</span>)
                = { <span class="setm">'*'</span> }  ∪  { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }   =   { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

### ε 이 포함될 경우 — 작은 문법으로

지금까지 `B` 뒤 `β` 는 늘 단말(`'+'`, `')'`, `'*'`)이라 단순했어요. 그런데 `β` 가 *사라질 수 있는* 비단말이면 한 겹이 더 생겨요. 우리 expr 문법엔 그런 비단말이 없으니, 이 절에서 예시로 쓸 문법은 아래와 같아요:

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

`A` 와 `B` 가 ε 로 사라질 수 있어요. `FOLLOW(A)` 를 정의대로 보면, `S → A B` 에서 `A` 바로 뒤엔 `B` 가 와요.

- `B` 가 *안 사라지면*: `A` 뒤 첫 단말은 `B` 의 첫 단말 `b` 예요. → `b ∈ FOLLOW(A)`. (`FIRST(B)` 의 `ε` 은 단말이 아니니 빼서 `FIRST(B) − ε` 만 넣어요.)
- `B` 가 *사라지면*: `S ⇒ A B ⇒ A` (B 가 ε) 이라 `A` 가 `S` 의 맨 끝이 되니, `A` 뒤는 곧 `S` 뒤가 되어 `FOLLOW(S)` 를 그대로 물려받아요. `FOLLOW(S) = { $ }` (시작 기호)니 `$ ∈ FOLLOW(A)`.

<pre class="lrbox">
   FOLLOW(<span class="nt">A</span>) = ( FIRST(<span class="nt">B</span>) − ε )  ∪  FOLLOW(<span class="nt">S</span>)  =  <span class="setb">{</span> <span class="setm">b</span> <span class="setb">}</span> ∪ <span class="setb">{</span> <span class="setm">$</span> <span class="setb">}</span> = <span class="setb">{</span> <span class="setm">b</span>, <span class="setm">$</span> <span class="setb">}</span>
</pre>

`β`(여기선 `B`)가 사라질 수 있어서 `$` 까지 흘러든 거예요. 방금 본 두 가지, 즉 *`FIRST(β)` 에서 `ε` 만 빼서 넣기* 와 *`β` 가 사라지면 LHS 의 FOLLOW 를 상속하기* 가 곧 아래 정리의 규칙 2·3 이에요.

### 왜 한 번에 안 끝날까 — 물려받을 FOLLOW 가 아직 안 정해졌을 수 있어요

방금 봤듯, `B` 가 생성규칙의 **맨 끝** 에 오면, 그 생성규칙 **왼쪽 비단말(LHS)의 FOLLOW** 가
필요해요.\
그런데 그 LHS 의 FOLLOW 도 아직 채워지는 중일 수 있어요. (FIRST 때 재귀에서 막혔던 것과 똑같은
상황이죠.)\
그래서 정의를 *일일이 따라가는* 것만으론 한 번에 안 끝나요.

그래서 다음 페이지에선 — 이 과정을 **규칙 몇 개로 정리** 하고, FIRST 때처럼 **안 바뀔 때까지 반복**
으로 풀어요.

---

## 정리

정의대로 따라가 보니, FOLLOW 는 결국 세 가지로 채워졌어요.

1. **시작 기호** 의 FOLLOW 엔 `$` 가 들어간다. (문장 맨 끝에 올 수 있으니까.)\
   `$ ∈ FOLLOW(시작 기호)`
2. `B` **바로 뒤에 무언가 오면**, 그 뒤엣것의 *첫 단말* 이 FOLLOW(B) 에 들어간다.\
   `A → α B β` 면, `FOLLOW(B)` 에 `FIRST(β) − ε`
3. `B` 가 생성규칙의 **맨 끝** 에 오면, 그 생성규칙 **LHS 의 FOLLOW** 를 물려받는다.\
   `A → α B` 면, `FOLLOW(B)` 에 `FOLLOW(A)` 상속

## 다음 — 이 과정을 규칙으로

이 셋을 *어떤 문법에나* 통하는 계산 규칙으로 정리하고, 반복으로 푸는 게 다음이에요.

👉 **[FOLLOW · 계산 규칙](follow-rules.md)**

---

👈 기본 개념으로 돌아가기: [FIRST / FOLLOW](first-follow.md)
