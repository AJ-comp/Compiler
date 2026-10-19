# Janglim MCP 사용 가이드

Janglim MCP는 Claude Code·Codex가 작성한 EBNF 문법을 실제 Janglim 엔진으로 검증하고,
입력 문자열을 파싱하며, 정상·오류 예제를 반복 검사하는 .NET 10 서비스입니다.
사용자는 기존 AI 코딩 도구의 대화에서 문법을 요청하고, AI는 장림의 검사 결과를
받아 문법과 프로젝트 파일을 수정합니다.

이 서비스는 저자가 설계한 기존 LALR 엔진을 호출합니다. 문법 작성과 수정에 쓰는
AI 추론은 사용자의 Claude Code·Codex에서 수행합니다. 서버가 사용자 프로젝트의
파일을 읽거나 수정하지는 않습니다.

## 운영 서버 연결

2026년 10월 11일 Azure Container Apps에 배포하고 실제 HTTPS 연결, 인증, 문법 검증,
파싱·트리 위치, 정상·오류 예제와 충돌 거부를 확인했습니다.
같은 날 `c11-core` 버전 `1.0.0`과 조회 도구 두 개를 포함한 이미지를 재배포했습니다.
원격 15개 점검에서 기준 문법의 해시·로컬 원본 일치, 수정 사본 검증과 원본 불변성도 확인했습니다.

- [운영 MCP 엔드포인트](https://janglim-mcp.wonderfulsand-6b5e00a1.koreacentral.azurecontainerapps.io/mcp)
- [상태 확인](https://janglim-mcp.wonderfulsand-6b5e00a1.koreacentral.azurecontainerapps.io/health)

사용자는 운영자가 전달한 API 키를 클라이언트의 `JANGLIM_MCP_API_KEY` 환경 변수에
설정하고 아래 Codex·Claude Code 설정을 추가합니다. 원격 서버를 이용하는 사용자에게
.NET SDK나 Docker 설치는 필요하지 않습니다. 운영 서버를 관리하는 Azure 계정의 키
조회 절차는 [운영자용 키 조회](#운영자용-키-조회)에 정리했습니다.

키를 셸 기록에 직접 쓰지 않고 입력하는 PowerShell 예입니다.

```powershell
$janglimClientSecret = Read-Host 'Janglim MCP API key' -AsSecureString
$env:JANGLIM_MCP_API_KEY = [System.Net.NetworkCredential]::new('', $janglimClientSecret).Password
$janglimClientSecret = $null
```

키를 설정한 환경에서 Claude Code·Codex를 실행합니다. 이미 실행 중인 데스크톱 앱에는
새 셸의 환경 변수가 자동 전달되지 않으므로 변수를 전달한 환경에서 앱을 다시 시작합니다.
MCP 경로는 인증한 MCP 클라이언트에서 사용하며, 브라우저 주소창에서 열면 `401` 응답을
받을 수 있습니다. 브라우저로 서버 상태를 확인할 때는 `/health`를 사용합니다.

## 첫 버전의 범위

| 도구 | 입력 | 결과 |
| --- | --- | --- |
| `list_grammars` | 없음 | 내장 기준 문법의 ID, 버전, 지원 범위, 제한, SHA-256 |
| `get_grammar` | `grammarId` | 선택한 기준 문법의 메타데이터와 EBNF 사본 |
| `validate_grammar` | `grammar` | EBNF 오류, LALR 파싱 테이블 충돌, 문법 통계 |
| `parse_input` | `grammar`, `input`, 선택적 `includeTree` | 파싱 성공 여부, 토큰과 위치, 진단, 선택적 구문 트리 |
| `run_tests` | `grammar`, `cases` | 예제별 기대 결과와 실제 결과, 전체 통과 여부 |

전송 방식은 **stateless Streamable HTTP**이며 엔드포인트는 `/mcp`입니다.
별도의 stdio 서버나 데스크톱 UI를 설치할 필요는 없습니다. 원격 서버를 사용하면
사용자 PC에 .NET SDK를 설치할 필요도 없습니다. 직접 로컬 서버를 실행하는 개발자는
.NET 10 SDK가 필요합니다.

도구 이름은 `snake_case`, JSON 인수와 결과의 필드 이름은 `camelCase`입니다.
`/mcp`는 MCP 프로토콜 엔드포인트이며 도구 이름별 REST 경로는 제공하지 않습니다.

첫 버전은 내장 기준 문법 조회와 EBNF 텍스트의 검증·파싱을 제공합니다. 독립적인 파서 소스 코드 생성,
사용자 코드·의미 동작 실행, 프로젝트 파일 접근, OAuth 로그인은 제공하지 않습니다.
웹 Playground의 기존 브라우저 파싱 경로는 이 서비스와 독립적으로 동작합니다.

## 내장 기준 문법

`c11-core`는 [C11 Core 기준 문법](../grammars/c/README.md)의 내장 ID이며, 초기 버전은
`1.0.0`입니다. 서버는 빌드할 때 기준 EBNF를 내장합니다. 이 프로필은 C의 선언·함수·
표현식·제어문을 수정하기 위한 출발점이며, 전처리·`typedef` 이름 해석·의미 검사를
포함한 전체 C 구현은 아닙니다. 응답의 `supportedFeatures`와 `limitations`를 먼저 확인합니다.

**내장 기준 문법은 읽기 전용입니다.** 사용자가 수정을 요청해도 서버의 등록된 파일은
바뀌지 않습니다. AI는 받은 사본을 대화나 사용자 프로젝트에서 수정하고, 수정된
EBNF 텍스트를 기존 검사 도구의 `grammar` 인수로 보냅니다. 사용자별 수정본이나
프로젝트를 서버에 영구 저장하지 않으며 다른 사용자에게 공유하지 않습니다.
기준 문법의 업데이트는 저장소 변경과 관리자에 의한 서비스 릴리즈·재배포로만 관리합니다.

원격 서버가 제공하는 문법과 도구는 배포된 서비스 버전에 따라 결정됩니다.
저장소 변경만으로 운영 서버의 내장 문법이 업데이트되지는 않습니다.

## 로컬 실행

저장소 루트에서 실행합니다.

```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
dotnet run --project services/Janglim.Mcp/Janglim.Mcp.csproj --no-launch-profile -- --urls http://localhost:5080
```

- MCP: `http://localhost:5080/mcp`
- 상태 확인: `http://localhost:5080/health`

Development 환경에서 API 키가 없으면 실제 TCP 연결의 원격 주소가 loopback인 요청만
허용합니다. 이 모드는 로컬 개발용입니다. 다른 PC나 프록시를 통해 연결하려면 키를
설정하고 접근할 호스트 이름도 명시해야 합니다. 기본 허용 호스트는
`localhost`, `127.0.0.1`, `[::1]`입니다.

Development 이외의 환경은 `JanglimMcp__ApiKey`가 없으면 시작하지 않습니다.
키를 설정하는 경우 실행 환경에 관계없이 32–256자 길이여야 합니다.
키는 비밀 관리자에서 생성한 충분히 무작위인 값을 사용하고, 서버의 환경 변수나
호스팅 서비스의 secret으로 전달합니다. 키를 명령행 인수, 소스 파일, Docker 이미지,
웹 프런트엔드에 넣지 않습니다.

PowerShell에서 기존 키를 기록에 남기지 않고 입력하는 예입니다.

```powershell
$janglimSecret = Read-Host 'Janglim MCP API key' -AsSecureString
$env:JanglimMcp__ApiKey = [System.Net.NetworkCredential]::new('', $janglimSecret).Password
$env:ASPNETCORE_ENVIRONMENT = 'Production'
$env:AllowedHosts = 'localhost'
dotnet run --project services/Janglim.Mcp/Janglim.Mcp.csproj --no-launch-profile -- --urls http://localhost:5080
```

원격 서비스에는 HTTPS를 사용합니다. 로컬 HTTP 예제는 loopback 개발 연결용입니다.

## Codex 연결

Codex 설정 파일 `~/.codex/config.toml`에 다음 항목을 추가합니다.

```toml
[mcp_servers.janglim]
url = "https://janglim-mcp.wonderfulsand-6b5e00a1.koreacentral.azurecontainerapps.io/mcp"
bearer_token_env_var = "JANGLIM_MCP_API_KEY"
startup_timeout_sec = 60
tool_timeout_sec = 30
```

Codex를 시작하는 프로세스의 환경에 `JANGLIM_MCP_API_KEY`를 설정합니다.
이 변수의 값은 서버의 `JanglimMcp__ApiKey`와 일치해야 합니다. 설정 파일에는 키 값
대신 환경 변수 이름을 저장합니다. 이미 실행 중인 데스크톱 앱은 새 셸의 환경 변수를
자동으로 받지 않으므로 앱을 실행하는 환경에도 변수를 전달해야 합니다.

키 없는 loopback Development 서버를 연결할 때는 URL을
`http://localhost:5080/mcp`로 바꾸고 `bearer_token_env_var` 항목을 생략합니다.
연결 설정과 인증 옵션은 [Codex 공식 MCP 문서](https://learn.chatgpt.com/docs/extend/mcp)를
참고합니다.

## Claude Code 연결

프로젝트 루트의 `.mcp.json`에 다음 설정을 추가합니다. 다른 MCP 서버가 있다면 기존
`mcpServers` 객체 안에 `janglim` 항목만 합칩니다.

```json
{
  "mcpServers": {
    "janglim": {
      "type": "http",
      "url": "https://janglim-mcp.wonderfulsand-6b5e00a1.koreacentral.azurecontainerapps.io/mcp",
      "headers": {
        "Authorization": "Bearer ${JANGLIM_MCP_API_KEY}"
      },
      "timeout": 60000
    }
  }
}
```

Claude Code를 시작하는 환경에 `JANGLIM_MCP_API_KEY`를 설정하고 `/mcp`에서 연결
상태를 확인합니다. `.mcp.json`의 환경 변수 참조를 그대로 유지하면 실제 키를 저장소에
기록하지 않고 연결 설정을 공유할 수 있습니다. 프로젝트 MCP를 처음 사용할 때 표시되는
Claude Code의 연결 승인 절차를 따릅니다.

위 `timeout`은 도구 호출 제한 시간입니다. 유휴 인스턴스의 첫 연결을 기다리는 시작
제한 시간도 60초로 설정하려면 같은 PowerShell 환경에서 다음과 같이 실행합니다.

```powershell
$env:MCP_TIMEOUT = '60000'
claude
```

시작 제한 시간과 도구 호출 제한 시간의 차이는
[Claude Code 공식 MCP 문서](https://code.claude.com/docs/en/mcp)에 설명되어 있습니다.

키 없는 로컬 개발 연결은 다음 명령으로 추가할 수도 있습니다.

```shell
claude mcp add --transport http janglim http://localhost:5080/mcp
```

HTTP 연결과 `.mcp.json`의 환경 변수 치환은
[Claude Code 공식 MCP 문서](https://code.claude.com/docs/en/mcp)에 설명되어 있습니다.
이 서비스의 인증은 공유 Bearer 키이며 OAuth 로그인 명령을 사용하는 방식은 아닙니다.

## EBNF 토큰 종류 지정

EBNF 리더는 정규식 앞에 토큰 종류를 선택적으로 지정할 수 있습니다.
아래 토큰 종류와 거부 규칙 검사는 운영 MCP 서버에도 배포되어 있습니다.

```ebnf
Value : id | text | ch | number ;
id    := Identifier "[a-zA-Z_][a-zA-Z_0-9]*" ;
text  := StringLiteral """[^""\r\n]*""" ;
ch    := CharLiteral "'[^'\r\n]'" ;
number := NumberLiteral "[0-9]+(?:\.[0-9]+)?" ;
comment := Comment "//[^\r\n]*|/\*[\s\S]*?\*/" ;
invalid_comment := Reject "/\*(?:(?!\*/)[\s\S])*\z" ;
```

| 토큰 종류 | 동작 |
| --- | --- |
| `Identifier` | 기존 식별자용 단어 경계 처리 |
| `StringLiteral` | 단어 경계 없이 원래 정규식으로 문자열 토큰 처리 |
| `CharLiteral` | 단어 경계 없이 원래 정규식으로 문자 토큰 처리 |
| `NumberLiteral` | 단어 경계 없이 원래 정규식으로 숫자 토큰 처리 |
| `Comment` | 정규식으로 주석을 인식하고 파싱 대상에서 제외 |
| `Reject` | 정규식으로 거부할 입력을 표시하고 파싱 전에 오류로 반환 |

종류 이름은 대소문자를 구분하며 위 여섯 가지만 허용합니다. 종류를 생략한
`id := "[a-zA-Z]+" ;`는 기존과 동일하게 `Identifier`로 처리합니다. 이 확장은
저자가 설계한 기존 토큰 종류와 트리형 문법 API를 EBNF 텍스트에서도 선택하게 합니다.

`Comment`와 `Reject`는 어휘 분석 전용 규칙이므로 문법의 선택지에서 참조할 수 없습니다.
직접 API를 사용한다면 어휘 분석 후 각 토큰의 terminal을 `EbnfGrammar.IsRejectedToken`으로
확인하고 파싱 전에 거부해야 합니다. 현재 소스의 MCP 어댑터는 이 검사를 자동으로 수행합니다.
위 `invalid_comment`는 닫히지 않은 블록 주석을 `/`와 `*` 연산자로 잘못 해석하는 것을 막습니다.

큰따옴표로 감싼 정규식 안에서는 `""`가 큰따옴표 한 개를 나타냅니다. 백슬래시는
그대로 전달하므로 위 예제의 `\r`, `\n`은 정규식 엔진에서 해석합니다. 작은따옴표로
감싼 문법 리터럴의 문법은 바뀌지 않습니다. MCP의 JSON 문자열 안에 문법을 넣을 때는
JSON의 큰따옴표·백슬래시 이스케이프를 별도로 적용합니다.

위 문자열·문자 패턴은 간단한 예제입니다. 토큰 종류가 C의 이스케이프를 자동으로
검사하거나 값을 디코딩하지는 않으며 허용할 입력은 정규식으로 정합니다.
[C11 Core 기준 문법과 회귀 예제](../grammars/c/README.md)는 저장소에서 제공하며,
MCP의 `get_grammar`로 기준 문법 사본을 가져올 수 있습니다.
전처리·`typedef` 문맥 처리·의미 검사를 포함한 전체 C 컴파일 기능은 제공하지 않습니다.

## 도구 호출 예제

다음 JSON은 MCP `tools/call`의 `params`에 해당합니다. MCP 클라이언트는 연결 초기화와
프로토콜 헤더를 처리한 뒤 도구를 호출합니다. 아래 JSON만 일반 HTTP POST로 보내는
방식은 지원하지 않습니다.

### 기준 문법 조회

```json
{
  "name": "list_grammars",
  "arguments": {}
}
```

결과는 `status`와 `grammars` 배열입니다. 각 항목은 `id`, `name`, `version`, `language`,
`standard`, `description`, `supportedFeatures`, `limitations`, `sha256`을 포함하며,
목록에는 EBNF 본문을 넣지 않습니다.

```json
{
  "name": "get_grammar",
  "arguments": {
    "grammarId": "c11-core"
  }
}
```

성공 결과는 `status: "ok"`와 `preset`입니다. `preset`에는 목록의 메타데이터와 함께
`grammar`에 EBNF 텍스트가 들어 있습니다. 반환 텍스트는 줄바꿈을 LF로 통일하며,
`sha256`은 그 텍스트의 UTF-8 바이트에 대한 SHA-256을 소문자 16진수로 표현한 값입니다.
사용자는 `id`·`version`·`sha256`을 수정본과 함께 기록할 수 있습니다.
알 수 없는 ID는 `status: "unknown_grammar"`를 반환하며 문법 사본을 제공하지 않습니다.

AI는 `preset.grammar`의 사본을 수정한 뒤 아래 검사 도구에 텍스트 자체를 전달합니다.
검사 도구의 `grammar` 인수에 `c11-core` 같은 ID를 대신 넣지는 않습니다.

### 문법 검증

```json
{
  "name": "validate_grammar",
  "arguments": {
    "grammar": "Expr : Expr '+' Term | Term ;\nTerm : Term '*' Factor | Factor ;\nFactor : '(' Expr ')' | id ;\nid := \"[a-zA-Z]+\" ;"
  }
}
```

EBNF를 읽고 LALR 테이블을 구성합니다. 결과의 진단과 충돌 정보를 보고 AI가 문법을
수정할 수 있습니다. 충돌 보고는 현재 LALR 테이블에서 발생한 shift/reduce 또는
reduce/reduce 충돌을 의미하며, 모든 문법 모호성을 판정하는 증명은 아닙니다.
`parse_input`과 `run_tests`는 충돌이 있는 문법의 파싱을 거부하므로 먼저 충돌을 해결합니다.

### 입력 파싱

```json
{
  "name": "parse_input",
  "arguments": {
    "grammar": "Expr : Expr '+' Term | Term ;\nTerm : Term '*' Factor | Factor ;\nFactor : '(' Expr ')' | id ;\nid := \"[a-zA-Z]+\" ;",
    "input": "a + a * a",
    "includeTree": true
  }
}
```

`includeTree`의 기본값은 `false`입니다. 기본 응답에는 성공 여부, 진단과 제한된 토큰
정보를 포함하고, 트리가 필요한 요청에서만 구문 트리를 추가합니다. 트리는 노드와
자식 ID 목록으로 표현해 중첩된 객체를 반복 전송하지 않습니다. 위치는 입력 문자열의
UTF-16 단위를 기준으로 해석하며 `end` 위치는 해당 범위에 포함하지 않습니다.
출력이 잘렸는지 나타내는 필드도 함께 확인합니다.

이 트리는 문법에 따른 구문 구조입니다. 검색 실행, 계산 결과, 사용자 도메인의 의미
해석은 해당 프로젝트에서 별도로 구현해야 합니다. 전체 파싱 trace와 ACTION/GOTO
테이블을 덤프하는 도구는 첫 버전에 포함하지 않습니다.

### 정상·오류 예제 테스트

```json
{
  "name": "run_tests",
  "arguments": {
    "grammar": "Expr : Expr '+' Term | Term ;\nTerm : Term '*' Factor | Factor ;\nFactor : '(' Expr ')' | id ;\nid := \"[a-zA-Z]+\" ;",
    "cases": [
      { "name": "precedence", "input": "a + a * a", "expectSuccess": true },
      { "name": "parentheses", "input": "(a + a) * a", "expectSuccess": true },
      { "name": "incomplete", "input": "a +", "expectSuccess": false }
    ]
  }
}
```

`expectSuccess: false`인 예제가 파싱에 실패하면 그 테스트는 통과합니다. 이는 입력의
수용·거부를 검사하는 기능이며, 트리의 형태나 실행 결과를 자동으로 비교하는 assertion은
아닙니다. 우선순위의 트리 형태를 확인하려면 `parse_input`의 `includeTree: true`를
함께 사용합니다. 문법 컴파일 자체가 실패하거나 요청의 제한을 넘은 경우는 예상한 입력
거부와 구분해서 확인해야 합니다.

### 결과 해석

도구 결과의 JSON 객체는 MCP `structuredContent`로 반환합니다. 문법 조회 결과는
위 `grammars`·`preset` 계약을 사용합니다. 검증·파싱·테스트 도구의 공통 필드는
`status`, `message`, `grammar`, `parse`, `tests`이며 사용하지 않는 결과는 `null`이거나
SDK 직렬화 설정에 따라 필드 자체가 생략될 수 있습니다.
`status: "ok"`는 요청의 검사가 정상적으로 끝났다는 뜻입니다. 입력의 수용 여부는
`parse.success`, 테스트 통과 여부는 `tests.failed`와 예제별 `passed`를 따로 확인합니다.
검증 도구의 `status: "ok"`와 `grammar.hasConflicts: true`도 함께 나올 수 있습니다.

| `status` | 의미와 대응 |
| --- | --- |
| `ok` | 작업 완료; 파싱·테스트 성공 여부와 문법 충돌은 각 결과 필드에서 확인 |
| `invalid_request` | 필수 인수, 길이 또는 테스트 개수 조건을 수정 |
| `unknown_grammar` | `list_grammars`에서 제공하는 ID를 확인한 뒤 `get_grammar`를 재시도 |
| `invalid_grammar` | EBNF 문법이나 토큰 정규식의 진단을 확인하고 수정 |
| `grammar_conflict` | 보고된 LALR 테이블 충돌을 해결한 뒤 파싱·테스트를 재시도 |
| `runtime_limit` | 시간, 토큰, 상태 또는 규칙 제한을 넘었거나 정상 수용 전에 파싱이 중단됨; 문법·입력을 줄여 재시도 |
| `output_limit` | worker의 출력이 제한을 넘음; 반환 데이터와 입력을 줄여 재시도 |
| `busy` | 사용 가능한 worker가 없음; 잠시 후 재시도 |
| `worker_error` | 엔진 또는 worker가 작업을 완료하지 못함; 반복되면 운영자가 원인 확인 |

| 결과 | 주요 필드 |
| --- | --- |
| `grammar` | `isValid`, `hasConflicts`, `stateCount`, `ruleCount`, `conflicts`, `diagnostics`, `conflictsTruncated` |
| `parse` | `success`, `tokenCount`, `diagnostics`, `tokens`, `tokensTruncated`, `tree`, `treeTruncated` |
| `tests` | `total`, `passed`, `failed`, `cases` |
| `tests.cases[]` | `name`, `expectSuccess`, `actualSuccess`, `passed`, `diagnostics` |
| 진단 | `code`, `message`, 선택적 `start`, `end` |
| 토큰 | `symbol`, `text`, `start`, `end` |
| 트리 노드 | `id`, `kind`, `symbol`, 선택적 `text`, `start`, `end`, `children` |

트리 루트의 ID는 `0`이고 `children`은 같은 배열의 노드 ID를 참조합니다. `kind`는
`terminal` 또는 `nonterminal`입니다. 위치를 확정할 수 없는 노드의 `start`·`end`는
`null`이며, 트리가 잘렸다면 조상 노드의 전체 입력 범위를 확정할 수 없는 경우도 있습니다.
문법 진단은 원래 엔진의 문자열 진단을 바탕으로 하므로 모든 EBNF 오류에 소스 위치가
제공되는 것은 아닙니다.

HTTP 단계에서 거부된 요청은 위 객체가 없을 수 있습니다. 키가 잘못되면 `401`, 허용하지
않은 Origin이나 키 없는 비-loopback 개발 연결은 `403`, 본문이 너무 크면 `413`, 요청량이나
동시 HTTP 요청 제한은 `429`로 거부합니다. 먼저 연결·인증 문제를 해결한 뒤 도구 결과를
확인합니다. 필수 JSON 필드 누락이나 형식 오류는 MCP 프로토콜 오류로 거부될 수도 있습니다.
`run_tests`의 각 예제에는 `name`, `input`, `expectSuccess`를 모두 명시합니다.

## AI와 함께 작업하는 방법

연결 후 다음과 같이 요청할 수 있습니다.

> 덧셈·곱셈과 괄호를 지원하는 EBNF 문법을 만들어줘. 장림으로 문법을 검증하고,
> `a + a * a`의 트리에서 곱셈 우선순위를 확인해줘. 정상·오류 예제도 검사해서
> 프로젝트에 문법과 예제 파일을 저장해줘.

AI는 `validate_grammar` → `parse_input` → `run_tests` 순서로 검사하고 검사 결과를
근거로 문법을 수정합니다. 파일 저장은 Claude Code·Codex가 사용자 프로젝트에서
수행합니다. 장림 서버에 프로젝트 경로나 파일 접근 권한을 줄 필요는 없습니다.

C 문법을 수정하려면 다음과 같이 요청할 수 있습니다.

> 장림 MCP에서 C11 Core 기준 문법을 가져와서 `if` 대신 `unless`를 쓰도록 사본을
> 수정해줘. 새 표기의 수락과 기존 표기의 거부를 검사하고, 다른 제어문 예제도 확인해줘.
> 사용한 기준의 ID·버전·SHA-256과 수정된 EBNF를 내 프로젝트에 저장해줘.

AI는 `get_grammar` → 사본 수정 → `validate_grammar` → `parse_input`·`run_tests` 순서로
진행합니다. 원래의 내장 기준은 그대로 유지됩니다. 이 요청은 구문 변경을 검증하며,
`unless`의 실행 의미나 실행 코드 생성은 사용자 프로젝트에서 별도로 구현합니다.

웹 Playground에서 확인하려면 AI에게 다음 형식의 `.janglim.json` 파일을 저장하도록
요청한 뒤 Playground의 파일 열기로 가져옵니다.

```json
{
  "format": "janglim-workspace",
  "version": 1,
  "grammar": "Expr : Expr '+' Term | Term ;\nTerm : Term '*' Factor | Factor ;\nFactor : '(' Expr ')' | id ;\nid := \"[a-zA-Z]+\" ;",
  "input": "a + a * a"
}
```

MCP 결과가 웹 Studio에 자동 동기화되거나 공개 공유 URL로 저장되는 기능은 없습니다.
웹에서 파싱 결과를 다시 계산하므로 작업 파일에는 문법과 입력만 저장합니다.

## 처리 제한과 격리

첫 버전은 다음 기본값으로 작업량을 제한합니다. 이 값은 보호를 위한 시작점이며,
측정된 처리량이나 지원 사용자 수를 보장하는 수치는 아닙니다.

| 항목 | 기본 제한 |
| --- | --- |
| HTTP 요청 본문 | 128 KiB |
| 문법 텍스트 | 8,192 UTF-16 단위 |
| 입력 문자열 한 개 | 16,384 UTF-16 단위 |
| `run_tests` 예제 수 | 16개 |
| 테스트 입력 길이 합계 | 32,768 UTF-16 단위 |
| 파싱할 토큰 수 | 2,048개 |
| 생성 문법 규칙·LALR 상태 수 | 각 512개 |
| 반환 토큰 | 처음 128개 |
| 반환 트리 | 최대 512개 노드 |
| 반환 진단 | 최대 16개 |
| worker 작업 시간 | 10초 |
| 동시 worker | 인스턴스당 2개, 대기열 없음 |
| 동시 HTTP 요청 | 인스턴스당 4개 |
| HTTP 요청량 | 인스턴스당 분당 60개 |
| 응답 캐시 | 인스턴스당 64개, TTL 2분 |

상태·규칙 수 제한은 구성된 문법과 테이블을 검사합니다. 구성 작업 자체가 오래 걸리는
경우에는 worker의 시간 제한과 관리 heap 한도가 적용됩니다. 제한값을 높인다고 처리량이
자동으로 늘어나는 것은 아니므로 변경 전에 실제 문법으로 응답 시간과 메모리를 측정합니다.

기존 문법 생성기는 프로세스 공용 상태를 사용합니다. 서비스는 요청마다 독립적인
worker 프로세스를 실행하여 문법 생성과 파싱 상태를 분리합니다. 제한 시간이 지나거나
요청이 취소되면 worker와 자식 프로세스를 종료합니다. 종료 확인에 실패하면 해당 worker의
슬롯을 재사용하지 않고 오류를 기록합니다. 같은 worker에서 `run_tests`의
예제를 순차 실행하며 문법을 한 번 구성합니다.

캐시는 동일한 요청의 응답을 짧게 재사용합니다. 문법 객체를 여러 요청에서 공유하는
캐시나 영구 문법 저장소는 아닙니다. `status: "ok"`인 작은 응답만 캐시에 저장합니다.
캐시와 호출 제한은 각 서버 인스턴스에 적용되므로
인스턴스를 늘리면 서비스 전체의 허용 요청량도 늘어납니다.

첫 버전의 공유 키에는 사용자별 식별자가 없습니다. 호출 제한도 사용자별 과금·쿼터를
보장하는 장치가 아닙니다. 불특정 다수에게 공개하기 전에 사용자 인증, 별도 쿼터 정책,
부하 테스트와 관측 지표를 추가로 검토합니다. 컨테이너의 메모리 한도도 배포 환경에서
설정해야 합니다. worker는 .NET 관리 heap의 상한을 256 MiB로 설정하지만, 이는 네이티브
메모리와 호스트 프로세스를 포함한 전체 컨테이너 메모리의 상한은 아닙니다.

## 서버 설정

ASP.NET Core 설정은 환경 변수의 `__` 구분자로 전달할 수 있습니다.

| 환경 변수 | 기본값 | 용도 |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | Production | 실행 환경; 키 없는 loopback 개발은 Development에서만 허용 |
| `AllowedHosts` | `localhost;127.0.0.1;[::1]` | 허용 Host 이름; 여러 이름은 `;`로 구분 |
| `JanglimMcp__ApiKey` | 없음 | `Authorization: Bearer …` 인증 키; 32–256자, Development 이외에는 필수 |
| `JanglimMcp__AllowedOrigins__0` 등 | 빈 목록 | 허용할 Origin의 정확한 주소 |
| `JanglimMcp__MaxConcurrentWorkers` | `2` | 인스턴스별 동시 worker 수 |
| `JanglimMcp__WorkerTimeoutSeconds` | `10` | worker 작업 제한 시간 |
| `JanglimMcp__RequestsPerMinute` | `60` | 인스턴스별 요청량 제한 |
| `JanglimMcp__MaxConcurrentRequests` | `4` | 인스턴스별 동시 HTTP 요청 제한 |

Origin이 없는 CLI 요청은 허용하지만, Origin이 있는 요청은 허용 목록과 일치해야 합니다.
키를 브라우저 프런트엔드에 포함하는 방식으로 이 설정을 우회하지 않습니다. 현재
Playground는 브라우저에서 파싱하므로 원격 MCP 키를 필요로 하지 않습니다.

## 컨테이너와 Azure 배포

저장소 루트에서 이미지를 빌드합니다.

```shell
docker build -f services/Janglim.Mcp/Dockerfile -t janglim-mcp .
```

이미지는 .NET 10 ASP.NET runtime을 사용하며 non-root 사용자로 포트 `8080`에서
실행합니다. MCP 서비스와 필요한 엔진 프로젝트만 복사하므로 Playground 빌드,
Node.js, 라이선스 프런트엔드 소스는 필요하지 않습니다.

키를 앞의 PowerShell 예제로 환경에 설정한 뒤 로컬 컨테이너를 실행하는 예입니다.

```shell
docker run --rm -p 127.0.0.1:5080:8080 -e JanglimMcp__ApiKey -e AllowedHosts=localhost janglim-mcp
```

키 값은 `-e JanglimMcp__ApiKey`를 통해 실행하는 셸의 환경에서 전달됩니다.
이 실행 방식의 로컬 주소도 `http://localhost:5080/mcp`입니다.

운영 배포는 [`services/Janglim.Mcp/infra/Deploy-Azure.ps1`](../services/Janglim.Mcp/infra/Deploy-Azure.ps1)과
Bicep 템플릿으로 구성합니다. 현재 리소스는 다음과 같습니다.

| 항목 | 운영 설정 |
| --- | --- |
| Azure 지역 | Korea Central (`koreacentral`) |
| 리소스 그룹 | `janglim-mcp-rg` |
| Container App | `janglim-mcp` |
| API 키 secret | `mcp-api-key` |
| 인스턴스 자원 | CPU 1개, 메모리 2 GiB |
| replica 범위 | 최소 0개, 최대 2개 |
| HTTP 확장 기준 | 동시 요청 2개 |
| 이미지 | 전용 ACR의 SHA-256 digest로 고정 |

최소 replica가 0이므로 유휴 상태에서 인스턴스가 내려갈 수 있습니다. 이후 첫 연결에는
콜드 스타트 시간이 추가될 수 있어 Codex의 시작 제한 시간을 60초로 설정합니다.
서버 worker의 10초 작업 제한과 클라이언트의 연결 제한 시간은 서로 다른 설정입니다.
앱이 0개로 줄어도 전용 Basic ACR와 로그 저장 비용은 별도로 발생할 수 있으므로 전체
서비스가 무료가 되는 설정으로 해석하지 않습니다. 각각의 요금 범위는
[ACR 요금제](https://learn.microsoft.com/en-us/azure/container-registry/container-registry-skus)와
[Azure Monitor Logs 비용 안내](https://learn.microsoft.com/en-us/azure/azure-monitor/logs/cost-logs)를 참고합니다.

다른 환경에 배포하거나 설정을 변경할 때는 다음을 확인합니다.

1. 빌드한 이미지를 컨테이너 레지스트리에 게시하고 Container App에서 참조합니다.
2. ingress의 target port를 `8080`으로 지정하고 HTTPS를 사용합니다.
3. API 키를 Container Apps secret으로 저장하고 `JanglimMcp__ApiKey`에 secret reference로 연결합니다.
4. `AllowedHosts`를 실제 Container App FQDN과 사용할 사용자 도메인으로 명시합니다.
5. `/health`로 실제 HTTP 응답을 확인하고, 확인 요청의 Host도 허용 호스트와 맞춥니다. 포함된 Bicep의 자동 probes는 포트 `8080`의 TCP 연결을 검사합니다.
6. CPU·메모리 한도, 최대 replica 수, 비용 알림과 오류·지연 모니터링을 설정합니다.
7. 실제 문법을 대상으로 정상 호출, 잘못된 키, 취소·시간 초과, 호출 제한과 동시 요청을 검사합니다.

Azure의 HTTP 기반 확장 설정은
[Container Apps scaling 문서](https://learn.microsoft.com/en-us/azure/container-apps/scale-app)를
참고합니다. 최대 replica 수와 비용 알림은 운영을 통제하는 수단이며 절대적인 지출
차단 장치는 아닙니다. 응답 시간·CPU·메모리를 측정한 뒤 확장 기준을 조정합니다.

### 운영자용 키 조회

Azure 리소스를 조회할 권한이 있는 운영자만 실행합니다. Azure CLI에 로그인한 뒤
`YOUR_AZURE_SUBSCRIPTION_ID`를 운영 리소스가 있는 구독 ID로 바꿉니다. 키를 화면에
출력하지 않고 클라이언트 환경 변수에 바로 전달합니다.

```powershell
$janglimSubscriptionId = 'YOUR_AZURE_SUBSCRIPTION_ID'
$janglimApiKey = az containerapp secret list --name janglim-mcp --resource-group janglim-mcp-rg --subscription $janglimSubscriptionId --show-values --query "[?name=='mcp-api-key'].value | [0]" --output tsv --only-show-errors
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($janglimApiKey)) {
    throw 'Janglim MCP 키를 조회하지 못했습니다.'
}
$env:JANGLIM_MCP_API_KEY = $janglimApiKey.Trim()
$janglimApiKey = $null
```

`--show-values`를 생략하면 키 값을 조회하지 않습니다.
[Azure CLI secret 명령 문서](https://learn.microsoft.com/en-us/cli/azure/containerapp/secret?view=azure-cli-latest)를
참고합니다. 키 자체를 확인하는 `Write-Output`, 디버그 출력, 셸 transcript 기록은
추가하지 않습니다. 일반 이용자에게 Azure 구독 접근 권한을 주는 대신 API 키를 별도
보안 경로로 전달합니다. 첫 버전은 공유 키이므로 키 교체 시 연결한 모든 클라이언트도
새 값을 전달받아야 합니다.

### 재배포

운영 리소스와 ACR를 관리할 권한이 있는 Azure 계정, Azure CLI·Bicep·Docker,
PowerShell이 필요합니다. 저장소 루트에서 다음을 실행합니다.

```powershell
.\services\Janglim.Mcp\infra\Deploy-Azure.ps1 -SubscriptionId 'YOUR_AZURE_SUBSCRIPTION_ID'
```

스크립트는 전용 기반 리소스를 적용하고 이미지를 빌드·ACR에 게시한 뒤 digest로 고정해
앱을 배포합니다. 기존 앱이 있으면 `mcp-api-key`를 보존하며, 최초 배포에서만 새 키를
생성합니다. 키는 메모리에서 ARM의 secure parameter로 전달합니다. 로컬 secret 파일,
명령행 키 인수나 키가 포함된 배포 출력은 만들지 않습니다.

이미 검증한 로컬 이미지 `janglim-mcp:local`을 사용해 빌드를 생략하려면
`-UseExistingImage`를 추가합니다. 이 경우에도 이미지는 ACR에 게시하며 새 배포에는
확인한 digest를 사용합니다. 키가 없는 기존 앱은 자동으로 다른 키로 덮어쓰지 않고
배포를 중단하므로 운영자가 기존 키 상태를 먼저 확인합니다.

배포 결과는 ignored 경로 `services/Janglim.Mcp/obj/azure-deploy/deployment.json`에도
기록합니다. 이 파일에는 주소·리소스 이름·이미지 digest 등 메타데이터만 포함하며 키는
없습니다. 기존 master-push NuGet·Playground 배포 과정은 MCP를 빌드·검증하지만
운영 MCP 서버를 자동 재배포하지는 않습니다. 현재 MCP 재배포는 위 스크립트를 사용합니다.

### 원격 검증

앞의 절차로 `JANGLIM_MCP_API_KEY`를 설정한 뒤 Python 3으로 실행합니다. 추가 Python
패키지는 필요하지 않습니다.

```powershell
python services/Janglim.Mcp/scripts/smoke_remote.py https://janglim-mcp.wonderfulsand-6b5e00a1.koreacentral.azurecontainerapps.io/mcp
```

검사 스크립트는 상태 확인, 인증 없는 호출 거부, 허용하지 않은 Origin 거부, MCP 초기화,
도구 조회, 문법 검증, 파싱·트리 위치, 정상·오류 예제, 충돌 문법 거부를 확인합니다.
C11 목록·원본 해시와 로컬 파일 일치·잘못된 ID 거부·수정 문법·수정 예제·원본 불변성까지
총 15개 검사를 수행합니다. HTTPS만 허용하고 리다이렉트에는 키를 전달하지 않으며,
키나 응답 본문을 출력하지 않습니다. 성공하면 `PASS: 15 checks …`, 실패하면 실패 단계와 제한된 진단만
표시합니다. 배포 성공 상태와 함께 이 원격 검사를 확인한 뒤 새 배포를 사용합니다.
