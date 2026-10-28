using System.Text.Json;
using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers.LR;

var cases = new[] {
    ("class-control", "class C<T> { constructor(public value: T) {} }"),
    ("generic-call", "const value = make<string>();"),
    ("generic-new", "const value = new Box<string>();"),
    ("instantiation", "const makeString = make<string>;"),
    ("typed-arrow", "const id = (value: number): number => value;"),
    ("generic-arrow", "const id = <T>(value: T): T => value;"),
    ("angle-assertion", "const value = <string>input;"),
    ("type-import", "import type { User } from './user';"),
    ("type-export", "type User = string; export type { User };"),
    ("import-equals", "import fs = require('fs');"),
    ("export-equals", "const value = 1; export = value;"),
    ("decorator", "@sealed class C {}"),
    ("tsx", "const view = <Button />;")
};
var grammar = new TypeScriptGrammar(TypeScriptInputKind.Module);
var parser = new LALRParser(grammar.Grammar, false);
foreach (var (id, source) in cases)
{
    try
    {
        var parsed = parser.Parsing(grammar.CreateTokenSource(source));
        var diagnostics = parsed.Success ? grammar.Validate(parsed.ToParseTree).Select(d => d.Code).ToArray() : Array.Empty<string>();
        Console.WriteLine(JsonSerializer.Serialize(new { id, source, parsed = parsed.Success, valid = parsed.Success && diagnostics.Length == 0, diagnostics, exception = parser.FiredException?.GetType().Name }));
    }
    catch (Exception exception)
    {
        Console.WriteLine(JsonSerializer.Serialize(new { id, source, valid = false, exception = exception.GetType().Name, message = exception.Message }));
    }
}
