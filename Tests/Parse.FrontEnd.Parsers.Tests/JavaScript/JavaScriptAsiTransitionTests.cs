using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMA-262 2026 12.10, 14.3, 15.2, 15.5 and 15.6: when ASI ends a
// statement, the next declaration body must not retain expression-slash context.
// These tests parse and statically validate source without executing JavaScript.
[Collection(JavaScriptNonStrictSemanticCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptAsiTransition")]
public sealed class JavaScriptAsiTransitionTests
{
    private readonly JavaScriptNonStrictSemanticFixture fixture;

    public JavaScriptAsiTransitionTests(JavaScriptNonStrictSemanticFixture fixture)
        => this.fixture = fixture;

    private static readonly string[] DeclarationHeaders =
    {
        "class Next {}", "function next() {}", "function* next() {}",
        "async function next() {}", "async function* next() {}"
    };

    private static readonly string[] LineTerminators =
    {
        "\n", "\r\n", " /*\n*/ ", "\u2028", "\u2029"
    };

    // The placeholder is a StatementList position, including positions after
    // restricted return/yield and after an unbraced control-statement body.
    private static readonly string[] PrefixContexts =
    {
        "value$next", "object.value$next", "value()$next", "(value)$next",
        "value++$next", "value--$next", "async$next", "async(value)$next",
        "let$next", "yield$next", "var let$next", "var yield$next",
        "typeof let$next", "typeof yield$next", "value + let$next", "value + yield$next",
        "let = value$next", "let++$next", "yield++$next",
        "let value = 1$next", "var value = 1$next",
        "value = class {}$next", "value = function() {}$next",
        "function outer() { return$next }", "function outer() { return value$next }",
        "function* outer() { yield$next }", "function* outer() { yield value$next }",
        "while (false) { break$next }", "while (false) { continue$next }",
        "function outer() { if (false) return$next }",
        "while (false) { if (false) break$next }", "while (false) { if (false) continue$next }",
        "if (false) let$next", "while (false) let$next", "label: let$next",
        "for (; false;) let$next"
    };

    public static IEnumerable<object[]> DeclarationTransitions()
    {
        foreach (string context in PrefixContexts)
            foreach (string declaration in DeclarationHeaders)
            {
                // In a StatementList, `let async` has declaration precedence.
                // It cannot become an identifier statement plus async function.
                if (context == "let$next" && declaration.StartsWith("async ")) continue;
                yield return new object[] { context.Replace("$next", "\n" + declaration + "\n/ab/.test(text);") };
            }
    }

    [Theory]
    [MemberData(nameof(DeclarationTransitions))]
    public void Asi_before_a_declaration_restores_regexp_after_its_body(string input)
        => SuccessWithRegExp(input);

    public static IEnumerable<object[]> TriviaTransitions()
    {
        foreach (string trivia in LineTerminators)
            foreach (string declaration in new[] { "class Next {}", "function next() {}", "async function next() {}" })
                foreach (string context in new[] { "let$next", "var let$next", "function* outer() { yield$next }" })
                {
                    if (context == "let$next" && declaration.StartsWith("async ")) continue;
                    yield return new object[] { context.Replace("$next", trivia + declaration + trivia + "/ab/.test(text);") };
                }
    }

    [Theory]
    [MemberData(nameof(TriviaTransitions))]
    public void Every_line_terminator_and_a_multiline_comment_can_separate_the_statements(string input)
        => SuccessWithRegExp(input);

    [Theory]
    [InlineData("value\nasync function next() {}\n/ab/.test(text);")]
    [InlineData("async(value)\nasync function* next() {}\n/ab/.test(text);")]
    [InlineData("var value = 1\u2028async function next() {}\n/ab/.test(text);")]
    [InlineData("function outer() { return\nasync function next() {}\n/ab/.test(text); }")]
    [InlineData("function outer() { return value\nasync function* next() {}\n/ab/.test(text); }")]
    [InlineData("function* outer() { yield\u2029class Next {}\n/ab/.test(text); }")]
    [InlineData("function* outer() { yield\nasync function next() {}\n/ab/.test(text); }")]
    [InlineData("while (false) { break\nasync function next() {}\n/ab/.test(text); }")]
    public void Ordinary_and_restricted_statement_transitions_also_work_in_strict_code(string input)
    {
        fixture.WebSuccess(input, strict: true);
        fixture.WebSuccess("'use strict'; " + input);
    }

    [Theory]
    [InlineData("if (false) let\n{}\n/ab/.test(text);")]
    [InlineData("while (false) let\n{}\n/ab/.test(text);")]
    [InlineData("label: let\n{}\n/ab/.test(text);")]
    [InlineData("for (; false;) let\n{}\n/ab/.test(text);")]
    [InlineData("if (false) let\n{}function next() {}\n/ab/.test(text);")]
    public void A_single_statement_let_identifier_can_end_before_a_block(string input)
    {
        SuccessWithRegExp(input);
        fixture.WebInvalid(input, strict: true);
    }

    [Theory]
    [InlineData("let\n{[key / divisor]: value = function(){}} = source;")]
    [InlineData("let\n{[key / divisor]: value = class {}} = source;")]
    [InlineData("value\nlet\n{[key / divisor]: item = function(){}} = source;")]
    public void A_let_declaration_keeps_its_binding_pattern_and_computed_name_division(string input)
    {
        var tree = fixture.Success(input);
        Assert.Contains(tree.AllTokens, token => token.Input == "/");
        Assert.DoesNotContain(tree.AllTokens, token => token.Kind.Caption == "RegularExpressionLiteral");
        fixture.WebSuccess(input);
    }

    [Theory]
    [InlineData("value = function() {} / divisor;")]
    [InlineData("value = class {} / divisor;")]
    [InlineData("value = async function() {} / divisor;")]
    [InlineData("value = async function*() {} / divisor;")]
    [InlineData("var value = function() {}\n/ divisor / denominator;")]
    [InlineData("let value = class {}\n/ divisor / denominator;")]
    [InlineData("function outer() { return function() {} / divisor; }")]
    [InlineData("function outer() { return async function() {} / divisor; }")]
    [InlineData("function* outer() { yield function() {} / divisor; }")]
    [InlineData("function* outer() { yield async function() {} / divisor; }")]
    [InlineData("let\n(value) / divisor;")]
    [InlineData("yield\n(value) / divisor;")]
    [InlineData("yield\n[value] / divisor;")]
    [InlineData("yield\n`text` / divisor;")]
    public void A_continuing_expression_keeps_division_and_does_not_become_a_statement(string input)
    {
        var tree = fixture.Success(input);
        Assert.Contains(tree.AllTokens, token => token.Input == "/");
        Assert.DoesNotContain(tree.AllTokens, token => token.Kind.Caption == "RegularExpressionLiteral");
        fixture.WebSuccess(input);
    }

    [Theory]
    [InlineData("let\nasync function next() {}\n/ab/.test(text);")]
    [InlineData("let\nasync function* next() {}\n/ab/.test(text);")]
    [InlineData("let\n{}\n/ab/.test(text);")]
    [InlineData("value async function next() {}\n/ab/.test(text);")]
    [InlineData("var let async function next() {}\n/ab/.test(text);")]
    public void Declaration_precedence_and_missing_line_terminators_still_reject_invalid_source(string input)
    {
        fixture.Invalid(input);
        fixture.WebInvalid(input);
    }

    public static IEnumerable<object[]> AwaitOperandTransitions()
    {
        foreach (string trivia in LineTerminators)
            foreach (string operand in new[] { "class {}", "function() {}", "async function() {}", "function*() {}" })
                yield return new object[] { "async function outer() { await" + trivia + operand + " / divisor; }" };
    }

    // ECMA-262 2026 15.8: AwaitExpression always requires UnaryExpression;
    // a line terminator after its keyword does not end that expression.
    [Theory]
    [MemberData(nameof(AwaitOperandTransitions))]
    [InlineData("async function outer() { return await\nclass {} / divisor; }")]
    [InlineData("async function* outer() { yield await\nfunction() {} / divisor; }")]
    [InlineData("const outer = async () => await\nasync function() {} / divisor;")]
    [InlineData("({ async outer() { await\nclass {} / divisor; } });")]
    [InlineData("class Outer { async method() { await\nfunction() {} / divisor; } }")]
    [InlineData("async function outer() { await await\nclass {} / divisor; }")]
    [InlineData("async function outer() { await\nclass Named {} / divisor / denominator; }")]
    [InlineData("async function outer() { return `value ${await\nclass {} / divisor}`; }")]
    public void An_await_keyword_keeps_its_function_or_class_operand_in_expression_context(string input)
    {
        var tree = fixture.Success(input);
        Assert.Contains(tree.AllTokens, token => token.Input == "/");
        Assert.DoesNotContain(tree.AllTokens, token => token.Kind.Caption == "RegularExpressionLiteral");
        fixture.WebSuccess(input);
        fixture.WebSuccess(input, strict: true);
    }

    [Theory]
    [InlineData("await\nclass Next {}\n/ab/.test(text);")]
    [InlineData("await\nfunction next() {}\n/ab/.test(text);")]
    [InlineData("await\nasync function next() {}\n/ab/.test(text);")]
    [InlineData("var await\nclass Next {}\n/ab/.test(text);")]
    [InlineData("var await\nfunction next() {}\n/ab/.test(text);")]
    [InlineData("var await\nasync function next() {}\n/ab/.test(text);")]
    [InlineData("function outer() { return await\nclass Next {}\n/ab/.test(text); }")]
    [InlineData("function* outer() { yield await\nclass Next {}\n/ab/.test(text); }")]
    [InlineData("await: while (false) { break await\nclass Next {}\n/ab/.test(text); }")]
    [InlineData("await: while (false) { continue await\nasync function next() {}\n/ab/.test(text); }")]
    [InlineData("\\u0061wait\nclass Next {}\n/ab/.test(text);")]
    [InlineData("var \\u0061wait\nfunction next() {}\n/ab/.test(text);")]
    public void An_await_identifier_or_label_can_end_before_a_declaration(string input)
    {
        SuccessWithRegExp(input);
        fixture.WebSuccess(input, strict: true);
    }

    [Theory]
    [InlineData("var await = function() {} / divisor;")]
    [InlineData("let await = class {} / divisor;")]
    [InlineData("function outer(await = function() {} / divisor) {}")]
    [InlineData("const outer = (await = function() {} / divisor) => await;")]
    [InlineData("var {await = function() {} / divisor} = source;")]
    [InlineData("var [await = class {} / divisor] = source;")]
    [InlineData("try {} catch (await) { await / divisor; }")]
    [InlineData("class Outer { await = class {} / divisor; }")]
    [InlineData("({await: function() {} / divisor});")]
    [InlineData("({await} = source); await / divisor;")]
    [InlineData("for (var await of source) { await / divisor; }")]
    public void Await_bindings_property_names_and_initializers_keep_their_original_roles(string input)
    {
        var tree = fixture.Success(input);
        Assert.Contains(tree.AllTokens, token => token.Input == "/");
        Assert.DoesNotContain(tree.AllTokens, token => token.Kind.Caption == "RegularExpressionLiteral");
        fixture.WebSuccess(input, strict: true);
    }

    [Theory]
    [InlineData("async function outer() { await\n; }")]
    [InlineData("async function outer() { await\n}")]
    [InlineData("async function outer() { \\u0061wait\nclass {} / divisor; }")]
    [InlineData("async function outer(await = class {} / divisor) {}")]
    [InlineData("async function outer() { var await = function() {} / divisor; }")]
    [InlineData("async function outer() { await: while (false) { break await; } }")]
    public void A_missing_operand_or_reserved_await_identifier_remains_invalid(string input)
    {
        fixture.Invalid(input);
        fixture.WebInvalid(input);
        fixture.WebInvalid(input, strict: true);
    }

    private void SuccessWithRegExp(string input)
    {
        var tree = fixture.Success(input);
        Assert.Single(tree.AllTokens, token => token.Kind.Caption == "RegularExpressionLiteral");
        fixture.WebSuccess(input);
    }
}

[Collection(JavaScriptModuleCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptAsiTransition")]
public sealed class JavaScriptModuleAwaitTransitionTests
{
    private readonly JavaScriptModuleFixture fixture;
    public JavaScriptModuleAwaitTransitionTests(JavaScriptModuleFixture fixture) => this.fixture = fixture;

    public static IEnumerable<object[]> AwaitOperands()
    {
        foreach (string trivia in new[] { "\n", "\r\n", " /*\n*/ ", "\u2028", "\u2029" })
            foreach (string operand in new[] { "class {}", "function() {}", "async function() {}" })
                yield return new object[] { "await" + trivia + operand + " / divisor;" };
    }

    [Theory]
    [MemberData(nameof(AwaitOperands))]
    [InlineData("async function outer() { return await\nclass {} / divisor; }")]
    [InlineData("async function* outer() { yield await\nfunction() {} / divisor; }")]
    [InlineData("const value = `value ${await\nclass {} / divisor}`;")]
    public void Module_await_keeps_its_operand_across_every_line_terminator(string input)
    {
        var tree = fixture.Success(input);
        Assert.Contains(tree.AllTokens, token => token.Input == "/");
        Assert.DoesNotContain(tree.AllTokens, token => token.Kind.Caption == "RegularExpressionLiteral");
    }

    [Theory]
    [InlineData("await\n;")]
    [InlineData("var await = class {} / divisor;")]
    [InlineData("function outer(await = function() {} / divisor) {}")]
    [InlineData("\\u0061wait\nclass {} / divisor;")]
    public void Module_reserved_await_and_missing_operands_remain_invalid(string input)
        => fixture.Invalid(input);
}
