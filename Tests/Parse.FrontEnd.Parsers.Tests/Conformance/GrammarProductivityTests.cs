using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

public class GrammarProductivityTests
{
    [Theory]
    [InlineData("self", false)]
    [InlineData("self", true)]
    [InlineData("left-consuming", false)]
    [InlineData("left-consuming", true)]
    [InlineData("right-consuming", false)]
    [InlineData("right-consuming", true)]
    [InlineData("mutual", false)]
    [InlineData("mutual", true)]
    [InlineData("dependent-chain", false)]
    [InlineData("dependent-chain", true)]
    [InlineData("all-dead", false)]
    [InlineData("all-dead", true)]
    [InlineData("unreachable", false)]
    [InlineData("unreachable", true)]
    [InlineData("all-alternatives-dead", false)]
    [InlineData("all-alternatives-dead", true)]
    [InlineData("dead-suffix", false)]
    [InlineData("dead-suffix", true)]
    public void Nonproductive_rules_are_reported_in_ordinal_order_regardless_of_declaration_order(
        string shape, bool reverse)
    {
        var (rules, expectedNames) = NonproductiveGrammar(shape);
        var grammar = new NativeGrammar(rules, reverse);

        var error = Assert.Throws<GrammarProductivityException>(() => grammar.Optimization());

        Assert.Equal(expectedNames, error.RuleNames);
        Assert.Contains("finite token sequence", error.Message);
        foreach (string name in expectedNames)
            Assert.Contains(name, error.Message);
    }

    [Theory]
    [InlineData("unit-chain", false)]
    [InlineData("unit-chain", true)]
    [InlineData("left-recursive-base", false)]
    [InlineData("left-recursive-base", true)]
    [InlineData("right-recursive-base", false)]
    [InlineData("right-recursive-base", true)]
    [InlineData("mutual-recursive-base", false)]
    [InlineData("mutual-recursive-base", true)]
    [InlineData("mutual-epsilon-base", false)]
    [InlineData("mutual-epsilon-base", true)]
    [InlineData("one-finishable-alternative", false)]
    [InlineData("one-finishable-alternative", true)]
    [InlineData("all-symbols-finishable", false)]
    [InlineData("all-symbols-finishable", true)]
    public void Terminating_alternatives_make_recursive_rules_productive(
        string shape, bool reverse)
    {
        var grammar = new NativeGrammar(ProductiveGrammar(shape), reverse);

        grammar.Optimization();
        grammar.Optimization();

        Assert.Equal("S", grammar.StartSymbol.Name);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Productivity_validation_accepts_empty_rhs_and_explicit_epsilon(bool explicitEpsilon)
    {
        var grammar = new NativeGrammar(Spec(("S", "A"), ("A", "")));
        grammar.Optimization();

        // Existing normalization represents epsilon explicitly. To test the validator's
        // empty-RHS handling independently, replace that RHS through the mutable native API
        // after normalization, then exercise the normal revalidation path.
        if (!explicitEpsilon)
        {
            grammar.Rule("A").Clear();
            grammar.Rule("A").Add(Array.Empty<Symbol>());
        }

        // Establish that the native fixture exercises two different representations.
        var production = grammar.Rule("A").ElementAt(0);
        if (explicitEpsilon)
            Assert.IsType<Epsilon>(Assert.Single(production));
        else
            Assert.Empty(production);

        grammar.Optimization();
    }

    [Fact]
    public void Predicate_callback_is_not_evaluated_to_decide_structural_productivity()
    {
        var grammar = new NativeGrammar(Spec(("S", "Guard a"), ("Guard", "")));
        int calls = 0;
        grammar.Rule("Guard").ElementAt(0).Predicate = new GrammarPredicate("Allow", _ =>
        {
            calls++;
            throw new InvalidOperationException("Validation must not execute a predicate callback.");
        });

        grammar.Optimization();

        Assert.Equal(0, calls);
    }

    [Fact]
    public void Nonproductive_rule_is_rejected_without_evaluating_an_unrelated_predicate()
    {
        var grammar = new NativeGrammar(Spec(("S", "Guard a"), ("Guard", ""), ("U", "U")));
        int calls = 0;
        grammar.Rule("Guard").ElementAt(0).Predicate = new GrammarPredicate("Allow", _ =>
        {
            calls++;
            return false;
        });

        var error = Assert.Throws<GrammarProductivityException>(() => grammar.Optimization());

        Assert.Equal(new[] { "U" }, error.RuleNames);
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Parser_creation_rejects_a_nonproductive_grammar_before_table_generation()
    {
        var grammar = new NativeGrammar(Spec(("S", "A U | C x"), ("A", "B x"),
            ("B", "a"), ("C", "a"), ("U", "U")));

        var error = Assert.Throws<GrammarProductivityException>(() => new LALRParser(grammar));

        Assert.Equal(new[] { "U" }, error.RuleNames);
    }

    [Fact]
    public void A_productive_unit_chain_still_builds_and_parses_normally()
    {
        var parser = new LALRParser(new NativeGrammar(Spec(("S", "A"), ("A", "B"), ("B", "a"))));
        var lexer = new Lexer();
        foreach (var terminal in parser.Grammar.TerminalSet)
            lexer.AddTokenRule(terminal);

        var result = parser.Parsing(lexer.Lexing("a").TokensForParsing);

        Assert.True(result.Success);
        Assert.Empty(result.AllErrors);
        Assert.Equal("a", string.Concat(result.ToParseTree.AllTokens.Select(token => token.Input)));
    }

    [Fact]
    public void Already_normalized_grammar_is_revalidated_after_a_rule_becomes_nonproductive()
    {
        var grammar = new NativeGrammar(Spec(("S", "A"), ("A", "a")));
        grammar.Optimization();

        grammar.Rule("A").Clear();
        grammar.Rule("A").Add(grammar.Rule("A"));

        var error = Assert.Throws<GrammarProductivityException>(() => grammar.Optimization());
        Assert.Equal(new[] { "A", "S" }, error.RuleNames);

        grammar.Rule("A").Add(grammar.Token("a"));
        grammar.Optimization();
    }

    [Fact]
    public void A_failed_validation_can_be_retried_after_adding_a_terminating_alternative()
    {
        var grammar = new NativeGrammar(Spec(("S", "a"), ("U", "U")));
        Assert.Throws<GrammarProductivityException>(() => grammar.Optimization());

        grammar.Rule("U").Add(grammar.Token("a"));

        grammar.Optimization();
        grammar.Optimization();
    }

    private static (Dictionary<string, string[][]> Rules, string[] InvalidNames) NonproductiveGrammar(
        string shape) => shape switch
    {
        "self" => (Spec(("S", "U | z"), ("U", "U")), new[] { "U" }),
        "left-consuming" => (Spec(("S", "U | z"), ("U", "U a")), new[] { "U" }),
        "right-consuming" => (Spec(("S", "U | z"), ("U", "a U")), new[] { "U" }),
        "mutual" => (Spec(("S", "A | z"), ("A", "B"), ("B", "A")), new[] { "A", "B" }),
        "dependent-chain" => (Spec(("S", "A | z"), ("A", "B x"), ("B", "U"), ("U", "u U")),
            new[] { "A", "B", "U" }),
        "all-dead" => (Spec(("S", "A"), ("A", "B"), ("B", "A")), new[] { "A", "B", "S" }),
        "unreachable" => (Spec(("S", "a"), ("U", "U")), new[] { "U" }),
        "all-alternatives-dead" => (Spec(("S", "A | a"), ("A", "B | C"), ("B", "B"), ("C", "C")),
            new[] { "A", "B", "C" }),
        "dead-suffix" => (Spec(("S", "A U | C x"), ("A", "B x"), ("B", "a"), ("C", "a"), ("U", "U")),
            new[] { "U" }),
        _ => throw new ArgumentOutOfRangeException(nameof(shape))
    };

    private static Dictionary<string, string[][]> ProductiveGrammar(string shape) => shape switch
    {
        "unit-chain" => Spec(("S", "A"), ("A", "B"), ("B", "a")),
        "left-recursive-base" => Spec(("S", "A"), ("A", "A x | a")),
        "right-recursive-base" => Spec(("S", "A"), ("A", "x A | a")),
        "mutual-recursive-base" => Spec(("S", "A"), ("A", "B"), ("B", "A x | a")),
        "mutual-epsilon-base" => Spec(("S", "A x"), ("A", "B"), ("B", "A |")),
        "one-finishable-alternative" => Spec(("S", "A"), ("A", "A A | a")),
        "all-symbols-finishable" => Spec(("S", "A B"), ("A", "a"), ("B", "b")),
        _ => throw new ArgumentOutOfRangeException(nameof(shape))
    };

    private static Dictionary<string, string[][]> Spec(params (string Name, string Alternatives)[] rules)
        => rules.ToDictionary(rule => rule.Name, rule => rule.Alternatives.Split('|')
            .Select(body => body.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToArray(),
            StringComparer.Ordinal);

    private sealed class NativeGrammar : Grammar
    {
        private readonly Dictionary<string, NonTerminal> _rules;
        private readonly Dictionary<string, Terminal> _tokens;

        public override NonTerminal EbnfRoot => StartSymbol;

        internal NonTerminal Rule(string name) => _rules[name];
        internal Terminal Token(string name) => _tokens[name];

        internal NativeGrammar(Dictionary<string, string[][]> rules, bool reverse = false)
        {
            var names = reverse ? rules.Keys.Reverse().ToArray() : rules.Keys.ToArray();
            _rules = names.ToDictionary(name => name,
                name => new NonTerminal(name, name == "S"), StringComparer.Ordinal);
            _tokens = rules.Values.SelectMany(alternatives => alternatives).SelectMany(body => body)
                .Where(symbol => !_rules.ContainsKey(symbol)).Distinct(StringComparer.Ordinal)
                .ToDictionary(symbol => symbol, symbol => new Terminal(TokenType.Operator, symbol), StringComparer.Ordinal);

            foreach (var nonTerminal in _rules.Values)
            {
                keyManager.AllocateUniqueKey(nonTerminal);
                NonTerminalMultiples.Add(nonTerminal);
            }
            foreach (var terminal in reverse ? _tokens.Values.Reverse() : _tokens.Values)
            {
                keyManager.AllocateUniqueKey(terminal);
                TerminalSet.Add(terminal);
            }
            SetStartSymbol(_rules["S"]);
            foreach (string name in names)
                foreach (var body in reverse ? rules[name].Reverse() : rules[name])
                    _rules[name].Add(body.Length == 0
                        ? new Symbol[] { new Epsilon() }
                        : body.Select(symbol => _rules.TryGetValue(symbol, out var nonTerminal)
                            ? (Symbol)nonTerminal : _tokens[symbol]).ToArray());
        }
    }
}
