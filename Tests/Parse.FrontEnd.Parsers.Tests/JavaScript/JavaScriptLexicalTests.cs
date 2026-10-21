using System;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

[Collection(JavaScriptExpressionCollection.Name)]
[Trait("Category", "JavaScript")]
public sealed class JavaScriptLexicalTests
{
    private readonly JavaScriptExpressionFixture _fixture;
    public JavaScriptLexicalTests(JavaScriptExpressionFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("name")]
    [InlineData("$name")]
    [InlineData("name$")]
    [InlineData("_")]
    [InlineData("true$")]
    [InlineData("nullValue")]
    [InlineData("falsehood")]
    [InlineData("instanceofX")]
    [InlineData("typeofx")]
    [InlineData("한글")]
    [InlineData("日本語")]
    [InlineData("π")]
    [InlineData("\U00010400name")]
    [InlineData("a\u0301")]
    [InlineData("a\u200Cb")]
    [InlineData("a\u200Db")]
    [InlineData("\\u0061")]
    [InlineData("\\u{61}")]
    [InlineData("\\u{10400}name")]
    [InlineData("\\u{000000000000000000000000000061}")]
    [InlineData("a\\u200Cb")]
    public void Identifiers_include_dollar_unicode_and_valid_unicode_escapes_as_one_token(string input)
    {
        var tree = _fixture.Success(input);
        var token = Assert.Single(tree.AllTokens);
        Assert.Equal(input, token.Input);
        Assert.Equal(0, token.StartIndex);
        Assert.Equal(input.Length - 1, token.EndIndex);
    }

    [Theory]
    [InlineData("yield")]
    [InlineData("let")]
    [InlineData("implements")]
    [InlineData("interface")]
    [InlineData("package")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("public")]
    [InlineData("static")]
    [InlineData("if")]
    [InlineData("class")]
    [InlineData("\\u0069f")]
    [InlineData("\\u0074rue")]
    [InlineData("\\u0073tatic")]
    [InlineData("1name")]
    [InlineData("\u0301a")]
    [InlineData("\u200Ca")]
    [InlineData("\\u0030name")]
    [InlineData("a\\u002Db")]
    [InlineData("\\u{110000}")]
    [InlineData("\\uD800")]
    [InlineData("😀")]
    public void Invalid_identifier_characters_and_reserved_references_are_rejected(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("0")]
    [InlineData("123")]
    [InlineData("1_000")]
    [InlineData(".5")]
    [InlineData("1.")]
    [InlineData("1.e2")]
    [InlineData("1.2_3e+4_5")]
    [InlineData("0.01")]
    [InlineData("1e-3")]
    [InlineData("0xA_B")]
    [InlineData("0Xff")]
    [InlineData("0b1_0")]
    [InlineData("0B101")]
    [InlineData("0o7_1")]
    [InlineData("0O77")]
    [InlineData("0n")]
    [InlineData("123_456n")]
    [InlineData("0xFFn")]
    [InlineData("0b10n")]
    [InlineData("0o71n")]
    public void Numeric_and_bigint_literals_preserve_the_complete_spelling(string input)
    {
        var token = Assert.Single(_fixture.Success(input).AllTokens);
        Assert.Equal(input, token.Input);
    }

    [Theory]
    [InlineData("1__0")]
    [InlineData("1_")]
    [InlineData("1_.0")]
    [InlineData("1._0")]
    [InlineData("1e_2")]
    [InlineData("1e+")]
    [InlineData("0x_1")]
    [InlineData("0x")]
    [InlineData("0b2")]
    [InlineData("0o8")]
    [InlineData("00")]
    [InlineData("0123")]
    [InlineData("08")]
    [InlineData("00n")]
    [InlineData("1.0n")]
    [InlineData("1e2n")]
    [InlineData("1_n")]
    [InlineData("1n0")]
    [InlineData("123abc")]
    [InlineData("0x1g")]
    [InlineData("1\\u0061")]
    [InlineData("١")]
    public void Malformed_numeric_forms_cannot_be_split_into_smaller_valid_tokens(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("''")]
    [InlineData("\"\"")]
    [InlineData("'hello'")]
    [InlineData("\"한글\"")]
    [InlineData("'\\x41'")]
    [InlineData("'\\u0041'")]
    [InlineData("'\\u{1F600}'")]
    [InlineData("'\\uD800'")]
    [InlineData("'\\u{D800}'")]
    [InlineData("'\\q'")]
    [InlineData("'\\0'")]
    [InlineData("'\\n\\r\\t\\b\\f\\v'")]
    [InlineData("'a\\\nb'")]
    [InlineData("'a\\\r\nb'")]
    [InlineData("'a\\\rb'")]
    [InlineData("'a\u2028b'")]
    [InlineData("'a\u2029b'")]
    [InlineData("'\\u{000000000000000000000000000041}'")]
    public void String_literals_accept_modern_escape_and_line_continuation_forms(string input)
    {
        var token = Assert.Single(_fixture.Success(input).AllTokens);
        Assert.Equal(input, token.Input);
    }

    [Theory]
    [InlineData("'unterminated")]
    [InlineData("\"unterminated")]
    [InlineData("'a\nb'")]
    [InlineData("'a\rb'")]
    [InlineData("'\\xG0'")]
    [InlineData("'\\u00G0'")]
    [InlineData("'\\u{}'")]
    [InlineData("'\\u{110000}'")]
    [InlineData("'\\1'")]
    [InlineData("'\\8'")]
    [InlineData("'\\09'")]
    public void Unterminated_invalid_escape_and_legacy_octal_strings_are_rejected(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("a/b/c", "a|/|b|/|c")]
    [InlineData("f(x)/2", "f|(|x|)|/|2")]
    [InlineData("(a+b)/2", "(|a|+|b|)|/|2")]
    [InlineData("/a/.test(x)", "/a/|.|test|(|x|)")]
    [InlineData("a=/a+/g", "a|=|/a+/g")]
    [InlineData("a/ /b/.source", "a|/|/b/|.|source")]
    [InlineData("a/=/x/.test(s)", "a|/=|/x/|.|test|(|s|)")]
    [InlineData("a\n/b/g", "a|/|b|/|g")]
    [InlineData("x++/y", "x|++|/|y")]
    [InlineData("/x//y", "/x/|/|y")]
    [InlineData("typeof /x/", "typeof|/x/")]
    [InlineData("a ? /x/ : /y/", "a|?|/x/|:|/y/")]
    [InlineData("f(/x/, a/b)", "f|(|/x/|,|a|/|b|)")]
    [InlineData("a// comment", "a")]
    [InlineData("/* header */ a /* trailing */", "a")]
    [InlineData("a// comment\n+b", "a|+|b")]
    [InlineData("a// comment\n/b/g", "a|/|b|/|g")]
    public void Parser_context_selects_division_or_regexp_without_rewriting_the_source(
        string input, string expected)
    {
        Assert.Equal(expected.Split('|'), _fixture.Success(input).AllTokens.Select(t => t.Input));
    }

    [Theory]
    [InlineData("/a/")]
    [InlineData("/a+/g")]
    [InlineData("/a/dgimsuy")]
    [InlineData("/a/v")]
    [InlineData("/[a/b]+/u")]
    [InlineData("/\\//")]
    [InlineData("/[/]/")]
    [InlineData("/a\\/b/i")]
    public void Regexp_literals_keep_escaped_slashes_character_classes_and_flags_in_one_token(string input)
        => Assert.Equal(input, Assert.Single(_fixture.Success(input).AllTokens).Input);

    [Theory]
    [InlineData("/a/gg")]
    [InlineData("/a/z")]
    [InlineData("/a/uv")]
    [InlineData("/a/\\u0067")]
    [InlineData("/a\nb/")]
    [InlineData("/a\u2028b/")]
    [InlineData("/unterminated")]
    [InlineData("/[unterminated/")]
    public void Regexp_lexical_envelopes_and_flags_are_validated(string input) => _fixture.Invalid(input);

    [Theory]
    [InlineData("a+++b", "a|++|+|b")]
    [InlineData("a>>>1", "a|>>>|1")]
    [InlineData("a>>>=1", "a|>>>=|1")]
    [InlineData("x**=2", "x|**=|2")]
    [InlineData("a!==b", "a|!==|b")]
    [InlineData("a?.3:0", "a|?|.3|:|0")]
    [InlineData("0..toString()", "0.|.|toString|(|)")]
    public void Maximal_munch_preserves_punctuators_and_decimal_lookahead_exceptions(string input, string expected)
        => Assert.Equal(expected.Split('|'), _fixture.Success(input).AllTokens.Select(t => t.Input));

    [Theory]
    [InlineData("value/* gap */++")]
    [InlineData("value \t ++")]
    [InlineData("value\u00A0++")]
    public void Postfix_update_allows_trivia_without_line_terminators(string input) => _fixture.Success(input);

    [Theory]
    [InlineData("value\n++")]
    [InlineData("value\r++")]
    [InlineData("value\r\n++")]
    [InlineData("value\u2028++")]
    [InlineData("value\u2029++")]
    [InlineData("value/*\n*/++")]
    [InlineData("value// comment\n++")]
    public void Postfix_update_rejects_a_line_terminator_even_inside_a_comment(string input)
        => _fixture.Invalid(input);

    [Fact]
    public void Comments_and_unicode_whitespace_preserve_original_offsets_and_line_break_trivia()
    {
        const string input = "\uFEFF\u00A0a /* comment */ +\r\n b";
        var tokens = _fixture.Success(input).AllTokens.ToArray();
        Assert.Equal(new[] { "a", "+", "b" }, tokens.Select(t => t.Input));
        Assert.Equal(input.IndexOf('a'), tokens[0].StartIndex);
        Assert.Equal(input.IndexOf('+'), tokens[1].StartIndex);
        Assert.Equal(input.LastIndexOf('b'), tokens[2].StartIndex);
        Assert.False(tokens[1].HasLineBreakBefore);
        Assert.True(tokens[2].HasLineBreakBefore);
    }

    [Theory]
    [InlineData("value /* unterminated")]
    [InlineData("/* unterminated")]
    [InlineData("value @ other")]
    public void Invalid_source_never_disappears_as_ignored_trivia(string input) => _fixture.Invalid(input);

    [Theory]
    [InlineData(" \u00A0\\u0030name", "\\u0030", false)]
    [InlineData(" \uFEFFa\\u002Db", "\\u002D", false)]
    [InlineData(" \u00A0/a/gg", "g", true)]
    [InlineData(" \uFEFF/a/uv", "uv", false)]
    [InlineData(" \u00A0/* unterminated", "/* unterminated", false)]
    public void Direct_token_reads_report_exact_source_spans_for_malformed_tokens(
        string input, string offendingText, bool lastOccurrence)
    {
        var source = _fixture.Language.CreateTokenSource(input);
        var error = Assert.Throws<JavaScriptSyntaxException>(() =>
            source.Peek(source.Start, source.Policy.Goals[0]));
        int expectedOffset = lastOccurrence
            ? input.LastIndexOf(offendingText, StringComparison.Ordinal)
            : input.IndexOf(offendingText, StringComparison.Ordinal);
        Assert.Equal(expectedOffset, error.SourceOffset);
        Assert.Equal(offendingText.Length, error.Length);
        Assert.Equal(offendingText, input.Substring(error.SourceOffset, error.Length));
        Assert.False(string.IsNullOrWhiteSpace(error.Message));
        Assert.Equal(0, source.Start.SourceOffset);
        Assert.Equal(0, source.Start.TokenIndex);
    }

    [Fact]
    public void Lexical_error_positions_count_utf16_units_after_an_astral_character()
    {
        const string input = "'\U0001F600' + \\u0030bad";
        var source = _fixture.Language.CreateTokenSource(input);
        var goal = source.Policy.Goals[0];
        var literal = source.Peek(source.Start, goal);
        Assert.Equal("'\U0001F600'", literal.Token.Input);
        Assert.Equal(4, literal.Next.SourceOffset); // Quotes plus a two-unit surrogate pair.
        var plus = source.Peek(literal.Next, goal);
        Assert.Equal("+", plus.Token.Input);

        var error = Assert.Throws<JavaScriptSyntaxException>(() => source.Peek(plus.Next, goal));
        Assert.Equal(input.IndexOf("\\u0030", StringComparison.Ordinal), error.SourceOffset);
        Assert.Equal(6, error.Length);
        Assert.Equal("\\u0030", input.Substring(error.SourceOffset, error.Length));
    }
}
