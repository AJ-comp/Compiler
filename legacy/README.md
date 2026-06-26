# legacy/ — 동결(deprecated) 프로젝트 모음

이 폴더의 프로젝트들은 **더 이상 유지보수하지 않으며 빌드 대상이 아닙니다.**
삭제하지 않고 참고용으로 보관합니다.

| 폴더 | 내용 |
|---|---|
| `Applications/` | AJStudio WPF IDE 및 MVVM 레이어 (netcoreapp3.1, ActiPro 의존) |
| `Wpf.UI.Themes/` | WPF 테마 (Wpf.UI.Basic / Wpf.UI.Advance) |
| `Janglim.WpfControls/`, `Janglim.WpfControls.SyntaxEditor/` | IDE용 커스텀 구문 편집기 컨트롤 |
| `ParsingLibrary/` | 초기 파서 제너레이터 원형 (net472, 고아 — 솔루션 미포함) |

## 주의사항
- **이 머신에서는 빌드되지 않습니다**: net core 3.1(EOL) + ActiPro 절대경로 HintPath 의존.
- **라이브(파서/컴파일러) 코드는 이 폴더를 참조하지 않습니다.** 의존성은 위→아래 단방향이라,
  이 프로젝트들을 옮겨도 라이브 빌드·테스트에는 영향이 없습니다.
- 여기 프로젝트들이 라이브 프로젝트를 가리키던 `ProjectReference` 경로는 `legacy/`로 이동하면서
  한 단계(`..\`)씩 보정되었습니다. (되살릴 경우 참고)
- 빌드/테스트는 라이브 프로젝트 + 테스트만 대상으로 하세요.
  전체 솔루션 빌드(`dotnet build AJPGS.sln`)는 이 동결 프로젝트들 때문에 실패합니다.
