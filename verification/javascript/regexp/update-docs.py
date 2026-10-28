from pathlib import Path


def update(path, replacements):
    p = Path(path)
    raw = p.read_bytes()
    crlf = b"\r\n" in raw
    text = raw.decode("utf-8").replace("\r\n", "\n")
    for old, new in replacements:
        if old not in text:
            raise SystemExit(f"Missing text in {path}: {old[:90]}")
        text = text.replace(old, new, 1)
    p.write_bytes((text.replace("\n", "\r\n") if crlf else text).encode("utf8"))


update("docs/ko/javascript-expressions.html", [
    ('            <tr><td>태그 없는 템플릿에 잘못된 이스케이프가 있는가</td>',
     '            <tr><td>정규식 리터럴의 본문이 모드별 문법을 지키는가</td><td><code>/[z-a]/u</code>, <code>/(a)\\2/u</code>. 그룹·범위·역참조와 v 집합의 검사 순서는 <a href="javascript-regexp.html">정규식 본문 설명</a>에서 확인합니다.</td></tr>\n            <tr><td>태그 없는 템플릿에 잘못된 이스케이프가 있는가</td>'),
    ('다음 언어 단계는 async 함수·async 화살표 함수와 제너레이터, 각 함수에 대응하는 메서드와 객체 메서드·접근자입니다. 정규식 리터럴은 현재의 경계·플래그 검사에 이어 <code>RegExpPattern</code> 문법으로 본문 내부를 검사하는 단계가 남아 있습니다.',
     '정규식 리터럴도 <code>Validate</code>에서 본문의 <code>RegExpPattern</code> 문법을 검사합니다. 일반·u·v 모드의 그룹·역참조·Unicode 속성과 집합 조건은 <a href="javascript-regexp.html">정규식 본문 문서</a>에서 설명합니다. 다음 언어 단계는 async 함수·async 화살표 함수와 제너레이터, 각 함수에 대응하는 메서드와 객체 메서드·접근자입니다.'),
    ('          <li><a href="parser-driven-lexing.html">Lexer가 source를 만들고, 파서가 토큰을 받아 파싱하는 구조</a></li>',
     '          <li><a href="parser-driven-lexing.html">Lexer가 source를 만들고, 파서가 토큰을 받아 파싱하는 구조</a></li>\n          <li><a href="javascript-regexp.html">정규식 리터럴의 본문 문법과 검사 순서</a></li>'),
])

update("docs/ko/javascript-statements.html", [
    ('다음 단계는 async 함수·async 화살표 함수와 제너레이터, 각 함수에 대응하는 메서드와 객체 메서드·접근자입니다. 정규식 리터럴은 현재의 토큰 경계·플래그 검사에 이어, 본문 내부의 <code>RegExpPattern</code> 문법 검사를 추가하는 단계가 남아 있습니다.',
     'Script 안의 정규식 리터럴은 토큰의 경계·플래그를 확인한 뒤, <code>Validate</code>에서 본문 문법까지 검사합니다. 함수 본문과 템플릿 보간 안의 정규식에도 같은 검사를 적용하며, <a href="javascript-regexp.html">정규식 문서</a>에서 일반·u·v 모드의 차이와 검사 이유를 설명합니다. 다음 언어 단계는 async 함수·async 화살표 함수와 제너레이터, 각 함수에 대응하는 메서드와 객체 메서드·접근자입니다.'),
])

update("docs/ko/javascript-classes.html", [
    ('        <p><span class="core">따라서 파싱 성공만으로 클래스 검사가 끝나지 않습니다. 같은 구문 트리에 Validate를 적용하고 진단이 없는지도 확인해야 합니다.</span> 검사는 원래 트리의 노드와 입력 토큰을 바꾸지 않으며, 오류가 있는 원문 위치를 진단에 기록합니다.</p>',
     '        <p><span class="core">따라서 파싱 성공만으로 클래스 검사가 끝나지 않습니다. 같은 구문 트리에 Validate를 적용하고 진단이 없는지도 확인해야 합니다.</span> 검사는 원래 트리의 노드와 입력 토큰을 바꾸지 않으며, 오류가 있는 원문 위치를 진단에 기록합니다.</p>\n        <p>클래스 멤버 안의 표현식에도 기존 JavaScript 검사를 함께 적용합니다. 예를 들어 필드 초기값이나 메서드 본문에 <code>/[z-a]/u</code>가 있으면, <code>Validate</code>가 잘못된 정규식 범위를 찾습니다. 이 본문 검사의 모드와 실행 순서는 <a href="javascript-regexp.html">정규식 문서</a>에서 설명합니다.</p>'),
])

update("docs/ko/javascript-modules.html", [
    ('        <p>이 검사는 원문에 나타난 구조와 이름 관계를 확인합니다. 상대 모듈에 실제로 어떤 항목이 있는지 확인하고 여러 파일의 재내보내기를 연결하는 검사는 모듈 로더와 링커의 책임입니다.</p>',
     '        <p>모듈의 정적 조건을 검사할 때에는 내부 표현식의 기존 검사도 함께 수행합니다. 정규식 리터럴의 그룹·범위·역참조와 Unicode 집합도 여기에 포함됩니다. <a href="javascript-regexp.html#legacy">정규식의 일반·u·v 모드</a>는 Module 여부와 별도로 각 리터럴의 플래그에 따라 적용합니다.</p>\n        <p>이 검사는 원문에 나타난 구조와 이름 관계를 확인합니다. 상대 모듈에 실제로 어떤 항목이 있는지 확인하고 여러 파일의 재내보내기를 연결하는 검사는 모듈 로더와 링커의 책임입니다.</p>'),
])

update("grammars/javascript/README.md", [
    ('다음 단계는 async 함수·async 화살표 함수·제너레이터와 해당 메서드, 객체 메서드·접근자입니다. 정규식은 현재 리터럴 경계와 플래그를 검사하며, 본문 내부의 `RegExpPattern` 문법 검사를 별도 단계로 둡니다.',
     '정규식은 리터럴 경계와 플래그에 이어, `Validate`에서 본문의 `RegExpPattern` 문법까지 검사합니다. 일반·u·v 모드의 그룹·반복·역참조·Unicode 속성과 집합 조건을 적용합니다. 다음 단계는 async 함수·async 화살표 함수·제너레이터와 해당 메서드, 객체 메서드·접근자입니다.'),
    ('## 파일과 생성 과정',
     '''## 정규식 본문 검사

정규식 리터럴은 토큰 하나로 읽을 수 있어도 본문에 문법 오류가 있을 수 있습니다. 예를 들어 `/[z-a]/u`는 경계와 플래그를 만족하지만, 문자 범위가 거꾸로 지정되어 있습니다. 기존 `Parsing` 뒤에 `Validate`를 호출하면 이 오류를 `regexp_syntax` 진단으로 확인할 수 있습니다.

토큰 공급자는 리터럴의 경계와 플래그를 검사하고, 본문 검사는 최종 구문 트리에 들어간 정규식 토큰만 대상으로 합니다. 예측 중에 버리는 정규식 후보의 본문 오류가 실제 나눗셈 경로를 중단하지 않도록, 이 두 검사를 나눕니다. 공통 렉서·파서 엔진은 그대로 사용합니다.

- 일반 모드는 UTF-16 코드 단위와 Annex B.1.2 호환 문법을 적용합니다. 이 정규식 모드는 JavaScript의 엄격 모드·Module 여부와 별도로 정합니다.
- u 모드는 Unicode 코드 포인트, 그룹·역참조·범위와 이스케이프의 엄격한 조건을 적용합니다.
- v 모드는 Unicode 문자 처리에 중첩 집합·교집합·차집합·`\\q{...}` 문자열과 `MayContainStrings` 조건을 더합니다.

Unicode 속성 이름은 ECMAScript 2026의 명시 목록으로 한정하고, 범주·스크립트 값은 고정한 Unicode 17 공식 별칭으로 확인합니다. 매우 긴 반복 횟수는 정확한 정수로 비교하며, 중첩 그룹과 문자 집합은 명시적인 스택으로 읽습니다. 이 계층은 정규식을 매칭하거나 JavaScript를 실행하지 않고, 원문에 대한 UTF-16 진단 범위를 반환합니다.

각 문법의 예제와 구현 이유, C# 사용 코드는 [정규식 본문 설명](../../docs/ko/javascript-regexp.html)에 있습니다.

## 파일과 생성 과정'''),
    ('| [`JavaScriptTemplateTokens.cs`]',
     '| [`JavaScriptRegExpValidation.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptRegExpValidation.cs) | 최종 정규식 토큰의 그룹·반복·역참조·이스케이프와 본문 문법을 검사합니다. |\n| [`JavaScriptRegExpSets.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptRegExpSets.cs) | v 모드의 중첩 문자 집합·연산·문자열과 부정 조건을 검사합니다. |\n| [`JavaScriptRegExpProperties.cs`](../../src/FrontEnd/Parse.FrontEnd.Grammars.Ebnf/JavaScript/JavaScriptRegExpProperties.cs) | 명세의 Unicode 속성 이름과 고정된 공식 별칭을 확인합니다. |\n| [`JavaScriptTemplateTokens.cs`]'),
    ('C#은 고정된 Unicode 데이터로 토큰 정규식을 준비하고, 이스케이프의 코드 포인트·정규식 플래그·구문 트리의 정적 조건을 검사합니다.',
     'C#은 고정된 Unicode 데이터로 토큰 정규식을 준비하고, 이스케이프의 코드 포인트·정규식 플래그와 본문·구문 트리의 정적 조건을 검사합니다.'),
    ('- 템플릿: 보간 없는 문자열,',
     '- 정규식 본문: 일반·u·v 모드의 그룹·반복·역참조·이스케이프·범위, Unicode 속성과 집합 연산·문자열, 오류의 UTF-16 위치, 깊은 중첩·긴 숫자와 기존 문맥의 조합\n- 템플릿: 보간 없는 문자열,'),
])

update("docs/TODO.md", [
    ('functions/arrows, generators, object methods/accessors and RegExpPattern-body\nvalidation are the next language stages.',
     'functions/arrows, generators and object methods/accessors are the next language\nstages. RegExpPattern-body validation now runs on the final syntax tree in all\nthree JavaScript entry points.'),
    ('## JavaScript: modules and import expressions (source update 2026-10-20)',
     '''## JavaScript: RegExpPattern body validation (source update 2026-10-20)

The JavaScript layer now validates ordinary, Unicode and Unicode-sets pattern
bodies after the parser has selected the final concrete syntax tree. Literal
boundaries and flags retain their existing lexical checks. Deferring body checks
keeps discarded speculative regexp candidates from rejecting a valid division
parse; the common lexer/parser engine is unchanged.

`JavaScriptRegExpValidation` checks groups, alternatives, captures/backreferences,
assertions, scoped i/m/s modifiers, quantifiers and traditional character classes.
It supports legacy Annex B.1.2 syntax independently of Script/Module strictness,
and uses explicit stacks and exact decimal comparisons. Named captures can share
a decoded name across disjoint alternatives; same-sequence and nested duplicates
are rejected using the specification's structural rule.

`JavaScriptRegExpSets` checks nested v-mode sets, union/intersection/subtraction,
string operands and syntactic MayContainStrings restrictions on negation.
`JavaScriptRegExpProperties` uses ECMAScript's property-name whitelist and pinned
Unicode 17 value aliases generated from hash-verified official data.

All three entry points use the existing Parsing then Validate contract and return
UTF-16 source spans for regexp_syntax errors without modifying the tree or running
a matching engine. Tests cover acceptance/rejection, diagnostic spans, repeated
inputs, speculative reads and expression/Script/Module integration. Independent
engine comparisons are checked against the fixed specification where numeric,
duplicate-name and Unicode alias behaviour differs.

See the [regexp grammar and implementation guide](ko/javascript-regexp.html).

## JavaScript: modules and import expressions (source update 2026-10-20)'''),
])

update("docs/ko/toc.yml", [
    ('  - name: JavaScript 모듈 — import·export와 문맥\n    href: javascript-modules.html',
     '  - name: JavaScript 모듈 — import·export와 문맥\n    href: javascript-modules.html\n  - name: JavaScript 정규식 — 본문 문법 검사\n    href: javascript-regexp.html'),
])

update("docs/ko/index.md", [
    ('- **[ANTLR4의 LL · 장림의 LR](antlr4-ll-janglim-lr.html)**',
     '- **[JavaScript 정규식 — 본문 문법 검사](javascript-regexp.html)** — 리터럴 경계·플래그에 이어 일반·u·v 모드의 그룹·역참조·Unicode 속성과 문자 집합을 검사하는 순서와 이유를 설명합니다.\n- **[ANTLR4의 LL · 장림의 LR](antlr4-ll-janglim-lr.html)**'),
])

print("Updated all eight guide/index/readme files.")
