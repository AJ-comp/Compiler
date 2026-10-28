using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMAScript 2026 sections 14.3.1-14.3.3 and 15.1-15.2. Binding patterns create
// names; assignment patterns may instead assign an existing object's property.
[Collection(JavaScriptScriptCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptBindings")]
public sealed class JavaScriptBindingTests
{
    private readonly JavaScriptScriptFixture _fixture;
    public JavaScriptBindingTests(JavaScriptScriptFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("var value;")]
    [InlineData("var value = 1, other;")]
    [InlineData("let value;")]
    [InlineData("let value = 1, other = 2;")]
    [InlineData("const value = 1, other = 2;")]
    [InlineData("var [] = values;")]
    [InlineData("let [,] = values;")]
    [InlineData("const [,,] = values;")]
    [InlineData("let [first, second] = values;")]
    [InlineData("let [first,,third,] = values;")]
    [InlineData("let [first,,,] = values;")]
    [InlineData("const [first = 1, second = first + 1] = values;")]
    [InlineData("const [[first], {name: localName}] = values;")]
    [InlineData("let [[first] = fallback, {name} = user] = values;")]
    [InlineData("var {} = source;")]
    [InlineData("let {name} = source;")]
    [InlineData("const {name, age,} = source;")]
    [InlineData("let {name: localName} = source;")]
    [InlineData("let {name = fallback} = source;")]
    [InlineData("let {name: localName = fallback} = source;")]
    [InlineData("let {nested: {value = fallback}, list: [first,,third]} = source;")]
    [InlineData("let {[key]: value} = source;")]
    [InlineData("let {[(first, second)]: value} = source;")]
    [InlineData("let {[key = nextKey]: value = fallback} = source;")]
    [InlineData("let {'name': value, 1: number, 1n: bigint} = source;")]
    [InlineData("let {if: value, default: other, true: truth, null: empty} = source;")]
    [InlineData("let {eval: value, arguments: other} = source;")]
    [InlineData("let {__proto__: first, '__proto__': second} = source;")]
    [InlineData("let [await, of, 한글, $value] = values;")]
    [InlineData("let {await, of} = source;")]
    [InlineData("var [same, same] = values;")]
    [InlineData("var {a: same, b: same} = source;")]
    [InlineData("var a; var a;")]
    [InlineData("let [a = b += c] = values;")]
    [InlineData("let [a = b ??= c] = values;")]
    [InlineData("let [a = /a/.test(value), b = total / divisor] = values;")]
    [InlineData("const {a = `hello ${name}`, b = tag`raw\\xZ`} = source;")]
    [InlineData("let [{a = ({b = 1} = other)}] = values;")]
    [InlineData("let [a = function named({b}) { return b; }] = values;")]
    [InlineData("let {[function named({b}) { return b; }]: value} = source;")]
    public void Declaration_bindings_accept_nested_patterns_defaults_and_computed_keys(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("let [...rest] = values;")]
    [InlineData("let [, ...rest] = values;")]
    [InlineData("let [first,, ...rest] = values;")]
    [InlineData("let [...[first, second]] = values;")]
    [InlineData("let [...[first = fallback, ...rest]] = values;")]
    [InlineData("let [...{length}] = values;")]
    [InlineData("let [...[]] = values;")]
    [InlineData("let {...rest} = source;")]
    [InlineData("let {name, ...rest} = source;")]
    [InlineData("let {nested: {...rest}, list: [...tail]} = source;")]
    [InlineData("function f(...rest) {}")]
    [InlineData("function f(first, ...rest) {}")]
    [InlineData("function f(...[first, second]) {}")]
    [InlineData("function f(...{length}) {}")]
    [InlineData("function f(...[first = fallback, ...rest]) {}")]
    [InlineData("function f({a, ...rest}, [first,,...tail], ...other) {}")]
    public void Array_rest_can_bind_a_pattern_while_object_rest_binds_an_identifier(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("function f() {}")]
    [InlineData("function f(a) {}")]
    [InlineData("function f(a, b,) {}")]
    [InlineData("function f(a = 1, b = a + 1) { return b; }")]
    [InlineData("function f(a = 1, b) {}")]
    [InlineData("function f([], {}) {}")]
    [InlineData("function f([first,,third,], {name: localName = fallback}) { return localName; }")]
    [InlineData("function f([first] = values, {name} = source) {}")]
    [InlineData("function f({if: value, eval: other}) {}")]
    [InlineData("function f(await, of) { return await + of; }")]
    [InlineData("function f(a = /a/, b = value / divisor) { return b; }")]
    [InlineData("function f(a = `${value}`, {[key]: b} = source) { return b; }")]
    [InlineData("function f(a = function inner({x}) { return x; }) { return a; }")]
    [InlineData("function f(a = 0) { 'ordinary directive'; }")]
    [InlineData("function f(a = 0) { 'use\\x20strict'; }")]
    [InlineData("function f(a = 0) { ; 'use strict'; }")]
    [InlineData("function f(a = 0) { 0; 'use strict'; }")]
    [InlineData("function f(a = 0) { 'use strict' + ''; }")]
    [InlineData("function f(a = 0) { ('use strict'); }")]
    [InlineData("function f(a = 0) { { 'use strict'; } }")]
    [InlineData("function f(a) { 'use strict'; }")]
    [InlineData("'use strict'; function f(a = 0) {}")]
    public void Ordinary_parameters_accept_defaults_patterns_and_a_trailing_list_comma(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const value;")]
    [InlineData("const first = 1, second;")]
    [InlineData("var [value];")]
    [InlineData("let [value];")]
    [InlineData("const [value];")]
    [InlineData("var {value};")]
    [InlineData("let {value};")]
    [InlineData("let [object.value] = values;")]
    [InlineData("let {value: object.value} = source;")]
    [InlineData("function f(object.value) {}")]
    [InlineData("function f({value: object.value}) {}")]
    [InlineData("let [(value)] = values;")]
    [InlineData("let {value: (target)} = source;")]
    [InlineData("let [call()] = values;")]
    [InlineData("let [value += 1] = values;")]
    [InlineData("let [value = (other, third), next] ;")]
    [InlineData("let [...rest,] = values;")]
    [InlineData("let [...rest, next] = values;")]
    [InlineData("let [...rest = fallback] = values;")]
    [InlineData("let [...object.rest] = values;")]
    [InlineData("let [...[first],] = values;")]
    [InlineData("let {...rest,} = source;")]
    [InlineData("let {...rest, next} = source;")]
    [InlineData("let {...rest = fallback} = source;")]
    [InlineData("let {...object.rest} = source;")]
    [InlineData("let {...[first]} = source;")]
    [InlineData("let {...{name}} = source;")]
    [InlineData("function f(...rest,) {}")]
    [InlineData("function f(...rest, next) {}")]
    [InlineData("function f(...rest = fallback) {}")]
    [InlineData("function f(...object.rest) {}")]
    [InlineData("function f(...[first],) {}")]
    [InlineData("function f(a,,b) {}")]
    [InlineData("let {a = {b = 1}} = source;")]
    [InlineData("let {[{a = 1}]: b} = source;")]
    [InlineData("let {a = {__proto__: 1, __proto__: 2}} = source;")]
    [InlineData("let [a = `bad\\xZ`] = values;")]
    [InlineData("function f(a = `bad\\uZ`) {}")]
    [InlineData("let {name() {}} = source;")]
    public void Binding_targets_rest_positions_and_expression_subcontexts_are_checked(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("let eval;")]
    [InlineData("var arguments;")]
    [InlineData("const [eval] = values;")]
    [InlineData("let {arguments} = source;")]
    [InlineData("let {name: eval} = source;")]
    [InlineData("let [...arguments] = values;")]
    [InlineData("let {...eval} = source;")]
    [InlineData("function eval() {}")]
    [InlineData("const f = function arguments() {};")]
    [InlineData("function f(eval) {}")]
    [InlineData("function f({arguments}) {}")]
    [InlineData("function f(...eval) {}")]
    [InlineData("let \\u0065val;")]
    [InlineData("function f(\\u0061rguments) {}")]
    [InlineData("let \\u0069f;")]
    [InlineData("function f(\\u0079ield) {}")]
    [InlineData("let [same, same] = values;")]
    [InlineData("const {a: same, b: same} = source;")]
    [InlineData("let {a: {same}, b: [same]} = source;")]
    [InlineData("function f(a, a) {}")]
    [InlineData("function f([same], {same}) {}")]
    [InlineData("function f(a, ...a) {}")]
    [InlineData("function f(a, \\u0061) {}")]
    [InlineData("function f(a = 0) { 'use strict'; }")]
    [InlineData("function f({a}) { 'use strict'; }")]
    [InlineData("function f(...rest) { 'use strict'; }")]
    [InlineData("function f(a = 0) { 'first'; 'use strict'; }")]
    [InlineData("function f(a = 0) { 'use strict'\n }")]
    [InlineData("function f(a = 0) { 'first'\n'use strict'\n }")]
    [InlineData("let [a = function nested(x, x) {}] = values;")]
    [InlineData("let {[function nested(x, x) {}]: a} = source;")]
    [InlineData("function f(a = function nested(x, x) {}) {}")]
    [InlineData("function f({[function nested(x, x) {}]: a}) {}")]
    public void Strict_binding_names_duplicates_and_non_simple_strict_directives_are_rejected(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("var value; { let value; }")]
    [InlineData("{ let value; } var value;")]
    [InlineData("{ let value; } { let value; }")]
    [InlineData("let value; { let value; }")]
    [InlineData("function f(value) { var value; }")]
    [InlineData("function f(value) { function value() {} }")]
    [InlineData("function f(value) { { let value; } }")]
    [InlineData("function f() { let value; function g() { var value; } }")]
    [InlineData("let value = function value() {};")]
    [InlineData("const value = function named({value}) { return value; };")]
    [InlineData("let {[function inner(value) {}]: value} = source;")]
    [InlineData("let [value = function inner(value) {}] = values;")]
    [InlineData("function f(value = function inner(value) {}) {}")]
    [InlineData("function f({[function inner(value) {}]: value}) {}")]
    [InlineData("function f(value = function inner() { return; }) { let inner; }")]
    [InlineData("let {[eval]: value} = source;")]
    [InlineData("let [value = eval] = values;")]
    [InlineData("function f(value = eval, {other = arguments}) {}")]
    [InlineData("try {} catch (error) { { let error; } }")]
    [InlineData("for (let value = 0; value < 1; ++value) { let value; }")]
    [InlineData("for (let value of values) { let value; }")]
    [InlineData("for (var value of values) { let value; }")]
    [InlineData("switch (value) { case 0: { let item; } break; default: { let item; } }")]
    [InlineData("{ function f() {} } { function f() {} }")]
    public void Names_in_different_lexical_scopes_and_nested_function_expressions_do_not_collide(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("let value; let value;")]
    [InlineData("let value; const value = 1;")]
    [InlineData("let value; var value;")]
    [InlineData("var value; const value = 1;")]
    [InlineData("let value; function value() {}")]
    [InlineData("function value() {} let value;")]
    [InlineData("{ let value; { var value; } }")]
    [InlineData("function f(value) { let value; }")]
    [InlineData("function f([value]) { const value = 1; }")]
    [InlineData("function f() { let value; function value() {} }")]
    [InlineData("{ function value() {} function value() {} }")]
    [InlineData("{ function value() {} let value; }")]
    [InlineData("switch (value) { case 0: let item; break; default: let item; }")]
    [InlineData("try {} catch (error) { let error; }")]
    [InlineData("try {} catch ([error, error]) {}")]
    [InlineData("try {} catch ({a: error, b: error}) {}")]
    [InlineData("try {} catch ({error}) { var error; }")]
    [InlineData("for (let value = 0; value < 1; ++value) { var value; }")]
    [InlineData("for (const value of values) { var value; }")]
    [InlineData("for (let [value, value] of values) ;")]
    public void Lexical_declarations_conflict_only_with_names_in_their_own_scope(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("for (var [key] in object) ;")]
    [InlineData("for (let {key} in object) ;")]
    [InlineData("for (const [first,,...rest] of values) ;")]
    [InlineData("for (const {name: localName = fallback, ...rest} of values) ;")]
    [InlineData("for ([object.value] of values) ;")]
    [InlineData("for ({name: object.value} of values) ;")]
    [InlineData("for ({name = fallback} in object) ;")]
    [InlineData("try {} catch ([first, ...rest]) {}")]
    [InlineData("try {} catch ({message, name: localName = fallback, ...rest}) {}")]
    [InlineData("try {} catch ({}) {}")]
    [InlineData("try {} catch ([]) {}")]
    public void Loop_heads_and_catch_parameters_use_their_specific_binding_or_assignment_context(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("for (const [value] = fallback of values) ;")]
    [InlineData("for (let {value} = fallback in object) ;")]
    [InlineData("for ([...rest,] of values) ;")]
    [InlineData("for ({...{value}} in object) ;")]
    [InlineData("for (let [object.value] of values) ;")]
    [InlineData("try {} catch (error = fallback) {}")]
    [InlineData("try {} catch (object.error) {}")]
    [InlineData("try {} catch ({...{error}}) {}")]
    [InlineData("try {} catch (...error) {}")]
    [InlineData("try {} catch (eval) {}")]
    public void Loop_and_catch_bindings_reject_initializers_members_and_invalid_rest_patterns(string input)
        => _fixture.Invalid(input);

    public static IEnumerable<object[]> ReservedBindings()
    {
        foreach (string name in new[] { "eval", "arguments", "let", "yield", "static", "implements",
                     "interface", "package", "private", "protected", "public", "enum" })
        foreach (string form in new[] { "var {0};", "let [{0}] = values;", "const {{name: {0}}} = source;",
                     "function f({0}) {{}}", "function f(...{0}) {{}}", "try {{}} catch ({0}) {{}}" })
            yield return new object[] { string.Format(form, name) };
    }

    [Theory]
    [MemberData(nameof(ReservedBindings))]
    public void Strict_reserved_names_are_rejected_in_each_binding_position(string input) => _fixture.Invalid(input);

    [Theory]
    [InlineData("let eval;", "eval")]
    [InlineData("let {name: arguments} = source;", "arguments")]
    [InlineData("function f(\\u0065val) {}", "\\u0065val")]
    [InlineData("try {} catch (arguments) {}", "arguments")]
    public void Invalid_binding_diagnostics_identify_the_original_identifier(string input, string identifier)
    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        var diagnostic = Assert.Single(_fixture.Language.Validate(parsed.ToParseTree),
            d => d.Code == "invalid_binding_identifier");
        Assert.Equal(input.IndexOf(identifier, StringComparison.Ordinal), diagnostic.SourceOffset);
        Assert.Equal(identifier.Length, diagnostic.Length);
        Assert.Equal(identifier, input.Substring(diagnostic.SourceOffset, diagnostic.Length));
    }

    [Fact]
    public void Validation_does_not_reinterpret_default_values_as_binding_patterns_or_mutate_the_tree()
    {
        const string input = "const {name: localName = ({value = 1} = source), ...rest} = data;";
        var tree = _fixture.Success(input);
        string before = JavaScriptScriptFixture.Text(tree);
        var tokens = tree.AllTokens.ToArray();
        Assert.Empty(_fixture.Language.Validate(tree));
        Assert.Empty(_fixture.Language.Validate(tree));
        _fixture.Invalid("const {name = {value = 1}} = data;");
        Assert.Equal(before, JavaScriptScriptFixture.Text(tree));
        Assert.Equal(tokens, tree.AllTokens.ToArray());
        Assert.Empty(_fixture.Language.Validate(tree));
    }

    [Fact]
    public void Public_static_validation_checks_functions_and_Script_bindings_like_the_language_factory()
    {
        foreach (var example in new[]
                 {
                     (Input: "const callback = function named(value, value) {};", Code: "duplicate_parameter"),
                     (Input: "let eval;", Code: "invalid_binding_identifier"),
                     (Input: "let value; var value;", Code: "conflicting_declaration")
                 })
        {
            var parsed = _fixture.Parse(example.Input);
            Assert.True(parsed.Success, example.Input + Environment.NewLine + _fixture.Parser.FiredException);
            var direct = JavaScriptExpressionValidation.Validate(parsed.ToParseTree);
            var throughFactory = _fixture.Language.Validate(parsed.ToParseTree);
            Assert.Contains(direct, diagnostic => diagnostic.Code == example.Code);
            Assert.Equal(throughFactory.Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray(),
                direct.Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray());
            Assert.All(direct, diagnostic =>
            {
                Assert.InRange(diagnostic.SourceOffset, 0, example.Input.Length);
                Assert.InRange(diagnostic.Length, 0, example.Input.Length - diagnostic.SourceOffset);
            });
        }

        var valid = _fixture.Success("const callback = function named(value) { return value; };");
        Assert.Empty(JavaScriptExpressionValidation.Validate(valid));
    }

    [Fact]
    public void The_expression_entry_also_accepts_ordinary_function_expressions()
    {
        var language = new JavaScriptGrammar(inputKind: JavaScriptInputKind.Expression);
        var parser = new LALRParser(language.Grammar, false);
        foreach (string input in new[] { "function() {}", "function named(a) { return a; }",
                     "function({name}, [first] = values, ...rest) { return name; }",
                     "(function() { return /a/; })() / divisor",
                     "[function() {}, function named() {}]",
                     "({callback: function() { return 1; }})" })
        {
            var parsed = parser.Parsing(language.CreateTokenSource(input));
            Assert.True(parsed.Success, input + Environment.NewLine + parser.FiredException);
            Assert.Empty(language.Validate(parsed.ToParseTree));
        }
    }
}
