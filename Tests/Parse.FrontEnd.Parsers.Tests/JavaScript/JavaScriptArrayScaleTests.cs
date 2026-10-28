using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.JavaScript;
using Janglim.FrontEnd.ParseTree;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests.JavaScript;

[Collection(JavaScriptGrammarCollection.Name)]
[Trait("Category", "JavaScript")]
[Trait("Feature", "JavaScriptArrayScale")]
public sealed class JavaScriptArrayScaleTests
{
    private readonly JavaScriptGrammarFixture fixture;
    public JavaScriptArrayScaleTests(JavaScriptGrammarFixture fixture) => this.fixture = fixture;

    [Theory]
    [InlineData("literal")]
    [InlineData("assignment")]
    [InlineData("arrow")]
    public void Four_thousand_elements_reduce_as_a_list_instead_of_accumulating_on_the_LR_stack(string kind)
    {
        const int count = 4000;
        string elements = string.Join(",", Enumerable.Range(0, count).Select(index =>
            kind == "literal" ? "\"item" + index + "\"" : "item" + index));
        string source = kind == "literal" ? "consume([" + elements + "]);" :
            kind == "assignment" ? "([" + elements + "] = source);" :
            "([" + elements + "]) => 0;";
        var tree = ParseWithBoundedStack(source);
        var array = Nodes(tree).OfType<ParseTreeNonTerminal>().Single(node => node.SignPost.Name == "ArrayLiteral");
        Assert.Equal(3, array.Count);
        Assert.Equal("ArrayElements", Assert.IsType<ParseTreeNonTerminal>(array[1]).SignPost.Name);
        Assert.Equal(count, Nodes(array).OfType<ParseTreeNonTerminal>().Count(node => node.SignPost.Name == "ArrayElement"));
        Assert.Equal(count - 1, Nodes(array).OfType<ParseTreeTerminal>().Count(node => node.Token.Input == ","));
    }

    [Fact]
    public void Four_thousand_elisions_preserve_each_comma_without_accumulating_LR_stack_entries()
    {
        const int holes = 4000;
        var tree = ParseWithBoundedStack("consume([" + new string(',', holes) + "]);");
        var array = Nodes(tree).OfType<ParseTreeNonTerminal>().Single(node => node.SignPost.Name == "ArrayLiteral");
        Assert.Equal(holes, Nodes(array).OfType<ParseTreeTerminal>().Count(node => node.Token.Input == ","));
        Assert.DoesNotContain(Nodes(array).OfType<ParseTreeNonTerminal>(), node => node.SignPost.Name == "ArrayElement");
    }

    [Theory]
    [InlineData("declaration")]
    [InlineData("parameter")]
    [InlineData("catch")]
    public void Four_thousand_binding_names_reduce_incrementally_in_every_array_binding_context(string kind)
    {
        const int count = 4000;
        string names = string.Join(",", Enumerable.Range(0, count).Select(index => "item" + index));
        string source = kind == "declaration" ? "let [" + names + "] = source;" :
            kind == "parameter" ? "function read([" + names + "]) {}" :
            "try {} catch ([" + names + "]) {}";
        var tree = ParseWithBoundedStack(source);
        var pattern = Nodes(tree).OfType<ParseTreeNonTerminal>().Single(node => node.SignPost.Name == "ArrayBindingPattern");
        Assert.Equal(3, pattern.Count);
        Assert.Equal("ArrayBindingElements", Assert.IsType<ParseTreeNonTerminal>(pattern[1]).SignPost.Name);
        Assert.Equal(count, Nodes(pattern).OfType<ParseTreeNonTerminal>().Count(node => node.SignPost.Name == "BindingIdentifier"));
        Assert.Equal(count - 1, Nodes(pattern).OfType<ParseTreeTerminal>().Count(node => node.Token.Input == ","));
    }

    [Fact]
    public void A_binding_pattern_with_four_thousand_holes_preserves_all_its_commas()
    {
        const int count = 4000;
        var tree = ParseWithBoundedStack("let [" + new string(',', count) + "] = source;");
        var pattern = Nodes(tree).OfType<ParseTreeNonTerminal>().Single(node => node.SignPost.Name == "ArrayBindingPattern");
        Assert.Equal(count, Nodes(pattern).OfType<ParseTreeTerminal>().Count(node => node.Token.Input == ","));
        Assert.DoesNotContain(Nodes(pattern).OfType<ParseTreeNonTerminal>(), node => node.SignPost.Name == "BindingIdentifier");
    }

    [Theory]
    [InlineData("let [,] = source;")]
    [InlineData("let [,,] = source;")]
    [InlineData("let [first,, last,] = source;")]
    [InlineData("let [,, ...tail] = source;")]
    [InlineData("let [first,, ...tail] = source;")]
    [InlineData("const [...[first, last]] = source;")]
    [InlineData("function read([first,, ...tail]) {}")]
    [InlineData("function read([,, ...tail]) {}")]
    [InlineData("function read([first,, last,]) {}")]
    [InlineData("try {} catch ([first,, ...tail]) {}")]
    [InlineData("try {} catch ([,, ...tail]) {}")]
    public void Binding_lists_preserve_elisions_trailing_commas_and_a_final_rest_element(string source)
        => fixture.Accept(JavaScriptInputKind.Script, source);

    [Theory]
    [InlineData("let [...tail,] = source;")]
    [InlineData("let [...tail,,] = source;")]
    [InlineData("let [...tail, next] = source;")]
    [InlineData("let [first,, ...tail,] = source;")]
    [InlineData("let [first,, ...tail, next] = source;")]
    [InlineData("let [...tail = fallback] = source;")]
    [InlineData("let [...[first, last],] = source;")]
    [InlineData("function read([...tail,]) {}")]
    [InlineData("function read([first,, ...tail, next]) {}")]
    [InlineData("try {} catch ([...tail,]) {}")]
    [InlineData("try {} catch ([first,, ...tail, next]) {}")]
    [InlineData("function read([value,, value]) {}")]
    [InlineData("try {} catch ([value,, value]) {}")]
    public void A_binding_rest_element_still_has_no_following_comma_binding_or_initializer(string source)
        => fixture.Reject(JavaScriptInputKind.Script, source);

    [Theory]
    [InlineData("[,];")]
    [InlineData("[,,];")]
    [InlineData("[, first,, last,];")]
    [InlineData("[...values,];")]
    [InlineData("[, ...values,];")]
    [InlineData("[...first,, last];")]
    [InlineData("[...first, ...last,];")]
    [InlineData("([, first,, object.last,] = values);")]
    [InlineData("([,, ...tail] = values);")]
    [InlineData("([...[first, last]] = values);")]
    [InlineData("([,, first, ...tail]) => first;")]
    [InlineData("([first = 1,, ...tail]) => first;")]
    [InlineData("([...[first, last]]) => first;")]
    public void Array_literals_assignment_patterns_and_arrow_bindings_retain_holes_and_legal_trailing_commas(string source)
        => fixture.Accept(JavaScriptInputKind.Script, source);

    [Theory]
    [InlineData("([first, ...tail,] = values);")]
    [InlineData("([...tail,,] = values);")]
    [InlineData("([first, ...tail, last] = values);")]
    [InlineData("([...[first, last],] = values);")]
    [InlineData("([first, ...tail,]) => first;")]
    [InlineData("([...tail,,]) => tail;")]
    [InlineData("([...[first, last],]) => first;")]
    [InlineData("([first, ...tail, last]) => first;")]
    [InlineData("([value, value]) => value;")]
    public void Flattening_the_new_list_nodes_still_checks_rest_position_trailing_commas_and_bound_names(string source)
        => fixture.Reject(JavaScriptInputKind.Script, source);

    [Theory]
    [InlineData("['ordinary']; 'use strict'; with (object) { value; }")]
    [InlineData("'use strict' + suffix; with (object) { value; }")]
    [InlineData("'prefix'; ('use strict'); with (object) { value; }")]
    public void Directive_detection_stops_when_the_statement_is_not_a_single_string_literal(string source)
        => fixture.Accept(JavaScriptInputKind.Script, source, JavaScriptMode.NonStrict);

    [Fact]
    public void A_string_literal_directive_prologue_still_applies_strict_mode_to_the_entire_scope()
    {
        fixture.Reject(JavaScriptInputKind.Script,
            "'prefix'; 'use strict'; with (object) { value; }", JavaScriptMode.NonStrict);
        fixture.Reject(JavaScriptInputKind.Script,
            "function f(value, value) { 'prefix'; 'use strict'; }", JavaScriptMode.NonStrict);
    }

    private ParseTreeNonTerminal ParseWithBoundedStack(string source)
    {
        var profile = fixture.Profile(JavaScriptInputKind.Script);
        var parsed = profile.Parser.Parsing(profile.Language.CreateTokenSource(source));
        Assert.True(parsed.Success, profile.Parser.FiredException?.ToString());
        Assert.Empty(parsed.AllErrors);
        // This structural bound is deterministic: increasing a flat array's length
        // must not increase LR depth. It avoids machine-dependent timing assertions.
        int maximum = parsed.SelectMany(block => block.Units).Max(unit =>
            Math.Max(unit.BeforeStack.Stack.Count, unit.AfterStack.Stack.Count));
        Assert.InRange(maximum, 1, 256);
        var tree = Assert.IsType<ParseTreeNonTerminal>(parsed.ToParseTree);
        Assert.Empty(profile.Language.Validate(tree));
        return tree;
    }

    private static IEnumerable<ParseTreeSymbol> Nodes(ParseTreeSymbol root)
    {
        var pending = new Stack<ParseTreeSymbol>();
        pending.Push(root);
        while (pending.Count != 0)
        {
            var current = pending.Pop();
            yield return current;
            if (current is ParseTreeNonTerminal node)
                for (int index = node.Count - 1; index >= 0; index--) pending.Push(node[index]);
        }
    }
}
