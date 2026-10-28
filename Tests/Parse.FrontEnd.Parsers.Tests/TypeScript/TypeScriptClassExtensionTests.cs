using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

// Syntax cases are compared with the pinned Microsoft TypeScript 7.0.2 parser
// and grammar checks. Unresolved type names and assignability are intentionally
// outside the language grammar's acceptance criteria.
[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptClassExtensions")]
public sealed class TypeScriptClassExtensionTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("class C { public value: number; }")]
    [InlineData("class C { private value = 1; }")]
    [InlineData("class C { protected value?: string; }")]
    [InlineData("class C { readonly value: number = 1; }")]
    [InlineData("class C { public readonly value: number = 1; }")]
    [InlineData("class C { protected static readonly value: number = 1; }")]
    [InlineData("class C { value!: number; }")]
    [InlineData("class C { private value!: Box<User>; }")]
    [InlineData("class C { private readonly [\"value\"]: number = 1; }")]
    [InlineData("class C { readonly #value: number = 1; }")]
    [InlineData("class C { declare value: number; }")]
    [InlineData("class C { declare readonly value: number; }")]
    [InlineData("class C { accessor value: number = 1; }")]
    [InlineData("class C { public static accessor value = 1; }")]
    [InlineData("class C { accessor #value = 1; }")]
    [InlineData("class C { [key: string]: unknown; }")]
    [InlineData("class C { static readonly [key: string]: unknown; }")]
    [InlineData("class C { value?: number = 1; }")]
    public void Accepts_accessibility_readonly_optional_definite_and_accessor_fields(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("class C { public run(value: number): number { return value; } }")]
    [InlineData("class C { private run<T>(value: T): T { return value; } }")]
    [InlineData("class C { protected static run(): void {} }")]
    [InlineData("class C extends B { override run(): void {} }")]
    [InlineData("class C extends B { public override run(): void {} }")]
    [InlineData("class C extends B { public static override run(): void {} }")]
    [InlineData("class C { public async run(): Promise<void> { await work(); } }")]
    [InlineData("class C { protected *run(): Iterable<number> { yield 1; } }")]
    [InlineData("class C { private async *run(): AsyncIterable<number> { yield await work(); } }")]
    [InlineData("class C { public get value(): number { return 1; } public set value(next: number) {} }")]
    [InlineData("class C { protected static get value(): number { return 1; } }")]
    [InlineData("class C { run?(): void {} }")]
    [InlineData("class C { public run?(): void {} }")]
    [InlineData("class C { public constructor() {} }")]
    [InlineData("class C { protected constructor() {} }")]
    [InlineData("class C { private constructor(value: number) {} }")]
    public void Accepts_typed_methods_accessors_and_constructor_visibility(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("abstract class C {}")]
    [InlineData("abstract class C<T> {}")]
    [InlineData("abstract class C { abstract value: number; abstract run(): void; }")]
    [InlineData("abstract class C { public abstract run<T>(value: T): T; }")]
    [InlineData("abstract class C { protected abstract readonly value: number; }")]
    [InlineData("abstract class C { abstract get value(): number; abstract set value(next: number); }")]
    [InlineData("abstract class C extends B { abstract override run(): void; }")]
    [InlineData("class C implements Named {}")]
    [InlineData("class C implements Named, Printable<number> {}")]
    [InlineData("class C<T extends Named = Named> extends Base<T> implements Named, Printable<T> {}")]
    [InlineData("const C = class<T> extends Base<T> implements Named {};")]
    [InlineData("const C = class Local<T> implements Named {};")]
    [InlineData("class C extends Base<T> {}")]
    [InlineData("class C extends ns.Base<Box<T>> {}")]
    [InlineData("class C extends (factory()) implements Named {}")]
    [InlineData("class C<T> { run<U extends T>(value: U): U { return value; } }")]
    public void Accepts_abstract_classes_implements_and_generic_heritage(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("class C { constructor(public value: number) {} }")]
    [InlineData("class C { constructor(private value: number) {} }")]
    [InlineData("class C { constructor(protected value: number) {} }")]
    [InlineData("class C { constructor(readonly value: number) {} }")]
    [InlineData("class C { constructor(public readonly value: number) {} }")]
    [InlineData("class C { constructor(private readonly value = 1) {} }")]
    [InlineData("class C { constructor(protected readonly value?: string) {} }")]
    [InlineData("class C { constructor(public value: number = 1, private name?: string) {} }")]
    [InlineData("class C { constructor(public value: Box<User>) { this.value = value; } }")]
    [InlineData("class C extends B { constructor(public override value: number) { super(); } }")]
    [InlineData("class C extends B { constructor(public override readonly value: number) { super(); } }")]
    [InlineData("class C { constructor(public value = /ab/.test(text)) {} }")]
    [InlineData("class C { constructor(public value = () => 1) {} }")]
    [InlineData("class C { constructor(readonly value = function() { return 1; }) {} }")]
    public void Accepts_constructor_parameter_properties_without_type_checking(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("class C { run(value: string): string; run(value: number): number; run(value: unknown): unknown { return value; } }")]
    [InlineData("class C { constructor(value: string); constructor(value: number); constructor(value: unknown) {} }")]
    [InlineData("class C { constructor(value: number); constructor(public value: number) {} }")]
    [InlineData("class C { public run(value: string): string; public run(value: unknown): unknown { return value; } }")]
    [InlineData("class C { static run(value: string): string; static run(value: unknown): unknown { return value; } }")]
    [InlineData("declare class C { value: number; readonly name: string; run(value: number): void; constructor(value: number); }")]
    [InlineData("declare abstract class C { abstract run(): void; }")]
    [InlineData("declare class C<T> extends Base<T> implements Named { protected value: T; }")]
    [InlineData("declare class C { get value(): number; set value(next: number); }")]
    [InlineData("declare class C { readonly value = 1; }")]
    [InlineData("declare class C { private readonly value = \"ready\"; }")]
    [InlineData("declare class C { private constructor(); }")]
    public void Accepts_overload_and_ambient_signatures(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("class C { public() {} private() {} protected() {} }")]
    [InlineData("class C { readonly() {} abstract() {} override() {} }")]
    [InlineData("class C { declare() {} accessor() {} }")]
    [InlineData("class C { public: number; private: number; protected: number; }")]
    [InlineData("class C { readonly = 1; abstract = 2; override = 3; }")]
    [InlineData("class C { declare = 1; accessor = 2; }")]
    [InlineData("class C { readonly; value: number; }")]
    [InlineData("class C { public\nvalue: number; }")]
    [InlineData("class C { readonly\nvalue: number; }")]
    [InlineData("class C { static public() {} }")]
    [InlineData("class C { public static() {} }")]
    [InlineData("class C { get public(): number { return 1; } }")]
    [InlineData("class C { public /* comment */ readonly value: number; }")]
    [InlineData("abstract\nclass C {}")]
    public void Modifier_spellings_keep_their_name_and_line_break_roles(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("class C { public run() { return /ab/.test(text); } } const result = value / 2;")]
    [InlineData("class C { public value: Box<T> = left > right; }")]
    [InlineData("class C { public value = { count: 1 }; } /ab/.test(text);")]
    [InlineData("class C { #value = 1; readonly other = this.#value; public get() { return this.#value; } }")]
    public void Class_extensions_preserve_runtime_lexical_and_private_name_contexts_valid(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("class C { public #method() {} }")]
    public void Class_extensions_preserve_runtime_lexical_and_private_name_contexts_invalid(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("class C { public private value: number; }")]
    [InlineData("class C { public public value: number; }")]
    [InlineData("class C { static static value: number; }")]
    [InlineData("class C { readonly readonly value: number; }")]
    [InlineData("class C extends B { override override value: number; }")]
    [InlineData("class C { static public value: number; }")]
    [InlineData("class C { readonly public value: number; }")]
    [InlineData("class C { readonly static value: number; }")]
    [InlineData("class C extends B { override public value: number; }")]
    [InlineData("class C extends B { readonly override value: number; }")]
    [InlineData("class C extends B { override static value: number; }")]
    [InlineData("class C extends B { async override run() {} }")]
    [InlineData("class C { readonly run(): void {} }")]
    [InlineData("class C { readonly get value(): number { return 1; } }")]
    [InlineData("class C { public #value: number; }")]
    [InlineData("class C { private #run() {} }")]
    [InlineData("class C { declare #value: number; }")]
    [InlineData("class C extends B { declare override value: number; }")]
    [InlineData("class C { readonly accessor value = 1; }")]
    [InlineData("class C { declare accessor value: number; }")]
    [InlineData("class C { accessor run() {} }")]
    [InlineData("class C { accessor value?: number; }")]
    public void Rejects_duplicate_misordered_and_inapplicable_modifiers(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("class C { abstract value: number; }")]
    [InlineData("class C { abstract run(): void; }")]
    [InlineData("abstract class C { abstract abstract run(): void; }")]
    [InlineData("abstract class C { private abstract run(): void; }")]
    [InlineData("abstract class C { abstract private value: number; }")]
    [InlineData("abstract class C { static abstract run(): void; }")]
    [InlineData("abstract class C { abstract static value: number; }")]
    [InlineData("abstract class C { async abstract run(): void; }")]
    [InlineData("abstract class C { abstract run(): void {} }")]
    [InlineData("abstract class C { abstract value: number = 1; }")]
    [InlineData("abstract class C { abstract #value: number; }")]
    [InlineData("const C = abstract class {};")]
    public void Rejects_invalid_abstract_member_contexts_and_implementations(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("class C { value!; }")]
    [InlineData("class C { value!: number = 1; }")]
    [InlineData("class C { static value!: number; }")]
    [InlineData("abstract class C { abstract value!: number; }")]
    [InlineData("declare class C { value!: number; }")]
    [InlineData("class C { declare value!: number; }")]
    [InlineData("class C { value?!: number; }")]
    [InlineData("class C { value!?: number; }")]
    [InlineData("class C { [key: string]; }")]
    [InlineData("class C { public [key: string]: unknown; }")]
    [InlineData("class C { [key?: string]: unknown; }")]
    [InlineData("class C { constructor: number; }")]
    public void Rejects_invalid_definite_optional_and_index_fields(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("class C { constructor(public private value: number) {} }")]
    [InlineData("class C { constructor(readonly public value: number) {} }")]
    [InlineData("class C { constructor(public ...values: number[]) {} }")]
    [InlineData("class C { constructor(readonly { value }: { value: number }) {} }")]
    [InlineData("class C { constructor(public [value]: number[]) {} }")]
    [InlineData("class C { constructor(public constructor: number) {} }")]
    [InlineData("class C { run(public value: number) {} }")]
    [InlineData("function f(readonly value: number) {}")]
    [InlineData("class C { constructor(public this: C) {} }")]
    [InlineData("class C { constructor(static value: number) {} }")]
    [InlineData("class C { constructor(abstract value: number) {} }")]
    [InlineData("class C { constructor(public value?: number, next: number) {} }")]
    [InlineData("declare class C { constructor(public value: number); }")]
    [InlineData("class C { constructor(public value: number); constructor(value: number) {} }")]
    public void Rejects_invalid_parameter_properties(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("class C { constructor<T>() {} }")]
    [InlineData("class C { constructor(): void {} }")]
    [InlineData("class C { readonly constructor() {} }")]
    [InlineData("class C { override constructor() {} }")]
    [InlineData("class C { declare run(): void; }")]
    [InlineData("declare class C { run(): void {} }")]
    [InlineData("declare class C { value: number = 1; }")]
    [InlineData("declare class C { readonly value: number = 1; }")]
    [InlineData("declare class C { async run(): Promise<void>; }")]
    [InlineData("class C { get value?(): number { return 1; } }")]
    [InlineData("class C { set value(next?: number) {} }")]
    public void Rejects_invalid_constructor_accessor_and_ambient_signatures(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("class C implements {}")]
    [InlineData("class C implements A, {}")]
    [InlineData("class C extends A, B {}")]
    [InlineData("class C extends A extends B {}")]
    [InlineData("class C implements A implements B {}")]
    [InlineData("class C implements A extends B {}")]
    [InlineData("class C extends Base<> {}")]
    [InlineData("class C implements Named<> {}")]
    public void Rejects_invalid_extends_and_implements_lists(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("class C { public run() { return missing; } private #value = 1; }")]
    [InlineData("class C { public run() { super(); } }")]
    [InlineData("class C { public value = arguments; }")]
    [InlineData("class C { public run() { return this.#missing; } }")]
    [InlineData("class C { public run() { let name; let name; } }")]
    [InlineData("class C { public run(value: number, value: number) {} }")]
    public void Class_extensions_do_not_hide_existing_JavaScript_early_errors(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("class C { #x?: number }")]
    [InlineData("class C { #m?(): void; }")]
    public void Private_fields_and_signature_methods_can_be_optional(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("class C { constructor?() {} }")]
    public void Constructors_cannot_be_optional(string source)
        => fixture.Reject(source);

    [Fact]
    public void Validation_preserves_the_original_class_syntax_tokens_positions_and_parent_links()
    {
        const string source = "interface Named { id: number; } " +
            "abstract class Base<T> { protected abstract run(value: T): T; } " +
            "class Model extends Base<number> implements Named { " +
            "constructor(public readonly id: number) { super(); } " +
            "protected override run(value: number): number { return value; } }";
        var parsed = fixture.Parser.Parsing(fixture.Language.CreateTokenSource(source));
        Assert.True(parsed.Success, source + Environment.NewLine + fixture.Parser.FiredException + Environment.NewLine +
            string.Join(Environment.NewLine, parsed.AllErrors.Select(error => error.Message)));
        Assert.Empty(parsed.AllErrors);
        var tree = Assert.IsType<ParseTreeNonTerminal>(parsed.ToParseTree);
        var nodes = Descendants(tree).ToArray();
        var parents = nodes.Select(node => node.Parent).ToArray();
        var tokens = tree.AllTokens.ToArray();

        Assert.Empty(fixture.Language.Validate(tree));
        Assert.Empty(fixture.Language.Validate(tree));

        Assert.Equal(tokens, tree.AllTokens);
        Assert.Contains(tokens, token => token.Input == "implements");
        Assert.Contains(tokens, token => token.Input == "readonly");
        Assert.Contains(tokens, token => token.Input == "override");
        for (int i = 0; i < nodes.Length; i++) Assert.Same(parents[i], nodes[i].Parent);
        foreach (var token in tokens)
            Assert.Equal(token.Input, source.Substring(token.StartIndex, token.EndIndex - token.StartIndex + 1));
    }

    private static IEnumerable<ParseTreeSymbol> Descendants(ParseTreeSymbol root)
    {
        var pending = new Stack<ParseTreeSymbol>();
        pending.Push(root);
        while (pending.Count != 0)
        {
            var symbol = pending.Pop();
            yield return symbol;
            if (symbol is not ParseTreeNonTerminal node) continue;
            foreach (var child in node.Items) pending.Push(child);
        }
    }
}
