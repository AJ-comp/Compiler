# 클로저 · 정의

> 🎓 **심화 과정** 이에요.\
> 앞 [상태](lr-state.md) 장에서 — 상태가 *LR 아이템들의 집합 `Iₓ`* 이라고 봤죠.\
> 그런데 상태를 *제대로* 만들려면, 아이템들을 **클로저** 로 부풀려야 해요.\
> 클로저도 [FIRST/FOLLOW](first-formula.md) 처럼 **정의 · 계산법 · 구현** 으로 나눠 봐요 — 이 페이지는
> 그 첫걸음, **정의** 예요.

> 📍 **사는 곳** · `Analyzer.Closure` · `…/Parsers/Analyzer.cs`

클로저는 **상태** — 앞 [상태](lr-state.md) 장에서 본, *LR 아이템들의 집합 `Iₓ`* — 위에서 동작하는
연산이에요. 정확히는, 상태를 **완전하게 채우는** 일을 해요. 무슨 말인지 보죠.

## 왜 "부풀려야" 하나

어떤 상태가 아이템 `Expr → • Term` 하나를 품고 있다고 해봐요.\
점 바로 뒤가 **비단말 `Term`** 이죠. 즉 이제 막 `Term` 을 읽으려는 참이에요.

그런데 `Term` 을 읽는다는 건 — 결국 `Term` 의 *생성규칙 중 하나* 를 시작한다는 뜻이에요.\
`Term → Term '*' Factor` 일 수도, `Term → Factor` 일 수도 있죠.\
그러니 **그 생성규칙들도 "곧 시작할 수 있는" 상태** 로, 이 상태에 같이 들어 있어야 해요.

`Expr → • Term` 하나만 덩그러니 있으면 — *Term 을 어떻게 시작하는지* 가 빠진, **불완전한 상태** 예요.\
이 빈 곳을 채워 *완전한 상태* 로 만드는 게 **클로저(closure)** 예요.

## 정의 — 클로저란

[FIRST](first-formula.md) 를 한 문장으로 *"그 심볼이 유도할 수 있는 것 중에서 **맨 앞에 올 수 있는
단말들**"* 이라고 잡았었죠. 클로저도 딱 그렇게 한 문장으로 잡을 수 있어요.

> **클로저(`I`)** = 그 상태에서 *"지금 막 시작될 수 있는"* 생성규칙을 **하나도 빠짐없이 끌어모은**
> 아이템 집합.

*"막 시작될 수 있는"* — 이게 핵심이에요. 어떤 아이템의 점 바로 뒤에 비단말이 있다는 건, *"이제 곧 그
비단말을 시작한다"* 는 뜻이거든요. 그럼 그 비단말을 만드는 생성규칙들이 *막 시작될 후보* 니까, (점을
맨 앞에 둔 채) 다 끌어와야 하죠.

좀 더 또박또박, **두 규칙** 으로 적으면 이래요.

① **`I` 에 원래 있던 아이템은 그대로 둬요** — 버리는 건 하나도 없어요.\
② **어떤 아이템의 *점 바로 뒤에 비단말* 이 있으면, 그 비단말의 생성규칙들을 (점을 맨 앞에 찍어) 더해요.**

②가 실제로 어떤 모습인지, **우리 문법에서 한 조각** 만 떼어 볼게요 — 앞 *'왜 부풀려야'* 에서 든 그
`Expr → • Term` 의 *빈 곳* 을, 이번엔 ②가 어떻게 채우는지요. (이건 곧 만들 시작 상태 `I₀` 안에서
실제로 일어나는 일의 *일부* 이기도 해요.) 문법은 이거고요.

<pre class="lrbox">   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>   |  <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>  |  <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     |  <span class="setm">id</span></pre>

집합 안에 그 `Expr → • Term` 이 들어 있어요.

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="lrdot">•</span> <span class="nt">Term</span></pre>

점 바로 뒤가 비단말 `Term` 이죠. 그러니 ②가 작동해요 — 문법에서 `Term` 의 생성규칙 둘을 찾아,\
*아직 아무것도 안 읽었으니* 점을 맨 앞에 찍어서 집합에 더해요.

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span> → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>      <span style="opacity:.65">← ② 로 따라 들어옴</span>
   <span class="nt">Term</span> → <span class="lrdot">•</span> <span class="nt">Factor</span>               <span style="opacity:.65">← ② 로 따라 들어옴</span></pre>

*점 뒤 비단말* 을 따라 그 생성규칙들이 끌려 들어오는 것 — 이게 **② 한 번** 이에요.\
그리고 방금 들어온 `Term → • Factor` 의 점 뒤(`Factor`)에도 ②가 또 적용되고… 이렇게 *더 끌려올 게
없을 때까지* 이어져요.

(여기선 `I₀` 의 *한 조각* 만 봤어요. `Accept → • Expr` 에서 시작해 **처음부터 끝까지** 돌려 `I₀` 를
완성하는 과정은, 다음 [계산법](closure-calc.md)에서 한 단계씩 천천히 봐요.)

앞의 **두 규칙(①②)** 을 기호로 짧게 적으면 이렇게 돼요. (`A → α • B β` 는 [LR 아이템](lr-item.md)
장에서 본 표기 그대로예요 — `α`·`β` 는 점 앞뒤의 기호들, `B` 는 점 바로 뒤의 비단말, `γ` 는 `B` 규칙의
오른쪽이에요.)

> **CLOSURE(I)** = 아래 ①②에 **닫혀 있는, 가장 작은** 아이템 집합
>
> ① `I` 의 모든 아이템을 품는다.\
> ② `A → α • B β` 가 있으면 (점 뒤가 비단말 `B`), `B` 의 모든 생성규칙 `B → • γ` 도 품는다.

여기서 **"닫혀 있다"** 와 **"가장 작은"**, 이 두 마디만 잡으면 돼요.

- **닫혀 있다(closed)** — *"더 더할 게 없는"* 상태예요. ②를 아무리 다시 적용해도 새로 나오는 게 없으면
  닫힌 거죠.\
  `{ Expr → • Term }` 하나뿐이면, 점 뒤 `Term` 의 규칙이 빠졌으니 *아직* 안 닫혔어요. `Term` 것을 넣고,
  거기서 또 불려 나오는 `Factor` 것까지 다 넣어야 *비로소* 닫혀요. (그래서 이름도 *닫음* = 클로저.)
- **가장 작은** — *"딱 필요한 만큼만"* 이에요. ②가 *시킨* 아이템만 넣지, 시키지도 않은 걸 괜히 넣진
  않아요.

> 📎 **참고 — 이건 "LR(0)" 클로저예요.** 여기 아이템들은 *생성규칙 + 점* 뿐이라, *"다음에 어떤 토큰이
> 와야 한다"* 같은 **룩어헤드(lookahead)** 정보가 없어요. 그래서 비단말도 *이름만 보고* 한 번씩만
> 펼치면 되죠 (같은 비단말을 두 번 펼칠 일이 없어요).\
> *"그래서 어떤 토큰에서 묶을지(reduce)"* 는 — 클로저가 아니라 **뒤 단계** 에서 정해요. SLR 은
> [FOLLOW](follow-formula.md) 로, LALR 은 따로 계산한 룩어헤드로요. 클로저 단계에선 거기까진 신경 안
> 써요.

## 다음

정의는 *"②에 닫힌 가장 작은 집합"* 이라고 했어요.\
그럼 그걸 **실제로 어떻게 구하는지** — 한 단계씩 직접 돌려보는 게 다음이에요.

👉 **[클로저 · 계산법](closure-calc.md)**

---

👈 앞으로: [상태](lr-state.md)
