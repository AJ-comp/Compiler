using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptRuntimeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptAssertionsInLoops")]
public sealed class TypeScriptAssertionLoopTests
{
    private readonly TypeScriptRuntimeFixture fixture = TypeScriptRuntimeFixture.Shared.Value;

    [Theory]
    [InlineData("for (value as T in object) {}")]
    [InlineData("for (value satisfies T in object) {}")]
    [InlineData("for (value as T of values) {}")]
    [InlineData("for (value satisfies T of values) {}")]
    [InlineData("for (object.value as T of values) {}")]
    [InlineData("for (value as T as U in object) {}")]
    [InlineData("for ((value as T) in object) {}")]
    [InlineData("async function run() { for await (value as T of values) {} }")]
    public void Loop_target_assertions_reuse_the_runtime_assignment_target(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("for ((1 as T) in object) {}")]
    [InlineData("for ((left + right) as T of values) {}")]
    [InlineData("for (call() as T in object) {}")]
    [InlineData("for (base?.value as T of values) {}")]
    [InlineData("for (value as in object) {}")]
    [InlineData("for (value satisfies of values) {}")]
    public void Assertions_do_not_turn_invalid_loop_targets_into_assignable_values(string source)
        => fixture.Reject(source);
}
