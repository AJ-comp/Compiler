using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptClassIndexSignatures")]
public sealed class TypeScriptClassIndexSignatureTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("readonly")]
    [InlineData("string")]
    [InlineData("number")]
    [InlineData("boolean")]
    [InlineData("any")]
    [InlineData("unknown")]
    [InlineData("undefined")]
    [InlineData("intrinsic")]
    [InlineData("keyof")]
    [InlineData("unique")]
    [InlineData("infer")]
    [InlineData("abstract")]
    [InlineData("satisfies")]
    [InlineData("await")]
    public void Contextual_and_type_keywords_can_name_an_index_parameter(string name)
        => fixture.Accept("class C { [" + name + ": string]: number; }");

    [Theory]
    [InlineData("class C { readonly [string: string]: number; }")]
    [InlineData("class C { static [readonly: string]: number; }")]
    [InlineData("class C { static readonly [string: string]: number; }")]
    [InlineData("class C { [str\\u0069ng: string]: number; }")]
    [InlineData("class C { [read\\u006fnly: string]: number; }")]
    [InlineData("class C { [readonly]: number; [string](): void {} }")]
    public void Class_index_bindings_keep_modifiers_escapes_and_computed_member_names_separate(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("void")]
    [InlineData("class")]
    [InlineData("enum")]
    [InlineData("yield")]
    [InlineData("implements")]
    [InlineData("interface")]
    [InlineData("let")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("public")]
    [InlineData("static")]
    [InlineData("eval")]
    [InlineData("arguments")]
    [InlineData("pr\\u0069vate")]
    [InlineData("v\\u006fid")]
    public void Class_index_parameters_keep_reserved_and_strict_binding_restrictions(string name)
        => fixture.Reject("class C { [" + name + ": string]: number; }");

    [Theory]
    [InlineData("class C { [string: string, other: string]: number; }")]
    [InlineData("class C { [string: string]; }")]
    [InlineData("async function run() { class C { [await: string]: number; } }")]
    public void Index_shape_and_await_context_restrictions_remain_in_force(string source)
        => fixture.Reject(source);

    [Fact]
    public void Class_specific_parameter_nodes_keep_the_annotation_checks_and_original_source_positions()
    {
        const string source = "class C { [string: string]: number; }";
        var parsed = fixture.Parser.Parsing(fixture.Language.CreateTokenSource(source));
        Assert.True(parsed.Success, source + Environment.NewLine + fixture.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var tree = parsed.ToParseTree;
        var nodes = Descendants(tree).ToArray();
        var parents = nodes.Select(node => node.Parent).ToArray();
        var tokens = tree.AllTokens.ToArray();
        Assert.Single(nodes, node => Role(node) == "TSClassIndexParameter");
        Assert.Empty(fixture.Language.Validate(tree));
        Assert.Equal(tokens, tree.AllTokens);
        for (int i = 0; i < nodes.Length; i++) Assert.Same(parents[i], nodes[i].Parent);
        foreach (var token in tokens)
            Assert.Equal(token.Input, source.Substring(token.StartIndex, token.EndIndex - token.StartIndex + 1));
    }

    private static IEnumerable<ParseTreeNonTerminal> Descendants(ParseTreeSymbol root)
    {
        var pending = new Stack<ParseTreeSymbol>();
        pending.Push(root);
        while (pending.Count != 0)
        {
            if (!(pending.Pop() is ParseTreeNonTerminal node)) continue;
            yield return node;
            foreach (var child in node.Items) pending.Push(child);
        }
    }

    private static string Role(ParseTreeSymbol node)
        => (node as ParseTreeNonTerminal)?.SignPost.Name.Split(new[] { "__" }, StringSplitOptions.None)[0] ?? string.Empty;
}
