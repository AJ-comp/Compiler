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
using Janglim.FrontEnd.Tokenize;
using Xunit;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Small LR tables isolate prediction from grammar generation. The guarded path reads /a/g
/// as one token, while the Shift path reads four tokens from exactly the same source suffix.
/// </summary>
[Trait("Category", "Prediction")]
public class ContextualConflictPredictionTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_origin_that_accepts_earlier_survives_until_the_other_origin_fails(bool reverse)
    {
        var fixture = new Fixture(longAccepts: false);
        var actions = reverse ? fixture.Actions.Reverse().ToArray() : fixture.Actions;
        var input = fixture.Input(out var source);
        var originalCursor = input.Cursor;
        var originalHistory = input.History;
        var originalPending = input.Pending;
        var originalGoal = input.PendingGoal;
        var originalStates = new[] { 0, 1 };
        var originalTable = Snapshot(fixture.Table);

        var selected = fixture.Predictor().SelectFirstAction(actions, originalStates, input);

        Assert.Same(fixture.GuardedReduce, selected);
        Assert.Same(originalCursor, input.Cursor);
        Assert.Same(originalHistory, input.History);
        Assert.Same(originalPending, input.Pending);
        Assert.Same(originalGoal, input.PendingGoal);
        Assert.Equal(new[] { 0, 1 }, originalStates);
        Assert.Equal(originalTable, Snapshot(fixture.Table));
        Assert.Contains(source.Calls, call => call.Offset == source.Length && call.Ordinal == 6);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Different_token_counts_that_accept_the_same_source_are_ambiguous(bool reverse)
    {
        var fixture = new Fixture();
        var actions = reverse ? fixture.Actions.Reverse().ToArray() : fixture.Actions;
        var input = fixture.Input(out var source);

        Assert.Throws<AmbiguousPredictionException>(() =>
            fixture.Predictor().SelectFirstAction(actions, new[] { 0, 1 }, input));

        Assert.Contains(source.Calls, call => call.Offset == source.Length && call.Ordinal == 3);
        Assert.Contains(source.Calls, call => call.Offset == source.Length && call.Ordinal == 6);
    }

    [Fact]
    public void An_early_failed_origin_leaves_the_original_shift_action()
    {
        var fixture = new Fixture(shortAccepts: false);
        Assert.Same(fixture.Shift, fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
    }

    [Fact]
    public void A_false_initial_guard_is_not_followed_or_used_to_change_the_real_input()
    {
        var fixture = new Fixture(allow: false);
        var input = fixture.Input(out var source);
        Assert.Same(fixture.Shift, fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, input));
        Assert.DoesNotContain(source.Calls, call => ReferenceEquals(call.Goal, fixture.RegexGoal));
        Assert.Equal(1, input.Cursor.SourceOffset);
        Assert.Single(input.History);
    }

    [Fact]
    public void A_pending_lookahead_is_shared_through_epsilon_reduce_and_goto_without_rescanning()
    {
        var fixture = new Fixture();
        var input = fixture.Input(out var source);
        Assert.Throws<AmbiguousPredictionException>(() =>
            fixture.Predictor().SelectFirstAction(fixture.Actions, new[] { 0, 1 }, input));
        Assert.Single(source.Calls, call => call.Offset == 1);
        Assert.Equal(1, input.Cursor.SourceOffset);
        Assert.Equal("y", input.Pending.Token.Input);
    }

    [Fact]
    public void Prediction_can_read_a_copy_without_filling_the_real_inputs_pending_token()
    {
        var fixture = new Fixture(longAccepts: false);
        var input = fixture.Input(out _, preloadLookahead: false);
        Assert.Null(input.Pending);
        var cursor = input.Cursor;
        var history = input.History;

        Assert.Same(fixture.GuardedReduce, fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, input));

        Assert.Null(input.Pending);
        Assert.Null(input.PendingGoal);
        Assert.Same(cursor, input.Cursor);
        Assert.Same(history, input.History);
    }

    [Fact]
    public void Goal_selection_receives_each_candidates_immutable_consumed_prefix()
    {
        var fixture = new Fixture();
        var contexts = new List<LexicalContext>();
        var input = fixture.Input(out _, context =>
        {
            contexts.Add(context);
            Assert.Equal(context.TokenIndex, context.ConsumedTokens.Count);
            if (context.TokenIndex == 0) Assert.Null(context.PreviousToken);
            else Assert.Same(context.ConsumedTokens[context.TokenIndex - 1], context.PreviousToken);
            return context.State == fixture.ALast || context.State == fixture.AfterRegex
                ? context.Goals[1] : context.Goals[0];
        });

        Assert.Throws<AmbiguousPredictionException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, input));

        var shortEnd = Assert.Single(contexts, context => context.State == fixture.AfterRegex);
        var longEnd = Assert.Single(contexts, context => context.State == fixture.AfterFlag);
        Assert.Equal(new[] { "x", "y", "/a/g" }, shortEnd.ConsumedTokens.Select(token => token.Input));
        Assert.Equal(new[] { "x", "y", "/", "a", "/", "g" }, longEnd.ConsumedTokens.Select(token => token.Input));
        Assert.Single(input.History);
        Assert.Equal("x", Assert.Single(input.History).Input);
    }

    [Fact]
    public void Predicate_context_contains_each_candidates_own_previous_token_and_ordinal()
    {
        var fixture = new Fixture();
        var observations = new List<PredicateContext>();
        var atShortEnd = fixture.Production("ShortEnd", new Epsilon());
        atShortEnd.Predicate = new GrammarPredicate("ShortEnd", context => { observations.Add(context); return true; });
        var atLongEnd = fixture.Production("LongEnd", new Epsilon());
        atLongEnd.Predicate = new GrammarPredicate("LongEnd", context => { observations.Add(context); return true; });
        int shortAccepted = AddRow(fixture.Table);
        int longAccepted = AddRow(fixture.Table);
        fixture.Table[fixture.AfterRegex].MatchedValueSet[fixture.Eof] = List(new ActionData(ActionDir.EpsilonReduce, atShortEnd));
        fixture.Table[fixture.AfterRegex].MatchedValueSet[atShortEnd.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, shortAccepted));
        fixture.Table[shortAccepted].MatchedValueSet[fixture.Eof] = List(new ActionData(ActionDir.Accept, fixture.Start));
        fixture.Table[fixture.AfterFlag].MatchedValueSet[fixture.Eof] = List(new ActionData(ActionDir.EpsilonReduce, atLongEnd));
        fixture.Table[fixture.AfterFlag].MatchedValueSet[atLongEnd.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, longAccepted));
        fixture.Table[longAccepted].MatchedValueSet[fixture.Eof] = List(new ActionData(ActionDir.Accept, fixture.Start));

        Assert.Throws<AmbiguousPredictionException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));

        Assert.Equal(2, observations.Count);
        var shortContext = Assert.Single(observations, context => context.TokenIndex == 3);
        var longContext = Assert.Single(observations, context => context.TokenIndex == 6);
        Assert.Equal("/a/g", shortContext.PreviousToken.Input);
        Assert.Equal("g", longContext.PreviousToken.Input);
        Assert.All(observations, context => Assert.True(context.IsEndOfInput));
        Assert.NotSame(shortContext.PreviousToken, longContext.PreviousToken);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(30)]
    [InlineData(150)]
    public void Long_input_does_not_accumulate_past_configurations_against_the_live_storage_limit(int shared)
    {
        var fixture = new Fixture(shared, longAccepts: false);
        var limits = new PredictionLimits(maximumConfigurations: 12);
        Assert.Same(fixture.GuardedReduce, fixture.Predictor(limits).SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
    }

    [Fact]
    public void No_surviving_origin_uses_the_candidates_token_ordinal_for_its_diagnostic()
    {
        var fixture = new Fixture(shortAccepts: false, longAccepts: false);
        // Both tokenizations are valid, but neither path has a table action for its own token.
        fixture.Table[fixture.ALast].MatchedValueSet.Remove(fixture.Regex);
        fixture.Table[fixture.BLast].MatchedValueSet.Remove(fixture.Slash);
        var error = Assert.Throws<PredictionFailedException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
        Assert.Equal(2, error.TokenIndex);
        Assert.Equal(1, error.State);
        Assert.Equal(2, error.SourceOffset);
    }

    [Fact]
    public void Failure_after_whitespace_points_at_the_lookahead_instead_of_the_previous_tokens_end()
    {
        var fixture = new Fixture();
        fixture.Table[fixture.ALast].MatchedValueSet.Remove(fixture.Regex);
        fixture.Table[fixture.BLast].MatchedValueSet.Remove(fixture.Slash);
        const string text = "xy \r\n /a/g";
        var input = fixture.RealInput(text);

        var error = Assert.Throws<PredictionFailedException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, input));

        Assert.Equal(text.IndexOf('/'), error.SourceOffset);
        Assert.Equal(2, error.TokenIndex);
        Assert.Equal(1, input.Cursor.SourceOffset);
    }

    [Fact]
    public void Ambiguity_after_trailing_comments_points_at_the_actual_end_of_the_source()
    {
        var fixture = new Fixture();
        const string text = "xy/a/g /* tail */\r\n";
        var error = Assert.Throws<AmbiguousPredictionException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.RealInput(text)));

        Assert.Equal(text.Length, error.SourceOffset);
        Assert.Equal(6, error.TokenIndex);
    }

    [Fact]
    public void An_eof_table_error_points_after_trailing_trivia()
    {
        var fixture = new Fixture();
        fixture.Table[fixture.AfterFlag].MatchedValueSet[fixture.Eof] =
            List(new ActionData(ActionDir.Shift, fixture.AfterFlag));
        const string text = "xy/a/g /* tail */\r\n";
        var error = Assert.Throws<InvalidPredictionTableException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.RealInput(text)));

        Assert.Equal(text.Length, error.SourceOffset);
        Assert.Equal(6, error.TokenIndex);
    }

    [Fact]
    public void A_work_limit_with_a_pending_token_points_after_its_leading_whitespace()
    {
        var fixture = new Fixture();
        const string text = "x   y/a/g";
        var input = fixture.RealInput(text);
        Assert.Equal(1, input.Cursor.SourceOffset);
        Assert.Equal(4, input.Pending.Token.StartIndex);
        var error = Assert.Throws<PredictionLimitException>(() => fixture.Predictor(
            new PredictionLimits(maximumTransitions: 1)).SelectFirstAction(fixture.Actions, new[] { 0, 1 }, input));

        Assert.Equal(4, error.SourceOffset);
        Assert.Equal(1, error.TokenIndex);
    }

    [Fact]
    public void A_limit_before_reading_lookahead_preserves_the_known_cursor_boundary()
    {
        var fixture = new Fixture();
        var input = fixture.RealInput("x   y/a/g", preloadLookahead: false);
        Assert.Null(input.Pending);
        var error = Assert.Throws<PredictionLimitException>(() => fixture.Predictor(
            new PredictionLimits(maximumStateDepth: 1)).SelectFirstAction(fixture.Actions, new[] { 0, 1 }, input));

        Assert.Equal(1, error.SourceOffset);
        Assert.Equal(1, error.TokenIndex);
        Assert.Null(input.Pending);
    }

    [Fact]
    public void A_callback_exception_is_not_a_failed_candidate_or_a_fallback_to_shift()
    {
        var fixture = new Fixture();
        var expected = new InvalidOperationException("predicate failure");
        var predictor = new ContextualConflictPredictor(fixture.Table, (action, context) =>
        {
            if (ReferenceEquals(action, fixture.GuardedReduce)) throw expected;
            return true;
        });
        Assert.Same(expected, Assert.Throws<InvalidOperationException>(() => predictor.SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _))));
    }

    [Fact]
    public void A_token_source_exception_is_not_hidden_by_the_other_origin()
    {
        var fixture = new Fixture();
        var input = fixture.Input(out var source);
        var expected = new InvalidOperationException("source failure");
        source.FailAt = call => call.Offset == 2 && ReferenceEquals(call.Goal, fixture.RegexGoal) ? expected : null;
        var actual = Assert.Throws<ContextualLexingException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, input));
        Assert.Same(expected, actual.InnerException);
    }

    [Fact]
    public void A_lexical_policy_exception_is_not_hidden_by_the_other_origin()
    {
        var fixture = new Fixture();
        var expected = new InvalidOperationException("goal failure");
        var input = fixture.Input(out _, context =>
        {
            if (context.State == fixture.ALast) throw expected;
            return context.Goals[0];
        });
        var actual = Assert.Throws<ContextualLexingException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, input));
        Assert.Same(expected, actual.InnerException);
    }

    [Fact]
    public void An_eof_shift_is_an_error_even_after_the_other_origin_has_already_accepted()
    {
        var fixture = new Fixture();
        fixture.Table[fixture.AfterFlag].MatchedValueSet[fixture.Eof] =
            List(new ActionData(ActionDir.Shift, fixture.AfterFlag));
        var error = Assert.Throws<InvalidPredictionTableException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
        Assert.Equal(6, error.TokenIndex);
        Assert.Equal(6, error.SourceOffset);
        Assert.Contains("EOF must not be shifted", error.Message);
    }

    [Fact]
    public void Accept_with_an_unconsumed_non_eof_token_is_a_table_error()
    {
        var fixture = new Fixture();
        fixture.Table[fixture.BLast].MatchedValueSet[fixture.Slash] = List(new ActionData(ActionDir.Accept, fixture.Start));
        var error = Assert.Throws<InvalidPredictionTableException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
        Assert.Contains("final EOF", error.Message);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10000)]
    public void Invalid_shift_destinations_are_diagnosed(int destination)
    {
        var fixture = new Fixture();
        var actions = new[] { new ActionData(ActionDir.Shift, destination), fixture.GuardedReduce };
        Assert.Throws<InvalidPredictionTableException>(() => fixture.Predictor().SelectFirstAction(
            actions, new[] { 0, 1 }, fixture.Input(out _)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Missing_or_conflicting_goto_is_an_error(bool conflicting)
    {
        var fixture = new Fixture();
        var target = fixture.GuardedReduce.Dest as NonTerminalSingle;
        if (conflicting)
            fixture.Table[1].MatchedValueSet[target.ToNonTerminal()].Add(new ActionData(ActionDir.Goto, fixture.ALast));
        else
            fixture.Table[1].MatchedValueSet.Remove(target.ToNonTerminal());
        var error = Assert.Throws<InvalidPredictionTableException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
        Assert.Contains("requires one Goto", error.Message);
    }

    [Fact]
    public void A_reduce_cannot_pop_the_initial_state()
    {
        var fixture = new Fixture();
        var invalid = fixture.Production("TooLong", fixture.X, fixture.Y, fixture.X, fixture.Y);
        fixture.Table[fixture.BLast].MatchedValueSet[fixture.Slash] = List(new ActionData(ActionDir.Reduce, invalid));
        var error = Assert.Throws<InvalidPredictionTableException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
        Assert.Contains("initial state", error.Message);
    }

    [Fact]
    public void A_non_consuming_cycle_cannot_be_hidden_by_an_already_accepted_origin()
    {
        var fixture = new Fixture();
        var loop = fixture.Production("Loop", fixture.Flag);
        fixture.Table[fixture.AfterFlag].MatchedValueSet[fixture.Eof] = List(new ActionData(ActionDir.Reduce, loop));
        fixture.Table[fixture.BeforeFlag].MatchedValueSet[loop.ToNonTerminal()] =
            List(new ActionData(ActionDir.Goto, fixture.AfterFlag));
        var error = Assert.Throws<PredictionCycleException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
        Assert.Equal(6, error.TokenIndex);
        Assert.Equal(6, error.SourceOffset);
    }

    [Fact]
    public void A_growing_non_consuming_path_hits_the_state_depth_limit()
    {
        var fixture = new Fixture();
        var grow = fixture.Production("Grow", new Epsilon());
        fixture.Table[fixture.AfterFlag].MatchedValueSet[fixture.Eof] = List(new ActionData(ActionDir.EpsilonReduce, grow));
        fixture.Table[fixture.AfterFlag].MatchedValueSet[grow.ToNonTerminal()] =
            List(new ActionData(ActionDir.Goto, fixture.AfterFlag));
        var error = Assert.Throws<PredictionLimitException>(() => fixture.Predictor(
            new PredictionLimits(maximumStateDepth: 12)).SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
        Assert.Contains("state-stack depth", error.Message);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Work_and_storage_limits_produce_diagnostics_instead_of_selecting_another_origin(bool work)
    {
        var fixture = new Fixture();
        var limits = work ? new PredictionLimits(maximumTransitions: 1) : new PredictionLimits(maximumConfigurations: 1);
        var error = Assert.Throws<PredictionLimitException>(() => fixture.Predictor(limits).SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
        Assert.Contains(work ? "transition" : "configuration", error.Message);
    }

    [Fact]
    public void A_later_ordinary_epsilon_conflict_keeps_the_existing_first_action_policy()
    {
        var fixture = new Fixture();
        var ordinary = fixture.Production("Ordinary", new Epsilon());
        fixture.Table[fixture.AfterFlag].MatchedValueSet[fixture.Eof].Add(new ActionData(ActionDir.EpsilonReduce, ordinary));
        // The extra action has no Goto. Exploring it would be a table error.
        Assert.Throws<AmbiguousPredictionException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
    }

    [Fact]
    public void A_three_way_cell_does_not_become_supported_after_filtering_a_false_guard()
    {
        var fixture = new Fixture();
        var deny = fixture.Production("Deny", new Epsilon());
        deny.Predicate = new GrammarPredicate("Deny", _ => false);
        var allow = fixture.Production("AllowLater", new Epsilon());
        allow.Predicate = new GrammarPredicate("AllowLater", _ => true);
        fixture.Table[fixture.BLast].MatchedValueSet[fixture.Slash].Add(new ActionData(ActionDir.EpsilonReduce, deny));
        fixture.Table[fixture.BLast].MatchedValueSet[fixture.Slash].Add(new ActionData(ActionDir.EpsilonReduce, allow));
        Assert.Throws<AmbiguousPredictionException>(() => fixture.Predictor().SelectFirstAction(
            fixture.Actions, new[] { 0, 1 }, fixture.Input(out _)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_later_supported_conflict_can_accept_twice_from_the_same_original_action(bool reverseNestedActions)
    {
        var fixture = new Fixture(longAccepts: false);
        var nestedGuard = fixture.Production("Nested", new Epsilon());
        nestedGuard.Predicate = new GrammarPredicate("Nested", _ => true);
        int nestedState = AddRow(fixture.Table);
        int secondAcceptedState = AddRow(fixture.Table);
        var directShift = fixture.Table[fixture.ALast].MatchedValueSet[fixture.Regex][0];
        var nestedReduce = new ActionData(ActionDir.EpsilonReduce, nestedGuard);
        fixture.Table[fixture.ALast].MatchedValueSet[fixture.Regex] = reverseNestedActions
            ? List(nestedReduce, directShift) : List(directShift, nestedReduce);
        fixture.Table[fixture.ALast].MatchedValueSet[nestedGuard.ToNonTerminal()] =
            List(new ActionData(ActionDir.Goto, nestedState));
        fixture.Table[nestedState].MatchedValueSet[fixture.Regex] =
            List(new ActionData(ActionDir.Shift, secondAcceptedState));
        var secondAccept = new ActionData(ActionDir.Accept, fixture.Start);
        fixture.Table[secondAcceptedState].MatchedValueSet[fixture.Eof] = List(secondAccept);
        var firstAccept = fixture.Table[fixture.AfterRegex].MatchedValueSet[fixture.Eof][0];
        var acceptedActions = new List<ActionData>();
        var predictor = new ContextualConflictPredictor(fixture.Table, (action, context) =>
        {
            if (action.Direction == ActionDir.Accept) acceptedActions.Add(action);
            return Evaluate(action, context);
        });
        var input = fixture.Input(out _, context =>
            context.State == fixture.ALast || context.State == fixture.AfterRegex ||
            context.State == nestedState || context.State == secondAcceptedState
                ? context.Goals[1] : context.Goals[0]);
        var cursor = input.Cursor;
        var history = input.History;
        var pending = input.Pending;

        var selected = predictor.SelectFirstAction(fixture.Actions, new[] { 0, 1 }, input);

        Assert.Same(fixture.GuardedReduce, selected);
        Assert.Collection(acceptedActions.OrderBy(action => ReferenceEquals(action, firstAccept) ? 0 : 1),
            first => Assert.Same(firstAccept, first),
            second => Assert.Same(secondAccept, second));
        Assert.Same(cursor, input.Cursor);
        Assert.Same(history, input.History);
        Assert.Same(pending, input.Pending);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Each_candidates_next_lexical_state_changes_its_later_token_classification(bool divisionReadsAsLetter)
    {
        var fixture = new Fixture();
        var input = fixture.Input(out var source);
        source.ReadLettersUsingLexicalState = true;
        source.StateAfterDivisionSlash = divisionReadsAsLetter ? "div" : "regexp";
        var originalCursor = input.Cursor;
        var originalPending = input.Pending;

        if (divisionReadsAsLetter)
        {
            // Both independently preserved states read a complete path and reach EOF.
            Assert.Throws<AmbiguousPredictionException>(() => fixture.Predictor().SelectFirstAction(
                fixture.Actions, new[] { 0, 1 }, input));
            Assert.Contains(source.Calls, call => call.Offset == source.Length && call.Ordinal == 6 && call.LexicalState == "div");
        }
        else
        {
            // In this source state, the physical 'a' is classified as Flag rather than Letter.
            // Only the division candidate fails; the regexp candidate retains its own state.
            Assert.Same(fixture.GuardedReduce, fixture.Predictor().SelectFirstAction(
                fixture.Actions, new[] { 0, 1 }, input));
        }

        Assert.Contains(source.Calls, call => call.Offset == 3 &&
            call.LexicalState == (divisionReadsAsLetter ? "div" : "regexp"));
        Assert.Contains(source.Calls, call => call.Offset == source.Length && call.Ordinal == 3 && call.LexicalState == "regexp");
        Assert.Same(originalCursor, input.Cursor);
        Assert.Equal("div", input.Cursor.LexicalState);
        Assert.Same(originalPending, input.Pending);
    }

    [Fact]
    public void One_predictor_can_be_reused_with_independent_inputs_and_budgets_concurrently()
    {
        var fixture = new Fixture(longAccepts: false);
        var predictor = fixture.Predictor(new PredictionLimits(maximumTransitions: 20));
        Parallel.For(0, 24, iteration => Assert.Same(fixture.GuardedReduce,
            predictor.SelectFirstAction(fixture.Actions, new[] { 0, 1 }, fixture.Input(out _))));
    }

    [Fact]
    public void Invalid_calls_are_rejected_before_the_source_is_read()
    {
        var fixture = new Fixture();
        var input = fixture.Input(out var source);
        int readCount = source.Calls.Count;
        var predictor = fixture.Predictor();
        Assert.Throws<ArgumentNullException>(() => new ContextualConflictPredictor(null, Evaluate));
        Assert.Throws<ArgumentNullException>(() => new ContextualConflictPredictor(fixture.Table, null));
        Assert.Throws<ArgumentException>(() => predictor.SelectFirstAction(new[] { fixture.Shift }, new[] { 0, 1 }, input));
        Assert.Throws<ArgumentNullException>(() => predictor.SelectFirstAction(fixture.Actions, null, input));
        Assert.Throws<ArgumentException>(() => predictor.SelectFirstAction(fixture.Actions, Array.Empty<int>(), input));
        Assert.Throws<ArgumentException>(() => predictor.SelectFirstAction(fixture.Actions, new[] { 0, 10000 }, input));
        Assert.Throws<ArgumentNullException>(() => predictor.SelectFirstAction(fixture.Actions, new[] { 0, 1 }, null));
        Assert.Equal(readCount, source.Calls.Count);
    }

    private static bool Evaluate(ActionData action, PredicateContext context)
        => action.Direction != ActionDir.EpsilonReduce || !(action.Dest is NonTerminalSingle production) ||
           production.Predicate == null || production.Predicate.Evaluate(context);

    private static ActionDataList List(params ActionData[] actions)
    {
        var result = new ActionDataList();
        result.AddRange(actions);
        return result;
    }
    private static int AddRow(LRParsingTable table)
    {
        int result = table.Count;
        table.Add(new LRParsingRowDataFormat(null, new ActionDicSymbolMatched()));
        return result;
    }
    private static ActionData[] Snapshot(LRParsingTable table)
        => table.SelectMany(row => row.MatchedValueSet.Values).SelectMany(actions => actions).ToArray();

    private sealed class Fixture
    {
        private readonly KeyManager _keys = new KeyManager();
        private readonly int _shared;
        public readonly LRParsingTable Table = new LRParsingTable();
        public readonly Terminal X, Y, Slash, Letter, Flag, Regex;
        public readonly Terminal Eof = new EndMarker();
        public readonly ActionData Shift, GuardedReduce;
        public readonly ActionData[] Actions;
        public readonly NonTerminalSingle Start;
        public readonly int ALast, BLast, AfterRegex, AfterFlag, BeforeFlag;
        public readonly LexicalGoal DivGoal, RegexGoal;

        public Fixture(int shared = 1, bool shortAccepts = true, bool longAccepts = true, bool allow = true)
        {
            _shared = shared;
            X = Terminal("x"); Y = Terminal("y"); Slash = Terminal("/");
            Letter = Terminal("a"); Flag = Terminal("g"); Regex = Terminal("/a/g");
            var guard = Production("Guard", new Epsilon());
            guard.Predicate = new GrammarPredicate("Allow", _ => allow);
            Start = Production("S", X);
            ALast = 2 * shared + 1;
            BLast = shared + 1;
            int guardState = 2 * shared + 4;
            AfterRegex = guardState + 1;
            int afterSlash = guardState + 2;
            int afterLetter = guardState + 3;
            BeforeFlag = guardState + 4;
            AfterFlag = guardState + 5;
            for (int i = 0; i <= AfterFlag; i++) AddRow(Table);
            Shift = new ActionData(ActionDir.Shift, 2);
            GuardedReduce = new ActionData(ActionDir.EpsilonReduce, guard);
            Actions = new[] { Shift, GuardedReduce };
            Table[0].MatchedValueSet[X] = List(new ActionData(ActionDir.Shift, 1));
            Table[1].MatchedValueSet[Y] = List(Actions);
            Table[1].MatchedValueSet[guard.ToNonTerminal()] = List(new ActionData(ActionDir.Goto, guardState));
            Table[guardState].MatchedValueSet[Y] = List(new ActionData(ActionDir.Shift, shared + 2));
            for (int i = 2; i < BLast; i++) Table[i].MatchedValueSet[Y] = List(new ActionData(ActionDir.Shift, i + 1));
            for (int i = shared + 2; i < ALast; i++) Table[i].MatchedValueSet[Y] = List(new ActionData(ActionDir.Shift, i + 1));
            Table[ALast].MatchedValueSet[Regex] = List(new ActionData(ActionDir.Shift, AfterRegex));
            Table[AfterRegex].MatchedValueSet[Eof] = List(new ActionData(shortAccepts ? ActionDir.Accept : ActionDir.Failed, Start));
            Table[BLast].MatchedValueSet[Slash] = List(new ActionData(ActionDir.Shift, afterSlash));
            Table[afterSlash].MatchedValueSet[Letter] = List(new ActionData(ActionDir.Shift, afterLetter));
            Table[afterLetter].MatchedValueSet[Slash] = List(new ActionData(ActionDir.Shift, BeforeFlag));
            Table[BeforeFlag].MatchedValueSet[Flag] = List(new ActionData(ActionDir.Shift, AfterFlag));
            Table[AfterFlag].MatchedValueSet[Eof] = List(new ActionData(longAccepts ? ActionDir.Accept : ActionDir.Failed, Start));
            DivGoal = new LexicalGoal(new[] { X, Y, Slash, Letter, Flag });
            RegexGoal = new LexicalGoal(new[] { X, Y, Regex });
        }

        public Terminal Terminal(string text)
        {
            var terminal = new Terminal(TokenType.Operator, text);
            _keys.AllocateUniqueKey(terminal);
            return terminal;
        }
        public NonTerminalSingle Production(string name, params Symbol[] symbols)
        {
            var head = new NonTerminal(name);
            _keys.AllocateUniqueKey(head);
            head.Add(symbols);
            return head[0];
        }
        public ContextualConflictPredictor Predictor(PredictionLimits limits = null)
            => new ContextualConflictPredictor(Table, Evaluate, limits);
        public ContextualInput RealInput(string text, bool preloadLookahead = true)
        {
            var lexer = new Lexer();
            foreach (var terminal in new[] { X, Y, Slash, Letter, Flag, Regex }) lexer.AddTokenRule(terminal);
            foreach (var value in new[] { " ", "\t", "\r", "\n" })
                lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Delimiter, value));
            lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment, @"//[^\r\n]*|/\*[\s\S]*?\*/"));
            var policy = new LexicalPolicy(new[] { DivGoal, RegexGoal }, context =>
                context.State == ALast || context.State == AfterRegex ? context.Goals[1] : context.Goals[0]);
            var source = lexer.CreateTokenSource(text, policy);
            var input = new ContextualInput(source);
            input.Read(new[] { 0 }, Table);
            input.Shift();
            if (preloadLookahead) input.Read(new[] { 0, 1 }, Table);
            return input;
        }
        public ContextualInput Input(out MockSource source, Func<LexicalContext, LexicalGoal> selector = null,
            bool preloadLookahead = true)
        {
            var policy = new LexicalPolicy(new[] { DivGoal, RegexGoal }, selector ?? (context =>
                context.State == ALast || context.State == AfterRegex ? context.Goals[1] : context.Goals[0]));
            source = new MockSource(this, "x" + new string('y', _shared) + "/a/g", policy);
            var input = new ContextualInput(source);
            input.Read(new[] { 0 }, Table);
            input.Shift();
            if (preloadLookahead) input.Read(new[] { 0, 1 }, Table);
            return input;
        }
    }

    private sealed class MockSource : ITokenSource
    {
        private readonly Fixture _fixture;
        private readonly string _text;
        public TokenCursor Start { get; }
        public int Length => _text.Length;
        public LexicalPolicy Policy { get; }
        public readonly List<ReadCall> Calls = new List<ReadCall>();
        public Func<ReadCall, Exception> FailAt;
        public bool ReadLettersUsingLexicalState;
        public string StateAfterDivisionSlash = "div";
        public MockSource(Fixture fixture, string text, LexicalPolicy policy)
        {
            _fixture = fixture;
            _text = text;
            Start = new TokenCursor(this, 0);
            Policy = policy;
        }
        public TokenRead Peek(TokenCursor cursor, LexicalGoal goal)
        {
            var call = new ReadCall(cursor.SourceOffset, cursor.TokenIndex, goal, cursor.LexicalState);
            Calls.Add(call);
            var failure = FailAt?.Invoke(call);
            if (failure != null) throw failure;
            Terminal terminal;
            int length;
            if (cursor.SourceOffset == Length) { terminal = _fixture.Eof; length = 0; }
            else if (_text[cursor.SourceOffset] == '/' && ReferenceEquals(goal, _fixture.RegexGoal))
            { terminal = _fixture.Regex; length = 4; }
            else
            {
                terminal = _text[cursor.SourceOffset] switch
                {
                    'x' => _fixture.X,
                    'y' => _fixture.Y,
                    '/' => _fixture.Slash,
                    'a' => !ReadLettersUsingLexicalState || cursor.LexicalState == "div" ? _fixture.Letter : _fixture.Flag,
                    'g' => _fixture.Flag,
                    _ => throw new InvalidOperationException("Unexpected fixture input")
                };
                length = 1;
            }
            var token = new TokenData(terminal,
                new TokenCell(cursor.SourceOffset, 0, cursor.SourceOffset, _text.Substring(cursor.SourceOffset, length), null));
            string nextState = ReferenceEquals(goal, _fixture.RegexGoal) ? "regexp" : "div";
            if (length == 1 && ReferenceEquals(terminal, _fixture.Slash)) nextState = StateAfterDivisionSlash;
            var next = new TokenCursor(this, cursor.SourceOffset + length,
                cursor.TokenIndex + (length == 0 ? 0 : 1), length == 0 ? cursor.PreviousToken : token,
                length == 0 ? cursor.LexicalState : nextState);
            return new TokenRead(token, next);
        }
    }

    private sealed class ReadCall
    {
        public readonly int Offset, Ordinal;
        public readonly LexicalGoal Goal;
        public readonly string LexicalState;
        public ReadCall(int offset, int ordinal, LexicalGoal goal, string lexicalState)
        {
            Offset = offset; Ordinal = ordinal; Goal = goal; LexicalState = lexicalState;
        }
    }
}
