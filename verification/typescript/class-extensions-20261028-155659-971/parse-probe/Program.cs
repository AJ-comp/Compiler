using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.RegularGrammar;

var language = new TypeScriptGrammar(TypeScriptInputKind.Script, JavaScriptMode.Strict);
var parser = new LALRParser(language.Grammar, true);
foreach (var input in args)
{
    Console.WriteLine("INPUT " + input);
    var parsed = parser.Parsing(language.CreateTokenSource(input));
    Console.WriteLine("PARSED " + parsed.Success + " EXCEPTION " + parser.FiredException);
    if (parsed.Success && input.Contains(','))
    {
        var nodes = new Stack<Janglim.FrontEnd.ParseTree.ParseTreeSymbol>();
        nodes.Push(parsed.ToParseTree);
        while (nodes.Count > 0)
        {
            if (nodes.Pop() is not Janglim.FrontEnd.ParseTree.ParseTreeNonTerminal node) continue;
            if (node.SignPost.Name.StartsWith("FormalParameters"))
            {
                Console.WriteLine("PARAMETERS parent=" + (node.Parent as Janglim.FrontEnd.ParseTree.ParseTreeNonTerminal)?.SignPost.Name);
                foreach (var child in node.Items) Console.WriteLine("  CHILD " + child.GetType().Name + " " + (child as Janglim.FrontEnd.ParseTree.ParseTreeNonTerminal)?.SignPost.Name + " TOKENS=" + string.Concat(child.AllTokens.Select(t => t.Input)));
            }
            foreach (var child in node.Items) nodes.Push(child);
        }
    }
    if (parsed.Success) foreach (var diagnostic in language.Validate(parsed.ToParseTree)) Console.WriteLine("DIAGNOSTIC " + diagnostic.Code + " " + diagnostic.Message);
    var table = (LRParsingTable)parser.ParsingTable;
    foreach (var unit in parsed.SelectMany(block => block.Units))
    {
        var token = unit.InputValue;
        int state = unit.BeforeStack.Stack.OfType<int>().FirstOrDefault();
        Console.WriteLine("I" + state + " TOKEN=" + token?.Input + " KIND=" + token?.Kind.Caption + " ACTION=" + unit.Action + " ERROR=" + unit.ErrorMessage);
        if (token != null && table[state].MatchedValueSet.TryGetValue(token.Kind, out var cell))
            foreach (var action in cell) Console.WriteLine("  CANDIDATE " + action.Direction + " " + action.Dest + (action.Dest is NonTerminalSingle production ? " PRED=" + production.Predicate?.Name : ""));
        if (unit.IsError || token?.Input == "!" || token?.Input == "readonly" || token?.Input == "[")
            foreach (var item in parser.Canonical.IndexStateDic[state]) Console.WriteLine("  ITEM " + item);
    }
}
