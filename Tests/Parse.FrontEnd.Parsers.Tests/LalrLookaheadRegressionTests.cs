using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Ordinary nullable rules and cyclic lookahead dependencies must not lose a
/// terminal merely because a different LR item was calculated first.
/// The tiny grammar is also LL(1): its two runtime branches have disjoint FIRSTs.
/// </summary>
public sealed class LalrLookaheadRegressionTests
{
    [Theory]
    [InlineData("a")]
    [InlineData("ba")]
    [InlineData("ea")]
    [InlineData("bba")]
    [InlineData("bea")]
    [InlineData("eba")]
    [InlineData("eea")]
    [InlineData("bebeea")]
    public void Nullable_prefix_and_mutual_recursion_accept_their_language(string input)
    {
        var grammar = Tiny();
        var parser = new LALRParser(grammar);

        Assert.False(parser.HasConflicts, parser.ConflictReport);
        Assert.All(grammar.NonTerminalMultiples.SelectMany(rule => rule), rule => Assert.Null(rule.Predicate));
        Accept(parser, input);
    }

    [Theory]
    [InlineData("")]
    [InlineData("b")]
    [InlineData("e")]
    [InlineData("beeb")]
    [InlineData("aa")]
    [InlineData("ab")]
    [InlineData("ae")]
    [InlineData("baba")]
    public void Propagating_eof_does_not_accept_incomplete_or_trailing_input(string input)
    {
        var parser = new LALRParser(Tiny());
        Reject(parser, input);
    }

    [Fact]
    public void Finishing_a_has_exactly_the_required_reduce_on_eof()
    {
        var grammar = Tiny();
        var parser = new LALRParser(grammar);
        var reached = Assert.Single(Items(parser), entry => entry.Item.IsReachedHandle &&
            entry.Item.SingleNT.Name == "R" && entry.Item.SingleNT.Count == 1 &&
            entry.Item.SingleNT[0] == grammar.Token("a"));

        AssertTerminals(reached.Item.LookAhead, "$");
        var table = Assert.IsType<LRParsingTable>(parser.ParsingTable);
        Assert.True(table[reached.State].MatchedValueSet.TryGetValue(new EndMarker(), out var actions),
            "The completed R -> a item must reduce on EOF.");
        var action = Assert.Single(actions);
        Assert.Equal(ActionDir.Reduce, action.Direction);
        var production = Assert.IsType<NonTerminalSingle>(action.Dest);
        Assert.Equal("R", production.Name);
        Assert.Equal(grammar.Token("a"), Assert.Single(production));
    }

    [Fact]
    public void Eof_reaches_the_completed_unit_production_in_the_dependency_cycle()
    {
        var grammar = Tiny();
        var parser = new LALRParser(grammar);
        var completed = Assert.Single(Items(parser), entry => entry.Item.IsReachedHandle &&
            entry.Item.SingleNT.Name == "C" && entry.Item.SingleNT.Count == 1 &&
            entry.Item.SingleNT[0] == grammar.Rule("R"));

        Assert.True(parser.Canonical.LookAheadTable.TryGetValue(
            (completed.State, completed.Item), out var propagated));
        AssertTerminals(propagated.LookAhead, "$");
        Assert.Same(propagated.LookAhead, completed.Item.LookAhead);

        var table = Assert.IsType<LRParsingTable>(parser.ParsingTable);
        Assert.True(table[completed.State].MatchedValueSet.TryGetValue(new EndMarker(), out var actions));
        var action = Assert.Single(actions);
        Assert.Equal(ActionDir.Reduce, action.Direction);
        var production = Assert.IsType<NonTerminalSingle>(action.Dest);
        Assert.Equal("C", production.Name);
        Assert.Equal(grammar.Rule("R"), Assert.Single(production));
    }

    [Fact]
    public void Every_completed_tiny_production_has_its_exact_lookahead()
    {
        var parser = new LALRParser(Tiny());
        foreach (var entry in Items(parser).Where(entry => entry.Item.IsReachedHandle))
        {
            if (entry.Item.SingleNT.Name == "G")
                AssertTerminals(entry.Item.LookAhead, "a", "b", "e");
            else
                AssertTerminals(entry.Item.LookAhead, "$");
        }
    }

    [Theory]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Declaration_and_alternative_order_do_not_change_the_table_information(
        bool reverseDeclarationOrder, bool reverseAlternatives)
    {
        var original = new LALRParser(Tiny());
        var reordered = new LALRParser(Tiny(reverseDeclarationOrder, reverseAlternatives));

        Assert.False(original.HasConflicts, original.ConflictReport);
        Assert.False(reordered.HasConflicts, reordered.ConflictReport);
        Assert.Equal(LookaheadSnapshot(original), LookaheadSnapshot(reordered));
        foreach (var input in new[] { "a", "ba", "ea", "ebbea" }) Accept(reordered, input);
        foreach (var input in new[] { "", "eb", "aa", "aeb" }) Reject(reordered, input);
    }

    [Fact]
    public void Inlining_the_nullable_prefix_preserves_the_original_language()
    {
        var explicitPrefix = new LALRParser(Tiny());
        var inlined = new LALRParser(Tiny(inlineNullablePrefix: true));
        foreach (var input in new[] { "a", "ba", "ea", "bebeea" })
        {
            Accept(explicitPrefix, input);
            Accept(inlined, input);
        }
        foreach (var input in new[] { "", "be", "ab", "aaa" })
        {
            Reject(explicitPrefix, input);
            Reject(inlined, input);
        }
    }

    [Theory]
    [InlineData("az")]
    [InlineData("abz")]
    [InlineData("xa")]
    [InlineData("xab")]
    public void Nullable_suffix_combines_its_first_tokens_with_the_parent_lookahead(string input)
    {
        // S -> A B z | x A B; A -> a; B -> epsilon | b.
        // The shared completed A -> a state receives {b,z} from the first
        // occurrence and {b,EOF} from the nullable trailing occurrence.
        var grammar = SuffixGrammar();
        var parser = new LALRParser(grammar);
        Assert.False(parser.HasConflicts, parser.ConflictReport);
        var item = Assert.Single(Items(parser), entry => entry.Item.IsReachedHandle &&
            entry.Item.SingleNT.Name == "A");
        AssertTerminals(item.Item.LookAhead, "$", "b", "z");
        Accept(parser, input);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("ab")]
    [InlineData("xaz")]
    [InlineData("xabb")]
    public void Nullable_suffix_does_not_hide_a_required_suffix_or_allow_extra_tokens(string input)
        => Reject(new LALRParser(SuffixGrammar()), input);

    [Fact]
    public void A_nonnullable_suffix_does_not_inherit_eof()
    {
        var grammar = new NativeGrammar(new Dictionary<string, string[][]>
        {
            ["S"] = new[] { new[] { "A", "b" } },
            ["A"] = new[] { new[] { "a" } }
        });
        var parser = new LALRParser(grammar);
        var item = Assert.Single(Items(parser), entry => entry.Item.IsReachedHandle &&
            entry.Item.SingleNT.Name == "A");
        AssertTerminals(item.Item.LookAhead, "b");
        Accept(parser, "ab");
        Reject(parser, "a");
    }

    [Fact]
    public void Multiple_nullable_suffixes_propagate_first_and_eof_without_epsilon()
    {
        var grammar = new NativeGrammar(new Dictionary<string, string[][]>
        {
            ["S"] = new[] { new[] { "A", "B", "C" } },
            ["A"] = new[] { new[] { "a" } },
            ["B"] = new[] { Array.Empty<string>(), new[] { "b" } },
            ["C"] = new[] { Array.Empty<string>(), new[] { "c" } }
        });
        var parser = new LALRParser(grammar);
        var item = Assert.Single(Items(parser), entry => entry.Item.IsReachedHandle &&
            entry.Item.SingleNT.Name == "A");
        AssertTerminals(item.Item.LookAhead, "$", "b", "c");
        Assert.All(Items(parser).Where(entry => entry.Item.IsReachedHandle),
            entry => Assert.DoesNotContain(entry.Item.LookAhead, terminal => terminal is Epsilon));
        foreach (var input in new[] { "a", "ab", "ac", "abc" }) Accept(parser, input);
        foreach (var input in new[] { "", "b", "acb", "abcc" }) Reject(parser, input);
    }

    [Fact]
    public void A_nullable_start_can_complete_without_reading_a_physical_token()
    {
        var grammar = new NativeGrammar(new Dictionary<string, string[][]>
        {
            ["S"] = new[] { new[] { "A" } },
            ["A"] = new[] { Array.Empty<string>(), new[] { "a" } }
        });
        var parser = new LALRParser(grammar);
        Assert.False(parser.HasConflicts, parser.ConflictReport);
        var empty = Assert.Single(Items(parser), entry => entry.Item.SingleNT.Name == "A" &&
            entry.Item.SingleNT.IsEpsilon);
        AssertTerminals(empty.Item.LookAhead, "$");
        Accept(parser, string.Empty);
        Accept(parser, "a");
        Reject(parser, "aa");
    }

    [Fact]
    public void A_longer_cycle_propagates_eof_to_every_completed_layer()
    {
        const int layers = 32;
        var productions = new Dictionary<string, string[][]>
        {
            ["S"] = new[] { new[] { "G", "C" } },
            ["C"] = new[] { new[] { "Layer0" }, new[] { "e", "S" } },
            ["R"] = new[] { new[] { "a" }, new[] { "b", "S" } },
            ["G"] = new[] { Array.Empty<string>() }
        };
        for (int i = 0; i < layers; i++)
            productions["Layer" + i] = new[] { new[] { i + 1 == layers ? "R" : "Layer" + (i + 1) } };
        var parser = new LALRParser(new NativeGrammar(productions));

        Assert.False(parser.HasConflicts, parser.ConflictReport);
        var completedLayers = Items(parser).Where(entry => entry.Item.IsReachedHandle &&
            entry.Item.SingleNT.Name.StartsWith("Layer", StringComparison.Ordinal)).ToArray();
        Assert.Equal(layers, completedLayers.Length);
        Assert.Equal(layers, completedLayers.Select(entry => entry.Item.SingleNT.Name).Distinct().Count());
        Assert.All(completedLayers, entry => AssertTerminals(entry.Item.LookAhead, "$"));
        Accept(parser, "a");
        Accept(parser, "ebeeba");
    }

    [Fact]
    public void Predicate_lowering_uses_the_same_complete_lookahead_information()
    {
        int trueCalls = 0, falseCalls = 0;
        var options = new EbnfReaderOptions()
            .RegisterPredicate("Always", _ => { trueCalls++; return true; })
            .RegisterPredicate("Never", _ => { falseCalls++; return false; });
        var read = EbnfGrammarReader.Read(@"
S : {Always()}? C ;
C : P {Always()}? | P {Never()}? 'e' 'x' '?' S ':' S ;
P : R {Always()}? ;
R : id {Always()}? | id {Never()}? 'i' S ;
id := ""[a-z]+"" ;", options);
        Assert.True(read.Success, string.Join(Environment.NewLine, read.Errors));
        var parser = new LALRParser(read.Grammar);
        Assert.Equal(0, trueCalls);
        Assert.Equal(0, falseCalls);

        Accept(parser, "a");

        Assert.True(trueCalls > 0);
        Assert.Equal(0, falseCalls);
    }

    private static NativeGrammar Tiny(bool reverseDeclarationOrder = false, bool reverseAlternatives = false,
        bool inlineNullablePrefix = false)
    {
        var rules = new Dictionary<string, string[][]>
        {
            ["S"] = new[] { inlineNullablePrefix ? new[] { "C" } : new[] { "G", "C" } },
            ["C"] = new[] { new[] { "R" }, new[] { "e", "S" } },
            ["R"] = new[] { new[] { "a" }, new[] { "b", "S" } }
        };
        if (!inlineNullablePrefix) rules["G"] = new[] { Array.Empty<string>() };
        if (reverseAlternatives)
        {
            Array.Reverse(rules["C"]);
            Array.Reverse(rules["R"]);
        }
        return new NativeGrammar(rules, reverseDeclarationOrder);
    }

    private static NativeGrammar SuffixGrammar() => new NativeGrammar(new Dictionary<string, string[][]>
    {
        ["S"] = new[] { new[] { "A", "B", "z" }, new[] { "x", "A", "B" } },
        ["A"] = new[] { new[] { "a" } },
        ["B"] = new[] { Array.Empty<string>(), new[] { "b" } }
    });

    private static IEnumerable<(int State, LRItem Item)> Items(LALRParser parser)
        => parser.Canonical.IndexStateDic.SelectMany(state => state.Value.Select(item => (state.Key, item)));

    private static void AssertTerminals(TerminalSet actual, params string[] expected)
        => Assert.Equal(expected.OrderBy(value => value, StringComparer.Ordinal),
            actual.Select(TerminalName).OrderBy(value => value, StringComparer.Ordinal));

    private static string TerminalName(Terminal terminal) => terminal is EndMarker ? "$" : terminal.Value;

    private static string ItemKey(LRItem item) => item.SingleNT.Name + " -> " +
        string.Join(" ", item.SingleNT.Where(symbol => !(symbol is Epsilon)).Select(symbol =>
            symbol is NonTerminal rule ? "N:" + rule.Name : "T:" + TerminalName((Terminal)symbol))) +
        " @ " + item.SymbolListBeforeMarkSymbol.Count;

    private static string[] LookaheadSnapshot(LALRParser parser)
        => parser.Canonical.IndexStateDic.Values.SelectMany(state =>
        {
            string core = string.Join(" | ", state.Select(ItemKey).OrderBy(value => value, StringComparer.Ordinal));
            return state.Where(item => item.IsReachedHandle).Select(item => core + " // " + ItemKey(item) + " : " +
                string.Join(",", item.LookAhead.Select(TerminalName).OrderBy(value => value, StringComparer.Ordinal)));
        }).OrderBy(value => value, StringComparer.Ordinal).ToArray();

    private static void Accept(LALRParser parser, string input)
    {
        var lexer = new Lexer();
        foreach (var terminal in parser.Grammar.TerminalSet) lexer.AddTokenRule(terminal);
        var parsed = parser.Parsing(lexer.Lexing(input).TokensForParsing);
        Assert.True(parsed.Success, input + Environment.NewLine + parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        Assert.Equal(input, string.Concat(parsed.ToParseTree.AllTokens.Select(token => token.Input)));
    }

    private static void Reject(LALRParser parser, string input)
    {
        var lexer = new Lexer();
        foreach (var terminal in parser.Grammar.TerminalSet) lexer.AddTokenRule(terminal);
        var parsed = parser.Parsing(lexer.Lexing(input).TokensForParsing);
        Assert.False(parsed.Success);
        Assert.IsType<ParsingException>(parser.FiredException);
        Assert.NotEmpty(parsed.AllErrors);
    }

    private sealed class NativeGrammar : Grammar
    {
        private readonly Dictionary<string, NonTerminal> rules;
        private readonly Dictionary<string, Terminal> terminals;
        public override NonTerminal EbnfRoot => StartSymbol;
        internal NonTerminal Rule(string name) => rules[name];
        internal Terminal Token(string value) => terminals[value];

        internal NativeGrammar(Dictionary<string, string[][]> productions, bool reverseRegistration = false)
        {
            rules = productions.Keys.ToDictionary(name => name, name => new NonTerminal(name, name == "S"));
            terminals = productions.Values.SelectMany(alternatives => alternatives).SelectMany(alternative => alternative)
                .Where(value => !rules.ContainsKey(value)).Distinct()
                .ToDictionary(value => value, value => new Terminal(TokenType.Operator, value));
            var ruleOrder = reverseRegistration ? rules.Values.Reverse() : rules.Values.AsEnumerable();
            var tokenOrder = reverseRegistration ? terminals.Values.Reverse() : terminals.Values.AsEnumerable();
            foreach (var rule in ruleOrder) { keyManager.AllocateUniqueKey(rule); NonTerminalMultiples.Add(rule); }
            foreach (var terminal in tokenOrder) { keyManager.AllocateUniqueKey(terminal); TerminalSet.Add(terminal); }
            SetStartSymbol(rules["S"]);
            foreach (var production in productions)
                foreach (var alternative in production.Value)
                    rules[production.Key].Add(alternative.Length == 0
                        ? new Symbol[] { new Epsilon() }
                        : alternative.Select(value => rules.TryGetValue(value, out var rule)
                            ? (Symbol)rule : terminals[value]).ToArray());
        }
    }
}
