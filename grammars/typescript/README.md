# TypeScript 문법 지원

Janglim 0.6.0의 `TypeScriptGrammar`는 기존 JavaScript 지원 계층에 TypeScript의 타입·선언·표현식 단언·클래스 문법을 연결합니다. 입력을 파싱한 뒤 구문 트리의 정적 문법 조건을 검사하며, 원래 트리에는 타입 구문과 원문 위치가 그대로 남습니다.

현재는 **아래 범위의 TypeScript 문법을 지원합니다.** 전체 TypeScript·TSX 지원은 진행 중이며, 남은 문법은 이 문서의 마지막 표에 정리했습니다.

## 사용 순서

`TypeScriptGrammar`가 문법과 토큰 공급자를 준비하면 `LALRParser`가 입력을 파싱합니다. 파싱에 성공한 다음에는 `Validate`를 호출합니다. 두 검사를 모두 통과해야 입력이 현재 지원 범위의 문법 조건을 만족합니다.

```csharp
using System;
using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new TypeScriptGrammar();
var parser = new LALRParser(language.Grammar);
var input = "class Box<T> { constructor(public value: T) {} }";

var parsed = parser.Parsing(language.CreateTokenSource(input));
if (!parsed.Success)
    throw new InvalidOperationException(parser.FiredException?.Message ?? "Parsing failed.");

var diagnostics = language.Validate(parsed.ToParseTree);
foreach (var diagnostic in diagnostics)
    Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message} " +
        $"(offset {diagnostic.SourceOffset}, length {diagnostic.Length})");

Console.WriteLine(diagnostics.Count == 0);
```

진단의 위치와 길이는 .NET 문자열의 UTF-16 단위입니다. `Validate`는 수식의 대입 대상, 선언·매개변수·클래스 멤버의 문법 제한 등을 검사합니다. 타입 이름을 다른 파일에서 찾거나 값의 타입이 선언과 호환되는지 판단하는 작업은 타입 검사기의 역할입니다. 이 API는 JavaScript 실행이나 TypeScript 변환 결과를 생성하지 않습니다.

## 입력 종류

생성자의 `inputKind`로 입력의 시작 규칙을 선택합니다. 모든 입력 종류에서 위의 사용 순서는 같습니다.

| 입력 종류 | 입력 예 | 선택 코드 |
| --- | --- | --- |
| `Script` | `let value: string;`처럼 선언·문장이 있는 입력 | `new TypeScriptGrammar()` |
| `Module` | 지원하는 import·export와 선언·문장이 있는 입력 | `new TypeScriptGrammar(TypeScriptInputKind.Module)` |
| `Expression` | `value as User`처럼 표현식 하나 | `new TypeScriptGrammar(TypeScriptInputKind.Expression)` |
| `Type` | `User \| null`처럼 타입 하나 | `new TypeScriptGrammar(TypeScriptInputKind.Type)` |

기본 모드는 `JavaScriptMode.NonStrict`입니다. Script·Expression에서는 생성자의 `mode`로 엄격 모드를 선택할 수 있고, Module은 항상 엄격 모드를 적용합니다. TypeScript 계층은 JavaScript의 Annex B 웹 호환 옵션을 켜지 않습니다.

## 현재 지원 범위

| 구분 | 지원 내용 |
| --- | --- |
| 타입 | 기본·리터럴·타입 참조, 제네릭 타입 인수, 배열·튜플, union·intersection, 함수·생성자 타입, 객체 타입, 인덱스 접근, mapped·conditional·template literal 타입, `keyof`·`typeof`·`infer`와 타입 프레디케이트 |
| 선언 | 타입 주석이 있는 변수, 일반 함수의 타입 매개변수·매개변수·반환형과 서명, interface·type alias·enum·namespace, `declare`를 사용하는 ambient 선언 |
| 표현식 | `as`, `satisfies`, non-null 단언과 기존 JavaScript 표현식의 조합 |
| 클래스 | 타입 매개변수, 제네릭 `extends`, `implements`, 접근 제한자·`readonly`·`override`·`abstract`·`declare`·`accessor`, 타입이 있는 필드·메서드·접근자, optional·definite 필드, 메서드 서명, 생성자 매개변수 프로퍼티, 인덱스 서명 |
| 정적 문법 검사 | 잘못된 단언 대상, 타입 구문의 문법 제약, 선언·매개변수와 클래스 멤버 제한, private 이름과 기존 JavaScript 정적 검사 |

예를 들어 `Box<User>`라는 **타입 참조**와 `class Box<T> {}`라는 **클래스 선언**은 지원합니다. 런타임 표현식인 `new Box<User>()`와 `make<User>()`는 별도 문법이며 다음 작업 범위에 있습니다. 이 구분은 타입 구문 지원을 전체 제네릭 표현식 지원으로 오해하지 않도록 명시합니다.

## 구성

| 파일 | 역할 |
| --- | --- |
| [`typescript-types.ebnf`](typescript-types.ebnf) | 타입과 타입 매개변수·멤버·바인딩 구문을 정의합니다. |
| [`typescript-declarations.ebnf`](typescript-declarations.ebnf) | interface·type alias·enum·namespace·ambient 선언을 정의합니다. |
| [`TypeScriptGrammar.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/TypeScript/TypeScriptGrammar.cs) | 공개 API입니다. 입력 종류와 모드를 받아 문법·토큰 공급자·검사기를 연결합니다. |
| [`TypeScriptDialect.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/TypeScript/TypeScriptDialect.cs) | JavaScript 생성 규칙에 TypeScript 규칙과 프레디케이트를 연결합니다. |
| [`TypeScriptClasses.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/TypeScript/TypeScriptClasses.cs) | TypeScript 클래스 생성 규칙과 문법 선택 조건을 준비합니다. |
| [`TypeScriptLexicalPolicy.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/TypeScript/TypeScriptLexicalPolicy.cs) | 타입·클래스 문맥에서 사용할 토큰 규칙 묶음을 선택합니다. |
| [`TypeScriptValidation.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/TypeScript/TypeScriptValidation.cs) | TypeScript의 정적 문법 검사와 JavaScript 검사를 연결합니다. |

타입 영역에서 `A<B<C>>`를 읽을 때는 닫는 `>`를 각각의 토큰으로 받을 수 있어야 합니다. TypeScript 정책은 기존 parser-driven lexing API로 이 문맥을 구분합니다. 일반 JavaScript 표현식에서는 기존 비교·시프트 연산자 규칙을 유지합니다.

검사기는 JavaScript 검사에 필요한 별도 트리 표현을 만들지만, 호출자에게 반환한 원래 구문 트리의 타입 노드·토큰·부모 연결·원문 범위는 보존합니다. 클래스의 서명과 private 이름도 이 경계를 지키며 검사합니다.

## 기준과 검증

현재 구현의 기준 버전은 **TypeScript 7.0.2**입니다. `TypeScriptGrammar.LanguageVersion`도 이 기준을 나타냅니다. Microsoft의 `typescript-go` 커밋 `2bd066d87f5bafd315be9f40889d0a60b9e58e0b`에 있는 파서와 정적 문법 검사를 대조 대상으로 사용합니다.

단위테스트는 수락·거부 결과뿐 아니라 연산자 해석, 타입·일반 표현식의 경계, 줄바꿈, 입력 종류, 진단 위치와 원래 구문 트리의 보존을 확인합니다. 저장소 루트에서 다음 명령으로 TypeScript 테스트를 실행할 수 있습니다.

```powershell
dotnet test Tests/Parse.FrontEnd.Parsers.Tests/Parse.FrontEnd.Parsers.Tests.csproj -c Release --filter "Category=TypeScript"
```

0.6.0 준비 시점의 TypeScript 단위테스트는 975개이며, 이 가운데 클래스 확장 회귀 테스트가 328개입니다. 이 수치는 구현한 범위의 회귀 검증 결과입니다. 전체 문법 지원 판정에는 아래 작업과 TypeScript 전체 구문·정적 오류 대조가 남아 있습니다.

## 남은 문법

| 작업 | 대표 예 |
| --- | --- |
| 런타임 제네릭 호출·생성, instantiation 표현식 | `make<string>()`, `new Box<string>()`, `make<string>` |
| 타입이 있는 화살표 함수·제네릭 화살표 함수 | `(x: number): number => x`, `<T>(x: T) => x` |
| 꺾쇠괄호 타입 단언 | `<User>value` |
| TypeScript 전용 모듈 구문 | `import type { User } from './types'`, `export type { User }`, `import x = require('x')`, `export = x` |
| 데코레이터 | `@sealed class C {}` |
| TSX | `<Button />` |

`export type A = string`처럼 타입 별칭 선언을 내보내는 문법은 현재 지원합니다. 위 표의 `export type { User }`는 이미 있는 이름을 타입 전용으로 내보내는 별도 문법입니다.

## 저자와 구현 맥락

Janglim의 핵심 LR 구조는 저자가 설계하고 작성했습니다. 이 TypeScript 계층은 기존 엔진과 JavaScript 지원을 재사용하여, 저자와 논의한 범위에 맞춰 AI 보조로 추가한 언어 지원입니다. 기존 엔진에 담긴 저자의 설계·구현과 새로 추가한 언어 지원 계층을 구분합니다.
