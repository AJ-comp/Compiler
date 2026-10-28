using Janglim.FrontEnd.Grammars.JavaScript;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

[Collection(JavaScriptGrammarCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptStaticConformance")]
public sealed class JavaScriptStaticConformanceTests
{
    private readonly JavaScriptGrammarFixture fixture;
    public JavaScriptStaticConformanceTests(JavaScriptGrammarFixture fixture) => this.fixture = fixture;

    [Fact]
    public void Switch_lexical_names_conflict_with_vars_through_finally_and_loop_bodies()
    {
        // ECMA-262 2026 14.12.1: all CaseBlock clauses share one lexical scope;
        // VarDeclaredNames traverses statement bodies but stops at a nested function.
        fixture.Reject(JavaScriptInputKind.Script,
            "switch (key) { case 0: let value; break; default: try {} finally { for (;;) { var value; } } }");
        fixture.Accept(JavaScriptInputKind.Script,
            "switch (key) { case 0: let value; break; default: try {} finally { function inner() { for (;;) { var value; } } } }");
    }

    [Fact]
    public void Destructured_catch_bindings_see_nested_vars_but_not_nested_function_vars()
    {
        // 14.15.1: the Annex B simple-identifier exception does not cover patterns.
        fixture.Reject(JavaScriptInputKind.Script,
            "try {} catch ({error}) { try {} finally { if (flag) { var error; } } }",
            JavaScriptMode.NonStrict, web: true);
        fixture.Accept(JavaScriptInputKind.Script,
            "try {} catch ({error}) { try {} finally { function inner() { var error; } } }",
            JavaScriptMode.NonStrict, web: true);
    }

    [Fact]
    public void Arrow_bound_names_exclude_computed_keys_and_nested_initializer_parameters()
    {
        // 15.3.1 and 8.2.1: only the parameter's bindings contribute BoundNames.
        fixture.Accept(JavaScriptInputKind.Script,
            "({[label]: value = function(value) {}}, label) => value");
        fixture.Reject(JavaScriptInputKind.Script,
            "({[label]: value = function(label) {}}, value) => value");
    }

    [Fact]
    public void Setter_destructuring_checks_bound_names_without_counting_computed_references()
    {
        // 15.4.1: a setter has one formal parameter, but every bound name within
        // that parameter's destructuring pattern still participates in uniqueness.
        fixture.Accept(JavaScriptInputKind.Script,
            "({ set item({[value]: value}) {} });");
        fixture.Reject(JavaScriptInputKind.Script,
            "({ set item({first: value, second: value}) {} });");
    }

    [Fact]
    public void Continue_resolves_iteration_labels_through_switch_without_crossing_function_boundaries()
    {
        // 14.8.1 and 8.2.5: a switch does not hide an outer iteration label;
        // a nested function starts its own set of available continue targets.
        fixture.Accept(JavaScriptInputKind.Script,
            "outer: inner: for (;;) { switch (key) { case 0: continue outer; default: continue inner; } }");
        fixture.Reject(JavaScriptInputKind.Script,
            "outer: inner: for (;;) { switch (key) { default: function nested() { continue outer; } } }");
    }

    [Fact]
    public void Lexical_for_binding_conflicts_include_vars_under_nested_catch_and_finally()
    {
        // 14.7.5.1: a lexical ForDeclaration is compared with VarDeclaredNames
        // of its entire Statement, including nested catch/finally statements.
        fixture.Reject(JavaScriptInputKind.Script,
            "for (const value of items) { try {} catch (error) {} finally { var value; } }");
        fixture.Accept(JavaScriptInputKind.Script,
            "for (const value of items) { try {} catch (error) {} finally { function inner() { var value; } } }");
    }

    [Fact]
    public void Export_name_uniqueness_compares_decoded_supplementary_unicode_characters()
    {
        // 16.2.1.1: ExportedNames compares decoded strings, so a code-point
        // escape and the equivalent UTF-16 surrogate-pair spelling collide.
        fixture.Reject(JavaScriptInputKind.Module,
            "const value = 0; export {value as '\\u{1F47B}'}; export {value as '\\uD83D\\uDC7B'};");
        fixture.Accept(JavaScriptInputKind.Module,
            "const value = 0; export {value as '\\u{1F47B}'}; export {value as '\\u{1F47C}'};");
    }

    [Fact]
    public void Import_alias_bindings_use_local_names_separately_from_imported_names()
    {
        // 16.2.2.1/16.2.1.1: the imported name can repeat; local BoundNames
        // cannot. This also applies when the imported names are string literals.
        fixture.Accept(JavaScriptInputKind.Module,
            "import {'name' as first, name as second} from 'source'; export {first, second};");
        fixture.Reject(JavaScriptInputKind.Module,
            "import {'first' as local, second as local} from 'source'; export {local};");
    }
}
