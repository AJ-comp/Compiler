using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new TypeScriptGrammar(TypeScriptInputKind.Module);
var parser = new LALRParser(language.Grammar, true);
foreach (string source in new[]
{
    "export declare namespace Tools { function run(): void; }",
    "declare namespace Tools { function run(): void; }",
    "export declare namespace Tools { function run<T>(): void; }",
    "export declare namespace Tools { function run() ; }",
    "export declare namespace Tools { function run(): void {} }"
})
{
    var parsed = parser.Parsing(language.CreateTokenSource(source));
    Console.WriteLine("INPUT " + source + " SUCCESS=" + parsed.Success);
    Console.WriteLine("EXCEPTION " + parser.FiredException);
    foreach (var unit in parsed.SelectMany(block => block.Units).TakeLast(18))
    {
        Console.WriteLine("TOKEN=" + unit.InputValue?.Input + " ACTION=" + unit.Action + " ERROR=" + unit.ErrorMessage);
        if (unit.IsError)
        {
            foreach (var state in unit.BeforeStack.Stack.OfType<int>().Take(2))
            {
                Console.WriteLine("STATE " + state);
                if (parser.Canonical.IndexStateDic.TryGetValue(state, out var items))
                    foreach (var item in items) Console.WriteLine("  " + item + " LA=" + item.LookAhead);
            }
        }
    }
}
