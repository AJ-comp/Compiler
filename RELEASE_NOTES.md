# Janglim 0.6.0 출시 노트 — 2026-10-28

**Janglim 0.6.0은 JavaScript API를 하나로 통합하고 비엄격 모드·브라우저 호환 문법을 추가하며, TypeScript의 타입·선언·단언·클래스 문법을 제공합니다.** 함께 진행한 LALR 룩어헤드 전파, FIRST 계산과 문법 종료 가능성 검사도 이번 버전에 포함합니다.

| 대상 | 버전 | 이번 변경 |
| --- | --- | --- |
| Janglim 엔진·NuGet 패키지 | `0.6.0` | JavaScript API 통합, TypeScript 지원 추가, LALR 계산 수정 |
| 웹 Playground | `1.1.0` | 웹 버전은 유지하고 새 엔진으로 빌드 |
| MCP 서비스 소스 | `0.1.0` | 서비스 버전은 유지하고 새 엔진·문법 리소스로 빌드 |
| 내장 C11 Core 기준 문법 | `1.0.0` | 기존 읽기 전용 기준 문법 유지 |

```shell
dotnet add package Janglim --version 0.6.0
```

## JavaScript: 하나의 API에서 입력 종류와 실행 문맥을 선택합니다

이제 `JavaScriptGrammar` 하나로 Script·Module·독립 표현식을 준비합니다. 기본 생성자는 엄격 모드 Script를 선택합니다. 일반 Script의 비엄격 문법은 `JavaScriptMode.NonStrict`로, 브라우저 호환 문법은 `webCompatibility: true`로 선택합니다. Module에는 항상 엄격 모드 검사를 적용합니다.

```csharp
var script = new JavaScriptGrammar(JavaScriptMode.NonStrict, webCompatibility: true);
var module = new JavaScriptGrammar(inputKind: JavaScriptInputKind.Module);
var expression = new JavaScriptGrammar(inputKind: JavaScriptInputKind.Expression);
```

비엄격 모드에서는 `with`, 기존 숫자·문자열 표기와 문맥별 식별자를 처리합니다. 함수의 `"use strict"` 지시문, 상위 함수의 엄격 모드와 클래스의 엄격 모드도 검사합니다. 선택적 Annex B 지원에는 HTML 주석, 특정 위치의 함수 선언, catch/var와 for-in 초기화 등 브라우저 호환 규칙이 포함됩니다.

ECMAScript 2026 명세 대조 과정에서 배열·바인딩 목록, `yield`와 템플릿 경계, `let` 식별자와 선언 구별, ASI 이후의 `/` 문맥도 보강했습니다. 재현 도구와 고정한 Test262 자료는 [JavaScript 검증 문서](docs/ko/javascript-conformance.html)와 [Test262 도구](tools/Test262/README.md)에 기록했습니다.

## TypeScript: 타입·선언·단언·클래스 문법을 추가합니다

`Janglim.FrontEnd.Grammars.TypeScript`의 `TypeScriptGrammar`는 기존 JavaScript 문법·렉싱·정적 오류 검사를 재사용합니다. 입력 종류는 Script·Module·Expression·Type이며, 기본값은 비엄격 모드 Script입니다.

이번 지원 범위는 다음과 같습니다.

- 타입 참조, 배열·튜플·객체·함수 타입, 유니언·인터섹션, 제네릭 타입, 조건부·매핑 타입 등 타입 문법
- 타입 주석, 타입 별칭, 인터페이스, enum, namespace와 ambient 선언
- `as`, `satisfies`, non-null 단언과 관련 우선순위·문맥 검사
- 클래스 접근 제한자, `readonly`·`override`·`abstract`·`declare`·`accessor`, 선택적·확정 할당 프로퍼티
- 제네릭 클래스, 타입 인자가 있는 상속·`implements`, 메서드 시그니처·오버로드 선언, 생성자 매개변수 프로퍼티와 클래스 인덱스 시그니처

파싱 후에는 `Validate`까지 호출해 문법상의 정적 오류를 확인합니다. 원래 구문 트리에는 타입 노드와 원문 위치를 보존합니다. 사용 코드와 상세 범위는 [TypeScript 안내](grammars/typescript/README.md)와 [한국어 문서](docs/ko/typescript.html)에 정리했습니다.

**TypeScript 전체 문법 지원은 후속 작업입니다.** 제네릭 함수 호출·객체 생성, 타입을 붙인 화살표 함수, `<T>value` 단언, `import type`·`export type { ... }`·`import =`·`export =`, 데코레이터와 TSX가 남아 있습니다. 타입 이름 해석과 타입 호환성 검사는 별도의 타입 검사기 영역입니다. 개발 비교 기준은 고정한 TypeScript 7.0.2 소스이며, [참조 도구와 출처](tools/TypeScript/README.md)를 함께 제공합니다.

## LALR 계산과 문법 검증을 수정합니다

기존 재귀 계산에서는 뒤늦게 추가된 룩어헤드가 이미 계산한 항목까지 전달되지 않는 경우가 있었습니다. 이제 각 `(상태 번호, LR 항목)`의 `LookAheadEntry`에 **룩어헤드 집합과 전달 대상 `Targets`**를 보관합니다. 재귀 계산 중 전달 관계를 기록하고, 계산이 끝나면 `PropagateLookAhead`가 집합이 더 이상 늘어나지 않을 때까지 누락분을 전달합니다.

빈 기호열의 `FIRST`는 `{ε}`를 반환합니다. 빈 FIRST 집합과 생략 가능한 기호열을 구별하므로, 상위 항목의 룩어헤드는 ε을 포함한 경우에만 이어받습니다.

`Grammar.Optimization()`은 유한한 토큰열로 끝낼 수 없는 비단말을 찾아 `GrammarProductivityException`으로 알려줍니다. EBNF 입력에서는 이 오류가 EBNF 진단으로 전달됩니다. 자세한 계산 흐름은 [룩어헤드 구현 문서](docs/ko/lalr-lookahead-propagation.html)에 설명했습니다.

## 0.5.0에서 업그레이드할 때

이번 버전에는 공개 API 변경이 있습니다. 기존 코드를 다음과 같이 옮깁니다.

| 0.5.0 코드 | 0.6.0 코드 |
| --- | --- |
| `new JavaScriptScriptGrammar()` | `new JavaScriptGrammar()` |
| `new JavaScriptModuleGrammar()` | `new JavaScriptGrammar(inputKind: JavaScriptInputKind.Module)` |
| `new JavaScriptExpressionGrammar()` | `new JavaScriptGrammar(inputKind: JavaScriptInputKind.Expression)` |
| `relation.LookAheadTable[key]`로 집합 접근 | `relation.LookAheadTable[key].LookAhead`로 집합 접근 |
| `Dictionary<(int, LRItem), TerminalSet>` 테이블 | `Dictionary<(int, LRItem), CanonicalRelation.LookAheadEntry>` 테이블 |

`Grammar`, `Policy`, `CreateTokenSource(input)`, `Validate(tree)`의 사용 순서는 유지합니다. 문법 준비 시 새 종료 가능성 검사가 실행되므로, 종료 경로가 없는 비단말이 있으면 해당 생성 규칙을 수정해야 합니다.

지원 파서는 `LALRParser`이며 패키지는 .NET Standard 2.1을 대상으로 합니다. 문법 빌더의 전역 상태 때문에 한 프로세스에서 문법을 생성하는 작업은 순차적으로 수행합니다.

## 검증 결과와 범위

2026-10-28 클래스 확장 완료 시점의 전체 회귀 검증 결과는 다음과 같습니다. TypeScript 975개는 엔진 테스트 전체 수에 포함됩니다.

| 검증 | 결과 |
| --- | --- |
| 엔진·언어 지원 테스트 | 7,384개 통과, 기존 SLR 테스트 1개 제외, 실패 0개 |
| TypeScript 테스트 | 975개 통과. 클래스 확장 이전 647개 유지, 관련 328개 추가 |
| MCP 테스트 | 69개 통과 |
| 0.6.0 버전 적용 후 Release 솔루션 빌드 | 오류 0개, 기존 미사용 필드·이벤트 경고 5개 |
| UI 회귀 테스트 | 22개 통과 |
| 게시 인프라 테스트 | 12개 통과 |
| Test262·TypeScript 검증 도구 테스트 | 각각 187개·11개 통과 |
| NuGet 패키징·MCP 게시 빌드 | 성공 |

고정한 Test262 자료의 **48,375개 구문·정적 오류 사례 통과 기록은 2026-10-21 검증 실행**입니다. 이 기록은 실행 당시 바이너리와 입력 해시를 포함하며, 이번 릴리즈 바이너리로 전체 Test262를 다시 실행했다는 의미로 사용하지 않습니다. JavaScript 실행 결과·모듈 해석·타입 검사는 해당 구문 검증의 범위에 포함하지 않습니다.

최종 게시용 패키지와 Playground는 GitHub Actions가 푸시된 커밋에서 다시 테스트·빌드합니다. 새 버전의 패키징과 문서 예제 검증 결과는 출시 준비 산출물 `artifacts/release-0.6.0/`에 보관합니다.

## 푸시와 자동 게시

작업 브랜치 push와 PR은 [검증 워크플로](.github/workflows/verify-release.yml)를 실행합니다. **master에 반영되면 전체 검증 → NuGet 게시 → Playground 배포** 순서로 [출시 워크플로](.github/workflows/publish-nuget.yml)가 실행됩니다. 검증이 실패하면 게시 단계로 넘어가지 않습니다.

문서는 별도 GitHub Pages 워크플로가 배포합니다. 운영 MCP 재배포는 전용 Azure 스크립트로 수행합니다. 이번에는 MCP 컨테이너 빌드에도 새 TypeScript EBNF 리소스가 포함되도록 복사 목록을 보강했습니다. 실제 게시 결과는 [GitHub Actions](https://github.com/AJ-comp/Compiler/actions)에서 확인합니다.

## 저자와 이번 릴리즈

Janglim 핵심 구조는 저자가 직접 설계하고 작성해 온 코드에 기반합니다. 이번 문서·출시 준비의 AI 보조 작업은 저자 본인의 설계와 구현에 대한 저자성을 대체하지 않습니다.

C11 Core 문법과 회귀 예제는 AI 보조로 준비한 별도 자료입니다. 엔진에 담긴 저자의 설계·구현과 이 자료의 작성 맥락은 [C11 Core 안내](grammars/c/README.md#기준과-저자-맥락)에 함께 기록했습니다.

JavaScript·TypeScript 언어 규칙·검사기·테스트와 설명 문서는 저자의 기존 LR 구조 위에 AI 보조로 추가한 언어 지원 계층입니다. 각 문법 안내에 출처와 지원 범위를 기록했습니다.

이전 [0.5.0 출시 노트](https://github.com/AJ-comp/Compiler/blob/1b8f85f/RELEASE_NOTES.md)와 누적 [CHANGELOG](CHANGELOG.md)에서 이전 버전의 API와 검증 기록을 확인할 수 있습니다.
