using System;
using System.Linq;
using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers.LR;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptDeclarations")]
public sealed class TypeScriptDeclarationTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("interface Empty {}")]
    [InlineData("interface Box<T> { readonly value: T; optional?: T; run(x: T): T; }")]
    [InlineData("interface Derived<T> extends Base<T>, Other.X<T> {}")]
    [InlineData("interface Derived extends Base\n<string> {}")]
    [InlineData("interface Callable { (x: number): string; new(x: number): Callable; }")]
    [InlineData("interface Store { [key: string]: number; get size(): number; set size(value: number); }")]
    [InlineData("interface RecordLike { [key: `item-${string}`]: number; }")]
    [InlineData("interface NewLines { first: string\nsecond?: number\nrun(): void }")]
    public void Interface_members_and_heritage_reuse_the_type_grammar(string source) => fixture.Accept(source);

    [Theory]
    [InlineData("type Name = string;")]
    [InlineData("type Box<T extends Base = Base> = { value: T };")]
    [InlineData("type Transform<T> = { readonly [K in keyof T as `get${Capitalize<string & K>}`]-?: T[K] };")]
    [InlineData("type Select<T> = T extends infer U ? U : never;")]
    [InlineData("type Tuple = [first: string, second?: number, ...rest: boolean[]];")]
    [InlineData("type Ref = import('lib').Value<string>;")]
    [InlineData("type Name = string\n | number;")]
    [InlineData("type Nested = Map<string, Array<Map<number, Value>>>;")]
    [InlineData("type A = string\ntype B = number")]
    public void Alias_bodies_retain_generics_mapped_types_and_ASI(string source) => fixture.Accept(source);

    [Theory]
    [InlineData("enum Direction {}")]
    [InlineData("enum Direction { Up, Down = 3, Left, }")]
    [InlineData("enum Flag { On = 'on', Off = 'off' }")]
    [InlineData("enum\nDirection { Up }")]
    [InlineData("const enum Flag { On = 1, Off = 0 }")]
    [InlineData("enum Bits { First = 1 << 0, Second = 1 << 1, All = First | Second }")]
    [InlineData("enum Names { default = 1, while = 2, 'literal-name' = 3 }")]
    public void Enum_initializers_use_JavaScript_expressions(string source) => fixture.Accept(source);

    [Theory]
    [InlineData("namespace Tools { const value: number = 1; }")]
    [InlineData("namespace Outer.Inner { export function run(): void {} }")]
    [InlineData("namespace Outer.default { export const x = 1; }")]
    [InlineData("module Tools { export type Name = string; }")]
    [InlineData("declare interface Box<T> { value: T; }")]
    [InlineData("declare type Name = string;")]
    [InlineData("declare enum Flag { On = 1, Off = 0 }")]
    [InlineData("declare const enum Flag { On = 1, Off = 0 }")]
    [InlineData("declare namespace Tools { function run<T>(value: T): T; }")]
    [InlineData("declare module 'external' { export const version: string; }")]
    [InlineData("declare module 'external';")]
    [InlineData("declare function identity<T>(value: T): T;")]
    [InlineData("declare const version: string;")]
    [InlineData("declare let ready: boolean;")]
    [InlineData("declare var count: number;")]
    [InlineData("declare const count = 1;")]
    [InlineData("declare const count = -1;")]
    [InlineData("declare const label = 'ready';")]
    [InlineData("declare const label = `ready`;")]
    [InlineData("declare const ready = true;")]
    [InlineData("declare const count = 1n;")]
    public void Namespace_and_ambient_declarations_preserve_their_context(string source) => fixture.Accept(source);

    [Theory]
    [InlineData("type = string;")]
    [InlineData("const type = 1; const namespace = 2; type + namespace;")]
    [InlineData("const module = { type: 1, declare: 2 }; module.type + module.declare;")]
    [InlineData("function f(type: string, namespace: number) { return type + namespace; }")]
    [InlineData("type\nT = string;")]
    [InlineData("type /*\n*/ T = string;")]
    [InlineData("declare\nvar value: number;")]
    [InlineData("declare /*\n*/ var value: number;")]
    [InlineData("namespace\nTools;")]
    [InlineData("module\nvalue;")]
    public void Contextual_declaration_words_remain_runtime_identifiers(string source) => fixture.Accept(source);

    [Theory]
    [InlineData("interface I {} /ab/.test(value);")]
    [InlineData("type T = { value: string }; /ab/.test(value);")]
    [InlineData("type T = string\n/ab/.test(value);")]
    [InlineData("enum E { A } /ab/.test(value);")]
    [InlineData("namespace N {} /ab/.test(value);")]
    [InlineData("declare namespace N {} /ab/.test(value);")]
    [InlineData("declare function f(): void; /ab/.test(value);")]
    [InlineData("const before = value / divisor; interface I {} /after/.test(value);")]
    public void Declaration_end_restores_the_runtime_RegExp_context(string source) => fixture.Accept(source);

    [Theory]
    [InlineData("interface {}")]
    [InlineData("interface Box<T {}")]
    [InlineData("interface Box extends {}")]
    [InlineData("interface Box extends Base, Other, {}")]
    [InlineData("interface Box { value: ; }")]
    [InlineData("type Name string;")]
    [InlineData("type Name = ;")]
    [InlineData("enum {}")]
    [InlineData("enum E { A = }")]
    [InlineData("enum E { , }")]
    [InlineData("namespace {}")]
    [InlineData("namespace Outer.Inner;")]
    [InlineData("namespace\nTools {}")]
    [InlineData("module\nTools {}")]
    [InlineData("declare function f(value: );")]
    [InlineData("declare module 'external' with { type: 'json' } {}")]
    public void Malformed_or_prerelease_declarations_are_rejected(string source) => fixture.Reject(source);

    [Theory]
    [InlineData("declare function f() {}")]
    [InlineData("declare const value: number = compute();")]
    [InlineData("declare let value: number = 1;")]
    [InlineData("declare namespace N { value(); }")]
    [InlineData("module 'external';")]
    [InlineData("enum E { [key] = 1 }")]
    [InlineData("interface Bad { run(x?: string, y: number): void; }")]
    [InlineData("interface Bad { set value(): void; }")]
    public void Declaration_grammar_restrictions_are_checked_after_parsing(string source) => fixture.Reject(source);
}

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptDeclarations")]
public sealed class TypeScriptModuleDeclarationTests
{
    private static readonly Lazy<ModuleFixture> Shared = new(() => new());

    [Theory]
    [InlineData("export interface Box<T> { value: T; }")]
    [InlineData("export default interface Box<T> { value: T; }")]
    [InlineData("export type Pair<T> = [T, T];")]
    [InlineData("export enum Direction { Up, Down }")]
    [InlineData("export const enum Direction { Up, Down }")]
    [InlineData("export namespace Tools { export type Name = string; }")]
    [InlineData("export declare namespace Tools { function run(): void; }")]
    [InlineData("export declare const version: string;")]
    [InlineData("declare global { interface Window { version: string; } } export {};")]
    [InlineData("namespace Outer { export interface Options { debug: boolean; } export const value: Options = { debug: true }; }")]
    [InlineData("import { value } from 'lib'; export interface Result { value: typeof value; }")]
    [InlineData("export interface I {} /ab/.test(value);")]
    [InlineData("export namespace N {} /ab/.test(value);")]
    public void Module_exports_compose_with_TypeScript_declarations(string source) => Shared.Value.Accept(source);

    [Theory]
    [InlineData("export default interface {}")]
    [InlineData("export interface I extends {}")]
    [InlineData("export type Name = ;")]
    [InlineData("export declare namespace N { function run() {} }")]
    public void Invalid_exported_declarations_are_rejected(string source) => Shared.Value.Reject(source);

    private sealed class ModuleFixture
    {
        private readonly TypeScriptGrammar language = new(TypeScriptInputKind.Module);
        private readonly LALRParser parser;
        internal ModuleFixture() => parser = new LALRParser(language.Grammar, false);
        internal void Accept(string source)
        {
            var parsed = parser.Parsing(language.CreateTokenSource(source));
            Assert.True(parsed.Success, source + Environment.NewLine + parser.FiredException + Environment.NewLine +
                string.Join(Environment.NewLine, parsed.AllErrors.Select(error => error.Message)));
            Assert.Empty(parsed.AllErrors);
            var diagnostics = language.Validate(parsed.ToParseTree);
            Assert.True(diagnostics.Count == 0, source + Environment.NewLine +
                string.Join(Environment.NewLine, diagnostics.Select(diagnostic => diagnostic.Code + ": " + diagnostic.Message)));
        }
        internal void Reject(string source)
        {
            var parsed = parser.Parsing(language.CreateTokenSource(source));
            if (!parsed.Success) { Assert.NotEmpty(parsed.AllErrors); return; }
            Assert.NotEmpty(language.Validate(parsed.ToParseTree));
        }
    }
}
