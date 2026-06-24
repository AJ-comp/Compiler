using System.Linq;
using Parse.FrontEnd.Parsers.Datas;
using Parse.FrontEnd.Parsers.LR;
using Parse.FrontEnd.Tokenize;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

/// <summary>
/// Demonstrates what the LGLR paper ACTUALLY extends: the set of GRAMMARS, not languages.
/// <see cref="PaperConflictDeclExprGrammar"/> is the SAME language as the left-factored
/// grammar, but written naturally it is NOT LALR(1) (it has a shift/reduce conflict).
///   * plain LALR  : conflict in the table; forced to resolve one way, it cannot take
///                   both branches, so a const-less declaration fails.
///   * LGLR (UseBackTrackingOnConflict): parses the SAME conflicting grammar by trying
///                   the alternative on failure -> both declarations and expressions parse.
/// So the paper buys you "parse this grammar as-written" (grammar-class extension), which
/// for this DCFL example is convenience rather than new expressive power over the language.
/// </summary>
[Trait("Category", "GrammarExtension")]
public class GrammarClassExtensionTests
{
    private static int ConflictCount()
    {
        var g = new PaperConflictDeclExprGrammar();
        return new LALRParser(g, false).CheckAmbiguity()
                   .Count(i => i.IsShiftReduceConflict || i.IsReduceReduceConflict);
    }

    private static ParsingResult ParsePlain(string input)
    {
        var g = new PaperConflictDeclExprGrammar();
        var lexer = new Lexer();
        foreach (var t in g.TerminalSet) lexer.AddTokenRule(t);
        return new LALRParser(g, false).Parsing(lexer.Lexing(input).TokensForParsing);
    }

    private static ParsingResult ParseLglr(string input)
    {
        var g = new PaperConflictDeclExprGrammar();
        var lexer = new Lexer();
        foreach (var t in g.TerminalSet) lexer.AddTokenRule(t);
        // LGLR = LALR + a backtracking error handler + the flag. The flag alone does nothing;
        // the handler is what re-runs the losing branch of a conflict.
        var parser = new LALRParser(g, false);
        parser.AddErrorHandler(new ConflictBacktrackingErrorHandler());
        parser.UseBackTrackingOnConflict();
        return parser.Parsing(lexer.Lexing(input).TokensForParsing);
    }

    [Fact]
    public void Paper_grammar_is_NOT_LALR1_as_written()
        => Assert.True(ConflictCount() > 0,
            "the natural (optional-const) grammar must report a shift/reduce conflict");

    [Theory]
    [InlineData("a = b ;")]               // expression
    [InlineData("a.b = c.d ;")]           // expression, dotted both sides
    [InlineData("a b ;")]                 // const-less declaration  <-- the hard one for plain LALR
    [InlineData("a.b c.d ;")]             // declaration, dotted
    [InlineData("const a.b c.d ;")]       // const declaration
    public void LGLR_parses_the_conflicting_grammar(string input)
        => Assert.True(ParseLglr(input).Success, $"LGLR should ACCEPT the as-written grammar: \"{input}\"");

    [Fact]
    public void Plain_LALR_cannot_take_both_branches_of_the_conflict()
        => Assert.False(ParsePlain("a b ;").Success,
            "plain LALR resolves the conflict one way, so it cannot also parse the other branch");
}
