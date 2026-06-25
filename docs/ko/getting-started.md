# 빠른 시작

이 장의 목표는 딱 하나예요.\
**5줄짜리 코드로, 직접 정의한 문법으로 첫 파싱을 돌려보는 것.**
깊은 설명은 다음 장부터 천천히 할 거고, 여기선 딱 한 가지 — *파싱이 뭔지* 만 가볍게 짚고
바로 돌려볼게요.

## 그 전에 — 파싱이 뭔가요?

한 문장이면 돼요.\
우리가 `a + a * a` 같은 **글자들의 나열(텍스트)** 을 컴퓨터에게 주면, 컴퓨터
눈엔 처음엔 그냥 *글자 더미* 일 뿐이에요.\
아무 의미가 없죠.

**파싱(parsing)** 은 이 글자 더미가 **문법에 맞는지 확인하고, 그 속 구조를 트리 모양으로
밝혀내는** 일이에요.\
국어 시간에 문장을 "주어 + 서술어"로 나누던 것과 똑같아요.

```
a + a * a   →   "아, 이건 (a) 더하기 (a 곱하기 a) 구조구나"
```

이 구조를 알아야 그다음(계산·번역·컴파일)으로 넘어갈 수 있어요.\
파싱은 거의 모든 언어 처리의
**첫 관문**이에요.\
자 — 이 '파싱'을 이제 Orchid로 직접 해봅시다.

## 설치

먼저 패키지를 깔아요.\
.NET 프로젝트에서:

```bash
dotnet add package Orchidaceae --prerelease
```

> 패키지 이름이 왜 **`Orchidaceae`** 냐면 — 브랜드명 "Orchid"가 nuget.org에 이미 예약돼 있어서,
> 난초과(科)의 학명인 `Orchidaceae`를 id로 써요. `--prerelease`는 아직 초기 프리뷰라 붙입니다.

> 🌿 **설치가 번거로우면 잠깐 미뤄도 돼요.** 잠시 뒤에 짤 코드와 *똑같은 걸* **브라우저에서
> 클릭만으로** 해볼 수 있는 [라이브 플레이그라운드](https://polite-island-0b2142200.7.azurestaticapps.net)가
> 있거든요. 거기선 파싱 테이블·트리까지 그림으로 보여줘요. 일단은 눈으로 먼저 보고 와도 좋아요.

## 첫 파싱 (5줄)

설치했으면, 콘솔 프로젝트의 `Program.cs`에 아래를 그대로 붙여넣어 보세요.

```csharp
using Parse.FrontEnd.Grammars.Ebnf;   // 문법을 텍스트로 읽는 리더
using Parse.FrontEnd.Parsers.LR;       // LALR 파서
using Parse.FrontEnd.Tokenize;         // 렉서(토크나이저)

// ① 문법을 EBNF 텍스트로 정의한다
var read = EbnfGrammarReader.Read(@"
    Expr   : Expr '+' Term | Term ;
    Term   : Term '*' Factor | Factor ;
    Factor : '(' Expr ')' | id ;
    id     := ""[a-zA-Z]+"" ;");

var grammar = read.Grammar;

// ② 렉서를 만들고, 문법이 쓰는 토큰 규칙을 등록한다
var lexer = new Lexer();
foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);

// ③ 파서를 만들고
var parser = new LALRParser(grammar);

// ④ 입력을 토큰으로 쪼개서(lexing) 파싱한다
var result = parser.Parsing(lexer.Lexing("a + a * a").TokensForParsing);

// ⑤ 결과 확인
Console.WriteLine(result.Success);   // True
```

`dotnet run` 하면 **`True`** 가 찍혀요.\
방금 여러분은 *직접 정의한 문법* 으로 문자열을 파싱한
거예요. 🎉

## 방금, 무슨 일이 일어난 걸까요

코드 다섯 토막이 각각 한 가지씩 했어요.\
지금은 **이름만 눈에 익히면 충분해요** — 하나하나는
앞으로 한 장씩 천천히 풀어갈 거니까요.

| 단계 | 한 일 |
|---|---|
| ① `EbnfGrammarReader.Read` | 텍스트로 쓴 문법 → `Grammar` 객체로 |
| ② `lexer.AddTokenRule` | 글자를 *토큰* 으로 쪼갤 규칙 등록 |
| ③ `new LALRParser(grammar)` | 문법으로부터 **파싱 테이블**을 미리 계산 |
| ④ `parser.Parsing(...)` | 토큰들을 실제로 파싱 (shift / reduce) |
| ⑤ `result.Success` | 입력이 문법에 맞았는지 (`True` / `False`) |

딱 하나만 짚고 넘어갈게요.\
③에서 슬쩍 **"파싱 테이블을 *미리* 계산한다"** 고 했죠?\
사실 이게 LR
파서의 가장 큰 특징이자, 이 매뉴얼의 핵심 여정이에요.\
그리고 — **바로 다음 장이 그 이야기**예요.

## 다음 장

방금은 "돌아가는 것"을 손으로 만져봤어요.\
이제 한 발 물러서서 **전체 그림**을 봅시다 — 텍스트가
결과가 되기까지 안에서 어떤 단계들이 흐르는지, 그리고 방금 말한 그 '미리 만드는 표'가 뭔지.

👉 **[한눈에 보는 파이프라인](the-big-picture.md)**
