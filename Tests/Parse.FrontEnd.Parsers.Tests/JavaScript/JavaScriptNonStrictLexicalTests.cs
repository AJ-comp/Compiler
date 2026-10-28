using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMAScript 2026 12.9.3.1 / 12.9.4.1: leading-zero numbers and legacy string
// escapes are accepted in non-strict code, then rejected wherever IsStrict is true.
// https://tc39.es/ecma262/2026/multipage/ecmascript-language-lexical-grammar.html
// The source spellings and nested-mode expectations were independently compiled
// (never evaluated) with Node.js 24.12.0; the fixture exercises Janglim's public API.
[CollectionDefinition(Name)]
public sealed class JavaScriptNonStrictLexicalCollection : ICollectionFixture<JavaScriptNonStrictLexicalFixture>
{
    public const string Name = "ECMAScript non-strict lexical grammar";
}

public sealed class JavaScriptNonStrictLexicalFixture
{
    public JavaScriptGrammar Expression { get; }
    public LALRParser ExpressionParser { get; }
    public JavaScriptGrammar Script { get; }
    public LALRParser ScriptParser { get; }
    public JavaScriptGrammar StrictExpression { get; }
    public JavaScriptGrammar WebScript { get; }
    public LALRParser WebScriptParser { get; }
    public JavaScriptGrammar StrictWebScript { get; }
    public JavaScriptGrammar WebModule { get; }

    public JavaScriptNonStrictLexicalFixture()
    {
        Expression = new JavaScriptGrammar(JavaScriptMode.NonStrict, inputKind: JavaScriptInputKind.Expression);
        ExpressionParser = new LALRParser(Expression.Grammar, false);
        Script = new JavaScriptGrammar(JavaScriptMode.NonStrict);
        ScriptParser = new LALRParser(Script.Grammar, false);
        StrictExpression = new JavaScriptGrammar(inputKind: JavaScriptInputKind.Expression);
        WebScript = new JavaScriptGrammar(JavaScriptMode.NonStrict, true);
        WebScriptParser = new LALRParser(WebScript.Grammar, false);
        StrictWebScript = new JavaScriptGrammar(JavaScriptMode.Strict, true);
        WebModule = new JavaScriptGrammar(webCompatibility: true, inputKind: JavaScriptInputKind.Module);
    }

    public ParseTreeSymbol ExpressionSuccess(string input)
    {
        var parsed = ExpressionParser.Parsing(Expression.CreateTokenSource(input));
        Assert.True(parsed.Success, input + Environment.NewLine + ExpressionParser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var diagnostics = Expression.Validate(parsed.ToParseTree);
        Assert.True(diagnostics.Count == 0, input + Environment.NewLine +
            string.Join(Environment.NewLine, diagnostics.Select(item => item.Code + ": " + item.Message)));
        return parsed.ToParseTree;
    }

    public void ExpressionInvalid(string input)
    {
        var parsed = ExpressionParser.Parsing(Expression.CreateTokenSource(input));
        if (parsed.Success)
            Assert.NotEmpty(Expression.Validate(parsed.ToParseTree));
        else
            Assert.NotEmpty(parsed.AllErrors);
    }

    public ParsingResult ParseScript(string input) => ScriptParser.Parsing(Script.CreateTokenSource(input));

    public IReadOnlyList<JavaScriptDiagnostic> ScriptDiagnostics(string input)
    {
        var parsed = ParseScript(input);
        Assert.True(parsed.Success, input + Environment.NewLine + ScriptParser.FiredException);
        Assert.Empty(parsed.AllErrors);
        return Script.Validate(parsed.ToParseTree);
    }

    public ParseTreeSymbol WebScriptSuccess(string input)
    {
        var parsed = WebScriptParser.Parsing(WebScript.CreateTokenSource(input));
        Assert.True(parsed.Success, input + Environment.NewLine + WebScriptParser.FiredException);
        Assert.Empty(parsed.AllErrors);
        Assert.Empty(WebScript.Validate(parsed.ToParseTree));
        return parsed.ToParseTree;
    }
}

[Collection(JavaScriptNonStrictLexicalCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptNonStrictLexical")]
public sealed class JavaScriptNonStrictLexicalTests
{
    private readonly JavaScriptNonStrictLexicalFixture _fixture;
    public JavaScriptNonStrictLexicalTests(JavaScriptNonStrictLexicalFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("00")]
    [InlineData("000")]
    [InlineData("077")]
    [InlineData("0123")]
    [InlineData("08")]
    [InlineData("09")]
    [InlineData("078")]
    [InlineData("008")]
    [InlineData("009")]
    [InlineData("0009")]
    [InlineData("08.5")]
    [InlineData("09.")]
    [InlineData("08e2")]
    [InlineData("078.5e+2")]
    [InlineData("08.1_2")]
    [InlineData("08e1_2")]
    [InlineData("008.01")]
    public void Non_strict_numbers_keep_legacy_octal_and_non_octal_decimal_spellings(string input)
    {
        var token = Assert.Single(_fixture.ExpressionSuccess(input).AllTokens);
        Assert.Equal(input, token.Input);
        Assert.Equal("NumericLiteral", token.Kind.Caption);
        Assert.Equal(0, token.StartIndex);
        Assert.Equal(input.Length - 1, token.EndIndex);
    }

    [Theory]
    [InlineData("0_1")]
    [InlineData("00_1")]
    [InlineData("08_1")]
    [InlineData("077_1")]
    [InlineData("00n")]
    [InlineData("08n")]
    [InlineData("077n")]
    [InlineData("00.1")]
    [InlineData("0123.4")]
    [InlineData("00e1")]
    [InlineData("077e2")]
    [InlineData("08e")]
    [InlineData("08e+")]
    [InlineData("08e_1")]
    [InlineData("08._1")]
    [InlineData("08name")]
    [InlineData("08\\u0061")]
    [InlineData("077$")]
    [InlineData("0x_1")]
    [InlineData("0b2")]
    [InlineData("0o8")]
    [InlineData("1__0")]
    public void Non_strict_mode_does_not_allow_malformed_numbers_or_legacy_bigints(string input)
        => _fixture.ExpressionInvalid(input);

    [Theory]
    [InlineData("'\\1'")]
    [InlineData("'\\7'")]
    [InlineData("'\\00'")]
    [InlineData("'\\000'")]
    [InlineData("'\\123'")]
    [InlineData("'\\377'")]
    [InlineData("'\\400'")]
    [InlineData("'\\777'")]
    [InlineData("'\\08'")]
    [InlineData("'\\09'")]
    [InlineData("'\\8'")]
    [InlineData("'\\9'")]
    [InlineData("'\\18'")]
    [InlineData("'\\078'")]
    [InlineData("'\\0009'")]
    [InlineData("\"\\1\\8\\9\"")]
    public void Non_strict_strings_preserve_legacy_octal_and_decimal_escapes(string input)
    {
        var token = Assert.Single(_fixture.ExpressionSuccess(input).AllTokens);
        Assert.Equal(input, token.Input);
        Assert.Equal("StringLiteral", token.Kind.Caption);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.1")]
    [InlineData("0e1")]
    [InlineData("1_000")]
    [InlineData("0n")]
    [InlineData("0x1n")]
    [InlineData("0o7_1n")]
    [InlineData("0b10n")]
    [InlineData("'\\0'")]
    [InlineData("'\\q'")]
    [InlineData("'\\x41'")]
    [InlineData("'\\u0041'")]
    [InlineData("'\\u{1F600}'")]
    [InlineData("'\\\\1'")]
    [InlineData("'a\\\nb'")]
    public void Modern_literals_remain_valid_when_the_non_strict_profile_is_selected(string input)
        => _fixture.ExpressionSuccess(input);

    [Theory]
    [InlineData("'unterminated")]
    [InlineData("'a\nb'")]
    [InlineData("'\\xG0'")]
    [InlineData("'\\u00G0'")]
    [InlineData("'\\u{}'")]
    [InlineData("'\\u{110000}'")]
    [InlineData("\\u0030name")]
    [InlineData("name\\u002D")]
    [InlineData("/a/gg")]
    [InlineData("/a/uv")]
    [InlineData("/a/z")]
    [InlineData("`\\1`")]
    [InlineData("`\\8`")]
    public void Unicode_regexp_and_template_lexical_errors_remain_errors_in_non_strict_code(string input)
        => _fixture.ExpressionInvalid(input);

    [Theory]
    [InlineData("00")]
    [InlineData("077")]
    [InlineData("08")]
    [InlineData("08.5")]
    [InlineData("'\\1'")]
    [InlineData("'\\8'")]
    [InlineData("'\\09'")]
    public void Default_expression_profile_keeps_strict_tokenization(string input)
    {
        var source = _fixture.StrictExpression.CreateTokenSource(input);
        Assert.Throws<JavaScriptSyntaxException>(() => source.Peek(source.Start, source.Policy.Goals[0]));
    }

    [Theory]
    [InlineData("function f(){return 077;}")]
    [InlineData("function f(){'use strict';return 1;} 077;")]
    [InlineData("{ 'use strict'; 077; }")]
    [InlineData("function f(){ ('use strict'); return 077; }")]
    [InlineData("'use\\x20strict'; 077;")]
    [InlineData("function f(){return '\\1';} function g(){'use strict';return 1;}")]
    [InlineData("'\\\\1'; 'use strict';")]
    [InlineData("tag`\\1`")]
    public void Strictness_stays_with_the_enclosing_code_and_does_not_leak_to_sibling_scopes(string input)
        => Assert.Empty(_fixture.ScriptDiagnostics(input));

    [Theory]
    [InlineData("'use strict'; 077;", "077")]
    [InlineData("function f(){'use strict'; return 08;}", "08")]
    [InlineData("function f(){'use strict';function g(){return 077;}}", "077")]
    [InlineData("class A{field=077;}", "077")]
    [InlineData("class A extends (077, B) {}", "077")]
    [InlineData("class A{method(){return 08.5;}}", "08.5")]
    [InlineData("/* 😀 */ 'use strict'; 0123;", "0123")]
    public void Strict_directives_and_classes_report_the_entire_legacy_numeric_token(string input, string literal)
    {
        var diagnostic = Assert.Single(_fixture.ScriptDiagnostics(input),
            item => item.Code == "strict_legacy_numeric_literal");
        Assert.Equal(input.IndexOf(literal, StringComparison.Ordinal), diagnostic.SourceOffset);
        Assert.Equal(literal.Length, diagnostic.Length);
        Assert.Equal(literal, input.Substring(diagnostic.SourceOffset, diagnostic.Length));
    }

    [Theory]
    [InlineData("'use strict'; '\\1';", "\\1")]
    [InlineData("'use strict'; '\\8';", "\\8")]
    [InlineData("'use strict'; '\\9';", "\\9")]
    [InlineData("'use strict'; '\\08';", "\\0")]
    [InlineData("'use strict'; '\\09';", "\\0")]
    [InlineData("'use strict'; '\\077';", "\\077")]
    [InlineData("'use strict'; '\\400';", "\\40")]
    [InlineData("'use strict'; '\\777';", "\\77")]
    [InlineData("'use strict'; '\\0009';", "\\000")]
    [InlineData("'use strict'; '\\18';", "\\1")]
    [InlineData("'\\1'; 'use strict';", "\\1")]
    [InlineData("function f(){'\\7';'use strict';}", "\\7")]
    [InlineData("class A{method(){return '\\1';}}", "\\1")]
    [InlineData("/* 😀 */ 'use strict'; '\\400';", "\\40")]
    public void Strict_string_diagnostics_point_to_the_actual_escape_in_utf16_source(string input, string escape)
    {
        var diagnostic = Assert.Single(_fixture.ScriptDiagnostics(input),
            item => item.Code == "strict_legacy_string_escape");
        Assert.Equal(input.IndexOf(escape, StringComparison.Ordinal), diagnostic.SourceOffset);
        Assert.Equal(escape.Length, diagnostic.Length);
        Assert.Equal(escape, input.Substring(diagnostic.SourceOffset, diagnostic.Length));
    }

    [Fact]
    public void Non_strict_token_reads_still_reject_out_of_range_unicode_at_its_exact_escape()
    {
        const string input = " /* 😀 */ '\\u{110000}'";
        var source = _fixture.Expression.CreateTokenSource(input);
        var error = Assert.Throws<JavaScriptSyntaxException>(() => source.Peek(source.Start, source.Policy.Goals[0]));
        Assert.Equal(input.IndexOf("\\u{110000}", StringComparison.Ordinal), error.SourceOffset);
        Assert.InRange(error.Length, 1, "\\u{110000}".Length);
    }

    [Theory]
    [InlineData("<!-- hidden\n1;")]
    [InlineData("1;<!-- hidden\n2;")]
    [InlineData("--> hidden\n1;")]
    [InlineData(" \t--> hidden\n1;")]
    [InlineData("/* first */ --> hidden\n1;")]
    [InlineData("1;/*\n*/ --> hidden\n2;")]
    [InlineData("1;\n/* first */ --> hidden\n2;")]
    [InlineData("1;\n/* first\n*/ /* second */ /* third */ --> hidden\n2;")]
    [InlineData("'use strict';\n<!-- hidden\n1;")]
    [InlineData("'use strict';\n--> hidden\n1;")]
    public void Web_compatibility_accepts_HTML_comments_in_Script_including_strict_code(string input)
        => _fixture.WebScriptSuccess(input);

    [Theory]
    [InlineData("\n")]
    [InlineData("\r")]
    [InlineData("\r\n")]
    [InlineData("\u2028")]
    [InlineData("\u2029")]
    public void HTML_comments_preserve_all_ECMAScript_line_terminators_and_exact_trivia(string newline)
    {
        string prefix = "<!-- hidden" + newline + " \t/* prefix */ --> ignored" + newline;
        string input = prefix + "1;";
        var token = Assert.Single(_fixture.WebScriptSuccess(input).AllTokens, item => item.Input == "1");
        Assert.Equal(prefix.Length, token.StartIndex);
        Assert.Equal(prefix, token.LeadingTrivia.Text);
        Assert.True(token.HasLineBreakBefore);
    }

    [Fact]
    public void A_multiline_block_comment_can_absorb_HTML_close_text_after_earlier_code()
    {
        const string input = "1; /* before\nnewline */ /* adjacent */ --> ignored\n2;";
        var tokens = _fixture.WebScriptSuccess(input).AllTokens.ToArray();
        Assert.Equal(new[] { "1", ";", "2", ";" }, tokens.Select(item => item.Input));
        Assert.Equal(" /* before\nnewline */ /* adjacent */ --> ignored\n", tokens[2].LeadingTrivia.Text);
        Assert.Equal(input.IndexOf("2;", StringComparison.Ordinal), tokens[2].StartIndex);
        Assert.True(tokens[2].HasLineBreakBefore);
    }

    [Fact]
    public void HTML_close_text_after_code_on_the_same_line_remains_decrement_and_greater_than()
    {
        const string input = "let a = 1, b = 2; a /* same line */ --> b;";
        var tokens = _fixture.WebScriptSuccess(input).AllTokens.Select(item => item.Input).ToArray();
        Assert.Equal(new[] { "let", "a", "=", "1", ",", "b", "=", "2", ";", "a", "--", ">", "b", ";" }, tokens);
    }

    [Theory]
    [InlineData("<!-- hidden\n1", "<")]
    [InlineData("--> hidden\n1", "--")]
    public void HTML_comments_require_web_compatibility_and_are_never_enabled_for_Module(string input, string firstToken)
    {
        var ordinary = _fixture.Script.CreateTokenSource(input);
        Assert.Equal(firstToken, ordinary.Peek(ordinary.Start, ordinary.Policy.Goals[0]).Token.Input);
        var module = _fixture.WebModule.CreateTokenSource(input);
        Assert.Equal(firstToken, module.Peek(module.Start, module.Policy.Goals[0]).Token.Input);
    }

    [Fact]
    public void Strict_Script_constructor_can_enable_HTML_comments_without_enabling_legacy_literals()
    {
        var source = _fixture.StrictWebScript.CreateTokenSource("<!-- hidden\n1;");
        var first = source.Peek(source.Start, source.Policy.Goals[0]);
        Assert.Equal("1", first.Token.Input);
        Assert.Equal("<!-- hidden\n", first.Token.LeadingTrivia.Text);
        var legacy = _fixture.StrictWebScript.CreateTokenSource("<!-- hidden\n077;");
        Assert.Throws<JavaScriptSyntaxException>(() => legacy.Peek(legacy.Start, legacy.Policy.Goals[0]));
    }

    [Theory]
    [InlineData("<!-- only comment")]
    [InlineData("--> only comment")]
    [InlineData("/* first\nsecond */ --> only comment")]
    public void HTML_comment_at_end_of_input_is_preserved_as_EOF_trivia(string input)
    {
        var source = _fixture.WebScript.CreateTokenSource(input);
        var eof = source.Peek(source.Start, source.Policy.Goals[0]);
        Assert.IsType<EndMarker>(eof.Token.Kind);
        Assert.Equal(input, eof.Token.LeadingTrivia.Text);
        Assert.Equal(input.Length, eof.Next.SourceOffset);
        Assert.Equal(0, eof.Next.TokenIndex);
    }

    [Fact]
    public void Unterminated_block_comment_is_not_reinterpreted_as_an_HTML_comment()
    {
        var source = _fixture.WebScript.CreateTokenSource("/* unterminated\n--> ignored");
        Assert.Throws<JavaScriptSyntaxException>(() => source.Peek(source.Start, source.Policy.Goals[0]));
    }
}
