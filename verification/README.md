# 개발·검증 기록

이 폴더는 Janglim을 개발하면서 저장한 **문제 재현 자료, 빌드·테스트 결과, 출시·게시 확인 기록**을 보관합니다. 2026년 10월 28일에 로컬 `artifacts/`에서 보존할 자료를 선별해 옮겼습니다.

`Tests/`에는 지금 실행하는 단위테스트 코드가 있습니다. 이 폴더에는 과거 실행의 결과와 그 결과를 조사할 때 사용한 코드가 있습니다. 실패 당시의 기록과 수정 후 기록을 함께 보관했으므로, 파일의 실행 시점·대상 버전·결과를 함께 확인하세요.

## 필요한 기록 찾기

| 폴더 | 보관 내용 |
| --- | --- |
| [releases/](releases/) | 0.3.0~0.6.0 출시 검증, NuGet 패키지 소비 예제, 실제 게시·배포 확인 기록 |
| [javascript/](javascript/) | JavaScript 기능별 검증, ECMAScript·Test262 비교 결과와 재현 입력 |
| [typescript/](typescript/) | TypeScript 문법·단언·클래스 검증, 기준 구현과 비교한 결과 및 재현 코드 |
| [parser/](parser/) | 렉싱, FIRST, 룩어헤드 전파, 문법 검증 문제의 조사와 회귀 검증 |
| [maintenance/](maintenance/) | 솔루션 구성 정리와 프로젝트 제거 전후의 빌드 확인 |
| [documentation/](documentation/) | 문서 수정과 Playground 작업 파일에 관한 검증·조사 기록 |

최근 결과는 다음 파일에서 확인할 수 있습니다.

- [0.6.0 출시 검증](releases/0.6.0/verification.json)
- [0.6.0 공개 NuGet 검증](releases/0.6.0/public-nuget/verification.json)
- [0.6.0 Playground 배포 검증](releases/0.6.0/playground-deployment-verification.json)
- [TypeScript 클래스 확장 검증](typescript/class-extensions-20261028-155659-971/verification.json)
- [ECMAScript 2026 대조 결과](javascript/ecmascript-2026/final-latest/summary.json)

각 파일은 해당 검증을 실행했던 시점의 기록입니다. 현재 소스의 검증 결과는 현재 소스로 테스트를 실행해 확인합니다.

## 원문과 이동 경로

[manifest.json](manifest.json)에 보관한 파일마다 다음 정보를 기록했습니다.

- `originalPath`: 이동 전의 저장소 기준 경로
- `path`: 이 폴더를 기준으로 한 현재 경로
- `compression`: 압축 여부 (`none` 또는 `gzip`)
- `sha256`: 압축 전 원문 바이트의 SHA-256
- `storedSha256`: 현재 보관 파일 바이트의 SHA-256
- `originalBytes`, `storedBytes`: 압축 전후의 크기
- `modifiedUtc`: 이동 전 파일의 수정 시각

기록의 내용과 기존 저자·설계자 표기는 그대로 보존했습니다. 로그와 재현 코드에 들어 있는 `artifacts/...` 또는 `C:\...` 경로도 실행 당시의 값입니다. 예전 경로로 적힌 자료를 찾을 때는 `manifest.json`의 `originalPath`를 검색하면 됩니다.

재현 프로젝트·스크립트는 당시의 소스 버전, 패키지, 도구 또는 로컬 경로를 참조할 수 있습니다. 다시 실행하려면 해당 기록의 버전을 확인하고 필요한 참조 경로와 도구를 준비하세요. 원래 폴더 구조가 필요한 경우에는 `originalPath`에 해당하는 위치로 파일을 **복사**해서 사용하면 됩니다.

## 큰 결과 파일 읽기

1 MiB 이상인 기록은 전체 내용을 `.gz` 파일로 압축했습니다. Python이 있으면 아래처럼 특정 기록을 읽을 수 있습니다.

```python
import gzip
from pathlib import Path

path = Path("verification/releases/0.5.0/TestResults/engine.trx.gz")
content = gzip.decompress(path.read_bytes())
print(content.decode("utf-8"))
```

압축 해제한 바이트의 SHA-256을 `manifest.json`의 `sha256`과 비교하면 원문 보존 여부를 확인할 수 있습니다. 이 폴더의 `.gitattributes`는 다른 운영체제에서 가져올 때도 Git이 기록의 줄바꿈을 바꾸지 않도록 설정합니다.

## artifacts에 남긴 자료

다시 만들거나 내려받을 수 있는 DLL·패키지·배포 산출물, `bin`·`obj`·의존성 캐시, 외부 도구와 전체 테스트 말뭉치는 이동 대상에서 제외했습니다. 큰 Test262 전체 입력 묶음은 제외하고, 해당 입력의 출처·선정 목록과 실제 검증 결과는 보관했습니다. 이러한 자료는 기존 `artifacts/`에 남겨 두었습니다. 현재 문서에서 사용하지 않는 옛 룩어헤드 설명 그림 29개는 기록 정리 후 삭제했습니다.

이 폴더는 당시 저장해 둔 기록을 모은 것입니다. 모든 실행이나 대화가 자동으로 기록된 저장소는 아닙니다.

## 외부 테스트 자료의 출처

일부 재현 입력에는 [TC39 Test262](https://github.com/tc39/test262)의 테스트 원문이 포함돼 있습니다. 기존 파일의 저작권 고지와 함께 [Test262 라이선스](licenses/Test262-LICENSE.txt)를 보관합니다. 입력 묶음의 manifest·selection 파일에는 사용한 커밋과 원본 경로가 기록돼 있습니다.
