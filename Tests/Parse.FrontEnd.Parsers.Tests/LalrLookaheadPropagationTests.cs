using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;
using LookAheadEntry = Janglim.FrontEnd.Parsers.Collections.CanonicalRelation.LookAheadEntry;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Exercises the post-recursion propagation independently of predecessor discovery.
/// Real LR items and entries are used; expected random-graph results are obtained
/// by token reachability rather than by repeating the production union algorithm.
/// </summary>
[Trait("Category", "LalrLookahead")]
[Trait("Feature", "LalrRecursivePropagation")]
public sealed class LalrLookaheadPropagationTests
{
    [Fact]
    public void New_entries_have_separate_empty_sets_and_target_collections()
    {
        var first = new LookAheadEntry();
        var second = new LookAheadEntry();

        Assert.Empty(first.LookAhead);
        Assert.Empty(first.Targets);
        Assert.NotSame(first.LookAhead, second.LookAhead);
        Assert.NotSame(first.Targets, second.Targets);
    }

    [Fact]
    public void Empty_table_does_not_create_entries_or_states()
    {
        var canonical = new CanonicalRelation();
        var table = canonical.LookAheadTable;

        Propagate(canonical);

        Assert.Same(table, canonical.LookAheadTable);
        Assert.Empty(table);
        Assert.Empty(canonical.IndexStateDic);
        Assert.Empty(canonical);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Chain_reaches_every_target_regardless_of_initial_queue_order(bool reverse)
    {
        var graph = new Graph();
        foreach (int state in reverse ? new[] { 3, 2, 1, 0 } : new[] { 0, 1, 2, 3 })
            graph.Add(state);
        graph.Seed(0, "a");
        graph.Link(0, 1);
        graph.Link(1, 2);
        graph.Link(2, 3);

        Propagate(graph.Canonical);

        for (int state = 0; state < 4; state++)
            AssertNames(graph.Entry(state).LookAhead, "a");
    }

    [Fact]
    public void Diamond_combines_late_arrivals_without_leaking_between_sibling_branches()
    {
        var graph = new Graph();
        graph.Add(4, "d");
        graph.Add(3);
        graph.Add(2, "c");
        graph.Add(1, "b");
        graph.Add(0, "a");
        graph.Link(0, 1);
        graph.Link(0, 2);
        graph.Link(1, 3);
        graph.Link(2, 3);
        graph.Link(3, 4);

        Propagate(graph.Canonical);

        AssertNames(graph.Entry(0).LookAhead, "a");
        AssertNames(graph.Entry(1).LookAhead, "a", "b");
        AssertNames(graph.Entry(2).LookAhead, "a", "c");
        AssertNames(graph.Entry(3).LookAhead, "a", "b", "c");
        AssertNames(graph.Entry(4).LookAhead, "a", "b", "c", "d");
    }

    [Fact]
    public void Mutual_cycle_merges_each_nodes_distinct_seed_and_terminates()
    {
        var graph = new Graph();
        graph.Add(0, "a");
        graph.Add(1, "b");
        graph.Add(2, "c");
        graph.Link(0, 1);
        graph.Link(1, 2);
        graph.Link(2, 0);

        Propagate(graph.Canonical);

        for (int state = 0; state < 3; state++)
            AssertNames(graph.Entry(state).LookAhead, "a", "b", "c");
    }

    [Fact]
    public void Self_reference_stops_without_skipping_another_target()
    {
        var graph = new Graph();
        graph.Add(0, "a");
        graph.Add(1);
        graph.Link(0, 0);
        graph.Link(0, 1);

        Propagate(graph.Canonical);

        AssertNames(graph.Entry(0).LookAhead, "a");
        AssertNames(graph.Entry(1).LookAhead, "a");
    }

    [Fact]
    public void Empty_cycle_does_not_invent_tokens_or_read_an_unrelated_seed()
    {
        var graph = new Graph();
        graph.Add(0);
        graph.Add(1);
        graph.Add(2, "c");
        graph.Add(3);
        graph.Link(0, 1);
        graph.Link(1, 0);
        graph.Link(1, 3);

        Propagate(graph.Canonical);

        Assert.Empty(graph.Entry(0).LookAhead);
        Assert.Empty(graph.Entry(1).LookAhead);
        Assert.Empty(graph.Entry(3).LookAhead);
        AssertNames(graph.Entry(2).LookAhead, "c");
    }

    [Fact]
    public void Disconnected_components_keep_their_own_tokens()
    {
        var graph = new Graph();
        graph.Add(0, "a");
        graph.Add(1);
        graph.Add(2, "b");
        graph.Add(3);
        graph.Link(0, 1);
        graph.Link(2, 3);

        Propagate(graph.Canonical);

        AssertNames(graph.Entry(0).LookAhead, "a");
        AssertNames(graph.Entry(1).LookAhead, "a");
        AssertNames(graph.Entry(2).LookAhead, "b");
        AssertNames(graph.Entry(3).LookAhead, "b");
    }

    [Fact]
    public void Repeated_dependency_is_deduplicated_and_still_propagates()
    {
        var graph = new Graph();
        graph.Add(0, "a");
        graph.Add(1);

        Assert.True(graph.Entry(0).Targets.Add(graph.Key(1)));
        Assert.False(graph.Entry(0).Targets.Add(graph.Key(1)));
        Assert.Single(graph.Entry(0).Targets);

        Propagate(graph.Canonical);

        AssertNames(graph.Entry(1).LookAhead, "a");
        Assert.Single(graph.Entry(0).Targets);
    }

    [Fact]
    public void Unchanged_first_target_does_not_return_before_remaining_targets()
    {
        var graph = new Graph();
        graph.Add(0, "a");
        graph.Add(1);
        graph.Add(2);
        graph.Link(0, 1);
        graph.Link(0, 2);

        // Whichever key the HashSet enumerates first already has the seed.
        // The next target must still receive it: the guard must continue,
        // rather than return from the whole propagation method.
        var firstTarget = graph.Entry(0).Targets.First();
        graph.Canonical.LookAheadTable[firstTarget].LookAhead.UnionWith(graph.Entry(0).LookAhead);

        Propagate(graph.Canonical);

        AssertNames(graph.Entry(1).LookAhead, "a");
        AssertNames(graph.Entry(2).LookAhead, "a");
    }

    [Fact]
    public void Unchanged_early_source_does_not_skip_later_sources_in_the_queue()
    {
        var graph = new Graph();
        graph.Add(0);
        graph.Add(1);
        graph.Add(2, "b");
        graph.Add(3);
        graph.Link(0, 1);
        graph.Link(2, 3);

        Propagate(graph.Canonical);

        Assert.Empty(graph.Entry(1).LookAhead);
        AssertNames(graph.Entry(3).LookAhead, "b");
    }

    [Fact]
    public void Propagation_preserves_entries_sets_targets_and_LR_item_metadata()
    {
        var graph = new Graph();
        graph.Add(0, "a");
        graph.Add(1);
        graph.Add(2);
        graph.Link(0, 1);
        graph.Link(1, 2);
        var table = graph.Canonical.LookAheadTable;
        var before = table.Select(pair => new
        {
            pair.Key,
            Entry = pair.Value,
            Set = pair.Value.LookAhead,
            Targets = pair.Value.Targets,
            TargetKeys = pair.Value.Targets.ToArray(),
            ItemText = pair.Key.Item2.ToString(),
            ItemHash = pair.Key.Item2.GetHashCode(),
            ItemLookAhead = pair.Key.Item2.LookAhead
        }).ToArray();

        Propagate(graph.Canonical);

        Assert.Same(table, graph.Canonical.LookAheadTable);
        Assert.Equal(before.Length, table.Count);
        foreach (var snapshot in before)
        {
            var entry = table[snapshot.Key];
            Assert.Same(snapshot.Entry, entry);
            Assert.Same(snapshot.Set, entry.LookAhead);
            Assert.Same(snapshot.Targets, entry.Targets);
            Assert.True(entry.Targets.SetEquals(snapshot.TargetKeys));
            Assert.Equal(snapshot.ItemText, snapshot.Key.Item2.ToString());
            Assert.Equal(snapshot.ItemHash, snapshot.Key.Item2.GetHashCode());
            Assert.Same(snapshot.ItemLookAhead, snapshot.Key.Item2.LookAhead);
        }
        Assert.Empty(graph.Canonical.IndexStateDic);
        Assert.Empty(graph.Canonical);
    }

    [Fact]
    public void A_second_call_is_idempotent_and_new_seeds_use_the_retained_relations()
    {
        var graph = new Graph();
        graph.Add(2);
        graph.Add(1);
        graph.Add(0, "a");
        graph.Link(0, 1);
        graph.Link(1, 2);

        Propagate(graph.Canonical);
        Propagate(graph.Canonical);
        for (int state = 0; state < 3; state++)
            AssertNames(graph.Entry(state).LookAhead, "a");

        graph.Seed(0, "c");
        Propagate(graph.Canonical);

        for (int state = 0; state < 3; state++)
            AssertNames(graph.Entry(state).LookAhead, "a", "c");
        Assert.Single(graph.Entry(0).Targets);
        Assert.Single(graph.Entry(1).Targets);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(8)]
    [InlineData(9)]
    [InlineData(10)]
    [InlineData(11)]
    [InlineData(12)]
    [InlineData(13)]
    [InlineData(14)]
    [InlineData(15)]
    public void Graph_results_match_independent_per_token_reachability(int seed)
    {
        const int count = 6;
        var graph = new Graph();
        var random = new Random(0x5EED + seed);
        var initial = new HashSet<string>[count];
        var edges = new List<int>[count];
        foreach (int state in Enumerable.Range(0, count).OrderBy(_ => random.Next()))
            graph.Add(state);
        for (int state = 0; state < count; state++)
        {
            initial[state] = new HashSet<string>(new[] { "a", "b", "c" }.Where(_ => random.Next(3) == 0));
            graph.Seed(state, initial[state].ToArray());
            edges[state] = new List<int>();
            for (int target = 0; target < count; target++)
            {
                if (random.Next(4) != 0) continue;
                edges[state].Add(target);
                graph.Link(state, target);
            }
        }

        var expected = Enumerable.Range(0, count).Select(_ => new HashSet<string>()).ToArray();
        for (int origin = 0; origin < count; origin++)
        {
            // Determine all destinations reachable from each original seed.
            // This oracle never reads the mutated production entry sets.
            var reached = new HashSet<int> { origin };
            var remaining = new Stack<int>();
            remaining.Push(origin);
            while (remaining.Count > 0)
            {
                int state = remaining.Pop();
                foreach (int target in edges[state])
                    if (reached.Add(target)) remaining.Push(target);
            }
            foreach (int target in reached)
                expected[target].UnionWith(initial[origin]);
        }

        Propagate(graph.Canonical);

        for (int state = 0; state < count; state++)
            AssertNames(graph.Entry(state).LookAhead, expected[state].ToArray());
    }

    [Fact]
    public void Recursive_parser_records_valid_inherited_edges_and_publishes_shared_sets()
    {
        var parser = new LALRParser(new TestGrammar());
        var table = parser.Canonical.LookAheadTable;
        int inheritedEdges = 0;

        foreach (var source in table)
        {
            Assert.True(parser.Canonical.IndexStateDic.TryGetValue(source.Key.Item1, out var state));
            Assert.True(state.TryGetValue(source.Key.Item2, out var item));
            Assert.Same(source.Value.LookAhead, item.LookAhead);

            foreach (var targetKey in source.Value.Targets)
            {
                Assert.True(table.TryGetValue(targetKey, out var target));
                Assert.True(source.Value.LookAhead.IsSubsetOf(target.LookAhead));
                if (!source.Key.Equals(targetKey)) inheritedEdges++;
            }
        }

        Assert.True(inheritedEdges > 0, "S -> A and A -> a must record a real cross-item inheritance edge.");
        var completedChild = Assert.Single(table, pair => pair.Key.Item2.IsReachedHandle &&
            pair.Key.Item2.SingleNT.Name == "A");
        Assert.Single(completedChild.Value.LookAhead);
        Assert.IsType<EndMarker>(Assert.Single(completedChild.Value.LookAhead));
    }

    private static void Propagate(CanonicalRelation canonical)
    {
        var method = typeof(CanonicalRelation).GetMethod(
            "PropagateLookAhead", BindingFlags.Instance | BindingFlags.NonPublic);
        Assert.NotNull(method);
        method.Invoke(canonical, null);
    }

    private static void AssertNames(TerminalSet actual, params string[] expected)
        => Assert.Equal(expected.OrderBy(value => value, StringComparer.Ordinal),
            actual.Select(token => token.Value).OrderBy(value => value, StringComparer.Ordinal));

    private sealed class Graph
    {
        private readonly TestGrammar grammar = new TestGrammar();
        private readonly LRItem item;
        internal CanonicalRelation Canonical { get; } = new CanonicalRelation();

        internal Graph()
        {
            grammar.Optimization();
            item = new LRItem(grammar.StartSymbol[0]);
        }

        internal (int State, LRItem Item) Key(int state) => (state, item);
        internal LookAheadEntry Entry(int state) => Canonical.LookAheadTable[Key(state)];

        internal void Add(int state, params string[] tokens)
        {
            Canonical.LookAheadTable.Add(Key(state), new LookAheadEntry());
            Seed(state, tokens);
        }

        internal void Seed(int state, params string[] tokens)
            => Entry(state).LookAhead.UnionWith(tokens.Select(grammar.Token));

        internal void Link(int source, int target) => Entry(source).Targets.Add(Key(target));
    }

    private sealed class TestGrammar : Grammar
    {
        private readonly Dictionary<string, Terminal> tokens;
        public override NonTerminal EbnfRoot => StartSymbol;
        internal Terminal Token(string value) => tokens[value];

        internal TestGrammar()
        {
            var start = new NonTerminal("S", true);
            var child = new NonTerminal("A");
            foreach (var rule in new[] { start, child })
            {
                keyManager.AllocateUniqueKey(rule);
                NonTerminalMultiples.Add(rule);
            }
            tokens = new[] { "a", "b", "c", "d" }.ToDictionary(
                value => value, value => new Terminal(TokenType.Operator, value));
            foreach (var token in tokens.Values)
            {
                keyManager.AllocateUniqueKey(token);
                TerminalSet.Add(token);
            }
            SetStartSymbol(start);
            start.Add(new Symbol[] { child });
            child.Add(new Symbol[] { tokens["a"] });
        }
    }
}
