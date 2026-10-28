using System;
using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new TypeScriptGrammar();
var parser = new LALRParser(language.Grammar);
var parsed = parser.Parsing(language.CreateTokenSource(
    "class Box<T> { constructor(public value: T) {} }"));
if (!parsed.Success)
    throw new InvalidOperationException(parser.FiredException?.Message ?? "Parsing failed.");

var diagnostics = language.Validate(parsed.ToParseTree);
foreach (var diagnostic in diagnostics)
    Console.WriteLine($"{diagnostic.Code}: {diagnostic.Message}");
Console.WriteLine(diagnostics.Count == 0);
