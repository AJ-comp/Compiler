using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// The parser normalizes the grammar automatically before building its table, so a grammar
/// author no longer has to remember to call Optimization(). A grammar that never calls it
/// (<see cref="NoOptimizationSelfRefGrammar"/>) must still parse, and must end up structurally
/// identical to the same grammar that called Optimization() explicitly.
/// </summary>
[Trait("Category", "GrammarNormalization")]
public class AutoNormalizationTests
{
    private static ParsingResult Parse(Grammar g, string input)
    {
        var lexer = new Lexer();
        foreach (var t in g.TerminalSet) lexer.AddTokenRule(t);
        return new LALRParser(g, false).Parsing(lexer.Lexing(input).TokensForParsing);
    }

    [Theory]
    [InlineData("a")]
    [InlineData("a + a")]
    [InlineData("a + a + a")]
    public void Parser_normalizes_a_grammar_that_never_called_Optimization(string input)
        => Assert.True(Parse(new NoOptimizationSelfRefGrammar(), input).Success, $"expected ACCEPT: \"{input}\"");

    [Fact]
    public void Auto_normalization_yields_the_same_grammar_as_an_explicit_call()
    {
        var explicitG = new SelfRefExprGrammar();          // calls Optimization() in its ctor
        var autoG = new NoOptimizationSelfRefGrammar();     // never calls it

        // building a parser triggers normalization (+ a virtual "Accept" symbol) on each
        new LALRParser(explicitG, false);
        new LALRParser(autoG, false);

        Assert.Equal(explicitG.NonTerminalMultiples.Count, autoG.NonTerminalMultiples.Count);
    }
}
