# ECMAScript 2026 구문·정적 오류 검증

이 도구는 공식 Test262 원문을 `JavaScriptGrammar`에 입력하고, 파싱과 정적 오류 검사의 결과를 Test262 메타데이터와 비교합니다. Script·Module, 엄격·비엄격 모드, Annex B를 각각 맞는 설정으로 검사합니다.

최종 전체 검증에는 아래의 **10월 고정 원본**을 사용합니다. 6월 원본은 이후에 추가·변경된 테스트를 추적하는 기준 자료로 함께 보관합니다. 실제 결과와 건수는 해당 실행의 입력·바이너리 해시를 검증한 보고서에서 확인합니다.

2026-10-21의 최종 수정본은 **48,375건 모두 통과**했습니다. 입력 누락·중복, 예상과 다른 판정, 내부 예외·시간 초과·프로세스 중단은 모두 0건입니다. 세 프로필과 바이너리 해시를 검증한 원본 집계 결과는 [ecmascript-2026-results.json](ecmascript-2026-results.json)에 있습니다. 추가 교차 검증과 수정 내역은 [HTML 검증 보고서](../../docs/ko/javascript-conformance.html)에 정리했습니다.

## 1. 원본과 검증 범위

| 항목 | 기준 |
|---|---|
| 명세 | [ECMAScript 2026, 17판](https://tc39.es/ecma262/2026/multipage/) |
| 공식 테스트 | [tc39/test262](https://github.com/tc39/test262) |
| 최종 전체 검증 원본 | [`d53ebc8b1931d9f1541e43d59650776e4b766248`](https://github.com/tc39/test262/tree/d53ebc8b1931d9f1541e43d59650776e4b766248) |
| 최종 원본 커밋 시각 | 2026-10-13 22:44:04 UTC. 2026-10-21에 확인한 공식 `main`의 최신 커밋 |
| 변경 대조용 6월 원본 | [`6eec1ac9ee144dafd8f344d73a21f36bfc9f6755`](https://github.com/tc39/test262/tree/6eec1ac9ee144dafd8f344d73a21f36bfc9f6755) |
| 6월 원본 선택 기준 | 공식 `main` 이력에서 2026-06-30 23:59:59 UTC 이전의 마지막 커밋 |
| 기본 대상 | `test/language`, `test/annexB`, `test/built-ins/RegExp` |
| 표준별 기능 판단 | [ecmascript-2026-features.json](ecmascript-2026-features.json) |
| 명세 항목 대조 | [ecmascript-2026-coverage.json](ecmascript-2026-coverage.json) |

검증기는 원문을 토큰화하고 파싱한 뒤, `JavaScriptGrammar.Validate`로 정적 오류를 검사합니다. 입력 프로그램의 실행, 모듈 의존성 로딩, `eval`·`Function`·`RegExp`에 전달한 동적 문자열의 실행 결과는 원문 검증의 집계 범위에서 제외합니다.

명세 항목 대조 파일에는 각 항목을 구현한 소스와 관련 테스트 위치를 기록합니다. 이 연결표로 구현 위치를 찾고, 실제 검증 결과는 실행 보고서에서 확인합니다.

### ECMAScript 2026에 맞춰 새 문법을 선별합니다

Test262에는 정식 표준과 제안 단계의 테스트가 함께 들어 있습니다. 따라서 커밋 날짜를 고정한 다음, **동결된 2026판에 없는 새 문법의 제외 이유와 공식 근거를 별도로 기록**합니다.

현재 제외 설정에는 decorators, explicit resource management의 `using`·`await using`, defer import, source phase import 관련 문법이 들어 있습니다. 각 feature 태그의 판단은 `ecmascript-2026-features.json`에서 확인합니다. 기능이 Stage 4에 도달했더라도, 해당 기능의 표준 수록 연도와 2026판의 실제 생성 규칙을 함께 확인합니다.

기존 멤버 접근·함수 호출 문법을 사용하는 새 API의 테스트 원문은 구문 검사에 사용할 수 있습니다. 이 경우 해당 API의 실행 결과는 검사하지 않고, 원문의 구문만 검사합니다. 테스트 실행 결과를 보고 실패 사례를 제외하는 필터는 사용하지 않습니다.

`_FIXTURE` 파일은 다른 모듈이 참조할 보조 파일이므로 독립 테스트에서 제외합니다. `staging`, `intl402`와 기본 대상 밖의 경로도 각각 제외 이유와 수를 기록합니다. 메타데이터가 없거나 형식이 잘못된 파일은 별도 준비 오류로 분류합니다.

### 원본과 라이선스를 함께 보관합니다

공식 ZIP, 추출한 원문, 원본의 [LICENSE](https://github.com/tc39/test262/blob/d53ebc8b1931d9f1541e43d59650776e4b766248/LICENSE)를 `artifacts/ecmascript-2026/` 아래에 보관합니다. 공식 Test262의 LICENSE에는 BSD License와 저작권 고지가 들어 있습니다.

준비 도구는 각 원문의 저작권 주석과 YAML 메타데이터를 유지하고, ZIP·원문·LICENSE의 해시를 기록합니다. 원본이나 준비한 corpus를 옮기거나 공유할 때도 해당 원본의 LICENSE와 저작권 고지를 함께 보관합니다.

## 2. 최신 고정 원본으로 입력 준비

프로젝트 루트에서 실행합니다. Python 3.10 이상, PyYAML과 .NET 8을 빌드할 수 있는 SDK가 필요합니다.

```powershell
python -m pip install PyYAML==6.0.3
```

처음에는 공식 ZIP을 받고 원문을 추출합니다. `--root`에는 비어 있는 새 디렉터리를 지정합니다. `--archive`는 추출이 끝난 뒤 동일한 ZIP에서 원문을 직접 읽어 입력 준비 속도를 높입니다.

```powershell
python tools/Test262/prepare_test262.py `
  --download `
  --revision d53ebc8b1931d9f1541e43d59650776e4b766248 `
  --root artifacts/ecmascript-2026/latest-head-audit/test262 `
  --archive artifacts/ecmascript-2026/latest-head-audit/test262-d53ebc8b1931d9f1541e43d59650776e4b766248.zip `
  --output artifacts/ecmascript-2026/latest-head-audit/test262-latest-es2026.jsonl `
  --exclude-feature-file tools/Test262/ecmascript-2026-features.json
```

이미 받은 원본으로 다시 준비할 때는 `--download`를 생략합니다. 도구는 원본 출처 기록에 저장한 ZIP 해시를 먼저 확인합니다.

```powershell
python tools/Test262/prepare_test262.py `
  --root artifacts/ecmascript-2026/latest-head-audit/test262 `
  --archive artifacts/ecmascript-2026/latest-head-audit/test262-d53ebc8b1931d9f1541e43d59650776e4b766248.zip `
  --output artifacts/ecmascript-2026/latest-head-audit/test262-latest-es2026.jsonl `
  --exclude-feature-file tools/Test262/ecmascript-2026-features.json
```

준비 도구는 다음 세 파일을 만듭니다.

| 파일 | 내용 |
|---|---|
| `test262-latest-es2026.jsonl` | 원문, 입력 종류, 모드, 기대 결과를 포함한 테스트 입력 |
| `test262-latest-es2026.manifest.json` | 고정 커밋, 해시, 대상·제외 통계, 기능별 파일 수 |
| `test262-latest-es2026.selection.jsonl` | 각 원본 파일의 선택·제외 여부와 구체적인 제외 이유 |

## 3. 원문과 모드 처리

준비 도구는 원문을 UTF-8 바이트에서 직접 읽습니다. 원문의 CR·LF 줄 끝, BOM, 해시뱅, 주석과 테스트 본문을 유지합니다. 엄격 모드 변형을 만들 때만 Test262 지침에 따라 맨 앞에 `"use strict";\n`을 붙입니다.

| Test262 플래그 | 생성할 입력 |
|---|---|
| 관련 플래그 없음 | 원문 그대로인 비엄격 Script와 엄격 모드 접두사를 붙인 Script |
| `noStrict` | 원문 그대로인 비엄격 Script |
| `onlyStrict` | 엄격 모드 접두사를 붙인 Script |
| `module` | 원문 그대로인 Module. Module의 엄격 모드 기준 적용 |
| `module`, `raw` 함께 지정 | 해시뱅 등을 그대로 유지한 Module. Module의 엄격 모드 기준 적용 |
| `raw` | 원문 그대로인 비엄격 Script 한 개 |

여기서 비엄격 설정은 Script 전체에 엄격 모드를 강제로 적용하지 않는다는 뜻입니다. 원문에 `"use strict"` 지시문이나 class가 있으면 JavaScript 지원 계층에서 해당 범위의 엄격 모드 규칙을 적용합니다.

모든 입력에서 `webCompatibility: true`를 사용합니다. Module·엄격 모드에 따라 허용 여부가 달라지는 Annex B 구문은 JavaScript 지원 계층에서 해당 문맥으로 검사합니다.

`includes`에 적힌 harness 파일은 메타데이터에 남깁니다. 구문 검사에 사용하는 원문에는 harness 코드를 추가하지 않습니다.

## 4. 메타데이터와 기대 결과

Test262의 오류 단계에 맞춰 **현재 원문이 구문·정적 오류 검사를 통과해야 하는지**를 정합니다.

| Test262 메타데이터 | 기대 결과 | 이유 |
|---|---|---|
| `negative` 없음 | 통과 | 정상 프로그램의 원문이므로 구문 검사를 통과해야 합니다. |
| `negative.phase: parse`, `type: SyntaxError` | 거부 | 구문이나 정적 오류 조건을 위반한 원문입니다. |
| `negative.phase: resolution` | 통과 | 원문 구문 검사를 마친 뒤 모듈 의존성을 연결하는 단계에서 발생할 오류입니다. |
| `negative.phase: runtime` | 통과 | 원문 구문 검사를 마친 뒤 코드를 실행하는 단계에서 발생할 오류입니다. |

예를 들어 `new RegExp("[")`는 호출식 자체의 문법이 올바릅니다. 문자열 `"["`의 정규식 문법 오류는 이 호출을 실행할 때 발생하므로, 원문 구문 검사의 기대 결과는 통과입니다.

`parse` 단계에 다른 오류 타입이 지정되거나 알려지지 않은 단계가 나오면 준비 도구가 별도 진단으로 분류합니다. 준비 도구가 이해하지 못한 메타데이터를 정상 통과 사례로 취급하지 않습니다.

메타데이터 해석의 근거는 고정한 원본의 [INTERPRETING.md](https://github.com/tc39/test262/blob/d53ebc8b1931d9f1541e43d59650776e4b766248/INTERPRETING.md)와 [CONTRIBUTING.md](https://github.com/tc39/test262/blob/d53ebc8b1931d9f1541e43d59650776e4b766248/CONTRIBUTING.md)에서 확인할 수 있습니다.

## 5. 검증용 바이너리 빌드와 전체 실행

다음 예제는 사용자 디렉터리에 설치한 .NET SDK를 사용합니다. SDK 설치 위치가 다르면 `$test262Dotnet`을 해당 경로로 바꿉니다.

실행에 사용할 바이너리와 결과는 같은 재현 디렉터리 아래에서 구분해 보관합니다.

```powershell
$test262Dotnet = "$env:USERPROFILE/.dotnet/dotnet.exe"
$test262RunRoot = "artifacts/ecmascript-2026/reproduction"
$test262RunnerDir = "$test262RunRoot/runner"

& $test262Dotnet build tools/Janglim.Test262/Janglim.Test262.csproj `
  -c Release --nologo --output "$test262RunnerDir"
```

실행 조정 도구는 Script 엄격·비엄격 모드와 Module을 별도 프로세스에서 검사합니다. `--timeout-ms`는 입력 한 개를 검사하는 시간 제한이며, 실행 manifest에 기록합니다.

```powershell
python tools/Test262/run_test262.py `
  --input artifacts/ecmascript-2026/latest-head-audit/test262-latest-es2026.jsonl `
  --output-dir "$test262RunRoot/results" `
  --runner "$test262RunnerDir/Janglim.Test262.dll" `
  --dotnet "$test262Dotnet" `
  --timeout-ms 120000
```

같은 입력·바이너리·실행 설정으로 중단한 검증을 이어갈 때는 `--resume`을 추가합니다.

```powershell
python tools/Test262/run_test262.py `
  --input artifacts/ecmascript-2026/latest-head-audit/test262-latest-es2026.jsonl `
  --output-dir "$test262RunRoot/results" `
  --runner "$test262RunnerDir/Janglim.Test262.dll" `
  --dotnet "$test262Dotnet" `
  --timeout-ms 120000 `
  --resume
```

입력, 제품·runner 바이너리 또는 실행 설정이 바뀌면 새 결과 디렉터리에서 전체 검증을 시작합니다. `--resume`은 기존 `run-manifest.json`의 해시와 설정이 일치하는 경우에만 허용됩니다.

### 구문 거부와 실행 실패를 구분합니다

거부를 기대한 입력에서 파서가 구문 위반이나 정적 오류를 확인하면 `pass`로 기록합니다. 이때 정상적인 구문 오류를 알리는 `ParsingException`·`JavaScriptSyntaxException`·`PredictionFailedException`이 결과에 함께 기록될 수 있습니다.

반면 예상하지 못한 내부 예외는 `error`, 시간 초과는 `timeout`, 비정상 프로세스 종료는 `crash`로 남깁니다. **입력 거부를 기대한 사례에서도 error·timeout·crash를 구문 거부 성공으로 계산하지 않습니다.**

실행 조정 도구는 중단된 프로세스의 다음 입력부터 새 프로세스로 이어갈 수 있습니다. 이미 기록한 실패도 결과에 그대로 남기므로, 모든 입력을 처리했다는 사실과 모든 입력을 통과했다는 사실을 구분할 수 있습니다.

## 6. 원본 결과 대조와 최종 보고서

전체 실행이 끝나면 집계 도구가 입력 원본과 각 결과를 전건 대조합니다. `--binary-dir`에는 해당 실행에 사용한 바이너리 디렉터리를 지정해 DLL·JSON의 실제 해시까지 검증합니다.

```powershell
python tools/Test262/summarize_test262.py `
  --input artifacts/ecmascript-2026/latest-head-audit/test262-latest-es2026.jsonl `
  --results-dir "$test262RunRoot/results" `
  --binary-dir "$test262RunnerDir" `
  --output "$test262RunRoot/conformance-report.json"
```

집계 도구는 입력 해시와 준비 manifest, 실행 manifest, `summary.json`을 먼저 대조합니다. 이후 세 프로필의 모든 결과를 ID·원문 해시·입력 종류·모드·변형·기대값과 비교합니다. 같은 ID가 다른 원문 해시로 중복된 경우도 오류로 처리합니다.

누락·중복·해시 불일치, 실제 결과와 summary의 모순, 검증 중 증거 파일의 변경이 있으면 새 보고서를 발행하지 않습니다. 증거 대조를 마친 실행에서 실제 실패가 발견되면 `passed: false`인 보고서를 만듭니다.

| 집계 명령의 종료 코드 | 의미 |
|---|---|
| `0` | 전체 증거를 확인했고 모든 입력의 실제 결과가 기대값과 일치합니다. |
| `1` | 전체 증거를 확인했으며 실제 결과에 실패가 있습니다. |
| `2` | 증거가 누락되거나 모순되어 보고서를 확정할 수 없습니다. |

증거 오류가 생기면 기존 보고서는 보존됩니다. 따라서 이번 집계 명령의 종료 코드와 보고서의 입력·실행 해시를 함께 확인합니다.

### 보고서 schemaVersion 1

| 필드 | 의미 |
|---|---|
| `scope` | 원문 구문·정적 오류 검증의 범위 |
| `complete` | 선정한 입력 전부와 세 프로필의 결과 대조를 완료했는지 여부 |
| `passed` | 모든 입력의 `outcome`이 `pass`인지 여부 |
| `input` | 입력 JSONL·준비 manifest의 경로와 해시, 원본 커밋·ZIP, 선정 경로·기능 필터와 준비 통계 |
| `run` | 실행 manifest·summary의 해시, runner·제품 바이너리 해시, 시간 제한과 실제 파일 검증 여부 |
| `counts` | 원본 파일·변형 수, 통과·거부 기대 수, 메타데이터 분류, 실제 outcome·stage 집계 |
| `profiles` | 각 프로필의 입력·기대 결과·분류·실제 결과 집계 |
| `checks` | 중복·누락·증거 변경과 예상치 못한 예외·시간 초과·프로세스 종료 등의 검사 결과 |
| `rawResults` | 각 프로필 원본 JSONL과 실패 sidecar의 경로·해시·기록 수 |

`complete: true`인 실행에도 실제 실패가 있을 수 있습니다. 전체 통과 여부는 `passed`와 `counts.outcomes`를 확인합니다.

`counts.sourceFiles`는 원본 파일 수이고, `counts.variants`는 실제 검사 입력 수입니다. 하나의 원본 파일이 엄격·비엄격 모드의 두 입력으로 펼쳐질 수 있습니다. `counts.classifications`에서는 정상 원문, parse-negative, resolution-negative-source-only, runtime-negative-source-only의 수를 각각 확인합니다.

`checks.unexpectedExceptions`, `checks.timeouts`, `checks.crashes`는 실행 실패의 수입니다. 정상적인 구문 거부에 붙은 예외는 `checks.syntaxExceptionRejections`에서 따로 집계합니다.

`run.binaryFilesVerified`는 `--binary-dir`로 실제 파일까지 대조했을 때 `true`입니다. 이 옵션을 생략하면 실행 기록에 저장한 바이너리 해시를 보고서에 보관합니다.

준비 통계의 `excluded:feature_filter`는 기능 필터로 제외한 파일의 유일 건수입니다. 한 파일에 여러 feature 태그가 붙을 수 있으므로 기능별 제외 수를 합산할 때는 중복을 고려합니다.

## 7. 6월 원본과 최신 원본의 변경 대조

명세가 확정된 뒤에도 Test262에는 새로운 오류 사례가 추가될 수 있습니다. 6월 원본과 최신 고정 원본을 비교하면 어떤 입력이 추가·변경됐는지 추적할 수 있습니다.

6월 원본이 필요하면 별도의 빈 디렉터리에 준비합니다. 기본 `--revision`이 6월 고정 커밋이므로 다음 명령에서 그 값을 생략할 수 있습니다.

```powershell
python tools/Test262/prepare_test262.py `
  --download `
  --root artifacts/ecmascript-2026/test262 `
  --archive artifacts/ecmascript-2026/test262-6eec1ac9ee144dafd8f344d73a21f36bfc9f6755.zip `
  --output artifacts/ecmascript-2026/test262-es2026-archive.jsonl `
  --exclude-feature-file tools/Test262/ecmascript-2026-features.json
```

비교 도구는 각 입력의 다음 일곱 값을 비교합니다.

`id`, `sourceSha256`, `expectAccept`, `inputKind`, `mode`, `webCompatibility`, `variant`

일곱 값이 모두 같으면 재사용 가능한 입력으로 기록합니다. 새 ID가 생기거나 값이 하나라도 달라지면 최신 원문 전체를 delta JSONL에 넣습니다. 주석·메타데이터만 바뀌었더라도 원문 해시가 달라지므로 delta에 포함합니다. 최신 원본에서 삭제된 ID도 비교 기록에 남깁니다.

```powershell
python tools/Test262/compare_test262.py `
  --baseline artifacts/ecmascript-2026/test262-es2026-archive.jsonl `
  --latest artifacts/ecmascript-2026/latest-head-audit/test262-latest-es2026.jsonl `
  --output artifacts/ecmascript-2026/latest-head-audit/test262-latest-delta.jsonl
```

비교 도구는 delta 입력, `.manifest.json`, `.comparison.jsonl`을 만듭니다. 두 원본 입력과 manifest는 그대로 유지합니다.

재사용 가능한 입력의 결과를 실제로 재사용하려면 제품·runner 바이너리와 실행 설정도 같아야 합니다. delta는 추가·변경 사례를 따로 조사할 때 사용하고, 최종 전체 보고서는 5·6절의 최신 전체 입력 실행으로 생성합니다.

## 8. 부분 실행과 도구 테스트

특정 문법을 조사할 때는 `--include-prefix`로 경로를 좁힐 수 있습니다. 이 옵션을 여러 번 지정하면 여러 경로를 함께 선택합니다.

```powershell
python tools/Test262/prepare_test262.py `
  --root artifacts/ecmascript-2026/latest-head-audit/test262 `
  --archive artifacts/ecmascript-2026/latest-head-audit/test262-d53ebc8b1931d9f1541e43d59650776e4b766248.zip `
  --output artifacts/ecmascript-2026/module-cases.jsonl `
  --include-prefix language/module-code `
  --exclude-feature-file tools/Test262/ecmascript-2026-features.json
```

`--limit`, `--offset`, `--shard-index`, `--shard-count`로 입력을 나눌 수도 있습니다. 준비 도구는 전체 계획 건수와 이번 출력에서 빠진 건수를 manifest에 남깁니다. 집계 도구는 준비 과정에서 변형이 생략된 입력을 전체 완료 보고서로 확정하지 않습니다.

준비·비교·실행 조정·집계 도구의 회귀 테스트를 함께 실행합니다.

```powershell
python -m unittest discover -s tools/Test262 -p "test_*.py" -v
```

## 9. 독립적인 보완 검사

공식 원문 검증과 별도로 생성한 문법 사례나 참조 엔진과의 교차 검증을 사용할 수 있습니다. 보완 검사의 입력·기대값·바이너리·결과를 별도 자료로 보관하고, 공식 Test262 입력의 집계와 구분합니다.

### 기본 선택 경로 밖의 구문 오류 테스트도 확인합니다

기본 선택 경로에는 언어 문법·Annex B·정규식 테스트가 들어 있습니다. 그 밖의 내장 객체 테스트에도 원문 자체의 구문 오류를 검사하는 사례가 있는지 확인하려면, 같은 고정 ZIP을 추가로 조사합니다.

```powershell
python tools/Test262/extract_additional_parse_negatives.py `
  --root artifacts/ecmascript-2026/latest-head-audit/test262 `
  --archive artifacts/ecmascript-2026/latest-head-audit/test262-d53ebc8b1931d9f1541e43d59650776e4b766248.zip `
  --output artifacts/ecmascript-2026/reproduction/additional-parse-negatives.jsonl `
  --feature-filter tools/Test262/ecmascript-2026-features.json
```

이 도구는 기본 선택 경로 밖에서 `negative.phase: parse`, `negative.type: SyntaxError`인 파일을 찾습니다. 2026판 기능 필터와 엄격·비엄격·Module 변형 기준은 기본 준비 도구와 같습니다. 원문·변형별 해시는 추가 JSONL과 `.manifest.json`에 보관합니다.

`staging`에서 찾은 사례는 `supplementary-staging`으로 따로 표시하고, 기본 경로 밖의 정식 테스트와 구분합니다. 이 보완 사례를 실행하더라도 기본 corpus의 결과에 합산하지 않습니다.

독립 메타데이터가 없는 `_FIXTURE`는 보조 파일로 기록합니다. 실제 테스트의 메타데이터가 잘못됐다면 도구는 오류를 manifest에 남기고 비정상 종료합니다. 이 경우 manifest의 `complete`도 `false`이므로, 선택된 추가 사례가 없다는 이유만으로 누락이 없다고 결론 내리지 않습니다. 고정 commit의 출처 기록·ZIP 해시·ZIP 최상위 경로가 맞지 않으면 입력 준비 자체를 중단합니다.

### 정규식 Pattern을 참조 엔진과 비교합니다

[audit_regexp_patterns.py](audit_regexp_patterns.py)는 정규식 Pattern 검사에 사용하는 도구입니다. 고정 corpus의 정적인 문자열에서 추출하거나 직접 생성한 패턴을 Janglim과 Node에 전달해 컴파일 허용 여부를 비교합니다. 이 검사에서는 정규식 매칭이나 원본 JavaScript 프로그램의 평가를 수행하지 않습니다.

정규식 교차 검증 결과에는 Node·Unicode 버전, 정확한 UTF-16 입력 해시, 불일치와 예상치 못한 예외를 기록합니다. 참조 엔진과 차이가 생기면 동결된 명세를 기준으로 원인을 확인합니다. 이 보완 결과는 해당 도구의 별도 `summary.json`과 패턴 보고서에서 확인합니다.
