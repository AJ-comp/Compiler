# 정준 집합 — 모든 상태 (I₀ ~ I₁₁)

> 🎓 **심화 과정** 이에요.\
> [클로저](closure-def.md)로 한 상태를 *빠짐없이 채우고*, [GOTO](goto.md)로 *기호 하나를 읽어 다음
> 상태로* 갔죠.\
> 이 둘을 — 시작 상태 `I₀` 에서 출발해 **더 새 상태가 안 나올 때까지** — 반복하면, *도달 가능한 모든
> 상태* 가 모여요. 그게 **정준 집합(canonical collection)** 이에요.

> 📍 **사는 곳** · `CanonicalRelation.Calculate` · `…/Parsers/Collections/CanonicalRelation.cs`

우리 예제 문법(증대된 것)으로 끝까지 돌리면 상태가 **딱 12개 — `I₀` ~ `I₁₁`** 나와요.\
아래에 전부 적어둘게요. 각 상태마다 *아이템들* 과, 거기서 기호를 읽어 가는 *전이(GOTO)* 를 같이 뒀어요.\
(점이 맨 끝까지 간 **완료(reduce) 아이템** 은 `← 완료` 로 표시했어요.)

증대 문법은 이거예요.

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>   |  <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>  |  <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     |  <span class="setm">id</span></pre>

---

## `I₀` — 시작 상태

`Accept → • Expr` 에서 클로저한 상태예요. ([계산법](closure-calc.md)에서 만든 그 7개.)

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

**전이:**

- `Expr` 를 읽으면 → `I₁`
- `Term` 을 읽으면 → `I₂`
- `Factor` 를 읽으면 → `I₃`
- `'('` 를 읽으면 → `I₄`
- `id` 를 읽으면 → `I₅`

## `I₁` — `GOTO(I₀, Expr)`

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="nt">Expr</span> <span class="lrdot">•</span>              <span style="opacity:.65">← 완료 (입력 끝 $ 에서 accept)</span>
   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">'+'</span> <span class="nt">Term</span></pre>

**전이:** `'+'` 를 읽으면 → `I₆`

## `I₂` — `GOTO(I₀, Term)`

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>               <span style="opacity:.65">← 완료 (reduce: Expr → Term)</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

**전이:** `'*'` 를 읽으면 → `I₇`

## `I₃` — `GOTO(I₀, Factor)`

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Factor</span> <span class="lrdot">•</span>             <span style="opacity:.65">← 완료 (reduce: Term → Factor)</span></pre>

**전이:** 없음 (완료 아이템만 있는 상태)

## `I₄` — `GOTO(I₀, '(')`

`'('` 를 읽고 점을 옮긴 `Factor → '(' • Expr ')'` 에, `Expr` 가 점 뒤라 다시 클로저가 붙은 7개예요.

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

**전이:**

- `Expr` 를 읽으면 → `I₈`
- `Term` 을 읽으면 → `I₂`
- `Factor` 를 읽으면 → `I₃`
- `'('` 를 읽으면 → `I₄`
- `id` 를 읽으면 → `I₅`

## `I₅` — `GOTO(I₀, id)`

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">id</span> <span class="lrdot">•</span>               <span style="opacity:.65">← 완료 (reduce: Factor → id)</span></pre>

**전이:** 없음 (완료 아이템만 있는 상태)

## `I₆` — `GOTO(I₁, '+')`

<pre class="lrbox">   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

**전이:**

- `Term` 을 읽으면 → `I₉`
- `Factor` 를 읽으면 → `I₃`
- `'('` 를 읽으면 → `I₄`
- `id` 를 읽으면 → `I₅`

## `I₇` — `GOTO(I₂, '*')`

<pre class="lrbox">   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

**전이:**

- `Factor` 를 읽으면 → `I₁₀`
- `'('` 를 읽으면 → `I₄`
- `id` 를 읽으면 → `I₅`

## `I₈` — `GOTO(I₄, Expr)`

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">')'</span>
   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">'+'</span> <span class="nt">Term</span></pre>

**전이:**

- `')'` 를 읽으면 → `I₁₁`
- `'+'` 를 읽으면 → `I₆`

## `I₉` — `GOTO(I₆, Term)`

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> <span class="lrdot">•</span>      <span style="opacity:.65">← 완료 (reduce: Expr → Expr '+' Term)</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

**전이:** `'*'` 를 읽으면 → `I₇`

## `I₁₀` — `GOTO(I₇, Factor)`

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> <span class="lrdot">•</span>    <span style="opacity:.65">← 완료 (reduce: Term → Term '*' Factor)</span></pre>

**전이:** 없음 (완료 아이템만 있는 상태)

## `I₁₁` — `GOTO(I₈, ')')`

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> <span class="lrdot">•</span>     <span style="opacity:.65">← 완료 (reduce: Factor → '(' Expr ')')</span></pre>

**전이:** 없음 (완료 아이템만 있는 상태)

---

## 전이 한눈에 보기

위 전이들을 한 표로 모으면 이래요. 빈 칸은 *그 기호로는 갈 곳이 없다* 는 뜻이에요.

| 상태 | `Expr` | `Term` | `Factor` | `'+'` | `'*'` | `'('` | `')'` | `id` |
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

## 다음 장

상태도, 상태 사이의 전이도 모두 모았어요 — 정준 집합 완성이에요.

이제 남은 건 이걸 **표 한 장** 으로 바꾸는 일이에요. 위 *전이표* 는 그대로 파서의 **shift / goto** 가
되고, 각 상태의 **완료(reduce) 아이템** 은 *"언제 묶을지"* 가 돼요. 이 둘을 합치면 — 다음 장의 **파싱
테이블** 이에요.

👉 **[파싱 테이블 · 만드는 법](parse-table-build.md)**

---

👈 앞으로: [GOTO](goto.md)
