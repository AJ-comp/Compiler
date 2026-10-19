# 렉서 심화 — 원문 경계 정보의 API와 구현 이유

> 🎓 **심화 과정**입니다.
> 먼저 [렉서 기본 편](lexer.md)을 읽으면, 아래 API가 왜 필요한지 이해하기 쉬워요.

저자가 설계한 기존 렉서의 **표시용 토큰 목록과 파싱용 토큰 목록을 구분하는 구조**를 바탕으로,
파싱 토큰에서 경계 원문까지 조회하도록 보강했습니다.
이 장에서는 저자의 설계를 바탕으로 한 기존 구조와 이번 보강의 동작을 살펴보고,
각 구현이 필요한 이유를 코드와 예제로 설명합니다.

## ① 두 토큰 목록을 유지하는 이유

`Lexer.Lexing(source)`는 `LexingData`를 돌려줍니다.
여기에 두 목록이 있습니다.

| 목록 | 내용 | 필요한 이유 |
|---|---|---|
| `TokensForView` | 공백·주석을 포함한 원문의 토큰들 | 편집기의 표시와 원문 보존에는 문법에서 제외하는 부분도 필요합니다 |
| `TokensForParsing` | 기존 분류 기준으로 파싱에 남는 토큰들 | 문법에 직접 참여하는 토큰열을 파서에 전달합니다 |

예를 들어 `return /* note */ value;`에서 주석을 제외하도록 규칙을 등록했다면,
파싱용 목록에는 `return`, `value`, `;`이 남습니다.
이 토큰열을 유지하면서 `value`에 `" /* note */ "`라는 경계 원문을 연결합니다.

**왜 줄바꿈을 경계 정보로 전달하나요?**
줄바꿈을 경계 정보로 보존하면 기존 파싱 토큰열을 유지하면서 다음 단계에 줄바꿈 여부를 제공할 수 있습니다.
공백을 제외하는 기존 문법을 그대로 사용하면서, 토큰 사이의 원문까지 함께 조회할 수 있어요.
줄바꿈을 문법의 구성 요소로 직접 파싱하는 방법도 가능하며, 그 경우에는 줄바꿈이 파싱 토큰으로 남습니다.

경계 정보는 **실제로 파싱에서 제외된 토큰**만 합칩니다.
일반 `Delimiter`와 통째로 인식한 `Comment`·`LineComment`는 제외됩니다.
기존 `ScopeComment` 방식의 주석 구분자는 파싱 목록에 남고,
알 수 없는 입력을 나타내는 `NotDefined`도 오류 판단을 위해 남습니다.

**왜 알 수 없는 입력은 파싱 토큰으로 유지하나요?**
잘못된 글자를 공백처럼 넘겨 버리면 파서가 오류 위치를 알 수 없기 때문입니다.
또한 줄바꿈 문자를 구분자로 등록하지 않아 `NotDefined`로 남긴 경우,
그 문자는 파싱 입력에 남습니다.

## ② 공개 API — 무엇을 읽고, 왜 필요한가요?

### `TokenCell`: 렉서가 만든 실제 토큰

```csharp
public TokenTrivia LeadingTrivia { get; internal set; }
public bool HasLineBreakBefore => LeadingTrivia.HasLineBreak;
```

위는 `TokenCell`의 속성 선언입니다. 사용자는 `token.LeadingTrivia.Text`처럼 읽습니다.
경계 정보의 교체는 엔진 내부에서 수행합니다.
`Data`가 토큰 본문이고, `LeadingTrivia`는 이전 파싱 토큰 이후 자기 앞까지의 제외된 원문입니다.
첫 토큰의 경우에는 입력 시작 이후 자기 앞까지예요.

### `TokenTrivia`: 경계 원문의 불변 스냅샷

| 속성 | 의미 | 필요한 이유 |
|---|---|---|
| `Text` | 제외된 원문 그대로 | 주석, 들여쓰기, 줄바꿈 형식을 확인하거나 원문을 복원합니다 |
| `StartIndex` | 원문 범위의 시작 위치, 포함 | 정확한 강조·진단·편집 위치를 찾습니다 |
| `Length` | `Text.Length` | 해당 범위의 길이를 바로 사용합니다 |
| `EndIndexExclusive` | 범위의 끝 위치, 미포함 | `Substring(start, end - start)`와 같은 계산을 단순하게 합니다 |
| `HasLineBreak` | 이 원문에 정해진 줄바꿈 문자가 있는지 | 같은 문자열을 조건마다 다시 검사할 필요가 없습니다 |

`TokenCell.HasLineBreakBefore`는 `LeadingTrivia.HasLineBreak`를 편하게 조회하는 속성입니다.
원문은 공백과 줄바꿈 형식까지 그대로 보관합니다.

**왜 불변 스냅샷인가요?**
조회 당시의 정보를 보관해 편집 전후의 내용을 정확히 비교할 수 있기 때문입니다.
**토큰 객체의 `LeadingTrivia` 참조는 편집 후 새 스냅샷으로 교체됩니다.**

```csharp
var before = token.LeadingTrivia;
// 같은 토큰이 유지되는 렉싱 데이터의 편집·갱신 후:
var after = token.LeadingTrivia;
// before는 조회 당시 정보, after는 갱신된 토큰의 현재 정보입니다.
```

오래 보관한 `TokenTrivia`와 지금 토큰에서 다시 읽은 정보를 구분해야 합니다.
`TokenTrivia`는 조회 당시의 값을 보관하고, `TokenCell`과 이를 감싼 `TokenData`는 현재 정보를 가리킵니다.

### `TokenData`: 파서에서 조회하는 토큰

파서는 렉서의 `TokenCell`을 감싼 `TokenData`를 사용합니다.
`TokenData`에서도 같은 이름의 `LeadingTrivia`, `HasLineBreakBefore`를 조회할 수 있습니다.
자신이 참조하는 `TokenCell`의 현재 정보를 읽습니다.

**왜 파서 쪽에도 조회 API가 필요한가요?**
렉서에서만 원문을 찾을 수 있다면 파싱 블록, 파스 트리, AST, 다음 입력 토큰을 보여주는 추적 기록에서
다시 원문을 역추적해야 하기 때문입니다. `TokenData`가 참조하는 토큰을 통해 같은 정보에 접근합니다.
AST에는 문법의 의미 태그 설정에 따라 남는 토큰이 달라질 수 있습니다.

직접 생성한 토큰과 가상 토큰은 기본적으로 빈 경계 정보를 가집니다.
`TokenCell`이 없는 `TokenData`도 빈 정보를 반환합니다.
실제 입력의 조건을 판단할 때는 렉싱 경로에서 만들어진 토큰을 사용합니다.
이 경로에서 입력 원문을 검사하고 경계 정보를 채우기 때문입니다.

## ③ 위치 범위를 정확하게 정의하는 이유

입력이 C# 문자열로 `"return\n    value\n;"`라면 결과는 다음과 같습니다.

| 토큰 본문 | 토큰 시작 | 앞 원문 | 경계 범위 | 줄바꿈 |
|---|---|---|---|---|
| `return` | 0 | `""` | `[0, 0)` | `false` |
| `value` | 11 | `"\n    "` | `[6, 11)` | `true` |
| `;` | 17 | `"\n"` | `[16, 17)` | `true` |

`[6, 11)`은 **6번 위치부터 10번 위치까지의 범위**입니다.
6번 위치의 줄바꿈과 7·8·9·10번 위치의 공백 네 개가 들어갑니다.
11번 위치부터는 토큰 본문 `value`가 시작해요.

**왜 시작을 포함하고 끝 직전까지의 범위로 표현하나요?**
범위의 길이가 `끝 - 시작`으로 바로 계산되기 때문입니다.
빈 범위도 `[11, 11)`로 자연스럽게 표현할 수 있습니다.
기존 `TokenCell.EndIndex`는 끝을 **포함**하는 값이므로, 경계 원문의 `EndIndexExclusive`와 구분하세요.

위치는 **0부터 시작하는 UTF-16 코드 단위**입니다. .NET의 `string.Length`와 문자열 인덱싱에 맞춥니다.
예를 들어 `😀`는 화면에는 한 글자로 보여도 UTF-16에서는 두 단위를 차지합니다.

**왜 UTF-16 코드 단위로 위치를 표현하나요?**
보이는 글자 수와 .NET 문자열 인덱스가 다르면, 저장한 위치로 `Substring`하거나 원문 범위를 찾을 때
다른 부분을 가리킬 수 있기 때문입니다. 표시할 때의 글자·열 계산은 별도로 다뤄야 합니다.

## ④ 줄바꿈 판정이 정확히 의미하는 것

현재 `TokenTrivia.HasLineBreak`는 `Text`에서 다음 문자 중 하나라도 발견하면 `true`입니다.

| 문자 | 표기 |
|---|---|
| LF | `\n`, U+000A |
| CR | `\r`, U+000D |
| Line Separator | U+2028 |
| Paragraph Separator | U+2029 |

`\r\n`은 원문 그대로 보존하며, 줄바꿈 여부는 `true`입니다.
이 속성은 위 네 문자의 존재 여부를 요약합니다.
경계 원문이 역슬래시와 `n` 두 글자로만 이루어져 있다면, 두 글자를 그대로 보관하고 줄바꿈 여부는 `false`입니다.
NEL(U+0085)·수직 탭(`\v`)·폼 피드(`\f`)만으로 이루어진 경계도 `false`입니다.

**왜 검사하는 문자를 명시하나요?**
“줄바꿈”이라는 이름만으로는 Unicode의 모든 줄 구분 문자를 검사하는지 알 수 없기 때문입니다.
사용할 언어가 요구하는 문자 집합과 현재 API의 계약을 비교할 수 있어야 합니다.
이 집합은 ECMAScript의 줄 종결 문자 네 가지와 일치합니다.
([ECMAScript 줄 종결 문자](https://tc39.es/ecma262/multipage/ecmascript-language-lexical-grammar.html#sec-line-terminators))

해당 문자가 `LeadingTrivia.Text`에 실제로 들어와야 판정 대상이 됩니다.
등록되지 않은 문자와 직접 파싱하는 줄바꿈은 파싱 토큰열에 남습니다.
문자열 토큰 **내부**의 줄바꿈은 해당 토큰의 본문에 속합니다.

기본 문법의 구분자는 공백·탭·CR·LF입니다.
독립적인 U+2028·U+2029도 경계 정보로 취급하려면 해당 문자를 제외할 구분자 규칙이 필요합니다.
반면 이미 인식한 주석 안의 U+2028·U+2029는 주석 전체가 제외될 때 경계 원문에 포함됩니다.

**왜 경계 원문을 직접 검사하나요?**
직접 경계 원문을 검사하면 줄 번호 계산과 독립적으로 해당 문자들의 존재를 확인할 수 있기 때문입니다.
기존 `StartLineIndex`·열 계산은 `\n` 중심의 처리를 포함하고,
경계 원문의 줄바꿈 판정은 위 네 문자를 직접 검사합니다.

## ⑤ C#에서 직접 확인하기

이 예제는 **현재 소스 트리의 엔진을 참조하는 콘솔 프로젝트**용입니다.
저장소 루트에서 실행하는 예를 들면 다음과 같습니다.
`LexerExample`은 예제용 폴더 이름이며 원하는 이름으로 바꿔도 됩니다.

```powershell
dotnet new console -n LexerExample -o artifacts/LexerExample
dotnet add artifacts/LexerExample/LexerExample.csproj reference src/FrontEnd/Parse.FrontEnd.Parsers/Parse.FrontEnd.Parsers.csproj
dotnet add artifacts/LexerExample/LexerExample.csproj reference src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/Parse.FrontEnd.Grammars.Ebnf.csproj
```

현재 소스의 새 API를 확인하기 위해 파서와 EBNF 리더 구현 프로젝트를 직접 참조합니다.
`Program.cs`를 다음 내용으로 바꾼 뒤 `dotnet run --project artifacts/LexerExample`을 실행하세요.
원시 문자열 리터럴을 사용하므로 C# 11 이상이 필요합니다. 저장소의 .NET 10 SDK 환경에서 실행할 수 있어요.

```csharp
using System;
using System.Linq;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;

// return, 식별자, 세미콜론 순서의 작은 문법입니다.
// 토큰 사이의 원문과 줄바꿈 여부를 조회합니다.
var read = EbnfGrammarReader.Read("""
    Start : 'return' id ';' ;
    id := "[a-z]+" ;
    comment := Comment "/\*[\s\S]*?\*/|//[^\r\n]*" ;
    """);

if (!read.Success)
    throw new InvalidOperationException(string.Join(Environment.NewLine, read.Errors));

var lexer = new Lexer();
foreach (var terminal in read.Grammar.TerminalSet)
    lexer.AddTokenRule(terminal);

var source = "return\n    value\n;";
var lexed = lexer.Lexing(source);
var value = lexed.TokensForParsing.Single(token => token.Data == "value");

Console.WriteLine($"토큰: {value.Data}");
Console.WriteLine($"앞 원문: \"{Escape(value.LeadingTrivia.Text)}\"");
Console.WriteLine($"원문 범위: [{value.LeadingTrivia.StartIndex}, {value.LeadingTrivia.EndIndexExclusive})");
Console.WriteLine($"앞 줄바꿈: {value.HasLineBreakBefore}");

var parser = new LALRParser(read.Grammar, true);
var result = parser.Parsing(lexed.TokensForParsing);
Console.WriteLine($"파싱 성공: {result.Success}");
if (!result.Success)
    throw new InvalidOperationException("예제 입력의 파싱이 실패했습니다.");

var treeValue = result.ToParseTree.AllTokens.Single(token => token.Input == "value");
Console.WriteLine($"트리에도 같은 원문: {treeValue.LeadingTrivia.Text == value.LeadingTrivia.Text}");

var restored = string.Concat(lexed.TokensForParsing.Select(
    token => token.LeadingTrivia.Text + token.Data)) + lexed.TrailingTrivia.Text;
Console.WriteLine($"원문 복원 일치: {restored == source}");

static string Escape(string text) => text
    .Replace("\\", "\\\\")
    .Replace("\r", "\\r")
    .Replace("\n", "\\n")
    .Replace("\t", "\\t")
    .Replace("\u2028", "\\u2028")
    .Replace("\u2029", "\\u2029");
```

예상 출력은 다음과 같습니다.

```text
토큰: value
앞 원문: "\n    "
원문 범위: [6, 11)
앞 줄바꿈: True
파싱 성공: True
트리에도 같은 원문: True
원문 복원 일치: True
```

**왜 줄바꿈이 있는데도 파싱 성공인가요?**
예제 문법이 `return`, `id`, `;`의 순서를 요구하고, 공백과 줄바꿈은 경계 정보로 보존하기 때문입니다.
파서는 이 세 토큰의 순서를 검사합니다.
`source`를 `"return value;"`로 바꾸면 같은 문법으로 성공하고, `앞 줄바꿈`은 `False`로 바뀝니다.

**왜 원문 복원도 확인하나요?**
경계 정보의 목적은 실제 입력을 정확히 보존하는 것입니다.
토큰마다 `앞 원문 + 본문`을 이어 붙이고 끝 원문을 더하면 입력과 같아야 합니다.
일반적인 전체 입력 렉싱에서 공백·주석을 누락하거나 두 번 포함한 경우를 이 확인으로 찾을 수 있어요.

## ⑥ `TrailingTrivia`와 EOF를 나누는 이유

```csharp
var lexedWithTail = lexer.Lexing("return value; // end\n");
Console.WriteLine($"\"{Escape(lexedWithTail.TrailingTrivia.Text)}\"");
// 출력: " // end\n"
Console.WriteLine(lexedWithTail.TrailingTrivia.HasLineBreak); // True
```

`TrailingTrivia`는 `LexingData`에 있습니다. 끝 원문의 줄바꿈 여부는 `HasLineBreak`로 읽습니다.
이 속성은 마지막 토큰 뒤에서 입력 끝까지의 원문을 담습니다.
파싱 토큰이 하나도 없는 공백·주석 전용 입력이라면, 제외된 입력 전체를 이 속성에 보관합니다.

입력 끝 토큰은 파서가 토큰 목록을 받을 때 합성하며, 이 토큰의 `LeadingTrivia`는 빈 값입니다.
실제 끝 원문은 렉싱 결과의 `TrailingTrivia`에 보관합니다.

**왜 이것을 구분해서 알아야 하나요?**
합성 EOF에서 `HasLineBreakBefore`를 읽고 “파일 끝에 줄바꿈이 없었다”고 결론 내리면 틀릴 수 있기 때문입니다.
현재 끝 경계의 정확한 정보는 `lexed.TrailingTrivia`에서 읽어야 합니다.
토큰 목록을 사용하는 파서 호출에서도, 끝 원문을 조회할 수 있도록 `LexingData`를 함께 보관합니다.
향후 끝 경계 조건을 실행하는 기능을 만들 때에는 이 정보의 전달 경로도 명시적으로 마련해야 합니다.

## ⑦ 구현은 언제 경계 정보를 만드나요?

현재 흐름은 다음과 같습니다.

```text
원문 토큰화
  → 표시용·파싱용 목록과 인덱스 구성
  → 토큰 위치 조정 완료
  → RebuildTrivia()
      제외된 토큰: 원문을 임시 버퍼에 순서대로 누적
      파싱 토큰: 누적 원문을 LeadingTrivia로 연결하고 버퍼 비움
      입력 끝: 남은 원문을 TrailingTrivia로 보관
```

**왜 토큰 분류와 위치 조정 뒤에 만드나요?**
어느 토큰이 파싱에서 제외되는지와 원문에서 어디에 놓이는지가 확정되어야 정확한 경계가 나오기 때문입니다.
처음 렉싱할 때의 `InitToken`뿐 아니라, 편집 후 토큰을 바꾸는 `ReplaceToken`에서도 다시 만듭니다.
삽입·삭제·치환으로 토큰 본문이 그대로 남아도, 그 앞의 주석이나 줄바꿈은 바뀔 수 있어요.

제외된 원문을 모을 때는 `StringBuilder`를 사용합니다.
경계 정보를 재구성하는 작업은 표시용 토큰 수와 제외된 원문의 길이에 비례합니다.
현재는 매번 전체 표시용 목록을 훑습니다.

**왜 이 비용을 감수하나요?**
인접한 토큰만 갱신하면 뒤쪽 토큰의 이동한 위치나 주석 경계를 빠뜨릴 수 있기 때문입니다.
첫 단계에서는 모든 경계가 현재 입력과 일치하는지 확실히 보장하고,
나중에 같은 결과를 유지하는 조건 아래에서 갱신 범위를 줄일 수 있습니다.

`InitToken`과 `ReplaceToken`에 들어오는 토큰 열거도 배열로 한 번 확보합니다.
**왜 한 번만 열거하나요?** 한 번만 읽을 수 있는 입력 열거자를 여러 번 읽거나,
교체 중에 바뀌는 목록을 다시 읽으면 서로 다른 토큰 집합으로 인덱스와 경계를 만들 수 있기 때문입니다.

## ⑧ 편집 후 파서 캐시도 확인하는 이유

다음과 같이 입력을 수정해 보세요.

```text
수정 전: a; b;
수정 후: a;
         b;
```

토큰 본문은 여전히 `a`, `;`, `b`, `;`입니다.
그러나 `b` 앞의 원문은 공백에서 줄바꿈으로 바뀌었습니다.
토큰 글자만 비교해서 옛 파싱 결과를 재사용하면, 재토큰화로 새 토큰 객체가 생겼을 때
트리나 추적 기록이 옛 토큰의 공백 정보를 가리킬 수 있습니다.

**왜 파서 캐시도 함께 확인하나요?**
렉서 결과가 정확해도 다음 단계가 옛 객체를 참조하면 사용자는 오래된 정보를 받기 때문입니다.
이전 결과를 사용하는 `Parsing(lexingData, previousResult)` 경로에서 경계 정보도 확인합니다.

- 이전 결과와 현재 토큰을 개수 기준으로 정렬할 수 없으면 기존 전체 파싱 경로를 사용합니다.
- 대응 토큰의 경계 원문이나 경계 시작 위치가 달라도 기존 전체 파싱 경로를 사용합니다.
- 같은 토큰 객체가 유지되어 현재 정보를 바로 조회할 수 있고 이 검사에서 차이가 없으면,
  기존 증분 파싱 경로를 계속 사용할 수 있습니다.

**왜 우선 전체 파싱을 사용하나요?**
옛 토큰 참조가 파싱 블록·트리·추적 기록에 함께 퍼져 있을 수 있기 때문입니다.
한 곳만 교체하는 것보다 기존 전체 파싱 경로로 일치하는 결과를 만드는 편이 현재 구현에서 확실합니다.
그 대신 새 토큰의 경계 정보나 개수가 달라지는 편집에서는 전체 파싱 비용이 들 수 있습니다.
기존 증분 파싱 경로와 전체 파싱 경로 중 어느 쪽을 사용할지는 앞의 검사 결과로 결정합니다.

## ⑨ 테스트는 무엇을, 왜 확인하나요?

이번 기능에는 렉서 경계 정보 **52개**, 파서 전달 **21개**, 총 **73개 테스트 사례**를 추가했습니다.
단순히 속성이 존재하는지보다 다음과 같은 동작을 확인합니다.

| 확인하는 것 | 필요한 이유 |
|---|---|
| `return` 예제의 원문·범위·줄바꿈 | 사용자가 요구한 토큰별 정보가 정확해야 합니다 |
| 연속 공백·주석, CRLF와 Unicode 줄바꿈, 실제 개행과 `\n` 두 글자 | 흔한 입력 형태와 서로 다른 표현을 혼동하면 안 됩니다 |
| 첫 토큰 앞, 마지막 토큰 뒤, 빈 입력·주석 전용 입력 | 다음 토큰이 없거나 처음인 경계도 원문을 잃으면 안 됩니다 |
| 문자열 내부 개행, 직접 파싱하는 개행, 알 수 없는 토큰 | 다른 경계의 정보나 오류 입력을 잘못 흡수하면 안 됩니다 |
| UTF-16 범위와 원문 복원 | 저장한 위치로 실제 문자열을 정확히 찾을 수 있어야 합니다 |
| 삽입·삭제·치환 후 새로 렉싱한 결과와 비교 | 편집 방식이 달라도 같은 원문에는 같은 경계 정보가 나와야 합니다 |
| 파싱 블록·CST·AST·추적 기록에서 조회 | 렉서에서 만든 정보가 사용자가 보는 결과까지 유지되어야 합니다 |
| 이전 파싱 결과를 재사용한 뒤의 원문 정보 | 본문이 같은 토큰에 옛 경계 정보가 남으면 안 됩니다 |
| 한 번만 읽을 수 있는 열거자와 과거 스냅샷 | 입력 소비 방식과 불변 정보의 계약이 유지되어야 합니다 |

## ⑩ 구현 위치와 다음 단계

저장소에서 확인할 파일은 다음과 같습니다.

| 파일 | 역할 |
|---|---|
| `src/FrontEnd/Parse.FrontEnd/Tokenize/TokenTrivia.cs` | 불변 경계 원문, 범위, 줄바꿈 판정 |
| `src/FrontEnd/Parse.FrontEnd/Tokenize/TokenCell.cs` | 렉서 토큰의 조회 API |
| `src/FrontEnd/Parse.FrontEnd/Tokenize/LexingData [Trivia].cs` | 경계 원문 재구성, 끝 원문 보관 |
| `src/FrontEnd/Parse.FrontEnd/Tokenize/LexingData.cs` | 초기화·교체 뒤 갱신 연결 |
| `src/FrontEnd/Parse.FrontEnd/TokenData.cs` | 파서 토큰에서 현재 정보 조회 |
| `src/FrontEnd/Parse.FrontEnd.Parsers/LR/LRParser.cs` | 경계 정보가 달라진 이전 결과의 재사용 검사 |
| `Tests/Parse.FrontEnd.Parsers.Tests/Common/TokenTriviaTests.cs` | 렉서의 정보 보존·갱신 검증 |
| `Tests/Parse.FrontEnd.Parsers.Tests/Conformance/TokenTriviaParserTests.cs` | 기존 문법→렉서→LALR 경로와 캐시 검증 |

현재 완성한 것은 **언어에 공통으로 필요한 입력 정보를 만들고 조회할 수 있게 하는 첫 단계**입니다.
다음 단계에서 줄바꿈 조건을 문법에 어떻게 표현하고 파싱 중 어디에서 판단할지를 설계해야 합니다.
EBNF 리더의 조건 읽기와 실제 조건 실행을 함께 연결하고,
그 다음에 특정 언어의 허용·오류 사례를 검증할 수 있습니다.

**왜 이 순서로 진행하나요?**
판단에 필요한 사실을 먼저 정확히 보존해야, 문법 조건을 구현할 때 정보 누락 문제와 규칙 적용 문제를
나누어 확인할 수 있기 때문입니다.

기본 설명으로 돌아가려면 → **[렉서 기본 편](lexer.md)**

심볼 구조를 이어서 보려면 → **[Terminal](deep-terminal.md)** · **[NonTerminal](deep-nonterminal.md)**
