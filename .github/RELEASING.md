# Janglim 게시와 Playground 배포

`master`에 push하면 GitHub Actions가 같은 커밋을 검증하고, 새 Janglim 버전을 NuGet에 게시한 뒤 [운영 Playground](https://janglim.mythosia.net/)를 배포합니다. 진입점은 [`publish-nuget.yml`](workflows/publish-nuget.yml)입니다. 파일 변경 경로와 관계없이 모든 `master` push가 이 절차를 실행합니다.

## 평소 출시 방법

1. 엔진을 새로 게시하려면 [`src/Directory.Build.props`](../src/Directory.Build.props)의 `Version`을 아직 게시하지 않은 버전으로 올립니다. 같은 버전의 NuGet 패키지는 덮어쓸 수 없습니다.
2. Playground 버전은 [`samples/Playground/Frontend/package.json`](../samples/Playground/Frontend/package.json)의 `version`과 [`Janglim.Playground.csproj`](../samples/Playground/Janglim.Playground.csproj)의 `Version`을 함께 갱신합니다. 엔진과 웹의 버전은 독립적입니다. 현재 출시 준비 버전은 엔진 `0.3.0`, 웹 `1.0.0`입니다.
3. README, 변경 이력, 릴리즈 노트를 갱신하고 PR 검증을 통과시킵니다.
4. 검토한 변경을 `master`에 반영해 push합니다. [Actions](https://github.com/AJ-comp/Compiler/actions)에서 해당 커밋의 실행 결과를 확인합니다.

엔진 버전을 바꾸지 않은 UI·문서 수정도 검증과 웹 배포를 수행합니다. NuGet에서 같은 버전을 확인하면 패키지 게시만 건너뜁니다. NuGet 버전 조회가 실패하면 새 버전으로 간주하지 않고 해당 실행을 중단합니다.

## 실행 순서와 결과 확인

[`verify-release.yml`](workflows/verify-release.yml)이 게시 스크립트 테스트, 엔진 Release 테스트, UI 테스트, NuGet 패키징, 공개 웹 게시 빌드를 수행합니다. PR과 `master` 외 브랜치의 push는 이 검증만 실행하며, NuGet 게시나 Azure 배포에 필요한 권한·비밀을 사용하지 않습니다.

현재 원격 소스처럼 React `Frontend/package.json`이 없는 기존 Playground에서는 UI 테스트만 건너뜁니다. 엔진 테스트·패키징·웹 게시 빌드는 계속 필수이며, React 소스가 들어오면 UI 의존성 설치와 테스트도 필수입니다. 공개 빌드는 라이선스 컴포넌트 없이 기본 로딩 표시를 사용합니다.

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

NuGet 게시 후 Azure 배포가 실패하면 원인을 수정한 뒤 동일한 실행을 **Re-run all jobs**로 재시도할 수 있습니다. 이미 게시한 버전은 건너뛰며 웹 배포를 다시 진행합니다. 원격 NuGet 버전 목록에 반영이 지연되어도 `--skip-duplicate`가 중복 게시를 추가로 방지합니다. 이미 게시된 패키지 자체를 수정해야 한다면 새 엔진 버전으로 출시합니다.

## 인증과 최초 적용

NuGet 게시에는 기존 Trusted Publishing 정책을 사용합니다. 정책은 패키지 소유자 `JJW`, 저장소 `AJ-comp/Compiler`, `publish-nuget.yml` 파일에 연결되어 있으며 `NuGet/login@v1`이 GitHub OIDC로 짧게 유효한 API 키를 얻습니다. 저장소에 장기 NuGet API 키를 새로 추가할 필요가 없습니다. Azure 배포는 기존 저장소 비밀 `AZURE_STATIC_WEB_APPS_API_TOKEN`을 사용합니다. 워크플로 파일명이나 저장소를 바꾸면 NuGet 정책도 함께 확인해야 합니다.

인프라 파일을 `master`에 처음 반영하는 push도 실제 출시 절차를 실행합니다. 인프라부터 검증하려면 먼저 PR 검증을 확인하고, 게시를 의도한 소스·버전만 `master`에 반영하세요. 새 워크플로의 원격 성공 여부는 해당 Actions 실행 결과로 확인해야 합니다.
