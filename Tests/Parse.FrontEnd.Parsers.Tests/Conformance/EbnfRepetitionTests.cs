using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Repetition must advance through input. Validate the original EBNF/model before lowering
/// creates a recursive nullable grammar that could loop while calculating FIRST/FOLLOW.
/// </summary>
[Trait("Category", "Ebnf")]
public class EbnfRepetitionTests
{
    [Theory]
    [InlineData("S : ( 'x'? )* 'y' ;", "*")]
    [InlineData("S : ( 'x'? )+ 'y' ;", "+")]
    [InlineData("S : ( 'x'* )* 'y' ;", "*")]
    [InlineData("S : ( 'x'* )+ 'y' ;", "+")]
    [InlineData("S : ( 'x' | 'z'? )* 'y' ;", "*")]
    [InlineData("S : ( 'x' | ( 'z'? ) )+ 'y' ;", "+")]
    [InlineData("S : ( ( 'x'? )? )* 'y' ;", "*")]
    [InlineData("S : ( id? )+ 'y' ; id := \"[a-z]+\" ;", "+")]
    [InlineData("S : empty* 'y' ; empty : 'x'? ;", "*")]
    [InlineData("S : empty+ 'y' ; empty : next ; next : 'x'? ;", "+")]
    [InlineData("S : empty* 'y' ; empty : next ; next : empty | 'x'? ;", "*")]
    [InlineData("S : 'y' ; Unused : ( 'x'? )* ;", "*")]
    [InlineData("S : empty* 'y' ; empty : 'x' ; empty : 'z'? ;", "*")]
    public void Nullable_repetition_is_a_grammar_error_even_without_predicates(string text, string marker)
    {
        AssertRepetitionError(EbnfGrammarReader.Read(text), marker);
    }

    [Theory]
    [InlineData("S : ( {Allow()}? )* 'y' ;", "*")]
    [InlineData("S : ( {Allow()}? )+ 'y' ;", "+")]
    [InlineData("S : ( ( {Allow()}? )? )* 'y' ;", "*")]
    [InlineData("S : ( 'x'? {Allow()}? )+ 'y' ;", "+")]
    [InlineData("S : ( 'x' {Allow()}? | {Allow()}? )* 'y' ;", "*")]
    [InlineData("S : empty* 'y' ; empty : next ; next : {Allow()}? ;", "*")]
    [InlineData("S : empty+ 'y' ; empty : next ; next : empty | {Allow()}? ;", "+")]
    [InlineData("S : 'y' ; Unused : ( {Allow()}? )+ ;", "+")]
    public void Predicate_repetition_validation_never_calls_the_registered_function(string text, string marker)
    {
        int calls = 0;
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", context => { calls++; return false; });

        AssertRepetitionError(EbnfGrammarReader.Read(text, options), marker);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("*")]
    [InlineData("+")]
    public void Diagnostic_points_to_the_repetition_marker_even_when_the_group_spans_lines(string marker)
    {
        string text = "\nS : (\n 'x'?\n)\n" + marker + " 'y' ;";
        var read = EbnfGrammarReader.Read(text);
        AssertRepetitionError(read, marker);
        Assert.StartsWith("line 5:", Assert.Single(read.Errors));
    }

    [Theory]
    [InlineData("S : 'x'* 'y' ;")]
    [InlineData("S : 'x'+ 'y' ;")]
    [InlineData("S : ( 'x'? 'z' )* 'y' ;")]
    [InlineData("S : ( 'x'? 'z' )+ 'y' ;")]
    [InlineData("S : ( 'x' | 'z' )* 'y' ;")]
    [InlineData("S : ( 'x'+ )* 'y' ;")]
    [InlineData("S : item* 'y' ; item : 'x' | 'z' ;")]
    [InlineData("S : item* 'y' ; item : item 'x' | 'x' ;")]
    [InlineData("S : item* 'y' ; item : next | 'x' ; next : item 'z' ;")]
    [InlineData("S : id+ 'y' ; id := \"[a-z]+\" ;")]
    public void Every_iteration_that_must_consume_a_token_remains_allowed(string text)
    {
        var read = EbnfGrammarReader.Read(text);
        Assert.True(read.Success, string.Join("; ", read.Errors));
    }

    [Theory]
    [InlineData("S : ( {Allow()}? 'x' )* 'y' ;")]
    [InlineData("S : ( 'x' {Allow()}? )+ 'y' ;")]
    [InlineData("S : ( 'x'? {Allow()}? 'z' )* 'y' ;")]
    [InlineData("S : item* 'y' ; item : item {Allow()}? 'x' | 'x' ;")]
    [InlineData("S : ( {Allow()}? )? 'y' ;")]
    public void Token_consuming_predicate_repetition_and_optional_condition_groups_remain_allowed(string text)
    {
        var read = EbnfGrammarReader.Read(text, Allow());
        Assert.True(read.Success, string.Join("; ", read.Errors));
    }

    [Theory]
    [InlineData("y")]
    [InlineData("x y")]
    [InlineData("x x y")]
    public void Valid_predicate_star_parses_zero_one_and_multiple_iterations(string input)
    {
        var read = EbnfGrammarReader.Read("S : ( 'x' {Allow()}? )* 'y' ;", Allow());
        Assert.True(read.Success, string.Join("; ", read.Errors));
        var lexer = new Lexer();
        foreach (var terminal in read.Grammar.TerminalSet) lexer.AddTokenRule(terminal);

        var result = new LALRParser(read.Grammar, true).Parsing(lexer.Lexing(input).TokensForParsing);
        Assert.True(result.Success);
        Assert.Equal(input.Split(' '), result.ToParseTree.AllTokens.Select(token => token.Input));
    }

    [Fact]
    public void Direct_csharp_model_repetition_has_the_same_validation_and_source_location()
    {
        var body = EbnfSym.GroupOf(new List<List<EbnfSym>>
        {
            new List<EbnfSym> { EbnfSym.PredicateCall("Allow") }
        });
        body.Quantifier = EbnfQuantifier.OneOrMore;
        body.SourceLine = 12;

        var error = Assert.Throws<EbnfException>(() => new EbnfGrammar(Model(body), Allow()));
        Assert.StartsWith("line 12:", error.Message);
        Assert.Contains("body of '+' repetition can consume no input", error.Message);
    }

    [Fact]
    public void Direct_model_named_forward_references_are_included_in_the_fixed_point()
    {
        var repeated = EbnfSym.Ref("A");
        repeated.Quantifier = EbnfQuantifier.ZeroOrMore;
        var model = Model(repeated);
        AddRule(model, "A", new[] { EbnfSym.Ref("B") });
        AddRule(model, "B", new[] { EbnfSym.Ref("C") });
        var optional = EbnfSym.Literal("x");
        optional.Quantifier = EbnfQuantifier.Optional;
        AddRule(model, "C", new[] { optional });

        var error = Assert.Throws<EbnfException>(() => new EbnfGrammar(model));
        Assert.Contains("body of '*' repetition can consume no input", error.Message);
    }

    [Fact]
    public void A_long_nullable_reference_chain_is_checked_before_lowering_or_first_follow()
    {
        var repeated = EbnfSym.Ref("N0");
        repeated.Quantifier = EbnfQuantifier.OneOrMore;
        var model = Model(repeated);
        for (int i = 0; i < 40; i++) AddRule(model, "N" + i, new[] { EbnfSym.Ref("N" + (i + 1)) });
        AddRule(model, "N40", new[] { EbnfSym.PredicateCall("Allow") });

        var error = Assert.Throws<EbnfException>(() => new EbnfGrammar(model, Allow()));
        Assert.Contains("body of '+' repetition can consume no input", error.Message);
    }

    [Fact]
    public void Shared_inline_group_objects_are_not_mistaken_for_a_cycle()
    {
        var shared = EbnfSym.GroupOf(new List<List<EbnfSym>> { new List<EbnfSym> { EbnfSym.Literal("x") } });
        var outer = EbnfSym.GroupOf(new List<List<EbnfSym>> { new List<EbnfSym> { shared, shared } });
        outer.Quantifier = EbnfQuantifier.OneOrMore;

        var grammar = new EbnfGrammar(Model(outer));
        Assert.NotNull(grammar.StartSymbol);
    }

    [Fact]
    public void Cyclic_inline_group_objects_report_a_model_error_without_recursing_forever()
    {
        var group = EbnfSym.GroupOf(new List<List<EbnfSym>> { new List<EbnfSym>() });
        group.Group[0].Add(group);
        group.Quantifier = EbnfQuantifier.ZeroOrMore;

        var error = Assert.Throws<EbnfException>(() => new EbnfGrammar(Model(group)));
        Assert.Contains("inline group model contains a cycle", error.Message);
    }

    [Theory]
    [InlineData("S : A 'y' ; A : {Allow()}? A | {Allow()}? ;")]
    [InlineData("S : A 'y' ; A : B ; B : A | {Allow()}? ;")]
    [InlineData("S : 'y' ; Unused : {Allow()}? Unused | {Allow()}? ;")]
    public void Shared_core_predicate_cycle_validation_is_returned_as_a_grammar_error(string text)
    {
        var read = EbnfGrammarReader.Read(text, Allow());
        Assert.False(read.Success);
        var error = Assert.Single(read.Errors);
        Assert.Contains("non-consuming predicate cycle", error);
        Assert.DoesNotContain("internal error:", error);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void An_optional_condition_as_the_complete_start_rule_keeps_a_valid_absent_path(bool allow)
    {
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", context => allow);
        var read = EbnfGrammarReader.Read("S : ( {Allow()}? )? ;", options);
        Assert.True(read.Success, string.Join("; ", read.Errors));
        Assert.All(read.Grammar.StartSymbol, production => Assert.True(production.Count > 0));

        var lexer = new Lexer();
        foreach (var terminal in read.Grammar.TerminalSet) lexer.AddTokenRule(terminal);
        var result = new LALRParser(read.Grammar, true).Parsing(lexer.Lexing("").TokensForParsing);

        Assert.True(result.Success);
        Assert.Empty(result.ToParseTree.AllTokens);
    }

    private static void AssertRepetitionError(EbnfReadResult result, string marker)
    {
        Assert.False(result.Success);
        var error = Assert.Single(result.Errors);
        Assert.Contains($"body of '{marker}' repetition can consume no input", error);
        Assert.Contains("at least one token", error);
        Assert.DoesNotContain("internal error:", error);
    }

    private static EbnfReaderOptions Allow()
        => new EbnfReaderOptions().RegisterPredicate("Allow", context => true);

    private static EbnfModel Model(EbnfSym symbol)
    {
        var model = new EbnfModel();
        AddRule(model, "S", new[] { symbol, EbnfSym.Literal("y") });
        return model;
    }

    private static void AddRule(EbnfModel model, string name, params EbnfSym[][] alternatives)
    {
        var rule = new EbnfRuleDef { Name = name };
        foreach (var alt in alternatives) rule.Alternatives.Add(new List<EbnfSym>(alt));
        model.Rules.Add(rule);
    }
}
