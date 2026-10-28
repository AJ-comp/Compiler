using System.Text.Json;
using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.Datas.LR;

var language = new TypeScriptGrammar(TypeScriptInputKind.Script, JavaScriptMode.Strict);
var failures = new List<object>();
foreach (var production in language.Grammar.NonTerminalSingles)
{
    int baseHash;
    try { baseHash = production.GetHashCode(); }
    catch (OverflowException) { failures.Add(new { Phase = "Production", Production = production.ToString() }); continue; }
    for (int dot = 0; dot <= production.Count; dot++)
    {
        var item = new LRItem(production, (sbyte)dot);
        try { _ = item.GetHashCode(); }
        catch (OverflowException)
        {
            failures.Add(new { Phase = "LRItem", Production = production.ToString(), Item = item.ToString(),
                UniqueKey = production.UniqueKey, BaseHash = baseHash, Dot = dot,
                Concatenated = baseHash.ToString() + dot.ToString(), Int32Max = int.MaxValue });
        }
    }
}
Console.WriteLine(JsonSerializer.Serialize(new { ProductionCount = language.Grammar.NonTerminalSingles.Count, Failures = failures }, new JsonSerializerOptions { WriteIndented = true }));
