# Symbol — 문법의 가장 작은 단위

> 🎓 여기는 **심화 과정** 이에요. 기본 과정이 *개념* 이었다면, 심화 과정은 **그 개념을 Orchid가
> 코드로 어떻게 쌓아 올렸는지** 를 — 그것도 **만든 순서 그대로, 천천히** — 따라가요.
>
> 한 가지 약속하고 갈게요. 이 심화 과정에서 **"저자"** 라고 하면, Orchid를 직접 설계하고 코드를
> 짠 **사람**(이 프로젝트의 주인)을 가리켜요. *지금 이 글을 정리하는 AI가 아니라요.*
>
> 그리고 솔직히 말하면, 여기 담은 "저자의 생각" 은 **두 가지가 섞여** 있어요:
>
> - **일부는 저자에게 직접 물어봐서** 그 의도를 그대로 담았어요. (다만 *모든* 결정을 다 여쭤본 건 아니에요.)
> - **나머지 많은 부분은 코드를 보고 "아마 이런 판단을 했겠다" 하고 추론** 한 거예요.
>
> 그래서 *"이건 저자 본인의 말, 저건 코드에서 읽어낸 추측"* 으로 딱 잘라 나뉘진 않아요 — 둘이
> 자연스럽게 섞였다고 보시면 돼요. 그래도 걱정 마세요, **코드가 생각보다 많은 걸 말해주거든요.** 🙂

## 저자의 설계 출발점 — "구체가 보이면, 추상부터 잡는다"

저자에겐 설계할 때 일관된 습관이 하나 있어요.

**서로 다른 구체적인 것들이 보이면, 그들을 하나로 묶는 *추상 클래스* 부터 잡는다.**

왜냐면 — **그래야 그 구체들의 *공통 부분* 을 한곳에 모아 묶어낼 수 있거든요.** (공통 동작·공통
데이터를 추상 베이스에 한 번만 두면, 구체 클래스들은 그걸 그냥 물려받으면 되죠.)

`Symbol`이 바로 그렇게 태어났어요.

문법에는 성격이 분명히 다른 두 가지가 있죠 — **Terminal(단말, 토큰)** 과 **NonTerminal(비단말,
규칙)**.\
저자는 이 두 *구체 클래스* 를 보고 이렇게 판단했을 거예요:

> *"이 둘은 다르지만, 결국 '문법에 등장하는 기호' 라는 점에선 한 식구야. 그러면 이 둘을 **추상화한
> 공통 클래스** 를 먼저 잡자."*

그렇게 **처음부터 추상 클래스(`abstract`)로 설계된** 게 `Symbol` 이에요.\
그래서 `Symbol`은 혼자선
못 태어나요 (`new Symbol()` 불가능) — 반드시 Terminal이나 NonTerminal 중 하나로 **구체화돼야**
하죠.

> 📍 **`Symbol`** · 모듈 `Parse.FrontEnd` (Layer 2) · `src/FrontEnd/Parse.FrontEnd/RegularGrammar/Symbol.cs`

```csharp
public abstract class Symbol : IShowable, IQuantifiable, IConvertableEbnfString
{
    // 두 구체(Terminal · NonTerminal)를 추상화한 공통 베이스
}
```

```
        Symbol  (추상 — 두 구체의 공통 추상)
        ├── Terminal      ← 잎: 더 안 쪼개지는 토큰   (다음 장)
        └── NonTerminal   ← 가지: 더 쪼개지는 규칙   (그다음 장)
```

> 💡 **이 습관은 매뉴얼 내내 또 나와요.** 앞으로 *구체 클래스 여럿이 보이면, 그 위에 그들을
> 추상화한 클래스가 있겠거니* 하고 보시면 저자의 머릿속을 따라가기 한결 쉬워요.

## 첫 번째 공통 부분 — 정체성 (UniqueKey)

추상 베이스를 잡았으니, 이제 거기에 *묶어낼 공통 부분* 을 하나씩 채울 차례예요.\
첫째는 —
**"두 기호가 같은지 다른지를 무엇으로 판단하지?"**, 즉 *정체성* 이에요. (Terminal이든
NonTerminal이든 똑같이 필요한 거죠 — 그래서 공통.) 파서는 끊임없이 "이 심볼 = 저 심볼?" 을
따져야 하거든요.

가장 쉬운 답은 *이름으로 비교* 하는 거예요.\
그런데 — 저자는 여기서 한 발 더 들어갔을 거예요:

> *"이름(화면에 보이는 표시)이 나중에 바뀌면? 예를 들어 `+` 의 표시를 'plus'로 바꾸면? 그럼
> 같은 기호인데 갑자기 달라 보이잖아. 그러면 파싱이 통째로 흔들릴 텐데... 안 되겠다. **'보이는
> 이름' 과 '진짜 정체성' 을 분리하자.**"*

그래서 들어간 게 **`UniqueKey`** 예요.\
표시 이름과는 *완전히 별개* 인, 숫자로 된 고유 식별자죠.

```csharp
public UInt32 UniqueKey { get; internal set; } = UInt32.MaxValue;

public override int GetHashCode() => (int)this.UniqueKey;   // 해시도
// == 도, Equals 도 — 전부 UniqueKey 로만 비교
```

같음 판정(`==`)도, 해시도, **오직 `UniqueKey` 로만** 해요.

덕분에 표시 이름을 아무리 바꿔도, 파서 입장에선 **같은 기호는 영원히 같은 기호** 예요.

작아 보이지만 — *컴파일러처럼 작은 실수가 크게 번지는 프로젝트* 에서 이런 분리는 큰 안전장치예요.

> (이 "정체성 ↔ 표시" 분리는 다음 [Terminal](deep-terminal.md) 장에서 `Value`/`Caption` 으로 또
> 구체화돼요.)

## 또 다른 공통 부분 — 연산자와 수량자

마지막으로, 우리가 C#으로 문법을 적을 때 `Expr + plus + Term` 이나 `... | Term` 처럼 쓰잖아요?\
이때 쓰는 **`+`(잇기)·`|`(고르기) 연산자**, 그리고 `?`·`*`·`+`(수량자) 가 — 어디에 있어야
할까요?

> 저자의 판단: *"이건 Terminal이든 NonTerminal이든 **아무 심볼에나** 쓸 수 있어야 해. 그러면
> 둘의 공통 추상인 `Symbol`에 두는 게 맞지."*

그래서 이 연산자·수량자도 전부 `Symbol`에 있어요. (추상 베이스에 공통 동작을 모아두는 거죠.)

```csharp
public static NonTerminal operator +(Symbol left, Symbol right);   // 잇기(연접)
public static NonTerminal operator |(Symbol left, Symbol right);   // 고르기(택일)
// ?(Optional) · *(ZeroOrMore) · +(OneOrMore) 도 Symbol 에 (IQuantifiable)
```

지금은 *"여기 있구나"* 만 — 이게 실제로 **어떤 구조를 만들어내는지** 는 뒤에서 한 장씩 천천히
파헤칠 거예요.

## 📐 저자의 설계 다이어그램

저자가 이 부분을 설계할 때 그린 다이어그램이에요 (코드 주석에도 같은 링크가 박혀 있어요).\
같이
보면 머릿속 그림이 또렷해져요.

- Symbol과 연산자 설계 — <https://www.lucidchart.com/documents/edit/d05e9c87-a3ab-4b64-8a75-c6b84c28aa45/0>

> (저자 본인의 설계 노트라, 열람 권한이 필요할 수 있어요.)

## 한눈에 — Symbol의 전체 모습

부분부분 봤으니, 이제 `Symbol` 클래스의 **전체 골격** 을 한 번에 볼게요.

로직은 비우고, *무엇이 들어 있는지* 만요. ("아, 이렇게 생겼구나" 하시라고요.)

```csharp
public abstract class Symbol : IShowable, IQuantifiable, IConvertableEbnfString
{
    // ── 정체성 ──────────────────────────────
    public UInt32 UniqueKey { get; internal set; }
    protected string EbnfString { get; set; }

    // ── 표현 (자식이 채움) ───────────────────
    public abstract string ToEbnfString(bool bContainLHS = false);
    public abstract string ToGrammarString();
    public abstract string ToTreeString(ushort depth = 1);

    // ── 같음 (전부 UniqueKey 기준) ───────────
    public bool Equals(Symbol other);
    public override int GetHashCode();
    public static bool operator ==(Symbol left, Symbol right);
    public static bool operator !=(Symbol left, Symbol right);

    // ── 잇기 / 고르기 ────────────────────────
    public static NonTerminal operator +(Symbol left, Symbol right);   // 연접: a 다음 b
    public static NonTerminal operator |(Symbol left, Symbol right);   // 택일: a 또는 b

    // ── 수량자 (IQuantifiable) ───────────────
    public NonTerminal ZeroOrMore();   // *
    public NonTerminal OneOrMore();    // +
    public NonTerminal Optional();     // ?
}
```

크지 않죠? **정체성 · 표현 · 같음 · 잇기/고르기 · 수량자** — 딱 이 다섯 묶음이에요.

## 다음 장

`Symbol`을 봤어요 — *왜* 처음부터 추상으로 잡았는지(두 구체의 추상화), *왜* 정체성을 이름이 아닌
키로 뒀는지까지.\
이제 그 두 갈래 중 더 단순한 쪽 — **잎(토큰)인 Terminal** 부터 들여다봐요.

👉 **[Terminal — 더 쪼개지지 않는 잎](deep-terminal.md)**
