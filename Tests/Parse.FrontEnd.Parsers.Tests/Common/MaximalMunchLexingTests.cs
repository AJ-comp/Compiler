using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// The eager lexer and parser-driven source choose the longest actual match at the
/// current offset. Equal matches retain category priority and registration order.
/// Each regex keeps its own .NET alternation, lazy-quantifier and boundary semantics.
/// </summary>
[Trait("Category", "Common")]
public class MaximalMunchLexingTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Identifier_patterns_accept_dollar_at_either_end_without_implicit_word_boundaries(bool source)
    {
        var lexer = WithWhitespace();
        var identifier = Identifier("[$A-Za-z_][$0-9A-Za-z_]*");
        lexer.AddTokenRule(identifier);

        var tokens = Read(lexer, "$name name$ $ $$ _x x_ $name$", source);

        Assert.Equal(new[] { "$name", "name$", "$", "$$", "_x", "x_", "$name$" }, tokens.Select(t => t.Text));
        Assert.All(tokens, token => Assert.Same(identifier, token.Kind));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Longer_identifiers_beat_keyword_prefixes_and_equal_keywords_keep_priority(bool source)
    {
        var lexer = WithWhitespace();
        var identifier = Identifier("[$A-Za-z_][$0-9A-Za-z_]*");
        // Register the identifier first so keyword priority cannot accidentally rely on order.
        lexer.AddTokenRule(identifier);
        var keyword = new Terminal(TokenType.Keyword, "if", "IF");
        lexer.AddTokenRule(keyword);

        var tokens = Read(lexer, "if ifx if$ $if if_ if9", source);

        Assert.Equal(new[] { "if", "ifx", "if$", "$if", "if_", "if9" }, tokens.Select(t => t.Text));
        Assert.Same(keyword, tokens[0].Kind);
        Assert.All(tokens.Skip(1), token => Assert.Same(identifier, token.Kind));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Longest_match_uses_matched_input_length_instead_of_regex_source_length(bool source)
    {
        var lexer = new Lexer();
        var shortMatch = Identifier("a(?:)(?:)(?:)", "short");
        var longMatch = Identifier("a+", "long");
        Assert.True(shortMatch.Value.Length > longMatch.Value.Length);
        lexer.AddTokenRule(shortMatch);
        lexer.AddTokenRule(longMatch);

        var token = Assert.Single(Read(lexer, "aaaa", source));

        Assert.Same(longMatch, token.Kind);
        Assert.Equal("aaaa", token.Text);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_longer_match_beats_a_rule_in_a_higher_priority_category(bool source)
    {
        var lexer = new Lexer();
        var word = new Terminal(TokenType.Keyword, "a");
        var operation = new Terminal(TokenType.Operator, "ab");
        lexer.AddTokenRule(word);
        lexer.AddTokenRule(operation);

        var token = Assert.Single(Read(lexer, "ab", source));

        Assert.Same(operation, token.Kind);
        Assert.Equal("ab", token.Text);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Equal_matches_in_the_same_category_use_registration_order_not_pattern_length(bool source)
    {
        var first = Identifier("a+", "first");
        var later = Identifier("a{2}", "later");
        Assert.True(first.Value.Length < later.Value.Length);

        var lexer = new Lexer();
        lexer.AddTokenRule(first);
        lexer.AddTokenRule(later);
        Assert.Same(first, Assert.Single(Read(lexer, "aa", source)).Kind);

        var reverse = new Lexer();
        reverse.AddTokenRule(later);
        reverse.AddTokenRule(first);
        Assert.Same(later, Assert.Single(Read(reverse, "aa", source)).Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Equal_keyword_and_operator_matches_keep_the_existing_category_priority(bool source)
    {
        var lexer = new Lexer();
        var operation = new Terminal(TokenType.Operator, "if");
        var keyword = new Terminal(TokenType.Keyword, "if");
        lexer.AddTokenRule(operation);
        lexer.AddTokenRule(keyword);

        Assert.Same(keyword, Assert.Single(Read(lexer, "if", source)).Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Adding_more_rules_does_not_reorder_existing_equal_length_winners(bool source)
    {
        var lexer = new Lexer();
        var first = Identifier("[ab]+", "first");
        lexer.AddTokenRule(first);
        lexer.AddTokenRule(Identifier("a+b*", "second"));
        Assert.Same(first, Assert.Single(Read(lexer, "ab", source)).Kind);

        lexer.AddTokenRule(Identifier("(?:a)(?:b)", "third"));
        lexer.AddTokenRule(new Terminal(TokenType.Operator, "+"));

        Assert.Same(first, Assert.Single(Read(lexer, "ab", source)).Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Overlapping_operators_choose_the_longest_sequence_and_keep_the_remaining_input(bool source)
    {
        var lexer = WithWhitespace();
        var identifier = Identifier("[a-z]+");
        lexer.AddTokenRule(identifier);
        foreach (string value in new[] { ">", ">=", ">>", ">>=", ">>>", ">>>=", "=", "+", "++" })
            lexer.AddTokenRule(new Terminal(TokenType.Operator, value));

        var tokens = Read(lexer, "a>>>=b a>>=b a>=b a>>>>b a+++b", source);

        Assert.Equal(new[]
        {
            "a", ">>>=", "b", "a", ">>=", "b", "a", ">=", "b",
            "a", ">>>", ">", "b", "a", "++", "+", "b"
        }, tokens.Select(t => t.Text));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Real_numbers_beat_integer_prefixes_and_dot_operators(bool source)
    {
        var lexer = WithWhitespace();
        var integer = new Terminal(TokenType.Literal.Digit10, "[0-9]+", "integer", true, true);
        var real = new Terminal(TokenType.Literal.Digit10, "[0-9]+\\.[0-9]+", "real", true, true);
        lexer.AddTokenRule(integer);
        lexer.AddTokenRule(real);
        lexer.AddTokenRule(new Terminal(TokenType.Operator, "."));

        var tokens = Read(lexer, "12.34 5 . 6", source);

        Assert.Equal(new[] { "12.34", "5", ".", "6" }, tokens.Select(t => t.Text));
        Assert.Same(real, tokens[0].Kind);
        Assert.Same(integer, tokens[1].Kind);
        Assert.Same(integer, tokens[3].Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Comments_beat_slash_and_lazy_block_comments_stop_at_the_first_closing_delimiter(bool source)
    {
        var lexer = WithWhitespace();
        var identifier = Identifier("[a-z]+");
        lexer.AddTokenRule(identifier);
        lexer.AddTokenRule(new Terminal(TokenType.Operator, "/"));
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment,
            @"/\*[\s\S]*?\*/|//[^\r\n]*", "comment", false, true));
        const string text = "a/* first */b/* second */ /c// tail\r\nd";

        var tokens = Read(lexer, text, source);

        Assert.Equal(new[] { "a", "b", "/", "c", "d" }, tokens.Select(t => t.Text));
        Assert.Equal("/* first */", tokens[1].Trivia);
        Assert.Equal("/* second */ ", tokens[2].Trivia);
        Assert.Equal("// tail\r\n", tokens[4].Trivia);
        Assert.True(tokens[4].LineBreak);
        Assert.Equal(text.LastIndexOf('d'), tokens[4].Start);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Explicit_user_word_boundaries_are_retained(bool source)
    {
        var lexer = WithWhitespace();
        var bounded = Identifier(@"\bfoo\b", "bounded");
        var fallback = Identifier("[a-z]+", "fallback");
        lexer.AddTokenRule(bounded);
        lexer.AddTokenRule(fallback);

        var tokens = Read(lexer, "xfoo foo foox", source);

        Assert.Equal(new[] { "xfoo", "foo", "foox" }, tokens.Select(t => t.Text));
        Assert.Same(fallback, tokens[0].Kind);
        Assert.Same(bounded, tokens[1].Kind);
        Assert.Same(fallback, tokens[2].Kind);
        Assert.Equal(@"\bfoo\b", bounded.RegexExpression);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Patterns_match_at_the_current_offset_and_keep_lookbehind_context(bool source)
    {
        var lexer = WithWhitespace();
        var first = Identifier("x", "x");
        var second = Identifier("(?<=x)ab+", "after-x");
        lexer.AddTokenRule(first);
        lexer.AddTokenRule(second);

        var tokens = Read(lexer, "xabb", source);

        Assert.Equal(new[] { "x", "abb" }, tokens.Select(t => t.Text));
        Assert.Equal(new[] { 0, 1 }, tokens.Select(t => t.Start));
        Assert.Same(second, tokens[1].Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Regex_internal_alternation_and_lazy_quantifiers_keep_their_authored_semantics(bool source)
    {
        var lexer = new Lexer();
        var alternative = Identifier("a|ab", "alternative");
        var remaining = Identifier("b", "remaining");
        lexer.AddTokenRule(alternative);
        lexer.AddTokenRule(remaining);

        var tokens = Read(lexer, "ab", source);

        Assert.Equal(new[] { "a", "b" }, tokens.Select(t => t.Text));
        Assert.Same(alternative, tokens[0].Kind);
        Assert.Same(remaining, tokens[1].Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Active_zero_width_rules_are_reported_even_when_another_rule_can_consume_input(bool source)
    {
        var lexer = new Lexer();
        lexer.AddTokenRule(Identifier("x", "consuming"));
        lexer.AddTokenRule(Identifier("(?=x)", "zero-width"));

        var error = Assert.Throws<EmptyTokenMatchException>(() => Read(lexer, "x", source));
        Assert.Equal(0, error.SourceOffset);
        Assert.Equal("zero-width", error.Terminal.Caption);
    }

    [Fact]
    public void A_goal_limits_longest_match_to_its_active_rules()
    {
        var lexer = new Lexer();
        var shortToken = Identifier("a", "short");
        var longToken = Identifier("ab", "long");
        lexer.AddTokenRule(shortToken);
        lexer.AddTokenRule(longToken);
        var shortGoal = new LexicalGoal(new[] { shortToken });
        var bothGoal = new LexicalGoal(new[] { shortToken, longToken });
        var source = lexer.CreateTokenSource("ab", new LexicalPolicy(shortGoal, bothGoal));

        Assert.Same(shortToken, source.Peek(source.Start, shortGoal).Token.Kind);
        var longest = source.Peek(source.Start, bothGoal);
        Assert.Same(longToken, longest.Token.Kind);
        Assert.Equal("ab", longest.Token.Input);
        Assert.Equal(0, source.Start.SourceOffset);
        Assert.Equal(2, longest.Next.SourceOffset);
    }

    [Fact]
    public void Speculative_longest_matching_does_not_consume_the_cursor_or_mutate_prior_reads()
    {
        var lexer = WithWhitespace();
        var shortToken = Identifier("a", "short");
        var longToken = Identifier("ab", "long");
        lexer.AddTokenRule(shortToken);
        lexer.AddTokenRule(longToken);
        var goal = new LexicalGoal(new[] { shortToken, longToken });
        var source = lexer.CreateTokenSource(" ab", new LexicalPolicy(goal));

        var first = source.Peek(source.Start, goal);
        var second = source.Peek(source.Start, goal);

        Assert.Equal(0, source.Start.SourceOffset);
        Assert.Equal(0, source.Start.TokenIndex);
        Assert.Same(longToken, first.Token.Kind);
        Assert.Same(longToken, second.Token.Kind);
        Assert.NotSame(first.Token, second.Token);
        Assert.Equal(" ", second.Token.LeadingTrivia.Text);
        Assert.Equal(1, second.Token.StartIndex);
        Assert.Equal(3, second.Next.SourceOffset);
        Assert.Equal(1, second.Next.TokenIndex);
    }

    [Theory]
    [InlineData("if value", 2, "x")]
    [InlineData("name", 0, "$")]
    [InlineData("name", 4, "$")]
    [InlineData("a>b", 2, ">")]
    [InlineData("a>>b", 3, "=")]
    public void Incremental_insertions_reselect_the_longest_token_and_match_fresh_lexing(
        string text, int offset, string inserted)
    {
        var lexer = EditingLexer();
        var incremental = lexer.Lexing(text);

        incremental = lexer.Lexing(incremental, offset, inserted);

        AssertSameLexing(lexer.Lexing(text.Insert(offset, inserted)), incremental);
    }

    [Theory]
    [InlineData("ifx value", 2, 1)]
    [InlineData("$name", 0, 1)]
    [InlineData("name$", 4, 1)]
    [InlineData("a> >b", 2, 1)]
    [InlineData("a>>=b", 3, 1)]
    public void Incremental_deletions_reselect_keyword_identifier_and_operator_matches(
        string text, int offset, int count)
    {
        var lexer = EditingLexer();
        var incremental = lexer.Lexing(text);

        incremental = lexer.Lexing(incremental, offset, count);

        AssertSameLexing(lexer.Lexing(text.Remove(offset, count)), incremental);
    }

    private static Lexer EditingLexer()
    {
        var lexer = WithWhitespace();
        lexer.AddTokenRule(Identifier("[$A-Za-z_][$0-9A-Za-z_]*"));
        lexer.AddTokenRule(new Terminal(TokenType.Keyword, "if"));
        foreach (string value in new[] { ">", ">>", ">>=", "=" })
            lexer.AddTokenRule(new Terminal(TokenType.Operator, value));
        return lexer;
    }

    private static void AssertSameLexing(LexingData expected, LexingData actual)
    {
        Assert.Equal(expected.TokensForView.Select(token => token.Data),
            actual.TokensForView.Select(token => token.Data));
        Assert.Equal(expected.TokensForParsing.Select(token =>
                (token.PatternInfo.Terminal, token.Data, token.StartIndex, token.LeadingTrivia.Text)),
            actual.TokensForParsing.Select(token =>
                (token.PatternInfo.Terminal, token.Data, token.StartIndex, token.LeadingTrivia.Text)));
        Assert.Equal(expected.TrailingTrivia.Text, actual.TrailingTrivia.Text);
        Assert.Equal(expected.TrailingTrivia.StartIndex, actual.TrailingTrivia.StartIndex);
    }

    private static Terminal Identifier(string pattern, string name = "identifier")
        => new Terminal(TokenType.Identifier, pattern, name, true, true);

    private static Lexer WithWhitespace()
    {
        var lexer = new Lexer();
        foreach (char value in new[] { ' ', '\t', '\r', '\n' })
            lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Delimiter, value.ToString(), false));
        return lexer;
    }

    private static IReadOnlyList<Lexeme> Read(Lexer lexer, string text, bool contextual)
    {
        if (!contextual)
            return lexer.Lexing(text).TokensForParsing.Select(token => new Lexeme(
                token.PatternInfo.Terminal, token.Data, token.StartIndex,
                token.LeadingTrivia.Text, token.HasLineBreakBefore)).ToArray();

        var goal = new LexicalGoal(lexer.TokenPatternList.Select(pattern => pattern.Terminal));
        var source = lexer.CreateTokenSource(text, new LexicalPolicy(goal));
        var cursor = source.Start;
        var result = new List<Lexeme>();
        for (int i = 0; i <= text.Length; i++)
        {
            var read = source.Peek(cursor, goal);
            if (read.Token.Kind is EndMarker) return result;
            Assert.True(read.Next.SourceOffset > cursor.SourceOffset);
            result.Add(new Lexeme(read.Token.Kind, read.Token.Input, read.Token.StartIndex,
                read.Token.LeadingTrivia.Text, read.Token.HasLineBreakBefore));
            cursor = read.Next;
        }
        throw new InvalidOperationException("The token source failed to reach EOF.");
    }

    private sealed record Lexeme(Terminal Kind, string Text, int Start, string Trivia, bool LineBreak);
}
