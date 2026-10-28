using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Grammars.JavaScript;
var language=new TypeScriptGrammar(mode:JavaScriptMode.Strict);
foreach(var rule in language.Grammar.NonTerminalSingles) {
 if(!int.TryParse(rule.GetHashCode().ToString()+rule.Count.ToString(),out _)) Console.WriteLine(rule.Name+" "+rule.Count+" "+rule.GetHashCode()+" "+rule);
}
