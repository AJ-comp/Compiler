using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMAScript 2026, 12.9.6 and 13.2.8: template fragments retain their source spelling;
// invalid escape sequences are early errors only for an untagged TemplateLiteral.
[Collection(JavaScriptExpressionCollection.Name)]
[Trait("Category", "JavaScript")]
public sealed class JavaScriptTemplateTests
{
    private readonly JavaScriptExpressionFixture _fixture;
    public JavaScriptTemplateTests(JavaScriptExpressionFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("``")]
    [InlineData("`hello world`")]
    [InlineData("`한글 日本語 😀`")]
    [InlineData("`'double\" quotes'`")]
    [InlineData("`// text /* text */`")]
    [InlineData("`$`")]
    [InlineData("`$$`")]
    [InlineData("`$ {value}`")]
    [InlineData("`a}b{c`")]
    [InlineData("`\u0000\u0001\u0085\ufeff`")]
    [InlineData("`line\nnext`")]
    [InlineData("`line\rnext`")]
    [InlineData("`line\r\nnext`")]
    [InlineData("`line\u2028next`")]
    [InlineData("`line\u2029next`")]
    [InlineData("`\\`quoted\\``")]
    [InlineData("`\\${notAnExpression}`")]
    [InlineData("`\\$\\{also text}`")]
    [InlineData("`\\\\`")]
    [InlineData("`\\0`")]
    [InlineData("`\\0x`")]
    [InlineData("`\\a\\z\\$\\{\\}\\/`")]
    [InlineData("`\\b\\f\\n\\r\\t\\v`")]
    [InlineData("`\\x00\\x7f\\xFF`")]
    [InlineData("`\\u0000\\uD800\\uDC00`")]
    [InlineData("`\\u{0}\\u{10FFFF}\\u{D800}`")]
    [InlineData("`\\u{000000000000000000000000000061}`")]
    [InlineData("`before\\\nafter`")]
    [InlineData("`before\\\rafter`")]
    [InlineData("`before\\\r\nafter`")]
    [InlineData("`before\\\u2028after`")]
    [InlineData("`before\\\u2029after`")]
    public void No_substitution_templates_accept_raw_characters_line_terminators_and_valid_escapes(string input)
    {
        var token = Assert.Single(_fixture.Success(input).AllTokens);
        Assert.Equal(input, token.Input);
        Assert.Equal("NoSubstitutionTemplate", token.Kind.Caption);
        Assert.Equal(0, token.StartIndex);
        Assert.Equal(input.Length - 1, token.EndIndex);
    }

    [Theory]
    [InlineData("`${value}`")]
    [InlineData("`before ${value} after`")]
    [InlineData("`${a}${b}${c}`")]
    [InlineData("`a${b}c${d}e`")]
    [InlineData("`${a + b * c}`")]
    [InlineData("`${a, b}`")]
    [InlineData("`${value = other}`")]
    [InlineData("`${a ? b : c}`")]
    [InlineData("`${[1, 2, ...items]}`")]
    [InlineData("`${{value: 1}}`")]
    [InlineData("`${{nested: {value: 1}}.nested.value}`")]
    [InlineData("`${({value: 1}).value}`")]
    [InlineData("`${object?.member ?? fallback}`")]
    [InlineData("`${f(...items,)}`")]
    [InlineData("`${\nvalue\r\n}`")]
    [InlineData("`${/* } ${ ` */ value /* } */}`")]
    [InlineData("`${value // } ${ `\n}`")]
    [InlineData("`${'}' + '${' + '`'}`")]
    [InlineData("`${object['}']}`")]
    [InlineData("`${{['}']: value}['}']}`")]
    [InlineData("`${/}/.test(value)}`")]
    [InlineData("`${/[{}]/.test(value)}`")]
    [InlineData("`${/\\}/.test(value)}`")]
    [InlineData("`${value / divisor}`")]
    [InlineData("`${value /= divisor}`")]
    [InlineData("`${/a/.test(value) ? value / divisor : other}`")]
    [InlineData("`${({a: value}) / divisor}`")]
    [InlineData("`${a}${/b/.test(c)}${d / e}`")]
    public void Substitutions_use_expression_grammar_without_confusing_braces_comments_strings_or_regexes(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("`${``}`")]
    [InlineData("`${`inner`}`")]
    [InlineData("`outer ${`inner ${value}`}`")]
    [InlineData("`a${`b${`c${value}d`}e`}f`")]
    [InlineData("`${{a: `nested ${value}`}.a}`")]
    [InlineData("`${`a${{b: value}.b}c`}d`")]
    [InlineData("`${tag`a${value}b`}`")]
    [InlineData("tag`a${other`b${value}c`}d`")]
    [InlineData("`${`a${value}b` / divisor}`")]
    [InlineData("`${`a${value}b`}${/}/.test(value)}`")]
    [InlineData("`${({a: `inner ${ {b: value}.b }`}).a}end`")]
    public void Nested_templates_keep_each_substitution_brace_depth_independent(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("tag``")]
    [InlineData("tag`text`")]
    [InlineData("tag`a${value}b`")]
    [InlineData("object.tag`text`")]
    [InlineData("object['tag']`text`")]
    [InlineData("f()`text`")]
    [InlineData("f(a).tag`text`")]
    [InlineData("tag`one``two`")]
    [InlineData("tag`text`(value)")]
    [InlineData("tag`text`.property")]
    [InlineData("tag`text`[index]")]
    [InlineData("tag`text`?.property")]
    [InlineData("tag`text`?.(value)")]
    [InlineData("tag\n`text`")]
    [InlineData("tag /* comment */ `text`")]
    [InlineData("(object?.tag)`text`")]
    [InlineData("(object?.tag)`text`?.property")]
    [InlineData("new Constructor`text`")]
    [InlineData("new Constructor`text`(value)")]
    [InlineData("new Constructor()`text`")]
    [InlineData("new (tag`text`)(value)")]
    [InlineData("1`text`")]
    [InlineData("null`text`")]
    [InlineData("`text``other`")]
    public void Tagged_templates_follow_member_call_and_new_binding_and_allow_runtime_only_failures(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("object?.tag`text`")]
    [InlineData("object?.[tag]`text`")]
    [InlineData("object?.()`text`")]
    [InlineData("object?.tag.property`text`")]
    [InlineData("object?.tag()`text`")]
    [InlineData("object?.`text`")]
    [InlineData("new object?.tag`text`")]
    public void A_tag_cannot_be_the_unparenthesized_result_of_an_optional_chain(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("`a` / value")]
    [InlineData("`a${value}b` / divisor")]
    [InlineData("tag`a${value}b` / divisor")]
    [InlineData("`${value / divisor}` / other")]
    [InlineData("/a/.test(`a`)")]
    [InlineData("value / `a`")]
    [InlineData("value / tag`a`")]
    [InlineData("`${/a/}`")]
    [InlineData("`${/a/ / divisor}`")]
    [InlineData("`a` + /b/.source")]
    [InlineData("`a${value}b` + /c/.source")]
    [InlineData("tag`a${/b/.test(value)}c` + /d/.source")]
    public void Slash_lexical_goals_remain_correct_before_inside_and_after_templates(string input)
        => _fixture.Success(input);

    public static IEnumerable<object[]> InvalidEscapes()
    {
        foreach (string escape in new[]
        {
            "\\1", "\\7", "\\8", "\\9", "\\01", "\\00", "\\07", "\\08", "\\09",
            "\\x", "\\xZ", "\\x1", "\\x1Z", "\\u", "\\uZ", "\\u1", "\\u12", "\\u123",
            "\\u123Z", "\\u{}", "\\u{Z}", "\\u{1", "\\u{110000}", "\\u{FFFFFFFFFFFFFFFF}"
        }) yield return new object[] { escape };
    }

    [Theory]
    [MemberData(nameof(InvalidEscapes))]
    public void Invalid_template_escapes_are_early_errors_when_the_template_is_untagged(string escape)
    {
        _fixture.Invalid("`" + escape + "`");
        _fixture.Invalid("`a" + escape + "${value}b`");
        _fixture.Invalid("`a${value}b" + escape + "`");
        _fixture.Invalid("`a${value}b" + escape + "${other}c`");
    }

    [Theory]
    [MemberData(nameof(InvalidEscapes))]
    public void Tagged_template_fragments_accept_invalid_escapes_as_raw_text(string escape)
    {
        _fixture.Success("tag`" + escape + "`");
        _fixture.Success("tag`a" + escape + "${value}b`");
        _fixture.Success("tag`a${value}b" + escape + "`");
        _fixture.Success("tag`a${value}b" + escape + "${other}c`");
    }

    [Theory]
    [InlineData("tag`outer ${`\\xZ`} text`")]
    [InlineData("tag`outer ${'\\xZ'} text`")]
    [InlineData("`outer ${tag`\\xZ`} \\uZ`")]
    [InlineData("tag`outer ${value = 1} text \\xZ` + `\\1`")]
    public void Tagged_escape_permission_does_not_leak_into_nested_untagged_templates_or_strings(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("`outer ${tag`\\xZ`} text`")]
    [InlineData("tag`\\xZ ${`valid`} \\uZ`")]
    [InlineData("tag`\\1 ${other`\\u{110000}`} \\8`")]
    public void Each_nested_template_uses_its_own_tagged_status(string input) => _fixture.Success(input);

    [Theory]
    [InlineData("`text` = value")]
    [InlineData("`text` += value")]
    [InlineData("`text`++")]
    [InlineData("++`text`")]
    [InlineData("tag`text` = value")]
    [InlineData("tag`text` += value")]
    [InlineData("tag`text`++")]
    [InlineData("++tag`text`")]
    [InlineData("`${eval = value}`")]
    [InlineData("tag`${arguments++}`")]
    [InlineData("`${{__proto__: 1, '__proto__': 2}}`")]
    public void Template_nodes_do_not_bypass_assignment_update_or_substitution_early_errors(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("tag`text`.value = other")]
    [InlineData("tag`text`[index]++")]
    [InlineData("`text`.value = other")]
    [InlineData("delete `text`")]
    [InlineData("delete tag`text`")]
    public void Member_access_after_a_template_can_be_an_assignment_target(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("f()`text`", "CallExpression", "f ( )")]
    [InlineData("tag`one``two`", "MemberExpression", "tag `one`")]
    [InlineData("new Constructor()`text`", "MemberExpression", "new Constructor ( )")]
    public void Concrete_tree_keeps_the_tag_to_the_left_of_its_template(
        string input, string nodeName, string tagText)
    {
        var nodes = JavaScriptExpressionFixture.Nodes(_fixture.Success(input));
        var tagged = Assert.Single(nodes, node => node.SignPost.Name == nodeName && node.Count == 2 &&
            node[1] is Janglim.FrontEnd.ParseTree.ParseTreeNonTerminal template &&
            template.SignPost.Name == "TaggedTemplate" &&
            JavaScriptExpressionFixture.Text(node[0]) == tagText);
        Assert.Equal(tagText, JavaScriptExpressionFixture.Text(tagged[0]));
    }

    [Fact]
    public void Constructor_arguments_apply_after_the_tagged_constructor_expression()
    {
        var tree = _fixture.Success("new Constructor`text`(value)");
        var constructor = Assert.Single(JavaScriptExpressionFixture.Nodes(tree), node =>
            node.SignPost.Name == "MemberExpression" && node.Count == 3 &&
            JavaScriptExpressionFixture.Text(node) == "new Constructor `text` ( value )");
        Assert.Equal("new", JavaScriptExpressionFixture.Text(constructor[0]));
        Assert.Equal("Constructor `text`", JavaScriptExpressionFixture.Text(constructor[1]));
        Assert.Equal("( value )", JavaScriptExpressionFixture.Text(constructor[2]));
    }

    [Fact]
    public void Invalid_untagged_fragment_diagnostic_points_to_the_escape_in_the_original_source()
    {
        const string input = "  `a${value}b\\xZ${other}c`";
        var result = _fixture.Parse(input);
        Assert.True(result.Success, _fixture.Parser.FiredException?.ToString());
        var diagnostic = Assert.Single(_fixture.Language.Validate(result.ToParseTree));
        Assert.Equal("invalid_template_escape", diagnostic.Code);
        Assert.Equal(input.IndexOf("\\x", StringComparison.Ordinal), diagnostic.SourceOffset);
        Assert.InRange(diagnostic.Length, 2, input.Length - diagnostic.SourceOffset);
        Assert.StartsWith("\\x", input.Substring(diagnostic.SourceOffset, diagnostic.Length));
    }

    [Theory]
    [InlineData("`")]
    [InlineData("`unterminated")]
    [InlineData("`trailing\\")]
    [InlineData("`trailing\\`")]
    [InlineData("`${")]
    [InlineData("`${value")]
    [InlineData("`${value}")]
    [InlineData("`${value}trailing\\")]
    [InlineData("`${}`")]
    [InlineData("`${ /* comment */ }`")]
    [InlineData("`${a + }`")]
    [InlineData("`${a; b}`")]
    [InlineData("`${a}${}`")]
    [InlineData("`${{a: 1}`")]
    [InlineData("`${{a: 1}}")]
    [InlineData("`${`nested`}")]
    [InlineData("`${`nested ${value}`")]
    [InlineData("`${value /* unterminated }`")]
    [InlineData("`${'unterminated }`")]
    [InlineData("tag`unterminated")]
    [InlineData("tag`${}`")]
    [InlineData("`${value}}` extra")]
    public void Malformed_or_unterminated_templates_and_substitutions_are_rejected(string input)
        => _fixture.Invalid(input);

    [Fact]
    public void Concrete_tokens_keep_template_fragment_boundaries_and_original_source_offsets()
    {
        const string input = "  `start ${ first + { value: second }.value } middle ${ third } end`  ";
        var tokens = _fixture.Success(input).AllTokens.ToArray();
        Assert.Equal(new[] { "`start ${", "first", "+", "{", "value", ":", "second", "}", ".", "value",
            "} middle ${", "third", "} end`" }, tokens.Select(token => token.Input));
        Assert.Equal(new[] { "TemplateHead", "TemplateMiddle", "TemplateTail" }, tokens
            .Where(token => token.Kind.Caption.StartsWith("Template", StringComparison.Ordinal))
            .Select(token => token.Kind.Caption));
        foreach (var token in tokens)
            Assert.Equal(token.Input, input.Substring(token.StartIndex, token.EndIndex - token.StartIndex + 1));
        Assert.Equal(" ", tokens.Single(token => token.Input == "} middle ${").LeadingTrivia.Text);
        Assert.Equal(" ", tokens.Single(token => token.Input == "} end`").LeadingTrivia.Text);
    }

    [Fact]
    public void Template_raw_text_is_part_of_the_token_and_never_leading_trivia()
    {
        const string input = "` \n/* raw */ ${ /* skipped */ value /* skipped */ } \r\n// raw `";
        var tokens = _fixture.Success(input).AllTokens.ToArray();
        Assert.Equal(new[] { "` \n/* raw */ ${", "value", "} \r\n// raw `" }, tokens.Select(token => token.Input));
        Assert.Equal(" /* skipped */ ", tokens[1].LeadingTrivia.Text);
        Assert.Equal(" /* skipped */ ", tokens[2].LeadingTrivia.Text);
        Assert.False(tokens[1].HasLineBreakBefore);
        Assert.False(tokens[2].HasLineBreakBefore);
    }

    [Fact]
    public void Source_peeks_preserve_checkpoints_and_only_return_the_next_template_state()
    {
        const string input = "`a${value}b${other}c` / divisor";
        var source = _fixture.Language.CreateTokenSource(input);
        var initial = source.Start;
        var head = source.Peek(initial, Goal(source, regex: true, tail: false));
        var repeatedHead = source.Peek(initial, Goal(source, regex: true, tail: false));
        Assert.Equal("`a${", head.Token.Input);
        Assert.Equal(0, initial.SourceOffset);
        Assert.Equal(0, initial.TokenIndex);
        Assert.Equal(head.Token.Input, repeatedHead.Token.Input);
        Assert.Equal(head.Next.LexicalState, repeatedHead.Next.LexicalState);
        Assert.NotEqual(initial.LexicalState, head.Next.LexicalState);

        var value = source.Peek(head.Next, Goal(source, regex: true, tail: true));
        var middleStart = value.Next;
        var middle = source.Peek(middleStart, Goal(source, regex: false, tail: true));
        var repeatedMiddle = source.Peek(middleStart, Goal(source, regex: true, tail: true));
        Assert.Equal("}b${", middle.Token.Input);
        Assert.Equal(middle.Token.Input, repeatedMiddle.Token.Input);
        Assert.Equal(middle.Next.LexicalState, repeatedMiddle.Next.LexicalState);
        Assert.Equal(input.IndexOf("}b${", StringComparison.Ordinal), middleStart.SourceOffset);
        Assert.Equal(2, middleStart.TokenIndex);

        var other = source.Peek(middle.Next, Goal(source, regex: true, tail: true));
        var tail = source.Peek(other.Next, Goal(source, regex: false, tail: true));
        Assert.Equal("}c`", tail.Token.Input);
        Assert.Equal(initial.LexicalState, tail.Next.LexicalState);
        Assert.Equal(5, tail.Next.TokenIndex);
        Assert.Same(source.Start.SourceIdentity, tail.Next.SourceIdentity);
        var division = source.Peek(tail.Next, Goal(source, regex: false, tail: false));
        Assert.Equal("/", division.Token.Input);
        Assert.Equal(" ", division.Token.LeadingTrivia.Text);
    }

    [Fact]
    public void Independent_source_checkpoints_and_later_parses_do_not_inherit_template_state()
    {
        var first = _fixture.Language.CreateTokenSource("`outer ${value}`");
        var second = _fixture.Language.CreateTokenSource("`other ${name}`");
        var firstHead = first.Peek(first.Start, Goal(first, regex: true, tail: false));
        Assert.NotEqual(first.Start.LexicalState, firstHead.Next.LexicalState);
        Assert.Equal(first.Start.LexicalState, second.Start.LexicalState);
        Assert.Equal("`other ${", second.Peek(second.Start, Goal(second, regex: true, tail: false)).Token.Input);

        _fixture.Invalid("`unterminated ${`nested ${value}");
        _fixture.Success("/a/.test(value)");
        var tree = _fixture.Success("`outer ${`inner ${value}`} tail`");
        var again = _fixture.Success("`outer ${`inner ${value}`} tail`");
        Assert.Equal(JavaScriptExpressionFixture.Text(tree), JavaScriptExpressionFixture.Text(again));
        Assert.NotSame(tree.AllTokens[0], again.AllTokens[0]);
    }

    [Fact]
    public void Many_substitutions_and_nested_templates_finish_with_the_matching_closing_fragment()
    {
        string substitutions = "`" + string.Concat(Enumerable.Repeat("${value}", 64)) + "`";
        var tokens = _fixture.Success(substitutions).AllTokens.ToArray();
        Assert.Equal(1, tokens.Count(token => token.Kind.Caption == "TemplateHead"));
        Assert.Equal(63, tokens.Count(token => token.Kind.Caption == "TemplateMiddle"));
        Assert.Equal(1, tokens.Count(token => token.Kind.Caption == "TemplateTail"));

        string nested = "value";
        for (int depth = 0; depth < 24; depth++) nested = "`${" + nested + "}`";
        _fixture.Success(nested);
        _fixture.Invalid(nested.Substring(0, nested.Length - 1));
    }

    // Equivalent token bundles may carry different branch facts; select a representative.
    private static LexicalGoal Goal(ITokenSource source, bool regex, bool tail)
        => source.Policy.Goals.First(goal =>
            goal.Terminals.Any(terminal => terminal.Caption == "RegularExpressionLiteral") == regex &&
            goal.Terminals.Any(terminal => terminal.Caption == "TemplateTail") == tail);
}
