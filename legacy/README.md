# legacy/ — 이전 솔루션과 구형 프로젝트 보관

이 폴더의 구형 IDE·WPF 프로젝트는 기본 빌드에서 제외하고 참고용으로 보관합니다.
저자가 직접 설계하고 작성해 온 AJ 언어·컴파일러·IDE의 남은 소스와 프로젝트 구성을 보관합니다.
참조하는 프로젝트가 없는 상위 실행 프로젝트 `AJStudio`와 `CommandPrompt.Compiler`만 삭제했으며,
저자의 설계와 원래 소스는 Git 기록에 남아 있습니다. 나머지 프로젝트의 소스 경로는 유지합니다.

## 솔루션 선택

- 저장소 루트의 [`AJPGS.sln`](../AJPGS.sln)은 엔진·패키징, 파서 테스트, Playground를 포함하는 지원 대상입니다.
- [`AJPGS.Legacy.sln`](AJPGS.Legacy.sln)은 위 두 실행 프로젝트를 제외한 28개 프로젝트의 구성을 보관합니다.
  엔진과 테스트뿐 아니라 `src/`의 AJ 언어·중간 표현·백엔드·컴파일러,
  `CommandPrompt.Builder`·`CommandPrompt.Util`·`CommandPrompt.Lint`, 이 폴더의 남은 WPF IDE 구성요소를 포함합니다.
  각 프로젝트 파일은 원래 경로에 있으며, 이 솔루션은 기본 빌드와 CI 대상에 포함하지 않습니다.

이전 구성을 조사하려면 보관 솔루션을 직접 열거나 저장소 루트에서 명시적으로 빌드합니다.

```shell
dotnet build legacy/AJPGS.Legacy.sln -c Release
```

이 명령은 현재 성공하는 빌드 절차가 아닙니다. 솔루션 분리는 아래 오류를 수정하지 않습니다.

| 폴더 | 내용 |
|---|---|
| `Applications/` | 남은 WPF IDE 뷰 및 MVVM 레이어 (netcoreapp3.1, Actipro 의존) |
| `Wpf.UI.Themes/` | WPF 테마 (Wpf.UI.Basic / Wpf.UI.Advance) |
| `Parse.WpfControls/`, `Parse.WpfControls.SyntaxEditor/` | IDE용 커스텀 구문 편집기 컨트롤 |
| `ParsingLibrary/` | 초기 파서 제너레이터 원형 (net472, 고아 — 솔루션 미포함) |

## 알려진 빌드 제약

- WPF 프로젝트에는 Windows와 구형 .NET Core 3.1 환경, 별도 Actipro DLL이 필요합니다.
  일부 `HintPath`는 특정 머신의 설치 경로에 의존합니다.
- `ApplicationLayer.Common`의 이전 `Parse.FrontEnd.AJ` 네임스페이스 참조,
  `Parse.WpfControls`의 XAML 타입 참조 오류가 남아 있습니다.
- 일부 구형 `ProjectReference`는 존재하지 않는 경로를 가리킵니다.

현재 엔진과 Playground 개발은 [기본 솔루션 빌드 안내](../README.md#building-from-source)를 따르세요.
