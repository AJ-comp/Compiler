using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;

int passed = 0;
void Check(LALRParser parser, Func<string, ITokenSource> source,
    Func<ParseTreeSymbol, IReadOnlyList<JavaScriptDiagnostic>> validate,
    string input, bool expected)
{
    var parsed = parser.Parsing(source(input));
    if (!parsed.Success) throw new Exception("Boundary parsing failed: " + input + "\n" + parser.FiredException);
    var diagnostics = validate(parsed.ToParseTree);
    if (expected != (diagnostics.Count == 0))
        throw new Exception("Incorrect body decision: " + input + "\n" + string.Join("\n", diagnostics.Select(d => d.Message)));
    foreach (var diagnostic in diagnostics)
    {
        if (diagnostic.Code != "regexp_syntax" || diagnostic.SourceOffset < 0 ||
            diagnostic.Length < 0 || diagnostic.SourceOffset + diagnostic.Length > input.Length)
            throw new Exception("Incorrect public diagnostic contract: " + input);
        _ = input.Substring(diagnostic.SourceOffset, diagnostic.Length);
    }
    passed++;
}

var expression = new JavaScriptExpressionGrammar();
var expressionParser = new LALRParser(expression.Grammar);
var patterns = new (string input, bool expected)[]
{
    (@"/abc+/gi", true), (@"/[z-a]/u", false), (@"/(/", false),
    (@"/a++/", false), (@"/a{4,2}/u", false), (@"/a{2,4}?/v", true),
    (@"/\8/", true), (@"/\8/u", false), (@"/[\d-z]/", true), (@"/[\d-z]/u", false),
    (@"/\1(a)/u", true), (@"/(a)\2/u", false),
    (@"/(?<a>x)\k<a>/u", true), (@"/(?<a>x)(?<a>y)/u", false),
    (@"/(?<a>x)|(?<a>y)/u", true), (@"/(?<a>(?<a>x)|y)/u", false),
    (@"/(?=a)*/", true), (@"/(?=a)*/u", false), (@"/(?<=a)*/", false),
    (@"/(?:(?=a))*/u", true), (@"/(?im-s:abc)/u", true), (@"/(?i-i:abc)/u", false),
    (@"/\p{Script=Latin}/u", true), (@"/\p{Script=Tolong_Siki}/u", true),
    (@"/\p{Script=Hrkt}/u", true), (@"/\p{WSpace}/u", false), (@"/\p{space}/u", true),
    (@"/[\q{ab|cd}]/v", true), (@"/[^\q{ab|cd}]/v", false), (@"/[^\q{a|b}]/v", true),
    (@"/[[a-z]--[aeiou]]/v", true), (@"/[a&&b--c]/v", false),
    (@"/\p{RGI_Emoji}/v", true), (@"/\P{RGI_Emoji}/v", false),
    (@"/[()] /v", false), (@"/[\(\)]/v", true),
    (@"value/+other/divisor", true), (@"value/+other/ divisor", true),
    (@"[/(/, /[a-z]/v]", false), (@"[/a/, /b/u]", true),
};
foreach (var example in patterns)
    Check(expressionParser, expression.CreateTokenSource, expression.Validate, example.input, example.expected);

var script = new JavaScriptScriptGrammar();
var scriptParser = new LALRParser(script.Grammar);
foreach (var example in new (string input, bool expected)[]
{
    (@"const pattern = /[a-z]+/u;", true), (@"const pattern = /[z-a]/u;", false),
    (@"function test(value) { return /(?<a>x)\k<a>/u.test(value); }", true),
    (@"function test(value) { return /(?<a>x)\k<b>/u.test(value); }", false),
    (@"class A { field = /[\q{ab|cd}]/v; method() { return /a+/; } }", true),
    (@"class A { field = /[^\q{ab}]/v; }", false),
    (@"if (ready) /[a-z]/u.test(value);", true),
    (@"if (ready) /[z-a]/u.test(value);", false),
}) Check(scriptParser, script.CreateTokenSource, script.Validate, example.input, example.expected);

var module = new JavaScriptModuleGrammar();
var moduleParser = new LALRParser(module.Grammar);
foreach (var example in new (string input, bool expected)[]
{
    (@"export const pattern = /\p{Script=Latin}/u;", true),
    (@"export const pattern = /\p{Unknown}/u;", false),
    (@"import config from 'config'; export default /[[a-z]--[aeiou]]/v;", true),
    (@"export default /a{5,2}/;", false),
    (@"export default /\8/;", true),
    (@"export const pattern = import.meta ? /a/ : /b/u;", true),
}) Check(moduleParser, module.CreateTokenSource, module.Validate, example.input, example.expected);

foreach (string input in new[] { "/a/gg", "/a/uv", "/a/z", "/a\nb/u" })
{
    var parsed = expressionParser.Parsing(expression.CreateTokenSource(input));
    if (parsed.Success) throw new Exception("Invalid literal envelope accepted: " + input);
    passed++;
}
Console.WriteLine($"Packaged public APIs: {passed} checks passed; Expression, Script, Module and original envelope diagnostics.");
