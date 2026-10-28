namespace DocumentationExample09
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
var parsed = parser.Parsing(language.CreateTokenSource(
    "async function load({path}) { return /ab/g.test(await read(path)); }"));
if (!parsed.Success)
    throw new InvalidOperationException(parser.FiredException?.Message ?? "Parsing failed.");

var diagnostics = language.Validate(parsed.ToParseTree);
if (diagnostics.Count > 0)
{
    foreach (var diagnostic in diagnostics)
        Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message} " +
            $"(offset {diagnostic.SourceOffset}, length {diagnostic.Length})");
    return;
}

Console.WriteLine(parsed.ToParseTree.AllInputDatas);

if (!parsed.Success || diagnostics.Count != 0)
    throw new InvalidOperationException("Example did not pass parsing and validation.");
Completed = true;
}
}
}
