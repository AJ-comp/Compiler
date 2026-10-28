using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// A predicate-bearing path back to the same rule must consume a token before recursive entry.
/// Plain LR left recursion and predicates after the recursive reference remain supported.
/// </summary>
[Trait("Category", "Ebnf")]
public class PredicateLeftRecursionTests
{
    [Theory]
    [InlineData("A : {Allow()}? A 'x' | 'x' ;")]
    [InlineData("A : 'x' | {Allow()}? A 'x' ;")]
    [InlineData("A : 'q'? {Allow()}? A 'x' | 'x' ;")]
    [InlineData("A : {Allow()}? 'q'? A 'x' | 'x' ;")]
    [InlineData("A : 'q'* {Allow()}? A 'x' | 'x' ;")]
    [InlineData("A : ({Allow()}?)? A 'x' | 'x' ;")]
    [InlineData("A : ({Allow()}? A 'x') | 'x' ;")]
    [InlineData("A : B 'x' | 'x' ; B : {Allow()}? A ;")]
    [InlineData("A : B 'x' | 'x' ; B : C ; C : {Allow()}? A ;")]
    [InlineData("A : {Allow()}? B 'x' | 'x' ; B : A ;")]
    [InlineData("A : B A 'x' | 'x' ; B : {Allow()}? | 'z' ;")]
    [InlineData("A : B A 'x' | 'x' ; B : C ; C : ({Allow()}?)? ;")]
    [InlineData("A : A 'q' | {Allow()}? A 'x' | 'x' ;")]
    [InlineData("A : B 'x' | {Allow()}? B 'x' | 'x' ; B : A ;")]
    [InlineData("A : {Allow()}? B 'x' | B 'x' | 'x' ; B : A ;")]
    [InlineData("A : {Allow()}? {Allow()}? A 'x' | 'x' ;")]
    [InlineData("S : 'y' ; A : {Allow()}? A 'x' | 'x' ;")]
    public void A_predicate_on_a_zero_token_left_recursive_path_is_a_reader_error(string text)
    {
        var calls = 0;
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => { calls++; return true; });

        var result = EbnfGrammarReader.Read(text, options);

        Assert.False(result.Success);
        Assert.Null(result.Grammar);
        var error = Assert.Single(result.Errors);
        Assert.Contains("left recursion", error);
        Assert.Contains("Allow", error);
        Assert.Contains("A", error);
        Assert.DoesNotContain("internal error:", error);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("A : A 'x' | 'x' ;")]
    [InlineData("A : A {Allow()}? 'x' | 'x' ;")]
    [InlineData("A : 'x' {Allow()}? A | 'x' ;")]
    [InlineData("A : A 'x' | {Allow()}? 'x' ;")]
    [InlineData("A : A {Allow()}? 'x' | {Allow()}? ;")]
    [InlineData("S : {Allow()}? A 'y' ; A : A 'x' | 'x' ;")]
    [InlineData("A : B 'x' | 'x' ; B : A {Allow()}? ;")]
    [InlineData("A : B {Allow()}? A 'x' | 'x' ; B : 'z' ;")]
    [InlineData("A : B A 'x' | 'x' ; B : {Allow()}? 'z' | 'q'? ;")]
    [InlineData("A : B A 'x' | 'x' ; B : C ; C : {Allow()}? 'z' | 'q'? ;")]
    [InlineData("A : ({Allow()}? 'z')? A 'x' | 'x' ;")]
    [InlineData("A : 'q'+ {Allow()}? A 'x' | 'x' ;")]
    [InlineData("S : ('x' {Allow()}?)* 'y' ;")]
    [InlineData("S : ('x' {Allow()}?)+ 'y' ;")]
    public void The_guard_must_lie_on_the_actual_zero_token_recursive_path(string text)
    {
        var calls = 0;
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => { calls++; return true; });

        var result = EbnfGrammarReader.Read(text, options);

        Assert.True(result.Success, string.Join("; ", result.Errors));
        result.Grammar.Optimization();
        Assert.Empty(result.Errors);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Native_validation_rejects_before_parser_construction_and_never_calls_the_condition(bool useParser)
    {
        var calls = 0;
        var grammar = DirectGuardedGrammar(_ => { calls++; throw new InvalidOperationException("must not run during validation"); });

        var error = Assert.Throws<PredicateLeftRecursionException>(() =>
        {
            if (useParser) _ = new LALRParser(grammar);
            else grammar.Optimization();
        });

        Assert.Equal("A", error.RuleName);
        Assert.Equal("Allow", error.PredicateName);
        Assert.Equal(new[] { "A", "A" }, error.RulePath);
        Assert.Contains("left recursion", error.Message);
        Assert.Equal("grammar", error.ParamName);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_condition_that_always_returns_false_still_has_the_same_structural_error(bool allow)
    {
        var calls = 0;
        var grammar = DirectGuardedGrammar(_ => { calls++; return allow; });

        Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());

        Assert.Equal(0, calls);
    }

    [Fact]
    public void An_indirect_native_cycle_reports_the_condition_and_closed_rule_path()
    {
        var grammar = new TestGrammar(g =>
        {
            g.Guard.Add(Condition("Allow", _ => true));
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.B, g.X);
            g.A.Add(g.X);
            g.B.Add(g.C);
            g.C.Add(g.Guard, g.A);
        });

        var error = Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());

        Assert.Equal("Allow", error.PredicateName);
        Assert.Equal(error.RulePath[0], error.RulePath[error.RulePath.Count - 1]);
        Assert.Contains("A", error.RulePath);
        Assert.Contains("B", error.RulePath);
        Assert.Contains("C", error.RulePath);
        Assert.DoesNotContain("Guard", error.RulePath);
    }

    [Fact]
    public void An_unused_named_rule_is_checked_before_first_follow_can_visit_it()
    {
        var grammar = new TestGrammar(g =>
        {
            g.Guard.Add(Condition("Allow", _ => true));
            g.Start.Add(g.Y);
            g.A.Add(g.Guard, g.A, g.X);
            g.A.Add(g.X);
        });

        Assert.Throws<PredicateLeftRecursionException>(() => new LALRParser(grammar));
    }

    [Fact]
    public void A_second_optimization_checks_new_productions_even_after_normalization()
    {
        var grammar = new TestGrammar(g =>
        {
            g.Guard.Add(Condition("Allow", _ => true));
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.X);
        });
        grammar.Optimization();
        grammar.A.Add(grammar.Guard, grammar.A, grammar.X);

        var first = Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());
        var second = Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());

        Assert.Equal(first.RuleName, second.RuleName);
        Assert.Equal(first.PredicateName, second.PredicateName);
        Assert.Equal(first.RulePath, second.RulePath);
    }

    [Fact]
    public void Validation_does_not_remove_or_rewrite_the_rejected_native_production()
    {
        var grammar = DirectGuardedGrammar(_ => true);
        var before = grammar.A.Select(production => production.ToArray()).ToArray();

        Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());

        var after = grammar.A.ToArray();
        Assert.Equal(before.Length, after.Length);
        for (int i = 0; i < before.Length; i++)
        {
            Assert.Equal(before[i].Length, after[i].Count);
            // NonTerminal is enumerable and can contain itself. Compare the actual
            // symbol references instead of asking xUnit to expand that recursive tree.
            for (int j = 0; j < before[i].Length; j++) Assert.Same(before[i][j], after[i][j]);
        }
    }

    [Fact]
    public void A_nullable_prefix_only_carries_conditions_from_its_nullable_alternatives()
    {
        var grammar = new TestGrammar(g =>
        {
            g.Guard.Add(Condition("Allow", _ => true));
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.B, g.A, g.X);
            g.A.Add(g.X);
            g.B.Add(g.Guard, g.Z); // This alternative runs Allow but must consume z.
            g.B.Add(new Epsilon()); // Only this alternative can be passed without input.
        });

        grammar.Optimization();
    }

    [Fact]
    public void A_guard_in_a_nullable_prefix_alternative_is_detected()
    {
        var grammar = new TestGrammar(g =>
        {
            g.Guard.Add(Condition("Allow", _ => true));
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.B, g.A, g.X);
            g.A.Add(g.X);
            g.B.Add(g.Guard);
            g.B.Add(g.Z);
        });

        Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());
    }

    [Theory]
    [InlineData("x y")]
    [InlineData("x x x y")]
    public void A_predicate_after_the_recursive_reference_keeps_working_in_the_real_parser(string input)
    {
        var calls = 0;
        var grammar = new TestGrammar(g =>
        {
            g.Guard.Add(Condition("Allow", _ => { calls++; return true; }));
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.A, g.Guard, g.X);
            g.A.Add(g.X);
        });
        var parser = new LALRParser(grammar, true);

        var result = parser.Parsing(Lex(grammar, input).TokensForParsing);

        Assert.True(result.Success, parser.FiredException?.Message);
        Assert.Equal(input.Split(' '), result.ToParseTree.AllTokens.Select(token => token.Input));
        Assert.Equal(input.Split(' ').Length - 2, calls);
    }

    [Theory]
    [InlineData("x y")]
    [InlineData("x x x y")]
    public void A_predicate_outside_a_plain_left_recursive_cycle_keeps_working(string input)
    {
        var calls = 0;
        var grammar = new TestGrammar(g =>
        {
            g.Guard.Add(Condition("Allow", _ => { calls++; return true; }));
            g.Start.Add(g.Guard, g.A, g.Y);
            g.A.Add(g.A, g.X);
            g.A.Add(g.X);
        });
        var parser = new LALRParser(grammar, true);

        var result = parser.Parsing(Lex(grammar, input).TokensForParsing);

        Assert.True(result.Success, parser.FiredException?.Message);
        Assert.Equal(1, calls);
    }

    [Fact]
    public void A_long_indirect_cycle_is_detected_without_executing_conditions()
    {
        var text = new StringBuilder("A : N0 'x' | 'x' ;\n");
        for (int i = 0; i < 60; i++) text.Append($"N{i} : N{i + 1} ;\n");
        text.Append("N60 : {Allow()}? A ;");

        var read = EbnfGrammarReader.Read(text.ToString(), Options());

        Assert.False(read.Success);
        var error = Assert.Single(read.Errors);
        Assert.Contains("left recursion", error);
        Assert.Contains("Allow", error);
        Assert.Contains("N60", error);
    }

    [Fact]
    public void A_csharp_ebnf_model_uses_the_same_validation_as_text_and_native_grammar()
    {
        var model = new EbnfModel();
        var rule = new EbnfRuleDef { Name = "A" };
        rule.Alternatives.Add(new List<EbnfSym> { EbnfSym.PredicateCall("Allow"), EbnfSym.Ref("A"), EbnfSym.Literal("x") });
        rule.Alternatives.Add(new List<EbnfSym> { EbnfSym.Literal("x") });
        model.Rules.Add(rule);

        var error = Assert.Throws<EbnfException>(() => new EbnfGrammar(model, Options()));

        Assert.Contains("left recursion", error.Message);
        Assert.Contains("Allow", error.Message);
    }

    [Fact]
    public void A_failed_grammar_does_not_leave_validation_state_in_the_next_grammar()
    {
        var invalid = EbnfGrammarReader.Read("A : {Allow()}? A 'x' | 'x' ;", Options());
        Assert.False(invalid.Success);

        var valid = EbnfGrammarReader.Read("A : A {Allow()}? 'x' | 'x' ;", Options());

        Assert.True(valid.Success, string.Join("; ", valid.Errors));
        valid.Grammar.Optimization();
    }

    [Theory]
    [InlineData("A : ({Allow()}? A 'x') | 'x' ;", "A")]
    [InlineData("A : (({Allow()}? A 'x')) | 'x' ;", "A")]
    [InlineData("A : ((({Allow()}? A 'x'))) | 'x' ;", "A")]
    [InlineData("__grp1 : ({Allow()}? __grp1 'x') | 'x' ;", "__grp1")]
    [InlineData("S : 'y' ; __grp1 : (({Allow()}? __grp1 'x')) | 'x' ;", "__grp1")]
    public void Group_lowering_keeps_the_real_user_rule_name_in_the_diagnostic(string text, string ruleName)
    {
        var result = EbnfGrammarReader.Read(text, Options());

        Assert.False(result.Success);
        var error = Assert.Single(result.Errors);
        // A lowered group is not a rule the user wrote. Conversely, a real user
        // rule named __grp1 must remain recognizable even though its name has that prefix.
        Assert.StartsWith($"Rule '{ruleName}'", error);
        Assert.Contains("Allow", error);
        Assert.DoesNotContain("internal error:", error);
    }

    [Fact]
    public void The_exception_keeps_a_defensive_copy_of_the_callers_rule_path()
    {
        var supplied = new List<string> { "A", "B", "A" };
        var error = new PredicateLeftRecursionException("A", "Allow", supplied);
        var message = error.Message;

        supplied[0] = "Changed";
        supplied.Clear();

        Assert.Equal(new[] { "A", "B", "A" }, error.RulePath);
        Assert.Equal(message, error.Message);
        Assert.Contains("A -> B -> A", error.Message);
    }

    [Fact]
    public void The_exception_path_is_read_only()
    {
        var grammar = DirectGuardedGrammar(_ => true);
        var error = Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());
        var original = error.RulePath.ToArray();

        if (error.RulePath is IList<string> path)
            Assert.Throws<NotSupportedException>(() => path[0] = "Changed");

        Assert.Equal(original, error.RulePath);
    }

    [Fact]
    public void A_condition_added_to_an_existing_production_is_checked_after_normalization()
    {
        var calls = 0;
        var grammar = new TestGrammar(g =>
        {
            g.Guard.Add(new Epsilon());
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.Guard, g.A, g.X);
            g.A.Add(g.X);
        });
        grammar.Optimization();

        // Changing an existing production must not bypass validation's early return
        // for a previously normalized grammar.
        grammar.Guard.ElementAt(0).Predicate = new GrammarPredicate("Late", _ =>
        {
            calls++;
            throw new InvalidOperationException("validation must not execute a condition");
        });

        var error = Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());

        Assert.Equal("A", error.RuleName);
        Assert.Equal("Late", error.PredicateName);
        Assert.Equal(new[] { "A", "A" }, error.RulePath);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("x y", 0)]
    [InlineData("x x x y", 2)]
    public void A_rejected_grammar_can_be_repaired_and_parsed_without_replacing_the_instance(
        string input, int expectedCalls)
    {
        var calls = 0;
        var grammar = DirectGuardedGrammar(_ => { calls++; return true; });
        Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());
        Assert.Equal(0, calls);

        // Move the condition behind the recursive reference on the same grammar.
        // A failed validation must not poison later normalization or parser construction.
        grammar.A.Clear();
        grammar.A.Add(grammar.A, grammar.Guard, grammar.X);
        grammar.A.Add(grammar.X);
        grammar.Optimization();
        var parser = new LALRParser(grammar, true);
        Assert.Equal(0, calls);

        var result = parser.Parsing(Lex(grammar, input).TokensForParsing);

        Assert.True(result.Success, parser.FiredException?.Message);
        Assert.Equal(input.Split(' '), result.ToParseTree.AllTokens.Select(token => token.Input));
        Assert.Equal(expectedCalls, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Reader_validation_does_not_evaluate_a_false_or_throwing_condition(bool throwOnCall)
    {
        var calls = 0;
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ =>
        {
            calls++;
            if (throwOnCall) throw new InvalidOperationException("must not run while reading");
            return false;
        });

        var result = EbnfGrammarReader.Read("A : {Allow()}? A 'x' | 'x' ;", options);

        Assert.False(result.Success);
        Assert.Null(result.Grammar);
        var error = Assert.Single(result.Errors);
        Assert.Contains("left recursion", error);
        Assert.Contains("Allow", error);
        Assert.DoesNotContain("internal error:", error);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void The_diagnostic_names_only_the_condition_on_the_actual_empty_path(bool nonEmptyFirst)
    {
        var calls = 0;
        var grammar = new TestGrammar(g =>
        {
            g.Guard.Add(Condition("AA_NotEmpty", _ => { calls++; return true; }));
            g.C.Add(Condition("ZZ_Empty", _ => { calls++; return true; }));
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.B, g.A, g.X);
            g.A.Add(g.X);

            // AA_NotEmpty sorts before ZZ_Empty, but its branch requires z.
            // The diagnostic must not borrow that name from a different alternative.
            if (nonEmptyFirst) g.B.Add(g.Guard, g.Z);
            g.B.Add(g.C);
            if (!nonEmptyFirst) g.B.Add(g.Guard, g.Z);
        });

        var error = Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());

        Assert.Equal("A", error.RuleName);
        Assert.Equal("ZZ_Empty", error.PredicateName);
        Assert.Equal(new[] { "A", "A" }, error.RulePath);
        Assert.DoesNotContain("AA_NotEmpty", error.Message);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData("A : (({Allow()}? 'q')+)? A 'x' | 'x' ;", true)]
    [InlineData("A : (('q' | {Allow()}?) 'r'?)? A 'x' | 'x' ;", false)]
    public void Nested_groups_keep_the_condition_and_token_consumption_on_the_same_path(
        string text, bool expectedSuccess)
    {
        var calls = 0;
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => { calls++; return true; });

        var result = EbnfGrammarReader.Read(text, options);

        Assert.Equal(expectedSuccess, result.Success);
        if (expectedSuccess)
        {
            Assert.NotNull(result.Grammar);
            Assert.Empty(result.Errors);
            result.Grammar.Optimization();
        }
        else
        {
            Assert.Null(result.Grammar);
            var error = Assert.Single(result.Errors);
            Assert.Contains("left recursion", error);
            Assert.Contains("Allow", error);
            Assert.DoesNotContain("internal error:", error);
        }
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_later_recursive_reference_is_checked_when_an_earlier_one_can_be_empty(bool nullableBase)
    {
        var calls = 0;
        var options = new EbnfReaderOptions()
            .RegisterPredicate("Allow", _ => { calls++; return true; })
            .RegisterPredicate("Base", _ => { calls++; return true; });
        var text = "A : A {Allow()}? A 'x' | " + (nullableBase ? "{Base()}?" : "'z'") + " ;";

        var result = EbnfGrammarReader.Read(text, options);

        // With a nullable base, the first A can finish without input and expose
        // the condition before the second A. A mandatory z prevents that route.
        Assert.Equal(!nullableBase, result.Success);
        if (nullableBase)
        {
            Assert.Null(result.Grammar);
            var error = Assert.Single(result.Errors);
            Assert.Contains("left recursion", error);
            Assert.Contains("Allow", error);
        }
        else
        {
            Assert.NotNull(result.Grammar);
            Assert.Empty(result.Errors);
        }
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Optional_absorption_keeps_the_same_guarded_reentry_rule(
        bool absorbOptionals, bool guardInsideConsumingPrefix)
    {
        var calls = 0;
        var grammar = new TestGrammar(g =>
        {
            g.Guard.Add(Condition("Allow", _ => { calls++; return true; }));
            g.Start.Add(g.A, g.Y);
            if (guardInsideConsumingPrefix)
            {
                // Skipping B has no condition; using B must consume z.
                g.B.Add(g.Guard, g.Z);
                g.A.Add(g.B.Optional(), g.A, g.X);
            }
            else
            {
                // Skipping B exposes Allow immediately before reentering A.
                g.B.Add(g.Z);
                g.A.Add(g.B.Optional(), g.Guard, g.A, g.X);
            }
            g.A.Add(g.X);
        }, absorbOptionals);

        if (guardInsideConsumingPrefix)
        {
            grammar.Optimization();
            grammar.Optimization();
        }
        else
        {
            var error = Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());
            Assert.Equal("Allow", error.PredicateName);
        }
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void The_invalid_rule_is_reported_when_an_unrelated_valid_rule_is_reordered(
        bool reverseDefinitions, bool baseAlternativeFirst)
    {
        var calls = 0;
        var options = new EbnfReaderOptions()
            .RegisterPredicate("Alpha", _ => { calls++; return true; })
            .RegisterPredicate("Beta", _ => { calls++; return true; });
        var a = baseAlternativeFirst
            ? "A : 'a' | {Alpha()}? A 'x' ;"
            : "A : {Alpha()}? A 'x' | 'a' ;";
        // B also contains a condition and recursion, but its recursive branch
        // must consume b first. Only A has a guarded path with no input progress.
        var b = baseAlternativeFirst
            ? "B : 'b' | 'b' {Beta()}? B ;"
            : "B : 'b' {Beta()}? B | 'b' ;";
        var text = "S : A 'y' | B 'y' ;\n" +
            (reverseDefinitions ? b + "\n" + a : a + "\n" + b);

        var result = EbnfGrammarReader.Read(text, options);

        Assert.False(result.Success);
        Assert.Null(result.Grammar);
        var error = Assert.Single(result.Errors);
        Assert.StartsWith("Rule 'A'", error);
        Assert.Contains("predicate 'Alpha'", error);
        // EBNF sequences may introduce intermediate rules in the reported path.
        Assert.Contains("(A ->", error);
        Assert.Contains("-> A)", error);
        Assert.DoesNotContain("Beta", error);
        Assert.Equal(0, calls);
    }

    private static EbnfReaderOptions Options()
        => new EbnfReaderOptions().RegisterPredicate("Allow", _ => true);

    private static NonTerminalConcat Condition(string name, Func<PredicateContext, bool> callback)
        => new NonTerminalConcat(new Epsilon()) { Predicate = new GrammarPredicate(name, callback) };

    private static TestGrammar DirectGuardedGrammar(Func<PredicateContext, bool> callback)
        => new TestGrammar(g =>
        {
            g.Guard.Add(Condition("Allow", callback));
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.Guard, g.A, g.X);
            g.A.Add(g.X);
        });

    private static LexingData Lex(Grammar grammar, string input)
    {
        var lexer = new Lexer();
        foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
        return lexer.Lexing(input);
    }

    private sealed class TestGrammar : Grammar
    {
        internal readonly NonTerminal Start = new NonTerminal("S", true);
        internal readonly NonTerminal A = new NonTerminal("A");
        internal readonly NonTerminal B = new NonTerminal("B");
        internal readonly NonTerminal C = new NonTerminal("C");
        internal readonly NonTerminal Guard = new NonTerminal("Guard");
        internal readonly Terminal X = new Terminal(TokenType.Keyword, "x", "x", true, false);
        internal readonly Terminal Y = new Terminal(TokenType.Keyword, "y", "y", true, false);
        internal readonly Terminal Z = new Terminal(TokenType.Keyword, "z", "z", true, false);
        private readonly bool absorbOptionals;
        public override bool AbsorbOptionals => absorbOptionals;
        public override NonTerminal EbnfRoot => Start;
        public TestGrammar(Action<TestGrammar> configure, bool absorbOptionals = true)
        {
            this.absorbOptionals = absorbOptionals;
            configure(this);

            // Unconfigured helper fields that no production references are not
            // part of the configured test grammar. Keep referenced empty rules.
            var referenced = NonTerminalMultiples
                .SelectMany(rule => rule)
                .SelectMany(production => production)
                .OfType<NonTerminal>()
                .ToHashSet();
            NonTerminalMultiples.RemoveWhere(rule =>
                rule.Count == 0 && !rule.IsStartSymbol && !referenced.Contains(rule));
        }
    }
}
