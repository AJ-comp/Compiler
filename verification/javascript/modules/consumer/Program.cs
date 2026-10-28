using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using System.Text.Json;
var module = new JavaScriptModuleGrammar();
var script = new JavaScriptScriptGrammar();
var expression = new JavaScriptExpressionGrammar();
if (!module.GrammarText.Contains("ModuleItem") || !module.GrammarText.Contains("AwaitExpression"))
    throw new Exception("Module grammar resource or goal specialization is missing.");
var moduleParser = new LALRParser(module.Grammar, false);
var scriptParser = new LALRParser(script.Grammar, false);
var expressionParser = new LALRParser(expression.Grammar, false);
using var cases = JsonDocument.Parse(File.ReadAllText(args[0]));
int count = 0;
foreach (var item in cases.RootElement.EnumerateArray())
{
    string input = item.GetProperty("source").GetString();
    string mode = item.GetProperty("mode").GetString();
    bool expected = item.GetProperty("valid").GetBoolean();
    var parser = mode == "module" ? moduleParser : mode == "script" ? scriptParser : expressionParser;
    ITokenSource source = mode == "module" ? module.CreateTokenSource(input) : mode == "script" ? script.CreateTokenSource(input) : expression.CreateTokenSource(input);
    var result = parser.Parsing(source);
    bool valid = result.Success && JavaScriptExpressionValidation.Validate(result.ToParseTree).Count == 0;
    if (valid != expected) throw new Exception($"Package mismatch ({mode}): {input}\n{parser.FiredException}");
    count++;
}
Console.WriteLine($"PASS: {count} package-only consumer cases across Module, Script and Expression goals.");
