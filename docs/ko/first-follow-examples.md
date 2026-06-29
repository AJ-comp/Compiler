# FIRST · FOLLOW — 예제 모음 (직접 풀어보기)

> 🎓 **심화 과정 · 이론** 이에요.\
> [FIRST 계산 규칙](first-rules.md) · [FOLLOW 계산 규칙](follow-rules.md) 을 *여러 문법에 직접 적용* 해보는 **연습장** 이에요.

본문에서 쭉 쓴 `Expr` 문법은 ε(사라지는 비단말)이 없어서, 규칙 중 일부는 *실제로 돌아가는 모습* 을 못 봤어요. 그래서 여기선 **ε 이 들어간 문법** 들로, *쉬운 것부터 점점 빡세게* 풀어볼게요. 손으로 같이 따라오면 제일 좋아요.

---

## 쓸 규칙 한눈에 (치트시트)

풀기 전에 — 쓸 공식은 아래에 정리돼 있어요.

- **[FIRST 공식 정리](first-rules.md#정리--세-경우는-결국-한-식)**
- **[FOLLOW 공식 정리](follow-rules.md#정리)**

> 📎 아래 예제 문법에서 — *소문자* `a b c d x y …` = **단말**, *대문자* `A B S E T …` = **비단말**, `ε` = *사라짐(빈 것)*.

---

## 예제 1 — ε 이 처음 등장 (있어도 되고 없어도 되는 기호)

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="setm">b</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
</pre>

`A` 가 `a` 도 되고 *아무것도 아닌 것(ε)* 도 될 수 있어요 — "있어도 되고 없어도 되는" 기호죠.

### FIRST

`S → A b` 는 기호 열이니, ⊕ 공식으로 시작해요.

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = FIRST(<span class="nt">A</span>) ⊕ FIRST(<span class="setm">b</span>)
</pre>

조각을 채우면 —

<pre class="lrbox">
   FIRST(<span class="nt">A</span>) = { <span class="setm">a</span>, ε }     ← A → a | ε 라 nullable
   FIRST(<span class="setm">b</span>) = { <span class="setm">b</span> }
</pre>

`FIRST(A)` 에 ε 이 있으니 — ε 을 빼고 다음 칸 `b` 까지 ⊕:

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = { <span class="setm">a</span>, ε } ⊕ { <span class="setm">b</span> }
            = ( { <span class="setm">a</span>, ε } − ε ) ∪ { <span class="setm">b</span> }
            = { <span class="setm">a</span>, <span class="setm">b</span> }
</pre>

→ **`FIRST(S) = { a, b }`** (S 는 ε 을 못 내니 nullable 아님).

### FOLLOW

<pre class="lrbox">
   ① 시작 기호 <span class="nt">S</span>   →  FOLLOW(<span class="nt">S</span>) = { $ }
   ② <span class="nt">S</span> → <span class="nt">A</span> <span class="setm">b</span> 에서 A 뒤 β = "<span class="setm">b</span>"  →  FIRST(<span class="setm">b</span>) − ε = { <span class="setm">b</span> }  →  FOLLOW(<span class="nt">A</span>) ⊇ { <span class="setm">b</span> }
</pre>

`A` 는 어느 생성규칙에서도 *맨 끝* 이 아니에요(`b` 가 늘 뒤에 있음) → 규칙 ③ 해당 없음.

<pre class="lrbox">
   FOLLOW(<span class="nt">S</span>) = { $ }
   FOLLOW(<span class="nt">A</span>) = { <span class="setm">b</span> }
</pre>

> 🔖 **이 예제로 새로 본 것** — ε 이 `FIRST(A)` 에 들어가고, `FIRST(S)` 를 구할 때 ⊕ 가 *"앞이 사라질 수 있으니 다음 칸까지"* 넘어가는 장면. (Expr 문법에선 못 본 거예요.)

---

## 예제 2 — nullable 이 줄줄이 (ε 둘이 연달아)

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span> <span class="setm">c</span>
<span class="nt">A</span> → <span class="setm">a</span> | ε
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

이번엔 `A` 도 `B` 도 사라질 수 있어요. `S` 의 FIRST 를 구할 때 ⊕ 가 *두 칸* 을 건너뛰는지 봐요.

### FIRST

`S → A B c` 는 기호 열이니, ⊕ 공식으로 시작해요.

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = FIRST(<span class="nt">A</span>) ⊕ FIRST(<span class="nt">B</span>) ⊕ FIRST(<span class="setm">c</span>)
</pre>

조각을 채우면 —

<pre class="lrbox">
   FIRST(<span class="nt">A</span>) = { <span class="setm">a</span>, ε }     ← nullable
   FIRST(<span class="nt">B</span>) = { <span class="setm">b</span>, ε }     ← nullable
   FIRST(<span class="setm">c</span>) = { <span class="setm">c</span> }
</pre>

왼쪽부터 ⊕ — ε 이 있으면 그 칸을 빼고 다음 칸까지:

- `FIRST(A)` 에 ε 있음 → `a` 챙기고 **다음 칸으로**
- `FIRST(B)` 에 ε 있음 → `b` 챙기고 **또 다음 칸으로**
- `FIRST(c)` 엔 ε 없음 → `c` 챙기고 **멈춤**

<pre class="lrbox">
   FIRST(<span class="nt">S</span>) = { <span class="setm">a</span>, ε } ⊕ { <span class="setm">b</span>, ε } ⊕ { <span class="setm">c</span> }
            = { <span class="setm">a</span> } ∪ { <span class="setm">b</span> } ∪ { <span class="setm">c</span> } = { <span class="setm">a</span>, <span class="setm">b</span>, <span class="setm">c</span> }
</pre>

→ **`FIRST(S) = { a, b, c }`** (끝의 `c` 가 단말이라 S 는 nullable 아님).

### FOLLOW

<pre class="lrbox">
   ① 시작 기호 <span class="nt">S</span>   →  FOLLOW(<span class="nt">S</span>) = { $ }
</pre>

**② — `A` 뒤, `B` 뒤를 봐요.**

<pre class="lrbox">
   <span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span> <span class="setm">c</span>   :  A 뒤 β = "<span class="nt">B</span> <span class="setm">c</span>"   →  FIRST(<span class="nt">B</span> <span class="setm">c</span>) − ε
   <span class="nt">S</span> → <span class="nt">A</span> <span class="nt">B</span> <span class="setm">c</span>   :  B 뒤 β = "<span class="setm">c</span>"     →  FIRST(<span class="setm">c</span>)   − ε = { <span class="setm">c</span> }
</pre>

`FIRST(B c)` 를 ⊕ 로: `FIRST(B)={b,ε}` 에 ε 있음 → `b` + 다음 칸 `FIRST(c)={c}` → `{ b, c }` (ε 없음, 멈춤). ε 안 섞였으니 그대로:

<pre class="lrbox">
   FOLLOW(<span class="nt">A</span>) ⊇ { <span class="setm">b</span>, <span class="setm">c</span> }
   FOLLOW(<span class="nt">B</span>) ⊇ { <span class="setm">c</span> }
</pre>

`A`·`B` 둘 다 *맨 끝* 이 아니고, 뒤(`B c`, `c`)가 ε 으로 다 사라지지도 않아요(끝에 단말 `c`) → 규칙 ③ 해당 없음.

<pre class="lrbox">
   FOLLOW(<span class="nt">S</span>) = { $ }
   FOLLOW(<span class="nt">A</span>) = { <span class="setm">b</span>, <span class="setm">c</span> }
   FOLLOW(<span class="nt">B</span>) = { <span class="setm">c</span> }
</pre>

> 🔖 **이 예제로 새로 본 것** — ⊕ 가 *nullable 두 칸* 을 연달아 건너뛰는 것(`FIRST(S)`), 그리고 FOLLOW ② 가 *단말 하나가 아니라 `FIRST(β)`(여러 기호의 묶음)* 를 재료로 쓰는 장면.

---

## 예제 3 — 진짜 빡센 것 (재귀 + ε + 상속) ★

본문의 `Expr` 문법을, 좌재귀 대신 **ε 으로 푼** 사촌이에요. 컴파일러 교과서의 단골 문법이고 — 규칙이 *전부* 한 번씩 돌아가요.

<pre class="lrbox">
<span class="nt">E</span>  → <span class="nt">T</span> <span class="nt">E'</span>
<span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> | ε
<span class="nt">T</span>  → <span class="nt">F</span> <span class="nt">T'</span>
<span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span> | ε
<span class="nt">F</span>  → <span class="setm">'('</span> <span class="nt">E</span> <span class="setm">')'</span> | <span class="setm">id</span>
</pre>

`E'` 와 `T'` 가 *꼬리* 예요 — 더 붙을 수도(`+ T E'`), 끝날 수도(ε) 있죠. 그래서 둘 다 nullable.

### FIRST

쉬운 것(끝쪽)부터 올라가요.

<pre class="lrbox">
   FIRST(<span class="nt">F</span>)  : <span class="nt">F</span> → <span class="setm">'('</span> <span class="nt">E</span> <span class="setm">')'</span>  → { <span class="setm">(</span> }
               <span class="nt">F</span> → <span class="setm">id</span>         → { <span class="setm">id</span> }              →  FIRST(<span class="nt">F</span>)  = { <span class="setm">(</span>, <span class="setm">id</span> }

   FIRST(<span class="nt">T'</span>) : <span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span>  → { <span class="setm">*</span> }
               <span class="nt">T'</span> → ε         → { ε }               →  FIRST(<span class="nt">T'</span>) = { <span class="setm">*</span>, ε }

   FIRST(<span class="nt">T</span>)  : <span class="nt">T</span> → <span class="nt">F</span> <span class="nt">T'</span>  =  FIRST(<span class="nt">F</span>) ⊕ FIRST(<span class="nt">T'</span>)
                         =  { <span class="setm">(</span>, <span class="setm">id</span> } (ε 없음, 멈춤)  →  FIRST(<span class="nt">T</span>)  = { <span class="setm">(</span>, <span class="setm">id</span> }

   FIRST(<span class="nt">E'</span>) : <span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> → { <span class="setm">+</span> }
               <span class="nt">E'</span> → ε        → { ε }                →  FIRST(<span class="nt">E'</span>) = { <span class="setm">+</span>, ε }

   FIRST(<span class="nt">E</span>)  : <span class="nt">E</span> → <span class="nt">T</span> <span class="nt">E'</span>  =  FIRST(<span class="nt">T</span>) ⊕ FIRST(<span class="nt">E'</span>)
                         =  { <span class="setm">(</span>, <span class="setm">id</span> } (ε 없음, 멈춤)  →  FIRST(<span class="nt">E</span>)  = { <span class="setm">(</span>, <span class="setm">id</span> }
</pre>

> `E` · `T` · `F` 의 FIRST 가 `{ (, id }` 로 같아요 — 본문 `Expr` 문법과 똑같은 답이죠. 모양만 바꿨지 *같은 언어* 니까요.

### FOLLOW

**초깃값 — ①② 부터.**

<pre class="lrbox">
   ① 시작 기호 <span class="nt">E</span>                →  FOLLOW(<span class="nt">E</span>) ⊇ { $ }
   ② <span class="nt">F</span> → <span class="setm">'('</span> <span class="nt">E</span> <span class="setm">')'</span> : E 뒤 ")"   →  FOLLOW(<span class="nt">E</span>) ⊇ FIRST("<span class="setm">)</span>") = { <span class="setm">)</span> }
</pre>

이제 **상속(③)** 이 필요한 자리를 찾아요. *nullable 꼬리* 때문에 ③ 이 곳곳에서 걸려요.

<pre class="lrbox">
   <span class="nt">E</span>  → <span class="nt">T</span> <span class="nt">E'</span>   : E' 가 맨 끝          →  FOLLOW(<span class="nt">E'</span>) ⊇ FOLLOW(<span class="nt">E</span>)
   <span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> : E' 가 맨 끝         →  FOLLOW(<span class="nt">E'</span>) ⊇ FOLLOW(<span class="nt">E'</span>)   (자기 자신, 새것 없음)
   <span class="nt">E</span>  → <span class="nt">T</span> <span class="nt">E'</span>   : T 뒤 β = "<span class="nt">E'</span>", 그런데 E' 는 nullable!
                   → ② FIRST(<span class="nt">E'</span>) − ε = { <span class="setm">+</span> }   그리고  ③ FOLLOW(<span class="nt">T</span>) ⊇ FOLLOW(<span class="nt">E</span>)
   <span class="nt">E'</span> → <span class="setm">'+'</span> <span class="nt">T</span> <span class="nt">E'</span> : T 뒤 "<span class="nt">E'</span>" — 위와 같음  →  { <span class="setm">+</span> },  FOLLOW(<span class="nt">T</span>) ⊇ FOLLOW(<span class="nt">E'</span>)
   <span class="nt">T</span>  → <span class="nt">F</span> <span class="nt">T'</span>   : T' 가 맨 끝          →  FOLLOW(<span class="nt">T'</span>) ⊇ FOLLOW(<span class="nt">T</span>)
   <span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span> : T' 가 맨 끝         →  FOLLOW(<span class="nt">T'</span>) ⊇ FOLLOW(<span class="nt">T'</span>)   (자기 자신)
   <span class="nt">T</span>  → <span class="nt">F</span> <span class="nt">T'</span>   : F 뒤 β = "<span class="nt">T'</span>", T' 도 nullable!
                   → ② FIRST(<span class="nt">T'</span>) − ε = { <span class="setm">*</span> }   그리고  ③ FOLLOW(<span class="nt">F</span>) ⊇ FOLLOW(<span class="nt">T</span>)
   <span class="nt">T'</span> → <span class="setm">'*'</span> <span class="nt">F</span> <span class="nt">T'</span> : F 뒤 "<span class="nt">T'</span>" — 위와 같음  →  { <span class="setm">*</span> },  FOLLOW(<span class="nt">F</span>) ⊇ FOLLOW(<span class="nt">T'</span>)
</pre>

**반복해서 굳히면** (`E'` 는 `E` 를, `T` 는 `E` 를, `T'`·`F` 는 `T` 를 물려받아요):

<pre class="lrbox">
   FOLLOW(<span class="nt">E</span>)  = { $, <span class="setm">)</span> }
   FOLLOW(<span class="nt">E'</span>) = { $, <span class="setm">)</span> }                  ← E 에서 상속
   FOLLOW(<span class="nt">T</span>)  = { <span class="setm">+</span> } ∪ FOLLOW(<span class="nt">E</span>)  = { <span class="setm">+</span>, $, <span class="setm">)</span> }
   FOLLOW(<span class="nt">T'</span>) = FOLLOW(<span class="nt">T</span>)          = { <span class="setm">+</span>, $, <span class="setm">)</span> }   ← T 에서 상속
   FOLLOW(<span class="nt">F</span>)  = { <span class="setm">*</span> } ∪ FOLLOW(<span class="nt">T</span>)  = { <span class="setm">*</span>, <span class="setm">+</span>, $, <span class="setm">)</span> }
</pre>

> 🔖 **이 예제로 새로 본 것** — ③ 상속이 *진짜로* 도는 모습. 특히 **`T` 뒤의 `E'` 가 nullable 이라, ② 의 `{+}` 뿐 아니라 ③ 으로 `FOLLOW(E)` 까지 함께 받는** 장면이 핵심이에요. ("뒤가 사라질 수 있으면 사실상 맨 끝" → ③.)

---

## 스스로 풀어보기

규칙이 손에 익었는지 — 직접 한 번 해보세요. (답은 아래 접어뒀어요.)

<pre class="lrbox">
<span class="nt">S</span> → <span class="nt">B</span> <span class="setm">a</span>
<span class="nt">B</span> → <span class="setm">b</span> | ε
</pre>

`FIRST(S)` · `FIRST(B)` · `FOLLOW(S)` · `FOLLOW(B)` 를 구해보세요.

<details>
<summary>답 보기</summary>

<pre class="lrbox">
   FIRST(<span class="nt">B</span>) = { <span class="setm">b</span>, ε }
   FIRST(<span class="nt">S</span>) = FIRST(<span class="nt">B</span>) ⊕ FIRST(<span class="setm">a</span>) = { <span class="setm">b</span> } ∪ { <span class="setm">a</span> } = { <span class="setm">a</span>, <span class="setm">b</span> }
              (B 가 nullable 이라 a 까지 넘어감)

   FOLLOW(<span class="nt">S</span>) = { $ }                          ① 시작 기호
   FOLLOW(<span class="nt">B</span>) = FIRST(<span class="setm">a</span>) − ε = { <span class="setm">a</span> }           ② B 뒤 "<span class="setm">a</span>"
              (B 는 맨 끝이 아니고 뒤가 단말 a 라 ③ 없음)
</pre>

</details>

---

## 다음

이 연습으로 FIRST/FOLLOW 가 *어떤 문법에서도* 어떻게 도는지 감이 잡혔을 거예요.\
이제 이 둘을 *재료* 로 쓰는 — **LR 파서의 파싱 테이블** 로 갑니다.

👉 **[LR 파서 — 파싱 테이블 만들기](lr-item.md)**

---

👈 앞으로: [FOLLOW · 구현](follow-impl.md)
