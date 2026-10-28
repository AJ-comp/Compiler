using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.RegularGrammar;

var language = new TypeScriptGrammar(TypeScriptInputKind.Script, JavaScriptMode.Strict);
var parser = new LALRParser(language.Grammar, true);
foreach (var input in args.Length == 0 ? new[] { "let result = value as User;", "value as T + right;", "value! / 2;" } : args)
{
 Console.WriteLine("INPUT " + input);
 var result = parser.Parsing(language.CreateTokenSource(input));
 Console.WriteLine("SUCCESS " + result.Success + " EXCEPTION " + parser.FiredException);
 foreach (var unit in result.SelectMany(block => block.Units))
 {
  var token = unit.InputValue;
  var state = unit.BeforeStack.Stack.OfType<int>().FirstOrDefault();
  Console.WriteLine("I" + state + " TOKEN=" + token?.Input + " KIND=" + token?.Kind.Caption + " ACTION=" + unit.Action + " ERROR=" + unit.ErrorMessage);
  var table = (LRParsingTable)parser.ParsingTable;
  if (token != null && table[state].MatchedValueSet.TryGetValue(token.Kind, out var cell))
   foreach (var action in cell)
    Console.WriteLine("  CANDIDATE " + action.Direction + " " + action.Dest + (action.Dest is NonTerminalSingle production ? " PRED=" + production.Predicate?.Name : ""));
  if (token?.Input == "as" || token?.Input == "satisfies" || unit.IsError)
   foreach (var item in parser.Canonical.IndexStateDic[state]) Console.WriteLine("  ITEM " + item);
 }
}
