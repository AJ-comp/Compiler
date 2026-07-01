# FIRST / FOLLOW

이 장부터 LR 파싱의 "심장"으로 들어가요.\
첫 개념은 **FIRST 집합**과 **FOLLOW 집합**입니다.

솔직히 미리 말해둘게요 — 이름도 낯설고, 처음 보면 "이게 대체 뭐지?" 싶을 수 있어요.\
**그래도 괜찮아요. 원래 다들 여기서 한 번씩 멈칫해요.**\
컴파일러 수업에서 가장 많이 헤매는
부분이 바로 여기거든요.\
그러니 한 번에 안 와닿아도 전혀 이상한 게 아니에요.\
차근차근 짚어갈게요.\
천천히 따라오면, 이 두 집합이 왜 필요한지 자연스럽게 보일 거예요. 🙂

---

> 📖 **시작 전에:** 이 장은 예제 문법을 *읽을 줄 안다*고 가정해요. `:` `|` `;` 같은 표기가
> 낯설다면 [문법 읽는 법](grammar-reading.md)을 먼저 보고 오세요 — 5분이면 충분해요.

이 장에서 계속 쓰는 예제 문법이에요 (참고용):

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
<span class="setm">id</span>     := "[a-zA-Z]+" ;
</pre>

---

## ① 왜 필요한가

파서가 토큰을 왼쪽부터 하나씩 읽어 나가다 보면, **끊임없이 결정을 내려야** 해요.\
그중 가장
중요한 결정 두 가지가 이거예요.

1. **"지금 어떤 규칙을 시작해도 되나?"** — 다음 토큰을 보고 어디로 들어갈지 골라야 함
2. **"지금 읽던 규칙이 끝났나?"** — 어디까지가 한 덩어리인지, 언제 묶을지 정해야 함

말로만 하면 추상적이죠.\
예를 들어볼게요.\
`a + a * a` 를 읽다가 **첫 `a` 를 막 봤다**고 합시다.\
파서는 이런 고민에 빠져요.

> "이 `a` 하나로 한 덩어리(`Term`)를 끝낸 걸로 칠까? 아니면 뒤에 `* 무언가` 가 더 붙는,
> 아직 안 끝난 덩어리일까?"

어떻게 판단할까요?\
의외로 간단해요.\
**다음 토큰을 슬쩍 봅니다.**

- 다음이 `*` → 아직 안 끝났다 (곱셈이 더 붙는다)
- 다음이 `+` 거나 입력의 끝 → 여기서 끝이다 (묶어도 된다)

즉 파서는 **"어떤 토큰이 이 덩어리 *다음에* 올 수 있는가"** 를 미리 알아야 해요.\
그게 바로
**FOLLOW**예요.\
그리고 **"어떤 토큰이 이 덩어리를 *시작* 할 수 있는가"** 가 **FIRST**고요.

> 💡 너무 깊게 생각하지 마세요. 한 줄 요약은 이거예요:
> **FIRST/FOLLOW = 파서가 "시작/끝"을 판단하려고 미리 만들어 두는 *치트시트*.**
> 이게 있어야 뒤에 나올 [파싱 테이블](parse-table-build.md)을 만들 수 있어요.

## ② 무엇을 하는가

> 여기서부터 집합을 직접 손으로 구해볼 거예요. **계산이 좀 많아 보여도 부담 갖지 마세요.**
> 패턴은 단순하고, 천천히 한 줄씩 같이 채워나갈 거니까요.

### FIRST — "이건 어떤 토큰으로 *시작* 될 수 있나"

**FIRST(X)** = X 를 유도해서 **가장 처음 나타날 수 있는 단말(토큰)들의 모음**. (= 그 문자열의 *맨 앞* 에 오는 단말)

비단말 셋 — `Factor` · `Term` · `Expr` — 의 FIRST 를 하나씩 구해봐요. 제일 쉬운 것부터.

<div class="ex-card">

**① `Factor` — 막힘 없이 끝나요**

<pre class="lrbox">
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
</pre>

`Factor` 는 `(` 로 시작하거나 `id` 로 시작하죠? 그러니:

<pre class="lrbox">
FIRST(<span class="nt">Factor</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

> `{ }` 는 *집합* 을 뜻하는 표시예요 — 중괄호 안에 든 게 그 후보들이고요.

</div>

<div class="ex-card">

**② `Term` — 자기 자신이 또 나와요**

<pre class="lrbox">
<span class="nt">Term</span> : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
</pre>

`Term` 은 `Term` 으로 시작하거나(자기 자신! — 아까 그 재귀죠) `Factor` 로 시작해요.\
자기 자신으로
시작하는 걸 계속 펼쳐보면 결국 맨 앞은 `Factor` 가 돼요.\
그러니:

<pre class="lrbox">
FIRST(<span class="nt">Term</span>) = FIRST(<span class="nt">Factor</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

</div>

<div class="ex-card">

**③ `Expr` — 같은 모양**

같은 논리로 `Expr` 도:

<pre class="lrbox">
FIRST(<span class="nt">Expr</span>) = { <span class="setm">'('</span>, <span class="setm">id</span> }
</pre>

</div>

셋 다 `(` 아니면 `id` 로 시작하네요.\
말이 되죠?\
어떤 수식이든 결국 맨 앞은 **이름(`id`)** 이거나
**여는 괄호(`(`)** 니까요.\
**여기까지 따라왔으면 FIRST 는 끝!**\
생각보다 별거 없죠?

### FOLLOW — "이것 *다음에* 어떤 토큰이 올 수 있나"

**FOLLOW(X)** = 올바른 문장 어딘가에서 X **바로 다음에 나타날 수 있는 단말들의 모음**.
여기엔 특별한 기호 **`$`**(입력의 끝을 뜻하는 가상의 토큰)도 들어갈 수 있어요.

이번에도 셋 — `Expr` · `Term` · `Factor` — 을 하나씩 봐요.

<div class="ex-card">

**① `Expr` — 시작 기호부터**

"`Expr` 다음에 뭐가 올 수 있지?" 를 문법 전체에서 찾아보면:

- `Expr` 은 **시작 기호**예요 (문장 전체가 `Expr`) → 그러니 `Expr` 다음엔 **입력의 끝 `$`** 가 올 수 있음
- `Expr : Expr '+' Term` 에서 → 첫 `Expr` 다음엔 `+`
- `Factor : '(' Expr ')'` 에서 → `Expr` 다음엔 `)`

다 모으면:

<pre class="lrbox">
FOLLOW(<span class="nt">Expr</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span> }
</pre>

</div>

<div class="ex-card">

**② `Term` — `Expr` 의 FOLLOW 를 물려받아요**

`Term` 도 똑같이 "Term 다음에 오는 것"을 훑어보면:

- `Expr` 의 맨 끝이 `Term` 인 경우가 있죠(`Expr : ... Term`, `Expr : Term`) → 그러면 **Expr 다음에
  올 수 있는 건 Term 다음에도 올 수 있어요** → FOLLOW(Expr) 가 그대로 들어옴
- `Term : Term '*' Factor` → 첫 `Term` 다음엔 `*`

<pre class="lrbox">
FOLLOW(<span class="nt">Term</span>) = FOLLOW(<span class="nt">Expr</span>) ∪ { <span class="setm">'*'</span> } = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

> `∪` 는 *합집합* 기호예요 — 두 모음을 그냥 합친다는 뜻이고요.

</div>

<div class="ex-card">

**③ `Factor` — `Term` 과 똑같이**

`Factor` 도 같은 식으로 자리를 짚어 보면:

- `Factor` 는 늘 `Term` 규칙의 *맨 끝* 에 와요(`Term : Term '*' Factor`, `Term : Factor`) → 그러면 **Term 다음에
  올 수 있는 건 Factor 다음에도 올 수 있어요** → FOLLOW(Term) 이 그대로 들어옴

<pre class="lrbox">
FOLLOW(<span class="nt">Factor</span>) = FOLLOW(<span class="nt">Term</span>) = { $, <span class="setm">'+'</span>, <span class="setm">')'</span>, <span class="setm">'*'</span> }
</pre>

</div>

### 자, 다시 처음 그 고민으로 돌아가요

기억나죠?\
`a`(즉 `Factor` → `Term`)를 읽고 다음 토큰을 봤을 때의 그 고민.\
이제 답이 보여요.

- 다음이 `*` → "더 가라"는 신호 (`Term '*' Factor` 로)
- 다음이 `+` 또는 `$` → 이 둘은 **FOLLOW(Term)** 에 있음 → "Term 끝났으니 **묶어라(reduce)**"

**바로 이렇게 FOLLOW 가 "언제 묶을지"를 결정해요.**\
FIRST/FOLLOW 가 왜 파싱 테이블의 재료인지, 여기서 드러나요.\
다음 장에서 그 테이블을 직접 만들어 보면 더 또렷해져요.

### 특수 상황 둘 (지금은 가볍게만)

너무 깊이 안 가도 돼요.\
"이런 게 있구나" 정도만.

- **ε(엡실론, 빈 문자열):** 어떤 비단말이 "아무것도 아닌 것"이 될 수 있을 때 쓰는 표시예요.
  우리 예제엔 안 나오니 지금은 가볍게 넘기고, 심화 [FIRST · 계산 규칙](first-rules.md)에서 ε 이 있는 작은 문법으로 직접 보여줄게요.
- **`$`(끝 표시):** 방금 봤듯 입력의 끝을 나타내는 가상의 토큰이에요. 시작 기호의 FOLLOW 엔 항상 들어가요.

## ③ 플레이그라운드에서 보기

FIRST/FOLLOW 자체는 화면에 직접 안 보이지만, **그 결과물인 파싱 테이블의 reduce 칸**으로
효과를 눈으로 볼 수 있어요.\
플레이그라운드에서:

1. 기본 문법과 입력 `a + a * a` 로 **Run**
2. **Parse table** 에서, 다음 토큰이 `+`/`)`/`$`(즉 FOLLOW(Term))일 때 초록색 **reduce** 뱃지가
   뜨는 자리를 확인 — 바로 FOLLOW 가 "여기서 묶어라"라고 시킨 곳이에요.
3. **Step through** 로 한 칸씩 가보면, `a` 뒤 토큰이 `*` 냐 `+` 냐에 따라 행동이 갈리는 게 보여요.

👉 **[라이브 플레이그라운드](https://polite-island-0b2142200.7.azurestaticapps.net)**

> (FIRST/FOLLOW 집합을 *직접* 표로 보여주는 패널은 추가 예정이에요.)

---

## 한 걸음 더 — 심화 과정 (선택)

여기까지가 **개념**이에요.\
그리고 — 솔직히 기본 과정은 **여기까지면 충분해요.**\
다음 기본 장으로
바로 가셔도 돼요.\
다만 더 깊이 파고 싶은 분을 위해 길을 하나 열어둘게요.

> 🎓 방금 우리는 *이 예제 문법* 의 FIRST/FOLLOW 를 손으로 구했죠. 이걸 **어떤 문법에나 통하는
> 공식(알고리즘)** 으로 정리하고, 그게 **Janglim 코드에서 어떻게 구현됐는지**까지 보는 건 —
> **심화 과정**의 [FIRST — 정의와 유도](first-formula.md) (→ 계산 규칙 → 구현) 에서 따로 다뤄요.
>
> **안 봐도 전혀 문제없어요.** 개념은 이미 다 잡으셨으니까요. 🙂

---

## 다음 장

FIRST/FOLLOW 라는 재료가 준비됐어요.\
**여기까지 정말 잘 따라오셨어요 🎉**\
다음은 — 파서가 *"지금 어디까지 읽었는지"* 를 어떻게 기억하는지(**점**), 그리고 *"지금 가능한 것들"* 을
모은 **상태** 예요.\
그게 모이면 드디어 그 유명한 **파싱 테이블** 이 됩니다.

👉 **[점과 상태](dot-and-state.md)**
