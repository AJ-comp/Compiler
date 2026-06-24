using System.Linq;
using Parse.FrontEnd.Grammars;
using Parse.FrontEnd.Grammars.ExampleGrammars;
using Parse.FrontEnd.Tokenize;
using Xunit;

namespace Parse.FrontEnd.Parsers.Tests;

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
}
