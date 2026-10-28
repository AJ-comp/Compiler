using System.Reflection;
using System.Text.Json;
using Janglim;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;

// Diagnostic models only. No writes to parser.Canonical or product source.
var original = "S : {Always()}? C ; C : P {Always()}? | P {Never()}? 'e' 'x' '?' S ':' S ; P : R {Always()}? ; R : id {Always()}? | id {Never()}? 'i' S ; id := \"[a-z]+\" ;";
var options = new EbnfReaderOptions().RegisterPredicate("Always", _ => true).RegisterPredicate("Never", _ => false);
var reports = new List<object>();
foreach (var profile in new[] { (Name: "tiny", Grammar: (Grammar)new SharedTinyGrammar()), (Name: "original", Grammar: (Grammar)EbnfGrammarReader.Read(original, options).Grammar), (Name: "zero-conflict-ordinary-bnf", Grammar: (Grammar)new SharedFullGrammar()) })
{
    var parser = new LALRParser(profile.Grammar, false);
    var relation = parser.Canonical;
    var initial = relation.LookAheadTable.ToDictionary(p => p.Key, p => new TerminalSet(p.Value));
    var ff = new FirstFollowAnalyzer();ff.CalculateAllFollow(profile.Grammar.NonTerminalMultiples);
    var originMethod = typeof(CanonicalRelation).GetMethod("PredEx", BindingFlags.NonPublic | BindingFlags.Instance)!;
    var root = (NonTerminal)typeof(CanonicalRelation).GetField("virtualStartSymbol", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(relation)!;
    var reduceMethod = typeof(CanonicalRelation).GetMethod("GetStateSetHasReachedHandle", BindingFlags.NonPublic | BindingFlags.Instance)!;
    var states = ((IEnumerable<CanonicalState>)reduceMethod.Invoke(relation, null)!).ToArray();
    IEnumerable<CanonicalState> Origins(CanonicalState s, LRItem i) => (IEnumerable<CanonicalState>)originMethod.Invoke(relation, new object[] { s, i })!;
    var reference = WorklistCalculation.Calculate(relation, ff, root, Origins).Table;
    foreach (bool reverse in new[] { false, true })
    {
        var cache = new Dictionary<(int, LRItem), TerminalSet>();
        var trace = new List<string>();
        TerminalSet Shared(CanonicalState state, LRItem item)
        {
            var key = (state.StateNumber, item);
            if (cache.TryGetValue(key, out var previous)) return previous;
            var result = new TerminalSet();
            cache.Add(key, result); // The sole proposed change: cache and result share one set.
            var lhs = item.SingleNT.ToNonTerminal();
            if (lhs == root) { result.Add(new EndMarker()); return result; }
            foreach (var origin in Origins(state, item))
                foreach (var parent in origin)
                {
                    if (parent.MarkSymbol != lhs) continue;
                    var alpha = ff.First(parent.SymbolListAfterMarkSymbol);
                    if (alpha.Contains(new Epsilon()) || alpha.Count == 0)
                    {
                        var inherited = cache.TryGetValue((origin.StateNumber, parent), out var exists) ? exists : Shared(origin, parent);
                        int before = result.Count;
                        result.UnionWith(alpha.RingSum(inherited));
                        if (before == result.Count && inherited.Count == 0)
                            trace.Add($"I{state.StateNumber} {item} copied empty I{origin.StateNumber} {parent}");
                    }
                    else result.UnionWith(alpha);
                }
            return result;
        }
        foreach (var state in reverse ? states.Reverse() : states)
            foreach (var item in state.ReachedHandleItem) Shared(state, item);
        var differences = cache.Where(p => !p.Value.SetEquals(reference[p.Key])).Select(p => new { state = p.Key.Item1, item = p.Key.Item2.ToString(), actual = p.Value.Select(t => t.ToString()).Order().ToArray(), expected = reference[p.Key].Select(t => t.ToString()).Order().ToArray() }).ToArray();
        reports.Add(new { profile = profile.Name, conflicts = parser.Conflicts.Count, order = reverse ? "reverse outer state order" : "current outer state order", differences, emptyCopies = trace });
    }
    if (initial.Count != relation.LookAheadTable.Count || initial.Any(p => !p.Value.SetEquals(relation.LookAheadTable[p.Key])))
        throw new Exception("The original table changed.");
}
Console.WriteLine(JsonSerializer.Serialize(reports, new JsonSerializerOptions { WriteIndented = true }));

sealed class SharedFullGrammar : Grammar {
    public override NonTerminal EbnfRoot => StartSymbol;
    public SharedFullGrammar() {
        var ns = new[] { "S", "C", "P", "R", "G1", "G2", "G3", "G4", "G5", "G6" }.ToDictionary(s => s, s => new NonTerminal(s, s == "S"));
        var ts = new[] { "a", "b", "e", "x", "?", ":", "i" }.ToDictionary(s => s, s => new Terminal(TokenType.Operator, s));
        foreach (var n in ns.Values) { keyManager.AllocateUniqueKey(n); NonTerminalMultiples.Add(n); }
        foreach (var t in ts.Values) { keyManager.AllocateUniqueKey(t); TerminalSet.Add(t); }
        SetStartSymbol(ns["S"]);
        ns["S"].Add(ns["G1"], ns["C"]);
        ns["C"].Add(ns["P"], ns["G2"]);ns["C"].Add(ns["G3"], ts["e"], ts["x"], ts["?"], ns["S"], ts[":"], ns["S"]);
        ns["P"].Add(ns["R"], ns["G4"]);
        ns["R"].Add(ts["a"], ns["G5"]);ns["R"].Add(ts["b"], ns["G6"], ts["i"], ns["S"]);
        for (int i = 1; i <= 6; i++) ns["G" + i].Add(new Epsilon());
    }
}

sealed class SharedTinyGrammar : Grammar {
    public override NonTerminal EbnfRoot => StartSymbol;
    public SharedTinyGrammar() {
        var ns = new[] { "S", "C", "R", "G" }.ToDictionary(s => s, s => new NonTerminal(s, s == "S"));
        var ts = new[] { "a", "b", "e" }.ToDictionary(s => s, s => new Terminal(TokenType.Operator, s));
        foreach (var n in ns.Values) { keyManager.AllocateUniqueKey(n); NonTerminalMultiples.Add(n); }
        foreach (var t in ts.Values) { keyManager.AllocateUniqueKey(t); TerminalSet.Add(t); }
        SetStartSymbol(ns["S"]);
        ns["S"].Add(ns["G"], ns["C"]);ns["C"].Add(ns["R"]);ns["C"].Add(ts["e"], ns["S"]);
        ns["R"].Add(ts["a"]);ns["R"].Add(ts["b"], ns["S"]);ns["G"].Add(new Epsilon());
    }
}
