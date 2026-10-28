using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMA-262 2026 12.7.2, 14.3.1.1, 14.11.1, 15.1.1 and 15.2-15.8.
// The core profile and explicit webCompatibility host option are tested separately.
// Source is parsed and statically validated; JavaScript bodies are never executed.
[CollectionDefinition(Name)]
public sealed class JavaScriptNonStrictSemanticCollection : ICollectionFixture<JavaScriptNonStrictSemanticFixture>
{
    public const string Name = "JavaScriptNonStrictSemantics";
}

public sealed class JavaScriptNonStrictSemanticFixture
{
    public JavaScriptGrammar Language { get; } = new(JavaScriptMode.NonStrict);
    public LALRParser Parser { get; }
    private JavaScriptGrammar webLanguage;
    private LALRParser webParser;
    private JavaScriptGrammar strictWebLanguage;
    private LALRParser strictWebParser;

    public JavaScriptNonStrictSemanticFixture() => Parser = new LALRParser(Language.Grammar);

    public ParseTreeNonTerminal Parse(string input)
    {
        var result = Parser.Parsing(Language.CreateTokenSource(input));
        Assert.True(result.Success, input + Environment.NewLine + Parser.FiredException + Environment.NewLine +
            string.Join(Environment.NewLine, result.AllErrors.Select(e => e.Code + ": " + e.Message + " [" +
                string.Join(", ", e.ErrTokens.Select(t => t.Input + "@" + t.StartIndex)) + "]")));
        Assert.Empty(result.AllErrors);
        return Assert.IsType<ParseTreeNonTerminal>(result.ToParseTree);
    }

    public ParseTreeNonTerminal Success(string input)
    {
        var tree = Parse(input);
        var diagnostics = Language.Validate(tree);
        Assert.True(diagnostics.Count == 0, input + Environment.NewLine +
            string.Join(Environment.NewLine, diagnostics.Select(d => d.Code + ": " + d.Message)));
        return tree;
    }

    public void Invalid(string input, string code = null)
    {
        var result = Parser.Parsing(Language.CreateTokenSource(input));
        if (!result.Success) return;
        var diagnostics = Language.Validate(result.ToParseTree);
        Assert.NotEmpty(diagnostics);
        if (code != null) Assert.Contains(diagnostics, d => d.Code == code);
    }

    public void WebSuccess(string input, bool strict = false)
    {
        var (language, parser) = Web(strict);
        var result = parser.Parsing(language.CreateTokenSource(input));
        Assert.True(result.Success, input + Environment.NewLine + parser.FiredException);
        var diagnostics = language.Validate(result.ToParseTree);
        Assert.True(diagnostics.Count == 0, input + Environment.NewLine +
            string.Join(Environment.NewLine, diagnostics.Select(d => d.Code + ": " + d.Message)));
    }

    public void WebInvalid(string input, bool strict = false)
    {
        var (language, parser) = Web(strict);
        var result = parser.Parsing(language.CreateTokenSource(input));
        if (!result.Success) return;
        Assert.NotEmpty(language.Validate(result.ToParseTree));
    }

    public IReadOnlyList<JavaScriptDiagnostic> WebDiagnostics(string input)
    {
        var (language, parser) = Web(strict: false);
        var result = parser.Parsing(language.CreateTokenSource(input));
        Assert.True(result.Success, input + Environment.NewLine + parser.FiredException);
        return language.Validate(result.ToParseTree);
    }

    private (JavaScriptGrammar Language, LALRParser Parser) Web(bool strict)
    {
        if (strict)
        {
            if (strictWebLanguage == null)
            {
                strictWebLanguage = new JavaScriptGrammar(JavaScriptMode.Strict, webCompatibility: true);
                strictWebParser = new LALRParser(strictWebLanguage.Grammar);
            }
            return (strictWebLanguage, strictWebParser);
        }
        if (webLanguage == null)
        {
            webLanguage = new JavaScriptGrammar(JavaScriptMode.NonStrict, webCompatibility: true);
            webParser = new LALRParser(webLanguage.Grammar);
        }
        return (webLanguage, webParser);
    }
}

[Collection(JavaScriptNonStrictSemanticCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptNonStrict")]
public sealed class JavaScriptNonStrictSemanticTests
{
    private readonly JavaScriptNonStrictSemanticFixture fixture;
    public JavaScriptNonStrictSemanticTests(JavaScriptNonStrictSemanticFixture fixture) => this.fixture = fixture;

    [Theory]
    [InlineData("var eval; var arguments;")]
    [InlineData("let eval = 1; const arguments = 2;")]
    [InlineData("var {eval, arguments} = value;")]
    [InlineData("let [eval, arguments] = value;")]
    [InlineData("function eval(arguments) { return arguments; }")]
    [InlineData("function arguments(eval) { return eval; }")]
    [InlineData("function f(eval, arguments) { var eval; return arguments; }")]
    [InlineData("function f({eval}, [arguments], ...remaining) { return arguments; }")]
    [InlineData("try {} catch (eval) { arguments = eval; }")]
    [InlineData("try {} catch ({eval, arguments}) { result(eval, arguments); }")]
    [InlineData("for (var eval in object) { arguments = eval; }")]
    [InlineData("for (let arguments of values) { eval = arguments; }")]
    [InlineData("with (object) { value = next; }")]
    [InlineData("function f() { with (object) value(); }")]
    [InlineData("eval => eval")]
    [InlineData("(eval, arguments) => eval")]
    [InlineData("({eval}, [arguments]) => arguments")]
    [InlineData("(...eval) => eval")]
    [InlineData("async (eval, arguments) => await eval")]
    [InlineData("async function f(eval, arguments) { return await eval; }")]
    [InlineData("function* f(eval, arguments) { yield eval; }")]
    [InlineData("async function* f(eval, arguments) { yield await eval; }")]
    [InlineData("({ method(eval, arguments) { return eval; } });")]
    [InlineData("({ get value() { var eval; return eval; }, set value(arguments) {} });")]
    public void Non_strict_bindings_with_and_arrow_parameters_use_their_local_context(string input)
    {
        var tree = fixture.Success(input);
        // The original static entry point keeps its strict-default contract even
        // when the same concrete tree came from the explicit non-strict profile.
        Assert.NotEmpty(JavaScriptExpressionValidation.Validate(tree));
    }

    [Theory]
    [InlineData("implements")]
    [InlineData("interface")]
    [InlineData("package")]
    [InlineData("private")]
    [InlineData("protected")]
    [InlineData("public")]
    [InlineData("static")]
    [InlineData("yield")]
    [InlineData("let")]
    [InlineData("\\u0069mplements")]
    [InlineData("\\u0079ield")]
    [InlineData("\\u006cet")]
    public void Strict_only_reserved_names_can_be_ordinary_non_strict_bindings(string identifier)
    {
        fixture.Success("var " + identifier + " = 1;");
        fixture.Success("function f(" + identifier + ") { return " + identifier + "; }");
        fixture.Success("(" + identifier + ") => " + identifier);
    }

    [Theory]
    [InlineData("function f(a, a) { return a; }")]
    [InlineData("(function(a, a) { return a; });")]
    [InlineData("function f(a, b, a,) { return a; }")]
    [InlineData("function f(a, \\u0061) { return a; }")]
    [InlineData("async function f(a, a) { return await a; }")]
    [InlineData("(async function(a, a) { return await a; });")]
    [InlineData("function* f(a, a) { yield a; }")]
    [InlineData("(function*(a, a) { yield a; });")]
    [InlineData("async function* f(a, a) { yield await a; }")]
    [InlineData("(async function*(a, a) { yield await a; });")]
    public void Every_non_strict_function_definition_can_repeat_a_simple_parameter(string input)
    {
        var tree = fixture.Success(input);
        Assert.Contains(JavaScriptExpressionValidation.Validate(tree), d => d.Code == "duplicate_parameter");
    }

    [Theory]
    [InlineData("function f(a, a = 1) {}")]
    [InlineData("function f(a = 1, a) {}")]
    [InlineData("function f({a}, a) {}")]
    [InlineData("function f([a], a) {}")]
    [InlineData("function f(a, ...a) {}")]
    [InlineData("async function f(a, a = 1) {}")]
    [InlineData("function* f(a, a = 1) {}")]
    [InlineData("async function* f(a, a = 1) {}")]
    [InlineData("(a, a) => a")]
    [InlineData("async (a, a) => a")]
    [InlineData("({method(a, a) {}});")]
    [InlineData("({async method(a, a) {}});")]
    [InlineData("({*method(a, a) {}});")]
    [InlineData("({async *method(a, a) {}});")]
    public void Non_simple_function_parameters_methods_and_arrows_keep_unique_names(string input)
        => fixture.Invalid(input, "duplicate_parameter");

    [Theory]
    [InlineData("'use strict'; var eval;")]
    [InlineData("'use strict'; var arguments;")]
    [InlineData("'use strict'; var implements;")]
    [InlineData("'use strict'; with (object) value();")]
    [InlineData("function f(eval) { 'use strict'; }")]
    [InlineData("function eval() { 'use strict'; }")]
    [InlineData("function f(a, a) { 'use strict'; }")]
    [InlineData("async function f(a, a) { 'use strict'; }")]
    [InlineData("function* f(a, a) { 'use strict'; }")]
    [InlineData("async function* f(a, a) { 'use strict'; }")]
    [InlineData("eval => { 'use strict'; return eval; }")]
    [InlineData("(arguments) => { 'use strict'; return arguments; }")]
    [InlineData("({method(eval) { 'use strict'; }});")]
    [InlineData("function outer() { 'use strict'; function inner(eval) {} }")]
    [InlineData("function outer() { 'use strict'; const inner = arguments => arguments; }")]
    [InlineData("class C { method(eval) {} }")]
    [InlineData("class C { method() { with (object) value(); } }")]
    [InlineData("class C { field = eval => eval; }")]
    [InlineData("class C { static { var arguments; } }")]
    public void Directives_inherited_strictness_and_class_regions_restore_strict_checks(string input)
        => fixture.Invalid(input);

    [Theory]
    [InlineData("function f(a = 1) { 'use strict'; }")]
    [InlineData("function f({a}) { 'use strict'; }")]
    [InlineData("function f(...a) { 'use strict'; }")]
    [InlineData("(a = 1) => { 'use strict'; }")]
    [InlineData("({a}) => { 'use strict'; }")]
    [InlineData("(...a) => { 'use strict'; }")]
    [InlineData("async function f(a = 1) { 'use strict'; }")]
    [InlineData("function* f(a = 1) { 'use strict'; }")]
    [InlineData("async function* f(a = 1) { 'use strict'; }")]
    [InlineData("({method(a = 1) { 'use strict'; }});")]
    public void Local_use_strict_still_rejects_non_simple_parameter_lists(string input)
        => fixture.Invalid(input, "non_simple_strict_directive");

    [Theory]
    [InlineData("let let;")]
    [InlineData("const let = 1;")]
    [InlineData("let {let} = object;")]
    [InlineData("const [let] = values;")]
    [InlineData("let {name: \\u006cet} = object;")]
    [InlineData("for (let let in object) {}")]
    [InlineData("for (const let of values) {}")]
    [InlineData("for (let [let] of values) {}")]
    [InlineData("for (let let = 0; let < 1; let++) {}")]
    public void Lexical_declarations_prohibit_binding_let_in_every_mode(string input)
        => fixture.Invalid(input, "invalid_binding_identifier");

    [Theory]
    [InlineData("var let; function f(let) { return let; }")]
    [InlineData("function f(a, a) { ('use strict'); return a; }")]
    [InlineData("function f(a, a) { 0; 'use strict'; return a; }")]
    [InlineData("function f(a, a) { { 'use strict'; } return a; }")]
    [InlineData("function f(a, a) { function inner() { 'use strict'; } return a; }")]
    [InlineData("function f(a, a) { 'use\\x20strict'; return a; }")]
    [InlineData("function f(a, a) { 'first'; 'second'; return a; }")]
    [InlineData("function strict() { 'use strict'; } function sloppy(eval) { return eval; }")]
    [InlineData("({[function(eval) { return eval; }]: function() { 'use strict'; }});")]
    [InlineData("({[function(eval) { return eval; }]() { 'use strict'; }});")]
    public void Non_directives_siblings_and_computed_method_names_keep_outer_strictness(string input)
        => fixture.Success(input);

    [Theory]
    [InlineData("function f(a) { let a; }")]
    [InlineData("(a) => { let a; }")]
    [InlineData("let value; var value;")]
    [InlineData("function f() { let value; var value; }")]
    [InlineData("{ let value; let value; }")]
    [InlineData("try {} catch ({value, other: value}) {}")]
    [InlineData("try {} catch (value) { let value; }")]
    [InlineData("return value;")]
    [InlineData("break;")]
    [InlineData("continue;")]
    [InlineData("async function f(await) {}")]
    [InlineData("async (await) => await")]
    [InlineData("function* f(yield) {}")]
    [InlineData("function* f() { const inner = (yield) => yield; }")]
    public void Core_scope_flow_await_and_yield_rules_are_not_relaxed(string input)
        => fixture.Invalid(input);

    [Fact]
    public void Alternating_validation_contexts_does_not_mutate_the_tree_or_leak_strictness()
    {
        const string input = "function f(eval, eval) { return eval; }";
        var tree = fixture.Success(input);
        var tokens = tree.AllTokens.ToArray();
        string text = tree.AllInputDatas;
        Assert.NotEmpty(JavaScriptExpressionValidation.Validate(tree));
        Assert.Empty(fixture.Language.Validate(tree));
        fixture.Invalid("function f(eval) { 'use strict'; }");
        fixture.Success(input);
        Assert.Empty(fixture.Language.Validate(tree));
        Assert.Equal(text, tree.AllInputDatas);
        Assert.Equal(tokens, tree.AllTokens.ToArray());
    }

    [Theory]
    [InlineData("{ function f() {} function f() {} }")]
    [InlineData("{ function f() {} function f() {} function f() {} }")]
    [InlineData("function outer() { { function f() {} function f() {} } }")]
    [InlineData("switch (value) { case 0: function f() {} break; case 1: function f() {} }")]
    [InlineData("switch (value) { default: function f() {} function f() {} }")]
    public void Web_compatibility_allows_only_non_strict_ordinary_block_function_duplicates(string input)
    {
        fixture.Invalid(input, "duplicate_binding");
        fixture.WebSuccess(input);
        fixture.WebInvalid(input, strict: true);
        fixture.WebInvalid("'use strict'; " + input);
    }

    [Theory]
    [InlineData("{ function f() {} let f; }")]
    [InlineData("{ let f; function f() {} }")]
    [InlineData("{ function f() {} class f {} }")]
    [InlineData("{ function f() {} var f; }")]
    [InlineData("{ async function f() {} async function f() {} }")]
    [InlineData("{ function* f() {} function* f() {} }")]
    [InlineData("{ function f() {} async function f() {} }")]
    [InlineData("{ async function f() {} function f() {} }")]
    [InlineData("{ function f() {} function f() {} let f; }")]
    [InlineData("switch (value) { case 0: function f() {} break; default: let f; }")]
    [InlineData("switch (value) { case 0: function f() {} break; default: var f; }")]
    public void Web_compatibility_preserves_other_block_and_switch_binding_collisions(string input)
        => fixture.WebInvalid(input);

    [Theory]
    [InlineData("try {} catch (error) { var error; }")]
    [InlineData("try {} catch (error) { var error = value; }")]
    [InlineData("try {} catch (error) { { var error; } }")]
    [InlineData("try {} catch (\\u0065rror) { var error; }")]
    public void The_web_catch_var_exception_is_independent_of_strict_mode(string input)
    {
        fixture.Invalid(input, "conflicting_declaration");
        fixture.WebSuccess(input);
        fixture.WebSuccess(input, strict: true);
        fixture.WebSuccess("'use strict'; " + input);
    }

    [Theory]
    [InlineData("try {} catch ({error}) { var error; }")]
    [InlineData("try {} catch ([error]) { var error; }")]
    [InlineData("try {} catch (error) { let error; }")]
    [InlineData("try {} catch (error) { const error = 1; }")]
    [InlineData("try {} catch (error) { function error() {} }")]
    public void The_web_catch_exception_does_not_relax_patterns_or_lexical_collisions(string input)
    {
        fixture.WebInvalid(input);
        fixture.WebInvalid(input, strict: true);
    }

    [Theory]
    [InlineData("if (condition) function f() {}")]
    [InlineData("if (condition) function f() {} else function g() {}")]
    [InlineData("if (condition) value(); else function f() {}")]
    [InlineData("if (first) if (second) function f() {} else function g() {}")]
    [InlineData("if (condition) function f() { 'use strict'; }")]
    [InlineData("label: function f() {}")]
    [InlineData("first: second: function f() {}")]
    [InlineData("function outer() { label: function f() {} }")]
    public void Web_legacy_if_and_label_functions_require_an_explicit_non_strict_host_option(string input)
    {
        fixture.Invalid(input);
        fixture.WebSuccess(input);
        fixture.WebInvalid(input, strict: true);
        fixture.WebInvalid("'use strict'; " + input);
    }

    [Theory]
    [InlineData("if (condition) label: function f() {}")]
    [InlineData("if (condition) value(); else label: function f() {}")]
    [InlineData("while (condition) label: function f() {}")]
    [InlineData("do label: function f() {} while (condition);")]
    [InlineData("for (;;) label: function f() {}")]
    [InlineData("for (var key in object) label: function f() {}")]
    [InlineData("for (var value of values) label: function f() {}")]
    [InlineData("with (object) label: function f() {}")]
    [InlineData("if (condition) first: second: function f() {}")]
    [InlineData("while (condition) first: second: function f() {}")]
    public void Labelled_functions_are_not_if_iteration_or_with_statement_bodies(string input)
        => fixture.WebInvalid(input);

    [Theory]
    [InlineData("if (condition) { label: function f() {} }")]
    [InlineData("while (condition) { label: function f() {} break; }")]
    [InlineData("with (object) { label: function f() {} }")]
    [InlineData("for (var key = 0 in object) { break; }")]
    [InlineData("outer: for (var key = 0 in object) { continue outer; }")]
    public void An_explicit_block_or_real_loop_preserves_valid_label_and_flow_context(string input)
        => fixture.WebSuccess(input);

    [Theory]
    [InlineData("let f; label: function f() {}")]
    [InlineData("const f = 1; label: function f() {}")]
    [InlineData("class f {} label: function f() {}")]
    [InlineData("label: function f() {} let f;")]
    [InlineData("let f; first: second: function f() {}")]
    [InlineData("let f; first: second: third: function \\u0066() {}")]
    [InlineData("{ let f; label: function f() {} }")]
    [InlineData("{ label: function f() {} let f; }")]
    [InlineData("{ let f; first: second: function f() {} }")]
    [InlineData("{ var f; label: function f() {} }")]
    [InlineData("function outer() { let f; label: function f() {} }")]
    [InlineData("function outer() { let f; first: second: function f() {} }")]
    [InlineData("try {} catch (f) { label: function f() {} }")]
    [InlineData("switch (value) { case 0: let f; break; default: label: function f() {} }")]
    public void Direct_labelled_function_names_participate_in_the_containing_declaration_scope(string input)
        => Assert.Contains(fixture.WebDiagnostics(input), diagnostic =>
            diagnostic.Code == "duplicate_binding" || diagnostic.Code == "conflicting_declaration");

    [Theory]
    [InlineData("var f; label: function f() {}")]
    [InlineData("function outer(f) { label: function f() {} }")]
    [InlineData("function outer() { var f; label: function f() {} }")]
    [InlineData("let f; { label: function f() {} }")]
    [InlineData("{ let f; { label: function f() {} } }")]
    [InlineData("let inner; label: function f() { var inner; }")]
    [InlineData("let parameter; label: function f(parameter) { return parameter; }")]
    [InlineData("{ function f() {} label: function f() {} }")]
    [InlineData("{ first: function f() {} second: function f() {} }")]
    [InlineData("{ first: second: function f() {} third: fourth: function f() {} }")]
    [InlineData("switch (value) { case 0: first: function f() {} break; default: second: function f() {} }")]
    public void Label_chains_preserve_var_classification_block_shadowing_and_ordinary_function_duplicates(string input)
        => fixture.WebSuccess(input);

    [Theory]
    [InlineData("let key; for (var key = 0 in object);")]
    [InlineData("{ let key; for (var key = 0 in object); }")]
    [InlineData("function outer() { let key; for (var key = 0 in object); }")]
    [InlineData("let key; { for (var key = 0 in object); }")]
    [InlineData("let key; label: for (var key = 0 in object);")]
    [InlineData("for (var key = 0 in object); let key;")]
    [InlineData("let body; for (var key = 0 in object) { var body; }")]
    [InlineData("{ let body; for (var key = 0 in object) { var body; } }")]
    [InlineData("function outer() { let body; for (var key = 0 in object) { var body; } }")]
    [InlineData("try {} catch ({key}) { for (var key = 0 in object); }")]
    public void Legacy_for_in_headers_and_body_vars_share_the_enclosing_var_scope(string input)
        => Assert.Contains(fixture.WebDiagnostics(input), diagnostic => diagnostic.Code == "conflicting_declaration");

    [Theory]
    [InlineData("var key; for (var key = 0 in object);")]
    [InlineData("let inner; for (var key = function() { var inner; } in object);")]
    [InlineData("let key; function inner() { for (var key = 0 in object); }")]
    [InlineData("let body; for (var key = 0 in object) { let body; }")]
    [InlineData("let body; for (var key = 0 in object) { function inner() { var body; } }")]
    [InlineData("try {} catch (key) { for (var key = 0 in object); }")]
    public void Legacy_for_in_collection_stops_at_functions_and_keeps_lexical_body_names_local(string input)
        => fixture.WebSuccess(input);
}
