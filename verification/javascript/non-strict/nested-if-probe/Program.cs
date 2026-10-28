using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.RegularGrammar;

var language = new JavaScriptScriptGrammar(JavaScriptMode.NonStrict, true);
if (args.Contains("flat"))
{
    foreach (var rule in language.Grammar.NonTerminalMultiples.Where(n=>n.Name.StartsWith("IfStatement")).ToArray())
    {
        string suffix=rule.Name["IfStatement".Length..];
        var prefix=rule[0].Take(4).ToArray();
        var statement=language.Grammar.NonTerminalMultiples.Single(n=>n.Name=="Statement"+suffix);
        var function=language.Grammar.NonTerminalMultiples.Single(n=>n.Name=="LegacyFunctionDeclaration"+suffix);
        var otherwise=language.Grammar.TerminalSet.Single(t=>t.Value=="else");
        rule.Clear();
        foreach(var first in new[]{statement,function})
        {
            rule.Add(prefix.Append(first).ToArray());
            foreach(var second in new[]{statement,function})
                rule.Add(prefix.Append(first).Append(otherwise).Append(second).ToArray());
        }
    }
}
var parser = new LALRParser(language.Grammar, true);
foreach (string input in new[] {
    "if (first) if (second) function f() {} else function g() {} else function h() {}",
    "if (first) if (second) function f() {} else function g() {}",
    "if (first) if (second) x(); else y(); else z();",
    "if (first) {if (second) function f() {} else function g() {}} else function h() {}",
    "let eval = 1; const arguments = 2;",
    "for (let arguments of values) { eval = arguments; }",
    "let [eval, arguments] = value;"
}.Where(input=>!args.Contains("let") || input.StartsWith("let") || input.StartsWith("for")))
{
    Console.WriteLine("INPUT " + input);
    var parsed = parser.Parsing(language.CreateTokenSource(input));
    Console.WriteLine("SUCCESS " + parsed.Success + " EXCEPTION " + parser.FiredException);
    foreach (var error in parsed.AllErrors) Console.WriteLine("ERROR " + error);
    if (parsed.Success) Console.WriteLine("DIAGNOSTICS " + string.Join(";", language.Validate(parsed.ToParseTree).Select(d=>d.Code+":"+d.Message)));
    foreach (var step in parsed.Trace.Steps.TakeLast(35))
    {
        int state=step.StackBefore.LastOrDefault()?.State ?? -1;
        Console.WriteLine($"STEP {step.Index} STATE {state} LOOK {step.Lookahead?.Input} ACT {step.Action} ERR {step.ErrorMessage}");
        if (state>=0 && (step.Lookahead?.Input=="else" || step.IsError))
        {
            var row=((LRParsingTable)parser.ParsingTable)[state];
            foreach(var item in row.MatchedValueSet.Where(p=>p.Key.ToString()=="else" || p.Key.ToString()==step.Lookahead?.Kind.Caption))
                Console.WriteLine("ELSE-CELL " + string.Join(";",item.Value.Select(a=>a.Direction+":"+a.Dest)));
        }
    }
}
