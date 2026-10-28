using System;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.RegularGrammar;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// An empty sequence and an explicit epsilon both describe empty input.
/// A nonproductive symbol has an empty FIRST set, rather than epsilon.
/// Both concatenation query APIs must preserve that distinction.
/// </summary>
[Trait("Category", "Common")]
[Trait("Feature", "FirstEmptySequence")]
public sealed class FirstEmptySequenceTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Empty_sequence_contains_exactly_epsilon(bool calculateRecursively)
    {
        var analyzer = new FirstFollowAnalyzer();
        var sequence = new NonTerminalConcat();

        var result = Query(analyzer, sequence, calculateRecursively);

        AssertOnly(result, new Epsilon());
        Assert.Empty(sequence);
    }

    [Theory]
    [InlineData(false, 1)]
    [InlineData(false, 2)]
    [InlineData(false, 4)]
    [InlineData(true, 1)]
    [InlineData(true, 2)]
    [InlineData(true, 4)]
    public void Explicit_epsilon_and_empty_sequence_have_identical_first_sets(
        bool calculateRecursively, int epsilonCount)
    {
        var analyzer = new FirstFollowAnalyzer();
        var explicitEpsilon = new NonTerminalConcat();
        for (int i = 0; i < epsilonCount; i++)
            explicitEpsilon.Add(new Epsilon());

        var emptyFirst = Query(analyzer, new NonTerminalConcat(), calculateRecursively);
        var explicitFirst = Query(analyzer, explicitEpsilon, calculateRecursively);

        AssertOnly(emptyFirst, new Epsilon());
        AssertOnly(explicitFirst, new Epsilon());
        Assert.True(emptyFirst.SetEquals(explicitFirst));
        Assert.Equal(epsilonCount, explicitEpsilon.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Empty_sequence_queries_return_independent_sets(bool calculateRecursively)
    {
        var analyzer = new FirstFollowAnalyzer();
        var sequence = new NonTerminalConcat();
        var first = Query(analyzer, sequence, calculateRecursively);
        var second = Query(analyzer, sequence, calculateRecursively);

        Assert.NotSame(first, second);
        first.Clear();

        AssertOnly(second, new Epsilon());
        AssertOnly(Query(analyzer, sequence, calculateRecursively), new Epsilon());
        Assert.Empty(sequence);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Terminal_sequence_does_not_contain_epsilon(bool calculateRecursively)
    {
        var analyzer = new FirstFollowAnalyzer();
        var token = new Terminal(TokenType.Operator, "a");
        var sequence = new NonTerminalConcat(token);

        var result = Query(analyzer, sequence, calculateRecursively);

        AssertOnly(result, token);
        Assert.DoesNotContain(new Epsilon(), result);
    }

    [Theory]
    [InlineData(false, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public void Explicit_epsilon_does_not_change_the_first_token_of_a_nonempty_sequence(
        bool calculateRecursively, bool epsilonBefore, bool epsilonAfter)
    {
        var analyzer = new FirstFollowAnalyzer();
        var token = new Terminal(TokenType.Operator, "a");
        var sequence = new NonTerminalConcat();
        if (epsilonBefore)
            sequence.Add(new Epsilon());
        sequence.Add(token);
        if (epsilonAfter)
            sequence.Add(new Epsilon());

        var result = Query(analyzer, sequence, calculateRecursively);

        AssertOnly(result, token);
        Assert.DoesNotContain(new Epsilon(), result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Nullable_nonterminal_chain_has_the_same_first_as_empty_input(bool calculateRecursively)
    {
        var grammar = new SequenceGrammar();
        var analyzer = Analyze(grammar);

        var result = Query(analyzer, new NonTerminalConcat(grammar.NullableHead), calculateRecursively);

        AssertOnly(result, new Epsilon());
        Assert.True(result.SetEquals(Query(analyzer, new NonTerminalConcat(), calculateRecursively)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Empty_rhs_and_explicit_epsilon_productions_have_the_same_first(bool calculateRecursively)
    {
        var grammar = new SequenceGrammar();
        var analyzer = Analyze(grammar);

        var emptyRuleFirst = Query(analyzer, new NonTerminalConcat(grammar.EmptyProduction), calculateRecursively);
        var explicitRuleFirst = Query(analyzer, new NonTerminalConcat(grammar.NullableTail), calculateRecursively);

        AssertOnly(emptyRuleFirst, new Epsilon());
        AssertOnly(explicitRuleFirst, new Epsilon());
        Assert.True(emptyRuleFirst.SetEquals(explicitRuleFirst));
        // The public cached-production lookup must agree with both query APIs.
        AssertOnly(analyzer.First(grammar.EmptyProduction[0]), new Epsilon());
        AssertOnly(analyzer.First(grammar.NullableTail[0]), new Epsilon());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Nonproductive_symbol_is_not_confused_with_empty_input(bool calculateRecursively)
    {
        var grammar = new SequenceGrammar();
        var analyzer = Analyze(grammar);

        var nonproductiveFirst = Query(analyzer, new NonTerminalConcat(grammar.Unproductive), calculateRecursively);
        var emptyFirst = Query(analyzer, new NonTerminalConcat(), calculateRecursively);

        Assert.Empty(nonproductiveFirst);
        Assert.DoesNotContain(new Epsilon(), nonproductiveFirst);
        AssertOnly(emptyFirst, new Epsilon());
        Assert.False(nonproductiveFirst.SetEquals(emptyFirst));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Nullable_prefix_does_not_make_a_nonproductive_suffix_nullable(bool calculateRecursively)
    {
        var grammar = new SequenceGrammar();
        var analyzer = Analyze(grammar);
        var sequence = new NonTerminalConcat(grammar.NullableHead, grammar.Unproductive);

        var result = Query(analyzer, sequence, calculateRecursively);

        Assert.Empty(result);
        Assert.DoesNotContain(new Epsilon(), result);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Terminal_prefix_keeps_its_first_before_a_nullable_suffix(bool calculateRecursively)
    {
        var grammar = new SequenceGrammar();
        var analyzer = Analyze(grammar);
        var sequence = new NonTerminalConcat(grammar.Token, grammar.NullableHead);

        var result = Query(analyzer, sequence, calculateRecursively);

        AssertOnly(result, grammar.Token);
        Assert.DoesNotContain(new Epsilon(), result);
    }

    private static TerminalSet Query(FirstFollowAnalyzer analyzer, NonTerminalConcat sequence, bool calculateRecursively)
        => calculateRecursively ? analyzer.FirstSet(sequence) : analyzer.First(sequence);

    private static FirstFollowAnalyzer Analyze(SequenceGrammar grammar)
    {
        var analyzer = new FirstFollowAnalyzer();
        analyzer.CalculateAllFirst(grammar.NonTerminalMultiples);
        return analyzer;
    }

    private static void AssertOnly(TerminalSet actual, Terminal expected)
    {
        Assert.Single(actual);
        Assert.Contains(expected, actual);
    }

    private sealed class SequenceGrammar : Grammar
    {
        public override NonTerminal EbnfRoot => StartSymbol;
        internal NonTerminal NullableHead { get; } = new NonTerminal("G");
        internal NonTerminal NullableTail { get; } = new NonTerminal("N");
        internal NonTerminal EmptyProduction { get; } = new NonTerminal("E");
        internal NonTerminal Unproductive { get; } = new NonTerminal("U");
        internal Terminal Token { get; } = new Terminal(TokenType.Operator, "a");

        internal SequenceGrammar()
        {
            var start = new NonTerminal("S", true);
            foreach (var nonterminal in new[] { start, NullableHead, NullableTail, EmptyProduction, Unproductive })
            {
                keyManager.AllocateUniqueKey(nonterminal);
                NonTerminalMultiples.Add(nonterminal);
            }
            keyManager.AllocateUniqueKey(Token);
            TerminalSet.Add(Token);
            SetStartSymbol(start);
            start.Add(Token);
            NullableHead.Add(NullableTail);
            NullableTail.Add(new Epsilon());
            EmptyProduction.Add(Array.Empty<Symbol>());
            Unproductive.Add(Unproductive);
        }
    }
}
