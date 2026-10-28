using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Parsers.Collections;

var language = new TypeScriptGrammar(TypeScriptInputKind.Module);
Console.WriteLine("=== GRAMMAR ===");
foreach (var line in language.GrammarText.Split('\n'))
    if (line.StartsWith("TSFunctionOverload") || line.StartsWith("FunctionDeclaration") || line.StartsWith("FormalParameters")) Console.WriteLine(line);
var parser = new LALRParser(language.Grammar, true);
var table = (LRParsingTable)parser.ParsingTable;
var parsed = parser.Parsing(language.CreateTokenSource("export declare namespace Tools { function run(): void; }"));
Console.WriteLine("SUCCESS " + parsed.Success);
foreach (var unit in parsed.SelectMany(block => block.Units))
{
    var token = unit.InputValue;
    Console.WriteLine("TOKEN=" + token?.Input + " ACTION=" + unit.Action + " ERROR=" + unit.ErrorMessage);
    if (token == null) continue;
    int state = unit.BeforeStack.Stack.OfType<int>().First();
    Console.WriteLine("STATE=" + state);
    if (table[state].MatchedValueSet.TryGetValue(token.Kind, out var actions))
        Console.WriteLine("CANDIDATES=" + string.Join(" | ", actions.Select(action => action.ToString())));
    if (token.Input == "function" || token.Input == "(" || token.Input == ")")
        foreach (var item in parser.Canonical.IndexStateDic[state]) Console.WriteLine("  " + item);
}
