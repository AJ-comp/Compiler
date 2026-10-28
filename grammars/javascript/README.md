# ECMAScript 2026 표현식·함수·문장·클래스·모듈 문법

`JavaScriptGrammar` 하나로 JavaScript 문법을 준비합니다. 기본 입력 종류는 선언과 문장 목록으로 구성된 Script입니다. 모듈 파일이나 표현식 하나를 읽을 때에는 생성자의 `inputKind`로 Module 또는 Expression을 선택합니다. 이후에는 입력 종류에 관계없이 `language.Grammar`로 `LALRParser`를 만들고, `language.CreateTokenSource(input)`을 파싱한 뒤 `language.Validate(tree)`로 정적 문법 오류를 검사합니다.

표현식·템플릿·구조 분해 할당은 [표현식 문서](../../docs/ko/javascript-expressions.html)에, 일반 함수·화살표 함수·문장·선언과 매개변수의 구조 분해는 [함수·문장 문서](../../docs/ko/javascript-statements.html)에 사용 예제와 구현 이유를 기록했습니다. 클래스 선언·표현식과 일반 메서드·접근자·필드·private 이름·static 블록은 [클래스 문서](../../docs/ko/javascript-classes.html)에서 설명합니다. Module 입력과 import·export, 동적 import·import.meta·top-level await는 [모듈 문서](../../docs/ko/javascript-modules.html)에 순서대로 정리했습니다. async 함수·화살표, 제너레이터·async 제너레이터와 객체·클래스 메서드는 [함수 종류별 문서](../../docs/ko/javascript-functions.html)에서 표기와 본문 경계를 차례로 설명합니다. 비엄격 모드의 선택, 코드 구간별 엄격 모드와 별도 웹 호환 옵션은 [모드·웹 호환 문서](../../docs/ko/javascript-non-strict.html)에 정리했습니다.

## 입력 문맥과 지원 범위

기준은 [ECMAScript 2026, 제17판](https://tc39.es/ecma262/2026/multipage/ecmascript-language-expressions.html)입니다. `new JavaScriptGrammar()`는 **엄격 모드 Script**를 읽습니다. Script와 Expression 입력은 `JavaScriptMode.NonStrict`로 비엄격 모드를 선택할 수 있습니다. Module 입력은 항상 엄격 모드인 **Module 문맥**을 사용합니다. `webCompatibility`는 이 모드와 독립적인 웹 호환 옵션이며 기본값은 `false`입니다.

| `JavaScriptInputKind` | 입력과 검사 기준 |
| --- | --- |
| `Script` | 기본값. 변수·함수·클래스 선언과 여러 문장을 읽습니다. 처음 적용할 엄격 모드를 선택할 수 있습니다. |
| `Module` | import·export를 포함할 수 있는 모듈 전체를 읽습니다. 모듈의 시작 문법과 엄격 모드 검사를 적용합니다. |
| `Expression` | `price * count` 같은 표현식 하나를 읽습니다. Script 문맥에서 처음 적용할 엄격 모드를 선택할 수 있습니다. |

선택한 종류는 `language.InputKind`에 보관합니다. 클래스가 통합되어도 내부에서는 입력 종류에 맞는 시작 규칙과 검사를 사용합니다. 예를 들어 Module의 `export`와 비엄격 Script의 `with`를 한 입력에서 함께 허용하지 않습니다. 생성자에서 선택한 입력 종류의 기준을 파싱과 정적 검사 전체에 적용합니다.

어떤 입력 종류를 선택하더라도 일반·async 함수와 화살표 함수, 제너레이터·async 제너레이터, 객체·클래스 메서드의 매개변수·본문을 함께 파싱합니다. Script 최상위와 일반 함수 본문에서는 문맥에 따라 `await`를 식별자로 사용할 수 있지만, Module 문맥에서는 중첩된 일반 함수까지 `await`를 예약어로 검사합니다. `yield`·`let` 등은 실제 코드 구간의 엄격 모드와 함수 종류에 맞춰 검사합니다. 속성 이름에는 예약어를 사용할 수 있습니다.

| 구문 | 지원 내용 |
| --- | --- |
| 기본 값과 토큰 | Unicode 식별자·이스케이프, 숫자·BigInt·문자열·정규식 리터럴, `true`·`false`·`null`·`this`, 괄호식 |
| 연산자 | 단항·증감·거듭제곱·산술·시프트·비교·동등·비트·논리·null 병합, 조건식, 대입·복합 대입·논리 대입, 쉼표 |
| 속성과 호출 | 점·대괄호 속성 접근, 함수 호출·인자 펼치기·뒤쪽 쉼표, `new`, 선택적 체이닝·선택적 호출 |
| 배열 | 요소·빈 요소·뒤쪽 쉼표·펼치기 |
| 객체 | 이름과 값·축약 속성·계산된 속성 이름·펼치기·뒤쪽 쉼표, 일반·async·제너레이터 메서드와 getter·setter |
| 템플릿 | 일반·태그 템플릿, 여러 보간 표현식, 중첩 템플릿, 줄바꿈·이스케이프, 보간 안의 객체·정규식·나눗셈 |
| 구조 분해 할당 | 배열·객체 패턴, 중첩·기본값, 빈 배열 요소, rest 대상과 위치 제한, 속성 대입 대상 |
| 일반 함수 | 함수 선언·함수 표현식, 매개변수의 기본값·구조 분해·rest, 본문의 선언·문장·return |
| 화살표 함수 | 일반·async, 단일·괄호·빈 매개변수 목록, 기본값·구조 분해·rest, 표현식 본문·블록 본문과 중첩 |
| async·제너레이터 | 함수 선언·표현식, async·제너레이터·async 제너레이터 메서드, 종류별 await·yield·yield*와 for-await-of |
| 클래스 | 선언·표현식·extends, 일반 생성자·일반/async/제너레이터 메서드·get/set, 인스턴스·static·private 필드, static 블록, private 접근·brand 검사, super·new.target 문맥 |
| 바인딩과 선언 | `var`·`let`·`const`, 배열·객체 바인딩 패턴과 중첩·기본값·rest, for·catch의 바인딩 |
| 문장 | 블록·빈 문장·표현식 문장, if·else, while·do/while·for·for/in·for/of, break·continue·레이블, switch, throw·try·catch·finally, debugger |
| 모듈 | default·namespace·named·side-effect import, 선언·목록·default export, 재내보내기, 문자열 공개 이름과 `with` 속성 |
| 모듈 관련 표현식 | 모든 입력의 동적 import, Module 입력의 `import.meta`·top-level await·for-await-of와 본문 문맥 검사 |
| 문장 경계 | 명시적 세미콜론, ASI의 줄바꿈·닫는 중괄호·입력 끝 조건과 제한 구문 |
| 모드 | Strict·NonStrict Script와 표현식, 직접·상속된 use-strict 지시문, 클래스·Module의 엄격 모드와 계산된 메서드 이름의 바깥 문맥 |
| 웹 호환 옵션 | Script HTML 주석, Annex B.3.1~B.3.5·B.3.9의 문법과 정적 검사 예외. 아래에 항목별 범위를 정리합니다. |
| 정적 검사 | 모드·함수 문맥별 예약어·`eval`·`arguments`·`delete`, 대입·증감 대상, 구형 리터럴의 엄격 구간 사용, 중복 `__proto__` 설정, 일반 템플릿의 잘못된 이스케이프 |

`inputKind: JavaScriptInputKind.Expression`은 표현식 하나를 읽습니다. 예를 들어 `result = a / b`, `function (value) { return value; }`, `value => value + 1`을 각각 입력합니다. 기본값인 Script에는 세미콜론으로 끝나는 표현식 문장, 변수·함수 선언과 여러 문장을 함께 전달할 수 있습니다.

정규식은 리터럴 경계와 플래그에 이어, `Validate`에서 본문의 `RegExpPattern` 문법까지 검사합니다. 일반·u·v 모드의 그룹·반복·역참조·Unicode 속성과 집합 조건을 적용합니다. 비엄격·웹 호환 문법과 async·제너레이터·객체 메서드도 같은 Parsing → Validate 순서로 검사합니다. 이 구문·정적 검사 범위의 명세 대조와 공식 테스트 결과는 [ECMAScript 2026 검증 보고서](../../docs/ko/javascript-conformance.html)에 기록합니다.

## C# 사용 순서

```csharp
using System;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new JavaScriptGrammar();
var parser = new LALRParser(language.Grammar);

var source = language.CreateTokenSource("const result = /ab/g.test(value);");
var parsed = parser.Parsing(source);
if (!parsed.Success)
{
    Console.WriteLine(parser.FiredException?.Message);
    return;
}

var diagnostics = language.Validate(parsed.ToParseTree);
if (diagnostics.Count > 0)
{
    foreach (var diagnostic in diagnostics)
        Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message} " +
            $"(위치 {diagnostic.SourceOffset}, 길이 {diagnostic.Length})");
    return;
}

var tree = parsed.ToParseTree;
Console.WriteLine(tree.AllInputDatas);
```

위 코드는 기본값인 Script를 검사합니다. 표현식 하나만 따로 읽으려면 같은 클래스의 `inputKind`를 Expression으로 지정합니다. 파싱과 검사 순서는 같습니다.

```csharp
var language = new JavaScriptGrammar(inputKind: JavaScriptInputKind.Expression);
var parser = new LALRParser(language.Grammar);
var input = "value => value + 1";
var parsed = parser.Parsing(language.CreateTokenSource(input));
// 이후에도 parsed.Success와 language.Validate(parsed.ToParseTree)를 함께 확인합니다.
```

**파싱 성공과 정적 검사 성공을 함께 확인해야 합니다.** 파싱은 구문 트리를 만들고, `Validate`는 `(a + b) = value`처럼 그 구조의 사용 조건을 어긴 오류를 찾습니다. Script 입력에는 선언 이름의 중복·충돌, 잘못된 return·break·continue와 레이블, switch의 중복 default 검사도 적용합니다. 반환하는 `JavaScriptDiagnostic`에는 `Code`, `Message`, `SourceOffset`, `Length`가 있습니다. 위치와 길이는 .NET 문자열의 UTF-16 코드 단위이며, 오류 범위는 `input.Substring(SourceOffset, Length)`로 가져올 수 있습니다.

`CreateTokenSource`는 원문과 `Policy`를 연결합니다. 파서는 현재 파싱 상태와 입력 커서 등의 문맥을 정책에 전달하고, 정책이 반환한 규칙 묶음으로 `source`에 다음 토큰을 요청합니다. `source`는 그 묶음 안에서 실제 입력을 가장 길게 읽는 규칙을 사용하며 동률이면 규칙 우선순위를 적용합니다. 정규식에 단어 경계 `\b`를 자동으로 덧붙이지 않습니다.

문장 문법에서는 호출 괄호와 if 조건 괄호가 닫힌 뒤의 문맥도 구분해야 합니다. 정책은 파싱 테이블의 후보와 직전 토큰에 연결된 JavaScript 문맥을 살펴 `call(value) / divisor`의 나눗셈과 `if (ready) /ab/.test(text);`의 정규식을 구분합니다. 파서는 그 판단을 직접 수행하지 않고 정책의 반환값으로 다음 토큰을 요청합니다. [슬래시 문맥 설명](../../docs/ko/javascript-statements.html#slash)에 이 책임과 호출 순서를 기록했습니다.

## 비엄격 모드와 웹 호환 옵션

비엄격 Script는 다음처럼 준비합니다. 아래 C# 코드의 `input`이 실제 JavaScript 원문이며, 이후에도 위의 `Parsing`과 `Validate`를 모두 호출합니다.

```csharp
var language = new JavaScriptGrammar(JavaScriptMode.NonStrict);
var input = "var value = 077; function read(item, item) { return item; }";
```

표현식 하나에는 `new JavaScriptGrammar(mode: JavaScriptMode.NonStrict, inputKind: JavaScriptInputKind.Expression)`를 사용합니다. 오래된 웹 입력에 필요한 호환 문법은 별도로 켭니다.

```csharp
var script = new JavaScriptGrammar(
    mode: JavaScriptMode.NonStrict,
    webCompatibility: true);

var expression = new JavaScriptGrammar(
    mode: JavaScriptMode.NonStrict,
    webCompatibility: true,
    inputKind: JavaScriptInputKind.Expression);

var module = new JavaScriptGrammar(
    webCompatibility: true,
    inputKind: JavaScriptInputKind.Module); // Mode는 항상 Strict
```

**Mode는 처음 적용할 엄격 모드를, WebCompatibility는 웹 호환 규칙의 사용 여부를 정합니다.** 기본 생성자의 설정은 Strict·웹 호환 끄기입니다. HTML 주석은 웹 호환 옵션을 켠 엄격 Script에도 허용하지만 Module에는 허용하지 않습니다. 단순 catch 매개변수와 같은 이름의 `var`를 허용하는 예외는 웹 호환 옵션을 켠 엄격 Script와 Module에도 적용합니다. 이 차이를 표현하기 위해 두 설정을 분리합니다.

### 기본 NonStrict 모드

웹 호환 옵션을 끈 비엄격 코드에서도 구형 숫자 `077`·`08.5`, 문자열의 `\141`·`\8`, `with` 문, 문맥에 맞는 엄격 전용 예약어의 식별자 사용, `eval`·`arguments` 바인딩·대입, 식별자 `delete`를 허용합니다.

중복 매개변수는 단순 식별자만 받는 비엄격 함수 선언·함수 표현식에 허용하며, 일반·async·제너레이터·async 제너레이터 함수가 모두 이 조건을 사용합니다. 화살표 함수와 객체·클래스 메서드는 모드와 관계없이 중복 매개변수를 금지합니다. 기본값·구조 분해·rest가 있는 비단순 목록도 모든 함수 종류에서 중복 이름을 금지합니다.

비엄격으로 시작한 입력에서도 Script·함수의 실제 `"use strict"` 지시문, 포함된 엄격 함수, 클래스와 Module은 해당 코드 구간을 엄격 모드로 만듭니다. 함수의 지시문은 그 함수의 매개변수와 본문 전체에 적용하며, 지시문 영역에서 앞서 나온 문자열도 검사합니다. 일반 블록의 문자열, 괄호로 감싼 `("use strict")`, 이스케이프가 있는 `"use\x20strict"`는 엄격 모드 지시문으로 취급하지 않습니다.

객체 메서드의 계산된 이름은 바깥 표현식의 모드를 사용하고, 매개변수·본문은 그 메서드의 모드를 사용합니다. 클래스는 그 자체가 엄격 모드이므로 계산된 클래스 멤버 이름도 클래스의 엄격 조건을 받습니다. 한 함수에서 선택한 엄격 모드는 바깥 비엄격 코드나 별도 형제 함수로 번지지 않습니다.

토큰 공급자는 비엄격 입력의 구형 리터럴을 먼저 원문 그대로 읽습니다. 최종 트리에서 `JavaScriptStrictness`가 각 구간의 엄격 모드를 정한 뒤, `Validate`가 금지된 위치의 리터럴을 검사합니다. 첫 문자열 뒤에 `"use strict"`가 나오는 경우처럼 토큰 하나만으로는 사용 조건을 확정할 수 없기 때문입니다. 진단 `strict_legacy_numeric_literal`은 숫자 토큰 전체를, `strict_legacy_string_escape`는 문자열 안의 첫 금지 이스케이프를 가리킵니다. 잘못된 Unicode·정규식 플래그와 `00n`·`08_1` 같은 잘못된 숫자는 모드와 관계없이 검사합니다.

### WebCompatibility가 추가하는 범위

| 항목 | 문법·정적 검사 범위 |
| --- | --- |
| B.1.1 | Script의 HTML 주석. `<!--`는 줄 끝까지, `-->`는 줄 시작 조건에서 읽습니다. 엄격 Script에도 허용하며 Module에는 금지합니다. |
| B.3.1 | 비엄격 코드의 레이블이 붙은 일반 함수 선언 |
| B.3.2 | 비엄격 블록·switch 범위의 일반 함수 선언끼리 중복 이름을 허용하는 정적 예외 |
| B.3.3 | 비엄격 코드의 if·else 바로 뒤에 적는 일반 함수 선언 |
| B.3.4 | 단순 catch 식별자와 같은 이름의 var 선언. 엄격 Script·Module에도 적용하며 구조 분해 catch에는 기존 충돌 조건을 유지합니다. |
| B.3.5 | 비엄격 `for (var key = 0 in object)` 형태. 식별자 하나의 var에 적용하며 for-of·구조 분해에는 확장하지 않습니다. |
| B.3.9 | 비엄격 일반 호출의 대입·증감·반복 대상 허용. 논리 대입·구조 분해·선택적 호출 등의 대상에는 기존 조건을 유지합니다. |

블록 함수의 실행 시 바인딩 생성과 호이스팅은 JavaScript 실행 환경의 역할입니다. `call() = value` 같은 호환 형태도 JavaScript에서 평가하면 `ReferenceError`가 될 수 있는 구문을 정적 단계에서 표현하는 범위입니다. Janglim은 입력을 실행하는 대신 구문 트리와 정적 진단을 반환합니다. 옵션별 예제와 이 구분의 이유는 [비엄격·웹 호환 설명](../../docs/ko/javascript-non-strict.html)에 있습니다.

## 템플릿과 보간 표현식

다음 표현식에서는 문자열 구간과 `${...}` 안의 보간 표현식을 번갈아 읽습니다.

```javascript
`안녕하세요, ${name}님!`
```

`source`는 먼저 여는 백틱부터 `${`까지를 `TemplateHead` 토큰으로 반환합니다. 파서는 `name`을 표현식으로 파싱하고, 이어서 `source`가 `}님!`과 닫는 백틱을 `TemplateTail` 토큰 하나로 반환합니다. 보간이 여러 개라면 중간 문자열 구간에 `TemplateMiddle`을 사용하고, 보간이 전혀 없으면 `NoSubstitutionTemplate` 하나로 읽습니다.

보간 안에는 객체나 다른 템플릿도 넣을 수 있습니다.

```javascript
`결과: ${{value: a / b}.value}`
`바깥: ${`안쪽: ${name}`}`
```

첫 번째 예제에서 `a / b` 뒤의 `}`는 객체를 닫고, `.value` 뒤의 `}`는 보간을 끝냅니다. 이 차이를 기억하기 위해 `source`는 다음 커서의 `LexicalState`에 보간마다 중괄호 깊이를 기록합니다. 정책은 커서의 보간 문맥으로 일반 `}`를 읽을지 템플릿 문자열을 이어 읽을지 정하고, 파싱 상태에서 기대하는 토큰으로 Div·RegExp 계열을 정합니다. 이 두 구분을 조합한 네 규칙 묶음을 미리 등록합니다.

두 번째 예제에서는 안쪽 템플릿의 보간 문맥을 별도로 추가했다가, 안쪽 템플릿이 끝나면 바깥 문맥으로 돌아갑니다. 문자열·정규식·주석 안의 중괄호는 보간 깊이에 영향을 주지 않습니다. 파서는 Shift할 때 다음 커서를 채택하며, 예측 후보도 각자 커서를 가지므로 서로의 보간 문맥을 바꾸지 않습니다.

태그가 붙은 템플릿은 잘못된 이스케이프도 원문으로 표현할 수 있습니다. 예를 들어 일반 템플릿 `` `\u{110000}` ``에는 `Validate`가 `invalid_template_escape` 진단을 반환하지만, 태그 템플릿 `` tag`\u{110000}` ``에는 이 진단을 반환하지 않습니다. 이 허용은 태그에 직접 연결된 템플릿의 문자열 구간에만 적용됩니다. 보간 안에 중첩한 일반 템플릿은 그 자체의 이스케이프 검사를 받으므로 **태그 템플릿을 파싱한 뒤에도 `Validate`를 호출합니다.**

각 템플릿 토큰의 `Input`에는 구분자·이스케이프·줄바꿈을 포함한 원문이 그대로 남습니다. 이 단계는 이스케이프를 해석한 문자열 값이나 태그 함수의 실행 결과를 계산하지 않습니다. 예제와 규칙 묶음별 흐름은 [템플릿 설명](../../docs/ko/javascript-expressions.html#templates)에 있습니다.

## 구조 분해 할당

일반 대입 연산자 `=`의 왼쪽에 배열·객체 패턴을 사용하면, 요소나 속성을 각각의 대상에 대입할 수 있습니다. 다음 각 줄은 독립적인 표현식 입력입니다.

```javascript
[first, second] = values
({name: displayName, age} = user)
({profile: {name}, scores: [first]} = user)
({name = 'guest'} = user)
[first, ...remaining] = values
({name, ...other} = user)
```

첫 번째 표현식은 첫 요소를 `first`에, 두 번째 요소를 `second`에 대입합니다. 두 번째 표현식은 `user.name`을 `displayName`에, `user.age`를 `age`에 대입합니다. 세 번째 표현식처럼 내부 배열·객체에 패턴을 다시 적용하거나, 네 번째 표현식처럼 꺼낸 값이 `undefined`일 때 사용할 기본값도 지정할 수 있습니다.

마지막 두 표현식의 `...`는 남은 요소나 속성을 받는 rest 대상입니다. **rest 대상은 마지막에 와야 하며, 뒤에 쉼표나 rest 자체의 기본값을 붙일 수 없습니다.** 따라서 `[...rest,] = values`와 `[...rest = fallback] = values`는 정적 오류입니다. 배열 rest는 `[...[first]] = values`처럼 중첩 패턴도 허용합니다. 객체 rest는 식별자 또는 대입 가능한 속성을 대상으로 받으므로 `({...target.other} = user)`는 허용하지만, `({...{name}} = user)`는 거부합니다.

패턴은 일반 대입 `=`에 사용합니다. `[first] += values`와 `[first]++`는 오류입니다. `[target.name] = values`는 허용하고, `[1] = values`와 `[target?.name] = values`는 거부합니다. `[eval] = values`는 해당 위치가 엄격 모드일 때 거부하며, 비엄격 대입 패턴에는 그 모드의 이름 조건을 적용합니다.

이 사용 조건은 파싱 후 `Validate`가 검사합니다. EBNF의 `ArrayLiteral`·`ObjectLiteral`은 배열·객체를 만드는 표현식과 대입 패턴의 공통 구문을 담으며, 객체의 축약 기본값에는 `CoverInitializedName`을 사용합니다. `Validate`는 실제 패턴에 해당하는 노드만 구분해 검사하고 **구문 트리의 노드·원문을 변경하지 않습니다.**

예를 들어 `({name = 'guest'} = user)`에서는 축약 기본값이 허용되지만, `({name = 'guest'})`는 일반 객체 리터럴이므로 오류입니다. 기본값·계산된 속성 이름·속성 접근의 기반 표현식에도 일반 표현식 검사를 계속 적용합니다. 따라서 `({name = {key = 1}} = user)`도 오른쪽 객체의 잘못된 속성 표기 때문에 거부합니다. 중복 `__proto__` 제한도 실제 객체 리터럴에 계속 적용하며, 실제 대입 패턴인 `({__proto__: a, __proto__: b} = user)`는 허용합니다.

여기서 설명한 구문은 기존 대상에 값을 대입하는 표현식입니다. 변수 선언과 함수 매개변수에서 새 이름을 만드는 바인딩 패턴은 별도의 EBNF 규칙으로 구분합니다. 괄호와 검사 경계를 포함한 자세한 예제는 [구조 분해 할당 설명](../../docs/ko/javascript-expressions.html#destructuring)에 있습니다.

## 화살표 함수

화살표 함수는 매개변수와 본문을 `=>`로 연결합니다. 식별자 하나는 괄호 없이 적을 수 있고, 빈 목록·여러 매개변수·기본값·구조 분해·rest는 괄호 안에 적습니다.

```javascript
const increment = value => value + 1;
const empty = () => 0;
const pick = ({name = 'guest'}, [first, ...rest]) => name;
const collect = (...[first, second]) => first;
```

본문에 표현식 하나를 적으면 그 결과를 반환하는 형태입니다. 중괄호로 감싼 본문에는 선언과 문장이 들어가며, 값을 반환할 때 `return`을 적습니다. 객체를 바로 반환할 때는 `value => ({name: value})`처럼 객체 리터럴을 괄호로 감쌉니다. `=>` 바로 뒤의 `{`는 블록 본문을 시작하기 때문입니다.

매개변수와 `=>` 사이에는 줄바꿈을 넣을 수 없습니다. 화살표 함수는 모드와 관계없이 중복 매개변수 이름과 본문의 직접 lexical 선언에 겹치는 매개변수를 거부합니다. `eval`·`arguments` 바인딩에는 해당 함수의 엄격 모드 조건을 적용합니다. 기본값·구조 분해·rest가 있는 매개변수와 함수 자신의 `use strict` 지시문 조합도 오류입니다. rest는 마지막에 오며 뒤쪽 쉼표나 rest 자체의 기본값을 허용하지 않습니다.

`(value)`는 괄호식과 매개변수 목록이 같은 모양이므로, EBNF는 공통 구문을 읽고 `JavaScriptArrowFunctions`가 실제 화살표 헤드를 바인딩 목록으로 검사합니다. 일반 괄호식에서는 `()`·`(value,)`·`(...rest)`를 거부합니다. 매개변수의 배열·객체만 바인딩 패턴으로 해석하며, 기본값·계산된 속성 이름 안의 식은 일반 표현식으로 계속 검사합니다. 검사는 구문 트리와 원문을 변경하지 않습니다.

화살표 함수의 블록 본문이 끝난 뒤의 `(other)`는 그 함수에 붙는 호출 인자가 아닙니다. 따라서 `const transform = value => {}` 다음 줄의 `(other);`는 별도 문장으로 읽습니다. 함수 전체를 괄호로 감싼 `(value => {})(other)`는 호출로 파싱합니다. 표현식 본문 `value => read` 다음 줄의 `(other)`는 본문의 `read(other)` 호출로 이어집니다. 이 차이와 줄바꿈·범위 검사를 [화살표 함수 설명](../../docs/ko/javascript-statements.html#arrows)에 순서대로 정리했습니다.

## async·제너레이터와 객체 메서드

함수 선언·표현식 앞의 `async`는 비동기 함수를, `function` 뒤의 `*`는 제너레이터를 나타냅니다. 두 표기를 함께 사용할 수 있습니다.

```javascript
async function load(path) { return await read(path); }
function* values() { yield 1; yield* others; }
async function* pages() { yield await readPage(); }
const loadAgain = async path => await read(path);
```

객체와 클래스에도 같은 메서드 종류를 정의할 수 있습니다. 객체의 getter는 매개변수 없이, setter는 rest·뒤쪽 쉼표 없이 매개변수 하나를 받습니다.

```javascript
({
    run(value) { return value; },
    get value() { return this._value; },
    set value(next) { this._value = next; },
    async load() { return await read(); },
    *values() { yield 1; },
    async *pages() { yield await readPage(); }
})
```

`await`·`yield`는 각 함수의 본문 종류에 따라 검사합니다. 일반 함수·일반 화살표·일반 메서드·접근자는 둘 다 표현식으로 사용할 수 없습니다. async 본문은 await를, 제너레이터 본문은 yield를, async 제너레이터 본문은 둘 다 허용합니다. 중첩된 일반 함수는 이 권한을 바깥 함수에서 이어받지 않습니다. 메서드의 계산된 이름에는 바깥 표현식 문맥을 적용하고, 매개변수·본문에는 메서드 자체의 함수 문맥을 적용합니다.

해당 함수의 매개변수 기본값이나 계산된 바인딩 이름에서 직접 사용하는 await·yield는 오류입니다. 그 기본값 안에 별도로 정의한 async 함수·제너레이터의 본문은 독립적으로 검사합니다. 기존 매개변수 중복·본문 lexical 선언 충돌·rest·use strict 조건도 함께 적용합니다.

`async(value)`는 일반 호출이고 `async (value) => value`는 async 화살표입니다. 화살표 헤드가 확정되면 그 부분에 바인딩 조건을 적용합니다. async 수식어 직후와 `=>` 직전의 줄바꿈은 금지하며, yield 뒤의 줄바꿈은 값 없는 yield와 다음 문장을 구분합니다. 각 예제의 순서와 구현 이유는 [함수·메서드 문서](../../docs/ko/javascript-functions.html)에 있습니다.

## 선언·매개변수의 구조 분해와 문장 경계

선언과 일반 함수 매개변수에는 `BindingIdentifier`·`BindingPattern` 문법을 사용합니다. 화살표 매개변수도 위의 cover 검사에서 같은 바인딩 조건을 적용합니다. 다음 구문은 새 바인딩 이름을 만듭니다.

```javascript
const {name: displayName = 'guest', ...other} = user;
let [first, second = 0, ...rest] = values;
function read({name}, [first, ...rest]) { return name; }
for (const {name} of users) { display(name); }
```

할당은 `target.name` 같은 기존 속성을 대상으로 받을 수 있지만, 선언·매개변수의 패턴에는 새 이름이 필요합니다. 따라서 `const {name: target.name} = user;`는 오류입니다. 객체 rest 바인딩에는 식별자 하나만 사용할 수 있고, 배열·함수 매개변수의 rest에는 중첩 바인딩 패턴도 허용합니다. rest는 마지막에 오며, 뒤쪽 쉼표나 rest 자체의 기본값을 허용하지 않습니다.

`Validate`는 바인딩 이름과 그 이름이 속한 범위를 검사합니다. `function read(name) { let name; }`는 매개변수와 본문 선언의 충돌이고, `function read(name) { { let name; } }`는 안쪽 블록의 별도 바인딩으로 허용합니다. 기본값·계산된 속성 이름·중첩 함수의 식별자는 외부 패턴의 바인딩 이름 목록에 포함하지 않습니다.

문장 경계는 JavaScript EBNF의 종료 규칙과 등록한 C# 프레디케이트로 판단합니다. `a` 다음 줄의 `(b)`는 호출로 이어지지만, `return` 다음 줄의 `value`는 반환문에 붙이지 않습니다. 일반 for 헤더의 세미콜론 두 개는 직접 적습니다. 이 ASI 처리는 공통 엔진에 가짜 세미콜론 토큰을 추가하지 않고 기존 토큰의 줄바꿈 정보와 조건 검사를 사용합니다. [문장 경계 설명](../../docs/ko/javascript-statements.html#semicolon)에 각 예제의 파싱 결과를 정리했습니다.

## 클래스와 본문의 문맥

클래스 선언은 Script에, 클래스 표현식은 표현식의 자리에 입력합니다. 기존 API로 파싱과 정적 검사를 이어서 수행합니다.

```javascript
class Counter {
    #value = 0;
    constructor(start) { this.#value = start; }
    get value() { return this.#value; }
    set value(next) { this.#value = next; }
    static { this.defaultValue = 0; }
}
```

클래스 본문은 자기 private 이름의 범위를 만들고, 중첩 클래스에는 바깥 private 이름도 전달합니다. 반면 `extends`의 부모 클래스 표현식은 자기 private 이름을 추가하기 전의 바깥 범위를 사용합니다. 계산된 속성 이름과 메서드·필드·static 블록의 본문도 서로 다른 문맥으로 구분합니다.

`Validate`는 생성자 중복, 멤버 이름·접근자의 매개변수 조건, private 선언·중복, 잘못된 private 삭제, super·new.target·arguments 사용 조건과 static 블록 범위를 검사합니다. 필드 종료 조건과 슬래시 선택은 기존 JavaScript 프레디케이트·정책을 사용하며, 공통 렉서·파서 엔진의 구조를 변경하지 않습니다. 사용 순서와 각 제한의 이유는 [클래스 설명](../../docs/ko/javascript-classes.html)에 정리했습니다.

## Module 입력과 import·export

정적 import·export가 있는 파일에는 같은 `JavaScriptGrammar`에서 `inputKind: JavaScriptInputKind.Module`을 선택합니다. 나머지 사용 순서는 동일합니다.

```csharp
var language = new JavaScriptGrammar(inputKind: JavaScriptInputKind.Module);
var parser = new LALRParser(language.Grammar);
var input = @"
import {calculate} from './math.js';
export const answer = calculate(6, 7);
";
var parsed = parser.Parsing(language.CreateTokenSource(input));
// parsed.Success를 확인한 뒤 language.Validate(parsed.ToParseTree)로 검사합니다.
```

Module 문법은 정적 import·export를 최상위에만 연결합니다. 가져온 이름과 일반 선언의 충돌, 공개 이름의 중복, 선언하지 않은 이름의 local export, 해독한 문자열 이름과 import 속성의 중복도 검사합니다. 이름이 있는 default 함수·클래스 선언은 Module의 lexical 이름을 만들고, 괄호로 감싼 함수·클래스 표현식의 이름은 그 표현식의 범위에 속합니다.

동적 `import(path[, options])`는 표현식이므로 세 입력 종류에서 모두 사용할 수 있습니다. Module 전용 `import.meta`는 중첩된 일반 함수에서도 사용할 수 있습니다. 반면 Module의 `await` 표현식과 for-await-of는 일반 함수·일반 화살표 함수의 매개변수·본문, 클래스 필드 초기값·static 블록에 들어갈 수 없습니다. async 함수·async 화살표·async 제너레이터는 자기 본문에서 await를 허용합니다. 부모 클래스 표현식과 계산된 멤버 이름은 바깥 Module 문맥을 사용합니다.

이 언어 계층은 구문 트리와 정적 진단을 만듭니다. 의존 파일을 불러오고 모듈을 연결·실행하는 작업은 애플리케이션의 로더·런타임에서 수행합니다. 각 구문과 문맥을 구분하는 이유는 [모듈 설명](../../docs/ko/javascript-modules.html)에 기록했습니다.

## 정규식 본문 검사

정규식 리터럴은 토큰 하나로 읽을 수 있어도 본문에 문법 오류가 있을 수 있습니다. 예를 들어 `/[z-a]/u`는 경계와 플래그를 만족하지만, 문자 범위가 거꾸로 지정되어 있습니다. 기존 `Parsing` 뒤에 `Validate`를 호출하면 이 오류를 `regexp_syntax` 진단으로 확인할 수 있습니다.

토큰 공급자는 리터럴의 경계와 플래그를 검사하고, 본문 검사는 최종 구문 트리에 들어간 정규식 토큰만 대상으로 합니다. 예측 중에 버리는 정규식 후보의 본문 오류가 실제 나눗셈 경로를 중단하지 않도록, 이 두 검사를 나눕니다. 공통 렉서·파서 엔진은 그대로 사용합니다.

- 일반 모드는 UTF-16 코드 단위와 Annex B.1.2 호환 문법을 적용합니다. 이 정규식 모드는 JavaScript의 엄격 모드·Module 여부와 별도로 정합니다.
- u 모드는 Unicode 코드 포인트, 그룹·역참조·범위와 이스케이프의 엄격한 조건을 적용합니다.
- v 모드는 Unicode 문자 처리에 중첩 집합·교집합·차집합·`\q{...}` 문자열과 `MayContainStrings` 조건을 더합니다.

Unicode 속성 이름은 ECMAScript 2026의 명시 목록으로 한정하고, 범주·스크립트 값은 고정한 Unicode 17 공식 별칭으로 확인합니다. 매우 긴 반복 횟수는 정확한 정수로 비교하며, 중첩 그룹과 문자 집합은 명시적인 스택으로 읽습니다. 이 계층은 정규식을 매칭하거나 JavaScript를 실행하지 않고, 원문에 대한 UTF-16 진단 범위를 반환합니다.

각 문법의 예제와 구현 이유, C# 사용 코드는 [정규식 본문 설명](../../docs/ko/javascript-regexp.html)에 있습니다.

## 파일과 생성 과정

| 파일 | 역할 |
| --- | --- |
| [`ecmascript-2026-expressions.ebnf`](ecmascript-2026-expressions.ebnf) | 표현식의 기준 생성 규칙. 토큰 정규식이 들어갈 자리에 `{{IdentifierName}}` 등의 표시를 둡니다. |
| [`ecmascript-2026-bindings.ebnf`](ecmascript-2026-bindings.ebnf) | 선언·매개변수·catch의 바인딩 패턴과 rest 제약입니다. |
| [`ecmascript-2026-statements.ebnf`](ecmascript-2026-statements.ebnf) | 일반 함수·문장·선언, 문장 종료 조건, for 헤더의 NoIn 생성 규칙입니다. |
| [`ecmascript-2026-classes.ebnf`](ecmascript-2026-classes.ebnf) | 클래스·메서드·필드·static 블록과 private·super·new.target 관련 구문입니다. |
| [`ecmascript-2026-modules.ebnf`](ecmascript-2026-modules.ebnf) | Module 항목과 import·export·속성, import 관련 표현식과 await·for-await-of 생성 규칙입니다. |
| [`ecmascript-2026-functions.ebnf`](ecmascript-2026-functions.ebnf) | async·제너레이터 함수, async 화살표, yield와 객체·클래스의 추가 메서드 생성 규칙입니다. |
| [`JavaScriptFunctionGrammar.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptFunctionGrammar.cs) | Strict에서는 Await의 두 문맥으로, NonStrict에서는 Await·Yield를 조합한 네 문맥으로 EBNF 규칙을 연결합니다. 함수·메서드의 매개변수와 본문에 맞는 문맥을 적용합니다. |
| [`JavaScriptSyntax.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptSyntax.cs) | 검사기들이 `__Await`·`__Yield` 변형에서 원래 생성 규칙 이름과 문맥을 확인하고, 함수·메서드 종류를 같은 기준으로 구분하게 합니다. |
| [`JavaScriptMode.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptMode.cs) | 표현식·Script 입력이 시작할 엄격 모드를 선택합니다. 웹 호환 옵션은 공개 언어 클래스가 별도로 보관합니다. |
| [`JavaScriptStrictness.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptStrictness.cs) | 지시문·포함 함수·클래스·Module과 계산된 메서드 이름을 구분해 노드별 엄격 모드를 정합니다. |
| [`JavaScriptNonStrictGrammar.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptNonStrictGrammar.cs) | 기존 EBNF 리더로 비엄격·웹 호환 생성 규칙을 연결합니다. |
| [`JavaScriptInputKind.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptInputKind.cs) | Script·Module·Expression 입력 종류를 선택합니다. |
| [`JavaScriptGrammar.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptGrammar.cs) | 세 입력 종류의 공통 공개 API입니다. EBNF 리소스와 토큰 규칙을 연결하고, 선택한 입력의 시작 규칙·정책·입력 공급자·검사기를 준비합니다. |
| [`JavaScriptModuleValidation.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptModuleValidation.cs) | 공개 이름·local export·속성과 import.meta·await의 문맥을 검사합니다. |
| [`JavaScriptTokenPatterns.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptTokenPatterns.cs) | 식별자·숫자·문자열·정규식·주석 규칙과 토큰 검사를 준비합니다. |
| [`JavaScriptRegExpValidation.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptRegExpValidation.cs) | 최종 정규식 토큰의 그룹·반복·역참조·이스케이프와 본문 문법을 검사합니다. |
| [`JavaScriptRegExpSets.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptRegExpSets.cs) | v 모드의 중첩 문자 집합·연산·문자열과 부정 조건을 검사합니다. |
| [`JavaScriptRegExpProperties.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptRegExpProperties.cs) | 명세의 Unicode 속성 이름과 고정된 공식 별칭을 확인합니다. |
| [`JavaScriptTemplateTokens.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptTemplateTokens.cs) | 템플릿 구간의 토큰 정규식, 커서의 보간 중괄호 깊이, 잘못된 템플릿 이스케이프 검사를 담당합니다. |
| [`JavaScriptExpressionValidation.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptExpressionValidation.cs) | 성공한 구문 트리에서 정적 문법 오류를 검사합니다. |
| [`JavaScriptAssignmentPatterns.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptAssignmentPatterns.cs) | 배열·객체 구문을 실제 대입 패턴으로 해석하고 대상·기본값·rest 제약을 검사합니다. 일반 표현식의 검사 경계와 원래 구문 트리를 보존합니다. |
| [`JavaScriptArrowFunctions.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptArrowFunctions.cs) | 화살표 헤드를 바인딩 목록으로 검사하고 일반 괄호식·기본값·계산된 속성 이름과 구분합니다. |
| [`JavaScriptClassValidation.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptClassValidation.cs) | 클래스 멤버·private 이름과 본문별 사용 조건을 검사합니다. |
| [`JavaScriptStatementValidation.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptStatementValidation.cs) | 바인딩 이름·선언 범위·매개변수·제어문 목적지를 검사합니다. |
| [`JavaScriptStatementPredicates.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptStatementPredicates.cs) | 문장 종료와 줄바꿈 제한을 검사하는 C# 함수를 등록합니다. |
| [`JavaScriptSlashContext.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptSlashContext.cs) | 직전 토큰에서 다음 토큰의 불변 문맥을 계산해 슬래시 정책과 화살표 블록의 문장 종료 조건에 제공합니다. |
| [`unicode/`](unicode/README.txt) | Unicode 17.0.0 출처·해시·라이선스와 식별자 문자 범위·정규식 속성 별칭 재생성 스크립트입니다. |

생성자는 여섯 EBNF 파일을 연결하고, `{{IdentifierName}}`, `{{PrivateIdentifier}}`, `{{NumericLiteral}}`, `{{StringLiteral}}`, `{{RegularExpressionLiteral}}`, `{{Comment}}`와 네 템플릿 토큰 표시를 실제 정규식으로 바꿉니다. 선택한 모드로 숫자·문자열 패턴을, 웹 호환 옵션과 입력 종류로 주석 패턴을 준비합니다. `JavaScriptNonStrictGrammar`는 비엄격·웹 호환 생성 규칙을 연결합니다. 생성자는 문장 경계와 줄바꿈의 C# 검사 함수도 등록합니다. `language.GrammarText`는 이 준비를 마친 EBNF이며, `InputKind`에 따라 시작 규칙을 Script의 문장 목록, Module의 최상위 항목 목록 또는 표현식 하나로 정합니다.

이 과정에서 `JavaScriptFunctionGrammar`는 함수 종류에 맞는 문맥별 생성 규칙을 준비합니다. 기본값인 Strict에서는 Await 문맥에 영향을 받는 생성 규칙을 Await가 꺼진 기본 규칙과 Await가 켜진 `__Await` 규칙으로 나눕니다. 일반 함수·제너레이터에서는 Await를 끄고, async 함수·async 제너레이터에서는 Await를 켜서 매개변수와 본문을 연결합니다. Module 최상위도 Await가 켜진 규칙에서 시작합니다. 이 Strict 경로에서는 Yield 표현식을 공통 구문으로 읽은 뒤, 최종 언어 검사가 함수 종류와 매개변수 경계에 맞게 사용했는지 확인합니다.

NonStrict에서는 일반 코드의 `yield`를 식별자로 사용할 수 있으므로, 구문을 읽을 때부터 `yield` 식별자와 Yield 표현식을 구분해야 합니다. 따라서 문맥에 영향을 받는 생성 규칙을 **Await·Yield가 모두 꺼진 경우, Await만 켠 경우, Yield만 켠 경우, 둘 다 켠 경우의 네 가지**로 준비합니다. 각각 일반 함수·async 함수·제너레이터·async 제너레이터의 매개변수와 본문에 적용합니다. 문맥별 생성 규칙 이름에는 `__Await`, `__Yield`, `__Yield__Await` 접미사를 붙이며, 계산된 메서드 이름에는 바깥 문맥을 유지합니다. `JavaScriptSyntax`는 이 접미사에서 문맥을 확인하고, 원래 생성 규칙 이름을 기준으로 구문 트리의 역할을 구분합니다.

이 문맥 연결은 JavaScript 지원 계층에서 일반 EBNF 생성 규칙을 준비하는 작업입니다. 기준 파일을 직접 사용하는 도구도 같은 파일 연결·문맥별 규칙 준비·정규식 치환·프레디케이트 등록이 필요합니다.

이 구성에서 EBNF는 연산자 계층과 표현식 구조를 담당합니다. C#은 고정된 Unicode 데이터로 토큰 정규식을 준비하고, 이스케이프의 코드 포인트·정규식 플래그와 본문·구문 트리의 정적 조건을 검사합니다.

일반 for 초기식에는 `in`을 구분자로 남기는 `NoIn` 문법을 사용하며, 화살표 함수의 표현식 본문에도 이 조건을 전달합니다. `key => (key in object)`처럼 괄호를 사용하면 그 안에는 일반 표현식 문법을 적용할 수 있습니다. 화살표 함수의 블록 본문은 일반 문장 문법을 사용합니다.

입력 공급자가 다음 토큰을 반환할 때는 `JavaScriptSlashContext`가 직전 토큰의 문맥에서 그 토큰의 문맥을 계산합니다. 문맥은 토큰 객체에 연결된 불변 값이며, 정책과 문장 종료 조건이 함께 사용합니다. 이 값은 약한 참조 캐시에 보관하므로 더 이상 사용하지 않는 토큰을 계속 유지하지 않으며, 같은 원문 위치라도 서로 다른 예측 후보의 토큰을 합치지 않습니다.

## Unicode 원본과 라이선스

식별자 문자 범위는 다음 원본의 `ID_Start`·`ID_Continue`에서 생성하며, ECMAScript의 `$`·`_` 규칙을 적용합니다.

- 버전: Unicode 17.0.0
- 원본: [DerivedCoreProperties.txt](https://www.unicode.org/Public/17.0.0/ucd/DerivedCoreProperties.txt)
- SHA-256: `24c7fed1195c482faaefd5c1e7eb821c5ee1fb6de07ecdbaa64b56a99da22c08`
- 사용 조건: [Unicode License v3](unicode/LICENSE.txt)

재생성 스크립트는 원본 SHA-256을 확인한 뒤 `JavaScriptUnicodeData.g.cs`를 만듭니다. 생성 결과를 저장소에 포함하므로 일반 빌드는 네트워크나 .NET 런타임의 Unicode 버전에 의존하지 않습니다.

```powershell
python grammars/javascript/unicode/generate_identifier_data.py
```

오프라인 재생성은 공식 원본 파일의 로컬 경로를 마지막 인자로 전달합니다. 세부 내용은 [Unicode 데이터 안내](unicode/README.txt)에 있습니다.

## 검증

저장소 루트에서 JavaScript 테스트를 실행합니다.

```powershell
dotnet test Tests/Parse.FrontEnd.Parsers.Tests/Parse.FrontEnd.Parsers.Tests.csproj -c Release --filter "Category=JavaScript"
```

테스트는 다음 내용을 확인합니다.

- 기존 표현식: 수락·거부 결과, 구문 트리의 연산자 우선순위·결합 방향, 배열·객체 데이터와 정적 오류 진단
- 토큰: Unicode 식별자, 숫자·문자열·정규식의 범위, Div·RegExp 전환, 주석의 줄바꿈과 후위 증감
- 정규식 본문: 일반·u·v 모드의 그룹·반복·역참조·이스케이프·범위, Unicode 속성과 집합 연산·문자열, 오류의 UTF-16 위치, 깊은 중첩·긴 숫자와 기존 문맥의 조합
- 템플릿: 보간 없는 문자열, 여러 보간 구간, 중첩 템플릿과 객체 중괄호, 태그 유무에 따른 이스케이프 검사, 원문·위치 보존
- 구조 분해: 중첩·기본값·rest 위치와 대상·괄호·대입 연산자, 일반 표현식과 패턴의 검사 경계, 원문과 구문 트리 보존
- async·제너레이터·메서드: 함수 선언·표현식·화살표·객체/클래스 메서드와 접근자, await·yield와 매개변수·본문 경계, async 호출과 화살표 헤드, 줄바꿈·선언 범위·super·생성자 조건
- 일반 함수·문장: 함수 선언·표현식·본문, 반복문·switch·try/catch, 레이블과 함수 경계를 지키는 return·break·continue
- 화살표 함수: 매개변수의 cover 해석·기본값·구조 분해·rest, 두 본문 형태와 중첩, NoIn·줄바꿈·블록 종료와 호출 구분, 기존 일반 괄호식의 검사 유지
- 클래스: 선언·표현식·상속, 일반 메서드·접근자·필드·static 블록, private 이름 범위와 중복, super·new.target·arguments 문맥, 기존 함수·ASI·템플릿과의 조합
- 모듈: import·export·재내보내기·속성, 문자열 이름, local export의 선언 범위, 동적 import와 import.meta의 입력 문맥, top-level await·for-await-of와 함수·클래스 경계
- 바인딩: 선언·매개변수·for·catch 패턴, 이름 중복·선언 범위 충돌, 엄격 모드 이름, const 초기값, 매개변수와 use strict 지시문
- 비엄격·웹 호환: 기본 모드 유지, 직접·상속 지시문과 클래스 경계, 구형 리터럴·모드별 이름·함수 조건, HTML 주석의 원문·줄바꿈, 항목별 웹 호환 예외와 Module 경계
- 문장 경계: ASI와 명시적 세미콜론, 호출·후위 증감·return·throw의 줄바꿈, 일반 for 헤더의 구분자
- 문맥 분리: 같은 커서에서의 반복 요청, 별도 입력 공급자와 예측 후보가 사용하는 보간 문맥의 독립성

문법의 충돌 검사도 함께 수행합니다. 공식 Unicode 원본을 이용한 식별자 판정 비교와 Node.js의 구문 컴파일 비교는 입력을 실행하지 않고 진행합니다. 엔진 간 판정이 다르면 지원 범위를 먼저 확인한 뒤, 고정한 ECMAScript 2026 명세의 구문·정적 오류 조건과 대조합니다.

## ECMAScript 2026 명세와 Test262 대조

[명세 대조 보고서](../../docs/ko/javascript-conformance.html)는 생성 규칙 388개와 Early Errors 213개 항목을 구현·테스트 파일에 연결합니다. 공식 Test262 원문은 각 사례의 메타데이터에 따라 엄격 Script·비엄격 Script·Module로 나눠 파싱하고, 성공한 트리에 정적 검사를 적용합니다. 예상된 문법 오류와 내부 예외·시간 초과는 서로 구분해 집계합니다.

고정한 원본 커밋, 2026 이후 제안 문법의 제외 이유, 실행 결과와 재현 명령은 [검증 도구 문서](../../tools/Test262/README.md)에 기록합니다. 테스트 안의 JavaScript를 실행하는 대신, 각 원문의 구문과 정적 오류 판정을 비교합니다.

## 기준과 저자 맥락

언어 규칙은 ECMA-262의 [2026 어휘 문법](https://tc39.es/ecma262/2026/multipage/ecmascript-language-lexical-grammar.html)과 [2026 표현식 문법](https://tc39.es/ecma262/2026/multipage/ecmascript-language-expressions.html)을 기준으로 구성했습니다. Unicode 데이터의 출처와 라이선스는 위에 별도로 기록했습니다.

Janglim의 핵심 LR 구조는 저자가 설계하고 작성했습니다. 이번 JavaScript 표현식 문법·토큰 규칙·정적 검사·테스트와 문서는 그 기존 구조 위에 대화에서 합의한 범위를 AI 보조로 추가한 언어 지원 계층입니다. 기존 엔진에 담긴 저자의 설계·구현과 새로 추가한 언어 지원을 구분합니다.
