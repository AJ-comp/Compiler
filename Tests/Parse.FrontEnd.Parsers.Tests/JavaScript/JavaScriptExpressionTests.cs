using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// A parser owns mutable per-parse state. Both suites use one collection so its expensive
// LALR table is shared without running calls on the same parser concurrently.
[CollectionDefinition(Name)]
public sealed class JavaScriptExpressionCollection : ICollectionFixture<JavaScriptExpressionFixture>
{
    public const string Name = "ECMAScript expression grammar";
}

public sealed class JavaScriptExpressionFixture
{
    public JavaScriptExpressionGrammar Language { get; } = new JavaScriptExpressionGrammar();
    public LALRParser Parser { get; }

    public JavaScriptExpressionFixture() => Parser = new LALRParser(Language.Grammar, false);

    public ParsingResult Parse(string input) => Parser.Parsing(Language.CreateTokenSource(input));

    public ParseTreeSymbol Success(string input)
    {
        var result = Parse(input);
        Assert.True(result.Success, input + Environment.NewLine + Parser.FiredException);
        Assert.Empty(result.AllErrors);
        Assert.NotNull(result.ToParseTree);
        Assert.Empty(Language.Validate(result.ToParseTree));
        return result.ToParseTree;
    }

    public void Invalid(string input)
    {
        var result = Parse(input);
        if (!result.Success)
        {
            Assert.NotEmpty(result.AllErrors);
            return;
        }

        var diagnostics = Language.Validate(result.ToParseTree);
        Assert.True(diagnostics.Count > 0, "Unexpectedly accepted: " + input);
        Assert.All(diagnostics, diagnostic =>
        {
            Assert.False(string.IsNullOrWhiteSpace(diagnostic.Code));
            Assert.False(string.IsNullOrWhiteSpace(diagnostic.Message));
            Assert.InRange(diagnostic.SourceOffset, 0, input.Length);
            Assert.InRange(diagnostic.Length, 0, input.Length - diagnostic.SourceOffset);
        });
    }

    public static string Text(ParseTreeSymbol node) => string.Join(" ", node.AllTokens.Select(t => t.Input));

    public static IEnumerable<ParseTreeNonTerminal> Nodes(ParseTreeSymbol node)
    {
        if (node is not ParseTreeNonTerminal parent) yield break;
        yield return parent;
        foreach (var child in parent)
        foreach (var descendant in Nodes(child))
            yield return descendant;
    }
}

[Collection(JavaScriptExpressionCollection.Name)]
[Trait("Category", "JavaScript")]
public sealed class JavaScriptExpressionTests
{
    private readonly JavaScriptExpressionFixture _fixture;
    public JavaScriptExpressionTests(JavaScriptExpressionFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("value")]
    [InlineData("this")]
    [InlineData("true")]
    [InlineData("false")]
    [InlineData("null")]
    [InlineData("(value)")]
    [InlineData("((1 + 2) * 3)")]
    [InlineData("+value")]
    [InlineData("-value")]
    [InlineData("!value")]
    [InlineData("~value")]
    [InlineData("typeof value")]
    [InlineData("void value")]
    [InlineData("delete object.value")]
    [InlineData("++value")]
    [InlineData("--value")]
    [InlineData("value++")]
    [InlineData("value--")]
    [InlineData("a + b - c * d / e % f")]
    [InlineData("a << b >> c >>> d")]
    [InlineData("a < b <= c > d >= e")]
    [InlineData("a in b")]
    [InlineData("a instanceof B")]
    [InlineData("a == b != c === d !== e")]
    [InlineData("a & b ^ c | d")]
    [InlineData("a && b || c")]
    [InlineData("a ?? b ?? c")]
    [InlineData("a ? b : c")]
    [InlineData("a ? b = c : d = e")]
    [InlineData("a, b, c")]
    [InlineData("a = b = c")]
    [InlineData("a += b")]
    [InlineData("a -= b")]
    [InlineData("a *= b")]
    [InlineData("a /= b")]
    [InlineData("a %= b")]
    [InlineData("a **= b")]
    [InlineData("a <<= b")]
    [InlineData("a >>= b")]
    [InlineData("a >>>= b")]
    [InlineData("a &= b")]
    [InlineData("a ^= b")]
    [InlineData("a |= b")]
    [InlineData("a &&= b")]
    [InlineData("a ||= b")]
    [InlineData("a ??= b")]
    [InlineData("await + 1")]
    [InlineData("\\u0061wait + 1")]
    [InlineData("delete (a, b)")]
    public void Strict_script_expression_forms_are_accepted(string input) => _fixture.Success(input);

    [Theory]
    [InlineData("object.member")]
    [InlineData("object.default")]
    [InlineData("object.if")]
    [InlineData("object.true")]
    [InlineData("object.null")]
    [InlineData("object.await")]
    [InlineData("object.yield")]
    [InlineData("object[1 + 2]")]
    [InlineData("f()")]
    [InlineData("f(a)")]
    [InlineData("f(a, b)")]
    [InlineData("f(a,)")]
    [InlineData("f(...items)")]
    [InlineData("f(a, ...items)")]
    [InlineData("f()(a).member[index]")]
    [InlineData("new Constructor")]
    [InlineData("new Constructor()")]
    [InlineData("new Constructor(a).member")]
    [InlineData("new new Constructor()")]
    [InlineData("object?.member")]
    [InlineData("object?.[index]")]
    [InlineData("object?.(argument)")]
    [InlineData("object?.member[index](argument)?.next")]
    [InlineData("(object?.member).next = value")]
    [InlineData("f().member = value")]
    [InlineData("(value) = other")]
    [InlineData("(value)++")]
    [InlineData("new (object?.constructor)()")]
    public void Member_call_new_and_optional_chains_are_accepted(string input) => _fixture.Success(input);

    [Theory]
    [InlineData("[]")]
    [InlineData("[,]")]
    [InlineData("[,,]")]
    [InlineData("[,a]")]
    [InlineData("[a,,b,]")]
    [InlineData("[...items]")]
    [InlineData("[a, ...items,]")]
    [InlineData("[a ? b : c, [d]]")]
    [InlineData("{}")]
    [InlineData("({})")]
    [InlineData("{a}")]
    [InlineData("{a: 1}")]
    [InlineData("{default: value}")]
    [InlineData("{true: value}")]
    [InlineData("{[key]: value}")]
    [InlineData("{'name': value, 1: other,}")]
    [InlineData("{...source, a: [1, 2]}")]
    [InlineData("f(...items,)")]
    [InlineData("object.\\u0069f")]
    [InlineData("object.\\u0073tatic")]
    [InlineData("{__proto__: 1, a: {__proto__: 2}}")]
    [InlineData("{__proto__: 1, ['__proto__']: 2}")]
    [InlineData("{__proto__: 1, __proto__}")]
    [InlineData("{'\\u{00000000000000000000000000005f}_proto__': 1}")]
    public void Arrays_and_object_data_properties_preserve_elisions_spread_and_reserved_property_names(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("[a,,b")]
    [InlineData("[, ...]")]
    [InlineData("{a:}")]
    [InlineData("{a: 1,,b: 2}")]
    [InlineData("{,}")]
    [InlineData("{if}")]
    [InlineData("{[a,b]: value}")]
    public void Malformed_arrays_and_object_data_properties_are_rejected(string input) => _fixture.Invalid(input);

    [Theory]
    [InlineData("2 ** 3 ** 4")]
    [InlineData("2 ** -3")]
    [InlineData("(-2) ** 3")]
    [InlineData("(++value) ** 2")]
    [InlineData("++value ** 2")]
    [InlineData("(a ?? b) || c")]
    [InlineData("a ?? (b || c)")]
    [InlineData("a && (b ?? c)")]
    [InlineData("(a && b) ?? c")]
    [InlineData("a?.3:0")]
    [InlineData("0..toString()")]
    public void Operator_restrictions_allow_explicit_grouping_and_valid_lexical_boundaries(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("()")]
    [InlineData("a +")]
    [InlineData("a b")]
    [InlineData("a;")]
    [InlineData("f(,)")]
    [InlineData("f(a,,b)")]
    [InlineData("object[]")]
    [InlineData("object.")]
    [InlineData("object?.")]
    [InlineData("new object?.constructor()")]
    [InlineData("new C?.x")]
    [InlineData("-2 ** 3")]
    [InlineData("+2 ** 3")]
    [InlineData("!value ** 2")]
    [InlineData("typeof value ** 2")]
    [InlineData("a ?? b || c")]
    [InlineData("a || b ?? c")]
    [InlineData("a ?? b && c")]
    [InlineData("a && b ?? c")]
    [InlineData("a >>>> b")]
    [InlineData("a ? b")]
    [InlineData("a ? : b")]
    [InlineData("1.toString()")]
    public void Malformed_syntax_and_forbidden_operator_combinations_are_rejected(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("1 = value")]
    [InlineData("true = value")]
    [InlineData("(a + b) = value")]
    [InlineData("a?.b = value")]
    [InlineData("a?.b += value")]
    [InlineData("a?.b++")]
    [InlineData("++a?.b")]
    [InlineData("1++")]
    [InlineData("++(a + b)")]
    [InlineData("delete value")]
    [InlineData("delete (value)")]
    [InlineData("eval = value")]
    [InlineData("arguments++")]
    [InlineData("(eval) = value")]
    [InlineData("f() = value")]
    [InlineData("f()++")]
    [InlineData("delete ((value))")]
    public void Strict_script_early_errors_are_reported_with_source_diagnostics(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("{__proto__: 1, '__proto__': 2}")]
    [InlineData("{__proto__: 1, '\\u005f_proto__': 2}")]
    [InlineData("{__proto__: 1, '\\u{00000000000000000000000000005f}_proto__': 2}")]
    [InlineData("{__proto__: 1, \\u005f_proto__: 2}")]
    public void Duplicate_prototype_setters_are_rejected_after_decoding_property_names(string input)
    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, _fixture.Parser.FiredException?.ToString());
        var diagnostic = Assert.Single(_fixture.Language.Validate(parsed.ToParseTree));
        Assert.Equal("duplicate_proto", diagnostic.Code);
        Assert.True(diagnostic.Length > 0);
        Assert.Contains("proto", input.Substring(diagnostic.SourceOffset, diagnostic.Length));
    }

    [Theory]
    [InlineData("1n + 1")]
    [InlineData("1n >>> 1n")]
    [InlineData("null.value")]
    [InlineData("1()")]
    public void Runtime_errors_are_not_misclassified_as_syntax_errors(string input) => _fixture.Success(input);

    [Theory]
    [InlineData("1 + 2 * 3", "AdditiveExpression", "MultiplicativeExpression", "2 * 3")]
    [InlineData("a + b << c", "ShiftExpression", "AdditiveExpression", "a + b")]
    [InlineData("a | b && c", "LogicalANDExpression", "BitwiseORExpression", "a | b")]
    [InlineData("a === b | c", "BitwiseORExpression", "EqualityExpression", "a === b")]
    [InlineData("a ?? b ? c : d", "ConditionalExpression", "CoalesceExpression", "a ?? b")]
    [InlineData("a = b ? c : d", "AssignmentExpression", "ConditionalExpression", "b ? c : d")]
    public void Concrete_tree_preserves_operator_precedence(
        string input, string outerName, string innerName, string innerText)
    {
        var tree = _fixture.Success(input);
        var outer = Assert.Single(JavaScriptExpressionFixture.Nodes(tree), node =>
            node.SignPost.Name == outerName && JavaScriptExpressionFixture.Text(node) == input);
        Assert.Contains(JavaScriptExpressionFixture.Nodes(outer), node =>
            node.SignPost.Name == innerName && JavaScriptExpressionFixture.Text(node) == innerText);
    }

    [Theory]
    [InlineData("a - b - c", "AdditiveExpression", "a - b", "c")]
    [InlineData("a ** b ** c", "ExponentiationExpression", "a", "b ** c")]
    [InlineData("a = b = c", "AssignmentExpression", "a", "b = c")]
    [InlineData("a ?? b ?? c", "CoalesceExpression", "a ?? b", "c")]
    [InlineData("a ? b : c ? d : e", "ConditionalExpression", "a", "c ? d : e")]
    public void Concrete_tree_preserves_left_and_right_associativity(
        string input, string nodeName, string left, string right)
    {
        var tree = _fixture.Success(input);
        var operation = Assert.Single(JavaScriptExpressionFixture.Nodes(tree), node =>
            node.SignPost.Name == nodeName && JavaScriptExpressionFixture.Text(node) == input);
        Assert.Equal(left, JavaScriptExpressionFixture.Text(operation[0]));
        Assert.Equal(right, JavaScriptExpressionFixture.Text(operation[operation.Count - 1]));
    }

    [Fact]
    public void Repeated_parses_keep_sources_and_concrete_trees_independent()
    {
        var first = _fixture.Success("a + b * c");
        var firstTokens = first.AllTokens.ToArray();
        firstTokens[0].IsNotUsed = true;
        var second = _fixture.Success("/a/.test(value)");
        var third = _fixture.Success("a + b * c");

        Assert.Equal("a + b * c", JavaScriptExpressionFixture.Text(first));
        Assert.Equal("/a/ . test ( value )", JavaScriptExpressionFixture.Text(second));
        Assert.Equal(JavaScriptExpressionFixture.Text(first), JavaScriptExpressionFixture.Text(third));
        Assert.NotSame(first, third);
        Assert.NotSame(firstTokens[0], third.AllTokens[0]);
        Assert.False(third.AllTokens[0].IsNotUsed);
        Assert.Same(_fixture.Language.Policy, _fixture.Language.CreateTokenSource("value").Policy);
    }

    [Fact]
    public void Factory_and_validation_reject_null_arguments_with_the_public_parameter_name()
    {
        Assert.Equal("input", Assert.Throws<ArgumentNullException>(
            () => _fixture.Language.CreateTokenSource(null)).ParamName);
        Assert.Equal("tree", Assert.Throws<ArgumentNullException>(
            () => _fixture.Language.Validate(null)).ParamName);
    }

    [Fact]
    public void Fresh_language_factories_own_separate_grammar_terminals_policy_and_sources()
    {
        var language = new JavaScriptExpressionGrammar();
        var first = _fixture.Language.CreateTokenSource("value");
        var repeated = _fixture.Language.CreateTokenSource("value");
        var other = language.CreateTokenSource("value");
        Assert.NotSame(_fixture.Language.Grammar, language.Grammar);
        Assert.NotSame(_fixture.Language.Policy, language.Policy);
        Assert.NotSame(first.Start.SourceIdentity, repeated.Start.SourceIdentity);
        Assert.NotSame(first.Start.SourceIdentity, other.Start.SourceIdentity);

        var firstRead = first.Peek(first.Start, first.Policy.Goals[0]);
        var repeatedRead = repeated.Peek(repeated.Start, repeated.Policy.Goals[0]);
        var otherRead = other.Peek(other.Start, other.Policy.Goals[0]);
        Assert.Same(firstRead.Token.Kind, repeatedRead.Token.Kind);
        Assert.NotSame(firstRead.Token, repeatedRead.Token);
        Assert.NotSame(firstRead.Token.Kind, otherRead.Token.Kind);
        firstRead.Token.TokenCell.RefOptionalData = new object();
        Assert.Null(repeatedRead.Token.TokenCell.RefOptionalData);
        Assert.Null(otherRead.Token.TokenCell.RefOptionalData);

        var parser = new LALRParser(language.Grammar, false);
        var parsed = parser.Parsing(other);
        Assert.True(parsed.Success, parser.FiredException?.ToString());
        Assert.Empty(language.Validate(parsed.ToParseTree));
        Assert.Equal("value", parsed.ToParseTree.AllInputDatas);
    }
}
