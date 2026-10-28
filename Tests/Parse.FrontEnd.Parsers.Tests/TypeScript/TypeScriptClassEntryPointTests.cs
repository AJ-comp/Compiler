using System;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Grammars.TypeScript;
using Janglim.FrontEnd.Parsers;
using Janglim.FrontEnd.Parsers.LR;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptClassEntryPoints")]
public sealed class TypeScriptClassEntryPointTests
{
    private static readonly Lazy<Entry> Module = new(() => new(TypeScriptInputKind.Module));
    private static readonly Lazy<Entry> Expression = new(() => new(TypeScriptInputKind.Expression));
    private static readonly Lazy<Entry> NonStrict = new(() => new(TypeScriptInputKind.Script, JavaScriptMode.NonStrict));

    [Theory]
    [InlineData("export abstract class Base { abstract run(): void; }")]
    [InlineData("export default abstract class { abstract run(): void; }")]
    [InlineData("export default abstract class Base<T> implements Named { abstract run(value: T): T; }")]
    [InlineData("export declare abstract class Base { abstract run(): void; }")]
    [InlineData("export class User { constructor(public readonly id: number) {} }")]
    [InlineData("interface I { value: number; } export abstract class C implements I { abstract value: number; }")]
    public void Modules_keep_abstract_and_parameter_property_class_declarations(string source)
        => Module.Value.Check(source, true);

    [Theory]
    [InlineData("export class C { [string: string]: number; }", true)]
    [InlineData("export class C { [await: string]: number; }", false)]
    [InlineData("export class C { [a\\u0077ait: string]: number; }", false)]
    public void Module_index_signatures_keep_their_await_binding_context(string source, bool valid)
        => Module.Value.Check(source, valid);

    [Theory]
    [InlineData("class C { [string: string]: number; }", true)]
    [InlineData("class C { [yield: string]: number; }", false)]
    [InlineData("class C { [eval: string]: number; }", false)]
    public void Index_signatures_remain_strict_inside_a_non_strict_script(string source, bool valid)
        => NonStrict.Value.Check(source, valid);

    [Theory]
    [InlineData("class<T> extends Base<T> implements Named { readonly value?: T; }")]
    [InlineData("(class Local { public value!: number; })")]
    [InlineData("class { readonly #value = 1; read() { return this.#value; } }")]
    [InlineData("(class { constructor(public value: number) {} })")]
    public void Standalone_class_expressions_keep_their_TypeScript_members(string source)
        => Expression.Value.Check(source, true);

    [Theory]
    [InlineData("abstract class C {}")]
    [InlineData("class { constructor(readonly value: number); }")]
    [InlineData("class { readonly run() {} }")]
    public void Expression_entry_does_not_admit_declarations_or_invalid_modifiers(string source)
        => Expression.Value.Check(source, false);

    [Fact]
    public void Non_strict_scripts_keep_class_strictness_and_contextual_identifier_roles()
    {
        var entry = NonStrict.Value;
        entry.Check("var public = 1; class C { public value = 1; } public / 2;", true);
        entry.Check("class C { constructor(public value: number) {} }", true);
        entry.Check("class C { public run() { with (obj) {} } }", false);
    }

    private sealed class Entry
    {
        private readonly TypeScriptGrammar language;
        private readonly LALRParser parser;
        internal Entry(TypeScriptInputKind kind, JavaScriptMode mode = JavaScriptMode.Strict)
        {
            language = new TypeScriptGrammar(kind, mode);
            parser = new LALRParser(language.Grammar, false);
        }

        internal void Check(string source, bool valid)
        {
            var parsed = parser.Parsing(language.CreateTokenSource(source));
            Assert.True(parser.FiredException == null || parser.FiredException is ParsingException ||
                parser.FiredException is JavaScriptSyntaxException ||
                parser.FiredException.GetType().Name == "PredictionFailedException", parser.FiredException?.ToString());
            if (!parsed.Success)
            {
                Assert.False(valid, source + Environment.NewLine + string.Join(Environment.NewLine, parsed.AllErrors.Select(error => error.Message)));
                Assert.NotEmpty(parsed.AllErrors);
                return;
            }
            Assert.Empty(parsed.AllErrors);
            var diagnostics = language.Validate(parsed.ToParseTree);
            Assert.Equal(valid, diagnostics.Count == 0);
        }
    }
}
