using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new JavaScriptExpressionGrammar();
var parser = new LALRParser(language.Grammar);
var cases = new[] {
    ("$result = /ab/g.test(name$)", true), ("a / b + c ** 2", true),
    ("({name: '한글', values: [1, 2n, ...items]})", true), ("obj?.value ?? fallback", true),
    ("(a + b) = 1", false), ("-2 ** 2", false), ("a ?? b || c", false), ("'bad\\8'", false)
};
foreach (var (input, expected) in cases)
{
    var parsed = parser.Parsing(language.CreateTokenSource(input));
    bool actual = parsed.Success && language.Validate(parsed.ToParseTree).Count == 0;
    if (actual != expected) throw new Exception("Package consumer mismatch: " + input);
}
Console.WriteLine("NuGet consumer: " + cases.Length + " cases passed; EBNF resource and Unicode rules loaded from the package.");
