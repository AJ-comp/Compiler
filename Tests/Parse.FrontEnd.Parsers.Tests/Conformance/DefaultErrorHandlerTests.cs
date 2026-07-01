using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using System.Linq;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// With no custom error handler registered, a failed parse now surfaces a positioned error (via the
/// engine's grammar-agnostic DefaultLRErrorHandler) instead of an empty result — the basis an editor
/// needs for squiggles. Behaviour is otherwise unchanged (still fails, no recovery).
/// </summary>
[Trait("Category", "ErrorDefault")]
public class DefaultErrorHandlerTests
{
    private const string Grammar = @"
        S : 'a' 'b' ;
    ";

    private static LALRParser Parse(string input, out Janglim.FrontEnd.Parsers.Datas.ParsingResult result)
    {
        var read = EbnfGrammarReader.Read(Grammar);
        Assert.True(read.Success, string.Join("; ", read.Errors));

        var lexer = new Lexer();
        foreach (var t in read.Grammar.TerminalSet) lexer.AddTokenRule(t);

        var parser = new LALRParser(read.Grammar, false);
        result = parser.Parsing(lexer.Lexing(input).TokensForParsing);
        return parser;
    }

    [Fact]
    public void Failed_parse_surfaces_a_positioned_error()
    {
        // 'a' then another 'a' — 'b' was expected, so the second 'a' is the error.
        Parse("a a", out var result);

        Assert.False(result.Success);
        Assert.NotEmpty(result.AllErrors);

        var err = result.AllErrors.First();
        Assert.NotEmpty(err.ErrTokens);                       // the error carries the offending token
        var offending = err.ErrTokens.First();
        Assert.Equal("a", offending.Input);                   // the unexpected 2nd 'a' (not the '$' end-marker)
        Assert.Equal(2, offending.StartIndex);                // ...at its real source offset (for a squiggle)
    }

    [Fact]
    public void Valid_parse_has_no_errors()
    {
        Parse("a b", out var result);

        Assert.True(result.Success);
        Assert.Empty(result.AllErrors);
    }
}
