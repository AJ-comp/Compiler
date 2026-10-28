using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using Janglim.FrontEnd.ParseTree;
using System.Text.Json;
var module = new JavaScriptModuleGrammar();
var script = new JavaScriptScriptGrammar();
var expression = new JavaScriptExpressionGrammar();
var moduleParser = new LALRParser(module.Grammar, false);
var scriptParser = new LALRParser(script.Grammar, false);
var expressionParser = new LALRParser(expression.Grammar, false);
using var data = JsonDocument.Parse(File.ReadAllText(args[0]));
foreach(var item in data.RootElement.EnumerateArray())
{
    string code = item.GetProperty("source").GetString();
    string mode = item.GetProperty("mode").GetString();
    var parser = mode == "module" ? moduleParser : mode == "script" ? scriptParser : expressionParser;
    ITokenSource source = mode == "module" ? module.CreateTokenSource(code) : mode == "script" ? script.CreateTokenSource(code) : expression.CreateTokenSource(code);
    var result = parser.Parsing(source);
    string[] errors = result.Success ? JavaScriptExpressionValidation.Validate(result.ToParseTree).Select(x=>x.Code).ToArray() : Array.Empty<string>();
    Console.WriteLine(JsonSerializer.Serialize(new {source=code, mode, valid=result.Success && errors.Length==0, errors,error=parser.FiredException?.Message}));
}
