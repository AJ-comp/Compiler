using System;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers.LR;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[CollectionDefinition(Name)]
public sealed class TypeScriptRuntimeCollection
{ public const string Name = "TypeScript runtime composition"; }

internal sealed class TypeScriptRuntimeFixture
{
    internal static readonly Lazy<TypeScriptRuntimeFixture> Shared = new(() => new());
    internal TypeScriptGrammar Language { get; } = new(mode: JavaScriptMode.Strict);
    internal LALRParser Parser { get; }
    private TypeScriptRuntimeFixture() => Parser = new LALRParser(Language.Grammar, false);
    internal void Accept(string input)
    {
        var parsed = Parser.Parsing(Language.CreateTokenSource(input));
        Assert.True(parsed.Success, input + Environment.NewLine + Parser.FiredException + Environment.NewLine +
            string.Join(Environment.NewLine, parsed.AllErrors.Select(e => e.Message)));
        Assert.Empty(parsed.AllErrors);
        var diagnostics = Language.Validate(parsed.ToParseTree);
        Assert.True(diagnostics.Count == 0, input + Environment.NewLine +
            string.Join(Environment.NewLine, diagnostics.Select(d => d.Code + ": " + d.Message)));
    }
    internal void Reject(string input)
    {
        var parsed = Parser.Parsing(Language.CreateTokenSource(input));
        if (!parsed.Success)
        {
            Assert.True(Parser.FiredException == null || Parser.FiredException is ParsingException ||
                Parser.FiredException is JavaScriptSyntaxException ||
                Parser.FiredException.GetType().Name == "PredictionFailedException",
                "An internal parser failure must not count as a rejected TypeScript program: " + Parser.FiredException);
            Assert.NotEmpty(parsed.AllErrors); return;
        }
        Assert.NotEmpty(Language.Validate(parsed.ToParseTree));
    }
}

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptRuntime")]
public sealed class TypeScriptRuntimeTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("let value: number = 1; value / 2;")]
    [InlineData("const name: string = 'test';")]
    [InlineData("let value: Array<Map<string, number>>;")]
    [InlineData("const { name }: {name: string} = user;")]
    [InlineData("function add(a: number, b: number): number { return a + b; }")]
    [InlineData("function identity<T>(value: T): T { return value; }")]
    [InlineData("function f(value?: string): void {}")]
    [InlineData("function f(value: string = 'a', ...rest: string[]): void {}")]
    [InlineData("function f(this: Context, value: string): void {}")]
    [InlineData("async function f(value: Promise<number>): Promise<number> { return await value; }")]
    [InlineData("class Counter { value: number = 0; next(step: number): number { return this.value += step; } }")]
    [InlineData("let type = 1; let string = 2; type + string;")]
    [InlineData("const value: number = 'text';")]
    [InlineData("let value: UnknownType;")]
    public void Accepts_types_without_losing_JavaScript_runtime_validation(string source) => fixture.Accept(source);

    [Theory]
    [InlineData("let value: = 1;")]
    [InlineData("function f(a?: number, b: number) {}")]
    [InlineData("function f(a?: number = 1) {}")]
    [InlineData("for (const item: string of list) {}")]
    [InlineData("function f(a: number, a: string) {}")]
    [InlineData("const value: number = ;")]
    public void Rejects_syntax_and_grammar_errors(string source) => fixture.Reject(source);
}
