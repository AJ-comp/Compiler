using System.Linq;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

[Trait("Category", "Ebnf")]
public class MaximalMunchGrammarTests
{
    private const string KeywordAndIdentifier = """
        Statement : 'if' id ';' | id ';' ;
        id := Identifier "[$A-Za-z_][$0-9A-Za-z_]*" ;
        comment := Comment "/\*[\s\S]*?\*/|//[^\r\n]*" ;
        """;

    [Theory]
    [InlineData("if value;")]
    [InlineData("if ifx;")]
    [InlineData("if $name;")]
    [InlineData("if name$;")]
    [InlineData("ifx;")]
    [InlineData("if$;")]
    [InlineData("$name;")]
    [InlineData("name$;")]
    [InlineData("$;")]
    [InlineData("$$;")]
    [InlineData("if/* comment */$name;")]
    [InlineData("if// comment\r\nname$;")]
    public void Ebnf_keywords_and_dollar_identifiers_parse_through_both_lexer_entry_points(string input)
    {
        AssertParsesBoth(KeywordAndIdentifier, input);
    }

    [Theory]
    [InlineData("if;")]
    [InlineData("if value extra;")]
    [InlineData("if $name")]
    [InlineData("$name$ other;")]
    public void Longest_matching_keeps_keywords_reserved_and_does_not_hide_invalid_input(string input)
    {
        AssertParsesBoth(KeywordAndIdentifier, input, expected: false);
    }

    [Theory]
    [InlineData("a>b;")]
    [InlineData("a>=b;")]
    [InlineData("a>>b;")]
    [InlineData("a>>=b;")]
    [InlineData("a>>>b;")]
    [InlineData("a>>>=b;")]
    public void Ebnf_overlapping_operators_parse_as_one_longest_token(string input)
    {
        const string grammar = """
            Statement : id Operation id ';' ;
            Operation : '>' | '>=' | '>>' | '>>=' | '>>>' | '>>>=' ;
            id := Identifier "[a-z]+" ;
            """;
        AssertParsesBoth(grammar, input);
    }

    [Theory]
    [InlineData("12.34+5", true)]
    [InlineData("12+5.67", true)]
    [InlineData("12.34.56+5", false)]
    public void Ebnf_numeric_patterns_choose_the_longest_match_without_implicit_boundaries(string input, bool expected)
    {
        const string grammar = """
            Sum : Number '+' Number ;
            Number : integer | real ;
            integer := NumberLiteral "[0-9]+" ;
            real := NumberLiteral "[0-9]+\.[0-9]+" ;
            """;
        AssertParsesBoth(grammar, input, expected);
    }

    private static void AssertParsesBoth(string grammarText, string input, bool expected = true)
    {
        var read = EbnfGrammarReader.Read(grammarText);
        Assert.True(read.Success, string.Join("; ", read.Errors));
        var lexer = new Lexer();
        foreach (var terminal in read.Grammar.TerminalSet) lexer.AddTokenRule(terminal);
        var parser = new LALRParser(read.Grammar, false);
        var lexed = lexer.Lexing(input);

        var legacy = parser.Parsing(lexed.TokensForParsing);
        Assert.Equal(expected, legacy.Success);
        var goal = new LexicalGoal(lexer.TokenPatternList.Select(pattern => pattern.Terminal));
        var contextual = parser.Parsing(lexer.CreateTokenSource(input, new LexicalPolicy(goal)));
        Assert.Equal(expected, contextual.Success);
        if (!expected) return;

        Assert.Equal(legacy.ToParseTree.AllTokens.Select(token => (token.Kind, token.Input, token.StartIndex)),
            contextual.ToParseTree.AllTokens.Select(token => (token.Kind, token.Input, token.StartIndex)));
        Assert.Empty(legacy.AllErrors);
        Assert.Empty(contextual.AllErrors);
    }
}
