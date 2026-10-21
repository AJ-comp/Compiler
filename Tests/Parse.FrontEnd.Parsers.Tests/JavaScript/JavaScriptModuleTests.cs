using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMAScript 2026 Module grammar and static semantics. These tests parse syntax
// only: imports are never resolved, linked, evaluated or downloaded.
[CollectionDefinition(Name)]
public sealed class JavaScriptModuleCollection : ICollectionFixture<JavaScriptModuleFixture>
{
    public const string Name = "ECMAScript Module grammar";
}

public sealed class JavaScriptModuleFixture
{
    public JavaScriptModuleGrammar Language { get; } = new JavaScriptModuleGrammar();
    public LALRParser Parser { get; }

    public JavaScriptModuleFixture() => Parser = new LALRParser(Language.Grammar, false);

    public ParsingResult Parse(string input) => Parser.Parsing(Language.CreateTokenSource(input));

    public ParseTreeSymbol Success(string input)
    {
        var result = Parse(input);
        Assert.True(result.Success, input + Environment.NewLine + Parser.FiredException);
        Assert.Empty(result.AllErrors);
        Assert.NotNull(result.ToParseTree);
        var diagnostics = Language.Validate(result.ToParseTree);
        Assert.True(diagnostics.Count == 0, input + Environment.NewLine +
            string.Join(Environment.NewLine, diagnostics.Select(d => d.Code + ": " + d.Message)));
        return result.ToParseTree;
    }

    public void Invalid(string input)
    {
        var result = Parse(input);
        if (!result.Success)
        {
            Assert.NotEmpty(result.AllErrors);
            return;
        }
        var diagnostics = Language.Validate(result.ToParseTree);
        Assert.True(diagnostics.Count > 0, "Unexpectedly accepted: " + input);
        Assert.All(diagnostics, d =>
        {
            Assert.False(string.IsNullOrWhiteSpace(d.Code));
            Assert.False(string.IsNullOrWhiteSpace(d.Message));
            Assert.InRange(d.SourceOffset, 0, input.Length);
            Assert.InRange(d.Length, 0, input.Length - d.SourceOffset);
        });
    }
}

[Collection(JavaScriptModuleCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptModules")]
public sealed class JavaScriptModuleTests
{
    private readonly JavaScriptModuleFixture _fixture;
    public JavaScriptModuleTests(JavaScriptModuleFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("import 'module';")]
    [InlineData("import \"\";")]
    [InlineData("import Default from 'module';")]
    [InlineData("import * as ns from 'module';")]
    [InlineData("import {} from 'module';")]
    [InlineData("import {value} from 'module';")]
    [InlineData("import {value, other} from 'module';")]
    [InlineData("import {value,} from 'module';")]
    [InlineData("import {value as local} from 'module';")]
    [InlineData("import Default, {value as local} from 'module';")]
    [InlineData("import Default, * as ns from 'module';")]
    [InlineData("import {default as local} from 'module';")]
    [InlineData("import {if as condition, null as nothing, true as yes} from 'module';")]
    [InlineData("import {'not-an-identifier' as local} from 'module';")]
    [InlineData("import {'👻' as ghost} from 'module';")]
    [InlineData("import {'a\\u0062' as local} from 'module';")]
    [InlineData("import {\\u0076alue as local} from 'module';")]
    [InlineData("import {value as \\u006cocal} from 'module';")]
    [InlineData("import {as, from, of, async, meta} from 'module';")]
    [InlineData("import Default from 'one'; import Other from 'two';")]
    [InlineData("import value from 'module'; value();")]
    [InlineData("import value from 'module'; function f() { return value; }")]
    [InlineData("import value from 'module'; { let value; }")]
    [InlineData("import value from 'module'; function f(value) { return value; }")]
    [InlineData("import Default, {default as other} from 'module';")]
    [InlineData("import /* head */ { /* item */ value /* alias */ as local, } /* source */ from /* module */ 'module';")]
    [InlineData("import\n{value}\nfrom\n'module'\nnext();")]
    [InlineData("import 'first'\nimport 'second'\nconst value = 1")]
    [InlineData("import '\\uD800';")]
    [InlineData("import 'module' with {type: '\\uD800'};")]
    [InlineData("import {'\\uD83D\\uDC7B' as ghost} from 'module';")]
    [InlineData("import {await as value} from 'module';")]
    public void Static_import_forms_accept_named_namespace_default_and_side_effect_bindings(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("import data from 'data' with {type: 'json'};")]
    [InlineData("import 'module' with {};")]
    [InlineData("import {} from 'module' with {type: 'json',};")]
    [InlineData("import * as ns from 'module' with {'type': 'json'};")]
    [InlineData("import data from 'module' with {type: 'json', mode: 'strict'};")]
    [InlineData("import data from 'module' with {if: 'value', 'not-an-identifier': 'value'};")]
    [InlineData("import data from 'module' with {\\u0074ype: 'json'};")]
    [InlineData("import data from 'module' with {type: 'json', Type: 'other'};")]
    [InlineData("import 'module' with\n{\n type: 'json'\n}\nnext();")]
    [InlineData("import 'module'\nwith {type: 'json'};")]
    [InlineData("export * from 'module' with {type: 'json'};")]
    [InlineData("export * as ns from 'module' with {type: 'json'};")]
    [InlineData("export {value as local} from 'module' with {type: 'json'};")]
    [InlineData("export {} from 'module' with {};")]
    public void Import_attributes_accept_literal_string_values_and_unescaped_distinct_keys(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("export {};")]
    [InlineData("export var value;")]
    [InlineData("export var first = 1, second = 2;")]
    [InlineData("export let value;")]
    [InlineData("export const value = 1;")]
    [InlineData("export const [first,,...rest] = source;")]
    [InlineData("export const {name: local, ...rest} = source;")]
    [InlineData("export function f() { return 1; }")]
    [InlineData("export class A { #value = /a/; method() { return this.#value; } }")]
    [InlineData("const value = 1; export {value};")]
    [InlineData("export {value}; const value = 1;")]
    [InlineData("export {value as renamed}; let value;")]
    [InlineData("export {value as default}; var value;")]
    [InlineData("export {f}; function f() {}")]
    [InlineData("export {A}; class A {}")]
    [InlineData("var value; export {value, value as other};")]
    [InlineData("const value = 1; export {value as 'not-an-identifier'};")]
    [InlineData("const value = 1; export {value as '👻'};")]
    [InlineData("const value = 1; export {value as if, value as null};")]
    [InlineData("import {value} from 'module'; export {value};")]
    [InlineData("import Default from 'module'; export {Default as default};")]
    [InlineData("export {value as other} from 'module';")]
    [InlineData("export {default} from 'module';")]
    [InlineData("export {default as other} from 'module';")]
    [InlineData("export {'not-an-identifier' as other} from 'module';")]
    [InlineData("export {'one' as 'two'} from 'module';")]
    [InlineData("export {if as null} from 'module';")]
    [InlineData("export {} from 'module';")]
    [InlineData("export * from 'module';")]
    [InlineData("export * as ns from 'module';")]
    [InlineData("export * as default from 'module';")]
    [InlineData("export * as 'not-an-identifier' from 'module';")]
    [InlineData("export default 1;")]
    [InlineData("export default value + 1;")]
    [InlineData("export default function () {};")]
    [InlineData("export default function named() {} export {named};")]
    [InlineData("export default class {};")]
    [InlineData("export default class Named extends Base {} export {Named};")]
    [InlineData("export default class extends Base { constructor() { super(); } };")]
    [InlineData("export default value => value;")]
    [InlineData("export default () => { return /a/; };")]
    [InlineData("export default {value: 1};")]
    [InlineData("export default /a/;")]
    [InlineData("export default `hello ${value}`;")]
    [InlineData("export default (first, second);")]
    [InlineData("export default\nfunction () {}")]
    [InlineData("export {value as 'a\\u0062'}; const value = 1;")]
    [InlineData("export {value}; for (var value of values) {}")]
    [InlineData("{ var value; } export {value};")]
    [InlineData("export const value = 1; export {value as other};")]
    [InlineData("export * from 'one'; export * from 'one';")]
    [InlineData("export * from 'one'; export * from 'two';")]
    [InlineData("export let value; { let value; }")]
    [InlineData("export {'\\uD83D\\uDC7B' as ghost} from 'module';")]
    [InlineData("export * as '\\uD83D\\uDC7B' from 'module';")]
    [InlineData("export default function () {}(1);")]
    [InlineData("export default class {}(1);")]
    [InlineData("export default function () {} /a/;")]
    [InlineData("export default class {} /a/;")]
    [InlineData("export var value; export {value as other};")]
    public void Exports_accept_declarations_local_aliases_defaults_and_reexports(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("import('module');")]
    [InlineData("import('module', {with: {type: 'json'}});")]
    [InlineData("import('module',);")]
    [InlineData("import('module', {},);")]
    [InlineData("import(name + suffix);")]
    [InlineData("import(`module-${name}`);")]
    [InlineData("import((first, second));")]
    [InlineData("import('module').then(handler);")]
    [InlineData("import('module')?.then(handler);")]
    [InlineData("import('module').default;")]
    [InlineData("const load = () => import('module');")]
    [InlineData("const load = function() { return import('module'); };")]
    [InlineData("const first = import('one'), second = import('two');")]
    [InlineData("const options = {with: {type: 'json'}}; import(path, options);")]
    [InlineData("import('module', null);")]
    [InlineData("import('module', condition ? options : fallback);")]
    [InlineData("import('module', import.meta);")]
    [InlineData("import.meta;")]
    [InlineData("import.meta.url;")]
    [InlineData("import.meta.resolve('module');")]
    [InlineData("import.meta['url'];")]
    [InlineData("import.meta?.url;")]
    [InlineData("import.meta.url = 'replacement';")]
    [InlineData("const {url} = import.meta;")]
    [InlineData("function f() { return import.meta; }")]
    [InlineData("const f = () => import.meta;")]
    [InlineData("class A { field = import.meta; static { import.meta.url; } method() { return import.meta; } }")]
    [InlineData("`${import.meta.url} ${import('module')}`;")]
    [InlineData("new (import('module'));")]
    [InlineData("import('module') / divisor / other;")]
    [InlineData("const value = import.meta / divisor;")]
    [InlineData("import.meta();")]
    [InlineData("new import.meta;")]
    [InlineData("import.meta`tag`;")]
    [InlineData("import('module')`tag`;")]
    [InlineData("import('module').then = handler;")]
    [InlineData("const f = function() { return import.meta.url; };")]
    [InlineData("for (const key in import.meta) ;")]
    public void Dynamic_import_and_meta_use_expression_grammar_with_module_context(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("await task;")]
    [InlineData("await import('module');")]
    [InlineData("const value = await task;")]
    [InlineData("export default await task;")]
    [InlineData("export const value = await import('module');")]
    [InlineData("await await task;")]
    [InlineData("await /a/.test(value);")]
    [InlineData("(await task) / divisor;")]
    [InlineData("await task / divisor;")]
    [InlineData("const value = (await task) ** 2;")]
    [InlineData("if (ready) await task;")]
    [InlineData("{ const result = await task; }")]
    [InlineData("for await (const value of values) { await consume(value); }")]
    [InlineData("for await (let value of values) ;")]
    [InlineData("for await (var value of values) ;")]
    [InlineData("for await (value of values) ;")]
    [InlineData("for await (target.value of values) ;")]
    [InlineData("for await (const [first, ...rest] of values) ;")]
    [InlineData("for await (const {value} of values) ;")]
    [InlineData("for await (const value of await values) ;")]
    [InlineData("for\nawait\n(const value of values) ;")]
    [InlineData("for await (async of values) ;")]
    [InlineData("for await (const value of values) { if (value) break; continue; }")]
    [InlineData("outer: for await (const value of values) { continue outer; }")]
    [InlineData("try { await task; } catch (error) { await recover(error); }")]
    [InlineData("switch (await task) { case 1: await next(); }")]
    [InlineData("class A extends (await Base) {}")]
    [InlineData("class A { [await name]() {} }")]
    [InlineData("class A { [await name] = value; }")]
    [InlineData("const value = {await: await task};")]
    [InlineData("value.await; value['await'];")]
    [InlineData("class A { await() {} await = value; }")]
    [InlineData("export {await as value} from 'module';")]
    [InlineData("export * as await from 'module';")]
    [InlineData("export default await import('module',);")]
    [InlineData("const value = `${await /a/.test(input)}`;")]
    public void Top_level_await_and_for_await_are_allowed_only_in_their_module_context(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("import;")]
    [InlineData("import value;")]
    [InlineData("import from 'module';")]
    [InlineData("import * from 'module';")]
    [InlineData("import * as ns;")]
    [InlineData("import {} 'module';")]
    [InlineData("import {value} from module;")]
    [InlineData("import {value} from `module`;")]
    [InlineData("import {value} from 1;")]
    [InlineData("import {value,,other} from 'module';")]
    [InlineData("import {value as} from 'module';")]
    [InlineData("import {value other} from 'module';")]
    [InlineData("import {value: local} from 'module';")]
    [InlineData("import {value = fallback} from 'module';")]
    [InlineData("import {...rest} from 'module';")]
    [InlineData("import Default, from 'module';")]
    [InlineData("import Default, Other from 'module';")]
    [InlineData("import Default, {} , from 'module';")]
    [InlineData("import {'value'} from 'module';")]
    [InlineData("import {default} from 'module';")]
    [InlineData("import {if} from 'module';")]
    [InlineData("import {value as if} from 'module';")]
    [InlineData("import eval from 'module';")]
    [InlineData("import arguments from 'module';")]
    [InlineData("import await from 'module';")]
    [InlineData("import {await} from 'module';")]
    [InlineData("import {value as await} from 'module';")]
    [InlineData("import * as yield from 'module';")]
    [InlineData("import * as let from 'module';")]
    [InlineData("import {value as enum} from 'module';")]
    [InlineData("{ import 'module'; }")]
    [InlineData("function f() { import 'module'; }")]
    [InlineData("if (ready) import 'module';")]
    [InlineData("const f = () => { import 'module'; };")]
    [InlineData("class A { static { import 'module'; } }")]
    [InlineData("import 'module' next();")]
    [InlineData("import {value} from 'module' const other = 1;")]
    [InlineData("import 'module' assert {type: 'json'};")]
    [InlineData("import 'module' with {type};")]
    [InlineData("import 'module' with {type: value};")]
    [InlineData("import 'module' with {type: 1};")]
    [InlineData("import 'module' with {type: `json`};")]
    [InlineData("import 'module' with {[type]: 'json'};")]
    [InlineData("import 'module' with {...options};")]
    [InlineData("import 'module' with {type: 'json', type: 'json'};")]
    [InlineData("import 'module' with {type: 'json', 'type': 'other'};")]
    [InlineData("import 'module' with {type: 'json', '\\u0074ype': 'other'};")]
    [InlineData("import 'module' with {type: 'json', \\u0074ype: 'other'};")]
    [InlineData("import 'module' with {type: 'json'} with {};")]
    public void Module_grammar_rejects_invalid_static_import_shape_and_nested_declarations(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("import value from 'one'; import value from 'two';")]
    [InlineData("import {first as value, second as value} from 'module';")]
    [InlineData("import value, {other as value} from 'module';")]
    [InlineData("import value, * as value from 'module';")]
    [InlineData("import value from 'module'; let value;")]
    [InlineData("import value from 'module'; const value = 1;")]
    [InlineData("import value from 'module'; var value;")]
    [InlineData("import value from 'module'; function value() {}")]
    [InlineData("import value from 'module'; class value {}")]
    [InlineData("import value from 'module'; { var value; }")]
    [InlineData("import value from 'module'; for (var value of values) ;")]
    [InlineData("import {x as \\u0061, y as a} from 'module';")]
    [InlineData("export {missing};")]
    [InlineData("export {missing as value};")]
    [InlineData("{ let value; } export {value};")]
    [InlineData("function f() { var value; } export {value};")]
    [InlineData("for (let value of values) ; export {value};")]
    [InlineData("export {if};")]
    [InlineData("export {'value'};")]
    [InlineData("export {'value' as other};")]
    [InlineData("export {value} with {type: 'json'}; const value = 1;")]
    [InlineData("export const value = 1; export const value = 2;")]
    [InlineData("const value = 1; export {value}; export {value};")]
    [InlineData("const value = 1; export {value, value};")]
    [InlineData("const first = 1, second = 2; export {first as value, second as value};")]
    [InlineData("const value = 1; export {value as ab, value as 'a\\u0062'};")]
    [InlineData("export default 1; export default 2;")]
    [InlineData("const value = 1; export {value as default}; export default 1;")]
    [InlineData("export * as ns from 'one'; export * as ns from 'two';")]
    [InlineData("export * as default from 'one'; export default 1;")]
    [InlineData("export {x as value} from 'one'; export {y as value} from 'two';")]
    [InlineData("export default function f() {} export {f as default};")]
    [InlineData("export default class A {} export {A as default};")]
    [InlineData("export default function () {} export {default};")]
    [InlineData("export default class {} export {default};")]
    [InlineData("export let [first, first] = values;")]
    [InlineData("export const {value: first, other: first} = source;")]
    [InlineData("import {'\\uD800' as value} from 'module';")]
    [InlineData("import {'\\uDC00' as value} from 'module';")]
    [InlineData("export {'\\uD800' as value} from 'module';")]
    [InlineData("export {value as '\\uD800'} from 'module';")]
    [InlineData("export * as '\\uDC00' from 'module';")]
    [InlineData("const value = 1; export {value as default, value as 'default'};")]
    [InlineData("const value = 1; export {value as '\\u0061', value as a};")]
    [InlineData("export * as '\\u0061' from 'one'; export * as a from 'two';")]
    public void Module_early_errors_reject_duplicate_bindings_exports_and_unresolved_local_exports(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("export;")]
    [InlineData("export value;")]
    [InlineData("export default;")]
    [InlineData("export default first, second;")]
    [InlineData("export default const value = 1;")]
    [InlineData("export default var value;")]
    [InlineData("export default let value;")]
    [InlineData("export *;")]
    [InlineData("export * 'module';")]
    [InlineData("export * as ns;")]
    [InlineData("export * as ns from module;")]
    [InlineData("export {value as};")]
    [InlineData("export {value,, other} from 'module';")]
    [InlineData("export {value: other} from 'module';")]
    [InlineData("export {...rest} from 'module';")]
    [InlineData("export {} from `module`;")]
    [InlineData("export {} from source;")]
    [InlineData("export var;")]
    [InlineData("export let;")]
    [InlineData("export const value;")]
    [InlineData("export function () {}")]
    [InlineData("export class {}")]
    [InlineData("export if (ready) value();")]
    [InlineData("export import value from 'module';")]
    [InlineData("{ export const value = 1; }")]
    [InlineData("function f() { export const value = 1; }")]
    [InlineData("const f = () => { export default 1; };")]
    [InlineData("if (ready) export default 1;")]
    [InlineData("class A { static { export const value = 1; } }")]
    [InlineData("export default 1 next();")]
    public void Module_rejects_malformed_export_forms_and_nested_exports(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("import();")]
    [InlineData("import(,);")]
    [InlineData("import(...values);")]
    [InlineData("import('module', ...options);")]
    [InlineData("import('module', {}, third);")]
    [InlineData("import('module', {},,);")]
    [InlineData("import('module',,);")]
    [InlineData("import?.('module');")]
    [InlineData("import?.meta;")]
    [InlineData("import.other;")]
    [InlineData("import['meta'];")]
    [InlineData("import.\\u006deta;")]
    [InlineData("import.meta = value;")]
    [InlineData("++import.meta;")]
    [InlineData("import.meta++;")]
    [InlineData("({value: import.meta} = source);")]
    [InlineData("new import('module');")]
    [InlineData("import`module`;")]
    public void Dynamic_import_and_import_meta_reject_invalid_arity_optional_calls_and_property_names(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("let await;")]
    [InlineData("var await;")]
    [InlineData("const await = 1;")]
    [InlineData("function await() {}")]
    [InlineData("class await {}")]
    [InlineData("function f(await) {}")]
    [InlineData("const f = await => await;")]
    [InlineData("const f = (await) => 1;")]
    [InlineData("function f() { await task; }")]
    [InlineData("function f() { await; }")]
    [InlineData("function f() { let await; }")]
    [InlineData("function f() { return await task; }")]
    [InlineData("const f = () => await task;")]
    [InlineData("const f = (value = await task) => value;")]
    [InlineData("class A { method() { await task; } }")]
    [InlineData("class A { field = await task; }")]
    [InlineData("class A { static field = await task; }")]
    [InlineData("class A { static { await task; } }")]
    [InlineData("await;")]
    [InlineData("await +;")]
    [InlineData("await task ** 2;")]
    [InlineData("for await (;;);")]
    [InlineData("for await (const value in values) ;")]
    [InlineData("for await (var value in values) ;")]
    [InlineData("for await (const value = 1 of values) ;")]
    [InlineData("for await (const value of values) { var value; }")]
    [InlineData("for await (let first, second of values) ;")]
    [InlineData("for await (value of values, other) ;")]
    [InlineData("function f() { for await (const value of values) ; }")]
    [InlineData("const f = () => { for await (const value of values) ; };")]
    [InlineData("class A { static { for await (const value of values) ; } }")]
    [InlineData("await label: ;")]
    [InlineData("await => value;")]
    [InlineData("await")]
    [InlineData("await\\n")]
    [InlineData("await \\u0061wait;")]
    [InlineData("var \\u0061wait;")]
    [InlineData("function f() { \\u0061wait; }")]
    [InlineData("await: while (ready) ;")]
    [InlineData("label: while (ready) { break await; }")]
    [InlineData("const {await} = source;")]
    [InlineData("for await (let.value of values) ;")]
    [InlineData("for await (const value of values) { function f() { await task; } }")]
    [InlineData("export default class await {}")]
    public void Await_grammar_and_scope_errors_are_detected_without_affecting_ordinary_functions(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("with (value) {}")]
    [InlineData("var eval;")]
    [InlineData("var arguments;")]
    [InlineData("let implements;")]
    [InlineData("function f(first, first) {}")]
    [InlineData("delete value;")]
    [InlineData("value = 077;")]
    [InlineData("'\\1';")]
    [InlineData("return 1;")]
    [InlineData("break;")]
    [InlineData("continue;")]
    [InlineData("new.target;")]
    [InlineData("super.value;")]
    [InlineData("let value; var value;")]
    [InlineData("function f() {} function f() {}")]
    [InlineData("function f() {} var f;")]
    [InlineData("var f; function f() {}")]
    [InlineData("class A {} function A() {}")]
    public void Module_strictness_and_top_level_scope_errors_reuse_existing_language_checks(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("import value from 'module'; value / divisor / other;")]
    [InlineData("import 'module'; /a/.test(value);")]
    [InlineData("export const value = /a/;")]
    [InlineData("export default function () {} /a/.test(value);")]
    [InlineData("export default class {} /a/.test(value);")]
    [InlineData("export default class Named {} /a/.test(value);")]
    [InlineData("export default function named() {} /a/.test(value);")]
    [InlineData("export default () => {}\n/a/.test(value);")]
    [InlineData("export default (class {}) / divisor;")]
    [InlineData("export default (function() {}) / divisor;")]
    [InlineData("export default `hello ${import.meta.url}`;")]
    [InlineData("const value = `${await task}`;")]
    [InlineData("const value = `${import('module')}`;")]
    [InlineData("export const f = ({value = 1}, ...rest) => value;")]
    [InlineData("export class A extends Base { #value; constructor() { super(); } method() { return import.meta; } }")]
    [InlineData("import value from 'module'\nexport {value}")]
    [InlineData("export default 1\nconst other = 2;")]
    [InlineData("export default\n/a/.test(value)")]
    [InlineData("export * from 'module'\n/a/.test(value);")]
    [InlineData("export {value}\nconst value = 1;")]
    [InlineData("await\n/a/.test(value);")]
    [InlineData("for await (const value of values) /a/.test(value);")]
    [InlineData("export default function () { return import.meta; }")]
    [InlineData("export default class { static { import('module'); } }")]
    [InlineData("export default function named() {} named();")]
    [InlineData("export default class Named {} new Named();")]
    [InlineData("export default function () {}; import 'module';")]
    [InlineData("export default class {}; import 'module';")]
    public void Lexical_goals_ASI_templates_and_existing_classes_arrows_integrate_with_module_items(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const value = 1; export {value}\n/a/.test(value);")]
    [InlineData("export {}\nfrom 'module';")]
    [InlineData("export default value\n/a/g;")]
    [InlineData("import\n.meta;")]
    [InlineData("import\n('module');")]
    [InlineData("import\n'module';")]
    [InlineData("import /* comment */ . /* comment */ meta;")]
    [InlineData("for await (const [value = await task] of values) {}")]
    [InlineData("for await (const {[await task]: value} of values) {}")]
    [InlineData("const value = import('module', {with: {type: 1}});")]
    [InlineData("const value = import('module', {with: {type: null}});")]
    [InlineData("const value = import('module', {with: {type: 'json', type: 'text'}});")]
    [InlineData("export default function named() {} export {named as other};")]
    [InlineData("export default class Named {} export {Named as other};")]
    [InlineData("export {value}; try {} catch (error) { var value; }")]
    [InlineData("import {'\\u0000' as nul} from 'module'; export {nul as '\\u0000'};")]
    [InlineData("import '\\uD800' with {'\\uD800': '\\uDC00'};")]
    [InlineData("import {\\u0069f as value} from 'module';")]
    [InlineData("export {\\u0069f as value} from 'module';")]
    [InlineData("import value from 'module' with {unknown: 'anything'};")]
    [InlineData("await /a/g;")]
    [InlineData("const value = await `${import.meta.url}`;")]
    [InlineData("const value = await (await task);")]
    [InlineData("class A { [await task]() { return import.meta; } }")]
    [InlineData("const f = () => ({await: 1});")]
    [InlineData("function f() { return {await: 1}; }")]
    public void Module_boundaries_distinguish_lexical_names_attributes_and_runtime_only_import_options(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("export default value\n/a/;")]
    [InlineData("export default (function named() {}); export {named};")]
    [InlineData("export default (class Named {}); export {Named};")]
    [InlineData("export default function named() {} var named;")]
    [InlineData("export default class Named {} var Named;")]
    [InlineData("export {error}; try {} catch (error) {}")]
    [InlineData("export {value}; class A { static { var value; } }")]
    [InlineData("export {value}; class A { method() { var value; } }")]
    [InlineData("export {value}; const f = () => { var value; };")]
    [InlineData("import {value \\u0061s local} from 'module';")]
    [InlineData("import value \\u0066rom 'module';")]
    [InlineData("export {value} \\u0066rom 'module';")]
    [InlineData("export * \\u0061s ns from 'module';")]
    [InlineData("class A { method(value = await task) {} }")]
    [InlineData("class A { get value() { return await task; } }")]
    [InlineData("class A { set value(input) { await task; } }")]
    public void Module_boundaries_reject_private_local_names_escaped_syntax_words_and_forbidden_await_scopes(string input)
        => _fixture.Invalid(input);

    // A direct default class/function is a declaration. An object or a parenthesized
    // class/function remains the default expression, so its following slash is division.
    [Theory]
    [InlineData("export default {} / 2;", 1, 0)]
    [InlineData("export default {}\n/2/g;", 2, 0)]
    [InlineData("export default {}\n/a/g;", 2, 0)]
    [InlineData("export default {} /* same line */ / 2;", 1, 0)]
    [InlineData("export default {} // next line\n/a/g;", 2, 0)]
    [InlineData("export default (class {}) / 2;", 1, 0)]
    [InlineData("export default (class {})\n/a/g;", 2, 0)]
    [InlineData("export default (function() {}) / 2;", 1, 0)]
    [InlineData("export default (function() {})\n/a/g;", 2, 0)]
    [InlineData("export default (() => {}) / 2;", 1, 0)]
    [InlineData("export default {Constructor: class {}} / 2;", 1, 0)]
    [InlineData("export default {callback: function() { return /a/; }} / 2;", 1, 1)]
    [InlineData("export default {Constructor: class { value = /a/; method() { return /b/; } }} / 2;", 1, 2)]
    [InlineData("export default {callback: () => { return /a/; }} / 2;", 1, 1)]
    [InlineData("export default ({Constructor: class {}}) / 2;", 1, 0)]
    [InlineData("export default class {} /a/g;", 0, 1)]
    [InlineData("export default class Named {}\n/a/g;", 0, 1)]
    [InlineData("export default function() {} /a/g;", 0, 1)]
    [InlineData("export default function named() {}\n/a/g;", 0, 1)]
    [InlineData("export default class { value = {}; } /a/g;", 0, 1)]
    [InlineData("export default class { static value = class {}; } /a/g;", 0, 1)]
    [InlineData("export default class { method() { return {value: class {}}; } } /a/g;", 0, 1)]
    [InlineData("export default function() { return {}; } /a/g;", 0, 1)]
    [InlineData("export default function() { return class {}; } /a/g;", 0, 1)]
    [InlineData("export default class { value = /a/; } /b/g;", 0, 2)]
    [InlineData("export default function() { return /a/; } /b/g;", 0, 2)]
    public void Default_declarations_and_expressions_choose_the_correct_following_slash_goal(
        string input, int divisionTokens, int regularExpressionTokens)
    {
        var tree = _fixture.Success(input);
        Assert.Equal(divisionTokens, tree.AllTokens.Count(token => token.Input == "/"));
        Assert.Equal(regularExpressionTokens, tree.AllTokens.Count(token =>
            token.Kind.Caption == "RegularExpressionLiteral"));
    }

    [Theory]
    [InlineData("export default {} /a/;")]
    [InlineData("export default {}\n/a/;")]
    [InlineData("export default (class {}) /a/;")]
    [InlineData("export default (function() {}) /a/;")]
    [InlineData("export default {Constructor: class {}}\n/a/;")]
    [InlineData("export default class {} / 2;")]
    [InlineData("export default function() {} / 2;")]
    public void Default_expressions_require_complete_division_operands_and_declarations_require_valid_following_statements(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n")]
    [InlineData("// empty Module")]
    [InlineData("/* empty Module */")]
    [InlineData(";")]
    [InlineData("{}")]
    public void Empty_and_statement_only_sources_keep_the_Module_parse_goal(string input)
    {
        var tree = _fixture.Success(input);
        Assert.Equal("Module", ((ParseTreeNonTerminal)tree).SignPost.Name);
        Assert.Empty(JavaScriptExpressionValidation.Validate(tree));
    }

    [Fact]
    public void Public_module_factory_exposes_the_same_grammar_policy_and_edition_contract()
    {
        var language = _fixture.Language;
        Assert.Equal(2026, JavaScriptModuleGrammar.Edition);
        Assert.NotNull(language.Grammar);
        Assert.NotEmpty(language.Policy.Goals);
        Assert.Contains("ImportDeclaration", language.GrammarText);
        Assert.Contains("ExportDeclaration", language.GrammarText);
        var source = language.CreateTokenSource("export default 1;");
        Assert.Same(language.Policy, source.Policy);
        Assert.Equal(0, source.Start.SourceOffset);
        Assert.Null(source.Start.PreviousToken);
        var parsed = _fixture.Parser.Parsing(source);
        Assert.True(parsed.Success, _fixture.Parser.FiredException?.ToString());
        Assert.Empty(language.Validate(parsed.ToParseTree));
        Assert.Throws<ArgumentNullException>(() => language.Validate(null));
        Assert.Throws<ArgumentNullException>(() => language.CreateTokenSource(null));
    }

    [Theory]
    [InlineData("import value from 'one'; import value from 'two';", "value")]
    [InlineData("import {x as \\u0061, y as a} from 'module';", "a}")]
    [InlineData("export {missing};", "missing")]
    [InlineData("const value = 1; export {value}; export {value};", "value")]
    [InlineData("export default 1; export default 2;", "default")]
    [InlineData("import 'module' with {type: 'json', type: 'other'};", "type")]
    public void Module_early_error_ranges_and_public_static_validation_agree(string input, string expected)
    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        var diagnostics = _fixture.Language.Validate(parsed.ToParseTree);
        Assert.NotEmpty(diagnostics);
        int offset = input.LastIndexOf(expected, StringComparison.Ordinal);
        Assert.Contains(diagnostics, diagnostic =>
            diagnostic.SourceOffset <= offset && diagnostic.SourceOffset + diagnostic.Length > offset);
        Assert.All(diagnostics, diagnostic =>
        {
            Assert.InRange(diagnostic.SourceOffset, 0, input.Length);
            Assert.InRange(diagnostic.Length, 0, input.Length - diagnostic.SourceOffset);
        });
        Assert.Equal(diagnostics.Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray(),
            JavaScriptExpressionValidation.Validate(parsed.ToParseTree)
                .Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray());
    }

    [Fact]
    public void Module_validation_keeps_concrete_tree_tokens_and_original_source_offsets_intact()
    {
        const string input = "// header\nimport {value as local} from 'module' with {type: 'json'}; export const f = ({value = local}) => class A { #value = value; method() { return import.meta.url; } }; export default await import(`part-${local}`, {with: {type: 'json'}});";
        var tree = _fixture.Success(input);
        var tokens = tree.AllTokens.ToArray();
        string text = JavaScriptScriptFixture.Text(tree);
        var firstNodes = JavaScriptScriptFixture.Nodes(tree).ToArray();
        Assert.Empty(_fixture.Language.Validate(tree));
        Assert.Empty(JavaScriptExpressionValidation.Validate(tree));
        Assert.Contains(firstNodes, node => node.SignPost.Name is "ImportDeclaration" or "ImportDeclaration__Await");
        Assert.Contains(firstNodes, node => node.SignPost.Name is "ExportDeclaration" or "ExportDeclaration__Await");
        foreach (var token in tokens)
            Assert.Equal(token.Input, input.Substring(token.StartIndex, token.Input.Length));
        _fixture.Invalid("export {missing};");
        _fixture.Success("import other from 'other'; export {other};");
        Assert.Equal(tokens, tree.AllTokens.ToArray());
        Assert.Equal(firstNodes, JavaScriptScriptFixture.Nodes(tree).ToArray());
        Assert.Equal(text, JavaScriptScriptFixture.Text(tree));
        Assert.Empty(_fixture.Language.Validate(tree));
        var again = _fixture.Success(input);
        Assert.Equal(text, JavaScriptScriptFixture.Text(again));
        Assert.NotSame(tokens[0], again.AllTokens[0]);
    }

    [Fact]
    public void Module_division_regex_speculation_does_not_advance_import_or_await_context()
    {
        const string input = "/a/g.test(value); export default await import('module');";
        var source = _fixture.Language.CreateTokenSource(input);
        var regex = source.Policy.Goals.Single(goal =>
            goal.Terminals.Any(terminal => terminal.Caption == "RegularExpressionLiteral") &&
            !goal.Terminals.Any(terminal => terminal.Caption == "TemplateTail"));
        var division = source.Policy.Goals.Single(goal =>
            !goal.Terminals.Any(terminal => terminal.Caption == "RegularExpressionLiteral") &&
            !goal.Terminals.Any(terminal => terminal.Caption == "TemplateTail"));
        var original = source.Start;
        var asRegex = source.Peek(original, regex);
        var asDivision = source.Peek(original, division);
        Assert.Equal("/a/g", asRegex.Token.Input);
        Assert.Equal("/", asDivision.Token.Input);
        Assert.NotSame(asRegex.Token, asDivision.Token);
        Assert.Equal(".", source.Peek(asRegex.Next, division).Token.Input);
        Assert.Equal("a", source.Peek(asDivision.Next, division).Token.Input);
        Assert.Equal(0, original.SourceOffset);
        Assert.Null(original.PreviousToken);
        var parsed = _fixture.Parser.Parsing(source);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        Assert.Empty(_fixture.Language.Validate(parsed.ToParseTree));
        Assert.Equal(new[] { "/a/g" }, parsed.ToParseTree.AllTokens
            .Where(token => token.Kind.Caption == "RegularExpressionLiteral").Select(token => token.Input).ToArray());
    }

    [Fact]
    public async Task Shared_module_language_keeps_import_export_and_lexical_contexts_separate_between_parsers()
    {
        var language = _fixture.Language;
        var parsers = Enumerable.Range(0, 2).Select(_ => new LALRParser(language.Grammar, false)).ToArray();
        var examples = new[]
        {
            "import value from 'module'; export const A = class { #value = /a/; method() { return this.#value; } };",
            "export default function () {} /b/.test(value); const value = await import('module');",
            "const value = `${await task}`; export {value}; export default () => import.meta.url;"
        };
        var firstTokens = new object[parsers.Length, examples.Length];
        await Task.WhenAll(Enumerable.Range(0, parsers.Length).Select(worker => Task.Run(() =>
        {
            for (int index = 0; index < examples.Length; index++)
            {
                string input = examples[(index + worker) % examples.Length];
                var parsed = parsers[worker].Parsing(language.CreateTokenSource(input));
                Assert.True(parsed.Success, input + Environment.NewLine + parsers[worker].FiredException);
                Assert.Empty(parsed.AllErrors);
                Assert.Empty(language.Validate(parsed.ToParseTree));
                firstTokens[worker, index] = parsed.ToParseTree.AllTokens[0];
            }
        })));
        var tokens = firstTokens.Cast<object>().ToArray();
        Assert.All(tokens, token => Assert.NotNull(token));
        for (int first = 0; first < tokens.Length; first++)
        for (int second = first + 1; second < tokens.Length; second++)
            Assert.NotSame(tokens[first], tokens[second]);
    }
}

[Collection(JavaScriptScriptCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptModules")]
public sealed class JavaScriptModuleScriptIntegrationTests
{
    private readonly JavaScriptScriptFixture _fixture;
    public JavaScriptModuleScriptIntegrationTests(JavaScriptScriptFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("import('module');")]
    [InlineData("import('module',);")]
    [InlineData("import('module', {},);")]
    [InlineData("import('module', {with: {type: 'json'}});")]
    [InlineData("import(path, options);")]
    [InlineData("import(1, false);")]
    [InlineData("import('module', 1);")]
    [InlineData("import('module', null);")]
    [InlineData("import('module', value => value);")]
    [InlineData("import('module')`tag`;")]
    [InlineData("new (import('module'));")]
    [InlineData("const f = () => import('module');")]
    [InlineData("function f() { return import('module'); }")]
    [InlineData("class A { field = import('module'); static { import('module'); } }")]
    [InlineData("const value = `${import('module')}`;")]
    [InlineData("let await; await = import('module');")]
    [InlineData("const f = await => import(await);")]
    [InlineData("var from = 1, as = 2, meta = 3; from + as + meta;")]
    [InlineData("import('module') / divisor;")]
    [InlineData("import('module'); /a/.test(value);")]
    [InlineData("const value = import('module', (first, second));")]
    [InlineData("const value = import('module', /a/);")]
    public void Dynamic_import_is_valid_in_Script_without_changing_await_identifier_rules(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("import value from 'module';")]
    [InlineData("import 'module';")]
    [InlineData("export {};")]
    [InlineData("export default 1;")]
    [InlineData("import.meta;")]
    [InlineData("function f() { return import.meta; }")]
    [InlineData("const f = () => import.meta;")]
    [InlineData("class A { field = import.meta; }")]
    [InlineData("class A { static { import.meta; } }")]
    [InlineData("`${import.meta}`;")]
    [InlineData("import();")]
    [InlineData("import(...values);")]
    [InlineData("import('module', {}, third);")]
    [InlineData("import?.('module');")]
    [InlineData("new import('module');")]
    [InlineData("await import('module');")]
    [InlineData("for await (const value of values) ;")]
    public void Script_rejects_static_module_items_and_import_meta_in_all_nested_contexts(string input)
        => _fixture.Invalid(input);
}

[Collection(JavaScriptExpressionCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptModules")]
public sealed class JavaScriptModuleExpressionIntegrationTests
{
    private readonly JavaScriptExpressionFixture _fixture;
    public JavaScriptModuleExpressionIntegrationTests(JavaScriptExpressionFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("import('module')")]
    [InlineData("import('module',)")]
    [InlineData("import('module', {},)")]
    [InlineData("import('module', {with: {type: 'json'}})")]
    [InlineData("import(path, options)")]
    [InlineData("import(1, false)")]
    [InlineData("import('module', null)")]
    [InlineData("import('module', true)")]
    [InlineData("import((first, second))")]
    [InlineData("import('module').then(handler)")]
    [InlineData("import('module')?.value")]
    [InlineData("import('module')`tag`")]
    [InlineData("new (import('module'))")]
    [InlineData("import('module') / divisor")]
    [InlineData("() => import('module')")]
    [InlineData("function() { return import('module'); }")]
    [InlineData("class { method() { return import('module'); } }")]
    [InlineData("`${import('module')}`")]
    [InlineData("[import('one'), import('two')]")]
    [InlineData("({value: import('module')})")]
    [InlineData("import('module', options = value)")]
    [InlineData("import('module', (first, second))")]
    [InlineData("import('module', () => /a/)")]
    public void Expression_entry_accepts_dynamic_imports_and_chains_as_ordinary_expressions(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("import.meta")]
    [InlineData("import.meta.url")]
    [InlineData("function() { return import.meta; }")]
    [InlineData("() => import.meta")]
    [InlineData("class { field = import.meta; }")]
    [InlineData("`${import.meta}`")]
    [InlineData("import value from 'module'")]
    [InlineData("export default 1")]
    [InlineData("await import('module')")]
    [InlineData("import()")]
    [InlineData("import(...values)")]
    [InlineData("import('module', ...values)")]
    [InlineData("import('module', {}, third)")]
    [InlineData("import('module',,)")]
    [InlineData("import?.('module')")]
    [InlineData("new import('module')")]
    [InlineData("import.other")]
    [InlineData("import`module`")]
    [InlineData("import('module'); next()")]
    public void Expression_entry_preserves_its_Script_goal_and_rejects_module_only_syntax(string input)
        => _fixture.Invalid(input);
}
