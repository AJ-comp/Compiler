# 파싱 테이블 · LALR — 원리

[CLR 장](parse-table-clr.md) 에서 봤듯 — CLR 은 *완벽하게 정밀* 하지만 **상태 폭증** 이 비싸요.\
**LALR** 은 그 둘을 화해시킨 *실용형* 이에요 — 정밀도는 CLR 급, 상태 수는 LR(0) 급.

---

## LALR = CLR 을 합치다

아이디어 한 줄: **CLR 의 정밀 lookahead 는 살리되, *겉모습(코어)이 같은 상태들은 도로 하나로 합쳐서* 상태 수를 LR(0) 만큼 줄이자.**

(사실 우리가 [정준 집합](canonical-set.md) 에서 만든 LR(0) 상태가 바로 그 "합쳐진" 코어예요. 그래서 엔진은 CLR 을 통째로 만들지 않고, LR(0) 상태에 lookahead 를 *직접 전파* 해서 같은 결과를 효율적으로 얻어요 — 그 코드는 [구현](parse-table-lalr-impl.md) 에서.)

> 🔖 **LALR (Look-Ahead LR)** — CLR 의 상태 중 *겉모습(코어)이 같은 것들을 합쳐*, LR(1) 급 정밀도를 LR(0) 상태 수로 내는 방식.

### 합치기를 눈으로 보기 — 실제 상태와 표

작은 문법으로 *상태를 실제로 만들어* 봅시다.

<pre class="lrbox">
1:  <span class="nt">S</span> → <span class="setm">b</span> <span class="nt">A</span> <span class="setm">x</span>
2:  <span class="nt">S</span> → <span class="setm">d</span> <span class="nt">A</span> <span class="setm">y</span>
3:  <span class="nt">A</span> → <span class="setm">c</span>
</pre>

`A → c` 는 `c` 를 읽으면 끝나는(완료) 규칙이에요. 그런데 이 규칙에 닿는 길이 *둘* 이에요 — `b` 를 거친 길과 `d` 를 거친 길.

<div class="ex-card">

**① `CLR` — 상태가 둘로 갈려요**

**CLR 로 만들면 — `A → c •` 상태가 *둘* 로 갈려요.** (lookahead 가 길마다 달라서요.)

<pre class="lrbox">
상태 5a :  <span class="nt">A</span> → <span class="setm">c</span> <span class="lrdot">•</span>   lookahead { <span class="setm">x</span> }    <span style="opacity:.65">b c 로 도착 — b A x 의 A 뒤는 x</span>
상태 5b :  <span class="nt">A</span> → <span class="setm">c</span> <span class="lrdot">•</span>   lookahead { <span class="setm">y</span> }    <span style="opacity:.65">d c 로 도착 — d A y 의 A 뒤는 y</span>
</pre>

그래서 CLR 상태는 전부 **10개** — `0, 1, 2, 3, 4,` **`5a, 5b`** `, 6, 7, 8`.

</div>

<div class="ex-card">

**② `LALR` — 코어가 같으니 합쳐요**

**LALR — `5a` 와 `5b` 는 아이템이 `A → c •` 로 똑같아요(코어가 같음).** 그래서 *한 상태로 합치고*, lookahead 는 합집합으로 묶어요.

<pre class="lrbox">
상태 5 :  <span class="nt">A</span> → <span class="setm">c</span> <span class="lrdot">•</span>   lookahead { <span class="setm">x</span> , <span class="setm">y</span> }
</pre>

`5a` 와 `5b` 가 사라지고, 그 자리에 **상태 5 하나.** 전체 **10개 → 9개** 가 됐어요.

</div>

<div class="ex-card">

**③ `파싱 표` — 합치기가 표에 박힌 모습**

**그 결과, 실제 LALR 파싱 표는 이렇게 나와요.**\
(`sN` = 상태 N 으로 이동(shift), `rN` = 규칙 N 으로 접기(reduce), `acc` = 수락, 빈칸 = 오류.)

| 상태 | `b` | `d` | `c` | `x` | `y` | `$` | **S** | **A** |
|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|:--:|
| **0** | s2 | s3 | | | | | 1 | |
| **1** | | | | | | acc | | |
| **2** | | | s**5** | | | | | 4 |
| **3** | | | s**5** | | | | | 6 |
| **4** | | | | s7 | | | | |
| **5** | | | | **r3** | **r3** | | | |
| **6** | | | | | s8 | | | |
| **7** | | | | | | r1 | | |
| **8** | | | | | | r2 | | |

합치기가 표에 *실제로 박힌 자리* 는 둘이에요.

1. **상태 2 와 상태 3 이 `c` 칸에서 *둘 다 상태 5* 로** 가요 (`s5`). CLR 이었다면 2는 `5a` 로, 3은 `5b` 로 *서로 다른* 상태로 갈렸을 자리예요.
2. **상태 5 한 줄** 이 `x`·`y` 둘 다 `r3` (= `A→c` 접기). CLR 이었다면 `5a` 는 `x` 칸만, `5b` 는 `y` 칸만 채운 *두 줄* 이던 게 — 한 줄로 포개졌어요.

두 칸 다 동작이 하나씩(`r3` 하나)이라 — **충돌은 없어요.**

> *틀린 입력은?* `b c y` 를 넣어보면: `b` → 상태 2, `c` → 상태 5. 거기서 `y` 를 보고 `r3` 으로 `A` 로 접어요 (`y` 가 lookahead `{ x , y }` 에 들었으니). 그리고 바로 다음 상태 4 (`S → b A • x`) 에서 `x` 를 기다리는데 `y` 가 왔으니 → **오류.** 틀린 입력은 *한 칸 늦게라도* 똑같이 걸러내요.

</div>

---

(참고: [CLR 장](parse-table-clr.md) 의 a/b 문법에선 `A → b •` 를 담은 두 상태(`a b`, `e b`)의 *코어가 서로 달라서* — 합쳐질 짝이 없어요. LALR 은 거기선 CLR 의 `{ c }` 를 그대로 써서, 똑같이 헛충돌이 안 생기고요.)

→ **정밀도는 CLR 급, 상태 수는 LR(0) 급.** yacc·bison, 그리고 **우리 엔진의 실작동 파서가 다 LALR** 인 이유예요.

---

## 다만 — 합치기가 *드물게* 충돌을 되살려요

거의 항상 합치기는 무해해요. 그런데 **아주 드물게**, 합치는 순간 *없던 충돌이 살아나기도* 해요. (조금 빡빡하지만 한 번만 따라오면 돼요.)

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">b</span> <span class="nt">B</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">e</span>
<span class="nt">S</span> → <span class="setm">b</span> <span class="nt">A</span> <span class="setm">e</span>
<span class="nt">A</span> → <span class="setm">c</span>
<span class="nt">B</span> → <span class="setm">c</span>
</pre>

`c` 를 막 읽은 상태가 *두 군데* 나와요. 아이템은 `{ A→c•, B→c• }` 로 같은데, *어디서 왔느냐* 에 따라 lookahead 가 엇갈려요.

| `c` 를 읽고 도착한 상태 | `A → c •` | `B → c •` |
|:--|:--:|:--:|
| `a c` 뒤 | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> | <code><span class="setb">{</span><span class="setm"> e </span><span class="setb">}</span></code> |
| `b c` 뒤 | <code><span class="setb">{</span><span class="setm"> e </span><span class="setb">}</span></code> | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> |

- **CLR** 은 — 둘을 *따로* 둬요. 각 상태 안에서 안 겹쳐요. → 충돌 없음.
- **LALR** 은 — 코어가 같으니 *합쳐요.* 그럼 lookahead 가 합집합이 돼 둘 다 <code><span class="setb">{</span><span class="setm"> d e </span><span class="setb">}</span></code> → `d`(와 `e`) 에서 A·B 둘 다 reduce → **reduce/reduce 충돌!** *합치기 전엔 없던* 게 살아났죠.

> 즉 이 문법은 *LR(1)(CLR)으론 풀리는데 LALR 로는 충돌* 나는 드문 경우예요. 하지만 실무 문법엔 이런 엇갈림이 거의 없어서, LALR 의 합치기는 대부분 *공짜로 상태만 아껴줘요.*

> 📎 **합치기로 새로 생기는 충돌은 *reduce/reduce 뿐* 이에요** — *shift/reduce 충돌* 은 합치기로 절대 안 생겨요. (CLR 이 충돌 없으면, LALR 이 더할 수 있는 건 r/r 충돌뿐이라는 게 증명돼 있어요. 그래서 위 예제도 r/r 였죠.)

> 💡 **여기서 한 걸음 더** — *"그럼 합쳐도 충돌 안 나는 것만 골라 합치면? LR(1) 정밀도는 지키면서 상태 수는 거의 LALR 만큼 아닌가?"* — 맞아요. 그게 **minimal LR(1)**(Pager, 1977) 과 그 현대판 **IELR(1)** 이에요. (GNU Bison 도 `%define lr.type ielr` 로 지원.) 우리 엔진은 아직 LALR 까지라, 이건 *잠재적 미래 개선* 거리예요.

---

## 정밀도 사다리 — 한눈에

| 방식 | reduce 를 적는 글자 | 정밀도 | 상태 수 | 헛충돌 |
|:--|:--|:--:|:--:|:--:|
| **LR(0)** | 모든 단말 | 최저 | 적음 | 우글우글 |
| **SLR** | FOLLOW(A) | 중 | 적음 | 가끔 |
| **LALR** | 코어별 정밀 lookahead *(CLR 을 합친 것)* | 높음 | 적음 (= LR(0)) | 거의 없음 |
| **CLR / LR(1)** | 문맥별 lookahead *(상태를 쪼갬)* | 최고 | **폭증** | 없음 |

위로 갈수록 정밀해지지만, 마지막 한 칸(CLR)에서 *상태 수* 라는 비싼 값을 치러요.\
그래서 **LALR 이 — 정밀도와 상태 수, 두 마리 토끼를 동시에 잡은 자리** 인 거예요.

---

## 다음

원리는 여기까지예요. 그럼 *우리 엔진* 은 이 "합치기(= 전파)" 를 코드로 어떻게 할까요?

👉 **[파싱 테이블 · LALR — 구현](parse-table-lalr-impl.md)**
