using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptClassPrivateSignatures")]
public sealed class TypeScriptClassPrivateSignatureTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    // Missing implementations and overload type compatibility are outside the
    // grammar layer. A signature still declares its private lexical name.
    [Theory]
    [InlineData("class C { #m(): void; use() { this.#m(); } }")]
    [InlineData("class C { #m(value: number): void; #m(value: string): void; use() { this.#m(1); } }")]
    [InlineData("class C { #m(value: number): void; #m(value: any) {} use() { this.#m(1); } }")]
    [InlineData("class C { static #m(): void; static use() { this.#m(); } }")]
    [InlineData("class C { static #m(value: number): void; static #m(value: any) {} static use() { this.#m(1); } }")]
    [InlineData("class Outer { #m(): void; create() { return class Inner { use(value: Outer) { value.#m(); } }; } }")]
    [InlineData("class Outer { #m(): void; create() { return class Inner { #m(): void; use() { this.#m(); } }; } }")]
    [InlineData("class C { #\\u006d(): void; #m(value: number): void; use() { this.#m(1); } }")]
    [InlineData("class C { #m(): IterableIterator<number>; *#m() { yield 1; } use() { this.#m(); } }")]
    [InlineData("class C { #m(): Promise<void>; async #m() {} use() { this.#m(); } }")]
    public void Private_signatures_keep_their_binding_without_duplicating_an_overload_implementation(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("class C { #m(): void; use() { this.#missing(); } }")]
    [InlineData("class C { #m(): void; } class D { use() { this.#m(); } }")]
    [InlineData("class C { #m(): void; #m: number; }")]
    [InlineData("class C { static #m(): void; #m() {} }")]
    [InlineData("class C { #m(): void; static #m() {} }")]
    [InlineData("class C { *m(): IterableIterator<number>; }")]
    [InlineData("class C { *#m(): IterableIterator<number>; }")]
    public void Signature_projection_preserves_private_scope_conflicts_and_generator_restrictions(string source)
        => fixture.Reject(source);

    [Fact]
    public void Private_binding_projection_preserves_the_original_tree_and_token_positions()
    {
        const string source = "class C { static #m(): void; static use() { this.#m(); } }";
        var parsed = fixture.Parser.Parsing(fixture.Language.CreateTokenSource(source));
        Assert.True(parsed.Success, source + Environment.NewLine + fixture.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var tree = parsed.ToParseTree;
        var nodes = Descendants(tree).ToArray();
        var parents = nodes.Select(node => node.Parent).ToArray();
        var tokens = tree.AllTokens.ToArray();

        Assert.Contains(nodes, node => Role(node) == "TSMethodOverload");
        Assert.Empty(fixture.Language.Validate(tree));
        Assert.Empty(fixture.Language.Validate(tree));

        Assert.Equal(tokens, tree.AllTokens);
        for (int i = 0; i < nodes.Length; i++) Assert.Same(parents[i], nodes[i].Parent);
        foreach (var token in tokens)
            Assert.Equal(token.Input, source.Substring(token.StartIndex, token.EndIndex - token.StartIndex + 1));
    }

    [Fact]
    public void An_undeclared_private_reference_keeps_its_original_diagnostic_location()
    {
        const string source = "class C { #m(): void; use() { this.#missing(); } }";
        var parsed = fixture.Parser.Parsing(fixture.Language.CreateTokenSource(source));
        Assert.True(parsed.Success, source + Environment.NewLine + fixture.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var diagnostic = Assert.Single(fixture.Language.Validate(parsed.ToParseTree),
            item => item.Code == "undeclared_private_name");
        Assert.Equal(source.IndexOf("#missing", StringComparison.Ordinal), diagnostic.SourceOffset);
        Assert.Equal("#missing".Length, diagnostic.Length);
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
