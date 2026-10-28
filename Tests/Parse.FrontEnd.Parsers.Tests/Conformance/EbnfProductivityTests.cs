using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// EBNF construction runs the shared productivity check before a parser is built.
/// Expected grammar failures belong in Errors, rather than the internal-error path.
/// </summary>
[Trait("Category", "Ebnf")]
[Trait("Feature", "GrammarProductivity")]
public sealed class EbnfProductivityTests
{
    [Theory]
    [InlineData("S : U ; U : U ;", "S,U")]
    [InlineData("S : A U | 'z' ; A : B 'x' ; B : 'a' ; U : U ;", "U")]
    [InlineData("S : A U | C 'x' ; A : B 'x' ; B : 'a' ; C : 'a' ; U : U ;", "U")]
    [InlineData("S : A U | C 'x' ; A : B 'x' ; B : 'a' ; C : 'a' ; U : U 'u' ;", "U")]
    [InlineData("S : 'z' | U ; U : 'u' U ;", "U")]
    [InlineData("S : 'z' | U ; U : V ; V : U ;", "U,V")]
    [InlineData("S : 'z' | U ; U : 'u' V ; V : U 'v' ;", "U,V")]
    [InlineData("S : 'z' | A ; A : B ; B : U ; U : U ;", "A,B,U")]
    [InlineData("S : 'z' ; U : U ;", "U")]
    [InlineData("S : U? 'z' ; U : U ;", "U")]
    [InlineData("S : G A U | 'z' ; G : 'g'? ; A : B 'x' ; B : 'a' ; U : U ;", "U")]
    public void Nonproductive_rules_are_reported_as_ordinary_reader_errors(
        string source, string expectedRules)
    {
        var read = EbnfGrammarReader.Read(source);

        AssertProductivityError(read, expectedRules.Split(','));
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void Dead_suffix_diagnosis_does_not_depend_on_rule_declaration_order(
        bool reverse, bool consumingRecursion)
    {
        // The reader uses the first declared rule as the start symbol, so S is
        // deliberately kept first while the other declarations are reversed.
        var declarations = new[]
        {
            "A : B 'x' ;",
            "B : 'a' ;",
            "C : 'a' ;",
            consumingRecursion ? "U : 'u' U ;" : "U : U ;"
        };
        string source = "S : A U | C 'x' ; " + string.Join(" ",
            reverse ? declarations.Reverse() : declarations);

        var read = EbnfGrammarReader.Read(source);

        AssertProductivityError(read, "U");
    }

    [Fact]
    public void Direct_model_construction_wraps_productivity_failure_as_EbnfException()
    {
        var model = new EbnfModel();
        var start = new EbnfRuleDef { Name = "S" };
        start.Alternatives.Add(new List<EbnfSym> { EbnfSym.Ref("U") });
        var dead = new EbnfRuleDef { Name = "U" };
        dead.Alternatives.Add(new List<EbnfSym> { EbnfSym.Ref("U") });
        model.Rules.Add(start);
        model.Rules.Add(dead);

        var error = Assert.Throws<EbnfException>(() => new EbnfGrammar(model));

        Assert.Matches(@"\bU\b", error.Message);
        Assert.Contains("finite", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("internal error:", error.Message);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Predicate_only_epsilon_rule_is_productive_without_evaluating_its_callback(bool value)
    {
        int calls = 0;
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => { calls++; return value; });

        var read = EbnfGrammarReader.Read("S : {Allow()}? ;", options);

        Assert.True(read.Success, string.Join(Environment.NewLine, read.Errors));
        Assert.Empty(read.Errors);
        Assert.Equal(0, calls);
        // Constructing the parser must not execute a condition either. Its
        // actual true/false value matters only when input is parsed.
        _ = new LALRParser(read.Grammar);
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Nonproductive_rule_is_reported_without_evaluating_a_neighboring_predicate(bool value)
    {
        int calls = 0;
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => { calls++; return value; });

        var read = EbnfGrammarReader.Read("S : 'z' | {Allow()}? U ; U : U ;", options);

        AssertProductivityError(read, "U");
        Assert.Equal(0, calls);
    }

    [Fact]
    public void Optional_in_a_direct_model_preserves_the_finite_empty_input_case()
    {
        var model = new EbnfModel();
        var start = new EbnfRuleDef { Name = "S" };
        var optional = EbnfSym.Literal("a");
        optional.Quantifier = EbnfQuantifier.Optional;
        start.Alternatives.Add(new List<EbnfSym> { optional });
        model.Rules.Add(start);

        var grammar = new EbnfGrammar(model);

        AssertParsing(grammar, "", true);
        AssertParsing(grammar, "a", true);
        AssertParsing(grammar, "aa", false);
    }

    [Theory]
    [InlineData("S : A ; A : A 'a' | 'b' ;", "b", true)]
    [InlineData("S : A ; A : A 'a' | 'b' ;", "baaa", true)]
    [InlineData("S : A ; A : A 'a' | 'b' ;", "a", false)]
    [InlineData("S : A ; A : 'a' A | 'b' ;", "aaab", true)]
    [InlineData("S : A ; A : 'a' A | 'b' ;", "", false)]
    [InlineData("S : A ; A : B 'a' | 'b' ; B : A 'c' | 'd' ;", "bca", true)]
    [InlineData("S : A ; A : B 'a' | 'b' ; B : A 'c' | 'd' ;", "da", true)]
    [InlineData("S : 'a'* 'b' ;", "b", true)]
    [InlineData("S : 'a'* 'b' ;", "aaab", true)]
    [InlineData("S : 'a'* 'b' ;", "a", false)]
    [InlineData("S : 'a'? 'b' ;", "b", true)]
    [InlineData("S : 'a'? 'b' ;", "ab", true)]
    [InlineData("S : 'a'+ 'b' ;", "ab", true)]
    [InlineData("S : 'a'+ 'b' ;", "b", false)]
    [InlineData("S : ('a' | 'b')* 'c' ;", "abc", true)]
    [InlineData("S : A ; A : B ; B : 'a' ;", "a", true)]
    [InlineData("S : 'a' ; Unused : 'z' ;", "a", true)]
    [InlineData("S : 'a'? ;", "", true)]
    public void Read_then_construct_parser_preserves_productive_grammar_behavior(
        string source, string input, bool expectedSuccess)
    {
        var read = EbnfGrammarReader.Read(source);

        Assert.True(read.Success, string.Join(Environment.NewLine, read.Errors));
        Assert.Empty(read.Errors);
        AssertParsing(read.Grammar, input, expectedSuccess);
    }

    private static string AssertProductivityError(EbnfReadResult read, params string[] expectedRules)
    {
        Assert.False(read.Success);
        Assert.Null(read.Grammar);
        string message = Assert.Single(read.Errors);
        Assert.DoesNotContain("internal error:", message);
        Assert.Contains("finite", message, StringComparison.OrdinalIgnoreCase);
        foreach (string rule in expectedRules)
            Assert.Matches(@"\b" + Regex.Escape(rule) + @"\b", message);
        return message;
    }

    private static void AssertParsing(EbnfGrammar grammar, string input, bool expectedSuccess)
    {
        var parser = new LALRParser(grammar);
        Assert.False(parser.HasConflicts, parser.ConflictReport);
        var lexer = new Lexer();
        foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);

        var result = parser.Parsing(lexer.Lexing(input).TokensForParsing);

        Assert.Equal(expectedSuccess, result.Success);
        if (!expectedSuccess)
        {
            Assert.NotEmpty(result.AllErrors);
            return;
        }

        Assert.Empty(result.AllErrors);
        Assert.Equal(input, string.Concat(result.ToParseTree.AllTokens.Select(token => token.Input)));
    }
}
