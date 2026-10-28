using System;
using System.Linq;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptClassLexing")]
public sealed class TypeScriptClassLexicalTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("left < right;")]
    [InlineData("left < right as T;")]
    [InlineData("left < right satisfies T;")]
    [InlineData("const result = left << right;")]
    [InlineData("enum Bits { First = 1 << 0, Second = 1 << 1 }")]
    [InlineData("class C { public value = left < right; }")]
    [InlineData("class C { public run() { return left < right; } }")]
    [InlineData("class C { constructor(public value = left < right) {} }")]
    [InlineData("class C extends factory(left < right) {}")]
    [InlineData("class C extends (left < right ? First : Second) {}")]
    [InlineData("class C extends registry[left < right] {}")]
    [InlineData("const value = obj.extends < right;")]
    [InlineData("const value = obj.class < right;")]
    [InlineData("const value = obj.value < right;")]
    [InlineData("const value = obj?.value < right;")]
    [InlineData("const value = obj[key] < right;")]
    [InlineData("const value = [left] < right;")]
    [InlineData("const value = obj[key] << right;")]
    [InlineData("class C { #value; run() { return this.#value < right; } }")]
    [InlineData("const value = ({extends: left < right});")]
    [InlineData("const value = ({class: left < right});")]
    [InlineData("class C extends Base { public value = obj.extends < right; }")]
    [InlineData("class C extends Base {} left < right;")]
    [InlineData("const value = class C extends Base {} < right;")]
    [InlineData("const value = function() {} < right;")]
    [InlineData("function run() { left < right; }")]
    [InlineData("class C { run() { left < right; } }")]
    [InlineData("const run = () => { left < right; };")]
    [InlineData("label: { left < right; }")]
    public void Generic_class_heritage_does_not_reclassify_runtime_less_than_or_left_shift(string source)
    {
        var tree = Parse(source);
        var operators = tree.AllTokens.Where(token => token.Input == "<" || token.Input == "<<").ToArray();
        Assert.NotEmpty(operators);
        Assert.All(operators, token => Assert.False(IsTypeOpen(token.Kind.Caption)));
    }

    [Theory]
    [InlineData("class C extends Base<T> {}", 1, 0)]
    [InlineData("class C<T> extends Base<T> {}", 2, 0)]
    [InlineData("class C extends ns.Base<Box<T>> {}", 2, 0)]
    [InlineData("class C extends factory(Base)<T> {}", 1, 0)]
    [InlineData("class C extends (Base)<T> {}", 1, 0)]
    [InlineData("class C extends class {}<T> {}", 1, 0)]
    [InlineData("class C extends function() {}<T> {}", 1, 0)]
    [InlineData("class C extends {}<T> {}", 1, 0)]
    [InlineData("class C extends factory(left < right)<T> {}", 1, 1)]
    [InlineData("class C<T> extends Base<T> { public value = left < right; }", 2, 1)]
    [InlineData("class C extends Base<T> implements Named<T> { public run<U>(x: U): U { return x; } }", 3, 0)]
    [InlineData("class C extends Base<T> { public run() { class D extends Other<U> {} return left < right; } }", 2, 1)]
    [InlineData("class C { public run<T>(x: T): T { return x; } }", 1, 0)]
    [InlineData("class C { public [key]<T>(x: T): T { return x; } }", 1, 0)]
    [InlineData("const obj = { run<T>(x: T): T { return x; } };", 1, 0)]
    [InlineData("const obj = ok ? {} : { run<T>(x: T): T { return x; } };", 1, 0)]
    [InlineData("class C { run() { return { run<T>(x: T): T { return x; } }; } }", 1, 0)]
    public void Actual_heritage_type_arguments_and_runtime_comparisons_keep_distinct_token_kinds(
        string source, int typeAngles, int comparisons)
    {
        var tree = Parse(source);
        Assert.Equal(typeAngles, tree.AllTokens.Count(token => IsTypeOpen(token.Kind.Caption)));
        Assert.Equal(comparisons, tree.AllTokens.Count(token => token.Input == "<" && !IsTypeOpen(token.Kind.Caption)));
    }

    private static bool IsTypeOpen(string kind) => kind == "TSTypeOpen" || kind == "TSClassHeritageOpen";

    private Janglim.FrontEnd.ParseTree.ParseTreeSymbol Parse(string source)
    {
        var parsed = fixture.Parser.Parsing(fixture.Language.CreateTokenSource(source));
        Assert.True(parsed.Success, source + Environment.NewLine + fixture.Parser.FiredException + Environment.NewLine +
            string.Join(Environment.NewLine, parsed.AllErrors.Select(error => error.Message)));
        Assert.Empty(parsed.AllErrors);
        var tree = parsed.ToParseTree;
        Assert.Empty(fixture.Language.Validate(tree));
        return tree;
    }
}
