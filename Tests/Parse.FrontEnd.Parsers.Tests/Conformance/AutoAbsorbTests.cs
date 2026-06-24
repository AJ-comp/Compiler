using System.Linq;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// optional-absorb is now AUTOMATIC by default (Grammar.AbsorbOptionals == true): a grammar that
/// writes <c>const?</c> and never calls NormalizeOptionals() still parses under plain LALR with no
/// conflict. (Grammars that want the natural/conflicting form override AbsorbOptionals to false —
/// e.g. PaperConflictDeclExprGrammar and AJ.)
/// </summary>
[Trait("Category", "GrammarNormalization")]
public class AutoAbsorbTests
{
    private static ParsingResult Parse(string input)
    {
        var g = new AutoAbsorbDeclExprGrammar();
        var lexer = new Lexer();
        foreach (var t in g.TerminalSet) lexer.AddTokenRule(t);
        return new LALRParser(g, false).Parsing(lexer.Lexing(input).TokensForParsing);
    }

    [Fact]
    public void Default_grammar_auto_absorbs_optionals_no_conflict()
    {
        var g = new AutoAbsorbDeclExprGrammar();   // no explicit NormalizeOptionals()
        var conflicts = new LALRParser(g, false).CheckAmbiguity()
                            .Count(i => i.IsShiftReduceConflict || i.IsReduceReduceConflict);
        Assert.Equal(0, conflicts);
    }

    [Theory]
    [InlineData("a b ;")]          // const-less declaration
    [InlineData("const a b ;")]    // const declaration
    [InlineData("a = b ;")]        // expression
    public void Default_grammar_parses_under_plain_LALR(string input)
        => Assert.True(Parse(input).Success, $"expected ACCEPT: \"{input}\"");
}
