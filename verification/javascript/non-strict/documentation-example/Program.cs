using System;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new JavaScriptScriptGrammar(
    JavaScriptMode.NonStrict,
    webCompatibility: true);
var parser = new LALRParser(language.Grammar);

// 아래 문자열이 실제 JavaScript 입력입니다.
var input = @"<!-- 이전 웹 문서의 주석
var total = 077;
with (settings) {
    total = value;
}";

var parsed = parser.Parsing(language.CreateTokenSource(input));
if (!parsed.Success)
{
    Console.WriteLine(parser.FiredException?.Message ?? "파싱 오류");
    return;
}

var diagnostics = language.Validate(parsed.ToParseTree);
foreach (var diagnostic in diagnostics)
    Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message} " +
        $"(위치 {diagnostic.SourceOffset}, 길이 {diagnostic.Length})");

if (diagnostics.Count == 0)
    Console.WriteLine("문법 검사 통과");

// Verification-only suffix: the documentation example above is unchanged.
if (!parsed.Success || diagnostics.Count != 0)
    throw new InvalidOperationException("The documentation example failed.");
Console.WriteLine($"Parsing.Success={parsed.Success}");
Console.WriteLine($"Validate.Count={diagnostics.Count}");
Console.WriteLine("ORIGINAL INPUT:");
Console.WriteLine(input);
Console.WriteLine("VERIFIED_DOC_EXAMPLE");
