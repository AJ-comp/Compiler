# 클로저 · 계산법

> 🎓 **심화 과정** 이에요.\
> 앞 [클로저 · 정의](closure-def.md)에서 — `CLOSURE(I)` 는 *"②에 닫힌 가장 작은 집합"* 이라고 했죠.\
> 정의는 *"어떤 집합인가"* 만 말해줘요. 이 페이지는 그걸 **실제로 어떻게 구하는지** — 그것도 우리
> 예제 문법의 **진짜 시작 상태 `I₀`** 를 직접 만들어 보면서 — 한 단계씩 봐요.

## 방법

방법 자체는 단순해요.

> 어떤 아이템 집합 `I` 에서 시작해서, **규칙 ②를 *더 넣을 게 없을 때까지* 한 단계씩 적용** 한다.

핵심은 *"더 넣을 게 없을 때까지"* 예요. 집합이 한 단계씩 **자라다가**, 어느 순간 *아무것도 안
늘어나면* — 그게 닫힌 거고, 멈춰요.\
[FIRST/FOLLOW](first-rules.md)에서 본 **고정점(안 바뀔 때까지 반복)** 과 똑같은 결이에요.

## 그 전에 — 시작은 어디서? (증대 문법)

클로저는 *시작 아이템 한 개* 에서 출발해요. 그 한 개는 어디서 올까요?

여기서 작은 장치 하나가 등장해요. LR 파서는 원래 문법 맨 위에 **시작 규칙을 딱 하나 덧대요** —
`Accept → Expr` 예요. (`Expr` 는 우리 문법의 원래 시작 기호죠.)\
이렇게 *시작 규칙 하나를 더 얹은* 문법을 **증대 문법(augmented grammar)** 이라고 불러요.

왜 이런 걸 덧댈까요? — **파서가 "이제 입력 전체가 끝났다" 를 깔끔하게 알기 위해서** 예요.\
원래 시작 기호 `Expr` 는 식 *안* 곳곳에 등장해요 (`Expr '+' Term` 의 그 `Expr`, `'(' Expr ')'` 의 그
`Expr` 처럼요). 그래서 `Expr` 하나를 완성했다고 해서 *입력 전체* 가 끝난 건 아니죠 — 더 큰 식의
일부일 수도 있으니까요.\
하지만 *어디에도 안 쓰이는* `Accept` 를 하나 얹어 두면 — `Accept → Expr` 를 완성하는 **그 순간이 곧
"전체 끝(accept)"** 이라고 단번에 알 수 있어요. (이 "끝 신호" 가 실제로 어떻게 쓰이는지는 *파싱
테이블* 장에서 매듭지어요. 지금은 *"시작점"* 으로만 써요.)

쉽게는 — **택배 상자** 를 떠올려 봐요. 상자 안에 또 작은 상자가 들어 있을 수 있죠 (우리 `Expr` 안에
`Expr` 이 또 들어가듯요 — `'(' Expr ')'` 처럼요). 그럼 *"어느 게 맨 바깥 상자냐"* 가 헷갈려요.\
그래서 **전체를 한 번 더 감싸는 겉상자** 를 씌우고 *"이게 마지막 겉포장"* 이라고 적어 두는 거예요.\
그 겉상자가 바로 `Accept → Expr` — 겉상자까지 다 닫으면(완성하면) *"전부 끝(accept)!"* 이라고 단번에
알죠.

그래서 출발 아이템은 딱 하나 — `Accept → • Expr` 예요.\
여기에 클로저를 돌린 결과가 곧 **시작 상태 `I₀`** 고요. 직접 자라나는 걸 봐요.

## 한 단계씩 — `I₀` 가 자라는 걸 보며

따라가기 전에 **증대 문법** 을 펴 두고 갈게요. (각 단계에서 *어떤 생성규칙이* 들어오는지, 여기서
바로바로 짚으려고요.)

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>   |  <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>  |  <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     |  <span class="setm">id</span></pre>

**시작 — 1개.** 가상 시작 아이템 하나에서 출발해요.

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span></pre>

**1단계.** `Accept → • Expr` 의 점 뒤는 `Expr` 죠.\
문법에서 `Expr` 줄을 보면, 생성규칙이 **`Expr → Expr '+' Term`** 과 **`Expr → Term`** 둘이에요.\
이 둘을 — 아직 아무것도 안 읽었으니 점을 맨 앞에 찍어 — 집합에 추가해요.

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>       ← 새로
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>                ← 새로</pre>

→ **3개.**

**2단계.** 방금 들어온 둘의 점 뒤를 봐요.

- `Expr → • Term` 의 점 뒤는 `Term` 이에요. 문법에서 `Term` 의 생성규칙은 **`Term → Term '*' Factor`**
  와 **`Term → Factor`** 둘이죠. 이 둘을 (점 맨 앞에 찍어) 추가해요.
- `Expr → • Expr '+' Term` 의 점 뒤도 `Expr` 지만 — `Expr` 는 **1단계에서 이미 펼쳤어요** (그 두 규칙이
  이미 집합에 있죠). 그래서 새로 넣을 게 없어요.

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>     ← 새로
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>              ← 새로</pre>

→ **5개.**

**3단계.** 또 방금 들어온 둘의 점 뒤를 봐요.

- `Term → • Factor` 의 점 뒤는 `Factor` 예요. 문법에서 `Factor` 의 생성규칙은 **`Factor → '(' Expr ')'`**
  와 **`Factor → id`** 둘이죠. 이 둘을 추가해요.
- `Term → • Term '*' Factor` 의 점 뒤는 `Term` → **이미 펼쳤으니** 새 거 없음.

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>        ← 새로
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span>                  ← 새로</pre>

→ **7개.**

**4단계.** 남은 점 뒤를 봐요. `Factor → • '(' Expr ')'` 의 점 뒤는 `'('`, `Factor → • id` 의 점 뒤는
`id` — 둘 다 **단말** 이에요.\
단말은 *시작할 생성규칙* 이 없으니 펼칠 게 없죠. 더 넣을 것도 없고요 → **닫혔어요. 끝!**

## 정리 — 이게 `I₀` 예요

집합이 **1개 → 3개 → 5개 → 7개** 로 *자라다가, 더 늘 게 없어 멈췄어요.*\
이 마지막 **7개짜리 닫힌 집합** 이 바로 우리 문법의 **시작 상태 `I₀`** 예요.

<pre class="lrbox">   I₀ = CLOSURE( { <span class="nt">Accept</span> → •<span class="nt">Expr</span> } )
      = <span class="nt">Accept</span>·<span class="nt">Expr</span>·<span class="nt">Term</span>·<span class="nt">Factor</span> 의 생성규칙들이 전부 점 맨 앞에 모인 7개 아이템</pre>

*"맨 앞에 올 수 있는 것 전부"* 가 `I₀` 에 담긴 거예요 — `Factor → •'(' Expr ')'` 와 `Factor → •id`
의 점 뒤 `(`·`id` 가, 곧 *맨 처음 읽을 수 있는 단말* 이죠. (어, 그거 [FIRST(Expr)](first-rules.md)
`= { '(', id }` 와 같네요! 우연이 아니에요.)

## 한 걸음 더 — 저자는 이 과정을 '재귀' 로 적어요

방금은 집합이 **옆으로 한 단계씩 자라는** 걸 봤죠. 같은 계산을, 저자의 설계 노트는 조금 더 압축된
꼴로 적어요 — **`Closure` 안에 또 `Closure`** 가 들어가는 *재귀* 모양이에요.

읽는 법만 알면 어렵지 않아요.

- `Closure({ … })` 는 *"이 아이템들은 아직 더 펼쳐야 한다"* 는 표시예요.
- <span class="lrmark">빨간 기호</span> 는 *지금 펼치는 중인 점 뒤 기호* (마커) 고요.
- 한 줄 내려갈 때마다, 그 빨간 비단말의 생성규칙들이 새 `Closure({ … })` 로 끌려 들어와요. (앞 줄에서
  이미 정리된 아이템은 *생략* 하고, 맨 끝 줄에 다 모아요.)

<pre class="lrbox">Closure({ <span class="nt">Accept</span> → • <span class="lrmark">Expr</span> })
 = { <span class="nt">Accept</span> → • <span class="nt">Expr</span>,   Closure({ <span class="nt">Expr</span> → • <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>,  <span class="nt">Expr</span> → • <span class="nt">Term</span> }) }
 = { <span class="nt">Expr</span> → • <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>,  <span class="nt">Expr</span> → • <span class="lrmark">Term</span>,   Closure({ <span class="nt">Term</span> → • <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>,  <span class="nt">Term</span> → • <span class="nt">Factor</span> }) }
 = { <span class="nt">Term</span> → • <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>,  <span class="nt">Term</span> → • <span class="lrmark">Factor</span>,   Closure({ <span class="nt">Factor</span> → • <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>,  <span class="nt">Factor</span> → • <span class="setm">id</span> }) }
 = { <span class="nt">Factor</span> → • <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>,  <span class="nt">Factor</span> → • <span class="setm">id</span> }      <span style="opacity:.6">(점 뒤가 '(' · id — 단말이라 멈춤)</span>
 = { <span class="nt">Accept</span>→•<span class="nt">Expr</span>, <span class="nt">Expr</span>→•<span class="nt">Expr</span><span class="setm">'+'</span><span class="nt">Term</span>, <span class="nt">Expr</span>→•<span class="nt">Term</span>, <span class="nt">Term</span>→•<span class="nt">Term</span><span class="setm">'*'</span><span class="nt">Factor</span>, <span class="nt">Term</span>→•<span class="nt">Factor</span>, <span class="nt">Factor</span>→•<span class="setm">'('</span><span class="nt">Expr</span><span class="setm">')'</span>, <span class="nt">Factor</span>→•<span class="setm">id</span> }   <span style="opacity:.6">= I₀</span></pre>

맨 끝에서 `Closure({ … })` 가 *사라졌죠?* 더 펼칠 게 없다는 뜻 — 즉 **닫힌** 거예요. 그게 `I₀` 고요.\
(*"이미 펼친 비단말은 다시 안 펼친다"* 는 약속도 그대로예요 — 그래서 `Expr → •Expr '+' Term` 의 그
`Expr`, `Term → •Term '*' Factor` 의 그 `Term` 은 마커로 안 잡혀요. 안 그러면 끝없이 돌 테니까요.)

그리고 이 **`Closure` 가 `Closure` 를 부르는 재귀 모양** 이 — 바로 다음 [구현](closure-impl.md) 의 코드
`result.UnionWith(Closure( … ))` 와 *판박이* 예요. 손으로 적던 걸 그대로 코드로 옮긴 셈이죠.\
(저자의 원본 노트는 자기 테스트 문법 `S' → G`, `G → E = E | f`, … 로 그려져 있지만, 펼치는 원리는
똑같아요.)

## 다음

손으로 돌려본 이 "닫힐 때까지 한 단계씩" 을, 코드는 어떻게 했을까요?

👉 **[클로저 · 구현](closure-impl.md)**

---

👈 앞으로: [클로저 · 정의](closure-def.md)
