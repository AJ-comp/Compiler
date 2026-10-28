using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new JavaScriptExpressionGrammar();
var parser = new LALRParser(language.Grammar, false);
if (parser.HasConflicts) throw new Exception(parser.ConflictReport);
var cases = new[] {
    ("`hello ${name}`", true), ("`outer ${`inner ${value}`}`", true),
    ("`${{a: /}/.test(value)}.a} tail`", true), ("tag`\\u{110000} ${value}`", true),
    ("`${value / divisor}` / other", true), ("(obj?.tag)`text`", true),
    ("a / b + 1", true), ("/ab/g.test(value)", true),
    ("`\\u{110000}`", false), ("tag`outer ${`\\xZ`}`", false),
    ("object?.tag`text`", false), ("`${}`", false), ("`unclosed ${value", false),
    ("tag`text` = value", false), ("`${eval = value}`", false)
};
foreach (var (input, expected) in cases)
{
    var parsed = parser.Parsing(language.CreateTokenSource(input));
    bool actual = parsed.Success && language.Validate(parsed.ToParseTree).Count == 0;
    if (actual != expected) throw new Exception("Package consumer mismatch: " + input + " " + parser.FiredException);
}
Console.WriteLine($"PASS: {cases.Length} NuGet consumer cases; four goals, template helpers and embedded EBNF loaded from the package; zero grammar conflicts.");
