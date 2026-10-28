using System.Reflection;
using System.Runtime.Loader;
using System.Text.Encodings.Web;
using System.Text.Json;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;

string bin = args[0];
AssemblyLoadContext.Default.Resolving += (_, name) => {
    string path = Path.Combine(bin, name.Name + ".dll");
    return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
};
var tests = AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(bin, "Parse.FrontEnd.Parsers.Tests.dll"));
var fixture = tests.GetType("Janglim.FrontEnd.Parsers.Tests.LalrLookaheadReferenceTests")!;
var native = fixture.GetNestedType("NativeGrammar", BindingFlags.NonPublic)!;
Dictionary<string, string[][]> Spec(params (string Name, string Body)[] rules) => rules.ToDictionary(r => r.Name, r => r.Body.Split('|').Select(body => body.Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToArray());
var cases = new[] {
    (Name: "original", Rules: Spec(("S", "A U | z"), ("A", "B x"), ("B", "a"), ("U", "U")), Inputs: new[] { "z", "ax" }),
    (Name: "overlapping-valid-path", Rules: Spec(("S", "A U | C x"), ("A", "B x"), ("B", "a"), ("C", "a"), ("U", "U")), Inputs: new[] { "ax" }),
    (Name: "pruned-valid-path", Rules: Spec(("S", "C x"), ("C", "a")), Inputs: new[] { "ax" })
};
var records = new List<object>();
foreach (var sample in cases) foreach (bool reverse in new[] { false, true }) {
    var grammar = (Grammar)Activator.CreateInstance(native, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new object[] { sample.Rules, reverse }, null)!;
    var parser = new LALRParser(grammar, false);
    var completed = parser.Canonical.IndexStateDic.SelectMany(s => s.Value.Where(i => i.IsReachedHandle).Select(i => new {
        State = s.Key, Item = i.ToString(), LookAhead = i.LookAhead.Select(t => t is EndMarker ? "EOF" : t.Value).Order().ToArray()
    })).ToArray();
    var outcomes = new List<object>();
    foreach (string input in sample.Inputs) {
        var lexer = new Lexer();
        foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
        var result = parser.Parsing(lexer.Lexing(input).TokensForParsing);
        outcomes.Add(new { Input = input, Success = result.Success, Exception = parser.FiredException?.ToString(), Errors = result.AllErrors.Select(e => e.ToString()).ToArray() });
    }
    records.Add(new { sample.Name, Reverse = reverse, parser.HasConflicts, ConflictCount = parser.Conflicts.Count, parser.ConflictReport, Completed = completed, Outcomes = outcomes });
}
Console.WriteLine(JsonSerializer.Serialize(records, new JsonSerializerOptions { WriteIndented = true, Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping }));
