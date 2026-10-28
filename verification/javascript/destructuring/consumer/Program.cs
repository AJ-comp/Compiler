using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new JavaScriptExpressionGrammar();
var parser = new LALRParser(language.Grammar, false);
if (parser.HasConflicts) throw new Exception(parser.ConflictReport);
if (!language.GrammarText.Contains("CoverInitializedName"))
    throw new Exception("Updated embedded grammar was not packaged.");
var cases = new[] {
    ("[a, b] = values", true),
    ("({name: local = defaultName, ...rest} = source)", true),
    ("[...[a = 1]] = values", true),
    ("({__proto__: a, __proto__: b} = source)", true),
    ("({a = `outer${`inner${value}`}`} = source)", true),
    ("({[/a/.source]: a = value / divisor} = source)", true),
    ("({a = ({b = 1} = other)} = source)", true),
    ("[(a)] = values", true),
    ("[...{length}] = values", true),
    ("[eval.value, arguments[index]] = values", true),
    ("({...rest,} = source)", false),
    ("[...rest = fallback] = values", false),
    ("({...{a}} = source)", false),
    ("[a] += values", false),
    ("({a = {b = 1}} = source)", false),
    ("({a = 1})", false),
    ("([a]) = values", false),
    ("[arguments] = values", false),
    ("[a = {__proto__: x, __proto__: y}] = values", false),
    ("({[`\\uZ`]: target} = source)", false)
};
foreach (var (input, expected) in cases)
{
    var parsed = parser.Parsing(language.CreateTokenSource(input));
    bool actual = parsed.Success && language.Validate(parsed.ToParseTree).Count == 0;
    if (actual != expected) throw new Exception("Package consumer mismatch: " + input + " " + parser.FiredException);
}
Console.WriteLine($"PASS: {cases.Length} NuGet consumer cases; updated embedded EBNF and destructuring validator loaded; zero grammar conflicts.");
