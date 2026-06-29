# 상태 — LR 아이템들의 집합 (I₀, I₁, …)

> 🎓 **심화 과정** 이에요.\
> 앞 [LR 아이템](lr-item.md)에서 *점 찍힌 생성규칙* 하나를 봤죠.\
> 그런데 파서가 입력을 읽다 어느 지점에 서면 — *가능성 있는 아이템* 이 보통 **여럿** 이에요.\
> 그 여럿을 한 묶음으로 모은 게 **상태(state)** 예요.

> 📍 **사는 곳** · `CanonicalState` · `…/Parsers/Collections/CanonicalState.cs`

## 잠깐 — 예제 문법부터 다시 펴 둘게요

본격적으로 들어가기 전에, 잠깐 숨 고르고 가요.\
앞으로 상태 이야기엔 우리 **예제 문법** 이 줄곧 등장하거든요. [FIRST / FOLLOW](first-follow.md) 에서부터
쭉 함께 써온 바로 그 문법이에요. 멀어지지 않게, 눈앞에 다시 펴 둘게요.

<pre class="lrbox">   <span class="nt">Expr</span>   → <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>   |  <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>  |  <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>     |  <span class="setm">id</span>
</pre>

- 식 `Expr` 은 — 항 `Term` 들을 `'+'` 로 이은 것,
- 항 `Term` 은 — 인자 `Factor` 들을 `'*'` 로 이은 것,
- 인자 `Factor` 는 — 괄호식 `'(' Expr ')'` 이거나, 이름 하나 `id` 예요.

곱셈 `'*'` 이 덧셈 `'+'` 보다 *더 안쪽* 에서 묶이는 (곱셈이 먼저인) 구조죠.\
이 작은 문법 하나면 충분해요 — **지금부터 나오는 모든 예시가 여기서** 나와요. 이 세 줄만 곁에 두고
가면 돼요.

## 상태란 — 아이템들의 집합, `Iₓ`

파서가 토큰을 읽어 나가다 어느 자리에 서 있다고 해봐요.\
그 자리에선 "지금 진행 중일 수 있는 규칙" 이 하나가 아니라 **여러 개** 일 수 있어요.\
그 *동시에 가능한 LR 아이템들* 을 모은 게 한 **상태** 예요.

교과서에선 상태마다 번호를 붙여 **`I₀`, `I₁`, `I₂` …** 로 적어요. (`I` 는 *item set* 의 I 예요.)

말로는 막연하니, **왜 아이템 하나로는 부족한지** 부터 짚고 — 진짜 상태 하나를 뜯어볼게요.

## 왜 '아이템 하나' 가 아니라 '집합' 일까

아이템 하나를 봐요 — `Expr → Term •` (*"Term 까지 읽어 Expr 이 끝났다"*).

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span></pre>

이 아이템 하나만 보면 *"Term 다 읽었으니 Expr 로 묶자"* 같죠.\
그런데 — 정말 그렇게 단정해도 될까요? 우리 문법엔 이 규칙도 있어요.

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
</pre>

즉 방금 그 `Term` 뒤에 `'*' Factor` 가 더 붙어 **더 큰 `Term`** 이 될 수도 있어요.\
그 가능성은 이 아이템으로 표현되죠.

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

점을 보세요 — *둘 다 `Term` 바로 뒤* 예요. **"여기까지 `Term` 을 읽었다"** 는 똑같은 상황을, 두 규칙이
각자의 입장에서 본 거예요.\
그러니 이 자리를 정직하게 적으려면 — **두 아이템을 동시에** 안고 있어야 해요.\
이렇게 *한 자리에서 가능한 아이템을 모두 모은* 게 바로 **상태** 예요.

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>                <span style="opacity:.65"># 이 Term 으로 Expr 이 끝났을 수도</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>     <span style="opacity:.65"># 아니면 '*' 가 붙는 더 큰 Term 의 앞부분일 수도</span></pre>

> 이런 상태가 *언제, 어떻게* 정확히 만들어지는지는 — 바로 다음 **[클로저](closure-def.md)** 와
> **[GOTO](goto.md)** 에서 다뤄요. 지금은 *"상태 = 한 자리에서 가능한 아이템을 다 모은 집합"* 이라는
> 것만 잡고 가요.

## 그래서, 이 상태에선 뭘 하지?

두 아이템은 서로 다른 말을 해요.

- **`Expr → Term •`** — 점이 *끝* 에 닿았어요. *"Term 하나면 그게 곧 Expr"* — 다 봤으니 **묶을(reduce)** 수 있어요.
- **`Term → Term • '*' Factor`** — 점 뒤에 `'*'` 가 남았어요. *"뒤에 `* Factor` 가 더 올 수도"* — 그러면 **더 읽어야(shift)** 해요.

**둘 중 뭘 할지는 — 다음 토큰** 이 정해요.

- 다음 토큰이 **`*`** 면 → `Term → Term • '*' Factor` 쪽. **`'*'` 를 더 읽어요(shift).**
- 다음 토큰이 **`+`·`)`·입력 끝(`$`)** 이면 → 더 붙을 게 없으니 `Expr → Term •` 쪽. **Term 을 Expr 로 묶어요(reduce).**

> 💡 *"다음이 `+`·`)`·`$` 면 묶는다"* — 어디서 본 집합이죠? 바로 **[FOLLOW(Expr)](follow-formula.md)** `= { $, '+', ')' }` 예요.\
> FIRST/FOLLOW 가 *여기서* 쓰여요 — *"묶어도 되는 다음 토큰"* 을 FOLLOW 가 알려주거든요. (이 연결은 *파싱 테이블* 장에서 확실히 매듭지어요.)

## 🌱 씨앗 — 두 행동이 겹치면? '충돌'

방금 그 상태엔 두 아이템이 같이 있었죠 — 묶으려는(reduce) `Expr → Term •` 와, 더 읽으려는(shift)
`Term → Term • '*' Factor`. **한 상태에 두 행동이 공존** 한 거예요.

그런데도 파서가 헷갈리지 않았던 건 — *각 행동이 반응하는 다음 토큰이 서로 겹치지 않았기* 때문이에요.

| 다음 토큰 | 이 상태의 행동 |
|:--|:--|
| `'*'` | **shift** — `'*'` 를 읽어 `Term → Term • '*' Factor` 진행 |
| `$` · `'+'` · `')'`  (`= FOLLOW(Expr)`) | **reduce** — `Expr → Term •` 로 묶기 |

표를 보세요 — **토큰마다 행동이 딱 하나씩** 이죠.\
`'*'` 는 reduce 쪽 토큰 `{ $, '+', ')' }` 에 없으니, *어떤 토큰이 오든 할 일이 하나로 정해져요* —
그래서 깔끔하게 갈렸어요.

### 그럼, 만약 겹친다면?

상상만 해봐요 — 만약 `'*'` 가 `FOLLOW(Expr)` 에도 들어 있었다면? 다음 토큰이 `'*'` 일 때, 두 아이템이
*동시에* 손을 들어요.

<pre class="lrbox">   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>    →  "'*' 를 읽자!"   (shift)
   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>               →  "Expr 로 묶자!"  (reduce — '*' 가 FOLLOW(Expr) 에 있다 가정)</pre>

표로 치면 — `'*'` 칸이 이렇게 돼요.

| 다음 토큰 | 이 상태의 행동 |
|:--|:--|
| `'*'` | **shift** _그리고_ **reduce**  ⚠️ |
| `$` · `'+'` · `')'` | reduce |

아까 깔끔한 표에선 `'*'` 칸에 **shift 하나** 뿐이었죠. 이제 거기에 **reduce 까지** 들어와 — *한 칸에
행동이 둘* 이에요.\
같은 토큰 하나를 두고 **shift 와 reduce 가 둘 다** 가능해져요. 파서가 *"묶을까, 더 읽을까"* 를 못
정하죠.\
이 *"한 자리에서 행동이 갈리는"* 게 바로 **충돌(conflict)** — 그중에서도 **shift/reduce 충돌** 이에요.

> 우리 예제 문법은 다행히 이런 겹침이 없어, 어떤 상태에서도 충돌이 안 나요.\
> 이렇게 *충돌이 하나도 없는* 문법엔 이름이 있어요 — 그 파싱 방식 이름 그대로 **LR 문법** 이라
> 불러요. 우리 건 가장 단순한 **SLR(1)** 만으로 충돌이 없는, 교과서의 대표적 **SLR(1) 문법** 이고요.
> (충돌이 사라지는 룩어헤드 정밀도 순으로 **SLR(1) ⊂ LALR(1) ⊂ LR(1)** 로 더 나뉘는데, 이건 *파싱
> 테이블* 장에서. — 흔히 헷갈리는 *'문맥 자유 문법(CFG)'* 은 충돌과 *무관한* 훨씬 넓은 갈래라, 거의
> 모든 언어 문법이 CFG 예요. 그중 충돌 없이 LR 파싱이 되는 일부가 **LR 문법** 이죠.)\
> (한편 충돌이 잘 나는 가장 유명한 예가 `if-then-else` 의 *"`else` 를 어느 `if` 에 붙이나"* 죠.)\
> 어떻게 **검출하고 가려내는지** 는 한참 뒤 *파싱 테이블* 장에서 제대로 다뤄요. 지금은 *"한 상태에 두
> 행동이 겹치면 충돌"* 이라는 씨앗만 심어두고 가요.

## 코드 — `CanonicalState`

상태가 *"아이템들의 집합"* 이라고 했죠. 코드도 말 그대로예요.

```csharp
public class CanonicalState : HashSet<LRItem>   // 한 상태 = LR 아이템들의 집합
{
    public int StateNumber { get; }   // 그 상태의 번호 — Iₓ 의 x (I0 이면 0)
}
```

> 💡 집합(`HashSet`)인 게 자연스러워요 — 한 상태에 *같은 아이템* 이 두 번 있을 이유가 없으니까요.\
> [LR 아이템](lr-item.md)의 정체성이 *"생성규칙 + 점 위치"* 라, 똑같은 아이템은 집합에서 자동으로
> 하나로 합쳐져요.

## 코드로 — 두 부류 가려내기

앞에서 본 그 두 행동 — *묶기(reduce)* 와 *더 읽기(shift)* — 는, 결국 아이템이 **점이 끝났는지** 로
갈리는 거였죠. 코드도 그 둘을 이렇게 가려내요.

- **shift 아이템** — 점이 아직 *안* 끝난 것 (`A → α • X β`).\
  더 읽을 게 남았다 → `X` 를 *읽어 나간다*. (코드: `ShiftItemList`)
- **완료(reduce) 아이템** — 점이 *끝* 에 닿은 것 (`A → α •`).\
  다 읽었다 → 이 규칙으로 *묶는다(reduce)*. (코드: `IsReachedHandle`, `ReachedHandleSet`)

```csharp
public bool IsReachedHandle { get; }                 // 이 상태에 완료 아이템이 있나
public HashSet<NonTerminalSingle> ReachedHandleSet;  // 완료된 생성규칙들 (reduce 후보)
public HashSet<NonTerminalSingle> ShiftItemList;     // 아직 진행 중인 생성규칙들
```

말로만 보면 흐릿하니, **예제 상태를 그대로 대입** 해 봐요. 각 변수에 무엇이 담기는지 보면 단번에
또렷해져요.

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← 완료 아이템</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>   <span style="opacity:.65">← 진행 중(shift) 아이템</span></pre>

| 변수 | 이 상태에서의 값 | 왜 |
|:--|:--|:--|
| `IsReachedHandle` | `true` | 완료 아이템 `Expr → Term •` 이 *하나라도* 있으니까 |
| `ReachedHandleSet` | `{ Expr → Term }` | 그 완료 아이템의 **생성규칙** |
| `ShiftItemList` | `{ Term → Term '*' Factor }` | 진행 중 아이템의 **생성규칙** |

보세요 — 한 상태의 두 아이템이, `Expr → Term •` 은 *완료* 칸(`ReachedHandleSet`)으로,
`Term → Term • '*' Factor` 는 *진행 중* 칸(`ShiftItemList`)으로 깔끔하게 갈라져 들어가죠.\
(둘 다 들어 있으니 `IsReachedHandle` 은 `true` — *"이 상태엔 묶을 것도 있다"* 는 신호예요.)

그리고 `ShiftItemList` 도 `ReachedHandleSet` 도 **집합** 이라, 아이템이 많으면 여러 개가 담겨요.\
*완료도 진행 중도 둘씩* 인 상태를 그려보면 한눈에 와닿아요. (우리 예제 문법엔 이렇게 모이진 않아요 —
나눗셈 `Term → Term '/' Factor` 와, '문장은 식 하나' 규칙 `Stmt → Term` 이 더 있었다고 *가정* 한
거예요.)

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← 완료</span>
   <span class="nt">Stmt</span> → <span class="nt">Term</span> <span class="lrdot">•</span>              <span style="opacity:.65">← 완료   (Stmt → Term 이 있었다면)</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span>   <span style="opacity:.65">← 진행 중</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'/'</span> <span class="nt">Factor</span>   <span style="opacity:.65">← 진행 중 (Term → Term '/' Factor 가 있었다면)</span></pre>

| 변수 | 값 (가정한 상태에서) |
|:--|:--|
| `IsReachedHandle` | `true` |
| `ReachedHandleSet` | `{ Expr → Term, `**`Stmt → Term`**` }` |
| `ShiftItemList` | `{ Term → Term '*' Factor, `**`Term → Term '/' Factor`**` }` |

완료 둘, 진행 중 둘 — 변수마다 여러 아이템이 담겼죠.\
실제 우리 예제 문법에선 이 상태가 완료 하나·진행 중 하나로 단출하지만, 구조상 **얼마든 늘 수 있는
집합** 이라는 걸 보여주려는 거예요.

## 이 상태에서 읽을 수 있는 기호 — `MarkSymbolSet`

shift 아이템들의 **점 바로 뒤 기호** 를 다 모은 게 `MarkSymbolSet` 이에요.\
*"이 상태에서 지금 읽을 수 있는 기호" 목록* 이죠. (다음 [GOTO](goto.md) 에서, 이 기호들로 다음 상태를
찾아가요.)

```csharp
public SymbolSet MarkSymbolSet { get; }   // 상태 안 아이템들의 '점 뒤 기호' 전부
```

위 예제 상태라면 `MarkSymbolSet = { '*' }` 예요. (`Term → Term • '*' Factor` 의 점 뒤 `'*'` 하나뿐.)

## 한눈에 — `CanonicalState`의 전체 모습

```csharp
public class CanonicalState : HashSet<LRItem>   // 상태 = LR 아이템 집합
{
    public int StateNumber { get; }                      // 상태 번호 (Iₓ 의 x)

    // ── 이 상태에서 읽을 수 있는 기호 ───────
    public SymbolSet MarkSymbolSet { get; }              // 점 뒤 기호 전부

    // ── 두 부류 가르기 ──────────────────────
    public bool IsReachedHandle { get; }                 // 완료 아이템이 있나
    public HashSet<NonTerminalSingle> ReachedHandleSet;  // 완료(reduce) 생성규칙들
    public CanonicalState ReachedHandleItem { get; }     // 완료 아이템만 모은 상태
    public HashSet<NonTerminalSingle> ShiftItemList;     // 진행 중(shift) 생성규칙들

    // ── 조회 ────────────────────────────────
    public bool   HasItem(LRItem item);
    public LRItem GetItem(LRItem item);
}
```

한 줄로 — **상태 `Iₓ` = LR 아이템들의 집합. 그 안엔 *더 읽을* shift 아이템과 *묶을* 완료 아이템이
섞여 있고, 다음 토큰이 길을 고른다.**

## 다음 장

상태가 *무엇인지* 봤어요 — 아이템들의 집합 `Iₓ`.

그런데 상태를 *만들* 땐, 아이템을 아무렇게나 모으는 게 아니라 — 점 뒤 비단말의 생성규칙들까지
빠짐없이 채워야 *완전한* 상태가 돼요.\
그 "빠짐없이 채우기" 가 바로 **클로저** 예요.

👉 **[클로저 · 정의](closure-def.md)**

---

👈 앞으로: [LR 아이템](lr-item.md)
