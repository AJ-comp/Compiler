using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMAScript 2026 section 15.3. These tests compile strict Script syntax without
// executing function bodies. Async variants have additional function-kind tests.
[Collection(JavaScriptScriptCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptArrows")]
public sealed class JavaScriptArrowFunctionTests
{
    private readonly JavaScriptScriptFixture _fixture;
    public JavaScriptArrowFunctionTests(JavaScriptScriptFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("const f = value => value;")]
    [InlineData("const f = (value) => value;")]
    [InlineData("const f = () => 1;")]
    [InlineData("const f = (first, second) => first + second;")]
    [InlineData("const f = (first, second,) => first + second;")]
    [InlineData("const f = (first = 1, second = first + 1) => second;")]
    [InlineData("const f = (first = 1, second) => second;")]
    [InlineData("const f = ([first,,third,], {name: localName}) => localName;")]
    [InlineData("const f = ([first] = values, {name = fallback} = source) => name;")]
    [InlineData("const f = ({nested: {value}, list: [first, ...tail]}) => value;")]
    [InlineData("const f = ({[key]: value = fallback}) => value;")]
    [InlineData("const f = ({[(first, second)]: value}) => value;")]
    [InlineData("const f = ({if: value, eval: other, arguments: third}) => value;")]
    [InlineData("const f = ({__proto__: first, '__proto__': second}) => second;")]
    [InlineData("const f = (...rest) => rest;")]
    [InlineData("const f = (first, ...rest) => rest;")]
    [InlineData("const f = (...[first, second]) => first;")]
    [InlineData("const f = (...[first = fallback, ...rest]) => first;")]
    [InlineData("const f = (...{length}) => length;")]
    [InlineData("const f = ({name, ...rest}, [...tail]) => rest;")]
    [InlineData("const f = ({}, []) => 0;")]
    [InlineData("const f = await => await;")]
    [InlineData("const f = (await, of, async) => await + of + async;")]
    [InlineData("const f = async => async;")]
    [InlineData("const f = \\u0061 => a;")]
    [InlineData("const f = 한글 => 한글;")]
    [InlineData("const f = ($value, _value) => $value + _value;")]
    [InlineData("const f = (value = other += 1) => value;")]
    [InlineData("const f = (value = other ??= 1) => value;")]
    [InlineData("const f = (value = ({name = 1} = source)) => value;")]
    [InlineData("const f = (value = function() { return 1; }) => value;")]
    [InlineData("const f = (value = (first, second)) => value;")]
    [InlineData("const f = (value = first, second) => value;")]
    [InlineData("const f = (value = other, other) => value;")]
    [InlineData("const f = ({[name]: value}, name) => value;")]
    [InlineData("const f = ({first: second, second: first}) => first;")]
    [InlineData("const f = (value = value => value) => value;")]
    [InlineData("const f = (...[...rest]) => rest;")]
    public void Arrow_parameters_accept_identifiers_binding_patterns_defaults_and_rest(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const f = () => {};")]
    [InlineData("const f = value => { return value; };")]
    [InlineData("const f = value => { if (value) return 1; else return 2; };")]
    [InlineData("const f = value => { const [first] = value; return first; };")]
    [InlineData("const f = () => { function inner() { return 1; } return inner(); };")]
    [InlineData("const f = () => { while (ready) { break; } return 1; };")]
    [InlineData("const f = () => { for (;;) { continue; } };")]
    [InlineData("const f = () => { switch (value) { default: break; } };")]
    [InlineData("const f = () => { label: while (ready) { continue label; } };")]
    [InlineData("const f = () => { try { step(); } catch ({message}) { return message; } };")]
    [InlineData("const f = () => { var value; var value; return value; };")]
    [InlineData("const f = value => { var value; return value; };")]
    [InlineData("const f = (value = 1) => { var value; return value; };")]
    [InlineData("const f = ({value}) => { var value; return value; };")]
    [InlineData("const f = value => { { let value; } return value; };")]
    [InlineData("const f = value => { function value() {} return value; };")]
    [InlineData("const f = value => ({value});")]
    [InlineData("const f = () => ({name: 'value'});")]
    [InlineData("const f = () => { name: 'value'; };")]
    [InlineData("const f = () => {value = 1};")]
    [InlineData("const f = () => ({value: () => 1});")]
    [InlineData("const f = () => (first, second);")]
    [InlineData("const f = () => this;")]
    [InlineData("const f = () => arguments;")]
    [InlineData("const f = (value = this) => arguments;")]
    [InlineData("const f = () => function() { return 1; };")]
    [InlineData("const f = () => ({value = 1} = source);")]
    [InlineData("const f = () => [first, second] = values;")]
    [InlineData("const f = () => { return\nvalue; };")]
    [InlineData("const f = () => { return 1 }\nnext();")]
    [InlineData("const f = () => { throw failure; };")]
    [InlineData("let value; const f = value => value;")]
    public void Expression_and_block_bodies_keep_their_own_statement_and_binding_context(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const f = first => second => first + second;")]
    [InlineData("const f = (first) => (second) => ({first, second});")]
    [InlineData("const f = (first = second => second) => first;")]
    [InlineData("const f = ({first = second => second}) => first;")]
    [InlineData("const f = (first = () => {}) => first;")]
    [InlineData("const f = () => { return value => value; };")]
    [InlineData("const f = () => { const g = () => 1; return g; };")]
    [InlineData("const f = condition ? first => first : second => second;")]
    [InlineData("const f = value => condition ? first : second;")]
    [InlineData("const f = value => other = value;")]
    [InlineData("const f = value => other += value;")]
    [InlineData("const f = value => value || fallback;")]
    [InlineData("const f = fallback || (value => value);")]
    [InlineData("const f = (value => value) || fallback;")]
    [InlineData("const f = (value => value)(input);")]
    [InlineData("const f = (() => value).name;")]
    [InlineData("const f = (() => value)?.(input);")]
    [InlineData("const f = (() => value)`text`;")]
    [InlineData("const f = new (value => value)(input);")]
    [InlineData("const f = [value => value, () => 1];")]
    [InlineData("const f = {callback: value => value};")]
    [InlineData("call(first => first, second => second);")]
    [InlineData("function f(callback = value => value) { return callback; }")]
    [InlineData("for (value => (value in object); ready; ) ;")]
    [InlineData("for (callback = () => { return key in object; }; ready; ) ;")]
    [InlineData("for ((value = key in object) => value; ready; ) ;")]
    [InlineData("for (value => value ? key in object : other; ready; ) ;")]
    [InlineData("for (value => { key in object; }; ready; ) ;")]
    [InlineData("value => value;")]
    [InlineData("if (ready) value => value;")]
    [InlineData("while (ready) value => value;")]
    [InlineData("label: value => value;")]
    [InlineData("async\nvalue => value;")]
    public void Arrows_bind_at_assignment_precedence_and_nest_to_the_right(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const f = value /* same line */ => value;")]
    [InlineData("const f = (value) /* same line */ => value;")]
    [InlineData("const f = (\nvalue\n) => value;")]
    [InlineData("const f = (\n{value}\n) => value;")]
    [InlineData("const f = value =>\nvalue;")]
    [InlineData("const f = value =>\n{ return value; };")]
    [InlineData("const f = value => // body begins on the next line\nvalue;")]
    [InlineData("const f = () => /a/.test(value);")]
    [InlineData("const f = value => value / divisor / other;")]
    [InlineData("const f = (value = /a/, other = total / divisor) => value;")]
    [InlineData("const f = ({[/a/.source]: value}, [other = total / divisor]) => value;")]
    [InlineData("const f = () => { return /a/; };")]
    [InlineData("const f = (() => {}) / divisor;")]
    [InlineData("const f = () => {}; /a/.test(value);")]
    [InlineData("const f = () => {}\n/a/.test(value);")]
    [InlineData("const f = () => {}\n(value);")]
    [InlineData("const f = () => {}\n[value];")]
    [InlineData("const f = () => {}\n`value`;")]
    [InlineData("const f = () => {}\n+1;")]
    [InlineData("const f = () => () => {}\n/a/.test(value);")]
    [InlineData("const f = value => `hello ${value}`;")]
    [InlineData("const f = (value = `${other}`) => tag`text ${value}`;")]
    [InlineData("const f = () => tag`raw\\xZ`;")]
    [InlineData("const f = value => `${() => ({value})}`;")]
    [InlineData("const f = () => { return `${value => value}`; };")]
    [InlineData("const f = value => value\n(next);")]
    [InlineData("const f = value => value\n[property];")]
    [InlineData("const f = value => value\n`text`;")]
    [InlineData("const f = value => value\n+ other;")]
    [InlineData("const f = value => value\n/other/divisor;")]
    [InlineData("const f = () => { return\n/a/; };")]
    [InlineData("const f = value => value\n++other;")]
    [InlineData("const f = (value = () => {}) => /a/;")]
    [InlineData("const f = (value = function () { return /a/; }) => value / divisor;")]
    [InlineData("const f = () => { return () => {}; }; /a/.test(value);")]
    [InlineData("const f = () => { return\n(value) => value; };")]
    public void Lexical_goals_templates_and_ASI_follow_the_arrow_body_context(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const f = value\n=> value;")]
    [InlineData("const f = (value)\n=> value;")]
    [InlineData("const f = ()\r=> value;")]
    [InlineData("const f = value\r\n=> value;")]
    [InlineData("const f = value\u2028=> value;")]
    [InlineData("const f = value\u2029=> value;")]
    [InlineData("const f = value /* newline\n */ => value;")]
    [InlineData("const f = (value) /* newline\u2028 */ => value;")]
    [InlineData("const f = value // newline\n=> value;")]
    [InlineData("const f = value = > value;")]
    [InlineData("const f = value =>;")]
    [InlineData("const f = () =>;")]
    [InlineData("const f = (first,,second) => first;")]
    [InlineData("const f = (,value) => value;")]
    [InlineData("const f = (,) => value;")]
    [InlineData("const f = (value,,) => value;")]
    [InlineData("const f = (value,) ;")]
    [InlineData("const value = (first,);")]
    [InlineData("const value = (...rest);")]
    [InlineData("const value = ();")]
    [InlineData("const value = ({name = 1});")]
    [InlineData("const f = ...rest => rest;")]
    [InlineData("const f = value = 1 => value;")]
    [InlineData("const f = {value} => value;")]
    [InlineData("const f = [value] => value;")]
    [InlineData("const f = ((value)) => value;")]
    [InlineData("const f = ((value = 1)) => value;")]
    [InlineData("const f = (value + other) => value;")]
    [InlineData("const f = (value += 1) => value;")]
    [InlineData("const f = (object.value) => value;")]
    [InlineData("const f = (call()) => value;")]
    [InlineData("const f = (this) => value;")]
    [InlineData("const f = (1) => value;")]
    [InlineData("const f = ('value') => value;")]
    [InlineData("const f = () => { value: 1, other: 2 };")]
    [InlineData("const f = value => if (ready) value;")]
    [InlineData("const f = value => return value;")]
    [InlineData("const f = value => throw failure;")]
    [InlineData("const f = value => let other = value;")]
    [InlineData("const f = fallback || value => value;")]
    [InlineData("const f = fallback && value => value;")]
    [InlineData("const f = fallback ?? value => value;")]
    [InlineData("const f = !value => value;")]
    [InlineData("const f = new value => value;")]
    [InlineData("const f = value + other => value;")]
    [InlineData("const f = () => {} /a/.test(value);")]
    [InlineData("const f = () => {}(value);")]
    [InlineData("const f = () => {}[value];")]
    [InlineData("const f = () => {}`value`;")]
    [InlineData("async\n(value) => value;")]
    [InlineData("for (value => value in object; ready; ) ;")]
    [InlineData("for (value => value ? other : key in object; ready; ) ;")]
    [InlineData("const f = value => `bad\\xZ`;")]
    [InlineData("const f = (value = `bad\\uZ`) => value;")]
    public void Arrow_syntax_rejects_line_breaks_before_the_arrow_and_invalid_cover_forms(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("const f = (...rest,) => rest;")]
    [InlineData("const f = (...rest, next) => rest;")]
    [InlineData("const f = (...rest = fallback) => rest;")]
    [InlineData("const f = (...object.rest) => rest;")]
    [InlineData("const f = ([...rest,]) => rest;")]
    [InlineData("const f = ([...rest, next]) => rest;")]
    [InlineData("const f = ([...rest = fallback]) => rest;")]
    [InlineData("const f = ({...rest,}) => rest;")]
    [InlineData("const f = ({...rest, next}) => rest;")]
    [InlineData("const f = ({...rest = fallback}) => rest;")]
    [InlineData("const f = ({...[rest]}) => rest;")]
    [InlineData("const f = ({...{value}}) => value;")]
    [InlineData("const f = ([object.value]) => value;")]
    [InlineData("const f = ({name: object.value}) => value;")]
    [InlineData("const f = ([(value)]) => value;")]
    [InlineData("const f = ({name: (value)}) => value;")]
    [InlineData("const f = ({name = {other = 1}}) => name;")]
    [InlineData("const f = ({[{other = 1}]: name}) => name;")]
    [InlineData("const f = (value = {__proto__: 1, __proto__: 2}) => value;")]
    [InlineData("const f = (value = {other = 1}) => value;")]
    [InlineData("const f = (first, first) => first;")]
    [InlineData("const f = (first, \\u0066irst) => first;")]
    [InlineData("const f = ([value], {value}) => value;")]
    [InlineData("const f = ({a: value, b: value}) => value;")]
    [InlineData("const f = (value, ...value) => value;")]
    [InlineData("const f = (...[value, value]) => value;")]
    [InlineData("const f = ({value: {same}, other: [same]}) => same;")]
    [InlineData("const f = ({name: value}, value) => value;")]
    public void Arrow_bindings_use_unique_bound_names_and_do_not_accept_assignment_targets(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("const f = value => { let value; };")]
    [InlineData("const f = value => { const value = 1; };")]
    [InlineData("const f = ({value}) => { let value; };")]
    [InlineData("const f = () => { let value; var value; };")]
    [InlineData("const f = () => { let value; function value() {} };")]
    [InlineData("const f = () => { break; };")]
    [InlineData("const f = () => { continue; };")]
    [InlineData("while (ready) { const f = () => { break; }; }")]
    [InlineData("while (ready) { const f = () => { continue; }; }")]
    [InlineData("label: while (ready) { const f = () => { break label; }; }")]
    [InlineData("label: while (ready) { const f = () => { continue label; }; }")]
    [InlineData("switch (value) { default: const f = () => { break; }; }")]
    [InlineData("const f = () => { label: { continue label; } };")]
    [InlineData("const f = () => { while (ready) { const g = () => { break; }; } };")]
    [InlineData("const f = () => { with (object) {} };")]
    [InlineData("const f = () => { throw\nerror; };")]
    [InlineData("const f = () => { return; } ; return;")]
    public void Arrow_functions_establish_fresh_function_scopes_and_control_flow_boundaries(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("const f = value => { 'use strict'; return value; };")]
    [InlineData("const f = () => { 'use strict'; };")]
    [InlineData("const f = (value = 1) => { 'ordinary directive'; return value; };")]
    [InlineData("const f = (value = 1) => { 'use\\x20strict'; return value; };")]
    [InlineData("const f = (value = 1) => { ; 'use strict'; return value; };")]
    [InlineData("const f = (value = 1) => { 0; 'use strict'; return value; };")]
    [InlineData("const f = (value = 1) => { ('use strict'); return value; };")]
    [InlineData("const f = (value = 1) => { 'use strict' + ''; return value; };")]
    [InlineData("const f = (value = 1) => { { 'use strict'; } return value; };")]
    [InlineData("'use strict'; const f = (value = 1) => value;")]
    [InlineData("const f = (value = () => { 'use strict'; }) => value;")]
    [InlineData("const f = (value = 1) => () => { 'use strict'; };")]
    public void Non_simple_parameters_only_restrict_a_direct_use_strict_directive(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const f = (value = 1) => { 'use strict'; };")]
    [InlineData("const f = ({value}) => { 'use strict'; };")]
    [InlineData("const f = ([value]) => { 'use strict'; };")]
    [InlineData("const f = (...rest) => { 'use strict'; };")]
    [InlineData("const f = (value = 1) => { 'ordinary'; 'use strict'; };")]
    [InlineData("const f = (value = 1) => { \"use strict\"; };")]
    [InlineData("const f = (value = 1) => { 'use strict'\nreturn value; };")]
    public void Use_strict_in_a_body_directive_prologue_rejects_non_simple_arrow_parameters(string input)
        => _fixture.Invalid(input);

    public static IEnumerable<object[]> ReservedParameters()
    {
        foreach (string name in new[] { "eval", "arguments", "let", "yield", "static", "implements",
                     "interface", "package", "private", "protected", "public", "enum" })
        foreach (string form in new[] { "const f = {0} => 1;", "const f = ({0}) => 1;",
                     "const f = ([{0}]) => 1;", "const f = ({{name: {0}}}) => 1;",
                     "const f = (...{0}) => 1;" })
            yield return new object[] { string.Format(form, name) };
    }

    [Theory]
    [MemberData(nameof(ReservedParameters))]
    public void Strict_reserved_names_are_invalid_in_each_arrow_binding_position(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("const f = (eval) => 0;", "eval")]
    [InlineData("const f = ({name: arguments}) => 0;", "arguments")]
    [InlineData("const f = (...eval) => 0;", "eval")]
    [InlineData("const f = ([\\u0065val]) => 0;", "\\u0065val")]
    public void Invalid_binding_diagnostics_preserve_the_original_parameter_source_range(string input, string identifier)
    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        var diagnostic = Assert.Single(_fixture.Language.Validate(parsed.ToParseTree),
            d => d.Code == "invalid_binding_identifier");
        Assert.Equal(input.IndexOf(identifier, StringComparison.Ordinal), diagnostic.SourceOffset);
        Assert.Equal(identifier.Length, diagnostic.Length);
        Assert.Equal(identifier, input.Substring(diagnostic.SourceOffset, diagnostic.Length));
    }

    [Theory]
    [InlineData("const f = (same, same) => 0;", "same")]
    [InlineData("const f = ([same], {same}) => 0;", "same")]
    [InlineData("const f = ({a: same, b: same}) => 0;", "same")]
    [InlineData("const f = (name, \\u006eame) => 0;", "\\u006eame")]
    public void Duplicate_parameter_diagnostics_identify_the_second_bound_name(string input, string identifier)
    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        var diagnostic = Assert.Single(_fixture.Language.Validate(parsed.ToParseTree),
            d => d.Code == "duplicate_parameter");
        Assert.Equal(input.LastIndexOf(identifier, StringComparison.Ordinal), diagnostic.SourceOffset);
        Assert.Equal(identifier.Length, diagnostic.Length);
    }

    [Fact]
    public void Validation_and_parsing_keep_arrow_state_isolated_and_preserve_the_concrete_tree()
    {
        const string input = "// header\nconst f = ({name = `hello ${value}`}, ...rest) => { return () => name; };";
        var tree = _fixture.Success(input);
        var tokens = tree.AllTokens.ToArray();
        string text = JavaScriptScriptFixture.Text(tree);
        Assert.Empty(_fixture.Language.Validate(tree));
        Assert.Empty(JavaScriptExpressionValidation.Validate(tree));
        _fixture.Invalid("const invalid = (same, same) => same;");
        _fixture.Invalid("const invalid = value\n=> value;");
        _fixture.Success("const g = () => /a/; const h = () => { return 1; };");
        var again = _fixture.Success(input);
        Assert.NotSame(tree, again);
        Assert.Equal(text, JavaScriptScriptFixture.Text(again));
        Assert.Equal(tokens, tree.AllTokens.ToArray());
        Assert.NotSame(tokens[0], again.AllTokens[0]);
        Assert.Empty(_fixture.Language.Validate(tree));
    }

    [Fact]
    public void Return_newline_inside_an_arrow_body_keeps_the_following_expression_separate()
    {
        var tree = _fixture.Success("const f = () => { return\nvalue; };");
        var returned = Assert.Single(JavaScriptScriptFixture.Nodes(tree),
            n => n.SignPost.Name == "ReturnStatement");
        Assert.DoesNotContain(returned.AllTokens, t => t.Input == "value");
        Assert.Contains(JavaScriptScriptFixture.Nodes(tree),
            n => n.SignPost.Name == "ExpressionStatement" && n.AllTokens.Any(t => t.Input == "value"));
    }

    [Fact]
    public void An_unparenthesized_nested_arrow_is_inside_the_outer_arrow_body()
    {
        var tree = _fixture.Success("const f = first => second => first + second;");
        var arrows = JavaScriptScriptFixture.Nodes(tree)
            .Where(n => n.SignPost.Name == "ArrowFunction").ToArray();
        Assert.Equal(2, arrows.Length);
        var outer = Assert.Single(arrows, n => n.AllTokens.First().Input == "first");
        var inner = Assert.Single(arrows, n => n.AllTokens.First().Input == "second");
        Assert.Contains(JavaScriptScriptFixture.Nodes(outer), n => ReferenceEquals(n, inner));
        Assert.DoesNotContain(JavaScriptScriptFixture.Nodes(inner), n => ReferenceEquals(n, outer));
    }

    [Fact]
    public void An_object_return_has_a_parenthesized_object_while_a_bare_brace_opens_a_function_body()
    {
        var tree = _fixture.Success("const block = () => { value: 1; }; const object = () => ({value: 1});");
        var nodes = JavaScriptScriptFixture.Nodes(tree).ToArray();
        Assert.Equal(2, nodes.Count(n => n.SignPost.Name == "ArrowFunction"));
        Assert.Single(nodes, n => n.SignPost.Name == "FunctionBody");
        Assert.Single(nodes, n => n.SignPost.Name == "ObjectLiteral");
        Assert.Single(nodes, n => n.SignPost.Name == "LabelledStatement");
        var body = Assert.Single(nodes, n => n.SignPost.Name == "FunctionBody");
        Assert.DoesNotContain(JavaScriptScriptFixture.Nodes(body), n => n.SignPost.Name == "ObjectLiteral");
    }

    [Fact]
    public void Public_static_validation_and_the_language_factory_report_the_same_arrow_errors()
    {
        foreach (var example in new[]
                 {
                     (Input: "const f = (same, same) => 0;", Code: "duplicate_parameter"),
                     (Input: "const f = (eval) => 0;", Code: "invalid_binding_identifier"),
                     (Input: "const f = (value = 1) => { 'use strict'; };", Code: "non_simple_strict_directive"),
                     (Input: "const f = value => { let value; };", Code: "conflicting_declaration")
                 })
        {
            var parsed = _fixture.Parse(example.Input);
            Assert.True(parsed.Success, example.Input + Environment.NewLine + _fixture.Parser.FiredException);
            var direct = JavaScriptExpressionValidation.Validate(parsed.ToParseTree);
            var throughFactory = _fixture.Language.Validate(parsed.ToParseTree);
            Assert.Contains(direct, diagnostic => diagnostic.Code == example.Code);
            Assert.Equal(throughFactory.Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray(),
                direct.Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray());
        }
    }

    [Fact]
    public async Task Shared_language_keeps_parallel_parser_sources_and_slash_contexts_independent()
    {
        var language = _fixture.Language;
        // Construct each mutable parser serially, then give each worker exclusive
        // ownership of one parser. Only the language and immutable policy are shared.
        var parsers = Enumerable.Range(0, 4).Select(_ => new LALRParser(language.Grammar, false)).ToArray();
        var examples = new[]
        {
            (Input: "const f = value => value / divisor; const g = () => /a/.test(value);",
                Arrows: 2, Statements: 0, Regex: 1, TemplateHeads: 0),
            (Input: "const f = () => {}\n/a/.test(value);",
                Arrows: 1, Statements: 1, Regex: 1, TemplateHeads: 0),
            (Input: "const f = ({value = `default ${name}`}) => `outer ${() => ({value})}`;",
                Arrows: 2, Statements: 0, Regex: 0, TemplateHeads: 2),
            (Input: "const f = () => { return\n(value) => value; };\nconst g = (x = /a/) => x / divisor;",
                Arrows: 3, Statements: 1, Regex: 1, TemplateHeads: 0)
        };
        const int rounds = 8;
        var firstTokens = new object[parsers.Length, rounds];
        var workers = Enumerable.Range(0, parsers.Length).Select(worker => Task.Run(() =>
        {
            var parser = parsers[worker];
            object previousSource = null;
            for (int round = 0; round < rounds; round++)
            {
                var example = examples[(worker + round) % examples.Length];
                var source = language.CreateTokenSource(example.Input);
                Assert.Same(language.Policy, source.Policy);
                Assert.NotSame(previousSource, source.Start.SourceIdentity);
                previousSource = source.Start.SourceIdentity;
                var parsed = parser.Parsing(source);
                Assert.True(parsed.Success, example.Input + Environment.NewLine + parser.FiredException);
                Assert.Empty(parsed.AllErrors);
                Assert.Empty(language.Validate(parsed.ToParseTree));
                var nodes = JavaScriptScriptFixture.Nodes(parsed.ToParseTree).ToArray();
                Assert.Equal(example.Arrows, nodes.Count(n => n.SignPost.Name == "ArrowFunction"));
                Assert.Equal(example.Statements, nodes.Count(n => n.SignPost.Name == "ExpressionStatement"));
                Assert.Equal(example.Regex, parsed.ToParseTree.AllTokens.Count(
                    token => token.Kind.Caption == "RegularExpressionLiteral"));
                Assert.Equal(example.TemplateHeads, parsed.ToParseTree.AllTokens.Count(
                    token => token.Kind.Caption == "TemplateHead"));
                firstTokens[worker, round] = parsed.ToParseTree.AllTokens[0];
            }
        })).ToArray();
        await Task.WhenAll(workers);

        // Every sample starts with the same textual token, const. Its object identity
        // must still be unique across separate sources and repeated parses.
        var tokens = firstTokens.Cast<object>().ToArray();
        Assert.All(tokens, token => Assert.NotNull(token));
        for (int first = 0; first < tokens.Length; first++)
        for (int second = first + 1; second < tokens.Length; second++)
            Assert.NotSame(tokens[first], tokens[second]);
    }

    [Fact]
    public void Speculative_division_and_regex_reads_do_not_change_the_following_real_arrow_parse()
    {
        const string input = "/a/g.test(value); const f = () => { return /b/; };";
        var source = _fixture.Language.CreateTokenSource(input);
        var regex = source.Policy.Goals.Single(goal =>
            goal.Terminals.Any(terminal => terminal.Caption == "RegularExpressionLiteral") &&
            !goal.Terminals.Any(terminal => terminal.Caption == "TemplateTail"));
        var division = source.Policy.Goals.Single(goal =>
            !goal.Terminals.Any(terminal => terminal.Caption == "RegularExpressionLiteral") &&
            !goal.Terminals.Any(terminal => terminal.Caption == "TemplateTail"));
        var initial = source.Start;
        var asDivision = source.Peek(initial, division);
        var asRegex = source.Peek(initial, regex);
        Assert.Equal("/", asDivision.Token.Input);
        Assert.Equal("/a/g", asRegex.Token.Input);
        Assert.NotSame(asDivision.Token, asRegex.Token);
        Assert.NotEqual(asDivision.Next.SourceOffset, asRegex.Next.SourceOffset);
        Assert.Equal("a", source.Peek(asDivision.Next, division).Token.Input);
        Assert.Equal(".", source.Peek(asRegex.Next, division).Token.Input);
        Assert.Equal("/a/g", source.Peek(initial, regex).Token.Input);
        Assert.Equal(0, initial.SourceOffset);
        Assert.Equal(0, initial.TokenIndex);
        Assert.Null(initial.PreviousToken);

        var parsed = _fixture.Parser.Parsing(source);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        Assert.Empty(_fixture.Language.Validate(parsed.ToParseTree));
        Assert.Equal(new[] { "/a/g", "/b/" }, parsed.ToParseTree.AllTokens
            .Where(token => token.Kind.Caption == "RegularExpressionLiteral").Select(token => token.Input).ToArray());
        Assert.Single(JavaScriptScriptFixture.Nodes(parsed.ToParseTree), n => n.SignPost.Name == "ArrowFunction");
        Assert.Single(JavaScriptScriptFixture.Nodes(parsed.ToParseTree), n => n.SignPost.Name == "ExpressionStatement");
    }

    [Theory]
    [InlineData("const f = () => {}\n(value);")]
    [InlineData("const f = () => {}\n[value];")]
    [InlineData("const f = () => {}\n`value`;")]
    [InlineData("const f = () => {}\n/a/.test(value);")]
    public void A_new_statement_after_an_arrow_block_is_not_a_call_member_template_or_division_suffix(string input)
    {
        var tree = _fixture.Success(input);
        Assert.Single(JavaScriptScriptFixture.Nodes(tree), n => n.SignPost.Name == "ExpressionStatement");
        var arrow = Assert.Single(JavaScriptScriptFixture.Nodes(tree), n => n.SignPost.Name == "ArrowFunction");
        Assert.Equal(new[] { "(", ")", "=>", "{", "}" }, arrow.AllTokens.Select(t => t.Input).ToArray());
    }
}

[Collection(JavaScriptExpressionCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptArrows")]
public sealed class JavaScriptArrowExpressionTests
{
    private readonly JavaScriptExpressionFixture _fixture;
    public JavaScriptArrowExpressionTests(JavaScriptExpressionFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("value => value")]
    [InlineData("() => 1")]
    [InlineData("(first, second,) => first + second")]
    [InlineData("({name = fallback}, [first] = values, ...rest) => name")]
    [InlineData("first => second => first + second")]
    [InlineData("() => ({value: 1})")]
    [InlineData("() => { return 1; }")]
    [InlineData("() => { function f() { return 1; } return f(); }")]
    [InlineData("(value => value)(input)")]
    [InlineData("[value => value, () => 1]")]
    [InlineData("({callback: value => value})")]
    [InlineData("function f(value = () => 1) { return value; }")]
    [InlineData("() => /a/.test(value)")]
    [InlineData("value => value / divisor")]
    [InlineData("value => `hello ${value}`")]
    [InlineData("() => tag`bad\\xZ`")]
    [InlineData("await => await")]
    [InlineData("async => async")]
    [InlineData("value => value, other")]
    [InlineData("(value => value) / divisor")]
    public void The_standalone_expression_entry_accepts_arrows_and_their_function_bodies(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("value\n=> value")]
    [InlineData("(value) /* newline\n */ => value")]
    [InlineData("((value)) => value")]
    [InlineData("(object.value) => value")]
    [InlineData("(same, same) => same")]
    [InlineData("({value: object.value}) => value")]
    [InlineData("(...rest,) => rest")]
    [InlineData("eval => eval")]
    [InlineData("(arguments) => arguments")]
    [InlineData("(value = 1) => { 'use strict'; }")]
    [InlineData("value || other => other")]
    [InlineData("() => { break; }")]
    [InlineData("() => { continue; }")]
    [InlineData("() => `bad\\xZ`")]
    [InlineData("() => ({value = 1})")]
    [InlineData("() => { return 1; }; next()")]
    public void The_expression_entry_runs_the_same_arrow_binding_and_early_error_checks(string input)
        => _fixture.Invalid(input);

    [Fact]
    public void A_comma_after_an_expression_body_stays_outside_the_arrow_function()
    {
        var tree = _fixture.Success("value => first, other");
        var arrow = Assert.Single(JavaScriptExpressionFixture.Nodes(tree), n => n.SignPost.Name == "ArrowFunction");
        Assert.Equal(new[] { "value", "=>", "first" }, arrow.AllTokens.Select(t => t.Input).ToArray());
        Assert.Contains(tree.AllTokens, t => t.Input == "other");
    }
}
