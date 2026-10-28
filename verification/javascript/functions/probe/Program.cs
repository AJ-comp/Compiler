using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
var language = new JavaScriptScriptGrammar();
var parser = new LALRParser(language.Grammar, args.Length == 0 || args[0] != "--batch");
if(args.Length==2 && args[0]=="--batch")
{
    var inputs=System.Text.Json.JsonSerializer.Deserialize<string[]>(File.ReadAllText(args[1]));
    foreach(var code in inputs)
    {
        var result=parser.Parsing(language.CreateTokenSource(code));
        var errors=result.Success? language.Validate(result.ToParseTree).Select(d=>d.Code).ToArray():Array.Empty<string>();
        Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new {input=code,success=result.Success&&errors.Length==0,errors,error=parser.FiredException?.Message}));
    }
    return;
}
Console.WriteLine(parser.ConflictReport);
foreach (var input in args.Length == 0 ? new[] { "a++;", "function f(a=0){'use strict'; let a;}", "{ function f(){} function f(){} }", "switch(v){case 0:let a;case 1:let a;}" } : args)
{
    Console.WriteLine("INPUT " + input);
    var parsed = parser.Parsing(language.CreateTokenSource(input));
    Console.WriteLine("RESULT " + parsed.Success + " " + parser.FiredException);
    foreach (var block in parsed.TakeLast(3))
        foreach (var unit in block.Units) Console.WriteLine(unit.InputValue + " " + unit.Action + " BEFORE " + string.Join(" ",unit.BeforeStack.Stack.Reverse()));
    if (parsed.Success) Dump(parsed.ToParseTree, 0);
}
void Dump(ParseTreeSymbol symbol,int depth)
{
    if (symbol is ParseTreeNonTerminal node)
    {
        Console.WriteLine(new string(' ',depth) + node.SignPost.Name);
        foreach (var child in node) Dump(child,depth+1);
    }
    else if(symbol is ParseTreeTerminal leaf) Console.WriteLine(new string(' ',depth)+"'"+leaf.Token.Input+"'");
}
