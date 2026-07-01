using System.Linq;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Grammars.ExampleGrammars;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// COMMON (parser-independent): the lexer turns an input string into tokens using the
/// grammar's terminals. No parser involved.
/// </summary>
[Trait("Category", "Common")]
public class LexerTests
{
    private static Lexer LexerFor(Grammar grammar)
    {
        var lexer = new Lexer();
        foreach (var t in grammar.TerminalSet) lexer.AddTokenRule(t);
        return lexer;
    }

    [Fact]
    public void Tokenizes_and_skips_whitespace()
    {
        var tokens = LexerFor(new Ex1Grammar()).Lexing("a = e").TokensForParsing;
        Assert.Equal(new[] { "a", "=", "e" }, tokens.Select(t => t.Data).ToArray());
    }

    // A lexer with a '/' division operator AND a '//' line comment: the comment's regex value
    // must beat the operator so "//..." is one comment, not two '/' tokens.
    private static Lexer SlashAndLineComment()
    {
        var lexer = new Lexer();
        lexer.AddTokenRule(new Terminal(TokenType.Operator.NormalOperator, "/", false));
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment, "//.*$", false, true));
        return lexer;
    }

    [Fact]
    public void Line_comment_lexes_as_one_comment_not_two_slashes()
    {
        var lexed = SlashAndLineComment().Lexing("//hi");
        var view = lexed.TokensForView;

        Assert.Single(view);
        Assert.Equal("//hi", view[0].Data);
        Assert.Equal(TokenType.SpecialToken.Comment, view[0].PatternInfo.Terminal.TokenType);

        // comments are filtered out of the parsing stream
        Assert.Empty(lexed.TokensForParsing);
    }

    [Fact]
    public void Single_slash_still_lexes_as_division_operator()
    {
        var tokens = SlashAndLineComment().Lexing("/").TokensForParsing;
        Assert.Equal(new[] { "/" }, tokens.Select(t => t.Data).ToArray());
    }

    [Fact]
    public void Line_comment_stops_at_end_of_line()
    {
        var view = SlashAndLineComment().Lexing("//hi\n/").TokensForView;

        Assert.Equal("//hi", view[0].Data);
        Assert.Equal(TokenType.SpecialToken.Comment, view[0].PatternInfo.Terminal.TokenType);
        Assert.Equal("/", view[view.Count - 1].Data);
        Assert.Equal(TokenType.Operator.NormalOperator, view[view.Count - 1].PatternInfo.Terminal.TokenType);
    }
}
