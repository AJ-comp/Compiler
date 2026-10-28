using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMA-262 2026 Annex B.1.1, B.3.1-B.3.5 and B.3.9.
// The browser option and strictness are independent. In particular HTML comments
// and the simple catch/var exception are not restricted to non-strict Script.
// These tests only parse and validate source: even accepted call targets such as
// f() = value are never evaluated, so their runtime ReferenceError is irrelevant.
// https://tc39.es/ecma262/2026/multipage/additional-ecmascript-features-for-web-browsers.html

[CollectionDefinition(Name)]
public sealed class JavaScriptNonStrictBrowserCollection : ICollectionFixture<JavaScriptNonStrictBrowserFixture>
{
    public const string Name = "ECMAScript optional browser syntax";
}

public sealed class JavaScriptNonStrictBrowserFixture
{
    private readonly Lazy<JavaScriptBrowserHost> browser = new(() => JavaScriptBrowserHost.Script(JavaScriptMode.NonStrict, true));
    private readonly Lazy<JavaScriptBrowserHost> core = new(() => JavaScriptBrowserHost.Script(JavaScriptMode.NonStrict, false));
    private readonly Lazy<JavaScriptBrowserHost> strict = new(() => JavaScriptBrowserHost.Script(JavaScriptMode.Strict, true));
    private readonly Lazy<JavaScriptBrowserHost> module = new(() => JavaScriptBrowserHost.Module(true));
    private readonly Lazy<JavaScriptBrowserHost> coreModule = new(() => JavaScriptBrowserHost.Module(false));
    private readonly Lazy<JavaScriptBrowserHost> expression = new(() => JavaScriptBrowserHost.Expression(true));
    private readonly Lazy<JavaScriptBrowserHost> coreExpression = new(() => JavaScriptBrowserHost.Expression(false));

    // Grammar construction remains sequential. Each table is built only when a
    // case first needs that source goal and is reused by the collection thereafter.
    internal JavaScriptBrowserHost Browser => browser.Value;
    internal JavaScriptBrowserHost Core => core.Value;
    internal JavaScriptBrowserHost Strict => strict.Value;
    internal JavaScriptBrowserHost Module => module.Value;
    internal JavaScriptBrowserHost CoreModule => coreModule.Value;
    internal JavaScriptBrowserHost Expression => expression.Value;
    internal JavaScriptBrowserHost CoreExpression => coreExpression.Value;
}

internal sealed class JavaScriptBrowserHost
{
    private readonly Func<string, ITokenSource> source;
    private readonly Func<ParseTreeSymbol, IReadOnlyList<JavaScriptDiagnostic>> validate;
    private readonly LALRParser parser;

    private JavaScriptBrowserHost(EbnfGrammar grammar, Func<string, ITokenSource> source,
        Func<ParseTreeSymbol, IReadOnlyList<JavaScriptDiagnostic>> validate)
    {
        this.source = source;
        this.validate = validate;
        parser = new LALRParser(grammar, false);
    }

    internal static JavaScriptBrowserHost Script(JavaScriptMode mode, bool webCompatibility)
    {
        var language = new JavaScriptGrammar(mode, webCompatibility: webCompatibility);
        return new JavaScriptBrowserHost(language.Grammar, language.CreateTokenSource, language.Validate);
    }

    internal static JavaScriptBrowserHost Module(bool webCompatibility)
    {
        var language = new JavaScriptGrammar(webCompatibility: webCompatibility, inputKind: JavaScriptInputKind.Module);
        return new JavaScriptBrowserHost(language.Grammar, language.CreateTokenSource, language.Validate);
    }

    internal static JavaScriptBrowserHost Expression(bool webCompatibility)
    {
        var language = new JavaScriptGrammar(JavaScriptMode.NonStrict, webCompatibility: webCompatibility,
            inputKind: JavaScriptInputKind.Expression);
        return new JavaScriptBrowserHost(language.Grammar, language.CreateTokenSource, language.Validate);
    }

    internal ParseTreeSymbol Success(string input)
    {
        var parsed = parser.Parsing(source(input));
        Assert.True(parsed.Success, input + Environment.NewLine + parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var tree = parsed.ToParseTree;
        Assert.NotNull(tree);
        var diagnostics = validate(tree);
        Assert.True(diagnostics.Count == 0, input + Environment.NewLine +
            string.Join(Environment.NewLine, diagnostics.Select(d => d.Code + ": " + d.Message)));
        return tree;
    }

    internal void Invalid(string input)
    {
        var parsed = parser.Parsing(source(input));
        if (!parsed.Success) return;
        var tree = parsed.ToParseTree;
        Assert.NotNull(tree);
        Assert.True(validate(tree).Count != 0, "Unexpectedly accepted: " + input);
    }

    internal IReadOnlyList<JavaScriptDiagnostic> Validate(ParseTreeSymbol tree) => validate(tree);
}

[Collection(JavaScriptNonStrictBrowserCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptNonStrictBrowser")]
public sealed class JavaScriptNonStrictBrowserTests
{
    private readonly JavaScriptNonStrictBrowserFixture fixture;
    public JavaScriptNonStrictBrowserTests(JavaScriptNonStrictBrowserFixture fixture) => this.fixture = fixture;

    [Theory]
    [InlineData("label: function f() {}")]
    [InlineData("first: second: function f() { return 1; }")]
    [InlineData("{ label: function f() {} }")]
    [InlineData("function outer() { label: function inner() { return 1; } }")]
    [InlineData("if (ready) { label: function f() {} }")]
    [InlineData("while (ready) { label: function f() {} break; }")]
    [InlineData("with (object) { label: function f() {} }")]
    [InlineData("label: function f() { 'use strict'; }")]
    [InlineData("label: function f() {} /a/.test(text);")]
    public void Labelled_ordinary_declarations_are_optional_browser_syntax(string input)
    {
        fixture.Browser.Success(input);
        fixture.Core.Invalid(input);
    }

    [Theory]
    [InlineData("if (ready) label: function f() {}")]
    [InlineData("if (ready) ; else label: function f() {}")]
    [InlineData("while (ready) label: function f() {}")]
    [InlineData("do label: function f() {} while (ready);")]
    [InlineData("for (;;) label: function f() {}")]
    [InlineData("for (key in object) label: function f() {}")]
    [InlineData("for (value of values) label: function f() {}")]
    [InlineData("async function f() { for await (value of values) label: function g() {} }")]
    [InlineData("with (object) label: function f() {}")]
    [InlineData("if (ready) first: second: function f() {}")]
    [InlineData("label: function* f() {}")]
    [InlineData("label: async function f() {}")]
    [InlineData("label: async function* f() {}")]
    [InlineData("label: function f() { break label; }")]
    public void Browser_support_does_not_make_every_statement_body_a_function_declaration(string input)
        => fixture.Browser.Invalid(input);

    [Theory]
    [InlineData("if (ready) function f() {}")]
    [InlineData("if (ready) function f() {} else ;")]
    [InlineData("if (ready) ; else function f() {}")]
    [InlineData("if (ready) function f() {} else function f() {}")]
    [InlineData("if (ready) function first() {} else function second() {}")]
    [InlineData("function outer() { if (ready) function inner() {} }")]
    [InlineData("if (first) if (second) function f() {} else function g() {}")]
    [InlineData("if (first) if (second) function f() {} else function g() {} else function h() {}")]
    [InlineData("outer: if (ready) function f() {}")]
    [InlineData("if (ready) function f(value, value) { return value; }")]
    [InlineData("if (ready) function f() { 'use strict'; return 1; }")]
    [InlineData("if (ready) function f() {} /a/.test(text);")]
    [InlineData("if (ready) function f() {} else function g() {} /a/.test(text);")]
    public void Direct_if_function_declarations_are_accepted_only_with_the_browser_option(string input)
    {
        fixture.Browser.Success(input);
        fixture.Core.Invalid(input);
    }

    [Theory]
    [InlineData("if (ready) function* f() {}")]
    [InlineData("if (ready) async function f() {}")]
    [InlineData("if (ready) async function* f() {}")]
    [InlineData("if (ready) ; else function* f() {}")]
    [InlineData("if (ready) ; else async function f() {}")]
    [InlineData("while (ready) function f() {}")]
    [InlineData("for (;;) function f() {}")]
    [InlineData("with (object) function f() {}")]
    [InlineData("if (ready) function f(value = 0) { 'use strict'; }")]
    [InlineData("if (ready) function f(value, value) { 'use strict'; }")]
    public void Direct_if_extension_keeps_function_kind_and_function_body_early_errors(string input)
        => fixture.Browser.Invalid(input);

    [Theory]
    [InlineData("{ function f() {} function f() {} }")]
    [InlineData("{ function f() { 'use strict'; } function f() {} }")]
    [InlineData("function outer() { { function f() {} function f() {} } }")]
    [InlineData("switch (value) { case 0: function f() {} break; case 1: function f() {} }")]
    [InlineData("let f; { function f() {} }")]
    [InlineData("function outer(f) { { function f() {} } }")]
    [InlineData("{ function f() {} } { function f() {} }")]
    public void Ordinary_block_declarations_keep_browser_scope_compatibility(string input)
    {
        fixture.Browser.Success(input);
    }

    [Theory]
    [InlineData("{ function f() {} function f() {} }")]
    [InlineData("switch (value) { case 0: function f() {} break; default: function f() {} }")]
    [InlineData("try {} catch (error) { function f() {} function f() {} }")]
    public void Duplicate_block_functions_require_both_non_strict_code_and_browser_compatibility(string input)
    {
        fixture.Browser.Success(input);
        fixture.Core.Invalid(input);
        fixture.Strict.Invalid(input);
        fixture.Module.Invalid(input);
        fixture.Browser.Invalid("'use strict'; " + input);
    }

    [Theory]
    [InlineData("{ function f() {} let f; }")]
    [InlineData("{ function f() {} const f = 1; }")]
    [InlineData("{ function f() {} class f {} }")]
    [InlineData("{ function f() {} var f; }")]
    [InlineData("{ function* f() {} function* f() {} }")]
    [InlineData("{ async function f() {} async function f() {} }")]
    [InlineData("{ function f() {} function* f() {} }")]
    [InlineData("{ function f() {} async function f() {} }")]
    [InlineData("switch (value) { case 0: function f() {} break; default: let f; }")]
    [InlineData("switch (value) { function f() {} }")]
    public void Duplicate_block_exception_applies_only_to_ordinary_function_declarations(string input)
        => fixture.Browser.Invalid(input);

    [Theory]
    [InlineData("try {} catch (error) { var error; }")]
    [InlineData("try {} catch (error) { var error = 1; }")]
    [InlineData("try {} catch (error) { { var error; } }")]
    [InlineData("try {} catch (error) { for (var error in object) ; }")]
    [InlineData("try {} catch (error) { for (var error of values) ; }")]
    [InlineData("function outer() { try {} catch (error) { var error; } }")]
    [InlineData("'use strict'; try {} catch (error) { var error; }")]
    public void Simple_catch_var_exception_is_independent_of_strictness(string input)
    {
        fixture.Browser.Success(input);
        fixture.Strict.Success(input);
        fixture.Module.Success(input);
        fixture.Core.Invalid(input);
        fixture.CoreModule.Invalid(input);
    }

    [Theory]
    [InlineData("try {} catch ({error}) { var error; }")]
    [InlineData("try {} catch ([error]) { var error; }")]
    [InlineData("try {} catch ({error = 1}) { var error; }")]
    [InlineData("try {} catch (error) { let error; }")]
    [InlineData("try {} catch (error) { const error = 1; }")]
    [InlineData("try {} catch (error) { class error {} }")]
    [InlineData("try {} catch ({error, other: error}) {}")]
    public void Catch_extension_does_not_relax_pattern_duplicates_or_lexical_collisions(string input)
    {
        fixture.Browser.Invalid(input);
        fixture.Strict.Invalid(input);
        fixture.Module.Invalid(input);
    }

    [Theory]
    [InlineData("try {} catch (error) { { let error; } }")]
    [InlineData("try {} catch ({error}) { function inner() { var error; } }")]
    [InlineData("try {} catch (error) { function inner() { let error; } }")]
    public void Catch_names_do_not_collide_with_declarations_in_independent_inner_scopes(string input)
    {
        fixture.Browser.Success(input);
        fixture.Core.Success(input);
        fixture.Strict.Success(input);
        fixture.Module.Success(input);
    }

    [Theory]
    [InlineData("for (var key = 0 in object) ;")]
    [InlineData("for (var key = create() in object) visit(key);")]
    [InlineData("for (var key = (first in other) in object) ;")]
    [InlineData("for (var key = first ? second in other : third in object) ;")]
    [InlineData("for (var key = `${value}` in object) ;")]
    [InlineData("function outer() { for (var key = 0 in object) ; }")]
    [InlineData("for (var key = 0 in object) { var key; }")]
    public void For_in_initializer_accepts_only_the_legacy_single_var_binding_form(string input)
    {
        fixture.Browser.Success(input);
        fixture.Core.Invalid(input);
        fixture.Strict.Invalid(input);
        fixture.Module.Invalid(input);
        fixture.Browser.Invalid("'use strict'; " + input);
    }

    [Theory]
    [InlineData("for (let key = 0 in object) ;")]
    [InlineData("for (const key = 0 in object) ;")]
    [InlineData("for (var [key] = values in object) ;")]
    [InlineData("for (var {key} = value in object) ;")]
    [InlineData("for (var first = 0, second = 1 in object) ;")]
    [InlineData("for (var value = 0 of values) ;")]
    [InlineData("async function f() { for await (var value = 0 of values) ; }")]
    public void For_in_initializer_extension_does_not_extend_to_patterns_multiple_bindings_or_for_of(string input)
        => fixture.Browser.Invalid(input);

    [Theory]
    [InlineData("label: function f() {}")]
    [InlineData("if (ready) function f() {}")]
    [InlineData("for (var key = 0 in object) ;")]
    [InlineData("{ function f() {} function f() {} }")]
    [InlineData("call() = value;")]
    public void Directive_inheritance_and_classes_disable_non_strict_only_browser_forms(string input)
    {
        fixture.Browser.Invalid("function outer() { 'use strict'; " + input + " }");
        fixture.Browser.Invalid("class C { method() { " + input + " } }");
        fixture.Browser.Invalid("'use strict'; function outer() { " + input + " }");
    }

    [Theory]
    [InlineData("<!-- lead\nvalue;")]
    [InlineData("--> lead\nvalue;")]
    [InlineData(" \t--> lead\nvalue;")]
    [InlineData("/* prefix */ --> lead\nvalue;")]
    [InlineData("value; <!-- tail\nother;")]
    [InlineData("'use strict'; <!-- tail\nvalue;")]
    public void Html_comments_use_the_browser_option_and_source_goal_not_strictness(string input)
    {
        fixture.Browser.Success(input);
        fixture.Strict.Success(input);
        fixture.Core.Invalid(input);
        fixture.Module.Invalid(input);
    }

    public static IEnumerable<object[]> BrowserCallAssignments()
    {
        foreach (var operation in new[] { "=", "+=", "-=", "*=", "/=", "%=", "**=", "<<=", ">>=", ">>>=", "&=", "|=", "^=" })
            yield return new object[] { "call() " + operation + " value;" };
    }

    [Theory]
    [MemberData(nameof(BrowserCallAssignments))]
    [InlineData("(call()) = value;")]
    [InlineData("((call())) += value;")]
    [InlineData("object.method() = value;")]
    [InlineData("call()() = value;")]
    [InlineData("eval() = value;")]
    [InlineData("async function f() { call() = value; }")]
    [InlineData("function* f() { call() = value; yield 1; }")]
    [InlineData("({ run() { call() = value; } });")]
    [InlineData("({ run() { super.value() = other; } });")]
    public void Call_assignment_targets_are_web_compatible_rather_than_valid_runtime_references(string input)
    {
        fixture.Browser.Success(input);
        fixture.Core.Invalid(input);
        fixture.Strict.Invalid(input);
        fixture.Module.Invalid(input);
    }

    [Theory]
    [InlineData("call()++;")]
    [InlineData("call()--;")]
    [InlineData("++call();")]
    [InlineData("--call();")]
    [InlineData("++(call());")]
    [InlineData("(call())--;")]
    [InlineData("for (call() in object) ;")]
    [InlineData("for ((call()) in object) ;")]
    [InlineData("for (call() of values) ;")]
    [InlineData("async function f() { for await (call() of values) ; }")]
    public void Updates_and_iteration_targets_also_accept_web_compat_targets_only_in_non_strict_browser_code(string input)
    {
        fixture.Browser.Success(input);
        fixture.Core.Invalid(input);
        fixture.Strict.Invalid(input);
        fixture.Module.Invalid(input);
    }

    [Theory]
    [InlineData("call() &&= value;")]
    [InlineData("call() ||= value;")]
    [InlineData("call() ??= value;")]
    [InlineData("[call()] = values;")]
    [InlineData("[call() = fallback] = values;")]
    [InlineData("[...call()] = values;")]
    [InlineData("({value: call()} = object);")]
    [InlineData("({...call()} = object);")]
    [InlineData("for ([call()] of values) ;")]
    [InlineData("for ({value: call()} in object) ;")]
    [InlineData("async function f() { for await ([call()] of values) ; }")]
    [InlineData("new Factory() = value;")]
    [InlineData("import(path) = value;")]
    [InlineData("tag`text` = value;")]
    [InlineData("call?.() = value;")]
    [InlineData("object?.method() = value;")]
    [InlineData("(first(), second()) = value;")]
    public void Call_target_extension_does_not_relax_simple_only_or_non_call_targets(string input)
        => fixture.Browser.Invalid(input);

    [Theory]
    [InlineData("[call().value] = values;")]
    [InlineData("({value: call().value} = object);")]
    [InlineData("call().value &&= value;")]
    [InlineData("for (call().value of values) ;")]
    public void Property_access_after_a_call_remains_a_standard_simple_target(string input)
    {
        fixture.Browser.Success(input);
        fixture.Core.Success(input);
        fixture.Strict.Success(input);
        fixture.Module.Success(input);
    }

    [Theory]
    [InlineData("call() = value")]
    [InlineData("call() **= value")]
    [InlineData("++call()")]
    [InlineData("(function () { if (ready) function f() {} })")]
    public void Expression_source_goal_uses_the_same_browser_host_choice(string input)
    {
        fixture.Expression.Success(input);
        fixture.CoreExpression.Invalid(input);
    }

    [Fact]
    public void Browser_extensions_preserve_source_trivia_token_positions_and_tree_identity()
    {
        const string input = "<!-- browser lead\r\nlabel: function read() { return /a/; }\r\n" +
            "if (ready) function chosen() {}\r\n" +
            "try {} catch (error) { var error; }\r\n" +
            "for (var key = 0 in object) ;\r\ncall() = value;";
        var tree = fixture.Browser.Success(input);
        var before = tree.AllTokens.ToArray();
        Assert.Equal(input, string.Concat(before.Select(token => token.LeadingTrivia.Text + token.Input)));
        Assert.Contains(before, token => token.LeadingTrivia.Text.Contains("<!-- browser lead", StringComparison.Ordinal));
        foreach (var token in before)
            Assert.Equal(input.Substring(token.StartIndex, token.Input.Length), token.Input);

        // Rechecking the completed tree is different from repeatedly extracting it
        // from ParsingResult, whose stack inspection is outside this test's scope.
        Assert.Empty(fixture.Browser.Validate(tree));
        var after = tree.AllTokens.ToArray();
        Assert.Equal(before.Length, after.Length);
        for (int index = 0; index < before.Length; index++) Assert.Same(before[index], after[index]);
    }
}
