using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Janglim.FrontEnd.Grammars.JavaScript;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

[Collection(JavaScriptNonStrictSemanticCollection.Name)]
[Trait("Feature", "JavaScriptNonStrict")]
public sealed class JavaScriptNonStrictTests
{
    private readonly JavaScriptNonStrictSemanticFixture fixture;
    public JavaScriptNonStrictTests(JavaScriptNonStrictSemanticFixture fixture) => this.fixture = fixture;

    public static IEnumerable<object[]> ContextualIdentifiers()
    {
        foreach (string name in new[] { "yield", "let", "static", "implements", "interface", "package", "private", "protected", "public" })
        {
            yield return new object[] { "var " + name + " = 1; " + name + " += 2;" };
            yield return new object[] { "function f(" + name + ") { return " + name + "; }" };
            yield return new object[] { name + ": { break " + name + "; }" };
        }
    }

    [Theory]
    [MemberData(nameof(ContextualIdentifiers))]
    public void Contextual_names_are_identifiers_in_non_strict_code(string input) => fixture.Success(input);

    [Theory]
    [InlineData("var yield = 4; yield / 2;")]
    [InlineData("var let = 4; let / 2;")]
    [InlineData("function* yield() { yield /a/; }")]
    [InlineData("function* g() { yield /a/; function f() { var yield = 4; yield / 2; } }")]
    [InlineData("function* g() { const f = () => { var yield = 4; yield / 2; }; }")]
    [InlineData("({ *g() { yield /a/; }, async f() { var yield = 4; return yield / 2; }, async *h() { yield await /a/; } });")]
    [InlineData("function f() { with (object) /a/.test(value); }")]
    [InlineData("var let; for (let in object) { break; }")]
    [InlineData("for (let of of list) { continue; }")]
    [InlineData("({ [eval = 1]() { 'use strict'; return 1; } });")]
    [InlineData("({ [(() => { with (object) ; })()]() { 'use strict'; } });")]
    [InlineData("'use\\x20strict'; with (object) ;")]
    [InlineData("('use strict'); with (object) ;")]
    [InlineData("{ 'use strict'; } with (object) ;")]
    [InlineData("; 'use strict'; with (object) ;")]
    public void Grammar_contexts_and_directive_boundaries_are_preserved(string input) => fixture.Success(input);

    [Theory]
    [InlineData("function* g(yield) {}")]
    [InlineData("const g = function* yield() {};")]
    [InlineData("function* g() { function yield() {} }")]
    [InlineData("function* g() { const f = (yield) => 1; }")]
    [InlineData("function g() { 'use strict'; function f() { with (object) ; } }")]
    [InlineData("class A extends (eval = Object) {}")]
    [InlineData("class A { [eval = 1]() {} }")]
    [InlineData("({ [eval = 1](eval) { 'use strict'; } });")]
    [InlineData("for (let of list) ;")]
    [InlineData("var let; let[0] = 1;")]
    [InlineData("({ '\\137\\137proto__': null, __proto__: {} });")]
    public void Invalid_context_switches_remain_errors(string input) => fixture.Invalid(input);

    [Theory]
    [InlineData("var yield; yield\n(value);", "CallExpression")]
    [InlineData("var yield; yield\n[property];", "MemberAccess")]
    [InlineData("var yield; yield\n`text`;", "TaggedTemplate")]
    public void A_newline_after_a_yield_identifier_does_not_insert_a_semicolon(string input, string expectedRole)
    {
        var tree = fixture.Success(input);
        var nodes = JavaScriptScriptFixture.Nodes(tree).ToArray();
        Assert.Single(nodes, node => node.SignPost.Name == "ExpressionStatement");
        Assert.Contains(nodes, node => node.SignPost.Name == expectedRole);
    }

    [Theory]
    [InlineData("function* g() { yield\n(value); }")]
    [InlineData("function* g() { yield\n[property]; }")]
    [InlineData("function* g() { yield\n`text`; }")]
    [InlineData("function* g() { const f = () => yield\n(value); }")]
    public void Generator_keywords_and_nested_arrow_identifiers_keep_different_line_break_rules(string input)
        => fixture.Success(input);

    [Fact]
    public void Shared_for_prefixes_and_nested_else_branches_keep_their_complete_follow_paths()
    {
        fixture.Success("var let; for (let = 0; let < 2; let++) { continue; }");
        fixture.WebSuccess("if (first) if (second) x(); else y(); else z();");
        fixture.WebSuccess("if (first) if (second) function f() {} else function g() {} else function h() {}");
    }

    [Fact]
    public void Public_mode_options_validate_values_without_changing_the_default_contract()
    {
        Assert.Equal(JavaScriptMode.NonStrict, fixture.Language.Mode);
        Assert.False(fixture.Language.WebCompatibility);
        Assert.Equal("mode", Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JavaScriptGrammar((JavaScriptMode)17)).ParamName);
        Assert.Equal("mode", Assert.Throws<ArgumentOutOfRangeException>(() =>
            new JavaScriptGrammar((JavaScriptMode)17, inputKind: JavaScriptInputKind.Expression)).ParamName);
        Assert.Equal("mode", Assert.Throws<ArgumentOutOfRangeException>(() =>
            JavaScriptExpressionValidation.Validate(fixture.Success("value;"), (JavaScriptMode)17)).ParamName);
        Assert.Equal("tree", Assert.Throws<ArgumentNullException>(() => fixture.Language.Validate(null)).ParamName);
        Assert.Equal("input", Assert.Throws<ArgumentNullException>(() => fixture.Language.CreateTokenSource(null)).ParamName);
    }

    [Fact]
    public async Task Strictness_is_local_to_each_validation_and_does_not_mutate_the_tree()
    {
        var valid = fixture.Success("var eval = 1; function f(a, a) { return a; }");
        var invalid = fixture.Parse("'use strict'; var eval = 1;");
        var before = fixture.Language.Validate(invalid).Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray();
        Assert.NotEmpty(before);
        var tokens = valid.AllTokens.ToArray();
        var invalidTokens = invalid.AllTokens.ToArray();
        await Task.WhenAll(Enumerable.Range(0, 8).Select(_ => Task.Run(() =>
        {
            for (int round = 0; round < 10; round++)
            {
                Assert.Empty(fixture.Language.Validate(valid));
                Assert.Equal(before, fixture.Language.Validate(invalid).Select(d => (d.Code, d.SourceOffset, d.Length)).ToArray());
            }
        })));
        Assert.Equal(tokens, valid.AllTokens.ToArray());
        Assert.Equal(invalidTokens, invalid.AllTokens.ToArray());
    }
}
