# Janglim 게시와 Playground 배포

`master`에 push하면 GitHub Actions가 같은 커밋을 검증하고, 새 Janglim 버전을 NuGet에 게시한 뒤 [운영 Playground](https://janglim.mythosia.net/)를 배포합니다. 진입점은 [`publish-nuget.yml`](workflows/publish-nuget.yml)입니다. 파일 변경 경로와 관계없이 모든 `master` push가 이 절차를 실행합니다.

## 평소 출시 방법

1. 엔진을 새로 게시하려면 [`src/Directory.Build.props`](../src/Directory.Build.props)의 `Version`을 아직 게시하지 않은 버전으로 올립니다. 같은 버전의 NuGet 패키지는 덮어쓸 수 없습니다.
2. Playground 버전은 [`samples/Playground/Frontend/package.json`](../samples/Playground/Frontend/package.json)의 `version`과 [`Janglim.Playground.csproj`](../samples/Playground/Janglim.Playground.csproj)의 `Version`을 함께 갱신합니다. 엔진과 웹의 버전은 독립적입니다. 엔진 `0.5.0` 릴리즈에서 웹은 `1.1.0`을 유지합니다. MCP 서비스도 독립 버전 `0.1.0`을 사용합니다.
3. README, 변경 이력, 릴리즈 노트를 갱신하고 PR 검증을 통과시킵니다.
4. 검토한 변경을 `master`에 반영해 push합니다. [Actions](https://github.com/AJ-comp/Compiler/actions)에서 해당 커밋의 실행 결과를 확인합니다.

엔진 버전을 바꾸지 않은 UI·문서 수정도 검증과 웹 배포를 수행합니다. NuGet에서 같은 버전을 확인하면 패키지 게시만 건너뜁니다. NuGet 버전 조회가 실패하면 새 버전으로 간주하지 않고 해당 실행을 중단합니다.

## 실행 순서와 결과 확인

[`verify-release.yml`](workflows/verify-release.yml)이 기본 솔루션 [`AJPGS.sln`](../AJPGS.sln)의 Debug·Release 빌드, 게시 스크립트 테스트, 엔진·MCP Release 테스트, UI 테스트, NuGet 패키징, 공개 웹 게시 빌드, MCP 서버 게시 빌드와 컨테이너 빌드를 수행합니다. 기본 솔루션에는 엔진·패키징 프로젝트, 파서 테스트, Playground, MCP 서버와 MCP 테스트가 포함됩니다. 테스트·게시·패키징은 각 프로젝트를 대상으로 수행하며, 구형 IDE·AJ 컴파일러·CLI를 보관한 [`legacy/AJPGS.Legacy.sln`](../legacy/AJPGS.Legacy.sln)은 CI 빌드 대상에 포함하지 않습니다. PR과 `master` 외 브랜치의 push는 이 검증만 실행하며, NuGet 게시나 Azure 배포에 필요한 권한·비밀을 사용하지 않습니다. MCP 게시 산출물은 Artifacts에 보관합니다. 원격 MCP는 별도 Azure Container Apps 서비스를 사용하며, 현재 `master`의 자동 배포 대상에는 포함하지 않습니다. MCP 배포는 [`Deploy-Azure.ps1`](../services/Janglim.Mcp/infra/Deploy-Azure.ps1)로 수행합니다. 실제 원격 주소·인증·재배포·원격 검증 방법은 [MCP 가이드](../docs/MCP.md)를 참고하세요.

로컬 기본 빌드에 필요한 .NET 10 SDK·테스트 실행용 .NET 8 런타임·Node.js 버전과 공개 빌드 환경 변수 `PLAYGROUND_PUBLIC_BUILD=1`은 [개발 빌드 안내](../README.md#building-from-source)에 정리했습니다. CI에는 .NET 8·10 SDK를 모두 설치합니다. 구형 솔루션의 알려진 오류와 외부 의존성은 [보관 안내](../legacy/README.md)를 참고하세요.

React `Frontend/package.json`이 없는 기존 Playground 체크아웃에서는 UI 테스트와 웹 배포를 건너뛰어 운영 화면을 유지합니다. 엔진 테스트·패키징·웹 게시 빌드는 계속 필수입니다. React 소스가 포함된 체크아웃에서는 UI 의존성 설치·테스트·버전 일치 검사와 웹 배포도 수행합니다. 공개 빌드는 라이선스 컴포넌트 없이 기본 로딩 표시를 사용합니다.

검증에 성공하면 동일한 실행에서 만든 NuGet 패키지와 웹 산출물을 사용합니다. [`deploy-playground.yml`](workflows/deploy-playground.yml)은 검증된 웹 산출물을 내려받아 Azure에 업로드하며 별도로 다시 빌드하지 않습니다. Actions 실행 화면의 작업 요약에서 게시·건너뜀·dry run 상태를, **Artifacts**에서 테스트 결과·NuGet 패키지·웹 게시 파일을 확인할 수 있습니다.

패키지 산출물 이름은 `janglim-package-<run_id>-<run_attempt>`, 웹 산출물은 `playground-site-<run_id>-<run_attempt>`입니다. 패키지에는 무결성을 확인할 `SHA256SUMS.txt`도 포함합니다. 재실행 횟수까지 이름에 포함하므로 이전 실행의 산출물과 구분할 수 있습니다.

`master`에서는 한 번에 하나의 출시만 실행하며, 다음 push가 와도 실행 중인 출시를 취소하지 않습니다. GitHub의 기본 동시성 정책에 따라 대기 중인 실행이 있으면 새 push가 그 실행을 교체할 수 있습니다. 출시 태그와 GitHub Release 생성은 이 워크플로에 포함하지 않습니다.

## 수동 검증과 재시도

[Actions](https://github.com/AJ-comp/Compiler/actions)에서 **Release Janglim → Run workflow**를 열고 `master`를 선택합니다.

- `dry_run: true`가 기본값입니다. 테스트와 산출물 생성까지 확인하며 NuGet 게시와 운영 웹 배포를 모두 건너뜁니다.
- `dry_run: false`이면 검증 후 NuGet 게시와 운영 웹 배포를 수행합니다.
- `master` 외 브랜치에서는 게시·배포를 수행하지 않습니다.

CLI로 수동 실행할 때도 브랜치와 모드를 명시합니다.

```shell
# 검증과 산출물 생성
gh workflow run publish-nuget.yml --ref master -f dry_run=true

# 검증 후 실제 게시와 배포
gh workflow run publish-nuget.yml --ref master -f dry_run=false
```

NuGet 게시 후 Azure 배포가 실패하면 해당 커밋이 여전히 최신 `master`인 경우 **Re-run all jobs**로 재시도할 수 있습니다. 이미 게시한 버전은 건너뛰며 웹 배포를 다시 진행합니다. `master`가 더 진행되었다면 최신 커밋으로 수동 실행하세요. 배포 직전에 현재 `master` 커밋을 확인하므로 예전 실행을 재시도해 최신 웹을 덮어쓰는 것을 막습니다. 원격 NuGet 버전 목록에 반영이 지연되어도 `--skip-duplicate`가 중복 게시를 추가로 방지합니다. 이미 게시된 패키지 자체를 수정해야 한다면 새 엔진 버전으로 출시합니다.

## 인증과 최초 적용

NuGet 게시에는 기존 Trusted Publishing 정책을 사용합니다. 정책은 패키지 소유자 `JJW`, 저장소 `AJ-comp/Compiler`, `publish-nuget.yml` 파일에 연결되어 있으며 `NuGet/login@v1`이 GitHub OIDC로 짧게 유효한 API 키를 얻습니다. 저장소에 장기 NuGet API 키를 새로 추가할 필요가 없습니다. Azure 배포는 기존 저장소 비밀 `AZURE_STATIC_WEB_APPS_API_TOKEN`을 사용합니다. 워크플로 파일명이나 저장소를 바꾸면 NuGet 정책도 함께 확인해야 합니다.

인프라 파일을 `master`에 처음 반영하는 push도 실제 출시 절차를 실행합니다. React 소스가 없는 체크아웃이면 운영 웹 배포를 건너뛰고, 이미 게시된 엔진 버전도 건너뜁니다. React 소스와 새 엔진 버전을 포함하면 전체 게시·배포가 실행됩니다. 먼저 PR 검증을 확인하고 게시를 의도한 소스·버전만 `master`에 반영하세요. 원격 성공 여부는 해당 Actions 실행 결과로 확인합니다.
