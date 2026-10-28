using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.TypeScript;

[Collection(TypeScriptTypeCollection.Name)]
[Trait("Category", "TypeScript")]
[Trait("Feature", "TypeScriptAssertions")]
public sealed class TypeScriptAssertionRegressionTests
{
    private readonly TypeScriptTypeFixture fixture = TypeScriptTypeFixture.Instance;

    [Theory]
    [InlineData("(value: unknown) => asserts value")]
    [InlineData("(value: unknown) => asserts value is string")]
    [InlineData("(value: unknown) => asserts this")]
    [InlineData("(value: unknown) => asserts this is Derived")]
    [InlineData("(asserts: unknown) => asserts asserts")]
    [InlineData("(asserts: unknown) => asserts asserts is string")]
    [InlineData("asserts is")]
    [InlineData("asserts is is string")]
    [InlineData("asserts unknown")]
    [InlineData("asserts unknown is string")]
    [InlineData("asserts string is number")]
    [InlineData("asserts keyof")]
    [InlineData("asserts readonly is string")]
    [InlineData("asserts let")]
    [InlineData("asserts static")]
    [InlineData("asserts implements")]
    [InlineData("asserts interface")]
    [InlineData("asserts package")]
    [InlineData("asserts private")]
    [InlineData("asserts protected")]
    [InlineData("asserts public")]
    [InlineData("asserts \\u0076alue is string")]
    public void Assertion_keyword_is_selected_before_ordinary_name_reduction(string source)
    {
        var tree = fixture.Accept(source);

        Assert.Contains(Descendants(tree), node => node.SignPost.Name == "TSAssertsTypePredicate");
    }

    [Theory]
    [InlineData("asserts/**/value")]
    [InlineData("asserts /* comment */ value is string")]
    [InlineData("asserts value\nis string")]
    [InlineData("asserts value\r\nis string")]
    [InlineData("asserts value\u2028is string")]
    [InlineData("asserts value /*\n*/ is string")]
    [InlineData("asserts value // comment\nis string")]
    public void Assertion_trivia_matches_the_native_parser_line_break_rules(string source)
        => fixture.Accept(source);

    [Theory]
    [InlineData("() => asserts is string")]
    [InlineData("(asserts: unknown) => asserts is string")]
    [InlineData("() => asserts /**/ is string")]
    [InlineData("() => asserts is\nstring")]
    [InlineData("{check(): asserts is string;}")]
    [InlineData("(() => asserts is string)[]")]
    [InlineData("new () => asserts is string")]
    [InlineData("new <T>(value: T) => asserts is string")]
    [InlineData("abstract new () => asserts is string")]
    [InlineData("abstract new <T>(value: T) => asserts is string")]
    public void Return_type_predicate_can_name_a_parameter_asserts(string source)
    {
        var tree = fixture.Accept(source);

        Assert.Contains(Descendants(tree), node => node.SignPost.Name == "TSAssertsNameTypePredicate");
        Assert.DoesNotContain(Descendants(tree), node => node.SignPost.Name == "TSAssertsTypePredicate");
    }

    [Theory]
    [InlineData("asserts")]
    [InlineData("asserts<T>")]
    [InlineData("asserts.Foo")]
    [InlineData("asserts[]")]
    [InlineData("asserts | string")]
    [InlineData("Array<asserts>")]
    [InlineData("{asserts: string;}")]
    [InlineData("{asserts(): asserts this;}")]
    [InlineData("() => asserts")]
    [InlineData("() => asserts<T>")]
    public void Ordinary_asserts_names_and_properties_keep_their_original_role(string source)
    {
        var tree = fixture.Accept(source);

        if (!source.Contains("asserts this", StringComparison.Ordinal))
            Assert.DoesNotContain(Descendants(tree), node => node.SignPost.Name == "TSAssertsTypePredicate");
    }

    [Theory]
    [InlineData("asserts\nvalue")]
    [InlineData("asserts\r\nvalue")]
    [InlineData("asserts\u2028value")]
    [InlineData("asserts\u2029value")]
    [InlineData("asserts /*\n*/ value")]
    [InlineData("asserts // comment\nvalue")]
    [InlineData("asserts is string")]
    [InlineData("asserts value is")]
    [InlineData("asserts 123")]
    [InlineData("asserts function")]
    [InlineData("asserts void")]
    [InlineData("() => asserts is")]
    [InlineData("() => asserts is is string")]
    [InlineData("() => asserts\nis string")]
    [InlineData("() => asserts /*\n*/ is string")]
    [InlineData("new () => asserts is")]
    [InlineData("new <T>(value: T) => asserts is")]
    [InlineData("abstract new () => asserts is")]
    [InlineData("abstract new <T>(value: T) => asserts is")]
    public void Invalid_assertion_syntax_is_still_rejected(string source)
        => fixture.Reject(source);

    private static IEnumerable<ParseTreeNonTerminal> Descendants(ParseTreeSymbol root)
    {
        var pending = new Stack<ParseTreeSymbol>();
        pending.Push(root);
        while (pending.Count != 0)
        {
            if (pending.Pop() is not ParseTreeNonTerminal node) continue;
            yield return node;
            foreach (var child in node.Items) pending.Push(child);
        }
    }
}
