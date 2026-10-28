using Janglim.FrontEnd.Grammars.JavaScript;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

[Collection(JavaScriptGrammarCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptYieldTemplate")]
public sealed class JavaScriptYieldTemplateTests
{
    private readonly JavaScriptGrammarFixture fixture;
    public JavaScriptYieldTemplateTests(JavaScriptGrammarFixture fixture) => this.fixture = fixture;

    [Theory]
    [InlineData("function* g(){ return `1${yield}3${4}5`; }")]
    [InlineData("function* g(){ return `${yield}`; }")]
    [InlineData("function* g(){ return tag`${yield}a${yield}b`; }")]
    [InlineData("function* g(){ return `a${yield\n}b${yield\n}c`; }")]
    [InlineData("function* g(){ return `a${yield /* gap */}b${yield 2}c`; }")]
    [InlineData("async function* g(){ return `a${yield}b${await work()}c`; }")]
    [InlineData("function* g(){ return `outer${`inner${yield}a${yield}b`}tail`; }")]
    public void Bare_yield_ends_before_middle_or_tail_without_consuming_the_template(string source)
    {
        // 15.5 YieldExpression and 12.9.6 Template: the next lexical token can be
        // TemplateMiddle/TemplateTail, whose text includes the interpolation '}'.
        fixture.Accept(JavaScriptInputKind.Script, source);
        fixture.Accept(JavaScriptInputKind.Script, source, JavaScriptMode.NonStrict, web: true);
        fixture.Accept(JavaScriptInputKind.Module, source);
    }

    [Theory]
    [InlineData("function* g(){ return `a${yield*}b${4}c`; }")]
    [InlineData("function* g(){ return `a${yield +}b${4}c`; }")]
    [InlineData("function* g(){ return `a${yield*}b`; }")]
    public void Yield_operators_still_require_an_operand_before_the_template_continuation(string source)
    {
        fixture.Reject(JavaScriptInputKind.Script, source);
        fixture.Reject(JavaScriptInputKind.Script, source, JavaScriptMode.NonStrict, web: true);
        fixture.Reject(JavaScriptInputKind.Module, source);
    }

    [Fact]
    public void Non_strict_yield_identifier_remains_a_template_expression_outside_a_generator()
        => fixture.Accept(JavaScriptInputKind.Script,
            "var yield = 1; const text = `a${yield}b${yield}c`;",
            JavaScriptMode.NonStrict, web: true);
}
