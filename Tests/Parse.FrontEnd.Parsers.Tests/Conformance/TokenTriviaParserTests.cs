using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// The existing EBNF -> lexer -> LALR pipeline must expose source trivia through
/// parser blocks, trees and lookahead traces, including when a previous parse is reused.
/// These tests do not implement JavaScript newline or semicolon rules.
/// </summary>
[Trait("Category", "Introspection")]
public class TokenTriviaParserTests
{
    [Theory]
    [InlineData(" ", false)]
    [InlineData("\t", false)]
    [InlineData("\n    ", true)]
    [InlineData("\r\n", true)]
    [InlineData(" /* comment */ ", false)]
    [InlineData(" /* comment\n */ ", true)]
    [InlineData("/* comment\u2028 */", true)]
    [InlineData("// comment\r\n", true)]
    public void Existing_ebnf_pipeline_forwards_the_exact_gap_to_parser_and_tree(string gap, bool hasLineBreak)
    {
        var grammar = Read("Start : 'return' id ';' ;");
        var lexed = CreateLexer(grammar).Lexing("return" + gap + "value\n;");
        var result = new LALRParser(grammar, true).Parsing(lexed.TokensForParsing);

        Assert.True(result.Success);
        AssertAllViewsMatchLexer(result, lexed);
        var value = Assert.Single(result.ToParseTree.AllTokens, token => token.Input == "value");
        Assert.Equal(gap, value.LeadingTrivia.Text);
        Assert.Equal(6, value.LeadingTrivia.StartIndex);
        Assert.Equal(hasLineBreak, value.HasLineBreakBefore);
    }

    [Fact]
    public void Failed_parse_retains_trivia_on_the_unrecognized_input_token()
    {
        var grammar = Read("Start : 'return' id ';' ;");
        var lexed = CreateLexer(grammar).Lexing("return /* reason\n */ @;");
        var result = new LALRParser(grammar, true).Parsing(lexed.TokensForParsing);

        Assert.False(result.Success);
        var errorToken = Assert.Single(result, block => block.Token.Input == "@").Token;
        Assert.Equal(" /* reason\n */ ", errorToken.LeadingTrivia.Text);
        Assert.True(errorToken.HasLineBreakBefore);
        Assert.Same(lexed.TokensForParsing[1], errorToken.TokenCell);
    }

    [Theory]
    [InlineData("a; b;", "a;\nb;")]
    [InlineData("a; b;", "a;/* comment\n */b;")]
    [InlineData("a; b;", "z;  b;")]
    [InlineData("a; b;", "alpha;\nb;")]
    [InlineData("a; b;", " a; b;")]
    [InlineData("a; b;", "a;\nb; c;")]
    [InlineData("a; b; c;", "a;\nc;")]
    public void Previous_parse_cannot_leave_stale_trivia_in_blocks_trees_or_trace(string before, string after)
    {
        var grammar = Read("Start : Statement+ ; Statement : id ';' ;");
        var lexer = CreateLexer(grammar);
        var parser = new LALRParser(grammar, true);
        var lexed = lexer.Lexing(before);
        var previous = parser.Parsing(lexed.TokensForParsing);
        Assert.True(previous.Success);

        // Replace the source through the public incremental storage API. Identical words
        // in the replacement are new cells and must not leave cached wrappers behind.
        var replacement = lexer.Lexing(after);
        lexed.ReplaceToken(new Janglim.Range(0, lexed.TokensForView.Count), replacement.TokensForView);
        var result = parser.Parsing(lexed, previous);

        Assert.True(result.Success);
        AssertAllViewsMatchLexer(result, lexed);
        Assert.Equal(string.Concat(lexed.TokensForParsing.Select(token => token.Data)), result.ToParseTree.AllInputDatas);
    }

    [Theory]
    [InlineData("a; b;", "a;\nb;")]
    [InlineData("a; b;", "z;  b;")]
    [InlineData("a; b;", "alpha;/* comment\n */b;")]
    public void Tagged_tree_grammar_preserves_trivia_in_ast_after_incremental_edit(string before, string after)
    {
        var grammar = new PairGrammar();
        var lexer = CreateLexer(grammar);
        var parser = new LALRParser(grammar, true);
        var lexed = lexer.Lexing(before);
        var previous = parser.Parsing(lexed.TokensForParsing);
        Assert.True(previous.Success);
        Assert.NotNull(previous.AstRoot);

        lexed.ReplaceToken(new Janglim.Range(0, lexed.TokensForView.Count), lexer.Lexing(after).TokensForView);
        var result = parser.Parsing(lexed, previous);

        Assert.True(result.Success);
        Assert.NotNull(result.AstRoot);
        Assert.Equal(lexed.TokensForParsing.Where(token => token.PatternInfo.Terminal.Meaning).Select(token => token.Data),
            result.AstRoot.AllTokens.Select(token => token.Input));
        AssertAllViewsMatchLexer(result, lexed);
    }

    [Fact]
    public void Retained_parser_tokens_see_refreshed_trivia_without_replacing_their_cells()
    {
        var grammar = Read("Start : Statement+ ; Statement : id ';' ;");
        var lexer = CreateLexer(grammar);
        var parser = new LALRParser(grammar, true);
        var lexed = lexer.Lexing("a; b;");
        var previous = parser.Parsing(lexed.TokensForParsing);
        var b = previous[2].Token;

        lexed.ReplaceToken(new Janglim.Range(2, 1), lexer.Lexing("/* comment\n */").TokensForView);
        var result = parser.Parsing(lexed, previous);

        Assert.True(result.Success);
        Assert.Same(previous, result);
        Assert.Same(b, result[2].Token);
        Assert.True(b.HasLineBreakBefore);
        Assert.Equal("/* comment\n */", b.LeadingTrivia.Text);
        AssertAllViewsMatchLexer(result, lexed);
    }

    [Fact]
    public void Trailing_trivia_is_available_on_lexing_data_and_not_invented_for_synthetic_eof()
    {
        var grammar = Read("Start : id ';' ;");
        var lexed = CreateLexer(grammar).Lexing("a; /* end\n */");
        var result = new LALRParser(grammar, true).Parsing(lexed.TokensForParsing);

        Assert.True(result.Success);
        Assert.Equal(" /* end\n */", lexed.TrailingTrivia.Text);
        Assert.True(lexed.TrailingTrivia.HasLineBreak);
        Assert.IsType<EndMarker>(result.Last().Token.Kind);
        Assert.Equal("", result.Last().Token.LeadingTrivia.Text);
        Assert.False(result.Last().Token.HasLineBreakBefore);
    }

    private static EbnfGrammar Read(string rules)
    {
        var read = EbnfGrammarReader.Read(rules + """

            id := "[a-z]+" ;
            comment := Comment "/\*[\s\S]*?\*/|//[^\r\n]*" ;
            """);
        Assert.True(read.Success, string.Join("; ", read.Errors));

        return read.Grammar;
    }

    private static Lexer CreateLexer(Grammar grammar)
    {
        var lexer = new Lexer();
        foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
        return lexer;
    }

    private static void AssertAllViewsMatchLexer(ParsingResult result, LexingData lexed)
    {
        Assert.Equal(lexed.TokensForParsing.Count + 1, result.Count);
        AssertTokensMatch(result.Take(lexed.TokensForParsing.Count).Select(block => block.Token), lexed);
        AssertTokensMatch(result.ToParseTree.AllTokens, lexed);

        if (result.AstRoot != null)
        {
            Assert.NotEmpty(result.AstRoot.AllTokens);
            foreach (var token in result.AstRoot.AllTokens)
                AssertTokenMatches(token, lexed);
        }

        var lookaheads = result.Trace.Steps.Select(step => step.Lookahead)
            .Where(token => token != null && token.StartIndex >= 0).ToArray();
        Assert.NotEmpty(lookaheads);
        foreach (var token in lookaheads) AssertTokenMatches(token, lexed);
    }

    private static void AssertTokensMatch(IEnumerable<TokenData> tokens, LexingData lexed)
    {
        var actual = tokens.ToArray();
        Assert.Equal(lexed.TokensForParsing.Select(token => token.Data), actual.Select(token => token.Input));
        foreach (var token in actual) AssertTokenMatches(token, lexed);
    }

    private static void AssertTokenMatches(TokenData actual, LexingData lexed)
    {
        var expected = Assert.Single(lexed.TokensForParsing, token => token.StartIndex == actual.StartIndex);
        Assert.Equal(expected.Data, actual.Input);
        Assert.Equal(expected.LeadingTrivia.Text, actual.LeadingTrivia.Text);
        Assert.Equal(expected.LeadingTrivia.StartIndex, actual.LeadingTrivia.StartIndex);
        Assert.Equal(expected.LeadingTrivia.EndIndexExclusive, actual.LeadingTrivia.EndIndexExclusive);
        Assert.Equal(expected.HasLineBreakBefore, actual.HasLineBreakBefore);
    }

    private sealed class PairGrammar : Grammar
    {
        private readonly Terminal id = new Terminal(TokenType.Identifier, "[a-z]+", "id", true, true);
        private readonly Terminal semicolon = new Terminal(TokenType.Operator, ";", false);
        private readonly Terminal comment = new Terminal(TokenType.SpecialToken.Comment, "/\\*[\\s\\S]*?\\*/", "comment", false, true);
        private readonly NonTerminal start = new NonTerminal("Start", true);

        public override NonTerminal EbnfRoot => start;

        public PairGrammar()
        {
            start.SetItem(id + semicolon + id + semicolon, new MeaningUnit("Pair"));
            Optimization();
        }
    }
}
