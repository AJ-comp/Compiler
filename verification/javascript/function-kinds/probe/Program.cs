using System.Text.Json;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
var runners = new Dictionary<string,(LALRParser parser,Func<string,ITokenSource> source,Func<ParseTreeSymbol,IReadOnlyList<JavaScriptDiagnostic>> validate)>();
var expression = new JavaScriptExpressionGrammar();
runners.Add("Expression",(new LALRParser(expression.Grammar),expression.CreateTokenSource,expression.Validate));
var script = new JavaScriptScriptGrammar();
runners.Add("Script",(new LALRParser(script.Grammar),script.CreateTokenSource,script.Validate));
var module = new JavaScriptModuleGrammar();
runners.Add("Module",(new LALRParser(module.Grammar),module.CreateTokenSource,module.Validate));
using var document=JsonDocument.Parse(File.ReadAllText(args[0]));
int total=0,mismatches=0;
foreach(var row in document.RootElement.EnumerateArray())
{
    string input=row.GetProperty("input").GetString();
    string goal=row.GetProperty("goal").GetString();
    bool expected=row.GetProperty("expected").GetBoolean();
    var r=runners[goal];
    var parsed=r.parser.Parsing(r.source(input));
    var diagnostics=parsed.Success?r.validate(parsed.ToParseTree):Array.Empty<JavaScriptDiagnostic>();
    bool accepted=parsed.Success&&diagnostics.Count==0;
    total++;
    if(expected!=accepted)
    {
        mismatches++;
        Console.WriteLine(JsonSerializer.Serialize(new{input,goal,expected,accepted,parsed=parsed.Success,error=r.parser.FiredException?.ToString(),diagnostics=diagnostics.Select(d=>new{d.Code,d.Message,d.SourceOffset,d.Length})}));
    }
}
Console.WriteLine($"Compared {total}; mismatches {mismatches}.");
Environment.ExitCode=mismatches==0?0:1;
