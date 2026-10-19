using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Trivia belongs to the gap before a parsing token, not to the token's own text.
/// These tests check source preservation and the public lexer/editing paths without
/// assigning any language-specific meaning to a line break.
/// </summary>
[Trait("Category", "Common")]
public class TokenTriviaTests
{
    [Fact]
    public void Return_example_preserves_each_gap_and_existing_token_offsets()
    {
        const string source = "return\n    value\n;";
        var lexed = CreateLexer().Lexing(source);
        var tokens = lexed.TokensForParsing;

        Assert.Equal(new[] { "return", "value", ";" }, tokens.Select(token => token.Data));
        Assert.Equal(new[] { 0, 11, 17 }, tokens.Select(token => token.StartIndex));
        AssertGap(tokens[0], "", 0, false);
        AssertGap(tokens[1], "\n    ", 6, true);
        AssertGap(tokens[2], "\n", 16, true);
        AssertTrivia(lexed.TrailingTrivia, "", source.Length, false);
        AssertReconstructs(source, lexed);
    }

    [Fact]
    public void Adjacent_tokens_have_empty_gaps_at_the_current_token_start()
    {
        const string source = "a();";
        var lexed = CreateLexer().Lexing(source);

        Assert.Equal(new[] { "a", "(", ")", ";" }, lexed.TokensForParsing.Select(token => token.Data));
        foreach (var token in lexed.TokensForParsing)
            AssertGap(token, "", token.StartIndex, false);
        AssertReconstructs(source, lexed);
    }

    [Fact]
    public void Leading_and_trailing_trivia_are_preserved_at_file_boundaries()
    {
        const string source = " \t/* head */\r\na /* tail\n */\t";
        var lexed = CreateLexer().Lexing(source);
        var token = Assert.Single(lexed.TokensForParsing);

        Assert.Equal("a", token.Data);
        AssertGap(token, " \t/* head */\r\n", 0, true);
        AssertTrivia(lexed.TrailingTrivia, " /* tail\n */\t", token.EndIndex + 1, true);
        AssertReconstructs(source, lexed);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t")]
    [InlineData("\r\n \u2028\u2029")]
    [InlineData("/* only a comment */")]
    [InlineData("/* only\na comment */\t")]
    [InlineData("// only a comment\r\n")]
    public void Files_without_parsing_tokens_keep_the_whole_input_as_trailing_trivia(string source)
    {
        var lexed = CreateLexer().Lexing(source);

        Assert.Empty(lexed.TokensForParsing);
        AssertTrivia(lexed.TrailingTrivia, source, 0,
            source.IndexOfAny(new[] { '\r', '\n', '\u2028', '\u2029' }) >= 0);
        AssertReconstructs(source, lexed);
    }

    [Theory]
    [InlineData("\r")]
    [InlineData("\n")]
    [InlineData("\r\n")]
    [InlineData("\u2028")]
    [InlineData("\u2029")]
    [InlineData(" \t\r\n\n\u2028\u2029 ")]
    public void Registered_line_break_delimiters_are_detected_without_changing_their_text(string gap)
    {
        var source = "a" + gap + "b";
        var lexed = CreateLexer().Lexing(source);

        Assert.Equal(new[] { "a", "b" }, lexed.TokensForParsing.Select(token => token.Data));
        AssertGap(lexed.TokensForParsing[1], gap, 1, true);
        AssertReconstructs(source, lexed);
    }

    [Theory]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\v\f\u0085")]
    [InlineData(" /* comment */ \t")]
    [InlineData("/* literal \\n, not a line break */")]
    public void Trivia_without_a_supported_line_break_is_not_marked_as_one(string gap)
    {
        var lexed = CreateLexer().Lexing("a" + gap + "b");

        AssertGap(lexed.TokensForParsing[1], gap, 1, false);
        AssertReconstructs("a" + gap + "b", lexed);
    }

    [Fact]
    public void Consecutive_comments_and_whitespace_form_one_exact_gap()
    {
        const string gap = " /* first */\t/* second\r\nline */ // third\n ";
        var lexed = CreateLexer().Lexing("a" + gap + "b");

        Assert.Equal(3, lexed.TokensForView.Count(token => token.PatternInfo.Terminal.TokenType is Comment));
        AssertGap(lexed.TokensForParsing[1], gap, 1, true);
        AssertReconstructs("a" + gap + "b", lexed);
    }

    [Fact]
    public void Line_comment_subtype_contributes_to_the_gap()
    {
        var lexer = CreateLexer(includeComments: false);
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment.LineComment,
            "//[^\\r\\n\\u2028\\u2029]*", "lineComment", false, true));
        const string source = "a// explanation\r\nb";
        var lexed = lexer.Lexing(source);

        Assert.Equal(new[] { "a", "b" }, lexed.TokensForParsing.Select(token => token.Data));
        AssertGap(lexed.TokensForParsing[1], "// explanation\r\n", 1, true);
        AssertReconstructs(source, lexed);
    }

    [Fact]
    public void Scope_comment_delimiters_remain_parsing_tokens()
    {
        var lexer = CreateLexer(includeComments: false);
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "/*", false));
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "*/", false));
        const string source = "a /*\nb */ c";
        var lexed = lexer.Lexing(source);

        Assert.Equal(new[] { "a", "/*", "b", "*/", "c" }, lexed.TokensForParsing.Select(token => token.Data));
        AssertGap(lexed.TokensForParsing[1], " ", 1, false);
        AssertGap(lexed.TokensForParsing[2], "\n", 4, true);
        AssertGap(lexed.TokensForParsing[3], " ", 6, false);
        AssertGap(lexed.TokensForParsing[4], " ", 9, false);
        AssertReconstructs(source, lexed);
    }

    [Fact]
    public void Token_internal_line_breaks_are_not_part_of_the_next_gap()
    {
        var lexer = CreateLexer();
        lexer.AddTokenRule(new Terminal(TokenType.Literal.StringLiteral, "`[^`]*`", "template", true, true));
        const string source = "`first\r\nsecond\u2028third` value";
        var lexed = lexer.Lexing(source);

        Assert.Equal(new[] { "`first\r\nsecond\u2028third`", "value" }, lexed.TokensForParsing.Select(token => token.Data));
        AssertGap(lexed.TokensForParsing[0], "", 0, false);
        AssertGap(lexed.TokensForParsing[1], " ", lexed.TokensForParsing[0].EndIndex + 1, false);
        AssertReconstructs(source, lexed);
    }

    [Fact]
    public void Explicit_line_break_parsing_token_is_not_duplicated_as_trivia()
    {
        var lexer = new Lexer();
        lexer.AddTokenRule(new Terminal(TokenType.Identifier, "[a-z]+", "id", true, true));
        lexer.AddTokenRule(new Terminal(TokenType.Literal.StringLiteral, "\\n", "lineBreak", true, true));
        const string source = "a\nb";
        var lexed = lexer.Lexing(source);

        Assert.Equal(new[] { "a", "\n", "b" }, lexed.TokensForParsing.Select(token => token.Data));
        AssertGap(lexed.TokensForParsing[1], "", 1, false);
        AssertGap(lexed.TokensForParsing[2], "", 2, false);
        AssertReconstructs(source, lexed);
    }

    [Fact]
    public void Undefined_tokens_are_preserved_in_the_parsing_stream()
    {
        const string source = "a @\n b";
        var lexed = CreateLexer().Lexing(source);

        Assert.Equal(new[] { "a", "@", "b" }, lexed.TokensForParsing.Select(token => token.Data));
        Assert.IsType<NotDefined>(lexed.TokensForParsing[1].PatternInfo.Terminal);
        AssertGap(lexed.TokensForParsing[1], " ", 1, false);
        AssertGap(lexed.TokensForParsing[2], "\n ", 3, true);
        AssertReconstructs(source, lexed);
    }

    [Fact]
    public void Unregistered_line_break_is_an_undefined_token_not_trivia()
    {
        var lexer = new Lexer();
        lexer.AddTokenRule(new Terminal(TokenType.Identifier, "[a-z]+", "id", true, true));
        var lexed = lexer.Lexing("a\nb");

        Assert.Equal(new[] { "a", "\n", "b" }, lexed.TokensForParsing.Select(token => token.Data));
        Assert.IsType<NotDefined>(lexed.TokensForParsing[1].PatternInfo.Terminal);
        Assert.All(lexed.TokensForParsing, token => Assert.False(token.HasLineBreakBefore));
        AssertReconstructs("a\nb", lexed);
    }

    [Fact]
    public void Trivia_ranges_use_utf16_offsets_and_preserve_unicode_exactly()
    {
        const string gap = "/* 😀 한글 日本語 */\r\n";
        var source = "a" + gap + "b";
        var lexed = CreateLexer().Lexing(source);
        var second = lexed.TokensForParsing[1];

        Assert.Equal(1 + gap.Length, second.StartIndex);
        Assert.Equal(gap.Length, second.LeadingTrivia.Length);
        Assert.Equal(second.StartIndex, second.LeadingTrivia.EndIndexExclusive);
        AssertGap(second, gap, 1, true);
        AssertReconstructs(source, lexed);
    }

    [Fact]
    public void New_lexing_storage_has_empty_trailing_trivia()
    {
        var storage = new LexingData(Array.Empty<TokenPatternInfo>());

        AssertTrivia(storage.TrailingTrivia, "", 0, false);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(12)]
    [InlineData(-1)]
    public void Existing_token_constructor_has_safe_empty_metadata(int startIndex)
    {
        var token = new TokenCell(startIndex, 0, 0, "value", null);

        AssertGap(token, "", startIndex, false);
    }

    [Fact]
    public void Token_data_forwards_the_same_metadata_as_its_token_cell()
    {
        var token = CreateLexer().Lexing("a /* comment\n */ b").TokensForParsing[1];
        var data = TokenData.CreateFromTokenCell(token, false);

        Assert.Same(token.LeadingTrivia, data.LeadingTrivia);
        Assert.Equal(" /* comment\n */ ", data.LeadingTrivia.Text);
        Assert.True(data.HasLineBreakBefore);
    }

    [Fact]
    public void Null_and_virtual_token_cells_have_safe_empty_metadata()
    {
        var terminal = new Terminal(TokenType.Identifier, "[a-z]+", "id", true, true);
        var withoutCell = new TokenData(terminal, null);
        var virtualToken = TokenData.CreateVirtualToken(terminal, "value");

        AssertTrivia(withoutCell.LeadingTrivia, "", -1, false);
        Assert.False(withoutCell.HasLineBreakBefore);
        AssertTrivia(virtualToken.LeadingTrivia, "", virtualToken.StartIndex, false);
        Assert.False(virtualToken.HasLineBreakBefore);
    }

    [Fact]
    public void Init_token_accepts_an_enumerable_that_can_only_be_enumerated_once()
    {
        var lexer = CreateLexer();
        const string source = " /* head */ a\n b \t";
        var cells = lexer.Lexing(source).TokensForView;
        var once = new SingleEnumeration<TokenCell>(cells);
        var storage = new LexingData(lexer.TokenPatternList);

        storage.InitToken(once);

        Assert.Equal(1, once.EnumerationCount);
        Assert.Equal(new[] { "a", "b" }, storage.TokensForParsing.Select(token => token.Data));
        AssertGap(storage.TokensForParsing[0], " /* head */ ", 0, false);
        AssertGap(storage.TokensForParsing[1], "\n ", 13, true);
        AssertTrivia(storage.TrailingTrivia, " \t", 16, false);
        AssertReconstructs(source, storage);
    }

    [Fact]
    public void Replacing_only_trivia_updates_the_existing_following_token()
    {
        var lexer = CreateLexer();
        var storage = lexer.Lexing("a \n b");
        var second = storage.TokensForParsing[1];
        var oldTrivia = second.LeadingTrivia;
        var replacement = new SingleEnumeration<TokenCell>(lexer.Lexing("/* replacement */ ").TokensForView);

        storage.ReplaceToken(new Janglim.Range(1, 3), replacement);

        Assert.Equal(1, replacement.EnumerationCount);
        Assert.Same(second, storage.TokensForParsing[1]);
        AssertGap(second, "/* replacement */ ", 1, false);
        Assert.Equal(" \n ", oldTrivia.Text);
        Assert.True(oldTrivia.HasLineBreak);
        AssertReconstructs("a/* replacement */ b", storage);
    }

    [Fact]
    public void Replacing_trailing_trivia_updates_the_end_boundary()
    {
        var lexer = CreateLexer();
        var storage = lexer.Lexing("a \n");

        storage.ReplaceToken(new Janglim.Range(1, 2), lexer.Lexing("/* tail */").TokensForView);

        AssertTrivia(storage.TrailingTrivia, "/* tail */", 1, false);
        AssertReconstructs("a/* tail */", storage);
        storage.ReplaceToken(new Janglim.Range(1, 1), Array.Empty<TokenCell>());
        AssertTrivia(storage.TrailingTrivia, "", 1, false);
        AssertReconstructs("a", storage);
    }

    [Theory]
    [InlineData("a b", 1, "\n")]
    [InlineData("a b", 0, "\t")]
    [InlineData("a b", 3, "\r\n")]
    [InlineData("a /*comment*/ b", 5, "\n")]
    [InlineData("a\n b\n c", 4, " /* extra\ncomment */ ")]
    [InlineData("", 0, " \n a")]
    public void Incremental_insertion_matches_fresh_lexing(string source, int offset, string inserted)
    {
        var lexer = CreateLexer();
        var incremental = lexer.Lexing(source);

        incremental = lexer.Lexing(incremental, offset, inserted);

        var updated = source.Insert(offset, inserted);
        AssertEquivalent(lexer.Lexing(updated), incremental);
        AssertReconstructs(updated, incremental);
    }

    [Theory]
    [InlineData("a\n b", 1, 1)]
    [InlineData(" \t a b", 0, 3)]
    [InlineData("a b\r\n", 3, 2)]
    [InlineData("a /*x\ny*/ b", 5, 1)]
    [InlineData("a\n b\n c", 2, 2)]
    public void Incremental_deletion_matches_fresh_lexing(string source, int offset, int length)
    {
        var lexer = CreateLexer();
        var incremental = lexer.Lexing(source);

        incremental = lexer.Lexing(incremental, offset, length);

        var updated = source.Remove(offset, length);
        AssertEquivalent(lexer.Lexing(updated), incremental);
        AssertReconstructs(updated, incremental);
    }

    [Theory]
    [InlineData("a b", " ", "\r\n")]
    [InlineData("a\n b", "\n", " ")]
    [InlineData("a /*old*/ b", "/*old*/", "/*new\ncomment*/")]
    [InlineData("a b \t", "\t", "/* end\n */")]
    public void Incremental_replacement_matches_fresh_lexing(string source, string removed, string inserted)
    {
        var lexer = CreateLexer();
        var incremental = lexer.Lexing(source);
        var offset = source.IndexOf(removed, StringComparison.Ordinal);
        var selection = Select(incremental, offset, removed.Length);

        incremental = lexer.Lexing(incremental, selection, inserted);

        var updated = source.Remove(offset, removed.Length).Insert(offset, inserted);
        AssertEquivalent(lexer.Lexing(updated), incremental);
        AssertReconstructs(updated, incremental);
    }

    private static Lexer CreateLexer(bool includeComments = true)
    {
        var lexer = new Lexer();
        foreach (var delimiter in new[] { " ", "\t", "\r", "\n", "\u2028", "\u2029", "\v", "\f", "\u0085" })
            lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Delimiter, delimiter, false));
        lexer.AddTokenRule(new Terminal(TokenType.Identifier, "[a-z]+", "id", true, true));
        foreach (var punctuation in new[] { ";", "(", ")", "+", "/" })
            lexer.AddTokenRule(new Terminal(TokenType.Operator.NormalOperator, punctuation, false));
        if (includeComments)
            lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment,
                "/\\*[\\s\\S]*?\\*/|//[^\\r\\n\\u2028\\u2029]*", "comment", false, true));
        return lexer;
    }

    private static void AssertGap(TokenCell token, string text, int startIndex, bool hasLineBreak)
    {
        AssertTrivia(token.LeadingTrivia, text, startIndex, hasLineBreak);
        Assert.Equal(hasLineBreak, token.HasLineBreakBefore);
        Assert.Equal(token.StartIndex, token.LeadingTrivia.EndIndexExclusive);
    }

    private static void AssertTrivia(TokenTrivia trivia, string text, int startIndex, bool hasLineBreak)
    {
        Assert.NotNull(trivia);
        Assert.Equal(text, trivia.Text);
        Assert.Equal(startIndex, trivia.StartIndex);
        Assert.Equal(text.Length, trivia.Length);
        Assert.Equal(startIndex + text.Length, trivia.EndIndexExclusive);
        Assert.Equal(hasLineBreak, trivia.HasLineBreak);
    }

    private static void AssertReconstructs(string source, LexingData lexed)
    {
        Assert.Equal(source, string.Concat(lexed.TokensForView.Select(token => token.Data)));
        Assert.Equal(source, string.Concat(lexed.TokensForParsing.Select(token => token.LeadingTrivia.Text + token.Data))
            + lexed.TrailingTrivia.Text);
        foreach (var token in lexed.TokensForParsing)
        {
            Assert.Equal(token.Data, source.Substring(token.StartIndex, token.Data.Length));
            Assert.Equal(token.LeadingTrivia.Text,
                source.Substring(token.LeadingTrivia.StartIndex, token.LeadingTrivia.Length));
            Assert.Equal(token.StartIndex, token.LeadingTrivia.EndIndexExclusive);
        }
        Assert.Equal(lexed.TrailingTrivia.Text,
            source.Substring(lexed.TrailingTrivia.StartIndex, lexed.TrailingTrivia.Length));
        Assert.Equal(source.Length, lexed.TrailingTrivia.EndIndexExclusive);
    }

    private static void AssertEquivalent(LexingData expected, LexingData actual)
    {
        Assert.Equal(expected.TokensForView.Select(token => token.Data), actual.TokensForView.Select(token => token.Data));
        Assert.Equal(expected.TokensForParsing.Count, actual.TokensForParsing.Count);
        for (var i = 0; i < expected.TokensForParsing.Count; i++)
        {
            var left = expected.TokensForParsing[i];
            var right = actual.TokensForParsing[i];
            Assert.Equal(left.Data, right.Data);
            Assert.Equal(left.StartIndex, right.StartIndex);
            Assert.Equal(left.EndIndex, right.EndIndex);
            Assert.Equal(left.PatternInfo.Terminal.TokenType, right.PatternInfo.Terminal.TokenType);
            AssertTrivia(right.LeadingTrivia, left.LeadingTrivia.Text, left.LeadingTrivia.StartIndex, left.HasLineBreakBefore);
        }
        AssertTrivia(actual.TrailingTrivia, expected.TrailingTrivia.Text,
            expected.TrailingTrivia.StartIndex, expected.TrailingTrivia.HasLineBreak);
    }

    private static SelectionTokensContainer Select(LexingData storage, int startIndex, int length)
    {
        var selection = new SelectionTokensContainer();
        var endIndexExclusive = startIndex + length;
        for (var i = 0; i < storage.TokensForView.Count; i++)
        {
            var token = storage.TokensForView[i];
            var start = Math.Max(startIndex, token.StartIndex);
            var end = Math.Min(endIndexExclusive, token.EndIndex + 1);
            if (start >= end) continue;
            if (start == token.StartIndex && end == token.EndIndex + 1)
                selection.WholeSelectionBag.Add(i);
            else
                selection.PartSelectionBag.Add(Tuple.Create(i, start - token.StartIndex, end - start));
        }
        selection.SortAll();
        return selection;
    }

    private sealed class SingleEnumeration<T> : IEnumerable<T>
    {
        private readonly IEnumerable<T> values;
        public int EnumerationCount { get; private set; }

        public SingleEnumeration(IEnumerable<T> values) => this.values = values;

        public IEnumerator<T> GetEnumerator()
        {
            if (++EnumerationCount != 1)
                throw new InvalidOperationException("This sequence can only be enumerated once.");
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
