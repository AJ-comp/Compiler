using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas.LR;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Completed LR items must publish the actual TerminalSet instances held in the
/// caller-owned table. Initial/intermediate entries are not required to exist.
/// No private queue state is read.
/// </summary>
[Trait("Category", "LalrLookahead")]
[Trait("Feature", "LalrPublicContract")]
public sealed class LalrLookaheadContractTests
{
    [Fact]
    public void Calculate_keeps_the_caller_assigned_empty_dictionary_and_publishes_completed_item_sets()
    {
        var grammar = new NativeGrammar(lateSecondContext: true);
        grammar.Optimization();
        var start = grammar.CreateVirtualSymbolForLRParsing("Accept");
        var analyzer = new FirstFollowAnalyzer();
        analyzer.CalculateAllFollow(grammar.NonTerminalMultiples);

        // A new calculation starts with an empty table, including when the
        // caller supplies that table. Resetting pre-populated tables is not
        // part of this contract.
        Dictionary<(int, LRItem), CanonicalRelation.LookAheadEntry> supplied =
            new Dictionary<(int, LRItem), CanonicalRelation.LookAheadEntry>();
        var canonical = new CanonicalRelation();
        canonical.LookAheadTable = supplied;

        canonical.Calculate(start, analyzer, CanonicalType.LALRC1);

        Dictionary<(int, LRItem), CanonicalRelation.LookAheadEntry> publishedTable = canonical.LookAheadTable;
        Assert.Same(supplied, publishedTable);
        var completedItems = canonical.IndexStateDic.SelectMany(state => state.Value
            .Where(item => item.IsReachedHandle)
            .Select(item => (State: state.Key, Item: item))).ToArray();
        Assert.NotEmpty(completedItems);
        foreach (var completed in completedItems)
        {
            Assert.True(supplied.TryGetValue((completed.State, completed.Item), out var published));
            Assert.Same(published.LookAhead, completed.Item.LookAhead);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Every_completed_LR_item_uses_the_same_set_published_in_its_entry(bool lateSecondContext)
    {
        var parser = new LALRParser(new NativeGrammar(lateSecondContext));
        Assert.NotEmpty(parser.Canonical.LookAheadTable);
        var completedItems = Items(parser).Where(entry => entry.Item.IsReachedHandle).ToArray();
        Assert.NotEmpty(completedItems);
        foreach (var completed in completedItems)
            Assert.Same(parser.Canonical.LookAheadTable[(completed.State, completed.Item)].LookAhead,
                completed.Item.LookAhead);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Both_delimiters_reach_the_completed_child_item(bool lateSecondContext)
    {
        // Immediate: S -> A ! | A ?; A -> a z.
        // Delayed:   S -> A ! | b c d A ?; A -> a z.
        // Both contexts share the completed A production. Its Reduce lookahead
        // must include both delimiters even when the second context is delayed.
        var parser = new LALRParser(new NativeGrammar(lateSecondContext));
        Assert.False(parser.HasConflicts, parser.ConflictReport);
        var completed = Assert.Single(Items(parser), entry =>
            entry.Item.SingleNT.Name == "A" && entry.Item.IsReachedHandle);
        AssertTerminals(completed.Item.LookAhead, "!", "?");
    }

    [Theory]
    [InlineData(false, "az!", true)]
    [InlineData(false, "az?", true)]
    [InlineData(false, "az", false)]
    [InlineData(false, "az!?", false)]
    [InlineData(false, "az??", false)]
    [InlineData(false, "bcdaz?", false)]
    [InlineData(true, "az!", true)]
    [InlineData(true, "bcdaz?", true)]
    [InlineData(true, "az?", false)]
    [InlineData(true, "bcdaz!", false)]
    [InlineData(true, "bcdaz", false)]
    [InlineData(true, "bcdaz?!", false)]
    public void Merged_lookaheads_preserve_both_language_branches_and_required_suffixes(
        bool lateSecondContext, string input, bool expectedSuccess)
    {
        var parser = new LALRParser(new NativeGrammar(lateSecondContext));
        var lexer = new Lexer();
        foreach (var terminal in parser.Grammar.TerminalSet) lexer.AddTokenRule(terminal);
        var result = parser.Parsing(lexer.Lexing(input).TokensForParsing);

        Assert.Equal(expectedSuccess, result.Success);
        if (expectedSuccess)
        {
            Assert.Empty(result.AllErrors);
            Assert.Equal(input, string.Concat(result.ToParseTree.AllTokens.Select(token => token.Input)));
        }
        else
        {
            Assert.IsType<ParsingException>(parser.FiredException);
            Assert.NotEmpty(result.AllErrors);
        }
    }

    private static IEnumerable<(int State, LRItem Item)> Items(LALRParser parser)
        => parser.Canonical.IndexStateDic.SelectMany(state => state.Value.Select(item => (state.Key, item)));

    private static string[] Names(TerminalSet terminals)
        => terminals.Select(terminal => terminal is EndMarker ? "$" : terminal.Value)
            .OrderBy(value => value, StringComparer.Ordinal).ToArray();

    private static void AssertTerminals(TerminalSet actual, params string[] expected)
        => Assert.Equal(expected.OrderBy(value => value, StringComparer.Ordinal), Names(actual));

    private sealed class NativeGrammar : Grammar
    {
        public override NonTerminal EbnfRoot => StartSymbol;

        internal NativeGrammar(bool lateSecondContext)
        {
            var start = new NonTerminal("S", true);
            var child = new NonTerminal("A");
            foreach (var rule in new[] { start, child })
            {
                keyManager.AllocateUniqueKey(rule);
                NonTerminalMultiples.Add(rule);
            }
            var tokens = new[] { "a", "z", "!", "?", "b", "c", "d" }
                .ToDictionary(value => value, value => new Terminal(TokenType.Operator, value));
            foreach (var token in tokens.Values)
            {
                keyManager.AllocateUniqueKey(token);
                TerminalSet.Add(token);
            }
            SetStartSymbol(start);
            start.Add(new Symbol[] { child, tokens["!"] });
            start.Add(lateSecondContext
                ? new Symbol[] { tokens["b"], tokens["c"], tokens["d"], child, tokens["?"] }
                : new Symbol[] { child, tokens["?"] });
            child.Add(new Symbol[] { tokens["a"], tokens["z"] });
        }
    }
}
