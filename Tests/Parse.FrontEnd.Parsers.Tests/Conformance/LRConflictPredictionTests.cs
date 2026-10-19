using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Xunit;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// State-only prediction is tested against deliberately small LR tables. These tables isolate
/// invariants that are difficult to force through grammar generation, including cycles and
/// malformed EOF actions. EBNF-to-parser integration is covered by EbnfPredicateTests.
/// </summary>
[Trait("Category", "Prediction")]
public class LRConflictPredictionTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public void Prefix_and_longer_input_select_the_original_action_in_either_array_order(bool reverse, bool longer)
    {
        var fixture = new PrefixFixture();
        var actions = reverse ? fixture.Actions.Reverse().ToArray() : fixture.Actions;
        var actualStates = new[] { 0, 1 };
        var tokens = fixture.Input(longer);
        var beforeTokens = tokens.ToArray();
        var beforeCells = Snapshot(fixture.Table);

        var action = fixture.Predictor().SelectFirstAction(actions, actualStates, tokens, 1);

        Assert.Same(longer ? fixture.Shift : fixture.GuardedReduce, action);
        Assert.Equal(new[] { 0, 1 }, actualStates);
        Assert.Equal(beforeTokens, tokens);
        Assert.Equal(beforeCells, Snapshot(fixture.Table));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(30)]
    [InlineData(300)]
    public void A_shared_prefix_is_examined_for_as_many_tokens_as_this_input_requires(int sharedTokens)
    {
        var fixture = new PrefixFixture(sharedTokens);
        Assert.Same(fixture.GuardedReduce,
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
        Assert.Same(fixture.Shift,
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(true), 1));
    }

    [Fact]
    public void A_false_guard_leaves_shift_as_the_only_origin()
    {
        var fixture = new PrefixFixture(allow: false);
        Assert.Same(fixture.Shift,
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(true), 1));
    }

    [Fact]
    public void Predicate_exception_is_not_interpreted_as_false_or_replaced_with_shift()
    {
        var fixture = new PrefixFixture();
        var expected = new InvalidOperationException("callback failed");
        var predictor = new LRConflictPredictor(fixture.Table, (action, tokens, index) =>
        {
            if (action == fixture.GuardedReduce) throw expected;
            return true;
        });

        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() =>
            predictor.SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1)));
    }

    [Fact]
    public void No_surviving_origin_reports_the_position_that_eliminated_both_candidates()
    {
        var fixture = new PrefixFixture();
        var tokens = new[] { Token(fixture.X), Token(fixture.Y), Token(fixture.Q), Token(new EndMarker()) };
        var error = Assert.Throws<PredictionFailedException>(() =>
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, tokens, 1));

        Assert.Equal(2, error.TokenIndex);
        Assert.Equal(1, error.State);
        Assert.Contains("No candidate", error.Message);
    }

    [Fact]
    public void Two_origins_accepting_at_eof_report_ambiguity_instead_of_choosing_the_first()
    {
        var fixture = new PrefixFixture();
        fixture.Table[fixture.BLast].MatchedValueSet[new EndMarker()] =
            List(new ActionData(ActionDir.Reduce, fixture.ShortB));

        var error = Assert.Throws<AmbiguousPredictionException>(() =>
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));

        Assert.Equal(2, error.TokenIndex);
        Assert.Contains("More than one initial action", error.Message);
    }

    [Fact]
    public void A_unique_origin_is_returned_before_later_unrelated_invalid_input()
    {
        var fixture = new PrefixFixture();
        var tokens = new[] { Token(fixture.X), Token(fixture.Y), Token(fixture.Z), Token(fixture.Q), Token(new EndMarker()) };

        // z eliminates A. q will fail when the real parser later validates the selected B rule.
        Assert.Same(fixture.Shift,
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, tokens, 1));
    }

    [Fact]
    public void Multiple_later_configurations_from_one_origin_still_mean_one_initial_action()
    {
        var fixture = new PrefixFixture();
        var nestedGuard = fixture.Production("NestedGuard", new Epsilon());
        nestedGuard.Predicate = new GrammarPredicate("Nested", context => true);
        int direct = AddRow(fixture.Table);
        int guardGoto = AddRow(fixture.Table);
        int indirect = AddRow(fixture.Table);
        fixture.Table[fixture.ALast].MatchedValueSet[fixture.Q] = List(
            new ActionData(ActionDir.Shift, direct), new ActionData(ActionDir.EpsilonReduce, nestedGuard));
        fixture.Table[fixture.ALast].MatchedValueSet[nestedGuard.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, guardGoto));
        fixture.Table[guardGoto].MatchedValueSet[fixture.Q] = List(new ActionData(ActionDir.Shift, indirect));
        var tokens = new[] { Token(fixture.X), Token(fixture.Y), Token(fixture.Q), Token(new EndMarker()) };

        Assert.Same(fixture.GuardedReduce,
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, tokens, 1));
    }

    [Fact]
    public void Multiple_eof_accepts_from_one_origin_return_that_action_without_false_ambiguity()
    {
        var fixture = new PrefixFixture();
        var nested = fixture.Production("Nested", new Epsilon());
        nested.Predicate = new GrammarPredicate("Nested", context => true);
        int afterGuard = AddRow(fixture.Table);
        int alternative = AddRow(fixture.Table);
        fixture.Table[fixture.GuardState].MatchedValueSet[fixture.Y] = List(
            new ActionData(ActionDir.Shift, fixture.ALast), new ActionData(ActionDir.EpsilonReduce, nested));
        fixture.Table[fixture.GuardState].MatchedValueSet[nested.ToNonTerminal()] =
            List(new ActionData(ActionDir.Goto, afterGuard));
        fixture.Table[afterGuard].MatchedValueSet[fixture.Y] = List(new ActionData(ActionDir.Shift, alternative));
        fixture.Table[fixture.ALast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Accept, fixture.StartA));
        fixture.Table[alternative].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Accept, fixture.StartA));

        Assert.Same(fixture.GuardedReduce,
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
    }

    [Fact]
    public void Candidate_identity_includes_the_full_stack_not_only_its_top_state()
    {
        var fixture = new PrefixFixture();
        var nested = fixture.Production("Nested", new Epsilon());
        nested.Predicate = new GrammarPredicate("Nested", context => true);
        var reduceY = fixture.Production("ReducedY", fixture.Y);
        int afterNested = AddRow(fixture.Table);
        int dead = AddRow(fixture.Table);
        int accepting = AddRow(fixture.Table);
        fixture.Table[fixture.GuardState].MatchedValueSet[fixture.Y] = List(
            new ActionData(ActionDir.Shift, fixture.ALast), new ActionData(ActionDir.EpsilonReduce, nested));
        fixture.Table[fixture.GuardState].MatchedValueSet[nested.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, afterNested));
        fixture.Table[afterNested].MatchedValueSet[fixture.Y] = List(new ActionData(ActionDir.Shift, fixture.ALast));
        fixture.Table[fixture.ALast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Reduce, reduceY));
        fixture.Table[fixture.GuardState].MatchedValueSet[reduceY.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, dead));
        fixture.Table[afterNested].MatchedValueSet[reduceY.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, accepting));
        fixture.Table[accepting].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Accept, fixture.StartA));
        fixture.Table[fixture.BLast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Accept, fixture.StartA));

        // Two A configurations share their top, but Reduce reaches different Goto rows below it.
        // The direct A path dies; the indirect one accepts. It must survive alongside origin B.
        Assert.Throws<AmbiguousPredictionException>(() =>
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
    }

    [Fact]
    public void Ordinary_epsilon_and_three_way_conflicts_are_not_supported()
    {
        var fixture = new PrefixFixture();
        var ordinary = fixture.Production("Ordinary", new Epsilon());
        Assert.False(LRConflictPredictor.IsSupportedConflict(new[] { fixture.Shift, new ActionData(ActionDir.EpsilonReduce, ordinary) }));
        Assert.False(LRConflictPredictor.IsSupportedConflict(new[] { fixture.Shift, fixture.GuardedReduce, new ActionData(ActionDir.EpsilonReduce, ordinary) }));
        Assert.False(LRConflictPredictor.IsSupportedConflict(new[] { fixture.GuardedReduce, fixture.GuardedReduce }));
        Assert.False(LRConflictPredictor.IsSupportedConflict(new[] { fixture.Shift }));
        Assert.False(LRConflictPredictor.IsSupportedConflict(null));
    }

    [Fact]
    public void Later_ordinary_epsilon_conflict_keeps_the_table_first_action_policy()
    {
        var fixture = new PrefixFixture();
        var ordinary = fixture.Production("Ordinary", new Epsilon());
        // If all epsilon conflicts were branched, this new ordinary epsilon action would loop.
        fixture.Table[fixture.ALast].MatchedValueSet[new EndMarker()].Add(new ActionData(ActionDir.EpsilonReduce, ordinary));

        Assert.Same(fixture.GuardedReduce,
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
    }

    [Fact]
    public void A_three_way_cell_does_not_become_predictable_after_one_false_predicate_is_filtered()
    {
        var fixture = new PrefixFixture();
        var denied = fixture.Production("Denied", new Epsilon());
        denied.Predicate = new GrammarPredicate("Denied", context => false);
        var permitted = fixture.Production("Permitted", new Epsilon());
        permitted.Predicate = new GrammarPredicate("Permitted", context => true);
        int next = AddRow(fixture.Table);
        fixture.Table[fixture.ALast].MatchedValueSet[fixture.Q] = List(
            new ActionData(ActionDir.Shift, next),
            new ActionData(ActionDir.EpsilonReduce, denied),
            new ActionData(ActionDir.EpsilonReduce, permitted));
        // No Goto for Permitted: it must not be explored as a supported two-way conflict.
        var tokens = new[] { Token(fixture.X), Token(fixture.Y), Token(fixture.Q), Token(new EndMarker()) };

        Assert.Same(fixture.GuardedReduce,
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, tokens, 1));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Callback_receives_the_candidate_physical_token_position_and_real_token_objects(bool allowed)
    {
        var fixture = new PrefixFixture();
        var seen = new List<PredicateContext>();
        var nextGuard = fixture.Production("AtEOF", new Epsilon());
        nextGuard.Predicate = new GrammarPredicate("AtEOF", context => { seen.Add(context); return allowed; });
        int after = AddRow(fixture.Table);
        fixture.Table[fixture.ALast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.EpsilonReduce, nextGuard));
        fixture.Table[fixture.ALast].MatchedValueSet[nextGuard.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, after));
        fixture.Table[after].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Accept, fixture.StartA));
        fixture.Table[fixture.BLast].MatchedValueSet[new EndMarker()] =
            List(new ActionData(ActionDir.Reduce, fixture.ShortB));
        var tokens = fixture.Input(false);

        if (allowed)
            Assert.Throws<AmbiguousPredictionException>(() =>
                fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, tokens, 1));
        else
            Assert.Same(fixture.Shift,
                fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, tokens, 1));
        var context = Assert.Single(seen);
        Assert.Equal(2, context.TokenIndex);
        Assert.Same(tokens[1], context.PreviousToken);
        Assert.Same(tokens[2], context.NextToken);
        Assert.True(context.IsEndOfInput);
    }

    [Fact]
    public void Non_consuming_reduce_cycle_reports_a_cycle_instead_of_hanging_or_discarding_the_candidate()
    {
        var fixture = new PrefixFixture();
        var loop = fixture.Production("Loop", fixture.Y);
        fixture.Table[fixture.ALast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Reduce, loop));
        fixture.Table[fixture.GuardState].MatchedValueSet[loop.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, fixture.ALast));

        var error = Assert.Throws<PredictionCycleException>(() =>
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
        Assert.Equal(2, error.TokenIndex);
        Assert.Contains("non-consuming", error.Message);
    }

    [Fact]
    public void Two_frontier_configurations_in_the_same_nullable_cycle_cannot_hide_the_cycle_by_merging()
    {
        var fixture = new PrefixFixture();
        var nested = fixture.Production("Nested", new Epsilon());
        nested.Predicate = new GrammarPredicate("Nested", context => true);
        var grow = fixture.Production("Grow", new Epsilon());
        var pair = fixture.Production("Pair", fixture.Y, nested.ToNonTerminal());
        int deeper = AddRow(fixture.Table);
        fixture.Table[fixture.GuardState].MatchedValueSet[fixture.Y] = List(
            new ActionData(ActionDir.Shift, fixture.ALast), new ActionData(ActionDir.EpsilonReduce, nested));
        fixture.Table[fixture.GuardState].MatchedValueSet[nested.ToNonTerminal()] =
            List(new ActionData(ActionDir.Goto, fixture.ALast));
        fixture.Table[fixture.ALast].MatchedValueSet[fixture.Y] = List(new ActionData(ActionDir.Shift, deeper));

        // After y, one A configuration has [..., ALast], the other [..., ALast, deeper].
        // EOF alternates between those exact configurations without consuming any token.
        fixture.Table[fixture.ALast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.EpsilonReduce, grow));
        fixture.Table[fixture.ALast].MatchedValueSet[grow.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, deeper));
        fixture.Table[deeper].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Reduce, pair));
        fixture.Table[fixture.GuardState].MatchedValueSet[pair.ToNonTerminal()] =
            List(new ActionData(ActionDir.Goto, fixture.ALast));
        fixture.Table[fixture.BLast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Accept, fixture.StartA));

        // B accepts. Silently losing the cycling A origin would incorrectly choose B.
        Assert.Throws<PredictionCycleException>(() =>
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
    }

    [Fact]
    public void Two_frontier_configurations_can_merge_without_a_cycle_or_losing_their_successful_origin()
    {
        var fixture = new PrefixFixture();
        var nested = fixture.Production("Nested", new Epsilon());
        nested.Predicate = new GrammarPredicate("Nested", context => true);
        var grow = fixture.Production("Grow", new Epsilon());
        var pair = fixture.Production("Pair", fixture.Y, nested.ToNonTerminal());
        int deeper = AddRow(fixture.Table);
        int accepted = AddRow(fixture.Table);
        fixture.Table[fixture.GuardState].MatchedValueSet[fixture.Y] = List(
            new ActionData(ActionDir.Shift, fixture.ALast), new ActionData(ActionDir.EpsilonReduce, nested));
        fixture.Table[fixture.GuardState].MatchedValueSet[nested.ToNonTerminal()] =
            List(new ActionData(ActionDir.Goto, fixture.ALast));
        fixture.Table[fixture.ALast].MatchedValueSet[fixture.Y] = List(new ActionData(ActionDir.Shift, deeper));
        fixture.Table[fixture.ALast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.EpsilonReduce, grow));
        fixture.Table[fixture.ALast].MatchedValueSet[grow.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, deeper));
        fixture.Table[deeper].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Reduce, pair));
        fixture.Table[fixture.GuardState].MatchedValueSet[pair.ToNonTerminal()] =
            List(new ActionData(ActionDir.Goto, accepted));
        fixture.Table[accepted].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Accept, fixture.StartA));

        // Both A configurations reach 'deeper', then the same completed state. That is a join,
        // rather than a cycle: global dedup may remove repeated work but must preserve origin A.
        Assert.Same(fixture.GuardedReduce,
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1000)]
    public void Malformed_shift_destinations_have_a_positioned_table_error(int destination)
    {
        var fixture = new PrefixFixture();
        var actions = new[] { new ActionData(ActionDir.Shift, destination), fixture.GuardedReduce };
        var error = Assert.Throws<InvalidPredictionTableException>(() =>
            fixture.Predictor().SelectFirstAction(actions, new[] { 0, 1 }, fixture.Input(false), 1));
        Assert.Equal(1, error.TokenIndex);
        Assert.Contains("destination state", error.Message);
    }

    [Fact]
    public void A_reduction_that_would_pop_the_initial_state_has_a_positioned_table_error()
    {
        var fixture = new PrefixFixture();
        var invalid = fixture.Production("TooLong", fixture.X, fixture.Y, fixture.Y, fixture.Y);
        fixture.Table[fixture.ALast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Reduce, invalid));
        var error = Assert.Throws<InvalidPredictionTableException>(() =>
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
        Assert.Equal(2, error.TokenIndex);
        Assert.Contains("initial state", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Missing_or_conflicting_goto_is_diagnosed_instead_of_guessing_a_state(bool conflicting)
    {
        var fixture = new PrefixFixture();
        if (conflicting)
            fixture.Table[0].MatchedValueSet[fixture.StartA.ToNonTerminal()].Add(new ActionData(ActionDir.Goto, fixture.ALast));
        else
            fixture.Table[0].MatchedValueSet.Remove(fixture.StartA.ToNonTerminal());
        var error = Assert.Throws<InvalidPredictionTableException>(() =>
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
        Assert.Equal(2, error.TokenIndex);
        Assert.Contains("requires one Goto", error.Message);
    }

    [Fact]
    public void A_growing_nullable_cycle_hits_an_explicit_storage_limit()
    {
        var fixture = new PrefixFixture();
        var grow = fixture.Production("Grow", new Epsilon());
        fixture.Table[fixture.ALast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.EpsilonReduce, grow));
        fixture.Table[fixture.ALast].MatchedValueSet[grow.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, fixture.ALast));

        var error = Assert.Throws<PredictionLimitException>(() => fixture.Predictor(
            new PredictionLimits(maximumStateDepth: 12)).SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
        Assert.Equal(2, error.TokenIndex);
        Assert.Contains("state-stack depth", error.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Work_or_configuration_limit_is_an_error_not_a_candidate_failure(bool transitions)
    {
        var fixture = new PrefixFixture();
        var limits = transitions
            ? new PredictionLimits(maximumTransitions: 1)
            : new PredictionLimits(maximumConfigurations: 1);
        var error = Assert.Throws<PredictionLimitException>(() =>
            fixture.Predictor(limits).SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
        Assert.Contains(transitions ? "transition" : "configuration", error.Message);
    }

    [Fact]
    public void A_synthetic_eof_shift_is_diagnosed_instead_of_losing_an_accepted_origin()
    {
        var fixture = new PrefixFixture();
        fixture.Table[fixture.BLast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Shift, fixture.BLast));
        var error = Assert.Throws<InvalidPredictionTableException>(() =>
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1));
        Assert.Contains("EOF must not be shifted", error.Message);
    }

    [Fact]
    public void Accept_before_the_final_eof_is_diagnosed_instead_of_ignoring_the_remaining_input()
    {
        var fixture = new PrefixFixture();
        var tokens = new[] { Token(fixture.X), Token(fixture.Y), Token(new EndMarker()), Token(fixture.Q), Token(new EndMarker()) };
        var error = Assert.Throws<InvalidPredictionTableException>(() =>
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, tokens, 1));
        Assert.Equal(2, error.TokenIndex);
        Assert.Contains("final EOF position", error.Message);
    }

    [Fact]
    public void A_single_predictor_can_serve_independent_concurrent_calls()
    {
        var fixture = new PrefixFixture(10);
        var predictor = fixture.Predictor();
        var before = Snapshot(fixture.Table);
        Parallel.For(0, 100, i =>
        {
            bool longer = i % 2 == 0;
            Assert.Same(longer ? fixture.Shift : fixture.GuardedReduce,
                predictor.SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(longer), 1));
        });
        Assert.Equal(before, Snapshot(fixture.Table));
    }

    [Fact]
    public void The_transition_budget_belongs_to_each_call_even_when_the_predictor_is_reused_concurrently()
    {
        var fixture = new PrefixFixture();
        var predictor = fixture.Predictor(new PredictionLimits(maximumTransitions: 5));

        // This input needs five prediction actions. Sharing the counter between calls would
        // make later calls fail although each individual prediction is within its budget.
        Parallel.For(0, 20, _ => Assert.Same(fixture.GuardedReduce,
            predictor.SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(false), 1)));
    }

    [Fact]
    public void Invalid_call_arguments_do_not_start_prediction()
    {
        var fixture = new PrefixFixture();
        var predictor = fixture.Predictor();
        var tokens = fixture.Input(false);
        Assert.Throws<ArgumentException>(() => predictor.SelectFirstAction(new[] { fixture.Shift }, new[] { 0, 1 }, tokens, 1));
        Assert.Throws<ArgumentNullException>(() => predictor.SelectFirstAction(fixture.Actions, null, tokens, 1));
        Assert.Throws<ArgumentException>(() => predictor.SelectFirstAction(fixture.Actions, Array.Empty<int>(), tokens, 1));
        Assert.Throws<ArgumentException>(() => predictor.SelectFirstAction(fixture.Actions, new[] { 0, 1000 }, tokens, 1));
        Assert.Throws<ArgumentNullException>(() => predictor.SelectFirstAction(fixture.Actions, new[] { 0, 1 }, null, 1));
        Assert.Throws<ArgumentOutOfRangeException>(() => predictor.SelectFirstAction(fixture.Actions, new[] { 0, 1 }, tokens, -1));
        Assert.Throws<ArgumentException>(() => predictor.SelectFirstAction(fixture.Actions, new[] { 0, 1 }, tokens.Take(2).ToArray(), 1));
    }

    private static TokenData Token(Terminal terminal) => TokenData.CreateVirtualToken(terminal);
    private static ActionDataList List(params ActionData[] actions)
    {
        var list = new ActionDataList();
        list.AddRange(actions);
        return list;
    }
    private static int AddRow(LRParsingTable table)
    {
        int state = table.Count;
        table.Add(new LRParsingRowDataFormat(null, new ActionDicSymbolMatched()));
        return state;
    }
    private static ActionData[] Snapshot(LRParsingTable table)
        => table.SelectMany(row => row.MatchedValueSet.Values).SelectMany(actions => actions).ToArray();

    private sealed class PrefixFixture
    {
        private readonly KeyManager _keys = new KeyManager();
        private readonly int _shared;
        public readonly LRParsingTable Table = new LRParsingTable();
        public readonly Terminal X;
        public readonly Terminal Y;
        public readonly Terminal Z;
        public readonly Terminal Q;
        public readonly ActionData Shift;
        public readonly ActionData GuardedReduce;
        public readonly ActionData[] Actions;
        public readonly NonTerminalSingle StartA;
        public readonly NonTerminalSingle ShortB;
        public readonly int ALast;
        public readonly int BLast;
        public readonly int GuardState;

        // S : 'x' {Allow()}? 'y' ... 'y' | 'x' 'y' ... 'y' 'z' ;
        public PrefixFixture(int shared = 1, bool allow = true)
        {
            _shared = shared;
            X = Terminal("x"); Y = Terminal("y"); Z = Terminal("z"); Q = Terminal("q");
            var guard = Production("Guard", new Epsilon());
            guard.Predicate = new GrammarPredicate("Allow", context => allow);
            var start = new NonTerminal("S", true);
            _keys.AllocateUniqueKey(start);
            start.Add(new[] { (Symbol)X, guard.ToNonTerminal() }.Concat(Enumerable.Repeat<Symbol>(Y, shared)).ToArray());
            start.Add(new[] { (Symbol)X }.Concat(Enumerable.Repeat<Symbol>(Y, shared)).Concat(new[] { (Symbol)Z }).ToArray());
            start.Add(new[] { (Symbol)X }.Concat(Enumerable.Repeat<Symbol>(Y, shared)).ToArray());
            StartA = start[0];
            ShortB = start[2];
            ALast = 2 * shared + 1;
            BLast = shared + 1;
            int accept = 2 * shared + 2;
            int afterZ = 2 * shared + 3;
            GuardState = 2 * shared + 4;
            for (int i = 0; i <= GuardState; i++) AddRow(Table);

            Shift = new ActionData(ActionDir.Shift, 2);
            GuardedReduce = new ActionData(ActionDir.EpsilonReduce, guard);
            Actions = new[] { Shift, GuardedReduce }; // mirrors the engine's usual Shift-first order
            Table[1].MatchedValueSet[Y] = List(Actions);
            Table[1].MatchedValueSet[guard.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, GuardState));
            Table[GuardState].MatchedValueSet[Y] = List(new ActionData(ActionDir.Shift, shared + 2));
            for (int i = 2; i < BLast; i++) Table[i].MatchedValueSet[Y] = List(new ActionData(ActionDir.Shift, i + 1));
            for (int i = shared + 2; i < ALast; i++) Table[i].MatchedValueSet[Y] = List(new ActionData(ActionDir.Shift, i + 1));
            Table[BLast].MatchedValueSet[Z] = List(new ActionData(ActionDir.Shift, afterZ));
            Table[ALast].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Reduce, StartA));
            Table[afterZ].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Reduce, start[1]));
            Table[0].MatchedValueSet[start] = List(new ActionData(ActionDir.Goto, accept));
            Table[accept].MatchedValueSet[new EndMarker()] = List(new ActionData(ActionDir.Accept, StartA));
        }

        public Terminal Terminal(string text)
        {
            var result = new Terminal(TokenType.Operator, text);
            _keys.AllocateUniqueKey(result);
            return result;
        }
        public NonTerminalSingle Production(string name, params Symbol[] rhs)
        {
            var head = new NonTerminal(name);
            _keys.AllocateUniqueKey(head);
            head.Add(rhs);
            return head[0];
        }
        public TokenData[] Input(bool longer)
            => new[] { Token(X) }.Concat(Enumerable.Range(0, _shared).Select(_ => Token(Y)))
                .Concat(longer ? new[] { Token(Z) } : Array.Empty<TokenData>())
                .Concat(new[] { Token(new EndMarker()) }).ToArray();

        public LRConflictPredictor Predictor(PredictionLimits limits = null)
            => new LRConflictPredictor(Table, Evaluate, limits);
        private static bool Evaluate(ActionData action, IReadOnlyList<TokenData> tokens, int index)
        {
            if (action.Direction != ActionDir.EpsilonReduce ||
                !(action.Dest is NonTerminalSingle production) || production.Predicate == null) return true;
            return production.Predicate.Evaluate(new PredicateContext(index == 0 ? null : tokens[index - 1], tokens[index], index));
        }
    }
}
