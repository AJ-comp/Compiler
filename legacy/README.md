# legacy/ — 이전 솔루션과 구형 프로젝트 보관

이 폴더의 구형 IDE·WPF 프로젝트는 기본 빌드에서 제외하고 참고용으로 보관합니다.
저자가 직접 설계하고 작성해 온 AJ 언어·컴파일러·IDE의 남은 소스와 프로젝트 구성을 보관합니다.
참조하는 프로젝트가 없는 상위 실행 프로젝트 `AJStudio`와 `CommandPrompt.Compiler`,
이후 사용되지 않는 `ApplicationLayer.Views`·`ApplicationLayer.ViewModels`·`ApplicationLayer.Models`·
`ApplicationLayer.Common`·`Parse.WpfControls.SyntaxEditor`·`Parse.WpfControls` 라이브러리를 삭제했으며,
저자의 설계와 원래 소스는 Git 기록에 남아 있습니다. 나머지 프로젝트의 소스 경로는 유지합니다.

## 솔루션 선택

- 저장소 루트의 [`AJPGS.sln`](../AJPGS.sln)은 엔진·패키징, 파서 테스트, Playground를 포함하는 지원 대상입니다.
- [`AJPGS.Legacy.sln`](AJPGS.Legacy.sln)은 위 여덟 프로젝트를 제외한 22개 프로젝트의 구성을 보관합니다.
  엔진과 테스트뿐 아니라 `src/`의 AJ 언어·중간 표현·백엔드·컴파일러,
  `CommandPrompt.Builder`·`CommandPrompt.Util`·`CommandPrompt.Lint`, 이 폴더의 남은 IDE Define 레이어와 WPF 테마를 포함합니다.
  각 프로젝트 파일은 원래 경로에 있으며, 이 솔루션은 기본 빌드와 CI 대상에 포함하지 않습니다.

이전 구성을 조사하려면 보관 솔루션을 직접 열거나 저장소 루트에서 명시적으로 빌드합니다.

```shell
dotnet build legacy/AJPGS.Legacy.sln -c Release
```

이 솔루션은 기본 빌드·CI 검증 범위 밖에 있습니다. 빌드 시 아래 구형 환경 요구 사항을 확인하세요.

| 폴더 | 내용 |
|---|---|
| `Applications/` | 남은 IDE Define 레이어 (netcoreapp3.1) |
| `Wpf.UI.Themes/` | WPF 테마 (Wpf.UI.Basic / Wpf.UI.Advance) |
| `ParsingLibrary/` | 초기 파서 제너레이터 원형 (net472, 고아 — 솔루션 미포함) |

## 알려진 빌드 제약

- WPF 빌드에는 Windows가 필요하며, 남은 WPF 프로젝트는 `netcoreapp3.1`을 대상으로 합니다.
  Actipro DLL을 참조하던 IDE 실행·뷰 프로젝트는 삭제되어 남은 프로젝트에는 해당 참조가 없습니다.
- `ApplicationLayer.Common`의 이전 `Parse.FrontEnd.AJ` 네임스페이스 참조와
  존재하지 않는 `Compile` 경로를 가리키던 `ProjectReference` 오류,
  `Parse.WpfControls`의 XAML 타입 참조 오류는 해당 프로젝트 삭제 이력에 속합니다.

현재 엔진과 Playground 개발은 [기본 솔루션 빌드 안내](../README.md#building-from-source)를 따르세요.
