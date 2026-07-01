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
    [InlineData("A : a*+ ;", "stacked")]               // stacked quantifier (a*+)
    [InlineData("A : ( a ;", "')'")]                   // unterminated group
    public void Read_reports_errors(string text, string expectedFragment)
    {
        var result = EbnfGrammarReader.Read(text);

        Assert.False(result.Success);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains(expectedFragment));
    }

    // ---- quantifiers ( * + ? ) and groups ( … ) ----

    [Theory]
    [InlineData("a")]              // zero repetitions of the group
    [InlineData("a , b")]          // one
    [InlineData("a , b , c")]      // many
    public void ZeroOrMore_group_parses(string input)
    {
        const string grammar = @"
            List : id ( ',' id )* ;
            id   := ""[a-zA-Z]+"" ;
        ";
        AssertParses(grammar, input, expected: true);
    }

    [Fact]
    public void ZeroOrMore_group_rejects_trailing_separator()
    {
        const string grammar = @"
            List : id ( ',' id )* ;
            id   := ""[a-zA-Z]+"" ;
        ";
        AssertParses(grammar, "a ,", expected: false);   // trailing ',' with no following id
    }

    [Theory]
    [InlineData("x")]
    [InlineData("x x")]
    [InlineData("x x x")]
    public void OneOrMore_parses(string input)
    {
        const string grammar = @"
            Words : w+ ;
            w     := ""[a-z]+"" ;
        ";
        AssertParses(grammar, input, expected: true);
    }

    [Theory]
    [InlineData("var a ;")]        // optional absent
    [InlineData("var a = b ;")]    // optional present
    public void Optional_parses(string input)
    {
        const string grammar = @"
            Decl : 'var' id ( '=' id )? ';' ;
            id   := ""[a-zA-Z]+"" ;
        ";
        AssertParses(grammar, input, expected: true);
    }

    [Theory]
    [InlineData("x")]
    [InlineData("y")]
    [InlineData("x y x")]
    public void Group_with_alternation_parses(string input)
    {
        const string grammar = @"
            Seq : ( 'x' | 'y' )+ ;
        ";
        AssertParses(grammar, input, expected: true);
    }

    // Adversarial (review finding #1): an optional as its OWN named rule, used before a required
    // symbol. AbsorbOptionals must make this conflict-free and correct — no silent misparse.
    [Theory]
    [InlineData("x", true)]        // opt absent
    [InlineData("a x", true)]      // opt present
    [InlineData("a a x", false)]   // opt is single -> two a's must be rejected
    public void Optional_named_rule_before_symbol(string input, bool expected)
    {
        const string grammar = @"
            Rule : opt 'x' ;
            opt  : 'a'? ;
        ";
        AssertParses(grammar, input, expected);
    }

    // Adversarial (review finding #1): an optional nested inside a group inside a '+'.
    [Theory]
    [InlineData("b", true)]
    [InlineData("a b", true)]
    [InlineData("b b", true)]
    [InlineData("a b a b", true)]
    [InlineData("a b b", true)]
    [InlineData("a", false)]       // 'b' is required in each repetition
    public void Optional_inside_group_inside_oneOrMore(string input, bool expected)
    {
        const string grammar = @"
            Reps : ( 'a'? 'b' )+ ;
        ";
        AssertParses(grammar, input, expected);
    }

    private static void AssertParses(string grammar, string input, bool expected)
    {
        var read = EbnfGrammarReader.Read(grammar);
        Assert.True(read.Success, string.Join("; ", read.Errors));

        var lexer = new Lexer();
        foreach (var term in read.Grammar.TerminalSet) lexer.AddTokenRule(term);

        var parse = new LALRParser(read.Grammar, false).Parsing(lexer.Lexing(input).TokensForParsing);
        Assert.Equal(expected, parse.Success);
    }
}
