using System.Reflection;
using System.Text.Json;
using Janglim;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;

var grammar = new SmallGrammar();
var parser = new LALRParser(grammar, false);
var before = parser.Canonical.LookAheadTable.ToDictionary(p => p.Key, p => new TerminalSet(p.Value));
var ff = new FirstFollowAnalyzer();ff.CalculateAllFollow(grammar.NonTerminalMultiples);
var predEx = typeof(CanonicalRelation).GetMethod("PredEx", BindingFlags.Instance | BindingFlags.NonPublic)!;
var accept = (NonTerminal)typeof(CanonicalRelation).GetField("virtualStartSymbol", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(parser.Canonical)!;
var mark = typeof(LRItem).GetField("markIndex", BindingFlags.Instance | BindingFlags.NonPublic)!;
var plan = WorklistCalculation.Calculate(parser.Canonical, ff, accept,
    (state, item) => (IEnumerable<CanonicalState>)predEx.Invoke(parser.Canonical, new object[] { state, item })!);
if (before.Count != parser.Canonical.LookAheadTable.Count ||
    before.Any(p => !p.Value.SetEquals(parser.Canonical.LookAheadTable[p.Key])))
    throw new Exception("The original lookahead table was changed.");
string Sym(Symbol s) => s is EndMarker ? "$" : s is Epsilon ? "EPS" : s is Terminal t ? "T:" + t.Caption + ":" + t.Value : "N:" + ((NonTerminal)s).Name;
string Key(NonTerminalSingle r) => r.Name + "->" + string.Join(" ", r.Where(s => s is not Epsilon).Select(Sym));
var rules = parser.Canonical.IndexStateDic.SelectMany(s => s.Value).Select(i => i.SingleNT)
    .GroupBy(Key).Select(g => g.First()).OrderBy(Key).ToArray();
var ids = rules.Select((rule, id) => (rule, id)).ToDictionary(p => Key(p.rule), p => p.id);
var lexer = new Lexer();foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
var actual = parser.Parsing(lexer.Lexing("a").TokensForParsing);
var dump = new {
    designModelOnly = true, originalTableUnchanged = true, originalStillRejectsA = !actual.Success,
    nodes = plan.Table.Count, edges = plan.EdgeCount, dequeued = plan.DequeuedCount,
    items = plan.Table.Select(p => new { state = p.Key.State, rule = ids[Key(p.Key.Item.SingleNT)], dot = System.Convert.ToInt32(mark.GetValue(p.Key.Item)), lookahead = p.Value.Select(Sym).Order().ToArray() }).OrderBy(p => p.state).ThenBy(p => p.rule).ThenBy(p => p.dot).ToArray(),
    updates = plan.Updates,
};
Console.WriteLine(JsonSerializer.Serialize(dump, new JsonSerializerOptions { WriteIndented = true }));

sealed class SmallGrammar : Grammar {
    public override NonTerminal EbnfRoot => StartSymbol;
    public SmallGrammar() {
        var ns = new[] { "S", "C", "R", "G" }.ToDictionary(s => s, s => new NonTerminal(s, s == "S"));
        var ts = new[] { "a", "b", "e" }.ToDictionary(s => s, s => new Terminal(TokenType.Operator, s));
        foreach (var n in ns.Values) { keyManager.AllocateUniqueKey(n); NonTerminalMultiples.Add(n); }
        foreach (var t in ts.Values) { keyManager.AllocateUniqueKey(t); TerminalSet.Add(t); }
        SetStartSymbol(ns["S"]);
        ns["S"].Add(ns["G"], ns["C"]);ns["C"].Add(ns["R"]);ns["C"].Add(ts["e"], ns["S"]);
        ns["R"].Add(ts["a"]);ns["R"].Add(ts["b"], ns["S"]);ns["G"].Add(new Epsilon());
    }
}
