using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptExpressionLexing")]
public sealed class TypeScriptAssertionLexicalTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("const result = left < right;")]
    [InlineData("const result = left > right;")]
    [InlineData("const result = value as T / 2;")]
    [InlineData("const result = value as T + /ab/.test(text);")]
    [InlineData("const result = (value as {a: number}) / 2;")]
    [InlineData("const result = (value as {run(): string;}) / 2;")]
    [InlineData("function f(): T { return /ab/.test(text); }")]
    [InlineData("function f(value: Box<T> = left > right): T { return value; }")]
    [InlineData("const result = value satisfies T / 2;")]
    public void Leaving_type_syntax_restores_runtime_operator_and_brace_roles(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("const result = value! / 2;")]
    [InlineData("const result = value!! / 2;")]
    [InlineData("const result = obj?.value! / 2;")]
    [InlineData("const result = !value / 2;")]
    [InlineData("const result = value != /ab/.test(text);")]
    [InlineData("const result = value! != /ab/.test(text);")]
    [InlineData("const result = value! + /ab/.test(text);")]
    public void Only_postfix_non_null_assertions_are_transparent_to_slash_context(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("const result = value as T >= other;")]
    [InlineData("const result = value as T >> count;")]
    [InlineData("const result = value as T >>> count;")]
    [InlineData("const result = value as Box<T>>= other;")]
    [InlineData("const result = value as Box<T>>> count;")]
    [InlineData("const result = value as Box<Box<T>>>> count;")]
    [InlineData("const result = (value as Box<T>) >>= count;")]
    [InlineData("const result = value as number << count;")]
    [InlineData("const result = value as T <= other;")]
    [InlineData("const result = value as Box<T> << count;")]
    public void Generic_closing_tokens_do_not_split_the_following_runtime_operator(string source)
        => fixture.Accept(source);

    [Fact]
    public void A_same_line_left_shift_after_a_type_name_starts_type_arguments_instead()
        => fixture.Reject("const result = value as T << count;");

    [Theory]
    [InlineData("function f() { return namespace + {} / 2; }")]
    [InlineData("function f() { return module + {} / 2; }")]
    [InlineData("function f(module: number) { return module + {} / 2; }")]
    [InlineData("function f(module) {} const result = {} / 2;")]
    [InlineData("const obj = { namespace: {}, module: {}, interface: {}, enum: {} }; obj.module / 2;")]
    [InlineData("const obj = { module() { return namespace + {} / 2; } }; obj.module();")]
    [InlineData("namespace\nvalue + {} / 2;")]
    [InlineData("module\nvalue + {} / 2;")]
    [InlineData("const enum Flags { A } /ab/.test(value);")]
    public void Contextual_declaration_spellings_do_not_create_a_false_declaration_scope(string source)
        => fixture.Accept(source);
}
