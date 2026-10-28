using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

// ECMAScript 2026 sections 12.10, 14 and 15.2. These are strict Script syntax
// checks: accepting a program does not execute it or resolve its identifiers.
[CollectionDefinition(Name)]
public sealed class JavaScriptScriptCollection : ICollectionFixture<JavaScriptScriptFixture>
{
    public const string Name = "ECMAScript Script grammar";
}

public sealed class JavaScriptScriptFixture
{
    public JavaScriptGrammar Language { get; } = new JavaScriptGrammar();
    public LALRParser Parser { get; }

    public JavaScriptScriptFixture() => Parser = new LALRParser(Language.Grammar, false);

    public ParsingResult Parse(string input) => Parser.Parsing(Language.CreateTokenSource(input));

    public ParseTreeSymbol Success(string input)
    {
        var result = Parse(input);
        Assert.True(result.Success, input + Environment.NewLine + Parser.FiredException);
        Assert.Empty(result.AllErrors);
        Assert.NotNull(result.ToParseTree);
        var diagnostics = Language.Validate(result.ToParseTree);
        Assert.True(diagnostics.Count == 0, input + Environment.NewLine +
            string.Join(Environment.NewLine, diagnostics.Select(d => d.Code + ": " + d.Message)));
        return result.ToParseTree;
    }

    public void Invalid(string input)
    {
        var result = Parse(input);
        if (!result.Success)
        {
            Assert.NotEmpty(result.AllErrors);
            return;
        }
        var diagnostics = Language.Validate(result.ToParseTree);
        Assert.True(diagnostics.Count > 0, "Unexpectedly accepted: " + input);
        Assert.All(diagnostics, d =>
        {
            Assert.False(string.IsNullOrWhiteSpace(d.Code));
            Assert.False(string.IsNullOrWhiteSpace(d.Message));
            Assert.InRange(d.SourceOffset, 0, input.Length);
            Assert.InRange(d.Length, 0, input.Length - d.SourceOffset);
        });
    }

    public static IEnumerable<ParseTreeNonTerminal> Nodes(ParseTreeSymbol tree)
        => JavaScriptExpressionFixture.Nodes(tree);
    public static string Text(ParseTreeSymbol tree) => JavaScriptExpressionFixture.Text(tree);
}

[Collection(JavaScriptScriptCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptStatements")]
public sealed class JavaScriptStatementTests
{
    private readonly JavaScriptScriptFixture _fixture;
    public JavaScriptStatementTests(JavaScriptScriptFixture fixture) => _fixture = fixture;

    [Theory]
    [InlineData("")]
    [InlineData(" \t\r\n")]
    [InlineData("// empty Script")]
    [InlineData("/* empty Script */")]
    [InlineData(";")]
    [InlineData(";;;")]
    [InlineData("{}")]
    [InlineData("{{}; {}}")]
    [InlineData("value;")]
    [InlineData("value = 1; next(value);")]
    [InlineData("({name: value});")]
    [InlineData("{ name: value; }")]
    [InlineData("debugger;")]
    [InlineData("if (condition) value();")]
    [InlineData("if (condition) value(); else other();")]
    [InlineData("if (a) if (b) x(); else y();")]
    [InlineData("if (a) { if (b) x(); } else y();")]
    [InlineData("if (a) ; else ;")]
    [InlineData("while (condition) step();")]
    [InlineData("while (condition) { if (done) break; continue; }")]
    [InlineData("do step(); while (condition);")]
    [InlineData("do { continue; } while (condition);")]
    [InlineData("for (;;) ;")]
    [InlineData("for (i = 0; i < 10; i++) step(i);")]
    [InlineData("for (var i = 0, j = 1; i < j; i++) {}")]
    [InlineData("for (let i = 0; i < 10; ++i) { let value = i; }")]
    [InlineData("for (const limit = 10; limit > 0; ) break;")]
    [InlineData("for (key in object) visit(key);")]
    [InlineData("for (object.key in object) ;")]
    [InlineData("for (var key in object) ;")]
    [InlineData("for (let key in object) ;")]
    [InlineData("for (const key in object) ;")]
    [InlineData("for (value of values) visit(value);")]
    [InlineData("for (object.value of values) ;")]
    [InlineData("for (var value of values) ;")]
    [InlineData("for (let value of values) ;")]
    [InlineData("for (const value of values) ;")]
    [InlineData("for (of of values) ;")]
    [InlineData("for (let of of values) ;")]
    [InlineData("for ((async) of []) ;")]
    [InlineData("for (\\u0061sync of []) ;")]
    [InlineData("for (async in []) ;")]
    [InlineData("var of = 1; of++; object.of; ({of});")]
    [InlineData("for (i = (key in object); i; ) ;")]
    [InlineData("for (i = condition ? key in object : other; i; ) ;")]
    [InlineData("for (i = call(key in object); i; ) ;")]
    [InlineData("for (i = [key in object]; i; ) ;")]
    [InlineData("for (i = {value: key in object}; i; ) ;")]
    [InlineData("for (i = `${key in object}`; i; ) ;")]
    [InlineData("for (i = function() { return key in object; }; i; ) ;")]
    [InlineData("for (i = condition ? first : (key in object); i; ) ;")]
    [InlineData("switch (value) {}")]
    [InlineData("switch (value) { case 0: first(); break; case 1: second(); default: last(); }")]
    [InlineData("switch (value) { default: first(); case 1: next(); }")]
    [InlineData("try {} catch (error) { handle(error); }")]
    [InlineData("try {} catch { recover(); }")]
    [InlineData("try {} finally { cleanup(); }")]
    [InlineData("try {} catch (error) {} finally {}")]
    [InlineData("throw failure;")]
    [InlineData("label: value();")]
    [InlineData("label: { break label; }")]
    [InlineData("outer: inner: while (condition) { continue outer; break inner; }")]
    [InlineData("outer: for (;;) { while (condition) { continue outer; } break; }")]
    [InlineData("label: {} label: {}")]
    [InlineData("label: while (condition) { function f() { label: while (other) { break label; } } break label; }")]
    [InlineData("function f() {}")]
    [InlineData("function f() { return; }")]
    [InlineData("function f() { return value; }")]
    [InlineData("function f(a, b) { var total = a + b; return total; }")]
    [InlineData("function f() { function g() { return 1; } return g(); }")]
    [InlineData("const f = function() { return 1; };")]
    [InlineData("const f = function named() { return named; };")]
    [InlineData("(function() { return 1; })();")]
    [InlineData("(function f() {}).name;")]
    [InlineData("if (condition) { function f() {} }")]
    [InlineData("function f() {} function f() {}")]
    [InlineData("function await(await) { return await; }")]
    [InlineData("function of(of) { return of; }")]
    [InlineData("var async = 1; function async(await) { return await; }")]
    public void Ordinary_functions_and_statement_forms_are_accepted(string input) => _fixture.Success(input);

    [Theory]
    [InlineData("var a = 1\nvar b = 2")]
    [InlineData("let a = 1\nconst b = 2")]
    [InlineData("value")]
    [InlineData("{ value }")]
    [InlineData("function f() { return 1 }")]
    [InlineData("function f() { return\nvalue; }")]
    [InlineData("function f() { return /* newline\n */ value; }")]
    [InlineData("function f() { return\rvalue }")]
    [InlineData("function f() { return\u2028value }")]
    [InlineData("function f() { return\u2029value }")]
    [InlineData("function f() { return /* same line */ value }")]
    [InlineData("a\n(b)")]
    [InlineData("a\n[b]")]
    [InlineData("a\n+ b")]
    [InlineData("a\n- b")]
    [InlineData("a\n`text`")]
    [InlineData("a\n/b/g;")]
    [InlineData("a\n++b;")]
    [InlineData("a\n--b;")]
    [InlineData("a++\nb++")]
    [InlineData("a++\n(b)")]
    [InlineData("a++\n[b]")]
    [InlineData("a++\n`text`")]
    [InlineData("a /* same line */ ++; next();")]
    [InlineData("a /* newline\n */ ++b;")]
    [InlineData("while (condition) { break\nlabel; }")]
    [InlineData("while (condition) { continue\nlabel; }")]
    [InlineData("do {} while (condition) next();")]
    [InlineData("do {} while (condition)\nnext()")]
    [InlineData("debugger\nnext()")]
    [InlineData("throw /* same line */ error\nnext()")]
    [InlineData("if (condition) value()\nelse other()")]
    [InlineData("if (condition) value()\nnext()")]
    [InlineData("try { value() } finally { cleanup() }")]
    [InlineData("var value\n;")]
    [InlineData("let value\n;")]
    [InlineData("const value = 1\n;")]
    [InlineData("debugger\n;")]
    [InlineData("function f() { return\n; }")]
    [InlineData("function f() { return value\n; }")]
    [InlineData("throw error\n;")]
    [InlineData("while (condition) { break\n; }")]
    [InlineData("while (condition) { continue\n; }")]
    public void Semicolons_are_inserted_only_at_statement_boundaries(string input) => _fixture.Success(input);

    [Theory]
    [InlineData("var a = 1 var b = 2")]
    [InlineData("a b")]
    [InlineData("if (condition)")]
    [InlineData("if (condition) else value();")]
    [InlineData("while (condition)")]
    [InlineData("for (i = 0\ni < 10; i++) ;")]
    [InlineData("for (i = 0; i < 10\ni++) ;")]
    [InlineData("for (var i = 0\ni < 10\ni++) ;")]
    [InlineData("for (i = key in object; i; ) ;")]
    [InlineData("for (i = condition ? first : key in object; i; ) ;")]
    [InlineData("for (i = key in object, other; i; ) ;")]
    [InlineData("for (let key = 0 in object) ;")]
    [InlineData("for (const key = 0 in object) ;")]
    [InlineData("for (var key = 0 in object) ;")]
    [InlineData("for (let value = 0 of values) ;")]
    [InlineData("for (var value = 0 of values) ;")]
    [InlineData("for (a, b in object) ;")]
    [InlineData("for (let a, b of values) ;")]
    [InlineData("for (async of []) ;")]
    [InlineData("for (async\nof []) ;")]
    [InlineData("for (async /* comment */ of []) ;")]
    [InlineData("for (const value; ; ) ;")]
    [InlineData("for (let [value]; ; ) ;")]
    [InlineData("for (value of a, b) ;")]
    [InlineData("for (call() of values) ;")]
    [InlineData("for (a?.b in object) ;")]
    [InlineData("for (eval of values) ;")]
    [InlineData("throw\nerror;")]
    [InlineData("throw /* newline\n */ error;")]
    [InlineData("throw\u2028error;")]
    [InlineData("throw\rerror;")]
    [InlineData("throw\u2029error;")]
    [InlineData("throw;")]
    [InlineData("throw")]
    [InlineData("return 1;")]
    [InlineData("{ return; }")]
    [InlineData("break;")]
    [InlineData("continue;")]
    [InlineData("break unknown;")]
    [InlineData("while (condition) break unknown;")]
    [InlineData("label: { continue label; }")]
    [InlineData("label: label: while (condition) ;")]
    [InlineData("outer: while (condition) { function f() { break outer; } }")]
    [InlineData("while (condition) { function f() { continue; } }")]
    [InlineData("switch (value) { default: ; default: ; }")]
    [InlineData("switch (value) { case: ; }")]
    [InlineData("switch (value) { continue; }")]
    [InlineData("try {}")]
    [InlineData("try {} catch () {}")]
    [InlineData("try value(); catch (error) {}")]
    [InlineData("with (object) value();")]
    [InlineData("function () {}")]
    [InlineData("function f(,) {}")]
    [InlineData("function f() return value;")]
    [InlineData("if (condition) function f() {}")]
    [InlineData("while (condition) function f() {}")]
    [InlineData("label: function f() {}")]
    [InlineData("if (condition) let x = 1;")]
    [InlineData("while (condition) const x = 1;")]
    [InlineData("function f() { return a\n/re/.test(value); }")]
    [InlineData("a?.b\n`text`")]
    public void Invalid_syntax_control_contexts_and_ASI_locations_are_rejected(string input) => _fixture.Invalid(input);

    [Theory]
    [InlineData("if (condition) /a/.test(value);")]
    [InlineData("if (condition) /[}]/.test(value); else /b/.test(value);")]
    [InlineData("while (condition) /a/.test(value);")]
    [InlineData("for (;;) /a/.test(value);")]
    [InlineData("for (const x of /a/) /b/.test(x);")]
    [InlineData("function f() {} /a/.test(value);")]
    [InlineData("{ value(); } /a/.test(value);")]
    [InlineData("if (condition) {} /a/.test(value);")]
    [InlineData("try {} finally {} /a/.test(value);")]
    [InlineData("switch (value) {} /a/.test(value);")]
    [InlineData("function f() { return /a/; }")]
    [InlineData("function f() { return\n/a/.test(value); }")]
    [InlineData("throw /a/;")]
    [InlineData("call(value) / divisor;")]
    [InlineData("object.if(value) / divisor;")]
    [InlineData("object.while(value) / divisor;")]
    [InlineData("object?.if(value) / divisor;")]
    [InlineData("object.return / divisor;")]
    [InlineData("object.function / divisor;")]
    [InlineData("object.if / divisor;")]
    [InlineData("object?.return / divisor;")]
    [InlineData("(value) / divisor;")]
    [InlineData("({value: 1}) / divisor;")]
    [InlineData("var value = {} / divisor;")]
    [InlineData("var f = function() {} / divisor;")]
    [InlineData("(function() {}) / divisor;")]
    [InlineData("(function() {})() / divisor;")]
    [InlineData("if (condition) call() / divisor;")]
    [InlineData("function f() { return call() / divisor; }")]
    [InlineData("function f() { return\n{} /a/.test(value); }")]
    [InlineData("const value = flag ? {key: 1} : function() {} / 2;")]
    [InlineData("try {} catch { f(value) / 2; }")]
    [InlineData("({if: f(value) / 2});")]
    [InlineData("({while: f(value) / 2});")]
    [InlineData("({catch: f(value) / 2});")]
    [InlineData("const object = {if: 1}; f(value) / 2;")]
    [InlineData("outer: while (condition) { break outer\n/a/.test(value); }")]
    [InlineData("outer: while (condition) { continue outer\n/a/.test(value); }")]
    [InlineData("while (condition) { break\n/a/.test(value); }")]
    [InlineData("while (condition) { continue\n/a/.test(value); }")]
    [InlineData("const text = `a${function() { return /a/; }()}b`;")]
    [InlineData("const text = `a${(function() {}) / divisor}b`;")]
    [InlineData("{ const text = `${({a: 1}).a}`; } /a/.test(value);")]
    public void Slash_goals_follow_control_parentheses_and_function_or_brace_roles(string input)
        => _fixture.Success(input);

    [Fact]
    public void Newline_before_a_call_does_not_split_a_valid_expression()
    {
        var tree = _fixture.Success("value\n(argument)");
        var nodes = JavaScriptScriptFixture.Nodes(tree).ToArray();
        Assert.Single(nodes, n => n.SignPost.Name == "ExpressionStatement");
        Assert.Contains(nodes, n => n.SignPost.Name == "CallExpression");
    }

    [Theory]
    [InlineData("value\n[property]")]
    [InlineData("value\n`text`")]
    [InlineData("value\n+ other")]
    [InlineData("value\n/other/divisor;")]
    public void A_valid_expression_continuation_remains_a_single_statement(string input)
    {
        var tree = _fixture.Success(input);
        Assert.Single(JavaScriptScriptFixture.Nodes(tree), n => n.SignPost.Name == "ExpressionStatement");
    }

    [Theory]
    [InlineData("value++\n(argument)")]
    [InlineData("value++\n[property]")]
    [InlineData("value++\n`text`")]
    public void An_update_result_cannot_become_the_base_of_a_new_call_member_or_template(string input)
    {
        var tree = _fixture.Success(input);
        Assert.Equal(2, JavaScriptScriptFixture.Nodes(tree).Count(n => n.SignPost.Name == "ExpressionStatement"));
    }

    [Fact]
    public void Return_newline_leaves_the_following_expression_outside_the_return_statement()
    {
        var tree = _fixture.Success("function f() { return\nvalue; }");
        var nodes = JavaScriptScriptFixture.Nodes(tree).ToArray();
        var returned = Assert.Single(nodes, n => n.SignPost.Name == "ReturnStatement");
        Assert.DoesNotContain(returned.AllTokens, t => t.Input == "value");
        Assert.Contains(nodes, n => n.SignPost.Name == "ExpressionStatement" &&
            n.AllTokens.Any(t => t.Input == "value"));
    }

    [Fact]
    public void Postfix_restriction_turns_a_newline_update_into_the_next_statement()
    {
        var tree = _fixture.Success("first\n++second;");
        var statements = JavaScriptScriptFixture.Nodes(tree)
            .Where(n => n.SignPost.Name == "ExpressionStatement").ToArray();
        Assert.Equal(2, statements.Length);
        Assert.DoesNotContain(statements[0].AllTokens, t => t.Input == "++");
        Assert.Contains(statements[1].AllTokens, t => t.Input == "++");
    }

    [Fact]
    public void Else_belongs_to_the_nearest_unmatched_if()
    {
        var tree = _fixture.Success("if (a) if (b) x(); else y();");
        var conditionals = JavaScriptScriptFixture.Nodes(tree)
            .Where(n => n.SignPost.Name == "IfStatement").ToArray();
        Assert.Equal(2, conditionals.Length);
        Assert.Contains(conditionals, n => n.AllTokens.First().Input == "if" &&
            n.AllTokens.Any(t => t.Input == "b") &&
            !n.AllTokens.Any(t => t.Input == "a") && n.AllTokens.Any(t => t.Input == "else"));
    }

    [Fact]
    public void Parsing_and_static_validation_preserve_input_and_per_parse_state()
    {
        const string input = "// header\nfunction f(a) { return `${a}` }\nconst result = f(1)";
        var first = _fixture.Success(input);
        var tokens = first.AllTokens.ToArray();
        string text = JavaScriptScriptFixture.Text(first);
        Assert.Empty(_fixture.Language.Validate(first));
        _fixture.Invalid("function f(a, a) {}");
        var other = _fixture.Success("if (ready) /a/.test(value);");
        var again = _fixture.Success(input);
        Assert.NotSame(first, again);
        Assert.Equal(text, JavaScriptScriptFixture.Text(again));
        Assert.Equal(tokens, first.AllTokens.ToArray());
        Assert.NotSame(tokens[0], again.AllTokens[0]);
        Assert.NotSame(first, other);
        Assert.Empty(_fixture.Language.Validate(first));
        Assert.Same(_fixture.Language.Policy, _fixture.Language.CreateTokenSource(input).Policy);
    }

    [Fact]
    public void Public_factory_contract_keeps_strict_Script_distinct_from_expression_input()
    {
        Assert.Equal(2026, JavaScriptGrammar.Edition);
        Assert.False(string.IsNullOrWhiteSpace(_fixture.Language.GrammarText));
        Assert.Equal("input", Assert.Throws<ArgumentNullException>(
            () => _fixture.Language.CreateTokenSource(null)).ParamName);
        Assert.Equal("tree", Assert.Throws<ArgumentNullException>(
            () => _fixture.Language.Validate(null)).ParamName);
        Assert.NotSame(_fixture.Language.CreateTokenSource("value;").Start.SourceIdentity,
            _fixture.Language.CreateTokenSource("value;").Start.SourceIdentity);
    }
}
