namespace DocumentationExample02
{
using System;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;


internal static class Example
{
public static bool Completed;
public static void Run()
{
var language = new JavaScriptGrammar(inputKind: JavaScriptInputKind.Expression);
var parser = new LALRParser(language.Grammar);

var input = "result = /ab/g.test(value)";
var source = language.CreateTokenSource(input);
var parsed = parser.Parsing(source);

if (!parsed.Success)
{
    Console.WriteLine(parser.FiredException?.Message);
    return;
}

var diagnostics = language.Validate(parsed.ToParseTree);
if (diagnostics.Count > 0)
{
    foreach (var diagnostic in diagnostics)
    {
        Console.WriteLine(
            $"{diagnostic.Code}: {diagnostic.Message} " +
            $"(위치 {diagnostic.SourceOffset}, 길이 {diagnostic.Length})");
    }
    return;
}

// 구조와 정적 문법 검사를 모두 통과한 구문 트리입니다.
var tree = parsed.ToParseTree;
Console.WriteLine(tree.AllInputDatas);

if (!parsed.Success || diagnostics.Count != 0)
    throw new InvalidOperationException("Example did not pass parsing and validation.");
Completed = true;
}
}
}
