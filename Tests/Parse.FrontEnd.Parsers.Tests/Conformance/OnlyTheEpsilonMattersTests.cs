using System.Linq;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Pins down WHICH twist actually removes the LGLR paper's conflict. The paper grammar
/// conflicts; <see cref="ConstExpandedDeclExprGrammar"/> differs from it by exactly one
/// thing — the optional const is expanded from an epsilon option into two explicit
/// productions — and is otherwise identical (Declare/Expression still separate, identChain
/// prefix still shared, NO manual left-factoring of it). It parses conflict-free under plain
/// LALR. Conclusion: the epsilon was the sole culprit; LR shares the identChain prefix for
/// free, so unlike LL you do NOT have to left-factor it by hand.
/// </summary>
[Trait("Category", "GrammarExtension")]
public class OnlyTheEpsilonMattersTests
{
    private static int ConflictCount()
    {
        var g = new ConstExpandedDeclExprGrammar();
        return new LALRParser(g, false).CheckAmbiguity()
                   .Count(i => i.IsShiftReduceConflict || i.IsReduceReduceConflict);
    }

    private static ParsingResult ParsePlain(string input)
    {
        var g = new ConstExpandedDeclExprGrammar();
        var lexer = new Lexer();
        foreach (var t in g.TerminalSet) lexer.AddTokenRule(t);
        return new LALRParser(g, false).Parsing(lexer.Lexing(input).TokensForParsing);
    }

    [Fact]
    public void Expanding_the_optional_const_removes_the_conflict()
        => Assert.Equal(0, ConflictCount());

    [Theory]
    [InlineData("a b ;")]                 // const-less declaration  <-- failed under plain LALR with the epsilon grammar
    [InlineData("a.b c.d ;")]             // declaration, dotted
    [InlineData("const a.b c.d ;")]       // const declaration
    [InlineData("a.b = c.d ;")]           // expression, dotted both sides
    [InlineData("a.b.c = d.e.f ;")]       // expression, long chains
    public void Plain_LALR_now_parses_everything_without_manual_left_factoring(string input)
        => Assert.True(ParsePlain(input).Success, $"plain LALR should ACCEPT: \"{input}\"");
}
