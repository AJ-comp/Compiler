import json
from pathlib import Path

groups = []

def add(goal, name, valid, examples):
    for text in examples.strip().split('\n@@\n'):
        groups.append(dict(goal=goal, group=name, valid=valid, text=text))

add('script', 'Async_functions_accept_await_and_preserve_nested_function_boundaries', True, r'''
async function f() {}
@@
async function f() { return await work(); }
@@
async function f() { await first; await second; }
@@
async function f() { return await await work(); }
@@
async function f() { return 1 + await value * 2; }
@@
async function f() { return typeof await value; }
@@
async function f() { return (await value).field; }
@@
async function f() { return (await factory)(); }
@@
async function f() { await /a/.test(value); return value / divisor; }
@@
async function f() { return `${await first}:${await second}`; }
@@
async function f() { return tag`value ${await task}`; }
@@
async function f() { return await import('module'); }
@@
async function f({value}, [first] = list, ...rest) { return await value; }
@@
async function f({[key]: value = fallback} = source) { return value; }
@@
async function f(value = () => await) { return value; }
@@
async function f(value = function(await) { return await; }) { return value; }
@@
async function f() { function inner(await) { return await; } return inner; }
@@
async function f() { const inner = () => await; return inner; }
@@
async function f() { const inner = async () => await value; return inner; }
@@
async function f() { function* inner() { yield value; } return inner; }
@@
async function f() { async function inner() { await value; } return inner; }
@@
async function f() { return new.target; }
@@
const f = async function(value) { return await value; };
@@
const f = async function named(value) { return await value; };
@@
const f = async /* same line */ function named() {};
@@
async function f() { try { return await work(); } catch ({message}) { throw message; } }
@@
async function f() { for (let i = await first; i < await last; i++) await step(i); }
@@
async function f() { for (const value of await source) { await consume(value); } }
@@
async function f() { return\nvalue; }
@@
async function f() { throw await failure; }
@@
async function f() { await\nwork(); }
@@
async function f() { await/*\n*/work(); }
''')

add('script', 'Async_parameters_and_body_context_reject_reserved_await_and_illegal_suspension', False, r'''
async function f(await) {}
@@
async function f(\u0061wait) {}
@@
async function f({await}) {}
@@
async function f([await]) {}
@@
async function f(...await) {}
@@
async function f(value = await task) {}
@@
async function f(value = (await task)) {}
@@
async function f({[await task]: value}) {}
@@
async function f(value = `${await task}`) {}
@@
async function f() { let await; }
@@
async function f() { var await; }
@@
async function f() { function await() {} }
@@
async function f() { class await {} }
@@
async function f() { try {} catch (await) {} }
@@
async function f() { await: while (true) break await; }
@@
async function f() { await; }
@@
async function f() { await * values; }
@@
async function f() { await value ** 2; }
@@
async function f() { yield value; }
@@
function f() { await value; }
@@
async function f() { function inner() { await value; } }
@@
async function f() { const inner = () => await value; }
@@
async\nfunction f() { await value; }
@@
async/*\n*/function f() { await value; }
@@
async function f(first, first) {}
@@
async function f({value}, value) {}
@@
async function f(value = 1) { 'use strict'; }
@@
async function f(value) { let value; }
@@
async function f(eval) {}
@@
async function f(arguments) {}
''')

add('script', 'Async_arrows_accept_both_parameter_forms_and_concise_or_block_bodies', True, r'''
const f = async value => await value;
@@
const f = async () => await work();
@@
const f = async (first, second,) => await first + await second;
@@
const f = async ({value}, [first] = list, ...rest) => await value;
@@
const f = async (...[first, ...rest]) => first;
@@
const f = async ({value = fallback} = source) => value;
@@
const f = async value => ({value: await value});
@@
const f = async value => { return await value; };
@@
const f = async () => {};
@@
const f = async () => async value => await value;
@@
const f = async (value = async () => await task) => value;
@@
const f = async (value = function(await) { return await; }) => value;
@@
const f = async (value = () => await) => value;
@@
const f = async /* same line */ value => await value;
@@
const f = async /* same line */ (value) => await value;
@@
const f = async (value\n) => await value;
@@
const f = async value =>\nawait value;
@@
const f = async () => /a/;
@@
const f = async () => await /a/.test(value);
@@
const f = async () => await value / divisor;
@@
const f = async () => `${await value}`;
@@
const f = async () => { for await (const value of stream) await consume(value); };
@@
function outer() { return async () => new.target; }
@@
class A extends Base { method() { return async () => super.value; } }
@@
const f = async async => async;
@@
const f = async (of, get, set) => await of + get + set;
@@
const f = async (value = await => await) => value;
''')

add('script', 'Async_arrows_enforce_line_terminators_parameters_and_lexical_context', False, r'''
const f = async await => await;
@@
const f = async (await) => await;
@@
const f = async ({await}) => 1;
@@
const f = async (value = await task) => value;
@@
const f = async ({[await task]: value}) => value;
@@
const f = async (value = `${await task}`) => value;
@@
const f = async (first, first) => first;
@@
const f = async ({value}, value) => value;
@@
const f = async (value = 1) => { 'use strict'; };
@@
const f = async (value) => { let value; };
@@
const f = async (...rest,) => rest;
@@
const f = async (first + second) => first;
@@
const f = async (first, (second)) => first;
@@
const f = async\nvalue => await value;
@@
const f = async\n() => await value;
@@
const f = async/*\n*/(value) => await value;
@@
const f = async (value)\n=> await value;
@@
const f = async value\n=> await value;
@@
const f = async () => { yield value; };
@@
const f = async () => { await; };
@@
const f = async () => { function inner() { await value; } };
@@
const f = async () => super.value;
@@
const f = async () => new.target;
@@
const f = new async value => value;
@@
const f = async * value => value;
@@
const f = async (eval) => eval;
''')

add('script', 'Generators_accept_yield_delegation_precedence_and_nested_function_boundaries', True, r'''
function* f() {}
@@
function * f() { yield; }
@@
function* f() { yield value; }
@@
function* f() { yield* values; }
@@
function* f() { yield * values; }
@@
function* f() { yield*\nvalues; }
@@
function* f() { return yield value; }
@@
function* f() { return (yield value) + (yield other); }
@@
function* f() { yield first = second; }
@@
function* f() { yield first ? second : third; }
@@
function* f() { yield first, second; }
@@
function* f() { yield yield value; }
@@
function* f() { yield* yield value; }
@@
function* f() { yield\nvalue; }
@@
function* f() { yield/*\n*/value; }
@@
function* f() { yield /a/; yield value / divisor; }
@@
function* f() { return `${yield value}`; }
@@
function* f() { for (const value of values) yield value; }
@@
function* f() { for (let value = yield first; value; value = yield next) {} }
@@
function* f() { for ((yield value) in source) {} }
@@
function* f() { function inner(await) { return await; } yield inner; }
@@
function* f() { const inner = async () => await task; yield inner; }
@@
function* f() { async function inner() { await task; } yield inner; }
@@
function* f() { function* inner() { yield value; } yield* inner(); }
@@
function* f() { return new.target; }
@@
function* f({value}, [first] = list, ...rest) { yield value; }
@@
function* f(value = function* () { yield task; }) { yield value; }
@@
function* f(value = () => 1) { yield value; }
@@
const f = function* () { yield value; };
@@
const f = function* named() { yield value; };
@@
function* f(await) { yield await; }
@@
function* f() { yield** values; }
''')

add('script', 'Generators_reject_yield_in_parameters_wrong_function_kinds_and_illegal_targets', False, r'''
function* f(yield) {}
@@
function* f(\u0079ield) {}
@@
function* f({yield}) {}
@@
function* f([yield]) {}
@@
function* f(value = yield task) {}
@@
function* f(value = yield) {}
@@
function* f({[yield task]: value}) {}
@@
function* f(value = `${yield task}`) {}
@@
function* f() { let yield; }
@@
function* f() { yield: while (true) break yield; }
@@
function* f() { yield = value; }
@@
function* f() { ++yield; }
@@
function* f() { (yield value) = other; }
@@
function* f() { yield\n* values; }
@@
function* f() { yield/*\n*/*values; }
@@
function* f() { yield*; }
@@
function* f() { yield value **; }
@@
function* f() { await value; }
@@
function f() { yield value; }
@@
function* f() { function inner() { yield value; } }
@@
function* f() { const inner = () => yield value; }
@@
function* f() { for await (const value of values) {} }
@@
function* f(first, first) {}
@@
function* f(value) { const value = 1; }
@@
function* f(value = 1) { 'use strict'; }
@@
function* f(eval) {}
@@
function* f(arguments) {}
''')

add('script', 'Async_generators_combine_await_yield_delegation_and_for_await', True, r'''
async function* f() {}
@@
async function * f() { yield await work(); }
@@
async function* f() { await work(); yield value; }
@@
async function* f() { yield* values; }
@@
async function* f() { yield* await values; }
@@
async function* f() { yield await yield value; }
@@
async function* f() { yield; return await done; }
@@
async function* f() { yield\nawait value; }
@@
async function* f() { yield /a/; await /b/; }
@@
async function* f() { yield `${await value}:${yield other}`; }
@@
async function* f() { for await (const value of stream) { yield value; } }
@@
async function* f({value}, [first] = list, ...rest) { yield value; }
@@
async function* f(value = async function* () { yield await task; }) { yield value; }
@@
async function* f() { function inner(await) { return await; } yield inner; }
@@
async function* f() { function* inner() { yield value; } yield* inner(); }
@@
async function* f() { async function inner() { await value; } yield inner; }
@@
async function* f() { return new.target; }
@@
const f = async function* () { yield await work(); };
@@
const f = async function* named() { yield await work(); };
@@
async/* same line */function* f() { yield await work(); }
''')

add('script', 'Async_generators_reject_both_suspension_keywords_in_parameters', False, r'''
async function* f(await) {}
@@
async function* f(yield) {}
@@
async function* f(value = await task) {}
@@
async function* f(value = yield task) {}
@@
async function* f({[await task]: value}) {}
@@
async function* f({[yield task]: value}) {}
@@
async function* f() { let await; }
@@
async function* f() { let yield; }
@@
async\nfunction* f() { yield await task; }
@@
async function* f() { yield\n* values; }
@@
async function* f(first, first) {}
@@
async function* f(value) { let value; }
@@
async function* f(value = 1) { 'use strict'; }
@@
async function* f() { const nested = () => await value; }
@@
async function* f() { const nested = () => yield value; }
''')

add('script', 'For_await_of_checks_its_function_context_and_binding_head', True, r'''
async function f() { for await (const value of stream) {} }
@@
async function f() { for await (let value of stream) {} }
@@
async function f() { for await (var value of stream) {} }
@@
async function f() { for await (value of stream) {} }
@@
async function f() { for await (object.value of stream) {} }
@@
async function f() { for await ({value} of stream) {} }
@@
async function f() { for await ([value, ...rest] of stream) {} }
@@
async function f() { for await (const {value} of stream) {} }
@@
async function f() { for await (const [value] of await stream) {} }
@@
async function f() { for await (async of stream) {} }
@@
async function f() { for await (const of of stream) {} }
@@
async function f() { outer: for await (const value of stream) { continue outer; } }
@@
async function f() { for\nawait\n(const value of stream) {} }
''')

add('script', 'For_await_of_rejects_ordinary_functions_in_heads_and_invalid_binding_targets', False, r'''
for await (const value of stream) {}
@@
function f() { for await (const value of stream) {} }
@@
async function f() { function inner() { for await (const value of stream) {} } }
@@
async function f() { for await (const value in stream) {} }
@@
async function f() { for await (value in stream) {} }
@@
async function f() { for await (;;) {} }
@@
async function f() { for await (const first, second of stream) {} }
@@
async function f() { for await (const value = 1 of stream) {} }
@@
async function f() { for await (var value = 1 of stream) {} }
@@
async function f() { for await (first + second of stream) {} }
@@
async function f() { for await (value?.field of stream) {} }
@@
async function f() { for await (const await of stream) {} }
@@
async function f() { for await (const value of first, second) {} }
''')

add('script', 'Object_methods_and_accessors_support_property_names_bindings_and_super', True, r'''
const object = { run() {} };
@@
const object = { run(value) { return value; } };
@@
const object = { run({value}, [first] = list, ...rest) { return value; } };
@@
const object = { 'run'() {}, 1() {}, 1n() {}, [key]() {} };
@@
const object = { if() {}, await() {}, yield() {}, static() {}, get() {}, set() {}, async() {} };
@@
const object = { constructor() { return super.value; } };
@@
const object = { __proto__() {}, __proto__: value };
@@
const object = { get value() { return 1; }, set value(next) { this.stored = next; } };
@@
const object = { get [key]() { return super.value; }, set [key]({value}) { super.value = value; } };
@@
const object = { get 'value'() { return 1; }, set 1(next) {} };
@@
const object = { get value() { return new.target; }, set value(next = fallback) {} };
@@
const object = { set value([first, ...rest]) {} };
@@
const object = { get value() {}, get value() {}, set value(next) {}, set value(next) {} };
@@
const object = { run() {}, run() {}, value: 1, value: 2 };
@@
const object = { run() { return () => super.value; } };
@@
const object = { run() { return new.target; } };
@@
const object = { run() { return this; }, ...other, tail() {} };
@@
const object = { run() { return /a/; } }; /b/.test(value);
@@
const object = { run() { return `${this.value}`; } };
@@
const object = { get\nvalue() {}, set\nvalue(next) {} };
@@
const object = { ['__proto__']() {}, '__proto__'() {} };
@@
const object = { await(await) { return await; } };
@@
const object = { run(value) { var value; return value; } };
@@
const object = { get value() { function nested() { return new.target; } return nested; } };
@@
({ run() {} }).run();
@@
({ get value() { return 1; } }).value / divisor;
''')

add('script', 'Object_methods_and_accessors_reject_invalid_arity_binding_and_super_calls', False, r'''
const object = { get value(arg) {} };
@@
const object = { get value(...rest) {} };
@@
const object = { set value() {} };
@@
const object = { set value(first, second) {} };
@@
const object = { set value(...rest) {} };
@@
const object = { run(first, first) {} };
@@
const object = { run(eval) {} };
@@
const object = { run(value) { let value; } };
@@
const object = { run(value = 1) { 'use strict'; } };
@@
const object = { run() { super(); } };
@@
const object = { constructor() { super(); } };
@@
const object = { run() { function nested() { return super.value; } } };
@@
const object = { get value() { super(); } };
@@
const object = { run() { await value; } };
@@
const object = { run() { yield value; } };
@@
const object = { get value() { await task; } };
@@
const object = { #value() {} };
@@
const object = { static run() {} };
@@
const object = { get value: 1 };
@@
const object = { set value = 1 };
@@
const object = { get async value() {} };
@@
const object = { set * value(next) {} };
@@
const object = { run() {} tail() {} };
@@
const object = { run() {}; tail() {} };
''')

add('script', 'Async_and_generator_methods_work_in_objects_and_classes', True, r'''
const object = { async run() { await work(); } };
@@
const object = { *run() { yield value; } };
@@
const object = { async *run() { yield await value; } };
@@
const object = { async [key]() { return await work(); }, *'run'() { yield value; }, async *1() {} };
@@
const object = { async async() {}, async get() {}, async set() {}, *async() {}, async *async() {} };
@@
const object = { async constructor() {}, *constructor() {}, async *constructor() {} };
@@
const object = { async run() { return super.value; }, *items() { yield super.value; } };
@@
const object = { async *items() { for await (const value of super.values) yield value; } };
@@
const object = { async run() { return () => super.value; } };
@@
const object = { async run() { return new.target; }, *items() { yield new.target; } };
@@
const object = { *\nrun() { yield value; }, async *\nitems() { yield await value; } };
@@
class A { async run() { return await work(); } }
@@
class A { *items() { yield value; } }
@@
class A { async *items() { yield await value; } }
@@
class A { static async run() { await work(); } static *items() { yield value; } static async *all() {} }
@@
class A { async #run() { await work(); } *#items() { yield value; } async *#all() {} }
@@
class A { static async #run() {} static *#items() {} static async *#all() {} }
@@
class A { async run({value}, ...rest) { return value; } *items([first] = list) { yield first; } }
@@
class A extends Base { async run() { return await super.run(); } *items() { yield* super.items(); } }
@@
class A extends Base { async *items() { yield* await super.items(); } }
@@
class A { async ['constructor']() {} *['constructor']() {} async *['constructor']() {} }
@@
class A { static async constructor() {} static *constructor() {} static async *constructor() {} }
@@
class A { async static() {} *static() {} async *static() {} }
@@
class A { async #constructor() {} *#items() {} }
@@
class A { async() {} *async() {} async async() {} async *async() {} }
@@
class A { static async() {} static *async() {} static async async() {} }
@@
class A { async run() { return this.#value; } #value; }
@@
class A { *items() { const f = () => this.#value; yield f; } #value; }
@@
class A { async *items() { for await (const value of source) yield value; } }
@@
class A { async run() { return /a/; } } /b/.test(value);
@@
class A { async run() { return `${await value}`; } *items() { yield `${yield value}`; } }
''')

add('script', 'Async_and_generator_methods_reject_bad_modifiers_constructor_kinds_and_scope', False, r'''
const object = { async\nrun() {} };
@@
const object = { async/*\n*/run() {} };
@@
const object = { async\n*run() {} };
@@
const object = { async run(await) {} };
@@
const object = { *run(yield) {} };
@@
const object = { async *run(await) {} };
@@
const object = { async *run(yield) {} };
@@
const object = { async run(value = await task) {} };
@@
const object = { *run(value = yield task) {} };
@@
const object = { async get value() {} };
@@
const object = { async set value(next) {} };
@@
const object = { *get value() {} };
@@
const object = { *async run() {} };
@@
const object = { async async run() {} };
@@
const object = { async run() { super(); } };
@@
const object = { *run() { super(); } };
@@
class A { async constructor() {} }
@@
class A { *constructor() {} }
@@
class A { async *constructor() {} }
@@
class A { async 'constructor'() {} }
@@
class A { *'constructor'() {} }
@@
class A { static async prototype() {} }
@@
class A { static *prototype() {} }
@@
class A { async #constructor() {} }
@@
class A { async #run() {} *#run() {} }
@@
class A { async run(await) {} }
@@
class A { *run(yield) {} }
@@
class A { async *run(value = await task) {} }
@@
class A { async *run(value = yield task) {} }
@@
class A { async run() { return this.#missing; } }
@@
class A extends Base { async run() { super(); } }
@@
class A extends Base { *run() { super(); } }
@@
class A { async\nrun() { await task; } }
@@
class A { static async\nrun() { await task; } }
@@
class A { async\n*run() { await task; } }
@@
class A { async run() { function inner() { return super.value; } } }
@@
class A { *run(first, first) {} }
@@
class A { async run(value = 1) { 'use strict'; } }
''')

add('script', 'Suspension_contexts_keep_synchronous_nested_scopes_and_method_computed_names_separate', True, r'''
async function f() { class A { [await key]() {} } return A; }
@@
function* f() { class A { [yield key]() {} } yield A; }
@@
async function f() { return { [await key]() {} }; }
@@
function* f() { yield { [yield key]() {} }; }
@@
async function* f() { return { [await key]() {}, [yield value]() {} }; }
@@
async function f() { return class extends (await Base) {}; }
@@
function* f() { yield class extends (yield Base) {}; }
@@
async function f() { return { get value() { return await; } }; }
@@
async function f() { return class { method(await) { return await; } }; }
@@
function* f() { return { async run() { await value; } }; }
@@
async function f() { class A { field = await; } return A; }
@@
async function f() { class A { static field = await; } return A; }
@@
async function f() { return function(await) { return await; }; }
@@
const async = value; async(value); async\nfunction f() {}
@@
let await = value; await += 1; const f = await => await;
@@
async(value); async(value, other); async({value: 1});
''')

add('script', 'Suspension_contexts_do_not_leak_into_nested_synchronous_bodies_or_class_initializers', False, r'''
async function f() { class A { field = await value; } }
@@
async function f() { class A { static field = await value; } }
@@
async function f() { class A { static { await value; } } }
@@
function* f() { class A { field = yield value; } }
@@
function* f() { class A { static { yield value; } } }
@@
async function f() { return { get value() { await task; } }; }
@@
function* f() { return { run() { yield value; } }; }
@@
async function f() { return class { method() { await value; } }; }
@@
async function f(value = class { [await task]() {} }) {}
@@
function* f(value = class { [yield task]() {} }) {}
@@
async function f() { await class { [await value]() {} }; }
''')

add('module', 'Modules_export_all_function_kinds_and_preserve_top_level_await', True, r'''
export async function f() { await task; }
@@
export function* f() { yield value; }
@@
export async function* f() { yield await value; }
@@
export default async function() { await task; }
@@
export default async function named() { await task; }
@@
export default function* () { yield value; }
@@
export default function* named() { yield value; }
@@
export default async function* () { yield await value; }
@@
export default async function* named() { yield await value; }
@@
export const f = async value => await value;
@@
export default async () => await task;
@@
async function f() {} export {f};
@@
function* f() {} export {f};
@@
async function* f() {} export {f};
@@
import value from 'module'; export async function f() { return await value; }
@@
export async function f() { return import.meta; }
@@
export function* f() { yield import.meta; }
@@
export default { async run() { await task; }, *items() { yield value; }, get value() {} };
@@
export class A { async run() { await task; } *items() { yield value; } async *all() {} }
@@
await (async () => await task)();
@@
for await (const value of stream) { await value; }
@@
const object = { [await key]() {}, async run() {}, *items() {} }; export {object};
@@
export async function f() { for await (const value of stream) await value; }
@@
export default async function* () { for await (const value of stream) yield value; }
@@
const f = async () => import.meta.url; export default f;
''')

add('module', 'Modules_reject_reserved_await_duplicate_exports_and_function_scope_errors', False, r'''
export async function await() {}
@@
export function* await() {}
@@
export const f = async await => await;
@@
export function* f() { await value; }
@@
export default function* (await) {}
@@
export default async function* (yield) {}
@@
export async function f() {} export {f};
@@
export default async function() {} export default function* () {}
@@
import f from 'module'; async function f() {}
@@
let f; function* f() {}
@@
export default async\nfunction() {}
@@
export default async *function() {}
@@
export const f = () => await value;
@@
export const object = { run(await) { return await; } };
@@
export const object = { get value() { return await; } };
@@
export async function f() { function inner(await) { return await; } }
@@
export async function* f() { yield\n* values; }
@@
export class A { async constructor() {} }
''')

add('script', 'Parameter_contains_checks_cross_computed_names_and_heritage_but_stop_at_nested_bodies', True, r'''
async function f(value = ({ run() { return await; } })) {}
@@
async function f(value = ({ async run() { await task; } })) {}
@@
async function f(value = ({ get value() { return await; } })) {}
@@
async function f(value = class { field = await; }) {}
@@
async function f(value = class { async run() { await task; } }) {}
@@
function* f(value = ({ *run() { yield task; } })) {}
@@
function* f(value = class { *run() { yield task; } }) {}
@@
function* f(value = function* () { yield task; }) {}
@@
async function* f(value = async function* () { yield await task; }) {}
@@
async function f(value = async () => await task) {}
@@
async function* f(value = async () => await task) {}
''')

add('script', 'Parameter_contains_checks_reject_suspension_in_computed_names_and_heritage', False, r'''
async function f(value = ({ [await task]() {} })) {}
@@
async function f(value = ({ async [await task]() {} })) {}
@@
async function f(value = ({ get [await task]() {} })) {}
@@
async function f(value = ({ [await task]: 1 })) {}
@@
async function f(value = class extends (await Base) {}) {}
@@
async function f(value = class { [await key]() {} }) {}
@@
async function f(value = class { [await key] = 1; }) {}
@@
function* f(value = ({ [yield task]() {} })) {}
@@
function* f(value = ({ *[yield task]() {} })) {}
@@
function* f(value = ({ get [yield task]() {} })) {}
@@
function* f(value = class extends (yield Base) {}) {}
@@
function* f(value = class { [yield key]() {} }) {}
@@
async function* f(value = ({ [await task]() {} })) {}
@@
async function* f(value = ({ [yield task]() {} })) {}
@@
async function f(value = await => await) {}
@@
async function f(value = (await) => await) {}
@@
async function f(value = ({await}) => await) {}
''')

add('expression', 'Expression_entry_accepts_function_expressions_async_arrows_and_object_methods', True, r'''
async function() { return await task; }
@@
async function named(value) { return await value; }
@@
function* () { yield value; }
@@
function* named() { yield* values; }
@@
async function* () { yield await value; }
@@
async function* named() { yield* await values; }
@@
async value => await value
@@
async () => await task
@@
async ({value} = source, ...rest) => value
@@
({ run() {}, get value() {}, set value(next) {} })
@@
({ async run() { await task; }, *items() { yield value; }, async *all() {} })
@@
({ async run() { return super.value; } })
@@
({ get value() { return super.value; } })
@@
({ async: value, get: other, set: third })
@@
class { async run() { await task; } *items() { yield value; } async *all() {} }
@@
(async () => /a/) / divisor
@@
async () => `${await value}`
@@
function* () { yield /a/; }
@@
async function() { return import('module'); }
@@
async(await)
@@
await => await
@@
({ await(await) { return await; } })
@@
async () => ({ [await key]() {} })
''')

add('expression', 'Expression_entry_rejects_statement_only_forms_and_invalid_function_contexts', False, r'''
async function() { yield value; }
@@
function* () { await value; }
@@
async function* (await) {}
@@
async function* (yield) {}
@@
async await => await
@@
async (value = await task) => value
@@
async\n() => await task
@@
async value\n=> value
@@
async () => await
@@
async () => new.target
@@
async () => super.value
@@
({ get value(arg) {} })
@@
({ set value() {} })
@@
({ async constructor() { super(); } })
@@
async function() { return import.meta; }
@@
async () => import.meta
@@
function* () { yield import.meta; }
@@
class { async constructor() {} }
@@
async function f() {} next()
@@
for await (const value of source) {}
''')

add('script', 'Contextual_function_keywords_distinguish_escaped_identifiers_and_method_names', True, r'''
async function await() {}
@@
async function f() { return { await: await task }; }
@@
const f = async function async() {};
@@
const object = { \u0061sync() {}, \u0067et() {}, \u0073et() {} };
@@
const object = { async \u0072un() { await task; }, *\u0072un() { yield value; } };
@@
const object = { get \u0076alue() {}, set \u0076alue(value) {} };
@@
class A { async \u0072un() { await task; } *\u0072un() { yield value; } }
@@
class A { \u0061sync() {} \u0067et() {} \u0073et() {} }
@@
const object = { async await() {}, *yield() {}, async *await() {} };
@@
class A { async await() {} *yield() {} async *await() {} }
@@
const f = async function f({await: value}) { return value; };
@@
const f = function* f({yield: value}) { yield value; };
@@
const f = async ({await: value}) => value;
@@
async function f() { const object = { async: 1, await: 2, yield: 3 }; return object.await; }
''')

add('script', 'Escaped_async_await_yield_get_and_set_do_not_become_syntactic_modifiers', False, r'''
\u0061sync function f() {}
@@
const f = \u0061sync function() {};
@@
const f = \u0061sync value => value;
@@
const object = { \u0061sync run() {} };
@@
const object = { \u0067et value() {} };
@@
const object = { \u0073et value(value) {} };
@@
class A { \u0061sync run() {} }
@@
async function f() { \u0061wait value; }
@@
function* f() { \u0079ield value; }
@@
const f = async function await() {};
@@
async function f() { async function await() {} }
@@
async function f() { const await = value; }
@@
const f = async \u0061wait => value;
@@
const object = { async run(\u0061wait) {} };
@@
const object = { *run(\u0079ield) {} };
''')

add('script', 'Function_kinds_preserve_parameter_body_scope_return_and_loop_boundaries', True, r'''
async function f(value) { var value; return value; }
@@
function* f(value) { var value; yield value; }
@@
async function* f(value) { var value; yield await value; }
@@
async function f(value) { function value() {} return value; }
@@
function* f(value) { function value() {} yield value; }
@@
async function* f(value) { function value() {} yield value; }
@@
async function f() { let value; { let value; } }
@@
function* f() { let value; { let value; } }
@@
async function f() { outer: while (value) { await step(); continue outer; } }
@@
function* f() { outer: while (value) { yield step(); continue outer; } }
@@
const object = { run(value) { function value() {} return value; } };
@@
class A { async run(value) { function value() {} return value; } }
@@
for (let f = (async () => key in source);;) { break; }
@@
for (let object = { run() { return key in source; } };;) { break; }
@@
async function f() { for (let value = (await (key in source));;) { break; } }
''')

add('script', 'Function_kinds_reset_flow_control_and_reject_lexical_declaration_conflicts', False, r'''
async function f(value) { class value {} }
@@
function* f(value) { let value; }
@@
async function* f(value) { const value = 1; }
@@
const object = { run(value) { const value = 1; } };
@@
const object = { get value() { let value; var value; } };
@@
const object = { set value(value) { let value; } };
@@
while (ready) { async function f() { break; } }
@@
while (ready) { function* f() { continue; } }
@@
outer: while (ready) { async function* f() { break outer; } }
@@
outer: while (ready) { const object = { run() { continue outer; } }; }
@@
const object = { get value() { break; } };
@@
async function f() { let value; function* value() {} }
@@
function* f() { const value = 1; async function value() {} }
@@
let f; async function f() {}
@@
let f; async function* f() {}
@@
for (let f = async () => key in source;;) { break; }
''')

add('script', 'Method_super_and_new_target_context_is_inherited_only_by_arrows', True, r'''
const object = { async run(value = super.value) { return await value; } };
@@
const object = { *run(value = super.value) { yield value; } };
@@
const object = { async *run(value = super.value) { yield await value; } };
@@
const object = { run() { return async () => super.value; } };
@@
const object = { run() { return async () => new.target; } };
@@
class A extends Base { async run(value = super.value) { return await value; } }
@@
class A extends Base { *run(value = super.value) { yield value; } }
@@
class A extends Base { constructor() { const f = async () => super(); } }
@@
class A extends Base { run() { return { [super.value]() {} }; } }
@@
class A { #value; async run() { function* inner() { yield object.#value; } return inner; } }
@@
class A { #value; *run() { const object = { async run() { return this.#value; } }; yield object; } }
@@
function f() { return { [new.target]() {} }; }
''')

add('script', 'Nested_non_arrow_functions_do_not_inherit_super_from_methods', False, r'''
const object = { run() { return async function() { return super.value; }; } };
@@
const object = { run() { return function* () { yield super.value; }; } };
@@
const object = { run() { return async function* () { yield await super.value; }; } };
@@
class A extends Base { run() { return async function() { super(); }; } }
@@
class A extends Base { constructor() { async function nested() { super(); } } }
@@
class A { async run() { return async () => super(); } }
@@
const object = { [super.value]() {} };
@@
const object = { [new.target]() {} };
@@
async function f(value = super.value) {}
@@
function* f(value = super.value) {}
@@
async function* f(value = super.value) {}
''')

corrections = {
    'const f = async (value = await => await) => value;': (False, 'Async_arrows_enforce_line_terminators_parameters_and_lexical_context'),
    'function* f() { for ((yield value) in source) {} }': (False, 'Generators_reject_yield_in_parameters_wrong_function_kinds_and_illegal_targets'),
    'function* f() { yield** values; }': (False, 'Generators_reject_yield_in_parameters_wrong_function_kinds_and_illegal_targets'),
    'async function* f() { yield await yield value; }': (False, 'Async_generators_reject_both_suspension_keywords_in_parameters'),
    'class A { async #constructor() {} *#items() {} }': (False, 'Async_and_generator_methods_reject_bad_modifiers_constructor_kinds_and_scope'),
    'async function f() { class A { static field = await; } return A; }': (False, 'Suspension_contexts_do_not_leak_into_nested_synchronous_bodies_or_class_initializers'),
    'async function f() { await class { [await value]() {} }; }': (True, 'Suspension_contexts_keep_synchronous_nested_scopes_and_method_computed_names_separate'),
    'export default async *function() {}': (True, 'Modules_export_all_function_kinds_and_preserve_top_level_await'),
}
for item in groups:
    if item['text'] in corrections:
        item['valid'], item['group'] = corrections[item['text']]
    # The hand-authored corpus spells source line breaks explicitly. Preserve
    # escaped identifiers and JavaScript strings while expanding only \n here.
    item['text'] = item['text'].replace(r'\n', '\n')

folder = Path('artifacts/function-kinds')
folder.mkdir(parents=True, exist_ok=True)
(folder / 'cases.json').write_text(json.dumps(groups, ensure_ascii=False, indent=2), encoding='utf-8')
print(f'{len(groups)} cases')

by_goal = {}
for item in groups:
    by_goal.setdefault(item['goal'], {}).setdefault((item['group'], item['valid']), []).append(item['text'])

output = '''using System;
using System.Linq;
using System.Threading.Tasks;
using Janglim.FrontEnd.Grammars.JavaScript;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMAScript 2026 sections 13.3.9, 13.3.14, 13.4, 14.7.5 and 15.4-15.8.
// Each fixture constructs its grammar/table once. Tests only parse and validate:
// user function bodies, getters, promises, iterators and imports are never run.
// Corpus expectations were independently compiled with Node.js 24.12.0 without
// evaluation. Context boundaries are checked in both positive and negative cases.
// https://tc39.es/ecma262/2026/multipage/ecmascript-language-functions-and-classes.html
'''
fixtures = {'script': 'Script', 'module': 'Module', 'expression': 'Expression'}
for goal, methods in by_goal.items():
    kind = fixtures[goal]
    output += f'''
[Collection(JavaScript{kind}Collection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptFunctionKinds")]
public sealed class JavaScriptFunctionKinds{kind}Tests
{{
    private readonly JavaScript{kind}Fixture _fixture;
    public JavaScriptFunctionKinds{kind}Tests(JavaScript{kind}Fixture fixture) => _fixture = fixture;
'''
    for (method, valid), texts in methods.items():
        output += '\n    [Theory]\n'
        output += ''.join('    [InlineData(' + json.dumps(text, ensure_ascii=False) + ')]\n' for text in texts)
        output += f'    public void {method}(string input)\n        => _fixture.{"Success" if valid else "Invalid"}(input);\n'
    if goal == 'script':
        output += '''
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
'''
    output += '}\n'

Path('Tests/Parse.FrontEnd.Parsers.Tests/JavaScript/JavaScriptFunctionKindsTests.cs').write_text(output, encoding='utf-8')
