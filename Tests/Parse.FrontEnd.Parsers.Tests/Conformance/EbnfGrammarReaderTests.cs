using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// End-to-end for the text path: EBNF grammar TEXT -> EbnfGrammarReader -> a runnable grammar that
/// actually parses input. Plus the reader's error reporting for malformed grammar text.
/// </summary>
[Trait("Category", "Ebnf")]
public class EbnfGrammarReaderTests
{
    private const string Arithmetic = @"
        # a tiny arithmetic grammar
        Expr   : Expr '+' Term | Term ;
        Term   : Term '*' Factor | Factor ;
        Factor : '(' Expr ')' | id ;
        id     := ""[a-zA-Z]+"" ;
    ";

    [Theory]
    [InlineData("a + a * a")]
    [InlineData("( a + a ) * a")]
    [InlineData("a")]
    public void Read_text_then_parse_input(string input)
    {
        var result = EbnfGrammarReader.Read(Arithmetic);
        Assert.True(result.Success, string.Join("; ", result.Errors));

        var g = result.Grammar;
        var lexer = new Lexer();
        foreach (var term in g.TerminalSet) lexer.AddTokenRule(term);

        var parse = new LALRParser(g, false).Parsing(lexer.Lexing(input).TokensForParsing);
        Assert.True(parse.Success, $"expected to parse: \"{input}\"");
    }

    // A word-like keyword literal ('h') overlaps the identifier token rule. The lexer must recognize
    // it as the keyword, not swallow it as an identifier — otherwise the rule never matches. (Regression
    // guard for the EbnfGrammar literal: word-like literals take bWordPattern=false so they out-rank id.)
    [Theory]
    [InlineData("h q;")]
    [InlineData("h foo;")]
    public void Keyword_literal_wins_over_identifier(string input)
    {
        const string grammar = @"
            Gate : 'h' id ';' ;
            id   := ""[a-zA-Z_][a-zA-Z0-9_]*"" ;
        ";

        var read = EbnfGrammarReader.Read(grammar);
        Assert.True(read.Success, string.Join("; ", read.Errors));

        var lexer = new Lexer();
        foreach (var term in read.Grammar.TerminalSet) lexer.AddTokenRule(term);

        var parse = new LALRParser(read.Grammar, false).Parsing(lexer.Lexing(input).TokensForParsing);
        Assert.True(parse.Success, $"the keyword 'h' should be recognized (not lexed as an identifier) for: \"{input}\"");
    }

    [Theory]
    [InlineData("Expr : foo ;", "undefined")]          // reference to an undefined symbol
    [InlineData("Expr : id | ;", "empty")]             // empty alternative after '|'
    [InlineData("Expr : id", "';'")]                   // missing terminator
    [InlineData("id := \"[a-z]\" ;", "non-terminal")]  // only a token rule, no start rule
    [InlineData("", "non-terminal")]                   // nothing
    public void Read_reports_errors(string text, string expectedFragment)
    {
        var result = EbnfGrammarReader.Read(text);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains(expectedFragment));
    }
}
