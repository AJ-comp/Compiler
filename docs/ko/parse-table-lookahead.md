# lookahead (LR(1) 아이템)

[SLR 장](parse-table-slr.md) 에서 우리는 *"`A → α •` 를 만나면, **어떤 다음 글자** 에 reduce 하나?"* 라는 질문을 `FOLLOW(A)` 로 답했어요. 그런데 [헛충돌](parse-table-slr.md) 에서 봤듯 — `FOLLOW(A)` 는 *문법 전체* 를 뭉뚱그린 거라, *지금 이 자리* 엔 너무 넓었죠.

그래서 한 걸음 더 갑니다 — **아이템 하나하나에, *그 자리에 딱 맞는* 다음 글자를 붙이자.** 이 "아이템에 붙는 다음 글자" 가 바로 **lookahead** 예요.

이 장에서 lookahead 가 *정확히 무엇이고, 어떻게 구하는지* 를 — **빠짐없이** 풀어볼게요. (다음에 올 CLR·LALR 이 전부 이 lookahead 위에 서 있어요.)

---

## 0. 기호 약속부터 — 안 헷갈리게

공식엔 `α` · `β` · `t` 같은 글자가 나와요. 시작 전에 *이게 무슨 글자인지* 부터 분명히 해둘게요. (이걸 안 하면 바로 헷갈려요.)

지금까지 예제의 `a` `b` `c` `d` `x` `y` 는 **문법에 진짜로 박힌 글자(단말)** 였죠. 그런데 *공식* 을 쓸 땐 — "*아무* 규칙, *아무* 글자나" 를 가리키는 **빈칸(자리표시)** 이 필요해요.

| 기호 | 무슨 *빈칸* 인가 | 채워질 수 있는 예 |
|:--:|:--|:--|
| `A` `B` | *비단말* 하나 (규칙 이름) | 예제의 `A` · `B` · `S` … |
| `α` `β` | *기호 묶음* — 단말·비단말이 **0개 이상** 늘어선 것 | 빈 묶음 · `c` · `A c` · `a A` … |
| `t` | *단말* **하나** — **lookahead 자리** | `c` · `d` · 또는 입력 끝 `$` |

> ⚠️ **표기 주의 — lookahead 는 `a` 가 아니라 `t` 로 씁니다.** 교과서는 lookahead 자리를 보통 `a` 로 써요. 그런데 우리 예제엔 *진짜 글자* `a` (예: `S → a A c`) 가 있어서, `a` 가 *진짜 글자* 와 *lookahead 자리* 두 뜻으로 겹쳐요.\
> 그래서 **이 매뉴얼에선 lookahead 자리를 `t`** (terminal = 단말) **로** 쓸게요. **`t` 는 특정 글자가 아니라 *"여기 올 다음 단말 하나"* 라는 빈칸** 이에요 — 거기 실제로 뭐가 들어갈지는 *공식으로 구하는* 거고요.

요점만: `α` `β` `t` = **빈칸**, &nbsp;&nbsp; `a` `b` `c` … = **진짜 글자.**

---

## 1. LR(1) 아이템 — 아이템에 lookahead 를 붙이다

[클로저 장](closure-def.md) 의 아이템은 *규칙 + 점* 이었어요 — 예: `A → α • β` (점 *앞* `α` 는 이미 읽음, 점 *뒤* `β` 는 아직).

여기에 **lookahead 한 글자(`t`)** 를 붙인 게 **LR(1) 아이템** 이에요.

<pre class="lrbox">
[ <span class="nt">A</span> → α <span class="lrdot">•</span> β , <span class="setm">t</span> ]
</pre>

뒤에 붙은 `t` 를 읽는 법은 이래요:

> *"`A → α β` 를 끝까지 읽어 **`A` 로 접고 난 *직후*, 바로 다음 글자가 `t` 면** — 그 접기가 옳다."*

즉 **`t` 는 *이 규칙을 접어도 되는 다음 글자* 예요.** (그래서 "앞보기(lookahead)" 라 불러요.)

**그래서 — "왜 lookahead 가 `t` 냐?"** `t` 는 *글자 이름* 이 아니라, **"접은 뒤에 올 다음 글자가 들어갈 자리"** 라는 빈칸이에요. 그 자리에 *실제로 어떤 글자가 들어가는지* 를 구하는 게 — 바로 다음 §2 고요.

> 💡 **SLR 과 비교:** SLR 은 "`A` 라는 규칙엔 `FOLLOW(A)`" 처럼 *비단말 단위* 로 lookahead 를 뭉쳤어요. LR(1) 아이템은 *아이템 단위* — 똑같은 `A → b •` 라도 *어느 상태에 있느냐* 에 따라 다른 `t` 를 가질 수 있어요. 이 *세밀함* 이 다음 장들의 핵심이에요.

---

## 2. 그 lookahead, 어떻게 구하나 — 클로저에서 FIRST 로

아이템은 *클로저* 로 불어나요([클로저 계산법](closure-calc.md)) — *점 뒤에 비단말이 있으면 그 규칙들을 펼치는* 거였죠. **LR(1) 에선, 펼치면서 새 아이템의 lookahead 까지 정해줘요.**

펼치는 상황은 이래요. 이미 이런 아이템이 있다고 합시다:

<pre class="lrbox">
[ <span class="nt">A</span> → α <span class="lrdot">•</span> <span class="nt">B</span> β , <span class="setm">t</span> ]
</pre>

점 뒤에 비단말 `B` 가 있죠 (그 뒤엔 `β`, 그리고 이 아이템의 lookahead `t`). 클로저는 `B → γ` 규칙들을 새로 넣어요:

<pre class="lrbox">
[ <span class="nt">B</span> → <span class="lrdot">•</span> γ , ? ]      ← 이 새 아이템의 lookahead 자리엔 뭘 넣지?
</pre>

**생각해봐요 — `B` 를 다 접고 나면, 그 *바로 뒤* 엔 뭐가 오죠?**

<pre class="lrbox">
[ <span class="nt">A</span> → α <span class="lrdot">•</span> <span class="nt">B</span> <span class="lrmark">β</span> , <span class="setm">t</span> ]
</pre>

원래 아이템에서 `B` 뒤에 있던 `β` 예요. 그러니 `B` 의 lookahead 는 *`β` 의 첫 글자* 들 (`FIRST(β)`) 이 되겠죠.

그런데 만약 **`β` 가 비었거나 통째로 사라질 수 있으면** (즉 `B` 뒤에 사실상 아무것도 없으면), 그땐 `B` 다음에 오는 건 *원래 아이템의 lookahead `t`* 예요.

> 새 lookahead = <span class="setm">t</span>

**왜 `t` 가 될까요?** 원래 아이템은 `[ A → α • B β , t ]` 인데, `β` 가 사라지면 사실상 `[ A → α • B , t ]` 가 돼요. 이 아이템대로 `A` 를 펼치면 `B` 가 *맨 끝* 에 오게 돼요. `B` 를 접으면 `A` 도 바로 끝나죠. 그럼 `B` 다음에 오는 건 곧 *`A` 다음에 오는 것* 이고, 그게 뭔지는 이 아이템이 이미 말해주고 있어요 — 바로 lookahead `t` 죠. 그래서 `B` 가 그 `t` 를 그대로 물려받아요.

이 두 경우를 한 식에 담은 게 —

> ### 새 lookahead = `FIRST( β t )`

(`FIRST(β t)` = `β` 의 FIRST 를 모으되, `β` 가 전부 사라질 수 있으면 `t` 까지 포함 — [FIRST](first-formula.md) 정의 그대로예요.)

이게 lookahead 를 구하는 **공식의 전부** 예요. *새로운 계산이 아니라*, 이미 배운 **FIRST 를 "점 뒤 `β`" 에 적용** 하는 것뿐이에요.

---

## 3. 손으로 한 번 — 진짜 글자로

빈칸(`α β t`) 말고 *진짜 글자* 로 해봐요. 작은 문법:

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">(</span> <span class="nt">A</span> <span class="setm">)</span>
<span class="nt">A</span> → <span class="setm">n</span>
</pre>

(`(` `)` `n` 은 진짜 단말이에요. `n` 은 숫자나 이름 같은 토큰이라고 보면 돼요.)

시작해서 `(` 를 읽으면 이런 아이템이 있어요 (맨 바깥은 입력 끝 `$` 로 닫히니 lookahead 가 `$`):

<pre class="lrbox">
[ <span class="nt">S</span> → <span class="setm">(</span> <span class="lrdot">•</span> <span class="nt">A</span> <span class="setm">)</span> , <span class="setm">$</span> ]
</pre>

점 뒤가 비단말 `A` 죠. 펼쳐서 `A → n` 을 새로 넣을 차례 — **공식 `FIRST(β t)` 에 대입** 해요:

- 점 뒤 비단말 `B` = `A`
- `A` 다음에 남은 `β` = `)`
- 이 아이템의 lookahead `t` = `$`
- → 새 lookahead = `FIRST( ) $ )` — 그런데 맨 앞 `)` 는 단말이라 사라질 수 없으니, FIRST 는 그 뒤 `$` 까진 닿지도 못해요. 그래서 `$` 는 빠지고 `FIRST( ) )` = <code><span class="setb">{</span><span class="setm"> ) </span><span class="setb">}</span></code> &nbsp; (`)` 는 단말이라 자기 자신)

그래서 새 아이템은:

<pre class="lrbox">
[ <span class="nt">A</span> → <span class="lrdot">•</span> <span class="setm">n</span> , <span class="setm">)</span> ]
</pre>

`n` 을 읽으면 점이 넘어가 — **`[ A → n • , ) ]`.**\
읽는 법: *"`A → n` 을 접은 직후 `)` 가 오면 옳다."* — 당연하죠, `( n )` 에서 `n` 뒤엔 `)` 가 오니까요.

**`β` 가 빈 경우도 한 번 봐요.** 만약 규칙이 `S → ( A )` 가 아니라 그냥 `S → A` 였다면? `[ S → • A , $ ]` 에서 `A` 를 펼칠 때 — `A` 뒤에 남은 게 *없어요* (`β` 가 빔). 그럼 공식이 `FIRST( $ )` = <code><span class="setb">{</span><span class="setm"> $ </span><span class="setb">}</span></code>, 즉 **부모의 lookahead `$` 를 그대로 물려받아** `[ A → • n , $ ]` 가 돼요.

> 🔖 **한 줄 정리** — lookahead 는 *아이템마다 붙는 "접어도 되는 다음 단말"* 이고, 클로저에서 **`FIRST(β t)`** 로 구해요. (점 뒤 `β` 의 FIRST, `β` 가 사라지면 부모 `t` 를 물려받기.)

---

## 다음

이제 lookahead 가 *무엇이고 어떻게 나오는지* 알았어요. 이걸 **상태를 만들 때부터 처음부터 달고** 표를 채우면 — 그게 **CLR** 이에요. 가 봅시다.

👉 **[파싱 테이블 · CLR — 원리](parse-table-clr.md)**
