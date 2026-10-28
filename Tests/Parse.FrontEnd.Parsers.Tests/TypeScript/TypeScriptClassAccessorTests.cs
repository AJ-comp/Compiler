using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

// Native TypeScript 7.0.2 accepts setter trailing commas, including signatures.
// These checks also cover declarations erased from the JavaScript validation view.
[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptClassAccessors")]
public sealed class TypeScriptClassAccessorTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("class C { set value(next: number,) {} }")]
    [InlineData("abstract class C { abstract set value(next: number,); }")]
    [InlineData("declare class C { set value(next: number,); }")]
    public void Setter_trailing_commas_are_valid_in_implementations_and_signatures(string source)
        => fixture.Accept(source);

    [Fact]
    public void A_default_inside_a_setter_binding_pattern_is_not_a_parameter_initializer()
        => fixture.Accept("class C { set value({item = 1}: {item?: number}) {} }");

    [Theory]
    [InlineData("abstract class C { abstract get value(next: number): number; }")]
    [InlineData("abstract class C { abstract set value(); }")]
    [InlineData("abstract class C { abstract set value(first: number, second: number); }")]
    [InlineData("abstract class C { abstract set value(...values: number[]); }")]
    [InlineData("declare class C { get value(next: number): number; }")]
    [InlineData("declare class C { set value(); }")]
    [InlineData("declare class C { set value(first: number, second: number); }")]
    [InlineData("declare class C { set value(...values: number[]); }")]
    public void Bodyless_accessor_signatures_keep_getter_and_setter_arity_restrictions(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("class C { constructor(this: C) {} }")]
    [InlineData("class C { get value(this: C): number { return 1; } }")]
    [InlineData("class C { set value(this: C, next: number) {} }")]
    [InlineData("abstract class C { abstract get value(this: C): number; }")]
    [InlineData("declare class C { set value(this: C, next: number); }")]
    public void Constructors_and_accessors_cannot_declare_a_this_pseudo_parameter(string source)
        => fixture.Reject(source);

    [Theory]
    [InlineData("abstract class C { abstract set value(next?: number); }")]
    [InlineData("declare class C { set value(next?: number); }")]
    public void Setter_signature_parameters_cannot_be_optional(string source)
        => fixture.Reject(source);

    [Fact]
    public void A_binding_pattern_default_is_still_forbidden_in_an_accessor_signature()
        => fixture.Reject("abstract class C { abstract set value({item = 1}: {item?: number}); }");
}
