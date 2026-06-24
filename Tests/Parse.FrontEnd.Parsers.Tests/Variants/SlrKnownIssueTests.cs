using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// KNOWN BUG (Phase 1): the SLR (C0) parser is broken. Its parsing table has no reduce
/// actions and no EndMarker column (see Infra/__snapshots__/Ex1.slr.parsing-table), so it
/// cannot parse even the unambiguous, SLR-valid Ex1 grammar. LALR works and is the live path.
/// This test is skipped and turns green once SLR is fixed — at which point the commented-out
/// SlrConformance class can also be enabled.
/// </summary>
[Trait("Category", "Variant")]
public class SlrKnownIssueTests
{
    [Fact(Skip = "SLR/C0 is broken: reduce-less parsing table; cannot parse even unambiguous Ex1. LALR works. Un-skip when fixed.")]
    public void Slr_should_parse_valid_Ex1()
    {
        var grammar = new Ex1Grammar();
        var lexer = new Lexer();
        foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);
        var slr = new SLRParser(grammar, false);

        var result = slr.Parsing(lexer.Lexing("a = e").TokensForParsing);
        Assert.True(result.Success, "SLR should parse the valid sentence \"a = e\"");
    }
}
