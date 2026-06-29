# 파싱 테이블 · CLR — 원리

[SLR 장](parse-table-slr.md) 에서 SLR 의 약점 — **헛충돌** — 을 봤어요. 뿌리는, SLR 이 reduce 를 정할 때 `FOLLOW` 를 *문법 전체* 에서 끌어온 데 있었죠. 그래서 *지금 이 상태* 와 무관한 글자까지 딸려 들어왔고요.

답은 분명해요 — **글로벌 `FOLLOW` 말고, *이 상태에 딱 맞는* 정밀한 lookahead 를 쓰자.** 그 lookahead 가 *무엇이고 어떻게 구하는지* 는 [앞 장 — lookahead](parse-table-lookahead.md) 에서 봤죠: *아이템마다* 붙는 다음 글자(LR(1) 아이템)이고, 그 값은 `FIRST(β t)` 로 구한다고요.

이 장의 **CLR** 은 — 그 lookahead 를 *가장 철저하게*, 상태를 만들 **때부터 전면적으로** 쓰는 방식이에요.

---

## 가장 철저한 답 — lookahead 를 처음부터

[앞 장](parse-table-lookahead.md) 의 LR(1) 아이템(규칙 + 점 + lookahead)을 — 상태를 만들 *때부터* 빠짐없이 달고 가요.

그러면 *겉모습(점 위치)은 같아도 lookahead 가 다르면* 서로 다른 상태로 갈라져요. 상태마다 *그 문맥에 딱 맞는* lookahead 만 가지니까 — **헛충돌이 아예 안 생겨요.**

> 🔖 **CLR (Canonical LR) = LR(1)** — lookahead 를 상태에 처음부터 박아 넣어, 문맥마다 정밀하게 가르는 방식.

---

## 예제 — SLR 의 헛충돌을 CLR 로

[SLR 장](parse-table-slr.md) 에서 헛충돌이 났던 *그 문법, 그 상태* 를 CLR 로 다시 봅시다.

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">c</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">d</span>
<span class="nt">S</span> → <span class="setm">e</span> <span class="nt">A</span> <span class="setm">d</span>
<span class="nt">A</span> → <span class="setm">b</span>
<span class="nt">B</span> → <span class="setm">b</span>
</pre>

SLR 은 `a b` 상태에서 `A → b •` 의 reduce 를 <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> *전체* 에 적어 — `d` 에서 `B → b •` 와 부딪혔어요. (그 `d` 는 *저 멀리 `e A d`* 에서 묻어온 것.)

**CLR 은 lookahead 를 *문맥별* 로 들고 다녀요.** 무슨 말이냐면 — 상태를 만들 때, 각 아이템 옆에 *"이 길에서 내 뒤에 올 수 있는 글자"* 를 같이 적어둔다는 뜻이에요. 천천히 따라가 봅시다.

**① `a` 를 읽었으면 — 우리는 지금 어느 길 위에 있나?**\
문법에서 `a` 로 시작하는 규칙은 *둘뿐* 이에요.

<pre class="lrbox">
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">A</span> <span class="setm">c</span>      <span style="opacity:.65">← a 다음 A, 그 다음 c</span>
<span class="nt">S</span> → <span class="setm">a</span> <span class="nt">B</span> <span class="setm">d</span>      <span style="opacity:.65">← a 다음 B, 그 다음 d</span>
</pre>

`S → e A d` 는 `e` 로 시작하니, `a` 를 읽은 *지금 우리* 와는 상관없는 길이에요.\
그러니 `a` 를 읽은 순간 — 갈 수 있는 길은 **이 둘뿐** 이에요.

**② 그럼 이 길에서 `A`·`B` 뒤엔 뭐가 오나?**\
여기에 *핵심* 이 있어요. **`A` 는 문법에 *두 군데* 나오는데 — 나오는 자리마다 뒤 글자가 달라요.**

- `a A c` 의 A → 뒤엔 **`c`**
- `e A d` 의 A → 뒤엔 **`d`**

이 둘을 *몽땅 합친* 게 <code>FOLLOW(A) = <span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> 예요 — SLR 이 쓰는 *뭉뚱그린* 집합이 바로 이거죠.

그런데 우린 방금 **`a`** 를 읽었어요. 그럼 *둘 중 앞쪽* — `a A c` 의 A — 길로 들어선 거예요. (뒤쪽 `e A d` 는 `e` 로 시작하니 *이미 탈락* 했고요.)\
→ 그러니 *지금 이 자리* 의 `A` 뒤엔 **`c` 뿐.** `e A d` 의 `d` 는 *다른 자리 A* 의 사정이라 — 여긴 못 와요.

(`B` 는 `a B d` 한 곳에만 나오니, 뒤엔 늘 `d` 예요.)

**③ CLR 은 그걸 그대로 아이템에 적어둬요.**\
그래서 `a b` 상태의 두 완료 아이템은 *각자 자기 글자* 를 달고 있어요.

<pre class="lrbox">
<span class="nt">A</span> → <span class="setm">b</span> <span class="lrdot">•</span>   <span style="opacity:.65">뒤에 올 수 있는 글자: c</span>
<span class="nt">B</span> → <span class="setm">b</span> <span class="lrdot">•</span>   <span style="opacity:.65">뒤에 올 수 있는 글자: d</span>
</pre>

**바로 이게 SLR 과 갈리는 지점이에요.** SLR 은 `A → b •` 에 *글로벌* `FOLLOW(A)` 를 통째로 적어 `d` 까지 딸려 왔지만 — CLR 은 *이 길에서 진짜 올 수 있는* 글자만 적어요.

| `a b` 상태 | `A → b •` 가 reduce 하는 글자 | `B → b •` 가 reduce 하는 글자 |
|:--|:--:|:--:|
| SLR (글로벌 FOLLOW) | <code><span class="setb">{</span><span class="setm"> c d </span><span class="setb">}</span></code> | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> |
| **CLR (문맥별)** | <code><span class="setb">{</span><span class="setm"> c </span><span class="setb">}</span></code> ← `d` 가 빠졌죠! | <code><span class="setb">{</span><span class="setm"> d </span><span class="setb">}</span></code> |

이제 **`d` 가 들어오면** — `A → b •` 의 글자(`c`)엔 `d` 가 없으니 *안 접고*, `B → b •` 의 글자(`d`)에만 맞으니 *`B` 로 접어요.* → 한 칸에 할 일이 *하나뿐* → **헛충돌이 사라졌어요.** ✅

CLR 은 이렇게 *"이 길에서 진짜 올 수 있는 글자"* 만 보니까, `e A d` 같은 *딴 길* 의 사정이 끼어들 틈이 없어요. 그래서 *완벽하게* 정밀한 거예요.

---

## 공짜가 아니에요 — 상태 폭증

대신 대가가 있어요. 겉모습(코어) 같은 상태도 lookahead 가 다르면 자꾸 갈라지니 — **상태 수가 확 불어나요(폭증).** 작은 문법에선 괜찮지만, 큰 문법에선 이 폭증이 꽤 부담이에요.

---

## 다음

CLR 은 *완벽하지만 비싸요.* 이 폭증을 없애면서 정밀도는 거의 그대로 가져가는 실용형이 — *곧 이어질* **LALR** 이에요.

그 전에 — *우리 엔진* 은 CLR 을 어떻게 구현했을까요? (솔직히 말하면, 좀 짧아요.)

👉 **[파싱 테이블 · CLR — 구현](parse-table-clr-impl.md)**
