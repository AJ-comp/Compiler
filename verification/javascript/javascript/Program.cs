using System.Text.Json;
using System.Text.RegularExpressions;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;

var language = new JavaScriptExpressionGrammar();
var parser = new LALRParser(language.Grammar, false);
var cases = JsonSerializer.Deserialize<OracleCase[]>(File.ReadAllText(args[0]), new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
var differences = new List<object>();
var expectedDifferences = new List<object>();
foreach (var test in cases)
{
    var result = parser.Parsing(language.CreateTokenSource(test.Input));
    bool accepted = result.Success && language.Validate(result.ToParseTree).Count == 0;
    if (accepted != test.Expected)
    {
        string known = test.Input.StartsWith("[] = ") || test.Input.StartsWith("{} = ")
            ? "Destructuring assignment belongs to the documented next grammar stage."
            : test.Input.StartsWith("f() = ") || test.Input.StartsWith("f() += ") ||
              test.Input == "++f()" || test.Input == "--f()" || test.Input == "f()++" || test.Input == "f()--"
            ? "ECMAScript 2026 8.6.4: strict CallExpression targets are invalid; the tested Node 24.12 parser accepts these cases."
            : null;
        var difference = new { test.Input, test.Expected, Actual = accepted, Reason = known,
            Exception = parser.FiredException?.ToString() };
        if (known == null) differences.Add(difference);
        else expectedDifferences.Add(difference);
    }
}
object unicodeCheck = null;
if (args.Length > 1)
{
    var starts = new bool[0x110000];
    var parts = new bool[0x110000];
    var points = new HashSet<int>();
    foreach (var line in File.ReadLines(args[1]))
    {
        var pieces = line.Split('#')[0].Split(';', StringSplitOptions.TrimEntries);
        if (pieces.Length < 2 || pieces[1] is not ("ID_Start" or "ID_Continue")) continue;
        var range = pieces[0].Split("..");
        int lo = Convert.ToInt32(range[0], 16), hi = Convert.ToInt32(range[range.Length - 1], 16);
        var target = pieces[1] == "ID_Start" ? starts : parts;
        for (int cp = lo; cp <= hi; cp++) target[cp] = true;
        foreach (int cp in new[] { lo - 1, lo, hi, hi + 1 }) if (cp >= 0 && cp < starts.Length) points.Add(cp);
    }
    starts['$'] = starts['_'] = parts['$'] = true;
    points.Add('$'); points.Add('_'); points.Add(0xFFFF); points.Add(0x10000); points.Add(0x10FFFF);
    var random = new Random(2026);
    for (int i = 0; i < 10000; i++) points.Add(random.Next(starts.Length));
    var pattern = language.Grammar.TerminalSet.Single(t => t.Caption == "IdentifierName").RegexExpression;
    var identifier = new Regex("\\A(?:" + pattern + ")\\z");
    var failed = new List<string>();
    int checkedPoints = 0;
    foreach (int cp in points.Where(cp => cp < 0xD800 || cp > 0xDFFF))
    {
        string value = char.ConvertFromUtf32(cp);
        if (identifier.IsMatch(value) != starts[cp] || identifier.IsMatch("a" + value) != parts[cp]) failed.Add(cp.ToString("X"));
        checkedPoints++;
    }
    unicodeCheck = new { CheckedCodePoints = checkedPoints, Checks = checkedPoints * 2, Failed = failed };
}
Console.WriteLine(JsonSerializer.Serialize(new { Cases = cases.Length, Differences = differences,
    MatchedCases = cases.Length - differences.Count - expectedDifferences.Count, ExpectedDifferences = expectedDifferences,
    Unicode = unicodeCheck,
    States = parser.Canonical.IndexStateDic.Count, Conflicts = parser.Conflicts.Count,
    Productions = language.Grammar.NonTerminalSingles.Count, GrammarCharacters = language.GrammarText.Length }));
record OracleCase(string Input, bool Expected);
