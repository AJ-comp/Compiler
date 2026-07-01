# 틀린 입력은 어떻게 걸러질까

[표로 실제 파싱](parsing-in-action.md)에서 `a + a * a` 가 **끝까지 성공** 하는 걸 봤어요. 그럼 약속했던
반대쪽 — *틀린* 입력은 어떻게 될까요? 그때 흘렸던 바로 그 예, **`a + + a`**(`+` 가 둘 연달아)를 *같은
표* 로 따라가 보면 답이 바로 나와요.

먼저 예제 문법을 다시 꺼내 놓을게요. (매뉴얼 내내 쓰는 그 산술식이에요.)

<pre class="lrbox">
<span class="nt">Expr</span>   : <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> | <span class="nt">Term</span> ;
<span class="nt">Term</span>   : <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span> | <span class="nt">Factor</span> ;
<span class="nt">Factor</span> : <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span> | <span class="setm">id</span> ;
</pre>

## `a + + a` 를 따라가 보기

[성공했을 때](parsing-in-action.md)와 똑같이 한 줄씩 가봐요. `a` 는 이름이라 `id` 고, 입력 끝은 `$` 예요.

| # | 스택 | 남은 입력 | 행동 |
|:--:|:--|:--|:--|
| 1 | (비었음) | `a + + a $` | `a` 를 **shift** |
| 2 | `a` | `+ + a $` | **reduce** `Factor → id` |
| 3 | `Factor` | `+ + a $` | **reduce** `Term → Factor` |
| 4 | `Term` | `+ + a $` | **reduce** `Expr → Term` |
| 5 | `Expr` | `+ + a $` | `+` 를 **shift** |
| 6 | `Expr +` | `+ a $` | ⛔ 표의 칸이 **비었음** — 할 수 있는 게 없어요 |

5번까지는 [성공했을 때](parsing-in-action.md)와 한 글자도 다르지 않아요 — 첫 `a` 가 `Expr` 로 묶이고,
`+` 를 스택에 밀어넣었죠. 갈림길은 **6번** 이에요. 스택이 `Expr +` 인 채로 다음 토큰이 *또* `+` 인데,
표에서 *"`Expr +` 상태에서 다음이 `+` 라면?"* 칸을 찾아보면 그 자리가 **비어 있어요.**

## 빈 칸이 곧 문법 오류예요

[파싱 테이블](parse-table-build.md)의 칸은 *"이 상태에서 이 토큰을 보면 이렇게 해라"* 를 적어둔
자리였죠. 그러니 칸이 **비어 있다** 는 건 *"이 상태에서 이 토큰으로는 갈 길이 없다"* 는 뜻이고, 그게
바로 **문법 오류** 예요.

여기서 중요한 게 하나 있어요 — 파서는 틀린 곳을 *따로 찾아다니지* 않아요. 그냥 표를 따라 걷다가
**빈 칸에 발이 빠지는 순간, 그 자리가 오류** 인 거예요. 맞는 입력이면 `accept` 까지 길이 이어지고, 틀린
입력이면 어딘가에서 길이 끊기니까요. 표 한 장이 맞고 틀림을 함께 가려주는 셈이에요. 이럴 때 [빠른
시작](getting-started.md)에서 봤던 `result.Success` 가 `False` 로 나와요.

## 그럼 뭐가 *왔어야* 했을까

파서는 *"틀렸어"* 에서 멈추지 않고, **뭐가 왔어야 했는지** 까지 알아요.

`Expr +` 다음에 와야 하는 건 *새로운 `Term`* 이에요. 그리고 문법을 보면 `Term` 은 결국 `Factor` 로
시작하고, `Factor` 는 **이름 `id` 아니면 여는 괄호 `(`** 로 시작하죠. 그러니 `Expr +` 자리에서 표에
*채워져 있는* 칸은 딱 그 둘, `id` 와 `(` 뿐이고 나머지는 다 비어 있어요.

> 💡 그 자리에서 **채워져 있는 칸들** 이 곧 *"여기 **올 수 있는 토큰**"* 이에요.
> 우리 경우엔 `{ id, '(' }` 죠. 그래서 파서는 이렇게 짚어줄 수 있어요 — *"여기엔 `id` 나 `(` 가 와야
> 하는데 `+` 가 왔어요."*

빈 칸이 *"여기서 틀렸다"* 를 알려준다면, 같은 줄의 **채워진 칸들** 은 *"대신 뭐가 맞았을지"* 를
알려줘요. 표의 한 줄이 이 둘을 함께 쥐고 있는 거예요.

## 파서는 그다음 무엇을 할까

빈 칸을 만나면 파서는 **어느 토큰에서 막혔는지**(위치)를 담아 오류를 하나 남기고, 거기서 멈춰요.
`a + + a` 라면 *두 번째* `+` 를 가리키죠. 이 위치 정보가 있어야 편집기가 바로 그 자리에 빨간 줄을 그을
수 있고요.

> 🌿 [플레이그라운드](https://polite-island-0b2142200.7.azurestaticapps.net)에 `a + + a` 를 넣어보면,
> `a + a * a` 와 달리 끝까지 가지 못하고 막히는 걸 눈으로 볼 수 있어요.

## 기본 과정 완주를 축하해요 🎊

[문법 읽는 법](grammar-reading.md) → [FIRST / FOLLOW](first-follow.md) → [점·상태](dot-and-state.md) →
[표로 실제 파싱](parsing-in-action.md) → *틀린 입력 걸러내기* 까지 — **LR 파싱의 큰 그림** 을 다
잡으셨어요. 한 줄로 줄이면 이거예요: *읽고(shift), 맞으면 묶고(reduce), 표가 시키는 대로 `accept` 까지,
그리고 빈 칸을 만나면 오류.*

> 🎓 *표를 직접 만드는 법* 과 *Janglim 코드* 가 궁금하면 **심화 과정** 으로 가요.\
> [LR 아이템](lr-item.md) → [상태](lr-state.md) → [클로저](closure-def.md) → [GOTO](goto.md) →
> [정준 집합](canonical-set.md) → [파싱 테이블](parse-table-build.md) 순서로, 방금 본 걸 *직접 만들어*
> 봐요. (안 봐도 개념은 이미 다 잡으셨어요. 🙂)

---

👈 이전 장: [표로 실제 파싱](parsing-in-action.md)
