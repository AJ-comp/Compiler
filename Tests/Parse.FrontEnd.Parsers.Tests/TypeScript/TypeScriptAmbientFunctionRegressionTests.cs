using System;
using System.Linq;
using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers;
using Janglim.FrontEnd.Parsers.LR;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptDeclarations")]
public sealed class TypeScriptAmbientFunctionRegressionTests
{
    private static readonly Lazy<ModuleFixture> Module = new(() => new());

    [Theory]
    [InlineData("export declare namespace Tools { function run(): void; }")]
    [InlineData("declare namespace Tools { function run(): void; } export {};")]
    [InlineData("export declare namespace Tools { function run<T>(value: T): T; }")]
    [InlineData("export declare namespace Tools { function run(value: number): number; }")]
    [InlineData("export declare namespace Tools { function run(value?: number): void; }")]
    [InlineData("export declare namespace Tools { function run(...values: number[]): void; }")]
    [InlineData("export declare namespace Tools { function run({ value }: { value: number }): void; }")]
    [InlineData("export declare namespace Tools { function run([value]: [number]): void; }")]
    [InlineData("export declare namespace Tools { export function run(): void; }")]
    [InlineData("export declare namespace Outer.Inner { function run(): void; }")]
    [InlineData("export declare namespace Tools { function run(): void }")]
    [InlineData("export declare namespace Tools { function first(): void\nfunction second(): void\n}")]
    public void Module_ambient_namespace_accepts_function_signatures(string source)
        => Module.Value.Accept(source);

    [Theory]
    [InlineData("function run(): void; function run(): void {} export { run };")]
    [InlineData("function run(value: number): number; function run(value: number): number { return value; } export { run };")]
    [InlineData("export function run<T>(value: T): T; export function run<T>(value: T): T { return value; }")]
    [InlineData("function run(): void {} export { run };")]
    [InlineData("function run(): void\n{} export { run };")]
    [InlineData("function run(value: number): number\n{ return value; } export { run };")]
    [InlineData("function run(value: number): number { return value; } export { run };")]
    [InlineData("function run(...values: number[]): number { return values.length; } export { run };")]
    [InlineData("export function run(value: number = 1): number { return value; }")]
    [InlineData("export function run(value: number): number; export function run(value: string): string; export function run(value: number | string): number | string { return value; }")]
    [InlineData("namespace Tools { export function run(): void {} } export {};")]
    [InlineData("export declare namespace Types { function make(): { value: number }; } export function make(): { value: number } { return { value: 1 }; }")]
    [InlineData("export declare namespace Types { function get(): (value: number) => number; }")]
    [InlineData("function outer(): void { function inner(): void; function inner(): void {} } export {};")]
    public void Shared_function_header_preserves_implementations_overloads_and_return_types(string source)
        => Module.Value.Accept(source);

    [Theory]
    [InlineData("declare namespace Tools { function run(): void; }")]
    [InlineData("declare namespace Tools { function run<T>(value: T): T; }")]
    [InlineData("function run(): void; function run(): void {}")]
    [InlineData("namespace Tools { export function run(): void {} }")]
    public void Script_declarations_keep_their_existing_function_context(string source)
        => TypeScriptRuntimeFixture.Shared.Value.Accept(source);

    [Theory]
    [InlineData("export declare namespace Tools { function run() {} }")]
    [InlineData("export declare namespace Tools { function run(): void {} }")]
    [InlineData("export declare namespace Tools { function run(): void\n{} }")]
    [InlineData("declare function run(): void\n{} export {};")]
    [InlineData("export declare namespace Tools { function run<T>(value: T): T { return value; } }")]
    [InlineData("export declare namespace Tools { function run(value?: number, next: number): void; }")]
    [InlineData("export declare namespace Tools { function run(...values: number[], next: number): void; }")]
    [InlineData("export declare namespace Tools { function run(value: number = 1): void; }")]
    [InlineData("declare function run(value: number = 1): void; export {};")]
    [InlineData("function run(value: number = 1): void; function run(value: number = 1): void {} export { run };")]
    [InlineData("export declare namespace Tools { function run(value: ): void; }")]
    [InlineData("export declare namespace Tools { function run(): ; }")]
    public void Shared_function_header_rejects_ambient_bodies_and_invalid_signature_parameters(string source)
        => Module.Value.Reject(source);

    [Theory]
    [InlineData("function run(): void { await 1; } export {};")]
    [InlineData("function outer(): void { function inner(): void; function inner(): void {} const value = 1; const value = 2; } export {};")]
    public void Signature_projection_keeps_ordinary_function_context_and_nested_implementation_checks(string source)
        => Module.Value.Reject(source);

    private sealed class ModuleFixture
    {
        private readonly TypeScriptGrammar language = new(TypeScriptInputKind.Module);
        private readonly LALRParser parser;

        internal ModuleFixture() => parser = new LALRParser(language.Grammar, false);

        internal void Accept(string source)
        {
            var parsed = parser.Parsing(language.CreateTokenSource(source));
            Assert.True(parsed.Success, Describe(source, parsed.AllErrors.Select(error => error.Message)));
            Assert.Empty(parsed.AllErrors);
            var diagnostics = language.Validate(parsed.ToParseTree);
            Assert.True(diagnostics.Count == 0, Describe(source,
                diagnostics.Select(diagnostic => diagnostic.Code + ": " + diagnostic.Message)));
        }

        internal void Reject(string source)
        {
            var parsed = parser.Parsing(language.CreateTokenSource(source));
            if (!parsed.Success)
            {
                Assert.True(parser.FiredException == null || parser.FiredException is ParsingException ||
                    parser.FiredException.GetType().Name == "JavaScriptSyntaxException" ||
                    parser.FiredException.GetType().Name == "PredictionFailedException",
                    "An internal failure must not count as a TypeScript syntax error: " + parser.FiredException);
                Assert.NotEmpty(parsed.AllErrors);
                return;
            }
            Assert.NotEmpty(language.Validate(parsed.ToParseTree));
        }

        private string Describe(string source, System.Collections.Generic.IEnumerable<string> messages)
            => source + Environment.NewLine + parser.FiredException + Environment.NewLine +
                string.Join(Environment.NewLine, messages);
    }
}
