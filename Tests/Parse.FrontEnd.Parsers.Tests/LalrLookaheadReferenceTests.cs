using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Grammars.ExampleGrammars;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Compares every completed LR item's exact Reduce/Accept lookaheads with
/// canonical LR(1) closure/goto followed by equal-LR(0)-core merging. Initial
/// and intermediate item sets need not be published. The reference does not
/// call production FIRST, FOLLOW, PredEx, propagation, state construction, or hashes.
/// Nonproductive grammars are rejected before table construction and have separate assertions.
/// </summary>
[Trait("Category", "LalrLookahead")]
[Trait("Feature", "LalrIndependentReference")]
public sealed class LalrLookaheadReferenceTests
{
    private const string End = "$";

    [Theory]
    [InlineData("nullable-prefix-cycle")]
    [InlineData("empty-start")]
    [InlineData("nullable-chain")]
    [InlineData("nullable-left-recursion")]
    [InlineData("late-eof-and-two-delimiters")]
    [InlineData("balanced-nullable-recursion")]
    [InlineData("expression-precedence")]
    [InlineData("nullable-shared-child")]
    [InlineData("lalr-not-slr")]
    [InlineData("lr1-not-lalr")]
    public void Every_completed_LR_item_matches_independent_canonical_LR1_core_merging(string name)
    {
        var spec = Named(name);
        var reference = Reference.Build(Model.FromSpec(spec));
        var parser = new LALRParser(new NativeGrammar(spec), false);

        if (name == "lalr-not-slr")
        {
            Assert.Equal(0, reference.CanonicalConflictCount);
            Assert.Equal(0, reference.MergedConflictCount);
            Assert.Empty(parser.Conflicts);
        }
        if (name == "lr1-not-lalr")
        {
            Assert.Equal(0, reference.CanonicalConflictCount);
            Assert.Equal(2, reference.MergedConflictCount);
            Assert.True(reference.CanonicalStateCount > reference.MergedByCore.Count);
            Assert.NotEmpty(parser.Conflicts);
        }
        AssertMatches(name, parser, reference);
    }

    [Theory]
    [InlineData("unproductive-suffix", "U")]
    [InlineData("unproductive-suffix-with-nullable-prefix", "U")]
    [InlineData("unproductive-empty-language", "S,U")]
    public void Nonproductive_grammars_are_rejected_before_lookahead_table_construction(
        string name, string expectedRules)
    {
        // These grammars must be diagnosed during preparation. They no longer
        // reach the stage where completed LR item lookaheads could be published.
        var error = Assert.Throws<GrammarProductivityException>(() =>
            new LALRParser(new NativeGrammar(Named(name)), false));

        Assert.Equal(expectedRules.Split(','), error.RuleNames);
    }

    [Fact]
    public void Existing_Ex1_has_independently_verified_completed_item_lookaheads()
    {
        var grammar = new Ex1Grammar();
        // Capture the declared/normalized grammar before the parser adds Accept.
        // Only public grammar productions are read; no production algorithm is reused.
        var reference = Reference.Build(Model.FromGrammarShape(grammar));
        var parser = new LALRParser(grammar, false);
        AssertMatches("Ex1Grammar", parser, reference);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Equivalent_nullable_cycle_is_independent_of_symbol_and_alternative_registration_order(bool reverse)
    {
        var spec = Named("late-eof-and-two-delimiters");
        var reference = Reference.Build(Model.FromSpec(spec));
        var parser = new LALRParser(new NativeGrammar(spec, reverse), false);
        AssertMatches("registration-order=" + reverse, parser, reference);
    }

    public static IEnumerable<object[]> SmallGrammarSeeds()
    {
        for (int seed = 0; seed < 64; seed++) yield return new object[] { seed };
    }

    [Theory]
    [MemberData(nameof(SmallGrammarSeeds))]
    public void Every_completed_LR_item_in_small_productive_grammar_matches_the_independent_reference(int seed)
    {
        var spec = RandomSpec(seed);
        var reference = Reference.Build(Model.FromSpec(spec));
        var parser = new LALRParser(new NativeGrammar(spec), false);
        AssertMatches("seed=" + seed + Environment.NewLine + Describe(spec), parser, reference);
    }

    private static void AssertMatches(string label, LALRParser parser, Reference reference)
    {
        Assert.Equal(reference.MergedByCore.Count, parser.Canonical.IndexStateDic.Count);
        var observedCores = new HashSet<string>(StringComparer.Ordinal);
        int compared = 0;
        foreach (var state in parser.Canonical.IndexStateDic)
        {
            var items = state.Value.Select(item =>
            {
                string signature = Signature("N:" + item.SingleNT.Name,
                    item.SingleNT.Where(s => !(s is Epsilon)).Select(SymbolName));
                Assert.True(reference.Model.ProductionIds.TryGetValue(signature, out int production),
                    label + ": unexpected production " + item.SingleNT);
                // Public properties only. PrevSymbolListFrom returns an empty list
                // once the mark reaches the end, so completed items use RHS length.
                // Epsilon's normalized RHS length is zero.
                int dot = item.IsReachedHandle ? reference.Model.Rules[production].Rhs.Length :
                    item.SymbolListBeforeMarkSymbol.Count;
                return (Item: item, Core: new CoreItem(production, dot));
            }).ToArray();
            string core = CoreKey(items.Select(i => i.Core));
            Assert.True(observedCores.Add(core), label + ": duplicate LR(0) state core");
            Assert.True(reference.MergedByCore.TryGetValue(core, out var expectedState),
                label + ": LR(0) state I" + state.Key + " has no independent counterpart");
            foreach (var item in items)
            {
                if (!item.Item.IsReachedHandle)
                    continue;

                Assert.True(parser.Canonical.LookAheadTable.TryGetValue((state.Key, item.Item), out var cached),
                    label + ": no lookahead entry for I" + state.Key + " " + item.Item);
                Assert.DoesNotContain(cached.LookAhead, terminal => terminal is Epsilon);
                var expected = expectedState[item.Core];
                var actual = new HashSet<string>(cached.LookAhead.Select(LookaheadName), StringComparer.Ordinal);
                string location = label + ": I" + state.Key + " " + item.Item;
                Assert.True(expected.SetEquals(actual), location + Environment.NewLine +
                    "Expected: " + string.Join(", ", expected.OrderBy(s => s, StringComparer.Ordinal)) + Environment.NewLine +
                    "Actual: " + string.Join(", ", actual.OrderBy(s => s, StringComparer.Ordinal)));
                Assert.True(cached.LookAhead.SetEquals(item.Item.LookAhead), location + ": cache and linked LR item disagree");
                compared++;
            }
        }
        int expectedCompleted = reference.MergedByCore.Values.Sum(state => state.Keys.Count(item =>
            item.Dot == reference.Model.Rules[item.Production].Rhs.Length));
        Assert.Equal(expectedCompleted, compared);
        Assert.True(compared > 0);
    }

    private static string SymbolName(Symbol symbol)
        => symbol is NonTerminal nt ? "N:" + nt.Name : "T:" + ((Terminal)symbol).Value;

    private static string LookaheadName(Terminal terminal) => terminal is EndMarker ? End : "T:" + terminal.Value;

    private static string Signature(string lhs, IEnumerable<string> rhs) => lhs + "\u001e" + string.Join("\u001f", rhs);

    private static string CoreKey(IEnumerable<CoreItem> items) => string.Join(";",
        items.Distinct().OrderBy(i => i.Production).ThenBy(i => i.Dot).Select(i => i.Production + ":" + i.Dot));

    private static Dictionary<string, string[][]> Named(string name) => name switch
    {
        "nullable-prefix-cycle" => Spec(("S", "G C"), ("C", "R | e S"), ("R", "a | b S"), ("G", "")),
        "empty-start" => Spec(("S", "")),
        "nullable-chain" => Spec(("S", "A B"), ("A", " | a"), ("B", " | b")),
        "nullable-left-recursion" => Spec(("S", "A z"), ("A", "B"), ("B", "A a | ")),
        "late-eof-and-two-delimiters" => Spec(("S", "G C"), ("C", "R | e S : S | f S ; S"),
            ("R", "a | b S"), ("G", "")),
        "balanced-nullable-recursion" => Spec(("S", "( S ) S | ")),
        "expression-precedence" => Spec(("S", "E"), ("E", "E + T | T"), ("T", "T * F | F"), ("F", "( E ) | a")),
        "nullable-shared-child" => Spec(("S", "A a | B b"), ("A", "N"), ("B", "N"), ("N", " | c")),
        "lalr-not-slr" => Spec(("S", "L = R | R"), ("L", "* R | i"), ("R", "L")),
        "lr1-not-lalr" => Spec(("S", "a A d | b A e | a B e | b B d"), ("A", "c"), ("B", "c")),
        "unproductive-suffix" => Spec(("S", "A U | z"), ("A", "B x"), ("B", "a"), ("U", "U")),
        "unproductive-suffix-with-nullable-prefix" => Spec(("S", "G A U | z"), ("G", ""), ("A", "B x"), ("B", "a"), ("U", "U")),
        "unproductive-empty-language" => Spec(("S", "U"), ("U", "U")),
        _ => throw new ArgumentOutOfRangeException(nameof(name))
    };

    private static Dictionary<string, string[][]> Spec(params (string Name, string Alternatives)[] rules)
        => rules.ToDictionary(r => r.Name, r => r.Alternatives.Split('|')
            .Select(body => body.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToArray(), StringComparer.Ordinal);

    private static Dictionary<string, string[][]> RandomSpec(int seed)
    {
        var random = new Random(0x1A1F000 + seed);
        var names = new[] { "S", "A", "B" };
        var result = new Dictionary<string, string[][]>(StringComparer.Ordinal);
        for (int i = 0; i < names.Length; i++)
        {
            // Every nonterminal has a terminal base, so no unproductive rules are
            // introduced. Recursive alternatives consume a terminal per traversal.
            var alternatives = new List<string[]> { new[] { ((char)('a' + i)).ToString() } };
            for (int j = 0, count = random.Next(1, 3); j < count; j++)
            {
                int target = random.Next(names.Length);
                string terminal = ((char)('d' + random.Next(4))).ToString();
                string[] alternative;
                switch (random.Next(4))
                {
                    case 0: alternative = new[] { terminal, names[target] }; break;
                    case 1: alternative = new[] { names[target], terminal }; break;
                    case 2 when i + 1 < names.Length:
                        alternative = new[] { names[random.Next(i + 1, names.Length)] }; break;
                    default: alternative = i == names.Length - 1 ? Array.Empty<string>() : new[] { terminal }; break;
                }
                if (!alternatives.Any(a => a.SequenceEqual(alternative))) alternatives.Add(alternative);
            }
            result.Add(names[i], alternatives.ToArray());
        }
        return result;
    }

    private static string Describe(Dictionary<string, string[][]> rules) => string.Join(Environment.NewLine,
        rules.Select(r => r.Key + " -> " + string.Join(" | ", r.Value.Select(rhs => rhs.Length == 0 ? "epsilon" : string.Join(" ", rhs)))));

    private sealed class NativeGrammar : Grammar
    {
        public override NonTerminal EbnfRoot => StartSymbol;
        internal NativeGrammar(Dictionary<string, string[][]> rules, bool reverse = false)
        {
            var names = reverse ? rules.Keys.Reverse().ToArray() : rules.Keys.ToArray();
            var nts = names.ToDictionary(n => n, n => new NonTerminal(n, n == "S"), StringComparer.Ordinal);
            var terminals = rules.Values.SelectMany(a => a).SelectMany(rhs => rhs).Where(s => !nts.ContainsKey(s))
                .Distinct(StringComparer.Ordinal).ToDictionary(s => s, s => new Terminal(TokenType.Operator, s), StringComparer.Ordinal);
            foreach (var nt in nts.Values) { keyManager.AllocateUniqueKey(nt); NonTerminalMultiples.Add(nt); }
            foreach (var terminal in (reverse ? terminals.Values.Reverse() : terminals.Values))
            { keyManager.AllocateUniqueKey(terminal); TerminalSet.Add(terminal); }
            SetStartSymbol(nts["S"]);
            foreach (string name in names)
                foreach (var rhs in reverse ? rules[name].Reverse() : rules[name])
                    nts[name].Add(rhs.Length == 0 ? new Symbol[] { new Epsilon() } : rhs
                        .Select(s => nts.TryGetValue(s, out var nt) ? (Symbol)nt : terminals[s]).ToArray());
        }
    }

    private readonly record struct CoreItem(int Production, int Dot);
    private readonly record struct Item(int Production, int Dot, string Lookahead);
    private sealed record Rule(int Id, string Lhs, string[] Rhs);

    private sealed class Model
    {
        internal readonly Rule[] Rules;
        internal readonly Dictionary<string, int> ProductionIds;
        internal readonly Dictionary<string, int[]> ByLhs;
        internal Model(string start, IEnumerable<(string Lhs, string[] Rhs)> declared)
        {
            Rules = new[] { (Lhs: "N:Accept", Rhs: new[] { start }) }.Concat(declared)
                .Select((rule, index) => new Rule(index, rule.Lhs, rule.Rhs)).ToArray();
            ProductionIds = Rules.ToDictionary(r => Signature(r.Lhs, r.Rhs), r => r.Id, StringComparer.Ordinal);
            ByLhs = Rules.GroupBy(r => r.Lhs).ToDictionary(g => g.Key, g => g.Select(r => r.Id).ToArray(), StringComparer.Ordinal);
        }
        internal static Model FromSpec(Dictionary<string, string[][]> spec) => new("N:S", spec.SelectMany(rule =>
            rule.Value.Select(rhs => ("N:" + rule.Key, rhs.Select(s => (spec.ContainsKey(s) ? "N:" : "T:") + s).ToArray()))));
        internal static Model FromGrammarShape(Grammar grammar) => new("N:" + grammar.StartSymbol.Name,
            grammar.NonTerminalMultiples.SelectMany(nt => nt.Select(rule =>
                ("N:" + nt.Name, rule.Where(s => !(s is Epsilon)).Select(SymbolName).ToArray()))));
    }

    private sealed class Reference
    {
        internal readonly Model Model;
        internal readonly Dictionary<string, Dictionary<CoreItem, HashSet<string>>> MergedByCore = new(StringComparer.Ordinal);
        internal int CanonicalStateCount { get; private set; }
        internal int CanonicalConflictCount { get; private set; }
        internal int MergedConflictCount { get; private set; }
        private Reference(Model model) { Model = model; }

        internal static Reference Build(Model model)
        {
            var answer = new Reference(model);
            var nullable = new HashSet<string>(StringComparer.Ordinal);
            var first = model.ByLhs.Keys.ToDictionary(s => s, _ => new HashSet<string>(StringComparer.Ordinal), StringComparer.Ordinal);
            foreach (string terminal in model.Rules.SelectMany(r => r.Rhs).Where(s => !model.ByLhs.ContainsKey(s)).Append(End).Distinct())
                first.Add(terminal, new HashSet<string>(StringComparer.Ordinal) { terminal });
            bool changed;
            do
            {
                changed = false;
                foreach (var rule in model.Rules)
                {
                    int before = first[rule.Lhs].Count;
                    bool allNullable = true;
                    foreach (string symbol in rule.Rhs)
                    {
                        first[rule.Lhs].UnionWith(first[symbol]);
                        if (!nullable.Contains(symbol)) { allNullable = false; break; }
                    }
                    if (allNullable && nullable.Add(rule.Lhs)) changed = true;
                    changed |= before != first[rule.Lhs].Count;
                }
            } while (changed);

            HashSet<string> First(IEnumerable<string> sequence)
            {
                var result = new HashSet<string>(StringComparer.Ordinal);
                foreach (string symbol in sequence)
                {
                    result.UnionWith(first[symbol]);
                    if (!nullable.Contains(symbol)) break;
                }
                return result;
            }
            HashSet<Item> Closure(IEnumerable<Item> seed)
            {
                var result = new HashSet<Item>(seed);
                var pending = new Queue<Item>(result);
                while (pending.Count != 0)
                {
                    var item = pending.Dequeue();
                    var rule = model.Rules[item.Production];
                    if (item.Dot == rule.Rhs.Length || !model.ByLhs.TryGetValue(rule.Rhs[item.Dot], out var productions)) continue;
                    foreach (string lookahead in First(rule.Rhs.Skip(item.Dot + 1).Append(item.Lookahead)))
                        foreach (int production in productions)
                        {
                            var next = new Item(production, 0, lookahead);
                            if (result.Add(next)) pending.Enqueue(next);
                        }
                }
                return result;
            }
            string StateKey(IEnumerable<Item> items) => string.Join(";", items.OrderBy(i => i.Production).ThenBy(i => i.Dot)
                .ThenBy(i => i.Lookahead, StringComparer.Ordinal).Select(i => i.Production + ":" + i.Dot + ":" + i.Lookahead.Length + ":" + i.Lookahead));
            var initial = Closure(new[] { new Item(0, 0, End) });
            var states = new List<HashSet<Item>> { initial };
            var stateIds = new Dictionary<string, int>(StringComparer.Ordinal) { [StateKey(initial)] = 0 };
            var pendingStates = new Queue<int>();pendingStates.Enqueue(0);
            var transitions = new Dictionary<(int State, string Symbol), int>();
            while (pendingStates.Count != 0)
            {
                int state = pendingStates.Dequeue();
                var symbols = states[state].Where(i => i.Dot < model.Rules[i.Production].Rhs.Length)
                    .Select(i => model.Rules[i.Production].Rhs[i.Dot]).Distinct().OrderBy(s => s, StringComparer.Ordinal);
                foreach (string symbol in symbols)
                {
                    var target = Closure(states[state].Where(i => i.Dot < model.Rules[i.Production].Rhs.Length &&
                        model.Rules[i.Production].Rhs[i.Dot] == symbol).Select(i => i with { Dot = i.Dot + 1 }));
                    string key = StateKey(target);
                    if (!stateIds.TryGetValue(key, out int id))
                    {
                        id = states.Count;stateIds.Add(key, id);states.Add(target);pendingStates.Enqueue(id);
                        Assert.True(states.Count <= 4096, "The deterministic reference fixture grew unexpectedly large.");
                    }
                    transitions.Add((state, symbol), id);
                }
            }
            answer.CanonicalStateCount = states.Count;

            // Build a separate LR(0) automaton too. With an unproductive suffix,
            // LR(0) closure may contain items that canonical LR(1) never emits:
            // FIRST(beta + lookahead) is empty. Such items need an explicitly
            // empty expected set, rather than a made-up spontaneous lookahead.
            HashSet<CoreItem> Closure0(IEnumerable<CoreItem> seed)
            {
                var result = new HashSet<CoreItem>(seed);
                var pending = new Queue<CoreItem>(result);
                while (pending.Count != 0)
                {
                    var item = pending.Dequeue();var rule = model.Rules[item.Production];
                    if (item.Dot == rule.Rhs.Length || !model.ByLhs.TryGetValue(rule.Rhs[item.Dot], out var productions)) continue;
                    foreach (int production in productions)
                    {
                        var next = new CoreItem(production, 0);
                        if (result.Add(next)) pending.Enqueue(next);
                    }
                }
                return result;
            }
            var initial0 = Closure0(new[] { new CoreItem(0, 0) });
            var states0 = new List<HashSet<CoreItem>> { initial0 };
            var ids0 = new Dictionary<string, int>(StringComparer.Ordinal) { [CoreKey(initial0)] = 0 };
            var pending0 = new Queue<int>();pending0.Enqueue(0);
            var mergedTransitions = new Dictionary<(int State, string Symbol), int>();
            while (pending0.Count != 0)
            {
                int state = pending0.Dequeue();
                foreach (string symbol in states0[state].Where(i => i.Dot < model.Rules[i.Production].Rhs.Length)
                    .Select(i => model.Rules[i.Production].Rhs[i.Dot]).Distinct().OrderBy(s => s, StringComparer.Ordinal))
                {
                    var target = Closure0(states0[state].Where(i => i.Dot < model.Rules[i.Production].Rhs.Length &&
                        model.Rules[i.Production].Rhs[i.Dot] == symbol).Select(i => i with { Dot = i.Dot + 1 }));
                    string core = CoreKey(target);
                    if (!ids0.TryGetValue(core, out int id))
                    { id = states0.Count;ids0.Add(core, id);states0.Add(target);pending0.Enqueue(id); }
                    mergedTransitions.Add((state, symbol), id);
                }
            }
            var mergedItems = states0.Select(state => state.ToDictionary(item => item,
                _ => new HashSet<string>(StringComparer.Ordinal))).ToList();
            for (int state = 0; state < states0.Count; state++) answer.MergedByCore.Add(CoreKey(states0[state]), mergedItems[state]);

            // Map each canonical LR(1) state to the independent LR(0) state
            // reached by the same symbol path, then merge its lookaheads there.
            // For productive grammars this is ordinary equal-core merging; the
            // path mapping additionally preserves LR(0)-only empty-LA items.
            var stateToMerged = Enumerable.Repeat(-1, states.Count).ToArray();stateToMerged[0] = 0;
            for (int state = 0; state < states.Count; state++)
            {
                Assert.True(stateToMerged[state] >= 0);
                foreach (var transition in transitions.Where(t => t.Key.State == state))
                {
                    int target = mergedTransitions[(stateToMerged[state], transition.Key.Symbol)];
                    if (stateToMerged[transition.Value] >= 0) Assert.Equal(stateToMerged[transition.Value], target);
                    else stateToMerged[transition.Value] = target;
                }
                foreach (var item in states[state])
                {
                    var key = new CoreItem(item.Production, item.Dot);
                    Assert.True(mergedItems[stateToMerged[state]].ContainsKey(key));
                    mergedItems[stateToMerged[state]][key].Add(item.Lookahead);
                }
            }
            answer.CanonicalConflictCount = CountConflicts(states.Select(state => state.GroupBy(i => new CoreItem(i.Production, i.Dot))
                .ToDictionary(g => g.Key, g => g.Select(i => i.Lookahead).ToHashSet(StringComparer.Ordinal))).ToList(), transitions, model);
            answer.MergedConflictCount = CountConflicts(mergedItems, mergedTransitions, model);
            return answer;
        }

        private static int CountConflicts(List<Dictionary<CoreItem, HashSet<string>>> states,
            Dictionary<(int State, string Symbol), int> transitions, Model model)
        {
            int conflicts = 0;
            for (int state = 0; state < states.Count; state++)
            {
                var actions = new Dictionary<string, HashSet<string>>(StringComparer.Ordinal);
                void Add(string token, string action)
                {
                    if (!actions.TryGetValue(token, out var choices)) actions.Add(token, choices = new HashSet<string>(StringComparer.Ordinal));
                    choices.Add(action);
                }
                foreach (var item in states[state])
                {
                    var rule = model.Rules[item.Key.Production];
                    if (item.Key.Dot < rule.Rhs.Length)
                    {
                        string symbol = rule.Rhs[item.Key.Dot];
                        if (!model.ByLhs.ContainsKey(symbol)) Add(symbol, "shift:" + transitions[(state, symbol)]);
                    }
                    else foreach (string token in item.Value) Add(token, rule.Id == 0 ? "accept" : "reduce:" + rule.Id);
                }
                conflicts += actions.Values.Count(set => set.Count > 1);
            }
            return conflicts;
        }
    }
}
