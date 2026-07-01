# Terminal — 더 쪼개지지 않는 잎

> 🎓 **심화 과정** 이에요.\
> 앞 장 [Symbol](deep-symbols.md)에서 모든 기호의 공통 뿌리를 봤죠.\
> 이제 그 두 갈래 중 더 단순한 쪽 — **잎(토큰)인 Terminal** 부터 봅니다.

`Terminal`은 입력에 진짜로 등장하는, 더 이상 쪼갤 수 없는 조각이에요.\
`+`, `*`, `(`, `id` 같은
거요.\
이걸 코드로 표현하려고 책상에 앉은 저자는 — **무엇부터 정했을까요?**

## 첫 고민 — "렉서가 맞춰볼 *값* 이 있어야지"

토큰의 본질은 "입력의 이 조각" 이에요.\
그러니 렉서가 입력과 **맞춰볼 실제 값** 이 가장 먼저
필요하죠.\
그게 `Value` 예요.

```csharp
public string Value { get; } = string.Empty;
```

> 저자에게 `Value`는 *"있으면 좋은 것"* 이 아니라 ***"반드시 줘야 하는 것"*** 이에요.\
> 맞춰볼 값이
> 없는 토큰은 존재 의미가 없으니까요.\
> 그래서 코드에서도 `Value`는 사용자가 준 값을 **그대로**
> 보관하고, 절대 비워 두지 않아요.

## 둘째 고민 — "근데 *보여줄 이름* 은 값과 다를 수 있잖아"

여기서 [Symbol 장](deep-symbols.md)의 **"정체성 ↔ 표시 분리"** 철학이 구체화돼요.\
저자는 이런
상황을 떠올렸을 거예요:

> *"`id` 토큰의 실제 값(Value)은 정규식 `[a-zA-Z]+` 인데, 화면엔 그냥 'id'라고 보여주고 싶어.\
> 값이랑 표시가 다른 거지.\
> 그러면 **표시용 이름을 따로** 두자."*

그래서 `Caption` 이 들어갔어요 (`ToString()`도 이걸 써요).

```csharp
public string Caption { get; } = string.Empty;
public override string ToString() => Caption;
```

그리고 — 이 부분은 *유추가 아니라, 저자가 코드 주석에 직접 적어둔* 내용이에요.

`Caption`은 표·진단·FIRST/FOLLOW 출력 등 **표시에 쓰여요.**\
그래서 null이면 텍스트 렌더링 오류가 날 수 있어요.

그래서 caption이 없으면 value로 보완해요.\
**하지만 `Value`는 손대지 않아요** — 렉서의 매칭 값이지
표시 문자열이 아니니까요.

```csharp
// caption이 null이면 value로 fallback (표시가 null이면 텍스트 렌더러가 깨지므로).
// 단, Value 자체는 준 그대로 — 렉서의 매칭 값/패턴이라 절대 바꾸지 않는다.
this.Caption = caption ?? value ?? string.Empty;
```

표시가 깨지는 건 fallback으로 막되, **정체성인 값은 그대로.**\
Symbol의 그 철학이 딱 한 줄에 담겨
있죠.

## 거드는 정보들 — 종류, 의미, 정규식

토큰 하나를 온전히 다루려면 몇 가지가 더 필요했어요.\
저자가 하나씩 덧붙인 것들이에요.

- **`TokenType`** — 이게 연산자냐, 구분자냐, 숫자냐, 키워드냐. (렉서가 어떻게 취급할지 갈려요.)
- **`Meaning`** — 이 토큰이 *의미 있는* 토큰인가. (나중에 AST를 만들 때 살릴지 버릴지의 기준.)
- **`RegexExpression`** — `Value`를 실제 렉서가 쓸 **정규식** 으로 바꿔줘요 (연산자/숫자/단어마다 다르게).

```csharp
public TokenType TokenType { get; }
public bool Meaning { get; } = true;
public string RegexExpression => (IsOper) ? ... : (IsNumber) ? Value : ... ;
```

## 그리고 — 잎이라서 *안이 비어 있어요*

`Terminal`이 `NonTerminal`과 결정적으로 다른 점 하나.\
**Terminal은 안에 "대안(alters)" 이
없어요.**\
더 풀어 쓸 게 없으니까요.\
말 그대로 **잎** 이에요.\
(다음 장의 NonTerminal은 정반대로,
안에 대안들을 잔뜩 품어요.)

## 특별한 잎 몇 개 — ε 와 $

마지막으로, 저자는 *진짜 입력엔 없지만 파싱엔 꼭 필요한* 가짜 토큰 몇 개를 `Terminal`의 자식으로
만들어 뒀어요.\
[FIRST/FOLLOW](first-follow.md)에서 만났던 바로 그것들이에요.

```csharp
public class Epsilon   : Terminal { ... }   // ε — "빈 것"
public class EndMarker : Terminal { ... }   // $ — "입력의 끝"
```

이들은 **고정된 UniqueKey** 를 가져요 (`KeyManager.EpsilonKey` 등).\
어디서 만들든 *언제나 같은
하나* 로 취급되도록요.\
(또 Symbol의 "정체성은 키" 철학이죠.)

## 한눈에 — Terminal의 전체 모습

`Terminal` 클래스의 **전체 골격** 이에요.\
로직은 비우고 *무엇이 있는지* 만 보여줘요.

```csharp
public class Terminal : Symbol
{
    // ── 무엇인가 ────────────────────────────
    public TokenType TokenType { get; }
    public string Value { get; }        // 렉서가 맞춰볼 값 (필수 · 건드리지 않음)
    public string Caption { get; }       // 표시 이름 (ToString 이 이걸 씀)
    public bool Meaning { get; }         // 의미 있는 토큰인가 (AST 용)
    public bool IsWordPattern { get; }

    // ── 파생 정보 ───────────────────────────
    public bool IsOper { get; }          // 연산자/구분자류인가
    public bool IsNumber { get; }
    public string RegexExpression { get; }   // Value → 실제 렉서용 정규식

    // ── 생성 ────────────────────────────────
    public Terminal(TokenType type, string value, bool meaning = true, bool bWord = false);
    public Terminal(TokenType type, string value, string caption, ...);

    // ── 표현 ────────────────────────────────
    public override string ToString();   // → Caption
    public override string ToEbnfString(bool bContainLHS = false);
    public override string ToGrammarString();
    public override string ToTreeString(ushort depth = 1);
}

// 특수한 잎들 — 전부 Terminal 의 자식, 고정 UniqueKey
public class Epsilon        : Terminal { }   // ε — 빈 것
public class EndMarker      : Terminal { }   // $ — 입력의 끝
public class InputTerminal  : Terminal { }
public class NotDefined     : Terminal { }
public class CustomTerminal : Terminal { }
```

## 다음 장

`Terminal` — 잎 하나를 표현하려고 저자가 **무엇을, 왜** 넣었는지 봤어요 (값 vs 표시, 종류, 그리고
ε·$ 같은 특수 잎까지).\
이제 반대편 — 안에 대안들을 품는 **가지, NonTerminal** 로 갑니다.

👉 **[NonTerminal — 안에 규칙을 품는 가지](deep-nonterminal.md)**
