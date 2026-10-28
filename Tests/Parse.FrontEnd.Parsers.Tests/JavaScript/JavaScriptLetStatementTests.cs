using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMA-262 2026 12.10, 14.3 and 14.5: a lexical declaration is available in
// StatementList, but not as an unbraced if/loop/label/with Statement body.
[Collection(JavaScriptNonStrictSemanticCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptNonStrict")]
public sealed class JavaScriptLetStatementTests
{
    private readonly JavaScriptNonStrictSemanticFixture fixture;

    public JavaScriptLetStatementTests(JavaScriptNonStrictSemanticFixture fixture)
        => this.fixture = fixture;

    private static IEnumerable<string> Bodies(string body)
    {
        yield return "if (false) " + body;
        yield return "while (false) " + body;
        yield return "for (; false;) " + body;
        yield return "for (var x in null) " + body;
        yield return "for (var x of []) " + body;
        yield return "label: " + body;
        yield return "with ({}) " + body;
        yield return "async function* f() { for await (var x of []) " + body + " }";
    }

    public static IEnumerable<object[]> NewlineBodies()
    {
        foreach (string trivia in new[] { "\n", "\r\n", " /*\n*/ " })
            foreach (string suffix in new[] { "x = 1;", "{}" })
                foreach (string input in Bodies("let" + trivia + suffix))
                    yield return new object[] { input };
    }

    [Theory]
    [MemberData(nameof(NewlineBodies))]
    public void A_single_statement_body_can_end_a_let_identifier_before_a_newline(string input)
    {
        var tree = fixture.Success(input);
        var nodes = JavaScriptScriptFixture.Nodes(tree).ToArray();
        var statement = Assert.Single(nodes, node => IsRule(node, "ExpressionStatement") &&
            node.AllTokens.Count == 1 && node.AllTokens[0].Input == "let");
        Assert.Equal("let", statement.AllTokens[0].Input);
        Assert.DoesNotContain(nodes, node => IsRule(node, "LexicalDeclaration"));
        // This is standard non-strict syntax, independently of the Annex B option.
        fixture.WebSuccess(input);
        fixture.WebInvalid(input, strict: true);
        fixture.WebInvalid("'use strict'; " + input);
    }

    [Theory]
    [InlineData("let\nname;")]
    [InlineData("let\nname = 1;")]
    [InlineData("let\n{value} = source;")]
    [InlineData("let\n[value] = source;")]
    [InlineData("{ let\nname; }")]
    [InlineData("function f() { let\nname; }")]
    [InlineData("if (false) { let\nname; }")]
    [InlineData("switch (value) { case 0: let\nname; }")]
    public void Statement_lists_keep_lexical_declaration_precedence_across_newlines(string input)
    {
        var nodes = JavaScriptScriptFixture.Nodes(fixture.Success(input)).ToArray();
        Assert.Single(nodes, node => IsRule(node, "LexicalDeclaration"));
        Assert.DoesNotContain(nodes, node => IsRule(node, "ExpressionStatement") &&
            node.AllTokens.Count == 1 && node.AllTokens[0].Input == "let");
        fixture.WebSuccess(input);
    }

    public static IEnumerable<object[]> ForbiddenBodies()
    {
        foreach (string body in new[] { "let x = 1;", "let {} = source;", "let [x] = source;", "let\n[x] = source;" })
            foreach (string input in Bodies(body))
                yield return new object[] { input };
    }

    [Theory]
    [MemberData(nameof(ForbiddenBodies))]
    public void Lexical_declarations_and_the_let_bracket_lookahead_are_not_single_statements(string input)
    {
        fixture.Invalid(input);
        fixture.WebInvalid(input);
    }

    [Theory]
    [InlineData("let\n{}")]
    [InlineData("let\n{value};")]
    [InlineData("let\nname: statement;")]
    public void A_statement_list_cannot_fall_back_to_a_let_expression_after_a_bad_declaration(string input)
    {
        fixture.Invalid(input);
        fixture.WebInvalid(input);
    }

    private static bool IsRule(ParseTreeNonTerminal node, string rule)
    {
        // Async/generator contexts use distinct generated rule names. Inspect the
        // production family as well as ordinary Script's unsuffixed rule name.
        string name = node.SignPost.Name;
        while (name.EndsWith("__Await", StringComparison.Ordinal) || name.EndsWith("__Yield", StringComparison.Ordinal))
            name = name.Substring(0, name.LastIndexOf("__", StringComparison.Ordinal));
        return name == rule;
    }
}
