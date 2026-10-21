using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Goals are registered once with a policy. Sources retain that immutable configuration,
/// while each parse owns its input progress and consumed-token history.
/// </summary>
[Trait("Category", "ContextualLexing")]
public class LexicalPolicyTests
{
    [Fact]
    public void Policy_rejects_null_goal_collections_in_both_constructor_forms()
    {
        Assert.Throws<ArgumentNullException>(() => new LexicalPolicy((LexicalGoal[])null));
        Assert.Throws<ArgumentNullException>(() => new LexicalPolicy((IEnumerable<LexicalGoal>)null, _ => null));
    }

    [Fact]
    public void Policy_requires_at_least_one_goal_before_a_source_is_created()
    {
        Assert.Throws<ArgumentException>(() => new LexicalPolicy());
        Assert.Throws<ArgumentException>(() => new LexicalPolicy(Enumerable.Empty<LexicalGoal>(), _ => null));
    }

    [Fact]
    public void Policy_rejects_null_goals_in_both_constructor_forms()
    {
        var goal = new LexicalGoal(Array.Empty<Terminal>());
        Assert.Throws<ArgumentException>(() => new LexicalPolicy(goal, null));
        Assert.Throws<ArgumentException>(() => new LexicalPolicy(new[] { goal, null }, _ => goal));
    }

    [Fact]
    public void Registering_one_goal_instance_twice_is_rejected_in_both_constructor_forms()
    {
        var goal = new LexicalGoal(Array.Empty<Terminal>());
        Assert.Throws<ArgumentException>(() => new LexicalPolicy(goal, goal));
        Assert.Throws<ArgumentException>(() => new LexicalPolicy(new[] { goal, goal }, _ => goal));
    }

    [Fact]
    public void Distinct_goal_instances_can_share_the_same_terminals()
    {
        var token = new Terminal(TokenType.Operator, "x", false);
        var first = new LexicalGoal(new[] { token });
        var second = new LexicalGoal(new[] { token });
        var policy = new LexicalPolicy(first, second);

        Assert.Same(first, policy.Goals[0]);
        Assert.Same(second, policy.Goals[1]);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Changing_the_callers_collection_cannot_replace_or_reorder_registered_goals(bool useSelector)
    {
        var first = new LexicalGoal(Array.Empty<Terminal>());
        var second = new LexicalGoal(Array.Empty<Terminal>());
        var supplied = new List<LexicalGoal> { first, second };
        var policy = useSelector
            ? new LexicalPolicy(supplied, _ => first)
            : new LexicalPolicy(supplied);
        supplied.Reverse();
        supplied.Clear();

        Assert.Equal(2, policy.Goals.Count);
        Assert.Same(first, policy.Goals[0]);
        Assert.Same(second, policy.Goals[1]);
        Assert.Throws<NotSupportedException>(() => ((IList<LexicalGoal>)policy.Goals)[0] = second);
    }

    [Fact]
    public void A_single_policy_can_be_reused_for_independent_sources_and_repeated_parses()
    {
        var read = EbnfGrammarReader.Read("S : id ';' ; id := Identifier \"[a-z]+\" ;");
        Assert.True(read.Success, string.Join("; ", read.Errors));
        var lexer = new Lexer();
        foreach (var terminal in read.Grammar.TerminalSet) lexer.AddTokenRule(terminal);
        var goal = new LexicalGoal(read.Grammar.TerminalSet.Where(t => t is not EndMarker && t is not Epsilon));
        var requests = new List<LexicalContext>();
        var policy = new LexicalPolicy(new[] { goal }, context =>
        {
            requests.Add(context);
            return goal;
        });
        var first = lexer.CreateTokenSource("one;", policy);
        var second = lexer.CreateTokenSource("two;", policy);
        var parser = new LALRParser(read.Grammar, true);

        foreach (var source in new[] { first, second, first })
        {
            requests.Clear();
            var result = parser.Parsing(source);
            Assert.True(result.Success, parser.FiredException?.ToString());
            Assert.Same(policy, source.Policy);
            Assert.Equal(3, requests.Count); // identifier, semicolon, EOF
            Assert.All(requests, context =>
            {
                Assert.Same(policy.Goals, context.Goals);
                Assert.Same(source.Start.SourceIdentity, context.Cursor.SourceIdentity);
            });
            Assert.Equal(new[] { 0, 1, 2 }, requests.Select(context => context.ConsumedTokens.Count));
            Assert.Equal(0, requests[0].SourceOffset);
            Assert.Null(requests[0].PreviousToken);
            var shifted = result.Trace.Steps.Where(step => step.Action is ParseAction.Shift)
                .Select(step => step.Lookahead.Input).ToArray();
            Assert.Equal(source == second ? new[] { "two", ";" } : new[] { "one", ";" }, shifted);
        }

        Assert.NotSame(first.Start.SourceIdentity, second.Start.SourceIdentity);
        Assert.Throws<ArgumentException>(() => second.Peek(first.Start, goal));
    }

    [Fact]
    public void An_explicit_null_selector_uses_the_default_selection_logic()
    {
        var read = EbnfGrammarReader.Read("S : 'x' ;");
        Assert.True(read.Success, string.Join("; ", read.Errors));
        var lexer = new Lexer();
        foreach (var terminal in read.Grammar.TerminalSet) lexer.AddTokenRule(terminal);
        var goal = new LexicalGoal(read.Grammar.TerminalSet.Where(t => t is not EndMarker && t is not Epsilon));
        var policy = new LexicalPolicy(new[] { goal }, null);
        var source = lexer.CreateTokenSource("x", policy);
        var parser = new LALRParser(read.Grammar, true);

        Assert.True(parser.Parsing(source).Success, parser.FiredException?.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    public void A_unique_goal_is_selected_from_three_goals_regardless_of_registration_position(int position)
    {
        var grammar = GrammarWithThreeTokens();
        var lexer = CreateLexer(grammar);
        var x = new LexicalGoal(new[] { FindTerminal(grammar, "x") });
        var y = new LexicalGoal(new[] { FindTerminal(grammar, "y") });
        var z = new LexicalGoal(new[] { FindTerminal(grammar, "z") });
        var goals = new List<LexicalGoal> { y, z };
        goals.Insert(position, x);
        var policy = new LexicalPolicy(goals);
        var source = new RecordingSource(lexer.CreateTokenSource("x", policy));
        var parser = new LALRParser(grammar, true);

        var result = parser.Parsing(source);

        Assert.True(result.Success, parser.FiredException?.ToString());
        Assert.Equal(2, source.RequestedGoals.Count); // x and EOF
        Assert.Same(x, source.RequestedGoals[0]);
        Assert.Same(goals[0], source.RequestedGoals[1]);
    }

    [Fact]
    public void Two_matching_goals_among_three_require_a_selector_before_the_source_is_read()
    {
        var grammar = GrammarWithThreeTokens();
        var lexer = CreateLexer(grammar);
        var x = FindTerminal(grammar, "x");
        var y = FindTerminal(grammar, "y");
        var z = FindTerminal(grammar, "z");
        var first = new LexicalGoal(new[] { x, y });
        var second = new LexicalGoal(new[] { x, z });
        var third = new LexicalGoal(new[] { y, z });
        var goals = new[] { first, second, third };
        var source = new RecordingSource(lexer.CreateTokenSource("x", new LexicalPolicy(goals)));
        var parser = new LALRParser(grammar, true);

        var failed = parser.Parsing(source);

        Assert.False(failed.Success);
        var error = Assert.IsType<ContextualLexingException>(parser.FiredException);
        Assert.Equal(0, error.SourceOffset);
        Assert.Equal(0, error.TokenIndex);
        Assert.Empty(source.RequestedGoals);

        var selectedSource = new RecordingSource(lexer.CreateTokenSource("x",
            new LexicalPolicy(goals, _ => second)));
        var success = parser.Parsing(selectedSource);

        Assert.True(success.Success, parser.FiredException?.ToString());
        Assert.Equal(2, selectedSource.RequestedGoals.Count);
        Assert.All(selectedSource.RequestedGoals, goal => Assert.Same(second, goal));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Common_only_expectations_use_the_first_registered_goal(bool reverseOrder)
    {
        var grammar = GrammarWithThreeTokens();
        var lexer = CreateLexer(grammar);
        var x = FindTerminal(grammar, "x");
        var first = new LexicalGoal(new[] { x, FindTerminal(grammar, "y") });
        var second = new LexicalGoal(new[] { x, FindTerminal(grammar, "z") });
        var goals = reverseOrder ? new[] { second, first } : new[] { first, second };
        var policy = new LexicalPolicy(goals);
        var source = new RecordingSource(lexer.CreateTokenSource("x", policy));
        var parser = new LALRParser(grammar, true);

        var result = parser.Parsing(source);

        Assert.True(result.Success, parser.FiredException?.ToString());
        Assert.Equal(2, source.RequestedGoals.Count);
        Assert.All(source.RequestedGoals, goal => Assert.Same(goals[0], goal));
    }

    private static EbnfGrammar GrammarWithThreeTokens()
    {
        var read = EbnfGrammarReader.Read("""
            S : x ;
            x := StringLiteral "x" ;
            y := StringLiteral "y" ;
            z := StringLiteral "z" ;
            """);
        Assert.True(read.Success, string.Join("; ", read.Errors));
        return read.Grammar;
    }

    private static Terminal FindTerminal(EbnfGrammar grammar, string caption)
        => Assert.Single(grammar.TerminalSet, terminal => terminal.Caption == caption);

    private static Lexer CreateLexer(EbnfGrammar grammar)
    {
        var lexer = new Lexer();
        foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
        return lexer;
    }

    private sealed class RecordingSource : ITokenSource
    {
        private readonly ITokenSource _inner;
        public TokenCursor Start => _inner.Start;
        public int Length => _inner.Length;
        public LexicalPolicy Policy => _inner.Policy;
        public List<LexicalGoal> RequestedGoals { get; } = new();

        public RecordingSource(ITokenSource inner) => _inner = inner;

        public TokenRead Peek(TokenCursor start, LexicalGoal goal)
        {
            RequestedGoals.Add(goal);
            return _inner.Peek(start, goal);
        }
    }
}
