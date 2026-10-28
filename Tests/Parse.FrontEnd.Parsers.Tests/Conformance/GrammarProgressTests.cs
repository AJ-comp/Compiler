using System;
using System.Linq;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

public class GrammarProgressTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Native_nullable_repetition_is_rejected_before_first_follow(bool plus, bool guarded)
    {
        var calls = 0;
        var grammar = new ProgressGrammar(g =>
        {
            g.A.Add(new NonTerminalConcat(new Epsilon())
            {
                Predicate = guarded ? new GrammarPredicate("Allow", _ => { calls++; return true; }) : null
            });
            g.Start.Add(plus ? g.A.OneOrMore() : g.A.ZeroOrMore(), g.Y);
        });

        var error = Assert.Throws<GrammarProgressException>(() => new LALRParser(grammar));

        Assert.Contains("each iteration", error.Message);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Optional_token_as_a_repetition_operand_is_nullable(bool plus)
    {
        var grammar = new ProgressGrammar(g =>
        {
            var optional = g.X.Optional();
            g.Start.Add(plus ? optional.OneOrMore() : optional.ZeroOrMore(), g.Y);
        });

        Assert.Throws<GrammarProgressException>(() => grammar.Optimization());
    }

    [Fact]
    public void Native_indirect_nullable_repetition_is_checked_after_all_rules_are_defined()
    {
        var grammar = new ProgressGrammar(g =>
        {
            g.Start.Add(g.A.ZeroOrMore(), g.Y);
            g.A.Add(g.B);
            g.B.Add(new Epsilon());
        });

        Assert.Throws<GrammarProgressException>(() => grammar.Optimization());
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Guarded_recursion_that_can_return_without_a_token_is_rejected(bool indirect, bool nullableBase)
    {
        var calls = 0;
        var grammar = new ProgressGrammar(g =>
        {
            g.Guard.Add(new NonTerminalConcat(new Epsilon())
            {
                Predicate = new GrammarPredicate("Allow", _ => { calls++; return false; })
            });
            g.Start.Add(g.A, g.Y);
            g.A.Add(nullableBase ? (Symbol)g.Guard : g.X);
            if (indirect)
            {
                g.A.Add(g.B);
                g.B.Add(g.Guard, g.A);
            }
            else g.A.Add(g.Guard, g.A);
        });

        var error = Assert.Throws<GrammarProgressException>(() => new LALRParser(grammar));

        Assert.Contains("non-consuming predicate cycle", error.Message);
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Unused_predicate_cycle_is_checked_because_first_follow_visits_all_rules()
    {
        var grammar = new ProgressGrammar(g =>
        {
            g.Start.Add(g.Y);
            g.Guard.Add(new NonTerminalConcat(new Epsilon()) { Predicate = new GrammarPredicate("Allow", _ => true) });
            g.A.Add(g.Guard);
            g.A.Add(g.A, g.Guard);
        });

        Assert.Throws<GrammarProgressException>(() => new LALRParser(grammar));
    }

    [Theory]
    [InlineData(false, "y")]
    [InlineData(false, "x x y")]
    [InlineData(true, "x y")]
    [InlineData(true, "x x x y")]
    public void Native_repetition_with_a_required_token_and_guard_is_allowed(bool plus, string input)
    {
        var grammar = new ProgressGrammar(g =>
        {
            g.Guard.Add(new NonTerminalConcat(new Epsilon()) { Predicate = new GrammarPredicate("Allow", _ => true) });
            g.A.Add(g.X, g.Guard);
            g.Start.Add(plus ? g.A.OneOrMore() : g.A.ZeroOrMore(), g.Y);
        });

        var parser = new LALRParser(grammar, true);
        var result = parser.Parsing(Lex(grammar, input).TokensForParsing);

        Assert.True(result.Success, parser.FiredException?.Message);
        Assert.Equal(input.Split(' '), result.ToParseTree.AllTokens.Select(token => token.Input));
    }

    [Theory]
    [InlineData(false, "y")]
    [InlineData(false, "x x y")]
    [InlineData(true, "y")]
    [InlineData(true, "x x y")]
    public void Nullable_base_preserves_left_recursion_only_when_the_guard_follows_recursive_entry(bool leadingGuard, string input)
    {
        var grammar = new ProgressGrammar(g =>
        {
            g.Guard.Add(new NonTerminalConcat(new Epsilon()) { Predicate = new GrammarPredicate("Allow", _ => true) });
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.Guard);
            if (leadingGuard) g.A.Add(g.Guard, g.A, g.X);
            else g.A.Add(g.A, g.Guard, g.X);
        });

        // The suffix x makes the whole alternative consuming, but a leading guard
        // still lies on the zero-token path into the recursive A.
        if (leadingGuard)
        {
            Assert.Throws<PredicateLeftRecursionException>(() => new LALRParser(grammar, true));
            return;
        }

        var parser = new LALRParser(grammar, true);
        var analyzer = new FirstFollowAnalyzer();
        analyzer.CalculateAllFirst(grammar.NonTerminalMultiples);
        Assert.Contains(grammar.X, analyzer.First(grammar.A));
        Assert.True(analyzer.First(grammar.A).IsNullAble);

        var result = parser.Parsing(Lex(grammar, input).TokensForParsing);

        Assert.True(result.Success, parser.FiredException?.Message);
    }

    private static LexingData Lex(Grammar grammar, string input)
    {
        var lexer = new Lexer();
        foreach (var token in grammar.TerminalSet) lexer.AddTokenRule(token);
        return lexer.Lexing(input);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void First_cache_does_not_mix_nullable_and_token_consuming_alternatives(bool recursiveFirst)
    {
        var grammar = new ProgressGrammar(g =>
        {
            g.Guard.Add(new NonTerminalConcat(new Epsilon()) { Predicate = new GrammarPredicate("Allow", _ => true) });
            g.Start.Add(g.A, g.Y);
            if (recursiveFirst) g.A.Add(g.A, g.Guard, g.X);
            g.A.Add(g.Guard);
            if (!recursiveFirst) g.A.Add(g.A, g.Guard, g.X);
        });
        grammar.Optimization();
        var analyzer = new FirstFollowAnalyzer();
        analyzer.CalculateAllFirst(grammar.NonTerminalMultiples);

        var nullableAlternative = grammar.A.Single(production => production.Count == 1);
        var consumingAlternative = grammar.A.Single(production => production.Count == 3);
        Assert.True(analyzer.First(nullableAlternative).IsNullAble);
        Assert.DoesNotContain(grammar.X, analyzer.First(nullableAlternative));
        Assert.False(analyzer.First(consumingAlternative).IsNullAble);
        Assert.Equal(new[] { grammar.X }, analyzer.First(consumingAlternative));
    }

    private sealed class ProgressGrammar : Grammar
    {
        internal readonly NonTerminal Start = new NonTerminal("S", true);
        internal readonly NonTerminal A = new NonTerminal("A");
        internal readonly NonTerminal B = new NonTerminal("B");
        internal readonly NonTerminal Guard = new NonTerminal("Guard");
        internal readonly Terminal X = new Terminal(TokenType.Keyword, "x", "x", true, false);
        internal readonly Terminal Y = new Terminal(TokenType.Keyword, "y", "y", true, false);
        public override NonTerminal EbnfRoot => Start;
        public ProgressGrammar(Action<ProgressGrammar> configure)
        {
            configure(this);

            // Unconfigured helper fields that no production references are not
            // part of the configured test grammar. Keep referenced empty rules.
            var referenced = NonTerminalMultiples
                .SelectMany(rule => rule)
                .SelectMany(production => production)
                .OfType<NonTerminal>()
                .ToHashSet();
            NonTerminalMultiples.RemoveWhere(rule =>
                rule.Count == 0 && !rule.IsStartSymbol && !referenced.Contains(rule));
        }
    }
}
