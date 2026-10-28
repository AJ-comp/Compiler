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
[Trait("Feature", "TypeScriptAssertionEntryPoints")]
public sealed class TypeScriptAssertionEntryPointTests
{
    private static readonly Lazy<EntryFixture> Expression = new(() => new(TypeScriptInputKind.Expression));
    private static readonly Lazy<EntryFixture> Module = new(() => new(TypeScriptInputKind.Module));
    private static readonly Lazy<EntryFixture> DefaultScript = new(() => new());

    [Theory]
    [InlineData("value! as Box<T> satisfies Named")]
    [InlineData("(value as { property: number })!.property")]
    [InlineData("(value satisfies unknown)!()")]
    [InlineData("value!!")]
    [InlineData("base?.property!.next as Result")]
    public void Expression_entry_accepts_assertions_and_their_runtime_continuations(string source)
        => Expression.Value.Accept(source);

    [Theory]
    [InlineData("value as number!")]
    [InlineData("value!; other")]
    [InlineData("base?.property! = next")]
    public void Expression_entry_keeps_type_target_and_whole_input_restrictions(string source)
        => Expression.Value.Reject(source);

    [Theory]
    [InlineData("export const result = (value as T)!;")]
    [InlineData("export const result = value! satisfies T;")]
    [InlineData("export const result = (await task)! as Result;")]
    [InlineData("export default (value as T)!;")]
    public void Module_entry_accepts_exported_assertions_and_top_level_await(string source)
        => Module.Value.Accept(source);

    [Theory]
    [InlineData("export const result = (await)!;")]
    [InlineData("export const result = value!; with (obj) {}")]
    [InlineData("const result = value!; export { missing };")]
    public void Module_entry_preserves_await_strictness_and_export_checks(string source)
        => Module.Value.Reject(source);

    [Theory]
    [InlineData("\"use strict\" as string; with (obj) { value!; }")]
    [InlineData("\"use strict\" satisfies string; with (obj) { value!; }")]
    [InlineData("\"use strict\"!; with (obj) { value!; }")]
    public void Default_non_strict_script_does_not_turn_asserted_strings_into_directives(string source)
        => DefaultScript.Value.Accept(source);

    [Theory]
    [InlineData("\"use strict\"; with (obj) { value!; }")]
    [InlineData("\"use strict\"; eval! = value;")]
    public void Default_script_still_applies_a_real_strict_directive_after_assertion_support_is_added(string source)
        => DefaultScript.Value.Reject(source);

    private sealed class EntryFixture
    {
        private readonly TypeScriptGrammar language;
        private readonly LALRParser parser;

        internal EntryFixture(TypeScriptInputKind? kind = null)
        {
            // Exercise the public defaults as well as the two explicit entries.
            // Each grammar/parser is built once and reused by this collection.
            language = kind.HasValue ? new TypeScriptGrammar(kind.Value) : new TypeScriptGrammar();
            Assert.Equal(kind ?? TypeScriptInputKind.Script, language.InputKind);
            Assert.Equal(kind == TypeScriptInputKind.Module ? JavaScriptMode.Strict : JavaScriptMode.NonStrict,
                language.Mode);
            parser = new LALRParser(language.Grammar, false);
        }

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
            if (!parsed.Success)
            {
                Assert.True(parser.FiredException == null || parser.FiredException is ParsingException ||
                    parser.FiredException is JavaScriptSyntaxException ||
                    parser.FiredException.GetType().Name == "PredictionFailedException",
                    "An internal failure must not count as a TypeScript syntax error: " + parser.FiredException);
                Assert.NotEmpty(parsed.AllErrors);
                return;
            }
            Assert.NotEmpty(language.Validate(parsed.ToParseTree));
        }
    }
}
