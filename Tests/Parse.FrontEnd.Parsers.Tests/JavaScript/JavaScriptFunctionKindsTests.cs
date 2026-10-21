using System;
using System.Linq;
using System.Threading.Tasks;
using Janglim.FrontEnd.Grammars.JavaScript;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMAScript 2026 sections 13.3.9, 13.3.14, 13.4, 14.7.5 and 15.4-15.8.
// Each fixture constructs its grammar/table once. Tests only parse and validate:
// user function bodies, getters, promises, iterators and imports are never run.
// Corpus expectations were independently compiled with Node.js 24.12.0 without
// evaluation. Published Await parameters take precedence over known host-engine
// differences in class fields; those cases are kept in a separate theory below.
// https://tc39.es/ecma262/2026/multipage/ecmascript-language-functions-and-classes.html

[Collection(JavaScriptScriptCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptFunctionKinds")]
public sealed class JavaScriptFunctionKindsScriptTests
{
    private readonly JavaScriptScriptFixture _fixture;
    public JavaScriptFunctionKindsScriptTests(JavaScriptScriptFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("async function f() {}")]
    [InlineData("async function f() { return await work(); }")]
    [InlineData("async function f() { await first; await second; }")]
    [InlineData("async function f() { return await await work(); }")]
    [InlineData("async function f() { return 1 + await value * 2; }")]
    [InlineData("async function f() { return typeof await value; }")]
    [InlineData("async function f() { return (await value).field; }")]
    [InlineData("async function f() { return (await factory)(); }")]
    [InlineData("async function f() { await /a/.test(value); return value / divisor; }")]
    [InlineData("async function f() { return `${await first}:${await second}`; }")]
    [InlineData("async function f() { return tag`value ${await task}`; }")]
    [InlineData("async function f() { return await import('module'); }")]
    [InlineData("async function f({value}, [first] = list, ...rest) { return await value; }")]
    [InlineData("async function f({[key]: value = fallback} = source) { return value; }")]
    [InlineData("async function f(value = () => await) { return value; }")]
    [InlineData("async function f(value = function(await) { return await; }) { return value; }")]
    [InlineData("async function f() { function inner(await) { return await; } return inner; }")]
    [InlineData("async function f() { const inner = () => await; return inner; }")]
    [InlineData("async function f() { const inner = async () => await value; return inner; }")]
    [InlineData("async function f() { function* inner() { yield value; } return inner; }")]
    [InlineData("async function f() { async function inner() { await value; } return inner; }")]
    [InlineData("async function f() { return new.target; }")]
    [InlineData("const f = async function(value) { return await value; };")]
    [InlineData("const f = async function named(value) { return await value; };")]
    [InlineData("const f = async /* same line */ function named() {};")]
    [InlineData("async function f() { try { return await work(); } catch ({message}) { throw message; } }")]
    [InlineData("async function f() { for (let i = await first; i < await last; i++) await step(i); }")]
    [InlineData("async function f() { for (const value of await source) { await consume(value); } }")]
    [InlineData("async function f() { return\nvalue; }")]
    [InlineData("async function f() { throw await failure; }")]
    [InlineData("async function f() { await\nwork(); }")]
    [InlineData("async function f() { await/*\n*/work(); }")]
    public void Async_functions_accept_await_and_preserve_nested_function_boundaries(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("async function f(await) {}")]
    [InlineData("async function f(\\u0061wait) {}")]
    [InlineData("async function f({await}) {}")]
    [InlineData("async function f([await]) {}")]
    [InlineData("async function f(...await) {}")]
    [InlineData("async function f(value = await task) {}")]
    [InlineData("async function f(value = (await task)) {}")]
    [InlineData("async function f({[await task]: value}) {}")]
    [InlineData("async function f(value = `${await task}`) {}")]
    [InlineData("async function f() { let await; }")]
    [InlineData("async function f() { var await; }")]
    [InlineData("async function f() { function await() {} }")]
    [InlineData("async function f() { class await {} }")]
    [InlineData("async function f() { try {} catch (await) {} }")]
    [InlineData("async function f() { await: while (true) break await; }")]
    [InlineData("async function f() { await; }")]
    [InlineData("async function f() { await * values; }")]
    [InlineData("async function f() { await value ** 2; }")]
    [InlineData("async function f() { yield value; }")]
    [InlineData("function f() { await value; }")]
    [InlineData("async function f() { function inner() { await value; } }")]
    [InlineData("async function f() { const inner = () => await value; }")]
    [InlineData("async\nfunction f() { await value; }")]
    [InlineData("async/*\n*/function f() { await value; }")]
    [InlineData("async function f(first, first) {}")]
    [InlineData("async function f({value}, value) {}")]
    [InlineData("async function f(value = 1) { 'use strict'; }")]
    [InlineData("async function f(value) { let value; }")]
    [InlineData("async function f(eval) {}")]
    [InlineData("async function f(arguments) {}")]
    public void Async_parameters_and_body_context_reject_reserved_await_and_illegal_suspension(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("const f = async value => await value;")]
    [InlineData("const f = async () => await work();")]
    [InlineData("const f = async (first, second,) => await first + await second;")]
    [InlineData("const f = async ({value}, [first] = list, ...rest) => await value;")]
    [InlineData("const f = async (...[first, ...rest]) => first;")]
    [InlineData("const f = async ({value = fallback} = source) => value;")]
    [InlineData("const f = async value => ({value: await value});")]
    [InlineData("const f = async value => { return await value; };")]
    [InlineData("const f = async () => {};")]
    [InlineData("const f = async () => async value => await value;")]
    [InlineData("const f = async (value = async () => await task) => value;")]
    [InlineData("const f = async (value = function(await) { return await; }) => value;")]
    [InlineData("const f = async (value = () => await) => value;")]
    [InlineData("const f = async /* same line */ value => await value;")]
    [InlineData("const f = async /* same line */ (value) => await value;")]
    [InlineData("const f = async (value\n) => await value;")]
    [InlineData("const f = async value =>\nawait value;")]
    [InlineData("const f = async () => /a/;")]
    [InlineData("const f = async () => await /a/.test(value);")]
    [InlineData("const f = async () => await value / divisor;")]
    [InlineData("const f = async () => `${await value}`;")]
    [InlineData("const f = async () => { for await (const value of stream) await consume(value); };")]
    [InlineData("function outer() { return async () => new.target; }")]
    [InlineData("class A extends Base { method() { return async () => super.value; } }")]
    [InlineData("const f = async async => async;")]
    [InlineData("const f = async (of, get, set) => await of + get + set;")]
    public void Async_arrows_accept_both_parameter_forms_and_concise_or_block_bodies(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const f = async (value = await => await) => value;")]
    [InlineData("const f = async await => await;")]
    [InlineData("const f = async (await) => await;")]
    [InlineData("const f = async ({await}) => 1;")]
    [InlineData("const f = async (value = await task) => value;")]
    [InlineData("const f = async ({[await task]: value}) => value;")]
    [InlineData("const f = async (value = `${await task}`) => value;")]
    [InlineData("const f = async (first, first) => first;")]
    [InlineData("const f = async ({value}, value) => value;")]
    [InlineData("const f = async (value = 1) => { 'use strict'; };")]
    [InlineData("const f = async (value) => { let value; };")]
    [InlineData("const f = async (...rest,) => rest;")]
    [InlineData("const f = async (first + second) => first;")]
    [InlineData("const f = async (first, (second)) => first;")]
    [InlineData("const f = async\nvalue => await value;")]
    [InlineData("const f = async\n() => await value;")]
    [InlineData("const f = async/*\n*/(value) => await value;")]
    [InlineData("const f = async (value)\n=> await value;")]
    [InlineData("const f = async value\n=> await value;")]
    [InlineData("const f = async () => { yield value; };")]
    [InlineData("const f = async () => { await; };")]
    [InlineData("const f = async () => { function inner() { await value; } };")]
    [InlineData("const f = async () => super.value;")]
    [InlineData("const f = async () => new.target;")]
    [InlineData("const f = new async value => value;")]
    [InlineData("const f = async * value => value;")]
    [InlineData("const f = async (eval) => eval;")]
    public void Async_arrows_enforce_line_terminators_parameters_and_lexical_context(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("function* f() {}")]
    [InlineData("function * f() { yield; }")]
    [InlineData("function* f() { yield value; }")]
    [InlineData("function* f() { yield* values; }")]
    [InlineData("function* f() { yield * values; }")]
    [InlineData("function* f() { yield*\nvalues; }")]
    [InlineData("function* f() { return yield value; }")]
    [InlineData("function* f() { return (yield value) + (yield other); }")]
    [InlineData("function* f() { yield first = second; }")]
    [InlineData("function* f() { yield first ? second : third; }")]
    [InlineData("function* f() { yield first, second; }")]
    [InlineData("function* f() { yield yield value; }")]
    [InlineData("function* f() { yield* yield value; }")]
    [InlineData("function* f() { yield\nvalue; }")]
    [InlineData("function* f() { yield/*\n*/value; }")]
    [InlineData("function* f() { yield /a/; yield value / divisor; }")]
    [InlineData("function* f() { return `${yield value}`; }")]
    [InlineData("function* f() { for (const value of values) yield value; }")]
    [InlineData("function* f() { for (let value = yield first; value; value = yield next) {} }")]
    [InlineData("function* f() { function inner(await) { return await; } yield inner; }")]
    [InlineData("function* f() { const inner = async () => await task; yield inner; }")]
    [InlineData("function* f() { async function inner() { await task; } yield inner; }")]
    [InlineData("function* f() { function* inner() { yield value; } yield* inner(); }")]
    [InlineData("function* f() { return new.target; }")]
    [InlineData("function* f({value}, [first] = list, ...rest) { yield value; }")]
    [InlineData("function* f(value = function* () { yield task; }) { yield value; }")]
    [InlineData("function* f(value = () => 1) { yield value; }")]
    [InlineData("const f = function* () { yield value; };")]
    [InlineData("const f = function* named() { yield value; };")]
    [InlineData("function* f(await) { yield await; }")]
    public void Generators_accept_yield_delegation_precedence_and_nested_function_boundaries(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("function* f() { for ((yield value) in source) {} }")]
    [InlineData("function* f() { yield** values; }")]
    [InlineData("function* f(yield) {}")]
    [InlineData("function* f(\\u0079ield) {}")]
    [InlineData("function* f({yield}) {}")]
    [InlineData("function* f([yield]) {}")]
    [InlineData("function* f(value = yield task) {}")]
    [InlineData("function* f(value = yield) {}")]
    [InlineData("function* f({[yield task]: value}) {}")]
    [InlineData("function* f(value = `${yield task}`) {}")]
    [InlineData("function* f() { let yield; }")]
    [InlineData("function* f() { yield: while (true) break yield; }")]
    [InlineData("function* f() { yield = value; }")]
    [InlineData("function* f() { ++yield; }")]
    [InlineData("function* f() { (yield value) = other; }")]
    [InlineData("function* f() { yield\n* values; }")]
    [InlineData("function* f() { yield/*\n*/*values; }")]
    [InlineData("function* f() { yield*; }")]
    [InlineData("function* f() { yield value **; }")]
    [InlineData("function* f() { await value; }")]
    [InlineData("function f() { yield value; }")]
    [InlineData("function* f() { function inner() { yield value; } }")]
    [InlineData("function* f() { const inner = () => yield value; }")]
    [InlineData("function* f() { for await (const value of values) {} }")]
    [InlineData("function* f(first, first) {}")]
    [InlineData("function* f(value) { const value = 1; }")]
    [InlineData("function* f(value = 1) { 'use strict'; }")]
    [InlineData("function* f(eval) {}")]
    [InlineData("function* f(arguments) {}")]
    public void Generators_reject_yield_in_parameters_wrong_function_kinds_and_illegal_targets(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("async function* f() {}")]
    [InlineData("async function * f() { yield await work(); }")]
    [InlineData("async function* f() { await work(); yield value; }")]
    [InlineData("async function* f() { yield* values; }")]
    [InlineData("async function* f() { yield* await values; }")]
    [InlineData("async function* f() { yield; return await done; }")]
    [InlineData("async function* f() { yield\nawait value; }")]
    [InlineData("async function* f() { yield /a/; await /b/; }")]
    [InlineData("async function* f() { yield `${await value}:${yield other}`; }")]
    [InlineData("async function* f() { for await (const value of stream) { yield value; } }")]
    [InlineData("async function* f({value}, [first] = list, ...rest) { yield value; }")]
    [InlineData("async function* f(value = async function* () { yield await task; }) { yield value; }")]
    [InlineData("async function* f() { function inner(await) { return await; } yield inner; }")]
    [InlineData("async function* f() { function* inner() { yield value; } yield* inner(); }")]
    [InlineData("async function* f() { async function inner() { await value; } yield inner; }")]
    [InlineData("async function* f() { return new.target; }")]
    [InlineData("const f = async function* () { yield await work(); };")]
    [InlineData("const f = async function* named() { yield await work(); };")]
    [InlineData("async/* same line */function* f() { yield await work(); }")]
    public void Async_generators_combine_await_yield_delegation_and_for_await(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("async function* f() { yield await yield value; }")]
    [InlineData("async function* f(await) {}")]
    [InlineData("async function* f(yield) {}")]
    [InlineData("async function* f(value = await task) {}")]
    [InlineData("async function* f(value = yield task) {}")]
    [InlineData("async function* f({[await task]: value}) {}")]
    [InlineData("async function* f({[yield task]: value}) {}")]
    [InlineData("async function* f() { let await; }")]
    [InlineData("async function* f() { let yield; }")]
    [InlineData("async\nfunction* f() { yield await task; }")]
    [InlineData("async function* f() { yield\n* values; }")]
    [InlineData("async function* f(first, first) {}")]
    [InlineData("async function* f(value) { let value; }")]
    [InlineData("async function* f(value = 1) { 'use strict'; }")]
    [InlineData("async function* f() { const nested = () => await value; }")]
    [InlineData("async function* f() { const nested = () => yield value; }")]
    public void Async_generators_reject_both_suspension_keywords_in_parameters(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("async function f() { for await (const value of stream) {} }")]
    [InlineData("async function f() { for await (let value of stream) {} }")]
    [InlineData("async function f() { for await (var value of stream) {} }")]
    [InlineData("async function f() { for await (value of stream) {} }")]
    [InlineData("async function f() { for await (object.value of stream) {} }")]
    [InlineData("async function f() { for await ({value} of stream) {} }")]
    [InlineData("async function f() { for await ([value, ...rest] of stream) {} }")]
    [InlineData("async function f() { for await (const {value} of stream) {} }")]
    [InlineData("async function f() { for await (const [value] of await stream) {} }")]
    [InlineData("async function f() { for await (async of stream) {} }")]
    [InlineData("async function f() { for await (const of of stream) {} }")]
    [InlineData("async function f() { outer: for await (const value of stream) { continue outer; } }")]
    [InlineData("async function f() { for\nawait\n(const value of stream) {} }")]
    public void For_await_of_checks_its_function_context_and_binding_head(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("for await (const value of stream) {}")]
    [InlineData("function f() { for await (const value of stream) {} }")]
    [InlineData("async function f() { function inner() { for await (const value of stream) {} } }")]
    [InlineData("async function f() { for await (const value in stream) {} }")]
    [InlineData("async function f() { for await (value in stream) {} }")]
    [InlineData("async function f() { for await (;;) {} }")]
    [InlineData("async function f() { for await (const first, second of stream) {} }")]
    [InlineData("async function f() { for await (const value = 1 of stream) {} }")]
    [InlineData("async function f() { for await (var value = 1 of stream) {} }")]
    [InlineData("async function f() { for await (first + second of stream) {} }")]
    [InlineData("async function f() { for await (value?.field of stream) {} }")]
    [InlineData("async function f() { for await (const await of stream) {} }")]
    [InlineData("async function f() { for await (const value of first, second) {} }")]
    public void For_await_of_rejects_ordinary_functions_in_heads_and_invalid_binding_targets(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("const object = { run() {} };")]
    [InlineData("const object = { run(value) { return value; } };")]
    [InlineData("const object = { run({value}, [first] = list, ...rest) { return value; } };")]
    [InlineData("const object = { 'run'() {}, 1() {}, 1n() {}, [key]() {} };")]
    [InlineData("const object = { if() {}, await() {}, yield() {}, static() {}, get() {}, set() {}, async() {} };")]
    [InlineData("const object = { constructor() { return super.value; } };")]
    [InlineData("const object = { __proto__() {}, __proto__: value };")]
    [InlineData("const object = { get value() { return 1; }, set value(next) { this.stored = next; } };")]
    [InlineData("const object = { get [key]() { return super.value; }, set [key]({value}) { super.value = value; } };")]
    [InlineData("const object = { get 'value'() { return 1; }, set 1(next) {} };")]
    [InlineData("const object = { get value() { return new.target; }, set value(next = fallback) {} };")]
    [InlineData("const object = { set value([first, ...rest]) {} };")]
    [InlineData("const object = { get value() {}, get value() {}, set value(next) {}, set value(next) {} };")]
    [InlineData("const object = { run() {}, run() {}, value: 1, value: 2 };")]
    [InlineData("const object = { run() { return () => super.value; } };")]
    [InlineData("const object = { run() { return new.target; } };")]
    [InlineData("const object = { run() { return this; }, ...other, tail() {} };")]
    [InlineData("const object = { run() { return /a/; } }; /b/.test(value);")]
    [InlineData("const object = { run() { return `${this.value}`; } };")]
    [InlineData("const object = { get\nvalue() {}, set\nvalue(next) {} };")]
    [InlineData("const object = { ['__proto__']() {}, '__proto__'() {} };")]
    [InlineData("const object = { await(await) { return await; } };")]
    [InlineData("const object = { run(value) { var value; return value; } };")]
    [InlineData("const object = { get value() { function nested() { return new.target; } return nested; } };")]
    [InlineData("({ run() {} }).run();")]
    [InlineData("({ get value() { return 1; } }).value / divisor;")]
    public void Object_methods_and_accessors_support_property_names_bindings_and_super(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const object = { get value(arg) {} };")]
    [InlineData("const object = { get value(...rest) {} };")]
    [InlineData("const object = { set value() {} };")]
    [InlineData("const object = { set value(first, second) {} };")]
    [InlineData("const object = { set value(...rest) {} };")]
    [InlineData("const object = { run(first, first) {} };")]
    [InlineData("const object = { run(eval) {} };")]
    [InlineData("const object = { run(value) { let value; } };")]
    [InlineData("const object = { run(value = 1) { 'use strict'; } };")]
    [InlineData("const object = { run() { super(); } };")]
    [InlineData("const object = { constructor() { super(); } };")]
    [InlineData("const object = { run() { function nested() { return super.value; } } };")]
    [InlineData("const object = { get value() { super(); } };")]
    [InlineData("const object = { run() { await value; } };")]
    [InlineData("const object = { run() { yield value; } };")]
    [InlineData("const object = { get value() { await task; } };")]
    [InlineData("const object = { #value() {} };")]
    [InlineData("const object = { static run() {} };")]
    [InlineData("const object = { get value: 1 };")]
    [InlineData("const object = { set value = 1 };")]
    [InlineData("const object = { get async value() {} };")]
    [InlineData("const object = { set * value(next) {} };")]
    [InlineData("const object = { run() {} tail() {} };")]
    [InlineData("const object = { run() {}; tail() {} };")]
    public void Object_methods_and_accessors_reject_invalid_arity_binding_and_super_calls(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("const object = { async run() { await work(); } };")]
    [InlineData("const object = { *run() { yield value; } };")]
    [InlineData("const object = { async *run() { yield await value; } };")]
    [InlineData("const object = { async [key]() { return await work(); }, *'run'() { yield value; }, async *1() {} };")]
    [InlineData("const object = { async async() {}, async get() {}, async set() {}, *async() {}, async *async() {} };")]
    [InlineData("const object = { async constructor() {}, *constructor() {}, async *constructor() {} };")]
    [InlineData("const object = { async run() { return super.value; }, *items() { yield super.value; } };")]
    [InlineData("const object = { async *items() { for await (const value of super.values) yield value; } };")]
    [InlineData("const object = { async run() { return () => super.value; } };")]
    [InlineData("const object = { async run() { return new.target; }, *items() { yield new.target; } };")]
    [InlineData("const object = { *\nrun() { yield value; }, async *\nitems() { yield await value; } };")]
    [InlineData("class A { async run() { return await work(); } }")]
    [InlineData("class A { *items() { yield value; } }")]
    [InlineData("class A { async *items() { yield await value; } }")]
    [InlineData("class A { static async run() { await work(); } static *items() { yield value; } static async *all() {} }")]
    [InlineData("class A { async #run() { await work(); } *#items() { yield value; } async *#all() {} }")]
    [InlineData("class A { static async #run() {} static *#items() {} static async *#all() {} }")]
    [InlineData("class A { async run({value}, ...rest) { return value; } *items([first] = list) { yield first; } }")]
    [InlineData("class A extends Base { async run() { return await super.run(); } *items() { yield* super.items(); } }")]
    [InlineData("class A extends Base { async *items() { yield* await super.items(); } }")]
    [InlineData("class A { async ['constructor']() {} *['constructor']() {} async *['constructor']() {} }")]
    [InlineData("class A { static async constructor() {} static *constructor() {} static async *constructor() {} }")]
    [InlineData("class A { async static() {} *static() {} async *static() {} }")]
    [InlineData("class A { async() {} *async() {} async async() {} async *async() {} }")]
    [InlineData("class A { static async() {} static *async() {} static async async() {} }")]
    [InlineData("class A { async run() { return this.#value; } #value; }")]
    [InlineData("class A { *items() { const f = () => this.#value; yield f; } #value; }")]
    [InlineData("class A { async *items() { for await (const value of source) yield value; } }")]
    [InlineData("class A { async run() { return /a/; } } /b/.test(value);")]
    [InlineData("class A { async run() { return `${await value}`; } *items() { yield `${yield value}`; } }")]
    public void Async_and_generator_methods_work_in_objects_and_classes(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("class A { async #constructor() {} *#items() {} }")]
    [InlineData("const object = { async\nrun() {} };")]
    [InlineData("const object = { async/*\n*/run() {} };")]
    [InlineData("const object = { async\n*run() {} };")]
    [InlineData("const object = { async run(await) {} };")]
    [InlineData("const object = { *run(yield) {} };")]
    [InlineData("const object = { async *run(await) {} };")]
    [InlineData("const object = { async *run(yield) {} };")]
    [InlineData("const object = { async run(value = await task) {} };")]
    [InlineData("const object = { *run(value = yield task) {} };")]
    [InlineData("const object = { async get value() {} };")]
    [InlineData("const object = { async set value(next) {} };")]
    [InlineData("const object = { *get value() {} };")]
    [InlineData("const object = { *async run() {} };")]
    [InlineData("const object = { async async run() {} };")]
    [InlineData("const object = { async run() { super(); } };")]
    [InlineData("const object = { *run() { super(); } };")]
    [InlineData("class A { async constructor() {} }")]
    [InlineData("class A { *constructor() {} }")]
    [InlineData("class A { async *constructor() {} }")]
    [InlineData("class A { async 'constructor'() {} }")]
    [InlineData("class A { *'constructor'() {} }")]
    [InlineData("class A { static async prototype() {} }")]
    [InlineData("class A { static *prototype() {} }")]
    [InlineData("class A { async #constructor() {} }")]
    [InlineData("class A { async #run() {} *#run() {} }")]
    [InlineData("class A { async run(await) {} }")]
    [InlineData("class A { *run(yield) {} }")]
    [InlineData("class A { async *run(value = await task) {} }")]
    [InlineData("class A { async *run(value = yield task) {} }")]
    [InlineData("class A { async run() { return this.#missing; } }")]
    [InlineData("class A extends Base { async run() { super(); } }")]
    [InlineData("class A extends Base { *run() { super(); } }")]
    [InlineData("class A { async\nrun() { await task; } }")]
    [InlineData("class A { static async\nrun() { await task; } }")]
    [InlineData("class A { async\n*run() { await task; } }")]
    [InlineData("class A { async run() { function inner() { return super.value; } } }")]
    [InlineData("class A { *run(first, first) {} }")]
    [InlineData("class A { async run(value = 1) { 'use strict'; } }")]
    public void Async_and_generator_methods_reject_bad_modifiers_constructor_kinds_and_scope(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("async function f() { class A { [await key]() {} } return A; }")]
    [InlineData("function* f() { class A { [yield key]() {} } yield A; }")]
    [InlineData("async function f() { return { [await key]() {} }; }")]
    [InlineData("function* f() { yield { [yield key]() {} }; }")]
    [InlineData("async function* f() { return { [await key]() {}, [yield value]() {} }; }")]
    [InlineData("async function f() { return class extends (await Base) {}; }")]
    [InlineData("function* f() { yield class extends (yield Base) {}; }")]
    [InlineData("async function f() { return { get value() { return await; } }; }")]
    [InlineData("async function f() { return class { method(await) { return await; } }; }")]
    [InlineData("function* f() { return { async run() { await value; } }; }")]
    [InlineData("async function f() { return function(await) { return await; }; }")]
    [InlineData("const async = value; async(value); async\nfunction f() {}")]
    [InlineData("let await = value; await += 1; const f = await => await;")]
    [InlineData("async(value); async(value, other); async({value: 1});")]
    [InlineData("async: while (ready) { if (skip) continue async; break async; }")]
    [InlineData("async function f() { await class { [await value]() {} }; }")]
    public void Suspension_contexts_keep_synchronous_nested_scopes_and_method_computed_names_separate(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("async function f() { class A { static field = await; } return A; }")]
    [InlineData("async function f() { class A { field = await value; } }")]
    [InlineData("async function f() { class A { static field = await value; } }")]
    [InlineData("async function f() { class A { static { await value; } } }")]
    [InlineData("function* f() { class A { field = yield value; } }")]
    [InlineData("function* f() { class A { static { yield value; } } }")]
    [InlineData("async function f() { return { get value() { await task; } }; }")]
    [InlineData("function* f() { return { run() { yield value; } }; }")]
    [InlineData("async function f() { return class { method() { await value; } }; }")]
    [InlineData("async function f(value = class { [await task]() {} }) {}")]
    [InlineData("function* f(value = class { [yield task]() {} }) {}")]
    public void Suspension_contexts_do_not_leak_into_nested_synchronous_bodies_or_class_initializers(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("async function f(value = ({ run() { return await; } })) {}")]
    [InlineData("async function f(value = ({ async run() { await task; } })) {}")]
    [InlineData("async function f(value = ({ get value() { return await; } })) {}")]
    [InlineData("async function f(value = class { async run() { await task; } }) {}")]
    [InlineData("function* f(value = ({ *run() { yield task; } })) {}")]
    [InlineData("function* f(value = class { *run() { yield task; } }) {}")]
    [InlineData("function* f(value = function* () { yield task; }) {}")]
    [InlineData("async function* f(value = async function* () { yield await task; }) {}")]
    [InlineData("async function f(value = async () => await task) {}")]
    [InlineData("async function* f(value = async () => await task) {}")]
    public void Parameter_contains_checks_cross_computed_names_and_heritage_but_stop_at_nested_bodies(string input)
        => _fixture.Success(input);

    // FieldDefinition passes ?Await to its initializer in the pinned 2026 grammar.
    // TC39 #2437/#3333 track implementation differences. Preserve the existing
    // project's published-grammar policy, including its pre-existing class tests.
    [Theory]
    [InlineData("class A { field = await; static field = await; }", true)]
    [InlineData("async function f() { class A { field = await; } return A; }", false)]
    [InlineData("async function f(value = class { field = await; }) {}", false)]
    [InlineData("async function f() { class A { static field = await; } return A; }", false)]
    [InlineData("class A { static { class B { field = await; } } }", false)]
    [InlineData("async function f() { class A { field = () => await; } return A; }", true)]
    [InlineData("async function f() { class A { static field = function(await) { return await; }; } }", true)]
    [InlineData("const f = async (a = class { field = await; }) => a;", false)]
    [InlineData("const f = async (a = class { field = () => await; }) => a;", true)]
    [InlineData("const f = async (a = class { static field = await; }) => a;", false)]
    public void Published_Await_parameters_apply_to_field_identifiers_while_nested_function_bodies_reset(string input, bool valid)
    {
        if (valid) _fixture.Success(input);
        else _fixture.Invalid(input);
    }

    [Theory]
    [InlineData("async function f(value = ({ [await task]() {} })) {}")]
    [InlineData("async function f(value = ({ async [await task]() {} })) {}")]
    [InlineData("async function f(value = ({ get [await task]() {} })) {}")]
    [InlineData("async function f(value = ({ [await task]: 1 })) {}")]
    [InlineData("async function f(value = class extends (await Base) {}) {}")]
    [InlineData("async function f(value = class { [await key]() {} }) {}")]
    [InlineData("async function f(value = class { [await key] = 1; }) {}")]
    [InlineData("function* f(value = ({ [yield task]() {} })) {}")]
    [InlineData("function* f(value = ({ *[yield task]() {} })) {}")]
    [InlineData("function* f(value = ({ get [yield task]() {} })) {}")]
    [InlineData("function* f(value = class extends (yield Base) {}) {}")]
    [InlineData("function* f(value = class { [yield key]() {} }) {}")]
    [InlineData("async function* f(value = ({ [await task]() {} })) {}")]
    [InlineData("async function* f(value = ({ [yield task]() {} })) {}")]
    [InlineData("async function f(value = await => await) {}")]
    [InlineData("async function f(value = (await) => await) {}")]
    [InlineData("async function f(value = ({await}) => await) {}")]
    public void Parameter_contains_checks_reject_suspension_in_computed_names_and_heritage(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("async function await() {}")]
    [InlineData("async function f() { return { await: await task }; }")]
    [InlineData("const f = async function async() {};")]
    [InlineData("const object = { \\u0061sync() {}, \\u0067et() {}, \\u0073et() {} };")]
    [InlineData("const object = { async \\u0072un() { await task; }, *\\u0072un() { yield value; } };")]
    [InlineData("const object = { get \\u0076alue() {}, set \\u0076alue(value) {} };")]
    [InlineData("class A { async \\u0072un() { await task; } *\\u0072un() { yield value; } }")]
    [InlineData("class A { \\u0061sync() {} \\u0067et() {} \\u0073et() {} }")]
    [InlineData("const object = { async await() {}, *yield() {}, async *await() {} };")]
    [InlineData("class A { async await() {} *yield() {} async *await() {} }")]
    [InlineData("const f = async function f({await: value}) { return value; };")]
    [InlineData("const f = function* f({yield: value}) { yield value; };")]
    [InlineData("const f = async ({await: value}) => value;")]
    [InlineData("async function f() { const object = { async: 1, await: 2, yield: 3 }; return object.await; }")]
    public void Contextual_function_keywords_distinguish_escaped_identifiers_and_method_names(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("\\u0061sync function f() {}")]
    [InlineData("const f = \\u0061sync function() {};")]
    [InlineData("const f = \\u0061sync value => value;")]
    [InlineData("const object = { \\u0061sync run() {} };")]
    [InlineData("const object = { \\u0067et value() {} };")]
    [InlineData("const object = { \\u0073et value(value) {} };")]
    [InlineData("class A { \\u0061sync run() {} }")]
    [InlineData("async function f() { \\u0061wait value; }")]
    [InlineData("function* f() { \\u0079ield value; }")]
    [InlineData("const f = async function await() {};")]
    [InlineData("async function f() { async function await() {} }")]
    [InlineData("async function f() { const await = value; }")]
    [InlineData("const f = async \\u0061wait => value;")]
    [InlineData("const object = { async run(\\u0061wait) {} };")]
    [InlineData("const object = { *run(\\u0079ield) {} };")]
    public void Escaped_async_await_yield_get_and_set_do_not_become_syntactic_modifiers(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("async function f(value) { var value; return value; }")]
    [InlineData("function* f(value) { var value; yield value; }")]
    [InlineData("async function* f(value) { var value; yield await value; }")]
    [InlineData("async function f(value) { function value() {} return value; }")]
    [InlineData("function* f(value) { function value() {} yield value; }")]
    [InlineData("async function* f(value) { function value() {} yield value; }")]
    [InlineData("async function f() { let value; { let value; } }")]
    [InlineData("function* f() { let value; { let value; } }")]
    [InlineData("async function f() { outer: while (value) { await step(); continue outer; } }")]
    [InlineData("function* f() { outer: while (value) { yield step(); continue outer; } }")]
    [InlineData("const object = { run(value) { function value() {} return value; } };")]
    [InlineData("class A { async run(value) { function value() {} return value; } }")]
    [InlineData("for (let f = (async () => key in source);;) { break; }")]
    [InlineData("for (let object = { run() { return key in source; } };;) { break; }")]
    [InlineData("async function f() { for (let value = (await (key in source));;) { break; } }")]
    public void Function_kinds_preserve_parameter_body_scope_return_and_loop_boundaries(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("async function f(value) { class value {} }")]
    [InlineData("function* f(value) { let value; }")]
    [InlineData("async function* f(value) { const value = 1; }")]
    [InlineData("const object = { run(value) { const value = 1; } };")]
    [InlineData("const object = { get value() { let value; var value; } };")]
    [InlineData("const object = { set value(value) { let value; } };")]
    [InlineData("while (ready) { async function f() { break; } }")]
    [InlineData("while (ready) { function* f() { continue; } }")]
    [InlineData("outer: while (ready) { async function* f() { break outer; } }")]
    [InlineData("outer: while (ready) { const object = { run() { continue outer; } }; }")]
    [InlineData("const object = { get value() { break; } };")]
    [InlineData("async function f() { let value; function* value() {} }")]
    [InlineData("function* f() { const value = 1; async function value() {} }")]
    [InlineData("let f; async function f() {}")]
    [InlineData("let f; async function* f() {}")]
    [InlineData("for (let f = async () => key in source;;) { break; }")]
    public void Function_kinds_reset_flow_control_and_reject_lexical_declaration_conflicts(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("const object = { async run(value = super.value) { return await value; } };")]
    [InlineData("const object = { *run(value = super.value) { yield value; } };")]
    [InlineData("const object = { async *run(value = super.value) { yield await value; } };")]
    [InlineData("const object = { run() { return async () => super.value; } };")]
    [InlineData("const object = { run() { return async () => new.target; } };")]
    [InlineData("class A extends Base { async run(value = super.value) { return await value; } }")]
    [InlineData("class A extends Base { *run(value = super.value) { yield value; } }")]
    [InlineData("class A extends Base { constructor() { const f = async () => super(); } }")]
    [InlineData("class A extends Base { run() { return { [super.value]() {} }; } }")]
    [InlineData("class A { #value; async run() { function* inner() { yield object.#value; } return inner; } }")]
    [InlineData("class A { #value; *run() { const object = { async run() { return this.#value; } }; yield object; } }")]
    [InlineData("function f() { return { [new.target]() {} }; }")]
    public void Method_super_and_new_target_context_is_inherited_only_by_arrows(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("const object = { run() { return async function() { return super.value; }; } };")]
    [InlineData("const object = { run() { return function* () { yield super.value; }; } };")]
    [InlineData("const object = { run() { return async function* () { yield await super.value; }; } };")]
    [InlineData("class A extends Base { run() { return async function() { super(); }; } }")]
    [InlineData("class A extends Base { constructor() { async function nested() { super(); } } }")]
    [InlineData("class A { async run() { return async () => super(); } }")]
    [InlineData("const object = { [super.value]() {} };")]
    [InlineData("const object = { [new.target]() {} };")]
    [InlineData("async function f(value = super.value) {}")]
    [InlineData("function* f(value = super.value) {}")]
    [InlineData("async function* f(value = super.value) {}")]
    public void Nested_non_arrow_functions_do_not_inherit_super_from_methods(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("const icon = '😀'; async function f(값, 값) {}", "값")]
    [InlineData("const icon = '😀'; function* f(값, 값) {}", "값")]
    [InlineData("const icon = '😀'; async function* f(값, 값) {}", "값")]
    [InlineData("const icon = '😀'; const f = async (값, 값) => 1;", "값")]
    [InlineData("const icon = '😀'; const object = { run(값, 값) {} };", "값")]
    [InlineData("const icon = '😀'; class A { async run(값, 값) {} }", "값")]
    public void Parameter_diagnostics_keep_original_UTF16_offsets_after_astral_source_text(string input, string identifier)
    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        Assert.Empty(parsed.AllErrors);
        var diagnostics = _fixture.Language.Validate(parsed.ToParseTree);
        Assert.Contains(diagnostics, diagnostic => diagnostic.Code == "duplicate_parameter" &&
            diagnostic.SourceOffset == input.LastIndexOf(identifier, StringComparison.Ordinal) &&
            diagnostic.Length == identifier.Length);
        Assert.All(diagnostics, diagnostic =>
        {
            Assert.False(string.IsNullOrWhiteSpace(diagnostic.Message));
            Assert.InRange(diagnostic.SourceOffset, 0, input.Length);
            Assert.InRange(diagnostic.Length, 0, input.Length - diagnostic.SourceOffset);
        });
        Assert.Equal(diagnostics.Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray(),
            JavaScriptExpressionValidation.Validate(parsed.ToParseTree)
                .Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray());
    }

    [Fact]
    public void Parsing_a_new_source_does_not_retain_async_generator_or_method_context()
    {
        for (int round = 0; round < 3; round++)
        {
            _fixture.Success("async function* f() { yield await /a/; }");
            _fixture.Invalid("function f() { await value; }");
            _fixture.Invalid("function f() { yield value; }");
            _fixture.Invalid("super.value;");
            _fixture.Success("let await; const f = await => await; const async = value;");
            _fixture.Success("const object = { run() { return super.value; } };");
            _fixture.Success("async function f() { return value / divisor; }");
            _fixture.Invalid("const object = { run() { super(); } };");
        }
    }

    [Fact]
    public async Task Repeated_and_parallel_validation_keeps_function_context_local_to_each_tree()
    {
        var valid = _fixture.Success("async function* f() { const object = { async run() { return super.value; } }; yield await task; }");
        var invalid = _fixture.Parse("const object = { async run(first, first) {} };");
        Assert.True(invalid.Success);
        var before = invalid.ToParseTree.AllTokens.ToArray();
        var expected = _fixture.Language.Validate(invalid.ToParseTree)
            .Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray();
        Assert.NotEmpty(expected);
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
        {
            for (int round = 0; round < 10; round++)
            {
                Assert.Empty(_fixture.Language.Validate(valid));
                Assert.Equal(expected, _fixture.Language.Validate(invalid.ToParseTree)
                    .Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray());
                Assert.Equal(expected, JavaScriptExpressionValidation.Validate(invalid.ToParseTree)
                    .Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray());
            }
        })));
        var after = invalid.ToParseTree.AllTokens.ToArray();
        Assert.Equal(before.Length, after.Length);
        for (int index = 0; index < before.Length; index++) Assert.Same(before[index], after[index]);
    }
}

[Collection(JavaScriptModuleCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptFunctionKinds")]
public sealed class JavaScriptFunctionKindsModuleTests
{
    private readonly JavaScriptModuleFixture _fixture;
    public JavaScriptFunctionKindsModuleTests(JavaScriptModuleFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("export async function f() { await task; }")]
    [InlineData("export function* f() { yield value; }")]
    [InlineData("export async function* f() { yield await value; }")]
    [InlineData("export default async function() { await task; }")]
    [InlineData("export default async function named() { await task; }")]
    [InlineData("export default function* () { yield value; }")]
    [InlineData("export default function* named() { yield value; }")]
    [InlineData("export default async function* () { yield await value; }")]
    [InlineData("export default async function* named() { yield await value; }")]
    [InlineData("export const f = async value => await value;")]
    [InlineData("export default async () => await task;")]
    [InlineData("async function f() {} export {f};")]
    [InlineData("function* f() {} export {f};")]
    [InlineData("async function* f() {} export {f};")]
    [InlineData("import value from 'module'; export async function f() { return await value; }")]
    [InlineData("export async function f() { return import.meta; }")]
    [InlineData("export function* f() { yield import.meta; }")]
    [InlineData("export default { async run() { await task; }, *items() { yield value; }, get value() {} };")]
    [InlineData("export class A { async run() { await task; } *items() { yield value; } async *all() {} }")]
    [InlineData("await (async () => await task)();")]
    [InlineData("for await (const value of stream) { await value; }")]
    [InlineData("const object = { [await key]() {}, async run() {}, *items() {} }; export {object};")]
    [InlineData("export async function f() { for await (const value of stream) await value; }")]
    [InlineData("export default async function* () { for await (const value of stream) yield value; }")]
    [InlineData("const f = async () => import.meta.url; export default f;")]
    [InlineData("export default async *function() {}")]
    public void Modules_export_all_function_kinds_and_preserve_top_level_await(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("export async function await() {}")]
    [InlineData("export function* await() {}")]
    [InlineData("export const f = async await => await;")]
    [InlineData("export function* f() { await value; }")]
    [InlineData("export default function* (await) {}")]
    [InlineData("export default async function* (yield) {}")]
    [InlineData("export async function f() {} export {f};")]
    [InlineData("export default async function() {} export default function* () {}")]
    [InlineData("import f from 'module'; async function f() {}")]
    [InlineData("let f; function* f() {}")]
    [InlineData("export default async\nfunction() {}")]
    [InlineData("export const f = () => await value;")]
    [InlineData("export const object = { run(await) { return await; } };")]
    [InlineData("export const object = { get value() { return await; } };")]
    [InlineData("export async function f() { function inner(await) { return await; } }")]
    [InlineData("export async function* f() { yield\n* values; }")]
    [InlineData("export class A { async constructor() {} }")]
    public void Modules_reject_reserved_await_duplicate_exports_and_function_scope_errors(string input)
        => _fixture.Invalid(input);
}

[Collection(JavaScriptExpressionCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptFunctionKinds")]
public sealed class JavaScriptFunctionKindsExpressionTests
{
    private readonly JavaScriptExpressionFixture _fixture;
    public JavaScriptFunctionKindsExpressionTests(JavaScriptExpressionFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("async function() { return await task; }")]
    [InlineData("async function named(value) { return await value; }")]
    [InlineData("function* () { yield value; }")]
    [InlineData("function* named() { yield* values; }")]
    [InlineData("async function* () { yield await value; }")]
    [InlineData("async function* named() { yield* await values; }")]
    [InlineData("async value => await value")]
    [InlineData("async () => await task")]
    [InlineData("async ({value} = source, ...rest) => value")]
    [InlineData("({ run() {}, get value() {}, set value(next) {} })")]
    [InlineData("({ async run() { await task; }, *items() { yield value; }, async *all() {} })")]
    [InlineData("({ async run() { return super.value; } })")]
    [InlineData("({ get value() { return super.value; } })")]
    [InlineData("({ async: value, get: other, set: third })")]
    [InlineData("class { async run() { await task; } *items() { yield value; } async *all() {} }")]
    [InlineData("(async () => /a/) / divisor")]
    [InlineData("async () => `${await value}`")]
    [InlineData("function* () { yield /a/; }")]
    [InlineData("async function() { return import('module'); }")]
    [InlineData("async(await)")]
    [InlineData("await => await")]
    [InlineData("({ await(await) { return await; } })")]
    [InlineData("async () => ({ [await key]() {} })")]
    public void Expression_entry_accepts_function_expressions_async_arrows_and_object_methods(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("async function() { yield value; }")]
    [InlineData("function* () { await value; }")]
    [InlineData("async function* (await) {}")]
    [InlineData("async function* (yield) {}")]
    [InlineData("async await => await")]
    [InlineData("async (value = await task) => value")]
    [InlineData("async\n() => await task")]
    [InlineData("async value\n=> value")]
    [InlineData("async () => await")]
    [InlineData("async () => new.target")]
    [InlineData("async () => super.value")]
    [InlineData("({ get value(arg) {} })")]
    [InlineData("({ set value() {} })")]
    [InlineData("({ async constructor() { super(); } })")]
    [InlineData("async function() { return import.meta; }")]
    [InlineData("async () => import.meta")]
    [InlineData("function* () { yield import.meta; }")]
    [InlineData("class { async constructor() {} }")]
    [InlineData("async function f() {} next()")]
    [InlineData("for await (const value of source) {}")]
    public void Expression_entry_rejects_statement_only_forms_and_invalid_function_contexts(string input)
        => _fixture.Invalid(input);
}
