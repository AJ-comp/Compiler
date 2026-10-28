using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptAssertionComposition")]
public sealed class TypeScriptAssertionCompositionTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("value! as number;")]
    [InlineData("value! satisfies number;")]
    [InlineData("(value as { property: number })!.property;")]
    [InlineData("(value satisfies { property: number })!.property;")]
    [InlineData("(value as any)!();")]
    [InlineData("(value as any)!`text`;")]
    [InlineData("new (value as any)!();")]
    [InlineData("(base?.property! as any)!.next = other;")]
    [InlineData("const result = [value! as number, other! satisfies number];")]
    [InlineData("function unwrap(value: number | null): number { return value! as number; }")]
    [InlineData("!(value as any)!;")]
    [InlineData("const result = (value as any)!.property! satisfies number;")]
    public void Assertions_compose_with_each_other_and_with_runtime_continuations(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("value as number!;")]
    [InlineData("value satisfies number!;")]
    [InlineData("const result = value! as ;")]
    [InlineData("const result = value! satisfies ;")]
    [InlineData("((value as any) + other)! = next;")]
    [InlineData("(base?.property as any)! = next;")]
    public void Assertion_composition_still_requires_valid_types_and_assignment_targets(string source)
        => fixture.Reject(source);
}
