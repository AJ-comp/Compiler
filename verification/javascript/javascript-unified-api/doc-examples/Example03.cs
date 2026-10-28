namespace DocumentationExample03
{
using System;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;


internal static class Example
{
public static bool Completed;
public static void Run()
{
var language = new JavaScriptGrammar();
var parser = new LALRParser(language.Grammar);
var input = @"
async function load(path) {
    return await read(path);
}
function* count() {
    yield 1;
    yield 2;
}
";

var parsed = parser.Parsing(language.CreateTokenSource(input));
if (!parsed.Success)
{
    Console.WriteLine(parser.FiredException?.Message);
    return;
}

var diagnostics = language.Validate(parsed.ToParseTree);
foreach (var diagnostic in diagnostics)
    Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message} " +
        $"(위치 {diagnostic.SourceOffset}, 길이 {diagnostic.Length})");

if (diagnostics.Count == 0)
    Console.WriteLine("문법 검사 통과");

if (!parsed.Success || diagnostics.Count != 0)
    throw new InvalidOperationException("Example did not pass parsing and validation.");
Completed = true;
}
}
}
