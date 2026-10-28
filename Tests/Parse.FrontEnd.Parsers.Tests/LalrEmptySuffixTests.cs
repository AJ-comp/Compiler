using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// A genuinely empty suffix inherits its parent's lookahead. An unproductive
/// suffix is rejected during grammar preparation, before a parsing table exists.
/// Accepted grammars are checked through completed LR items and actual parsing.
/// </summary>
[Trait("Category", "LalrLookahead")]
[Trait("Feature", "EmptySuffix")]
public sealed class LalrEmptySuffixTests
{
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Unproductive_suffix_is_rejected_before_the_completed_A_item_is_built(
        bool emptyPrefix, bool reverse)
    {
        // U -> U can produce neither a terminal nor epsilon. The agreed policy
        // rejects U instead of building a table for this unusable grammar path.
        var rules = Spec(("S", emptyPrefix ? "G A U | z" : "A U | z"),
            ("A", "B x"), ("B", "a"), ("U", "U"));
        if (emptyPrefix) rules.Add("G", new[] { Array.Empty<string>() });
        var error = Assert.Throws<GrammarProductivityException>(() =>
            new LALRParser(new NativeGrammar(rules, reverse)));

        // A and B themselves can finish. Only U is nonproductive.
        Assert.Equal(new[] { "U" }, error.RuleNames);
    }

    [Fact]
    public void Absent_suffix_passes_EOF_through_a_productive_unit_production_chain()
    {
        var parser = new LALRParser(new NativeGrammar(Spec(
            ("S", "A"), ("A", "B"), ("B", "a"))));

        AssertLookAhead(CompletedItem(parser, "A"), "$");
        AssertLookAhead(CompletedItem(parser, "B"), "$");
        AssertParsing(parser, "a", expectedSuccess: true);
        AssertParsing(parser, "", expectedSuccess: false);
        AssertParsing(parser, "aa", expectedSuccess: false);
    }

    [Theory]
    [InlineData("absent", "$")]
    [InlineData("epsilon", "$")]
    [InlineData("nullable", "$,e")]
    [InlineData("terminal", "e")]
    [InlineData("nonnullable", "e")]
    [InlineData("nullable-chain", "$,f")]
    public void Completed_A_lookahead_distinguishes_nullable_and_required_suffixes(
        string suffix, string expected)
    {
        var parser = new LALRParser(new NativeGrammar(SuffixGrammar(suffix)));

        AssertLookAhead(CompletedItem(parser, "A"), expected.Split(','));
    }

    [Theory]
    [InlineData("absent", "a", true)]
    [InlineData("absent", "", false)]
    [InlineData("absent", "aa", false)]
    [InlineData("epsilon", "a", true)]
    [InlineData("epsilon", "", false)]
    [InlineData("epsilon", "aa", false)]
    [InlineData("nullable", "a", true)]
    [InlineData("nullable", "ae", true)]
    [InlineData("nullable", "aee", false)]
    [InlineData("nullable", "e", false)]
    [InlineData("nullable", "", false)]
    [InlineData("terminal", "ae", true)]
    [InlineData("terminal", "a", false)]
    [InlineData("terminal", "aee", false)]
    [InlineData("terminal", "e", false)]
    [InlineData("nonnullable", "ae", true)]
    [InlineData("nonnullable", "a", false)]
    [InlineData("nonnullable", "aee", false)]
    [InlineData("nonnullable", "e", false)]
    [InlineData("nullable-chain", "a", true)]
    [InlineData("nullable-chain", "af", true)]
    [InlineData("nullable-chain", "aff", false)]
    [InlineData("nullable-chain", "f", false)]
    public void Suffix_lookaheads_preserve_accepted_inputs_and_reject_missing_or_extra_tokens(
        string suffix, string input, bool expectedSuccess)
    {
        var parser = new LALRParser(new NativeGrammar(SuffixGrammar(suffix)));

        AssertParsing(parser, input, expectedSuccess);
    }

    private static Dictionary<string, string[][]> SuffixGrammar(string suffix) => suffix switch
    {
        "absent" => Spec(("S", "A"), ("A", "a")),
        "epsilon" => Spec(("S", "A E"), ("A", "a"), ("E", "")),
        "nullable" => Spec(("S", "A E"), ("A", "a"), ("E", " | e")),
        "terminal" => Spec(("S", "A e"), ("A", "a")),
        "nonnullable" => Spec(("S", "A E"), ("A", "a"), ("E", "e")),
        "nullable-chain" => Spec(("S", "A E F"), ("A", "a"), ("E", ""), ("F", " | f")),
        _ => throw new ArgumentOutOfRangeException(nameof(suffix))
    };

    private static LRItem CompletedItem(LALRParser parser, string nonTerminal)
    {
        var completed = Assert.Single(parser.Canonical.IndexStateDic.SelectMany(state => state.Value
                .Where(item => item.IsReachedHandle && item.SingleNT.Name == nonTerminal)
                .Select(item => (State: state.Key, Item: item))));
        Assert.True(parser.Canonical.LookAheadTable.TryGetValue((completed.State, completed.Item), out var entry));
        Assert.Same(entry.LookAhead, completed.Item.LookAhead);
        Assert.DoesNotContain(entry.LookAhead, terminal => terminal is Epsilon);
        return completed.Item;
    }

    private static void AssertLookAhead(LRItem item, params string[] expected)
        => Assert.Equal(expected.OrderBy(value => value, StringComparer.Ordinal), item.LookAhead
            .Select(terminal => terminal is EndMarker ? "$" : terminal.Value)
            .OrderBy(value => value, StringComparer.Ordinal));

    private static void AssertParsing(LALRParser parser, string input, bool expectedSuccess)
    {
        var lexer = new Lexer();
        foreach (var terminal in parser.Grammar.TerminalSet) lexer.AddTokenRule(terminal);
        var result = parser.Parsing(lexer.Lexing(input).TokensForParsing);

        Assert.Equal(expectedSuccess, result.Success);
        if (expectedSuccess)
        {
            Assert.Empty(result.AllErrors);
            Assert.Equal(input, string.Concat(result.ToParseTree.AllTokens.Select(token => token.Input)));
            return;
        }

        Assert.IsType<ParsingException>(parser.FiredException);
        Assert.NotEmpty(result.AllErrors);
    }

    private static Dictionary<string, string[][]> Spec(params (string Name, string Alternatives)[] rules)
        => rules.ToDictionary(rule => rule.Name, rule => rule.Alternatives.Split('|')
            .Select(body => body.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)).ToArray(),
            StringComparer.Ordinal);

    private sealed class NativeGrammar : Grammar
    {
        public override NonTerminal EbnfRoot => StartSymbol;

        internal NativeGrammar(Dictionary<string, string[][]> rules, bool reverse = false)
        {
            var names = reverse ? rules.Keys.Reverse().ToArray() : rules.Keys.ToArray();
            var nonTerminals = names.ToDictionary(name => name,
                name => new NonTerminal(name, name == "S"), StringComparer.Ordinal);
            var terminals = rules.Values.SelectMany(alternatives => alternatives).SelectMany(body => body)
                .Where(symbol => !nonTerminals.ContainsKey(symbol)).Distinct(StringComparer.Ordinal)
                .ToDictionary(symbol => symbol, symbol => new Terminal(TokenType.Operator, symbol), StringComparer.Ordinal);

            foreach (var nonTerminal in nonTerminals.Values)
            {
                keyManager.AllocateUniqueKey(nonTerminal);
                NonTerminalMultiples.Add(nonTerminal);
            }
            foreach (var terminal in reverse ? terminals.Values.Reverse() : terminals.Values)
            {
                keyManager.AllocateUniqueKey(terminal);
                TerminalSet.Add(terminal);
            }
            SetStartSymbol(nonTerminals["S"]);
            foreach (string name in names)
                foreach (var body in reverse ? rules[name].Reverse() : rules[name])
                    nonTerminals[name].Add(body.Length == 0 ? new Symbol[] { new Epsilon() } : body
                        .Select(symbol => nonTerminals.TryGetValue(symbol, out var nonTerminal)
                            ? (Symbol)nonTerminal : terminals[symbol]).ToArray());
        }
    }
}
