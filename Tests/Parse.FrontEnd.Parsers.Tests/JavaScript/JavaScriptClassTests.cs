using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.LR;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMAScript 2026 section 15.7. These are strict Script syntax checks. The test
// programs are parsed and validated; no JavaScript constructor or body is executed.
// Async and generator methods have additional function-kind integration tests.
[Collection(JavaScriptScriptCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptClasses")]
public sealed class JavaScriptClassTests
{
    private readonly JavaScriptScriptFixture _fixture;
    public JavaScriptClassTests(JavaScriptScriptFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("class A {}")]
    [InlineData("class A {}; new A();")]
    [InlineData("class A { ;;; }")]
    [InlineData("const A = class {};")]
    [InlineData("const A = class Named {};")]
    [InlineData("const A = class extends Base {};")]
    [InlineData("class A extends Base {}")]
    [InlineData("class A extends null {}")]
    [InlineData("class A extends undefined {}")]
    [InlineData("class A extends namespace.Base {}")]
    [InlineData("class A extends namespace['Base'] {}")]
    [InlineData("class A extends factory(argument) {}")]
    [InlineData("class A extends (condition ? First : Second) {}")]
    [InlineData("class A extends (first, second) {}")]
    [InlineData("class A extends (value => value) {}")]
    [InlineData("class A extends class B {} {}")]
    [InlineData("class A extends class { method() { return 1; } } {}")]
    [InlineData("class A extends new BaseFactory() {}")]
    [InlineData("class A extends (class {}) {}")]
    [InlineData("class A extends tag`base` {}")]
    [InlineData("class A extends object?.Base {}")]
    [InlineData("const A = class {} / divisor;")]
    [InlineData("const A = (class {}).name;")]
    [InlineData("const A = new class { value = 1; };")]
    [InlineData("call(class {}, class extends Base {});")]
    [InlineData("const list = [class {}, class Named {}];")]
    [InlineData("const value = {Constructor: class {}};")]
    [InlineData("const make = () => class { method() { return 1; } };")]
    [InlineData("function make() { return class Named {}; }")]
    [InlineData("{ class A {} } { class A {} }")]
    [InlineData("let A; { class A {} }")]
    [InlineData("const Outer = class Inner extends Inner {};")]
    [InlineData("class 한글 { 메서드() { return 1; } }")]
    [InlineData("class \\u0041 { method() { return A; } }")]
    [InlineData("class await {}")]
    [InlineData("for (const A = class { field = key in object; }; ready; ) ;")]
    [InlineData("for (const A = class extends (key in object) {}; ready; ) ;")]
    public void Class_declarations_expressions_and_left_hand_side_heritage_are_accepted(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("class {}")]
    [InlineData("class extends Base {}")]
    [InlineData("class A")]
    [InlineData("class A extends {}")]
    [InlineData("class A extends Base")]
    [InlineData("class A extends First, Second {}")]
    [InlineData("class A extends condition ? First : Second {}")]
    [InlineData("class A extends First + Second {}")]
    [InlineData("class A extends value => value {}")]
    [InlineData("class A extends !Base {}")]
    [InlineData("class A extends Base = Other {}")]
    [InlineData("class A extends (Base,) {}")]
    [InlineData("class A {} class A {}")]
    [InlineData("let A; class A {}")]
    [InlineData("class A {} var A;")]
    [InlineData("function A() {} class A {}")]
    [InlineData("if (ready) class A {}")]
    [InlineData("while (ready) class A {}")]
    [InlineData("label: class A {}")]
    [InlineData("class eval {}")]
    [InlineData("class arguments {}")]
    [InlineData("class yield {}")]
    [InlineData("class static {}")]
    [InlineData("class let {}")]
    [InlineData("class implements {}")]
    public void Malformed_heritage_strict_class_names_and_conflicting_declarations_are_rejected(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("class A { constructor() {} }")]
    [InlineData("class A { constructor(value) { this.value = value; } }")]
    [InlineData("class A extends Base { constructor(value) { super(value); } }")]
    [InlineData("class A { 'constructor'() {} }")]
    [InlineData("class A { '\\u0063onstructor'() {} }")]
    [InlineData("class A { \\u0063onstructor() {} }")]
    [InlineData("class A { method() {} }")]
    [InlineData("class A { method(value) { return value; } }")]
    [InlineData("class A { method(value = 1, ...rest) { return rest; } }")]
    [InlineData("class A { method({name = 'x'}, [first], ...rest) { return name; } }")]
    [InlineData("class A { method(...[first, second]) { return first; } }")]
    [InlineData("class A { method(...{length}) { return length; } }")]
    [InlineData("class A { [key]() {} ['name']() {} [(first, second)]() {} }")]
    [InlineData("class A { 0() {} 1n() {} 'text'() {} }")]
    [InlineData("class A { if() {} return() {} class() {} function() {} }")]
    [InlineData("class A { static() {} get() {} set() {} async() {} }")]
    [InlineData("class A { static method() { return this; } }")]
    [InlineData("class A { static [key]() {} static 1() {} }")]
    [InlineData("class A { static constructor() {} constructor() {} }")]
    [InlineData("class A { ['constructor']() {} constructor() {} }")]
    [InlineData("class A { constructor() {} ['constructor']() {} }")]
    [InlineData("class A { method() {} method() {} }")]
    [InlineData("class A { static method() {} method() {} }")]
    [InlineData("class A { get value() { return this._value; } }")]
    [InlineData("class A { set value(value) { this._value = value; } }")]
    [InlineData("class A { get value() { return 1; } set value(value) {} }")]
    [InlineData("class A { set value(value = 1) {} }")]
    [InlineData("class A { set value({first, second}) {} }")]
    [InlineData("class A { set value([first, ...rest]) {} }")]
    [InlineData("class A { static get value() { return 1; } static set value(value) {} }")]
    [InlineData("class A { get [key]() {} set [key](value) {} }")]
    [InlineData("class A { get 1() {} set 'name'(value) {} }")]
    [InlineData("class A { get static() {} set async(value) {} }")]
    [InlineData("class A { static\nmethod() {} get\nvalue() {} set\nvalue(value) {} }")]
    [InlineData("class A { static\nconstructor() {} }")]
    [InlineData("class A { async\nmethod() {} }")]
    [InlineData("class A { get value() {} get value() {} }")]
    [InlineData("class A { method(value) { var value; return value; } }")]
    [InlineData("class A { method(value) { { let value; } return value; } }")]
    [InlineData("class A { method(value) { function value() {} } }")]
    [InlineData("class A { method() { while (ready) break; } }")]
    [InlineData("class A { method() { label: while (ready) continue label; } }")]
    [InlineData("class A { method() { return () => this; } }")]
    [InlineData("class A { method() { return class B {}; } }")]
    public void Constructors_methods_and_accessors_use_function_bodies_and_binding_parameters(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("class A { constructor() {} constructor() {} }")]
    [InlineData("class A { constructor() {} 'constructor'() {} }")]
    [InlineData("class A { '\\u0063onstructor'() {} constructor() {} }")]
    [InlineData("class A { \\u0063onstructor() {} constructor() {} }")]
    [InlineData("class A { get constructor() {} }")]
    [InlineData("class A { set constructor(value) {} }")]
    [InlineData("class A { get 'constructor'() {} }")]
    [InlineData("class A { static prototype() {} }")]
    [InlineData("class A { static 'prototype'() {} }")]
    [InlineData("class A { static \\u0070rototype() {} }")]
    [InlineData("class A { static get prototype() {} }")]
    [InlineData("class A { static set prototype(value) {} }")]
    [InlineData("class A { method(first, first) {} }")]
    [InlineData("class A { method({name}, name) {} }")]
    [InlineData("class A { method(first, \\u0066irst) {} }")]
    [InlineData("class A { method(eval) {} }")]
    [InlineData("class A { method({arguments}) {} }")]
    [InlineData("class A { method(...yield) {} }")]
    [InlineData("class A { method(value) { let value; } }")]
    [InlineData("class A { method({value}) { const value = 1; } }")]
    [InlineData("class A { method(value = 1) { 'use strict'; } }")]
    [InlineData("class A { method(...rest) { 'use strict'; } }")]
    [InlineData("class A { method({value}) { 'use strict'; } }")]
    [InlineData("class A { get value(first) {} }")]
    [InlineData("class A { get value(...rest) {} }")]
    [InlineData("class A { set value() {} }")]
    [InlineData("class A { set value(first, second) {} }")]
    [InlineData("class A { set value(...rest) {} }")]
    [InlineData("class A { set value(value = 1) { 'use strict'; } }")]
    [InlineData("class A { method(...rest,) {} }")]
    [InlineData("class A { method(value += 1) {} }")]
    [InlineData("class A { method() { break; } }")]
    [InlineData("while (ready) { class A { method() { continue; } } }")]
    [InlineData("outer: while (ready) { class A { method() { break outer; } } }")]
    [InlineData("class A { method() { with (object) {} } }")]
    [InlineData("class A { method() { delete value; } }")]
    [InlineData("class A { method() { return 010; } }")]
    [InlineData("class A { method() => value; }")]
    [InlineData("class A { function method() {} }")]
    [InlineData("class A { method: function() {} }")]
    [InlineData("class A { method() {}, other() {} }")]
    [InlineData("class A { [first, second]() {} }")]
    public void Invalid_constructor_accessors_parameters_and_method_control_contexts_are_rejected(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("class A { value; }")]
    [InlineData("class A { value }")]
    [InlineData("class A { first; second; }")]
    [InlineData("class A { first\nsecond }")]
    [InlineData("class A { first = 1; second = first + 1; }")]
    [InlineData("class A { first = 1\nsecond = 2 }")]
    [InlineData("class A { value = this; }")]
    [InlineData("class A { value = new.target; }")]
    [InlineData("class A { value = () => this; }")]
    [InlineData("class A { value = () => new.target; }")]
    [InlineData("class A { value = () => { return this; }; }")]
    [InlineData("class A { value = function() { return arguments; }; }")]
    [InlineData("class A { value = function() { return new.target; }; }")]
    [InlineData("class A { value = this.arguments; }")]
    [InlineData("class A { value = {arguments: 1}; }")]
    [InlineData("class A { value = class { method() { return arguments; } }; }")]
    [InlineData("class A { value = class B { method() { return this; } }; }")]
    [InlineData("class A { value = class { [new.target]() {} }; }")]
    [InlineData("class A { [key]; [other] = 2; }")]
    [InlineData("class A { [arguments] = 1; [arguments]() {} }")]
    [InlineData("function f() { class A { [new.target] = 1; [arguments]() {} } }")]
    [InlineData("class A { [(first, second)] = 2; }")]
    [InlineData("class A { 'name' = 'value'; 0 = 1; 1n = 2; }")]
    [InlineData("class A { if; return; class; function; }")]
    [InlineData("class A { static; get; set; async; }")]
    [InlineData("class A { static = 1; get = 2; set = 3; async = 4; }")]
    [InlineData("class A { static value; static other = 1; }")]
    [InlineData("class A { static static; static get; static set; static async; }")]
    [InlineData("class A { static [key] = 1; static 0 = 1; }")]
    [InlineData("class A { ['constructor'] = 1; static ['prototype'] = 2; }")]
    [InlineData("class A { value; value; method() {} method; }")]
    [InlineData("class A { value = total\n/ divisor / other; }")]
    [InlineData("class A { value = /a/.test(input); }")]
    [InlineData("class A { value = `hello ${name}`; }")]
    [InlineData("class A { value = tag`raw\\xZ`; }")]
    [InlineData("class A { value = {name: 1}; other = [1, 2]; }")]
    [InlineData("class A { value = (first, second); }")]
    [InlineData("class A { value = call\n(argument); }")]
    [InlineData("class A { value = object\n[key]; }")]
    [InlineData("class A { value = tag\n`text`; }")]
    [InlineData("class A { value = () => {}\nother = 1 }")]
    [InlineData("class A { value = () => {}\n[property]; }")]
    [InlineData("class A { value = () => {}\n[property] = 1; }")]
    [InlineData("class A { static\nvalue = 1; }")]
    [InlineData("class A { async\nmethod; }")]
    public void Fields_use_assignment_expressions_and_class_element_semicolon_insertion(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("class A { first second }")]
    [InlineData("class A { first = 1 second = 2 }")]
    [InlineData("class A { first, second; }")]
    [InlineData("class A { first = 1, second = 2; }")]
    [InlineData("class A { [first, second] = 2; }")]
    [InlineData("class A { get\nvalue; }")]
    [InlineData("class A { field += 1; }")]
    [InlineData("class A { field: 1; }")]
    [InlineData("class A { field = ; }")]
    [InlineData("class A { constructor; }")]
    [InlineData("class A { 'constructor' = 1; }")]
    [InlineData("class A { static constructor = 1; }")]
    [InlineData("class A { static prototype; }")]
    [InlineData("class A { static 'prototype' = 1; }")]
    [InlineData("class A { static \\u0070rototype = 1; }")]
    [InlineData("class A { value = arguments; }")]
    [InlineData("class A { value = () => arguments; }")]
    [InlineData("class A { value = (argument = arguments) => argument; }")]
    [InlineData("class A { value = {arguments}; }")]
    [InlineData("class A { value = arguments => arguments; }")]
    [InlineData("class A { value = () => { return arguments; }; }")]
    [InlineData("class A { value = class { [arguments] = 1; }; }")]
    [InlineData("class A { value = `bad\\xZ`; }")]
    [InlineData("class A { value = ({value = 1}); }")]
    [InlineData("class A { value = () => {} other = 1; }")]
    [InlineData("class A { value = () => {}\n(argument); }")]
    [InlineData("class A { value = () => {}\n`text`; }")]
    [InlineData("class A { value = () => {}\n+1; }")]
    public void Invalid_field_names_initializers_and_missing_class_element_terminators_are_rejected(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("class A { #value; }")]
    [InlineData("class A { #value = 1; get() { return this.#value; } }")]
    [InlineData("class A { get() { return this.#value; } #value = 1; }")]
    [InlineData("class A { #value; get(object) { return object.#value; } }")]
    [InlineData("class A { #value; get(object) { return object?.#value; } }")]
    [InlineData("class A { #value; has(object) { return #value in object; } }")]
    [InlineData("class A { #value; has(object) { return #value in object && ready; } }")]
    [InlineData("class A { #value; has(object) { return !(#value in object); } }")]
    [InlineData("class A { #value; set(value) { this.#value = value; this.#value++; } }")]
    [InlineData("class A { #value; set(value) { this.#value += value; } }")]
    [InlineData("class A { #method() {} run() { return this.#method(); } }")]
    [InlineData("class A { static #value = 1; static get() { return this.#value; } }")]
    [InlineData("class A { static #method() {} run() { return A.#method(); } }")]
    [InlineData("class A { get #value() { return 1; } set #value(value) {} }")]
    [InlineData("class A { set #value(value) {} get #value() { return 1; } }")]
    [InlineData("class A { static get #value() {} static set #value(value) {} }")]
    [InlineData("class A { static { object.#await; } #await; }")]
    [InlineData("class A { #if; #static; #await; #yield; }")]
    [InlineData("class A { #한글; method() { return this.#한글; } }")]
    [InlineData("class A { #\\u0078; method() { return this.#x; } }")]
    [InlineData("class A { #value; method() { return () => this.#value; } }")]
    [InlineData("class A { #value; method() { return function(object) { return object.#value; }; } }")]
    [InlineData("class A { #value; method() { return class B { get(object) { return object.#value; } }; } }")]
    [InlineData("class A { #value; Inner = class { get(object) { return object.#value; } }; }")]
    [InlineData("class A { #value; method() { return class B { #value; get(object) { return object.#value; } }; } }")]
    [InlineData("class A { #value; method() { return class B { #other; get(object) { return object.#value + object.#other; } }; } }")]
    [InlineData("class A { #value; method() { return class B extends this.#value {}; } }")]
    [InlineData("class A { #value; method() { return class B extends object.#value { #value; }; } }")]
    [InlineData("class A { #value; [this.#value]() {} }")]
    [InlineData("class A { #value; [#value in object]() {} }")]
    [InlineData("class A { #value; static { object.#value; } }")]
    [InlineData("class A { #value; value = object?.#value; }")]
    [InlineData("class A { #value; value = `${object.#value}`; }")]
    [InlineData("class A { #value; value = /a/.test(object.#value); }")]
    public void Private_names_have_class_wide_visibility_and_lexical_nested_class_capture(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("object.#value;")]
    [InlineData("#value in object;")]
    [InlineData("class A { method() { return this.#missing; } }")]
    [InlineData("class A { value = this.#missing; }")]
    [InlineData("class A { static { object.#missing; } }")]
    [InlineData("class A { method() { return #missing in object; } }")]
    [InlineData("class A { #constructor; }")]
    [InlineData("class A { #constructor() {} }")]
    [InlineData("class A { #\\u0063onstructor; }")]
    [InlineData("class A { static #constructor; }")]
    [InlineData("class A { #value; #value; }")]
    [InlineData("class A { #value; #value() {} }")]
    [InlineData("class A { #value() {} #value() {} }")]
    [InlineData("class A { #value; static #value; }")]
    [InlineData("class A { get #value() {} get #value() {} }")]
    [InlineData("class A { set #value(value) {} set #value(value) {} }")]
    [InlineData("class A { get #value() {} set #value(value) {} get #value() {} }")]
    [InlineData("class A { set #value(value) {} get #value() {} set #value(value) {} }")]
    [InlineData("class A { get #value() {} static set #value(value) {} }")]
    [InlineData("class A { static get #value() {} set #value(value) {} }")]
    [InlineData("class A { #x; #\\u0078; }")]
    [InlineData("class A { #value; method() { delete this.#value; } }")]
    [InlineData("class A { #value; method() { delete (this.#value); } }")]
    [InlineData("class A { #value; method() { delete this?.#value; } }")]
    [InlineData("class A { #value; method() { return #value; } }")]
    [InlineData("class A { #value; method() { return (#value) in object; } }")]
    [InlineData("class A { #value; method() { return object[#value]; } }")]
    [InlineData("class A { #value; method() { return #value instanceof object; } }")]
    [InlineData("class A { #value; method() { return #value + object; } }")]
    [InlineData("class A { #value; method() { #value in object = true; } }")]
    [InlineData("class A { #value; method() { return object.#other; } }")]
    [InlineData("class A { method() { return class B { #value; }; } field = object.#value; }")]
    [InlineData("class A extends object.#value { #value; }")]
    [InlineData("class A { #value; } class B { method() { return object.#value; } }")]
    [InlineData("class A { #value; } object.#value;")]
    [InlineData("class A { #[key]; }")]
    [InlineData("class A { # value; }")]
    [InlineData("class A { #1; }")]
    public void Undeclared_duplicate_constructor_and_invalid_private_operations_are_rejected(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("class A extends Base { constructor() { super(); } }")]
    [InlineData("class A extends Base { constructor(value = super()) {} }")]
    [InlineData("class A extends Base { constructor() { const f = () => super(); } }")]
    [InlineData("class A extends Base { constructor() { const f = () => () => super(); } }")]
    [InlineData("class A { method() { return super.value; } }")]
    [InlineData("class A { method(value = super.value) { return value; } }")]
    [InlineData("class A { get value() { return super.value; } }")]
    [InlineData("class A { set value(value) { super.value = value; } }")]
    [InlineData("class A { static method() { return super.value; } }")]
    [InlineData("class A { method() { return super[key](value); } }")]
    [InlineData("class A { method() { super.value = 1; super.value++; } }")]
    [InlineData("class A { method() { return () => super.value; } }")]
    [InlineData("class A { method() { return class B { [super.value]() {} }; } }")]
    [InlineData("class A { field = super.value; static field = super.value; }")]
    [InlineData("class A { field = () => super.value; }")]
    [InlineData("class A { static { super.value; } }")]
    [InlineData("class A { constructor() { return new.target; } }")]
    [InlineData("class A { method(value = new.target) { return new.target; } }")]
    [InlineData("class A { method() { return new.target(); } }")]
    [InlineData("class A { field = new.target; static field = new.target; }")]
    [InlineData("class A { static { new.target; } }")]
    [InlineData("function f(value = new.target) { return new.target; }")]
    [InlineData("function f() { return () => new.target; }")]
    [InlineData("class A { method() { return function() { return new.target; }; } }")]
    public void Super_and_new_target_follow_method_constructor_field_and_arrow_context(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("super.value;")]
    [InlineData("super();")]
    [InlineData("super;")]
    [InlineData("new.target;")]
    [InlineData("const f = () => new.target;")]
    [InlineData("function f() { return super.value; }")]
    [InlineData("class A { constructor() { super(); } }")]
    [InlineData("class A extends Base { method() { super(); } }")]
    [InlineData("class A extends Base { static constructor() { super(); } }")]
    [InlineData("class A extends Base { get constructor() { super(); } }")]
    [InlineData("class A extends Base { ['constructor']() { super(); } }")]
    [InlineData("class A extends Base { field = super(); }")]
    [InlineData("class A extends Base { static { super(); } }")]
    [InlineData("class A extends Base { constructor() { function f() { super(); } } }")]
    [InlineData("class A { method() { function f() { return super.value; } } }")]
    [InlineData("class A { field = function() { return super.value; }; }")]
    [InlineData("class A { #value; method() { return super.#value; } }")]
    [InlineData("class A { method() { return super?.value; } }")]
    [InlineData("class A { method() { return super?.(); } }")]
    [InlineData("class A { method() { return new super(); } }")]
    [InlineData("class A { method() { return new.other; } }")]
    [InlineData("class A { method() { return new.\\u0074arget; } }")]
    [InlineData("class A extends Base { constructor() { const f = () => class B { constructor() { super(); } }; } }")]
    [InlineData("class A extends Base { constructor() { const f = () => class B { field = super(); }; } }")]
    [InlineData("class A extends super.Base {}")]
    [InlineData("class A extends new.target {}")]
    [InlineData("class A { [super.value]() {} }")]
    [InlineData("class A { [new.target] = 1; }")]
    public void Super_and_new_target_are_rejected_outside_their_permitted_lexical_contexts(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("class A { static {} }")]
    [InlineData("class A { static {} static {} }")]
    [InlineData("class A { static { this.value = 1; } }")]
    [InlineData("class A { static { var value; let other; const third = 1; } }")]
    [InlineData("class A { static { function f() { return 1; } f(); } }")]
    [InlineData("class A { static { class B {} } }")]
    [InlineData("class A { static { for (;;) { break; } } }")]
    [InlineData("class A { static { label: while (ready) continue label; } }")]
    [InlineData("class A { static { if (ready) /a/.test(value); } }")]
    [InlineData("class A { static { try {} catch (error) {} } }")]
    [InlineData("class A { static { const f = () => { return 1; }; } }")]
    [InlineData("class A { static { const f = function() { return arguments; }; } }")]
    [InlineData("class A { static { var value; } static { let value; } }")]
    [InlineData("class A { static { let value; } method() { let value; } }")]
    [InlineData("class A { static\n{ value(); } }")]
    [InlineData("class A { static { let awaitValue; } }")]
    [InlineData("class A { static { const object = {await: 1, arguments: 2}; } }")]
    [InlineData("class A { static { object.arguments; } }")]
    [InlineData("class A { static { function f() { let await; } } }")]
    [InlineData("class A { static { const f = function await() {}; } }")]
    [InlineData("class A { static { var value; function value() {} } }")]
    [InlineData("class A { static { function value() {} function value() {} } }")]
    [InlineData("class A { static { const f = () => await; } }")]
    [InlineData("class A { field = class { static { const f = function() { return arguments; }; } }; }")]
    public void Static_initialization_blocks_have_their_own_declarations_and_control_scope(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("class A { static { return; } }")]
    [InlineData("function f() { class A { static { return; } } }")]
    [InlineData("class A { static { return value; } }")]
    [InlineData("class A { static { break; } }")]
    [InlineData("while (ready) { class A { static { break; } } }")]
    [InlineData("while (ready) { class A { static { continue; } } }")]
    [InlineData("outer: while (ready) { class A { static { break outer; } } }")]
    [InlineData("class A { static { arguments; } }")]
    [InlineData("class A { static { const f = () => arguments; } }")]
    [InlineData("class A { static { const f = () => { return arguments; }; } }")]
    [InlineData("class A { static { let value; var value; } }")]
    [InlineData("class A { static { let value; let value; } }")]
    [InlineData("class A { static { function f() {} let f; } }")]
    [InlineData("class A { static { var await; } }")]
    [InlineData("class A { static { function await() {} } }")]
    [InlineData("class A { static { let await; } }")]
    [InlineData("class A { static { const f = (await) => 1; } }")]
    [InlineData("class A { static { await value; } }")]
    [InlineData("class A { static { await; } }")]
    [InlineData("class A { static { yield value; } }")]
    [InlineData("class A { static { with (object) {} } }")]
    [InlineData("class A { static { delete value; } }")]
    [InlineData("class A { static static {} }")]
    [InlineData("class A { get {} }")]
    [InlineData("class A { { value(); } }")]
    public void Static_blocks_do_not_inherit_outer_function_loop_or_arguments_context(string input)
        => _fixture.Invalid(input);

    // The published grammar is authoritative for these cases. Node 24.12 differs:
    // FieldDefinition[Yield, Await] passes ?Await to Initializer, so a Script field
    // accepts await as an identifier; a nested class inside a static block inherits
    // that block's +Await. PropertySetParameterList has exactly one FormalParameter
    // and no trailing-comma production. Do not rewrite these to match a host runtime.
    // https://262.ecma-international.org/17.0/#prod-FieldDefinition
    // https://262.ecma-international.org/17.0/#prod-PropertySetParameterList
    [Theory]
    [InlineData("class A { field = await; static field = await; }", true)]
    [InlineData("class A { static { class B { field = await; } } }", false)]
    [InlineData("class A { set value(value,) {} }", false)]
    public void Published_ECMAScript_grammar_controls_Await_parameters_and_setter_arity(string input, bool accepted)
    {
        if (accepted) _fixture.Success(input);
        else _fixture.Invalid(input);
    }

    [Theory]
    [InlineData("class A {} /a/.test(value);")]
    [InlineData("class A extends Base {} /a/.test(value);")]
    [InlineData("const A = class {} / divisor;")]
    [InlineData("(class {}) / divisor;")]
    [InlineData("new class {} / divisor;")]
    [InlineData("const A = class extends Base { method() { return /a/; } } / divisor;")]
    [InlineData("class A { method() { return /a/; } other() { return value / divisor; } }")]
    [InlineData("class A { field = /a/; other = value / divisor; }")]
    [InlineData("class A { field = () => /a/; other = value => value / divisor; }")]
    [InlineData("class A { [/a/.source]() { return /b/; } }")]
    [InlineData("class A { [value / divisor]() { return /a/; } }")]
    [InlineData("class A { method() {} field = /a/; }")]
    [InlineData("class A { field = () => {}\nmethod() { return /a/; } }")]
    [InlineData("class A { static { if (ready) /a/.test(value); } field = /b/; }")]
    [InlineData("const text = `class ${class A { value = /a/; }} done`;")]
    [InlineData("class A { value = `outer ${class B { field = `${name}`; }}`; }")]
    [InlineData("class A { method() { return `value ${this.value / divisor}`; } }")]
    [InlineData("class A { [tag`key ${value}`]() {} }")]
    [InlineData("class A { method() { return class B {} / divisor; } }")]
    [InlineData("const make = () => class A { method() {} }; /a/.test(value);")]
    [InlineData("const value = condition ? class A {} : class B {} / divisor;")]
    [InlineData("class A { method() { return\n/a/; } }")]
    [InlineData("class A { method() { if (ready) /a/.test(value); } }")]
    [InlineData("class A { get() { return /a/; } set(value) { value / divisor; } }")]
    [InlineData("const object = {class: class A { static value = /a/; }};")]
    [InlineData("function f(value = class A { field = /a/; }) { return value / divisor; }")]
    public void Class_bodies_preserve_slash_goals_templates_arrow_bodies_and_expression_continuations(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("class C { static static\nfoo(){} }")]
    [InlineData("class C { static static\nget value(){} }")]
    [InlineData("class C { static get\nfoo(){} }")]
    [InlineData("class C { static set\nfoo(value){} }")]
    [InlineData("class C { static static\nset value(value){} }")]
    [InlineData("class C { static static\nstatic foo(){} }")]
    [InlineData("class C { static static\nstatic {} }")]
    [InlineData("class C { static static\n[value](){} }")]
    [InlineData("class C { static get\n[foo](){} }")]
    [InlineData("class C { static static\n#value; }")]
    [InlineData("class C { static static\nstatic #value; }")]
    [InlineData("class C { static get static\n() {} }")]
    [InlineData("class C { static static\n() {} }")]
    [InlineData("class C { static static\n=/a/; }")]
    [InlineData("class C { static static\n[foo]; }")]
    [InlineData("class C { static static\nget; }")]
    [InlineData("class C { static static\nset; }")]
    public void A_contextual_name_already_used_as_the_actual_name_does_not_become_another_modifier(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("class C { get static\nfoo(){} }")]
    [InlineData("class C { get get\nfoo(){} }")]
    [InlineData("class C { set set\nfoo(value){} }")]
    [InlineData("class C { static set\nfoo; }")]
    [InlineData("class C { static get\nfoo=1; }")]
    [InlineData("class C { static get static\nfoo(){} }")]
    public void An_incomplete_accessor_cannot_be_reinterpreted_as_a_field_to_insert_a_semicolon(string input)
        => _fixture.Invalid(input);

    [Theory]
    [InlineData("class C {field=()=>{}\ninstanceof(){} get value(){}}")]
    [InlineData("class C {field=()=>{}\nin(){} get value(){}}")]
    [InlineData("class C {field=()=>{}\ninstanceof=/x/; get value(){}}")]
    [InlineData("class C {field=()=>{}\nin; method(){return /a/;}}")]
    [InlineData("class C {static field=()=>{}\ninstanceof(){} set value(value){}}")]
    [InlineData("class C {static field=()=>{}\nin=/x/; static get value(){}}")]
    [InlineData("class C {field=value=>()=>{}\nin(){} get value(){}}")]
    [InlineData("class C {field=value=>{}\ninstanceof; static { /a/.test(value); }}")]
    public void In_and_instanceof_can_name_new_elements_after_an_arrow_block_ends_a_field_initializer(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("class C {field=value\nin object; get value(){}}")]
    [InlineData("class C {field=value\ninstanceof Type; get value(){}}")]
    [InlineData("class C {field=(()=>{})\nin object; get value(){}}")]
    [InlineData("class C {field=(()=>{})\ninstanceof Type; get value(){}}")]
    [InlineData("class C {field=()=>value\nin object; get value(){}}")]
    [InlineData("class C {field=()=>value\ninstanceof Type; get value(){}}")]
    public void In_and_instanceof_continue_ordinary_field_values_and_arrow_expression_bodies(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("class C {field=()=>{}\nin object;}")]
    [InlineData("class C {field=()=>{}\ninstanceof Type;}")]
    public void A_bare_arrow_block_cannot_be_the_left_operand_of_in_or_instanceof(string input)
        => _fixture.Invalid(input);

    [Fact]
    public void The_same_keyword_is_an_initializer_operator_or_a_new_method_name_according_to_the_preceding_expression()
    {
        foreach (string keyword in new[] { "in", "instanceof" })
        {
            var continued = _fixture.Success("class C { field=value\n" + keyword + " object; get property(){} }");
            var continuedNodes = JavaScriptScriptFixture.Nodes(continued).ToArray();
            var continuedField = Assert.Single(continuedNodes, node => node.SignPost.Name == "ClassField");
            Assert.Contains(continuedField.AllTokens, token => token.Input == keyword);
            Assert.Single(continuedNodes, node => node.SignPost.Name == "ClassMethod");

            var separated = _fixture.Success("class C { field=()=>{}\n" + keyword + "(){} get property(){} }");
            var separatedNodes = JavaScriptScriptFixture.Nodes(separated).ToArray();
            var separatedField = Assert.Single(separatedNodes, node => node.SignPost.Name == "ClassField");
            Assert.DoesNotContain(separatedField.AllTokens, token => token.Input == keyword);
            var methods = separatedNodes.Where(node => node.SignPost.Name == "ClassMethod").ToArray();
            Assert.Equal(2, methods.Length);
            Assert.Contains(methods, method => method.AllTokens[0].Input == keyword);
        }
    }

    [Fact]
    public void Contextual_static_field_names_and_static_accessors_preserve_their_element_boundaries()
    {
        const string input = "class C { static static\nfoo(){} static static\nget value(){} static get\nfoo(){} }";
        var tree = _fixture.Success(input);
        var nodes = JavaScriptScriptFixture.Nodes(tree).ToArray();
        var fields = nodes.Where(node => node.SignPost.Name == "ClassField").ToArray();
        Assert.Equal(2, fields.Length);
        Assert.All(fields, field => Assert.Equal(new[] { "static", "static" },
            field.AllTokens.Select(token => token.Input).ToArray()));
        var methods = nodes.Where(node => node.SignPost.Name == "ClassMethod").ToArray();
        Assert.Equal(3, methods.Length);
        Assert.Contains(methods, method => method.AllTokens.Take(3).Select(token => token.Input)
            .SequenceEqual(new[] { "static", "get", "foo" }));
        Assert.Contains(methods, method => method.AllTokens.Take(2).Select(token => token.Input)
            .SequenceEqual(new[] { "get", "value" }));
    }

    [Fact]
    public void Class_and_private_validation_leave_the_original_concrete_tree_and_tokens_intact()
    {
        const string input = "// header\nclass A extends Base { #value = `hello ${name}`; constructor(value) { super(value); } get value() { return this.#value; } static { /a/.test(value); } }";
        var tree = _fixture.Success(input);
        var tokens = tree.AllTokens.ToArray();
        string originalText = JavaScriptScriptFixture.Text(tree);
        Assert.Empty(JavaScriptExpressionValidation.Validate(tree));
        Assert.Empty(_fixture.Language.Validate(tree));
        _fixture.Invalid("class Invalid { #value; #value; }");
        _fixture.Invalid("class Invalid { method() { return this.#missing; } }");
        _fixture.Success("const Other = class { field = /b/; } / divisor;");
        var again = _fixture.Success(input);
        Assert.Equal(originalText, JavaScriptScriptFixture.Text(again));
        Assert.Equal(tokens, tree.AllTokens.ToArray());
        Assert.NotSame(tokens[0], again.AllTokens[0]);
        Assert.Contains(tokens, token => token.Input == "#value");
        Assert.Contains(tokens, token => token.Input == "/a/");
        Assert.Empty(_fixture.Language.Validate(tree));
    }

    [Theory]
    [InlineData("class A { method() { return this.#missing; } }", "#missing")]
    [InlineData("class A { #first; #first; }", "#first")]
    [InlineData("class A { #\\u0078; #x; }", "#x")]
    [InlineData("class A { method(first, first) {} }", "first")]
    public void Early_error_diagnostics_identify_the_original_private_name_or_parameter(string input, string identifier)
    {
        var parsed = _fixture.Parse(input);
        Assert.True(parsed.Success, input + Environment.NewLine + _fixture.Parser.FiredException);
        var diagnostics = _fixture.Language.Validate(parsed.ToParseTree);
        Assert.Contains(diagnostics, diagnostic =>
            diagnostic.SourceOffset == input.LastIndexOf(identifier, StringComparison.Ordinal) &&
            diagnostic.Length == identifier.Length);
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
    public void Division_and_regular_expression_speculation_do_not_advance_class_or_private_context()
    {
        const string input = "/a/g.test(value); class A { #value = /b/; method() { return this.#value; } }";
        var source = _fixture.Language.CreateTokenSource(input);
        // Equivalent token bundles may carry different branch facts; select a representative.
        var regex = source.Policy.Goals.First(goal =>
            goal.Terminals.Any(terminal => terminal.Caption == "RegularExpressionLiteral") &&
            !goal.Terminals.Any(terminal => terminal.Caption == "TemplateTail"));
        var division = source.Policy.Goals.First(goal =>
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
        Assert.Equal(new[] { "/a/g", "/b/" }, parsed.ToParseTree.AllTokens
            .Where(token => token.Kind.Caption == "RegularExpressionLiteral").Select(token => token.Input).ToArray());
    }

    [Fact]
    public async Task Shared_language_keeps_class_field_and_private_scope_contexts_separate_between_parsers()
    {
        var language = _fixture.Language;
        var parsers = Enumerable.Range(0, 2).Select(_ => new LALRParser(language.Grammar, false)).ToArray();
        var examples = new[]
        {
            "class A { #value = /a/; method() { return this.#value; } } /b/.test(value);",
            "const A = class { value = () => /a/; }; const f = () => class {};",
            "class A { static { const value = `${class B { field = /a/; }}`; } }"
        };
        var firstTokens = new object[parsers.Length, examples.Length];
        await Task.WhenAll(Enumerable.Range(0, parsers.Length).Select(worker => Task.Run(() =>
        {
            for (int index = 0; index < examples.Length; index++)
            {
                string input = examples[(index + worker) % examples.Length];
                var source = language.CreateTokenSource(input);
                var parsed = parsers[worker].Parsing(source);
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

[Collection(JavaScriptExpressionCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptClasses")]
public sealed class JavaScriptClassExpressionTests
{
    private readonly JavaScriptExpressionFixture _fixture;
    public JavaScriptClassExpressionTests(JavaScriptExpressionFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("class {}")]
    [InlineData("class Named {}")]
    [InlineData("class extends Base {}")]
    [InlineData("class A extends factory() {}")]
    [InlineData("class A { method() { return 1; } }")]
    [InlineData("class A { constructor(value) { this.value = value; } }")]
    [InlineData("class A extends Base { constructor() { super(); } }")]
    [InlineData("class A { #value = 1; get value() { return this.#value; } }")]
    [InlineData("class A { static { this.value = /a/; } }")]
    [InlineData("class A { field = `value ${name}`; }")]
    [InlineData("class A { method() { return () => super.value; } }")]
    [InlineData("(class A {}) / divisor")]
    [InlineData("new class A { value = 1; }")]
    [InlineData("class {} === class {}")]
    [InlineData("[class {}, class extends Base {}]")]
    [InlineData("({Constructor: class {}})")]
    [InlineData("() => class A { field = /a/; }")]
    public void Expression_entry_accepts_class_expressions_and_runs_class_body_validation(string input)
        => _fixture.Success(input);

    [Theory]
    [InlineData("class A { #value; #value; }")]
    [InlineData("class A { method() { return this.#missing; } }")]
    [InlineData("class A { method(first, first) {} }")]
    [InlineData("class A { get value(first) {} }")]
    [InlineData("class A { set value() {} }")]
    [InlineData("class A { constructor() {} constructor() {} }")]
    [InlineData("class A { static prototype; }")]
    [InlineData("class A { value = arguments; }")]
    [InlineData("class A { static { return; } }")]
    [InlineData("class A { constructor() { super(); } }")]
    [InlineData("class A extends Base + Other {}")]
    [InlineData("class A {} next()")]
    public void Expression_entry_rejects_class_early_errors_and_following_statement_text(string input)
        => _fixture.Invalid(input);
}
