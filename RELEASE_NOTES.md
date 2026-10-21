# Janglim 0.5.0 출시 노트 — 2026-10-21

**Janglim 0.5.0은 파서 주도 렉싱과 ECMAScript 2026 기준 JavaScript 언어 지원을 추가합니다.** 파서는 현재 파싱 문맥을 정책에 전달하고, 정책이 선택한 토큰 규칙 묶음으로 다음 토큰을 요청합니다. 이 입력 구조 위에 표현식·엄격 모드 Script·Module의 세 JavaScript API를 제공합니다.

| 대상 | 버전 | 역할 |
| --- | --- | --- |
| Janglim 엔진·NuGet 패키지 | `0.5.0` | .NET Standard 2.1 LALR 엔진, 요청형 렉싱과 JavaScript 언어 API |
| 웹 Playground | `1.1.0` | EBNF·입력 편집, 파싱·트리 탐색과 로컬 작업 파일 |
| MCP 서비스 소스 | `0.1.0` | 원격 문법 조회·검증·파싱·예제 검사 |
| 내장 C11 Core 기준 문법 | `1.0.0` | 수정할 사본과 회귀 예제를 제공하는 읽기 전용 기준 문법 |

엔진·웹·MCP·기준 문법은 독립된 버전을 사용합니다. 이번에 올리는 NuGet 패키지 ID는 `Janglim`입니다.

```shell
dotnet add package Janglim --version 0.5.0
```

## 파서가 필요할 때 다음 토큰을 요청합니다

새 입력 흐름은 **토큰 규칙 묶음 → 선택 정책 → 원문을 읽을 source → 파싱**입니다.

```csharp
var goal = new LexicalGoal(terminals);
var policy = new LexicalPolicy(goal);
var source = lexer.CreateTokenSource(input, policy);
var parsed = parser.Parsing(source);
```

`LexicalGoal`은 함께 사용할 단말 규칙의 묶음입니다. `LexicalPolicy`는 이 묶음들을 보관하고, 파서가 전달한 `LexicalContext`로 적합한 묶음을 선택합니다. `Lexer.CreateTokenSource`는 원문과 정책을 연결한 `ITokenSource`를 만듭니다.

파서는 정책이 반환한 묶음을 `source.Peek`에 전달합니다. `source`는 토큰과 다음 커서를 `TokenRead`로 반환하고, 파서는 실제로 Shift할 때 `Next`를 현재 커서로 채택합니다. 예측 후보도 각자의 커서·소비 이력·룩어헤드와 렉서 상태를 보관합니다. 한 후보가 입력을 소비해도 다른 후보나 실제 파서의 위치는 바뀌지 않습니다.

예를 들어 `result = /ab/g;`에서 파서가 `result`와 `=`를 소비했다면 현재 커서는 8번 공백 앞입니다. `Peek`는 9~13번의 `/ab/g` 토큰과 다음 위치 14를 반환하고, 현재 커서는 Shift할 때까지 8을 유지합니다. 위치·토큰 수·이전 토큰과 보관 구조를 설명하는 [세 장의 도식](docs/ko/parser-driven-lexing.html#cursor-example)을 추가했습니다.

기존 토큰 목록 입력과 요청형 입력을 모두 사용할 수 있습니다. 요청형 `Parsing(ITokenSource)`는 첫 오류에서 중단합니다. 사용자 오류 복구와 일반 충돌 백트래킹을 사용하는 코드는 기존 토큰 목록 입력 경로를 사용합니다.

## 렉서는 실제 입력을 가장 길게 읽는 규칙을 선택합니다

`Lexer.Lexing`과 요청형 `source`는 공통 선택 코드를 사용합니다.

1. 현재 위치에서 여러 토큰 규칙이 일치하면, 실제 입력을 가장 길게 읽은 일치를 선택합니다.
2. 길이가 같으면 토큰 분류의 우선순위와 등록 순서를 적용합니다.
3. 공백·주석 스킵 규칙에도 같은 비교 기준을 적용합니다.

예를 들어 키워드 `if`와 식별자 규칙이 함께 있을 때 `if`는 동률 우선순위에 따라 키워드로, `ifx`와 `if$`는 더 길게 일치하는 식별자로 읽습니다.

**업그레이드할 때 확인할 변경:** 토큰 정규식에 자동으로 붙이던 `` 경계를 제거했습니다. 자동 경계에 의존하던 사용자 규칙은 필요한 경계를 정규식에 직접 작성해야 합니다. 종류를 생략한 EBNF 토큰은 계속 Identifier로 분류되며, 작성한 정규식을 그대로 사용합니다.

각 정규식 내부의 대안 순서·탐욕성·지연 반복은 .NET 정규식 의미를 유지합니다. 비교 대상은 각 규칙이 반환한 일치입니다. 단일 결합 정규식 문자열을 받는 기존 `Tokenizer.Tokenize(string, ...)` 경로도 .NET 대안 순서를 유지합니다.

등록된 개별 규칙을 비교하는 렉싱 경로에서는 입력을 전혀 소비하지 않는 일치를 `EmptyTokenMatchException`으로 보고하고, 각 정규식 일치에 1초 제한을 적용합니다. MCP 어댑터는 빈 일치를 `invalid_grammar`/`zero_length_token`, 정규식 시간 초과를 `runtime_limit`으로 구분합니다.

## JavaScript 표현식·Script·Module을 파싱합니다

새 공개 네임스페이스는 `Janglim.FrontEnd.Grammars.JavaScript`입니다. 기존 EBNF 프로젝트와 Janglim 패키지 안에 언어 지원을 포함합니다.

| 진입점 | 입력 문맥 |
| --- | --- |
| `JavaScriptExpressionGrammar` | 엄격 모드 Script 문맥의 표현식 하나 |
| `JavaScriptScriptGrammar` | 엄격 모드 Script의 선언·문장 목록 |
| `JavaScriptModuleGrammar` | import·export와 top-level await를 포함한 Module 입력 |

세 API는 `Grammar`, `Policy`, `GrammarText`, `CreateTokenSource(string)`, `Validate(tree)`를 제공합니다. **입력을 허용하려면 파싱 성공과 정적 검사 성공을 함께 확인합니다.**

```csharp
using System;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new JavaScriptScriptGrammar();
var parser = new LALRParser(language.Grammar);
var parsed = parser.Parsing(language.CreateTokenSource(
    "const match = /ab/g.test(value);"));

if (!parsed.Success)
    throw new InvalidOperationException("JavaScript 구문을 확인하세요.");

var diagnostics = language.Validate(parsed.ToParseTree);
foreach (var diagnostic in diagnostics)
    Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message}");

Console.WriteLine(diagnostics.Count == 0);
```

이번 지원 범위는 다음과 같습니다.

- Unicode 17 식별자·이스케이프, 숫자·BigInt·문자열과 문맥에 따른 나눗셈·정규식 토큰 선택
- 연산자·대입·조건식·선택적 체이닝·호출·배열·객체와 일반·태그·중첩 템플릿
- 구조 분해 할당과 선언·매개변수·for·catch의 바인딩 패턴
- 일반·async 함수와 화살표 함수, 제너레이터·async 제너레이터, 객체·클래스 메서드와 getter·setter
- 클래스 선언·표현식·상속·필드·private 이름·static 블록, super·new.target 문맥
- 변수 선언, 조건문·반복문·제어문·switch·try/catch/finally·레이블, ASI
- 정적 import·export·재내보내기·import attributes, 동적 import와 Module의 import.meta·top-level await·for-await-of
- 정규식 리터럴의 경계·플래그와 일반·u·v 모드 본문 문법 검사
- 대입 대상·예약어·선언 범위·매개변수·함수 문맥·private 이름 등 정적 오류 검사

정규식 본문은 최종 구문 트리의 `Validate` 단계에서 검사합니다. 따라서 버려질 예측 후보의 정규식 오류가 정상적인 나눗셈 경로를 막지 않습니다. 진단에는 코드·설명과 UTF-16 원문 위치·길이가 들어 있습니다.

비엄격 모드 Script와 명세 전체·Test262 구문 적합성 검증은 후속 범위입니다. 다음 개발은 이 범위를 넓히고 적합성 사례를 누적하는 작업입니다. 언어 계층의 기준과 세부 범위는 [JavaScript 안내](grammars/javascript/README.md)에 정리했습니다.

## 패키지·웹·MCP의 연결 범위

이번 JavaScript API는 .NET 호스트에서 사용합니다. MCP 기준 문법 목록은 기존 `c11-core`를 유지합니다. Playground와 MCP의 일반 EBNF 처리 경로에는 공통 렉싱 개선이 적용됩니다.

패키지에는 여섯 JavaScript EBNF 내장 리소스와 Unicode 17 식별자 데이터를 포함합니다. Unicode 데이터 출처·라이선스는 [Unicode 안내](grammars/javascript/unicode/README.txt)에 기록합니다. NuGet과 MCP 게시 산출물에는 `licenses/Unicode-LICENSE.txt`를, Playground 정적 사이트에는 `wwwroot/licenses/Unicode-LICENSE.txt`를 함께 넣습니다. MCP 컨테이너 빌드에도 여섯 EBNF와 라이선스가 있는 JavaScript 문법 디렉터리를 복사합니다.

엔진은 계속 .NET Standard 2.1을 대상으로 하며 지원 파서는 `LALRParser`입니다. 문법 빌더의 전역 상태 때문에 한 프로세스의 문법 생성은 순차적으로 수행합니다. 0.x 공개 API는 후속 버전에서 바뀔 수 있습니다.

## 출시 검증

2026-10-21에 엔진 0.5.0을 적용한 소스에서 다음 로컬 검증을 완료했습니다.

| 검증 | 결과 |
| --- | --- |
| 기본 솔루션 Debug·Release 빌드 | 모두 성공, 오류 0개. 기존 미사용 필드·이벤트 경고 각 5개 |
| 엔진 Release 테스트 | 4,976개 통과, 알려진 SLR 결함 테스트 1개 제외, 실패 0개 |
| MCP Release 테스트 | 69개 통과, 실패 0개 |
| UI 회귀 테스트 | 22개 통과 |
| 게시 인프라 테스트 | 12개 통과 |
| 공개 Playground 게시 빌드 | 성공 |
| MCP 서버 게시 빌드 | 성공 |
| Docker 복사 목록만으로 구성한 MCP 소스 게시 빌드 | 성공. 컨텍스트 안의 프로젝트 참조·리소스 경로와 EBNF 6개 확인 |
| MCP·Playground 게시 라이선스 | 두 게시 위치의 Unicode 라이선스가 원본과 바이트 단위로 일치 |
| NuGet 패키징·내용 검사 | 0.5.0 패키지 생성. 엔진 DLL 9개 버전 0.5.0.0, 지역화 DLL 40개, EBNF 리소스 6개와 Unicode 라이선스 확인 |
| 독립 net8.0 패키지 소비 프로젝트 | 80개 검사 통과. 저장소 ProjectReference 없이 새 API·JavaScript 정상/오류 입력과 정적 검사 확인 |
| 저장소·패키지 README의 JavaScript 예제 | 문서의 코드 블록을 그대로 추출한 별도 프로젝트 2개에서 파싱 성공·정적 진단 0개 확인 |
| DocFX 문서 빌드 | 성공, 오류 0개. 기존 상대 링크 경고 11개 |
| 로컬 MCP 컨테이너 빌드 | Docker 엔진이 실행되지 않아 미수행. CI의 필수 컨테이너 빌드로 검증 |

산출물은 `artifacts/release-0.5.0/`에 보관합니다. NuGet 패키지는 `packages/Janglim.0.5.0.nupkg`, 엔진·MCP 결과는 `TestResults/`, 소비자 종합 결과는 `consumer-validation-summary.json`에 있습니다. 실제 게시에는 GitHub Actions가 해당 커밋에서 다시 검증해 생성한 산출물을 사용합니다.

## 푸시와 자동 게시

`master` push는 [출시 워크플로](.github/workflows/publish-nuget.yml)를 실행합니다. 전체 검증을 통과하면 아직 게시하지 않은 엔진 버전을 NuGet에 게시하고, 같은 실행에서 검증한 Playground 산출물을 배포합니다. 문서는 별도 GitHub Pages 워크플로로 배포합니다.

작업 브랜치 push와 PR은 검증만 실행합니다. 운영 MCP 재배포는 전용 Azure 스크립트로 수행합니다. 실제 게시·배포 상태는 해당 [GitHub Actions 실행](https://github.com/AJ-comp/Compiler/actions)에서 확인합니다.

## 저자와 이번 릴리즈

Janglim 핵심 구조는 저자가 직접 설계하고 작성해 온 코드에 기반합니다. 이번 문서·UI·출시 준비의 AI 보조 작업은 저자 본인의 설계와 구현에 대한 저자성을 대체하지 않습니다.

C11 Core 문법과 회귀 예제는 AI 보조로 준비한 별도 자료입니다. 엔진에 담긴 저자의 설계·구현과 이 자료의 작성 맥락은 [C11 Core 안내](grammars/c/README.md#기준과-저자-맥락)에 함께 기록했습니다.

이번 JavaScript 언어 규칙·검사기·테스트와 설명 문서는 저자의 기존 LR 구조 위에 AI 보조로 추가한 언어 지원 계층입니다. 언어 규칙과 데이터 출처, 저자 맥락은 [JavaScript 안내](grammars/javascript/README.md#기준과-저자-맥락)에 기록했습니다.

이전 릴리즈의 영문 변경 이력은 [CHANGELOG.md](CHANGELOG.md), 사용 예제와 프로젝트 소개는 [README.md](README.md)를 참고하세요.
