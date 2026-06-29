# GOTO — 한 기호를 읽고 다음 상태로

> 🎓 **심화 과정** 이에요.\
> 앞 [클로저 · 계산법](closure-calc.md)에서 시작 상태 `I₀` 를 만들었죠.\
> 이제 그 상태에서 **기호 하나를 읽으면** 어떤 상태로 가는지 — 그걸 정하는 **GOTO** 예요.

> 📍 **사는 곳** · `Analyzer.Goto` · `…/Parsers/Analyzer.cs`

## 정의

> **GOTO(I, X)** = 상태 `I` 안에서 **점 바로 뒤가 `X` 인 아이템들** 을 골라,\
> 그 점을 **`X` 뒤로 한 칸 옮기고**(`A → α • X β` → `A → α X • β`),\
> 그렇게 모은 아이템들을 다시 **[클로저](closure-def.md)** 한 것 — 그게 `X` 를 읽은 다음 상태예요.

GOTO 는 *한 번에 끝나는* 연산이에요.\
(클로저처럼 "닫힐 때까지 반복" 하는 게 아니라, 점을 옮기고 클로저 한 번이면 끝 — 그래서 *정의가 곧
계산법* 이에요.)

## 직접 해보기 — `I₀` 에서 한 기호씩

앞 [계산법](closure-calc.md)에서 만든 시작 상태 `I₀` (7개)를 다시 가져올게요.

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

이 상태에서 *점 바로 뒤* 에 올 수 있는 기호는 — `Expr`, `Term`, `Factor`, `'('`, `id` 예요.\
(이게 [상태](lr-state.md) 장에서 본 `MarkSymbolSet` 이죠.) 이 기호마다 GOTO 를 한 번씩 해봐요.

### `id` 를 읽으면 — `GOTO(I₀, id)`

점 뒤가 `id` 인 아이템은 `Factor → • id` 하나뿐이죠. 점을 `id` 뒤로 옮기면:

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span>        ──( id 읽음 )──▶        <span class="nt">Factor</span> → <span class="setm">id</span> <span class="lrdot">•</span></pre>

`Factor → id •` 은 점이 끝에 닿았어요 — *완료(reduce) 아이템* 이죠. (이 상태에 오면 *"`id` 를 `Factor`
로 묶어라"* 가 돼요.) 점 뒤에 비단말이 없어 클로저로 더 붙을 것도 없고요. → **아이템 1개짜리** 다음
상태예요.

### `Term` 을 읽으면 — `GOTO(I₀, Term)`

점 뒤가 `Term` 인 아이템은 *둘* 이에요. 둘의 점을 `Term` 뒤로 옮기면:

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="lrdot">•</span> <span class="nt">Term</span>              ──( Term )──▶   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>
   <span class="nt">Term</span> → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>   ──( Term )──▶   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

이 둘을 모으면 (점 뒤에 새 비단말이 없어 클로저로 더 안 붙어요):

<pre class="lrbox">   <span class="nt">Expr</span> → <span class="nt">Term</span> <span class="lrdot">•</span>
   <span class="nt">Term</span> → <span class="nt">Term</span> <span class="lrdot">•</span> <span class="setm">'*'</span> <span class="nt">Factor</span></pre>

> 💡 이 상태, 어디서 봤죠? 바로 **[상태](lr-state.md) 장의 그 `id * id` 상태** 예요!\
> *"`id` 를 `Term` 까지 읽었을 때"* 의 그 상태가 — 사실은 **`I₀` 에서 `Term` 을 읽고 도달하는 상태**
> 였던 거예요. 흩어져 보이던 두 챕터가 여기서 만나요.

### `Expr` 를 읽으면 — `GOTO(I₀, Expr)`

점 뒤가 `Expr` 인 아이템도 둘이에요 (`Accept → •Expr`, `Expr → •Expr '+' Term`). 옮기면:

<pre class="lrbox">   <span class="nt">Accept</span> → <span class="lrdot">•</span> <span class="nt">Expr</span>          ──( Expr )──▶   <span class="nt">Accept</span> → <span class="nt">Expr</span> <span class="lrdot">•</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span> ──( Expr )──▶   <span class="nt">Expr</span> → <span class="nt">Expr</span> <span class="lrdot">•</span> <span class="setm">'+'</span> <span class="nt">Term</span></pre>

여기 `Accept → Expr •` 가 특별해요 — *가상 시작 규칙이 끝까지 갔다* 는 건, **입력이 여기서 끝(`$`)나면
파싱을 받아들인다(accept)** 는 뜻이에요!\
같이 있는 `Expr → Expr • '+' Term` 은 — `'+'` 가 더 오면 식을 이어가고요.\
(그래서 이 상태가 곧 *"끝내고 받아들이거나, `+` 로 더 잇거나"* 예요. 우리가 만들 자동기계의 **목표
지점** 이죠.)

### `'('` 를 읽으면 — `GOTO(I₀, '(')` · 여기서 클로저가 진짜 일해요

점 뒤가 `'('` 인 아이템은 `Factor → • '(' Expr ')'` 하나죠. 점을 `'('` 뒤로 옮기면:

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>    ──( '(' 읽음 )──▶    <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">')'</span></pre>

그런데 이번엔 달라요 — 옮긴 자리의 **점 뒤가 비단말 `Expr`** 이에요!\
앞의 `id`·`Term`·`Expr` 때는 점만 옮기면 끝이었지만, 이 한 아이템만으론 *`Expr` 를 어떻게 시작하는지* 가
빠진 **불완전한 상태** 죠. 그래서 **클로저가 다시 작동** 해요.

점 뒤 `Expr` 를 따라 — `Expr` 의 규칙, 거기서 `Term` 의 규칙, 또 `Factor` 의 규칙까지 — 줄줄이 끌려
들어와 **7개로 채워져요.**

<pre class="lrbox">   <span class="nt">Factor</span> → <span class="setm">'('</span> <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">')'</span>        <span style="opacity:.65">← 점 옮긴 것</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Expr</span> <span class="setm">'+'</span> <span class="nt">Term</span>         <span style="opacity:.65">← 클로저로 채워짐</span>
   <span class="nt">Expr</span>   → <span class="lrdot">•</span> <span class="nt">Term</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Term</span> <span class="setm">'*'</span> <span class="nt">Factor</span>
   <span class="nt">Term</span>   → <span class="lrdot">•</span> <span class="nt">Factor</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">'('</span> <span class="nt">Expr</span> <span class="setm">')'</span>
   <span class="nt">Factor</span> → <span class="lrdot">•</span> <span class="setm">id</span></pre>

이게 바로 **`I₄`** 예요 ([정준 집합](canonical-set.md) 에 그대로 나와요).

> 💡 보세요 — `id`·`Term`·`Expr` 때는 *점만 옮기면 끝* 이었지만, `'('` 는 점 뒤가 비단말이라 **클로저가
> 진짜 일을 했어요** (1개 → 7개). 이게 GOTO 정의의 마지막 *"다시 클로저"* 부분이에요.\
> 그러니 **클로저는 `I₀` 만들 때만 쓰는 게 아니라, GOTO 마다 다시 쓰여요.** 덕분에 *어떤 GOTO 의
> 결과든 늘 빈 곳 없는 완전한 상태* 가 되죠.\
> 한 줄로 — **클로저 = 상태 하나 완성, GOTO = 점 옮기고 → 다시 클로저.**

### 정리 — GOTO 결과마다 번호(`Iₙ`)가 붙어요

방금 `I₀` 에서 GOTO 로 간 다음 상태들 — 하나하나가 **번호를 받는 새 상태** 예요.\
`I₀` 의 `MarkSymbolSet`(읽을 수 있는 기호)이 `{ Expr, Term, Factor, '(', id }` 다섯이었으니, 다음 상태도
다섯이고요. 정준 집합을 만들 때 *상태가 발견되는 순서대로* `I₁`, `I₂`, … 가 붙어요.

| `I₀` 에서 읽는 기호 | GOTO 결과 |
|:--|:--|
| `Expr`   | `I₁` |
| `Term`   | `I₂` |
| `Factor` | `I₃` |
| `'('`    | `I₄` |
| `id`     | `I₅` |

(그래서 위 `'('` 의 결과가 `I₄` 였죠. `Factor` 는 예제로 안 봤지만 같은 식이에요 — `Term → • Factor` 의
점을 옮겨 `Term → Factor •` 가 `I₃`.)\
주의할 점 하나 — 위에서는 *설명하기 좋은 순서* 로 `id`·`Term`·`Expr`·`'('` 를 봤지만, **번호 자체는
"발견 순서" 로** 매겨져요. 그래서 보여준 차례(`id` 먼저)와 번호(`id` = `I₅`)가 꼭 같진 않아요.

## 구현 — `Analyzer.Goto`

```csharp
public static CanonicalState Goto(CanonicalState iStatus, Symbol toSeeSymbol)
{
    if (toSeeSymbol == null) return null;
    var param = new CanonicalState();

    foreach (var item in iStatus)
    {
        if (item.MarkSymbol == toSeeSymbol)   // 점 뒤가 읽을 기호와 같은 아이템만
        {
            var clone = item.Clone() as LRItem;
            clone.MoveMarkSymbol();            // 점을 한 칸 앞으로
            param.Add(clone);
        }
    }

    return Analyzer.Closure(param);            // 옮긴 아이템들을 다시 클로저
}
```

- `item.MarkSymbol == toSeeSymbol` — *점 바로 뒤가 읽을 기호 `X` 인* 아이템만 고르는 거예요.
- `clone.MoveMarkSymbol()` — [LR 아이템](lr-item.md) 장에서 본 *"점 한 칸 전진"* 그대로고요. (원본을
  건드리지 않으려 `Clone` 부터 해요 — `I₀` 은 그대로 둬야 하니까요.)
- `return Closure(param)` — 옮긴 아이템들을 *다시 클로저* 해요. GOTO 의 결과도 **완전한 상태** 여야
  하거든요. (방금 `'('` 예처럼 점 뒤가 비단말이면 클로저가 채워 주고, `id`·`Term` 예처럼 아니면 그대로
  돌려줘요.) — 클로저가 `I₀` 뿐 아니라 *여기서도* 쓰인다는 게 이 한 줄에 들어 있어요.

## 다음 장

`I₀` 하나에서 `id`·`Term`·`Expr`·`'('`… 로 GOTO 를 해보니, *다음 상태들* 이 줄줄이 나왔어요.

이걸 **모든 상태에 대해, 더 새 상태가 안 나올 때까지 반복** 하면 — 시작 상태에서 *도달 가능한 모든
상태* 가 모여요.\
그 상태들의 모음이 바로 **정준 집합(canonical collection)** 이에요.

👉 **[정준 집합 — 모든 상태 만들기](canonical-set.md)**

---

👈 앞으로: [클로저 · 구현](closure-impl.md)
