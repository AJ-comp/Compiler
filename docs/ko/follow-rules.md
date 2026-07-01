# FOLLOW · 계산 규칙

> 🎓 **심화 과정 · 이론** 이에요.\
> 앞 [FOLLOW · 정의와 유도](follow-formula.md)에서 — 정의를 잡고, 정의대로 유도하다 *"맨 끝에 오면
> LHS 의 FOLLOW 를 상속"* 하는 벽을 만났죠.\
> 이 페이지는 그 과정을 **규칙 셋** 으로 정리하고, FIRST 때처럼 **반복** 으로 풀어요. (구현은 → **[FOLLOW · 구현](follow-impl.md)**.)

> 한 번에 다 받아들이려 하지 마세요.\
> **쉬운 것부터 한 걸음씩** 갈게요.

## 먼저 — 무엇을 채우나

FOLLOW 는 FIRST 와 *채울 대상* 부터 달라요.\
FIRST 는 단말까지 8개를 구했지만, **FOLLOW 는 비단말만** 구해요.\
(단말 뒤에 무엇이 오는지는 따로 따질 일이 없거든요 — "어느 규칙이 끝났나" 를 판단하는 건 비단말이니까요.)

그래서 우리 문법에선 `Expr` · `Term` · `Factor` **셋의 FOLLOW** 만 채우면 돼요.\
채우는 도구는 규칙 셋이에요 — 앞에서 유도하며 만난 바로 그 셋이고요.

- **규칙 ①** — 시작 기호엔 `$`
- **규칙 ②** — `B` 바로 뒤에 오는 것의 FIRST
- **규칙 ③** — `B` 가 맨 끝에 오면 LHS 의 FOLLOW 상속

쉬운 것부터 하나씩.

## 규칙 ① — 시작 기호엔 `$`

**시작 기호의 FOLLOW 에는 `$`(입력의 끝)를 넣어요.**

왜 그럴까요?\
[정의와 유도](follow-formula.md)에서 봤듯, 시작 기호는 *입력 전체* 라서, 그걸 끝까지 다 읽고 나면 바로 뒤가 **입력의 끝** 이니까요.

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>) ⊇ { $ }
</pre>

## 규칙 ② — `B` 바로 뒤에 오는 것의 FIRST

생성규칙에 `A → α B β` 처럼 (`α` 는 `B` *앞* 에 오는 무엇이든, `β` 는 `B` *뒤* 에 오는 무엇이든) `B` 다음에 **무언가(β)** 가 오는 경우예요.

**그러면 그 `β` 가 만들어내는 첫 단말이 `B` 바로 뒤에 올 수 있어요.** → `FOLLOW(B)` 에 `FIRST(β)` 를 넣습니다. (단, `ε` 은 빼고요.)

왜 그럴까요?\
`B` 다음에 `β` 가 붙으니, `β` 가 유도하는 *맨 앞 단말* 이 곧 `B` 바로 뒤에 오는 단말이죠.\
그 "맨 앞 단말" 이 바로 [FIRST(β)](first-rules.md) 예요. (**FOLLOW 가 FIRST 를 재료로 쓰는 지점** 이에요.)

> 📎 `ε` 은 왜 빼요?\
> `FIRST(β)` 에 `ε` 이 있다는 건 *"β 가 통째로 사라질 수도 있다"* 는 뜻이에요.\
> `ε` 은 단말이 아니라 '사라짐' 이라, 단말 집합인 FOLLOW 에는 안 넣어요.\
> 대신 β 가 사라지면 `B` 가 사실상 맨 끝이 되니 — 그건 **규칙 ③** 이 받아줘요.

우리 문법에선 `β` 가 늘 단말로 시작해서 간단해요.

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      <span class="nt">Expr</span> 뒤 β = "<span class="setm">'+'</span> <span class="nt">Term</span>"     →  FIRST = <span class="setm">'+'</span>   →  FOLLOW(<span class="nt">Expr</span>) ⊇ { <span class="setm">'+'</span> }
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     <span class="nt">Expr</span> 뒤 β = "<span class="setm">')'</span>"          →  FIRST = <span class="setm">')'</span>   →  FOLLOW(<span class="nt">Expr</span>) ⊇ { <span class="setm">')'</span> }
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    <span class="nt">Term</span> 뒤 β = "<span class="setm">'*'</span> <span class="nt">Factor</span>"    →  FIRST = <span class="setm">'*'</span>   →  FOLLOW(<span class="nt">Term</span>) ⊇ { <span class="setm">'*'</span> }
</pre>

## 규칙 ③ — `B` 가 맨 끝에 오면 LHS 의 FOLLOW 상속  ★

이게 [정의와 유도](follow-formula.md)에서 콜아웃으로 짚은 **그 핵심 규칙** 이에요.

생성규칙에 `A → α B` 처럼 `B` 가 **맨 끝** 에 오면:

> **`FOLLOW(B)` 는 그 생성규칙 LHS 인 `A` 의 FOLLOW 를 통째로 물려받아요.** → `FOLLOW(B) ⊇ FOLLOW(A)`.

왜 그럴까요?\
`B` 가 `A` 의 끝자리를 차지하니, *`A` 다음에 올 수 있는 것* 이 곧 *`B` 다음에 올 수 있는 것* 이거든요.\
(`( Expr )` → `( Term )` 에서 `)` 가 `Term` 뒤로 옮겨오던 그 장면이에요.)

<pre class="lrbox">
   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>      <span class="nt">Term</span> 이 맨 끝   →  FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)
   <span class="nt">Expr</span> → <span class="nt">Term</span>               <span class="nt">Term</span> 이 맨 끝   →  FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    <span class="nt">Factor</span> 가 맨 끝  →  FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)
   <span class="nt">Term</span> → <span class="nt">Factor</span>             <span class="nt">Factor</span> 가 맨 끝  →  FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)
</pre>

## 왜 한 번에 안 끝나나 — 반복

규칙 ③ 이 까다로워요.\
`FOLLOW(Term)` 에 `FOLLOW(Expr)` 을 부어야 하는데, 그 `FOLLOW(Expr)` 도 아직 채워지는 중일 수 있어요.\
FIRST 때 재귀에서 막혔던 것과 똑같죠 — **서로 의존** 해서 한 번에 안 풀려요.

그래서 처방도 똑같아요 — **규칙 ①·② 로 초깃값을 채운 뒤, 규칙 ③ 을 *안 바뀔 때까지* 반복.**

## 정의대로 — 우리 문법에 돌려보기

<div class="ex-card">

**① `초깃값` — 규칙 ①·② 로 먼저 채워요**

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }      ← ① 의 $ , ② 의 <span class="setm">'+'</span> <span class="setm">')'</span>
   FOLLOW(<span class="nt">Term</span>)   = { <span class="setm">'*'</span> }              ← ② 의 <span class="setm">'*'</span>
   FOLLOW(<span class="nt">Factor</span>) = { }                  ← 아직 비어 있음
</pre>

</div>

<div class="ex-card">

**② `1바퀴` — 규칙 ③ 으로 상속하니 늘어났어요**

<pre class="lrbox">
   FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)  →  { <span class="setm">'*'</span> } ∪ { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }  =  { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }   (늘었음)
   FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)  →  { }    ∪ { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> } = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> } (늘었음)
</pre>

→ 뭔가 늘었으니, 한 바퀴 더 돌아요.

</div>

<div class="ex-card">

**③ `2바퀴` — 더 안 늘면 멈춰요**

<pre class="lrbox">
   FOLLOW(<span class="nt">Term</span>)   ⊇ FOLLOW(<span class="nt">Expr</span>)  →  변화 없음
   FOLLOW(<span class="nt">Factor</span>) ⊇ FOLLOW(<span class="nt">Term</span>)  →  변화 없음
</pre>

→ 이번 바퀴엔 아무것도 안 늘었어요. **멈춤!**

</div>

<pre class="lrbox">
   FOLLOW(<span class="nt">Expr</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
   FOLLOW(<span class="nt">Term</span>)   = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
   FOLLOW(<span class="nt">Factor</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

[정의와 유도 페이지](follow-formula.md)·[기본 과정](first-follow.md)에서 구한 답과 정확히 같아요. ✓

## ε 이 포함될 경우 — 작은 문법으로 한 번

위 expr 문법엔 사라지는(nullable) 비단말이 없어서, 규칙 ② 의 *`− ε`* 와 규칙 ③ 의 *"β 가 사라지면"* 가지가 한 번도 안 돌았어요. 이 둘을 눈으로 보려고, 이 절에서 예시로 쓸 문법은 아래와 같아요:

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

`A` 와 `B` 가 각각 ε 로 사라질 수 있어요.

<pre class="lrbox">
   <span class="setf">FIRST(</span><span class="nt">A</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">a</span>, ε <span class="setb">}</span>
   <span class="setf">FIRST(</span><span class="nt">B</span><span class="setf">)</span> = <span class="setb">{</span> <span class="setm">b</span>, ε <span class="setb">}</span>
</pre>

이제 `FOLLOW(A)` 를 보면, `S → A B` 에서 `A` 뒤에 `β = B` 가 붙어 있어요.

- **규칙 ②** — `FOLLOW(A)` 에 `FIRST(B) − ε` 를 넣어요: `{ b, ε } − ε` = `{ b }`. ← *여기서 `− ε` 가 실제로 일해요.*
- **규칙 ③** — 그런데 `B` 가 *사라질 수 있으니*, `A` 도 사실상 맨 끝이 될 수 있어요. 그래서 LHS 인 `S` 의 FOLLOW 도 물려받아요: `FOLLOW(A) ⊇ FOLLOW(S)`.

`FOLLOW(S)` 는 시작 기호라 규칙 ① 로 `{ $ }` 예요. 그리고 `B` 는 `S → A B` 의 맨 끝이라, 규칙 ③ 으로 그 `FOLLOW(S)` 를 그대로 물려받아요.

<pre class="lrbox">
   FOLLOW(<span class="nt">S</span>) = <span class="setb">{</span> <span class="setm">$</span> <span class="setb">}</span>
   FOLLOW(<span class="nt">B</span>) = <span class="setb">{</span> <span class="setm">$</span> <span class="setb">}</span>              <span style="opacity:.6">B 가 맨 끝 → FOLLOW(S) 상속</span>
   FOLLOW(<span class="nt">A</span>) = <span class="setb">{</span> <span class="setm">b</span>, <span class="setm">$</span> <span class="setb">}</span>          <span style="opacity:.6">FIRST(B)−ε = {b}, 게다가 B 가 사라질 수 있어 FOLLOW(S) 상속</span>
</pre>

`B` 가 *사라질 수 없었다면* `FOLLOW(A) = { b }` 로 끝났을 텐데, `B` 가 ε 이 될 수 있어서 `$` 까지 물려받은 거예요. 규칙 ② 의 `− ε` 와 규칙 ③ 의 "사라지면 상속" 이 둘 다 여기서 실제로 돌아요.

## 정리

규칙에 쓰는 기호는 `A → α B β` 가 다예요.

<pre class="lrbox">
   <span class="nt">A</span> → α <span class="nt">B</span> β
   <span class="nt">A</span>, <span class="nt">B</span> = 비단말
   α, β = 기호 묶음 <span style="opacity:.6">(B 앞·뒤 부분 — 단말·비단말 섞여도, 없어도 됨)</span>
</pre>

1. **시작 기호**(`Expr`)의 FOLLOW 에 `$`.
2. `B` 바로 뒤 `β` 의 **`FIRST(β) − ε`** 를 `FOLLOW(B)` 에. (FIRST 를 재료로!)
3. `B` 가 **맨 끝**(또는 `B` 뒤가 *전부 사라질 수 있으면*)이면 **LHS(`A`)의 FOLLOW** 를 상속 — *안 바뀔 때까지 반복*.

FIRST 를 재료로 쓰고, 반복으로 상속을 푼다 — 이게 FOLLOW 계산의 전부예요. 🎯

## 다음 — 이 규칙이 코드로

이 세 규칙과 반복이 `FirstFollowAnalyzer` 코드에 어떻게 들어갔는지 봐요.\
(`CalculateAllFollow` 첫 줄이 `CalculateAllFirst` 인 것부터, FIRST 를 재료로 쓰는 게 그대로 보여요.)

👉 **[FOLLOW · 구현](follow-impl.md)**

---

👈 앞으로: [FOLLOW · 정의와 유도](follow-formula.md)
