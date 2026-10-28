using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptNonNull")]
public sealed class TypeScriptNonNullTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("value!;")]
    [InlineData("value!!;")]
    [InlineData("value !;")]
    [InlineData("value/* comment */!;")]
    [InlineData("!value!;")]
    [InlineData("value!.property;")]
    [InlineData("value![index];")]
    [InlineData("f()!;")]
    [InlineData("f()!.property;")]
    [InlineData("f()!();")]
    [InlineData("f!();")]
    [InlineData("f!.property![index]!;")]
    [InlineData("(value + other)!;")]
    [InlineData("new C!();")]
    [InlineData("new C()!;")]
    [InlineData("new C!.Nested();")]
    [InlineData("tag!`text`;")]
    [InlineData("tag!`value ${value!}`;")]
    [InlineData("/ab/!.test(text);")]
    [InlineData("({ property: 1 })!.property;")]
    public void Non_null_assertions_allow_member_call_constructor_and_template_continuations(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("value! = next;")]
    [InlineData("value! += 1;")]
    [InlineData("value!++;")]
    [InlineData("++value!;")]
    [InlineData("object!.property = next;")]
    [InlineData("object!.property!++;")]
    [InlineData("({ target: value! } = source);")]
    [InlineData("([value!] = source);")]
    public void Non_null_assertions_preserve_valid_assignment_and_update_targets(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("base?.property!;")]
    [InlineData("base!.property?.next;")]
    [InlineData("base?.method!();")]
    [InlineData("base?.property!.next;")]
    [InlineData("(base?.property)!.next = value;")]
    [InlineData("(base?.property)!`text`;")]
    // TS 7.0.2 allows a tagged template after the non-null wrapper. Its member
    // and call continuations, in contrast, propagate the inner optional chain.
    [InlineData("base?.tag!`text`;")]
    [InlineData("(base?.property)!();")]
    [InlineData("new (base?.Ctor)!();")]
    [InlineData("new (base?.tag!`text`)();")]
    public void Non_null_assertions_preserve_optional_chains_and_parenthesized_chain_boundaries(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("value\n!other;", 0)]
    [InlineData("value/*\n*/!other;", 0)]
    [InlineData("value\n!other!;", 1)]
    [InlineData("value!\n.property;", 1)]
    [InlineData("f!\n();", 1)]
    [InlineData("value!\n++other;", 1)]
    [InlineData("value != other;", 0)]
    [InlineData("value !== other;", 0)]
    [InlineData("value!!=other;", 1)]
    [InlineData("value!!==other;", 1)]
    [InlineData("value!!!=other;", 2)]
    [InlineData("!!value;", 0)]
    public void Line_breaks_and_longest_match_distinguish_assertions_from_logical_and_equality_operators(
        string source, int assertionCount)
    {
        var parsed = fixture.Parser.Parsing(fixture.Language.CreateTokenSource(source));
        Assert.True(parsed.Success, source + Environment.NewLine + fixture.Parser.FiredException + Environment.NewLine +
            string.Join(Environment.NewLine, parsed.AllErrors.Select(error => error.Message)));
        Assert.Empty(parsed.AllErrors);
        Assert.Empty(fixture.Language.Validate(parsed.ToParseTree));
        var assertions = Descendants(parsed.ToParseTree)
            .OfType<ParseTreeNonTerminal>()
            .Where(node => BaseRole(node) == "TSNonNullExpression")
            .ToArray();
        Assert.Equal(assertionCount, assertions.Length);
        Assert.All(assertions, node => Assert.Equal("!", node.AllTokens.Last().Input));
    }

    [Fact]
    public void Optional_asserted_tag_projection_preserves_the_callers_tree_tokens_and_parent_links()
    {
        const string source = "base?.tag!`text`;";
        var parsed = fixture.Parser.Parsing(fixture.Language.CreateTokenSource(source));
        Assert.True(parsed.Success);
        var tree = parsed.ToParseTree;
        var nodes = Descendants(tree).ToArray();
        var parents = nodes.Select(node => node.Parent).ToArray();
        var tokens = tree.AllTokens.ToArray();

        Assert.Empty(fixture.Language.Validate(tree));

        Assert.Equal(tokens, tree.AllTokens);
        Assert.Equal("base?.tag!`text`;", string.Concat(tree.AllTokens.Select(token => token.Input)));
        for (int i = 0; i < nodes.Length; i++) Assert.Same(parents[i], nodes[i].Parent);
    }

    [Theory]
    [InlineData("1! = value;")]
    [InlineData("(value + other)! = next;")]
    [InlineData("f()! = value;")]
    [InlineData("f()!++;")]
    [InlineData("1!++;")]
    [InlineData("base?.property! = next;")]
    [InlineData("base?.property!.next = next;")]
    [InlineData("++base?.property!;")]
    [InlineData("base?.property!++;")]
    [InlineData("new base?.Ctor!();")]
    [InlineData("new base?.tag!`text`();")]
    [InlineData("new base?.tag!`text`.Ctor();")]
    [InlineData("delete value!;")]
    [InlineData("let result = value\n!;")]
    [InlineData("let result = value\n!.property;")]
    public void Non_null_assertions_do_not_make_invalid_runtime_syntax_or_targets_valid(string source)
        => fixture.Reject(source);

    private static string BaseRole(ParseTreeNonTerminal node)
    {
        string role = node.SignPost.Name;
        int suffix = role.IndexOf("__", StringComparison.Ordinal);
        if (suffix >= 0) role = role.Substring(0, suffix);
        return role == "TSNonNullExpressionMember" || role == "TSNonNullExpressionCall" ||
            role == "TSNonNullExpressionOptional" ? "TSNonNullExpression" : role;
    }

    private static IEnumerable<ParseTreeSymbol> Descendants(ParseTreeSymbol root)
    {
        var pending = new Stack<ParseTreeSymbol>();
        pending.Push(root);
        while (pending.Count != 0)
        {
            var node = pending.Pop();
            yield return node;
            if (node is ParseTreeNonTerminal nonTerminal)
                for (int i = nonTerminal.Count - 1; i >= 0; i--) pending.Push(nonTerminal[i]);
        }
    }
}
