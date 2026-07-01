# 파싱 테이블 · SLR과 헛충돌

지난 장에서 충돌을 만났어요. 그리고 마지막에 이런 말을 흘려뒀죠 —

> 충돌엔 *진짜* 도 있지만, 표를 **덜 정밀하게 채워서 생긴 가짜** 도 있다.

이 장에서 그 **"정밀도"** 이야기를 시작해요. 그런데 신기하게도 — 이게 전부 **딱 한 가지 질문** 에서 갈라져 나와요.

---

## 모든 건 이 질문 하나에서

충돌은 거의 항상 **완료 아이템** 때문에 생겨요. 상태 안에 `A → α •` 처럼 점이 끝까지 간 아이템이 있으면 *"이제 reduce 해"* 라는 뜻이죠. 그런데 —

> **"`A → α •` 를 만났을 때, *어떤 다음 글자* 에 reduce 를 적어야 하나?"**

바로 이 질문이에요.\
이 답을 *얼마나 정밀하게* 하느냐가 — 충돌이 나느냐 마느냐를 가르고, 앞으로 나올 **SLR · CLR · LALR** 이라는 이름들이 사실은 전부 *이 한 질문에 대한 서로 다른 답* 이에요.

> 🔖 **lookahead(룩어헤드)** — 파서가 지금 결정을 내리려고 *흘끔 보는 다음 글자 한 개*. ([클로저](closure-def.md) 장에서 한 번 스쳤던 그 단어예요.)

자, 정밀도를 한 칸씩 올려가 봅시다.

---

## ① 가장 거친 답 — "아무 글자에나" (LR(0))

제일 단순한 답.\
`A → α •` 면, *다음 글자가 무엇이든* reduce.

이게 우리가 [정준 집합](canonical-set.md) 까지 만든 **LR(0) 상태** 그대로의 답이에요 — 그 상태들엔 *다음 글자에 대한 정보가 아예 없으니까요.*

문제는 뻔해요. reduce 를 너무 막 적어서, 같은 상태에 shift 아이템이 조금이라도 있으면 *곧장* 부딪혀요. 웬만한 실제 문법은 이 단계에서 충돌 천지가 돼요.\
→ 더 정밀해야 합니다.

---

## ② 한 칸 위 — "FOLLOW 에서만" (SLR)

똑똑한 첫걸음.\
`A → α •` 를 아무데서나 말고 — **A 뒤에 올 수 있는 글자들에서만** reduce 하는 거예요. 그 *"A 뒤에 올 수 있는 글자들"* 이 바로 [FOLLOW(A)](follow-formula.md) 였죠.

이게 [만드는 법](parse-table-build.md) 페이지에서 우리가 실제로 썼던 방식이고, 이름이 **SLR** 이에요.

> 🔖 **SLR (Simple LR)** — LR(0) 상태는 그대로 두되, reduce 는 **FOLLOW(A) 에 든 글자에서만** 적는 방식. ("Simple" = 가장 손쉬운 한 단계 개선이라는 뜻.)

**그냥 넘어가지 말고, 눈으로 한 번 봅시다.** 우리 Expr 문법으로 상태를 만들다 보면 — *Term 을 막 읽은 직후* 이런 상태가 하나 나와요.

<pre class="lrbox">
<span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← 완료. Expr→Term 으로 접을까?</span>
<span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    <span style="opacity:.65">← 진행. '*' 를 보면 shift</span>
</pre>

이 상태에서 **다음 글자가 `*`** 일 때 — 정할 건 딱 하나예요.\
*위의 `Expr → Term •` 를 지금 접을까(reduce), 말까?*

두 방식이 이 질문에 **다르게** 답해요.

**LR(0) 의 답 — "무조건 접어."**\
완료 아이템(`Expr → Term •`)은 *다음 글자를 아예 안 봐요.* 무슨 글자가 오든 그냥 "접어" 예요.\
그러니 `*` 에서도 "Expr→Term 으로 접어" 를 적어버리죠. 그런데 바로 아래 아이템(`Term → Term • '*' Factor`)은 `*` 에서 *shift* 하라고 해요.\
→ 한 칸에 "접어" 와 "더 읽어" 가 같이 → **충돌!** ⚠️

**SLR 의 답 — "`Expr` 뒤에 올 수 있는 글자일 때만 접어."**

그런데 — *왜* 하필 "올 수 있는 글자일 때만" 일까요? 여기에 논리가 있어요.

`Expr → Term •` 를 접는다는 건, 방금 만든 Term 을 **`Expr` 한 덩이로 묶어 스택에 올린다** 는 뜻이에요. 그러고 나면 파서는 *그 `Expr` 바로 다음에* 입력의 다음 글자를 이어 보게 되죠.

그러니 접어도 되려면 — **그 다음 글자가, `Expr` 뒤에 *진짜로 올 수 있는* 글자여야** 해요.\
만약 `Expr` 뒤에 *절대 올 수 없는* 글자라면? 그걸 접는 순간, **"`Expr` 다음에 그 글자"** 라는 — *문법 어디에도 없는 모양* 을 만들어버리는 거예요. 그건 곧 *"여기서 접으면 틀린다"* 는 뜻이죠.

그리고 **"`Expr` 뒤에 올 수 있는 글자"** — 이게 바로 [FOLLOW](follow-formula.md) 의 **정의** 였어요.

> 🔖 **`FOLLOW(Expr)`** = `Expr` 바로 뒤에 올 수 있는 단말들의 집합. *(정의 그대로.)*

그래서 SLR 의 규칙은 *정의에서 곧장 따라 나와요* — **다음 글자가 `FOLLOW(Expr)` 안에 있을 때만 접는다.**

**자, 그럼 `*` 는?** 정의대로 `FOLLOW(Expr)` 를 *직접 구해* 봅시다.\
FOLLOW 를 구하는 법은 — 문법에서 **`Expr` 가 등장하는 자리마다, 그 *바로 뒤* 에 무엇이 오는지** 모으는 거였죠. `Expr` 가 오른쪽에 나오는 규칙을 빠짐없이 찾아보면:

| `Expr` 가 나오는 규칙 | `Expr` 바로 뒤에 오는 것 |
|:--|:--:|
| `Expr → Expr '+' Term` | `+` |
| `Factor → '(' Expr ')'` | `)` |
| `Accept → Expr` (증대 규칙) | 입력 끝 `$` |

이게 *전부* 예요. 그래서 — <code>FOLLOW(Expr) = <span class="setb">{</span><span class="setm"> + ) $ </span><span class="setb">}</span></code> 예요.\
(<span class="setb">{ }</span> 는 *집합* 을 뜻하는 기호일 뿐이고, 안에 든 `+` · `)` · `$` 가 실제 원소예요.)

여기서 결정적인 한 가지가 보여요 — **어느 규칙에서도 `Expr` 뒤에 `*` 가 오지 않아요.**\
그럴 수밖에 없어요. `*` 는 문법에서 오직 `Term → Term '*' Factor` **한 곳에만** 나오거든요. 즉 `*` 는 언제나 *`Term` 바로 뒤* 자리지, *`Expr` 뒤* 자리가 **아니에요.**

이제 논리가 닫혀요:

> `*` 는 `FOLLOW(Expr)` 에 없다 (= `Expr` 뒤엔 `*` 가 올 수 없다).\
> 그러니 `*` 에서 `Expr → Term •` 를 접으면 *"`Expr` 다음에 `*`"* 라는 **문법에 없는 모양** 이 된다.\
> 따라서 SLR 은 `*` 에선 **접지 않는다.**

reduce 를 안 적으니 그 칸엔 *shift 하나만* 남고 → **충돌이 사라져요.** ✅

| 방식 | `*` 에서 하는 일 |
|:--|:--|
| **LR(0)** | 다음 글자 안 보고 *무조건* 접기 + shift → ⚠️ 충돌 |
| **SLR** | `*` 는 Expr 뒤에 못 오니 *안 접음* → shift 만 → ✅ 깔끔 |

**한 줄 직관:** `a * …` 를 읽는 중이면 — 당연히 *곱셈을 더 받아야지*, 거기서 통째로 `Expr` 로 접으면 안 되잖아요? SLR 은 *"`*` 는 `Expr` 뒤에 올 수 없는 글자"* 라는 사실 하나로, 바로 그 **"여기선 접지 마"** 를 정확히 알아챈 거예요.

이렇게 — *"이 글자가 Expr 뒤에 올 수 있나"* 만 한 번 따진 덕분에, 우리 Expr 문법은 SLR 로 충돌이 **0** 이 돼요. [만드는 법](parse-table-build.md) 의 표가 그렇게 깔끔했던 게 바로 이 덕분이고요.\
SLR 만으로도 꽤 많은 문법이 잘 풀려요. 그런데 —

---

## ③ SLR의 약점 — 헛충돌

`FOLLOW(A)` 가 뭐였는지 다시 떠올려봐요.\
그건 **문법 전체** 를 뒤져서, A가 *어디에 나타나든* 그 뒤에 올 수 있는 글자를 *모조리* 모은 집합이에요. **지금 이 상태와는 상관없이.**

바로 여기 함정이 있어요.

지금 파서가 서 있는 **이 상태** 는, 문법 속의 *특정한 한 자리* 예요.\
그 자리에서 A 뒤에 *실제로* 올 수 있는 글자는 — FOLLOW(A) **전체가 아니라 그중 일부** 일 때가 많아요.

그런데 SLR 은 *전체* FOLLOW(A) 를 가져다 reduce 를 적어요. 그러다 보면 —\
**이 상태에선 절대 오지 않을 글자** 에까지 reduce 를 적어버리고, 그게 옆에 있던 shift 와 부딪혀요.\
문법이 *진짜로* 모호한 게 아닌데, 단지 *너무 넓게 적어서* 생긴 충돌 — 이걸 **헛충돌(spurious conflict)** 이라고 불러요.

> 🔖 **헛충돌(spurious conflict)** — 문법이 실제로 모호한 게 아니라, reduce 를 *필요 이상으로 넓은 글자* 에 적어서 생긴 *가짜* 충돌.

말이 좀 추상적이죠. **작은 예제로 직접 봅시다.**

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">c</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">e</span> <span class="nt">A</span> <span class="setm">d</span>
<span class="nt">A</span> → <span class="setm">b</span>
<span class="nt">B</span> → <span class="setm">b</span>
</pre>

`A` 도 `b` 로, `B` 도 `b` 로 펼쳐지는 — 일부러 살짝 헷갈리게 만든 문법이에요. (그래도 *모호하진* 않아요. 앞뒤 글자로 충분히 갈리거든요.)

먼저 `FOLLOW(A)` 를 구해요. `A` 는 **두 군데** 나와요 — `a A c`(뒤에 `c`) 와 `e A d`(뒤에 `d`). 그래서 <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> 예요. (`B` 는 `a B d` 한 곳뿐이라 <code>FOLLOW(B) = <span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code>.)

이제 파서가 **`a b` 까지 읽은 상태** 를 봐요. 이 상태엔 완료 아이템이 둘 들어 있어요.

<pre class="lrbox">
<span class="nt">A</span> → <span class="setm">b</span> <span class="lrdot">•</span>      <span style="opacity:.65">← 완료</span>
<span class="nt">B</span> → <span class="setm">b</span> <span class="lrdot">•</span>      <span style="opacity:.65">← 완료</span>
</pre>

SLR 로 reduce 칸을 채우면:

- `A → b •` 는 <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> 에서 reduce
- `B → b •` 는 <code>FOLLOW(B) = <span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> 에서 reduce
- → **`d` 에서 둘 다 reduce!** reduce/reduce 충돌 ⚠️

**그런데 이게 진짜 충돌일까요?** `a b` 다음을 따져봐요.\
우린 `a` 로 시작했으니 — 갈 수 있는 길은 `a A c` 아니면 `a B d`, 둘뿐이에요.

- 다음이 `c` 면 → `a A c` 길 → `A` 로 접는 게 맞고
- 다음이 `d` 면 → `a B d` 길 → `B` 로 접는 게 맞아요

즉 **`d` 에선 무조건 `B` 예요. 여기서 `A` 로 접을 일은 *절대* 없어요.**

그럼 SLR 은 왜 `d` 칸에 `A → b` reduce 를 적었을까요?\
`A → b` 의 reduce 를 <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> *전체* 로 적었기 때문이에요. 그리고 그 `d` 는 — *지금 이 `a` 길* 이 아니라, **저 멀리 `e A d` 규칙** 에서 흘러든 거예요. `e A d` 에선 `A` 뒤에 `d` 가 오지만, *지금 우리가 서 있는 `a` 길* 에선 `A` 뒤엔 `c` 만 오거든요.

**이게 바로 헛충돌이에요.** 문법이 모호한 게 아니라 — *전혀 다른 자리(`e A d`)의 사정* 까지 `FOLLOW` 에 뭉뚱그려 담기는 바람에, *지금 자리엔 없어도 될 reduce* 가 적힌 거죠.

> 이건 [충돌이란?](parse-table-conflict.md) 장의 *진짜* reduce/reduce 와는 달라요. 거긴 문법이 *정말로* 모호했지만(같은 `a` 를 A 로도 B 로도 볼 수 있었죠), 여긴 모호하지 않은데 SLR 이 *헛충돌* 을 본 거예요. — 바로 **사다리 윗 칸(CLR)이 정확히 고치는 지점** 이고요. (CLR 장에서 *바로 이 문법* 을 다시 펴 보여드릴게요.)

비유로 한 번 더 —\
`FOLLOW(A)` 는 *"A 라는 사람이 살면서 한 번이라도 마주친 모든 사람 명단"* 같은 거예요.\
그런데 지금 이 상태는 *"오늘, 이 방"* 이고요.\
오늘 이 방에 올 사람은 그 명단의 일부뿐인데 — *명단 전체* 를 보고 "이 사람도 올 수 있으니 자리 비워둬" 하니까, 정작 오지도 않을 사람 자리 때문에 *엉뚱하게 부딪히는* 거예요.

---

## 그래서 — 다음

이 헛충돌은 — FOLLOW *전체* 말고 **"바로 이 상태에 진짜로 도달하는 글자"** 에만 reduce 하면 사라져요. 그 *"이 자리에 딱 맞는 글자"* 가 바로 다음 장의 **lookahead** 고 — 그걸 상태에 처음부터 붙이는 게 **CLR**, 합쳐서 줄인 게 **LALR** 이에요. 차근차근 올라갑니다.

그 전에 — 방금 배운 SLR 이 *엔진 코드* 로는 어떻게 생겼는지, 딱 한 페이지만 보고 가요. (말로 한 정의가 코드에 그대로 반영돼 있어서, 한 번 보면 더 또렷해져요.)

👉 **[파싱 테이블 · SLR — 구현](parse-table-slr-impl.md)**
