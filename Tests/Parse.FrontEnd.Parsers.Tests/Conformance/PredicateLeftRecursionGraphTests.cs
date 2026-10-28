using System;
using System.Linq;
using System.Text;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.RegularGrammar;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Exercises nullable-path propagation through the public EBNF reader and exact native return paths.
/// Validation must distinguish a grammar reference cycle from a predicate-bearing zero-token cycle.
/// </summary>
[Trait("Category", "Ebnf")]
public class PredicateLeftRecursionGraphTests
{
    [Fact]
    public void Predicate_names_continue_propagating_after_nullability_has_stabilized()
    {
        const int nodeCount = 128;
        var text = new StringBuilder("A : N000 A 'x' | 'x' ;\n");
        for (int i = 0; i < nodeCount - 1; i++)
            text.Append($"N{i:D3} : N{i + 1:D3} | 'q'? ;\n");
        text.Append($"N{nodeCount - 1:D3} : ").Append("{Allow()}? | 'q'? ;");

        var calls = 0;
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => { calls++; return true; });

        // Every N can already be empty through its optional q. The later discovery that
        // an empty path can also run Allow must travel back through all 128 references.
        var result = EbnfGrammarReader.Read(text.ToString(), options);

        var error = AssertLeftRecursionError(result);
        Assert.StartsWith("Rule 'A'", error);
        AssertClosedDiagnosticPath(error, "A");
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_indirect_return_is_rejected_only_when_its_prefix_can_skip_the_token(bool optionalToken)
    {
        var calls = 0;
        var grammar = new NativeGraphGrammar(g =>
        {
            g.Guard.Add(new NonTerminalConcat(new Epsilon())
            {
                Predicate = new GrammarPredicate("Allow", _ => { calls++; return true; })
            });
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.Guard, g.B, g.X);
            g.A.Add(g.AValue);
            g.B.Add(g.C);
            g.C.Add(optionalToken ? (Symbol)g.Q.Optional() : g.Q, g.A);
            g.C.Add(g.BValue);
        });

        // A -> B -> C is guarded in both grammars. Only a mandatory q breaks the
        // zero-token return from C to A; writing q before A is insufficient if q is optional.
        if (optionalToken)
        {
            var error = Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());
            Assert.Equal("A", error.RuleName);
            Assert.Equal("Allow", error.PredicateName);
            Assert.Equal(new[] { "A", "B", "C", "A" }, error.RulePath);
        }
        else
        {
            grammar.Optimization();
        }
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Return_path_search_skips_a_nonreturning_cycle_and_finds_the_other_branch(bool cycleBranchFirst)
    {
        var calls = 0;
        var grammar = new NativeGraphGrammar(g =>
        {
            g.Guard.Add(new NonTerminalConcat(new Epsilon())
            {
                Predicate = new GrammarPredicate("Allow", _ => { calls++; return true; })
            });
            g.Start.Add(g.A, g.Y);
            g.A.Add(g.Guard, g.B, g.X);
            g.A.Add(g.AValue);
            if (cycleBranchFirst)
            {
                g.B.Add(g.C);
                g.B.Add(g.D);
            }
            else
            {
                g.B.Add(g.D);
                g.B.Add(g.C);
            }
            g.C.Add(g.C, g.CValue);
            g.C.Add(g.BValue);
            g.D.Add(g.E);
            g.E.Add(g.A);
        });

        // C can revisit itself but cannot return to A. Its visited entry must neither
        // trap the search nor hide the separate B -> D -> E -> A return path.
        var error = Assert.Throws<PredicateLeftRecursionException>(() => grammar.Optimization());
        Assert.Equal("A", error.RuleName);
        Assert.Equal("Allow", error.PredicateName);
        Assert.Equal(new[] { "A", "B", "D", "E", "A" }, error.RulePath);
        Assert.DoesNotContain("C", error.RulePath);
        Assert.Equal(0, calls);
    }

    [Fact]
    public void An_external_predicate_does_not_mark_an_unrelated_mutually_recursive_component()
    {
        var text = """
            S : {Allow()}? B 'y' ;
            B : C 'x' | 'b' ;
            C : B 'c' | 'd' ;
            """;
        var calls = 0;
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => { calls++; return true; });

        var result = EbnfGrammarReader.Read(text, options);

        // B and C form an ordinary left-recursive component. Neither can return to S,
        // so the Allow check on S -> B is outside that component and must not reject it.
        Assert.True(result.Success, string.Join("; ", result.Errors));
        Assert.Empty(result.Errors);
        result.Grammar.Optimization();
        Assert.Equal(0, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void A_prefix_condition_propagates_only_when_its_whole_alternative_can_be_empty(bool optionalSuffix)
    {
        var suffix = optionalSuffix ? "'z'?" : "'z'";
        var text = "A : B A 'x' | 'x' ;\n"
            + "B : {Allow()}? C | 'q'? ;\n"
            + $"C : {suffix} ;";
        var calls = 0;
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => { calls++; return true; });

        var result = EbnfGrammarReader.Read(text, options);

        // B is nullable through q? in both grammars. Its guarded alternative contributes
        // Allow to an empty prefix only when C can also finish without consuming z.
        if (optionalSuffix)
        {
            var error = AssertLeftRecursionError(result);
            Assert.StartsWith("Rule 'A'", error);
            AssertClosedDiagnosticPath(error, "A");
        }
        else
        {
            Assert.True(result.Success, string.Join("; ", result.Errors));
            Assert.Empty(result.Errors);
            result.Grammar.Optimization();
        }
        Assert.Equal(0, calls);
    }

    private static string AssertLeftRecursionError(EbnfReadResult result)
    {
        Assert.False(result.Success);
        Assert.Null(result.Grammar);
        var error = Assert.Single(result.Errors);
        Assert.Contains("left recursion", error);
        Assert.Contains("Allow", error);
        Assert.DoesNotContain("internal error:", error);
        return error;
    }

    private static void AssertClosedDiagnosticPath(string error, string ruleName)
    {
        // EBNF lowering retains generated intermediate rules in the diagnostic path.
        // The public diagnostic must still close at the original named rule; it need
        // not present a direct A -> A edge when generated rules lie between them.
        int open = error.IndexOf('(');
        Assert.True(open >= 0, "The diagnostic must include its rule path.");
        int close = error.IndexOf(')', open + 1);
        Assert.True(close > open, "The diagnostic rule path must be closed with ')'.");
        var path = error.Substring(open + 1, close - open - 1)
            .Split(new[] { " -> " }, StringSplitOptions.None);
        Assert.True(path.Length >= 2, "A recursive path must include its return to the starting rule.");
        Assert.Equal(ruleName, path[0]);
        Assert.Equal(ruleName, path[path.Length - 1]);
    }

    private sealed class NativeGraphGrammar : Grammar
    {
        internal readonly NonTerminal Start = new NonTerminal("S", true);
        internal readonly NonTerminal A = new NonTerminal("A");
        internal readonly NonTerminal B = new NonTerminal("B");
        internal readonly NonTerminal C = new NonTerminal("C");
        internal readonly NonTerminal D = new NonTerminal("D");
        internal readonly NonTerminal E = new NonTerminal("E");
        internal readonly NonTerminal Guard = new NonTerminal("Guard");
        internal readonly Terminal X = new Terminal(TokenType.Keyword, "x", "x", true, false);
        internal readonly Terminal Y = new Terminal(TokenType.Keyword, "y", "y", true, false);
        internal readonly Terminal Q = new Terminal(TokenType.Keyword, "q", "q", true, false);
        internal readonly Terminal AValue = new Terminal(TokenType.Keyword, "a", "a", true, false);
        internal readonly Terminal BValue = new Terminal(TokenType.Keyword, "b", "b", true, false);
        internal readonly Terminal CValue = new Terminal(TokenType.Keyword, "c", "c", true, false);
        public override NonTerminal EbnfRoot => Start;
        public NativeGraphGrammar(Action<NativeGraphGrammar> configure)
        {
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
