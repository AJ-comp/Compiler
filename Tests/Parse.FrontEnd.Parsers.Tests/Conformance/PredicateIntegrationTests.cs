using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Ast;
using Janglim.FrontEnd.Grammars;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Exercise predicates through the public lexer/parser pipeline, independently of the
/// EBNF reader. These tests cover physical token context, real parse side effects, and
/// reuse after both an unsuccessful parse and an incremental source edit.
/// </summary>
[Trait("Category", "Predicates")]
public class PredicateIntegrationTests
{
    [Theory]
    [InlineData(true, "x y", true)]
    [InlineData(true, "x y z", true)]
    [InlineData(false, "x y", false)]
    [InlineData(false, "x y z", true)]
    public void Guarded_epsilon_and_shift_choose_the_path_that_fits_the_actual_input(
        bool allowed, string input, bool success)
    {
        var grammar = ChoiceGrammar(_ => allowed);
        var parser = new LALRParser(grammar, true);
        Assert.True(parser.HasConflicts);

        var result = Parse(parser, input);

        Assert.Equal(success, result.Success);
        if (success)
        {
            Assert.Empty(result.AllErrors);
            Assert.Equal(input.Split(' '), result.ToParseTree.AllTokens.Select(token => token.Input));
            Assert.Null(parser.FiredException);
        }
    }

    [Theory]
    [InlineData(" ", false)]
    [InlineData("\n    ", true)]
    [InlineData("\r\n", true)]
    [InlineData(" /* explanation */ ", false)]
    [InlineData(" /* explanation\n */ ", true)]
    [InlineData("// explanation\r\n", true)]
    public void Predicate_observes_the_exact_source_gap_between_previous_and_next_tokens(
        string gap, bool hasLineBreak)
    {
        var observations = new List<PredicateContext>();
        var grammar = SingleGuardGrammar(context =>
        {
            observations.Add(context);
            return !context.NextToken.HasLineBreakBefore;
        });
        var parser = new LALRParser(grammar, true);

        var result = Parse(parser, "x" + gap + "y");

        Assert.Equal(!hasLineBreak, result.Success);
        Assert.NotEmpty(observations);
        Assert.All(observations, context =>
        {
            Assert.Equal(1, context.TokenIndex);
            Assert.False(context.IsEndOfInput);
            Assert.Equal("x", context.PreviousToken.Input);
            Assert.Equal("y", context.NextToken.Input);
            Assert.Equal(gap, context.NextToken.LeadingTrivia.Text);
            Assert.Equal(1, context.NextToken.LeadingTrivia.StartIndex);
            Assert.Equal(1 + gap.Length, context.NextToken.StartIndex);
            Assert.Equal(hasLineBreak, context.NextToken.HasLineBreakBefore);
        });
        if (hasLineBreak)
        {
            var error = Assert.Single(result.AllErrors);
            var token = Assert.Single(error.ErrTokens);
            Assert.Equal("y", token.Input);
            Assert.Equal(1 + gap.Length, token.StartIndex);
        }
    }

    [Fact]
    public void Start_middle_and_eof_predicates_receive_their_physical_token_positions()
    {
        var observations = new List<(string Name, PredicateContext Context)>();
        var grammar = new TestGrammar(g =>
        {
            g.Guard(g.G1, "AtStart", context => Record("start", context));
            g.Guard(g.G2, "Between", context => Record("middle", context));
            g.Guard(g.G3, "AtEnd", context => Record("end", context));
            g.Start.Add(g.G1, g.X, g.G2, g.Y, g.G3);
        });
        bool Record(string name, PredicateContext context)
        {
            observations.Add((name, context));
            return true;
        }

        var result = Parse(new LALRParser(grammar, true), "  x\n y /* trailing */");

        Assert.True(result.Success);
        Assert.Contains(observations, item => item.Name == "start");
        Assert.Contains(observations, item => item.Name == "middle");
        Assert.Contains(observations, item => item.Name == "end");
        Assert.All(observations.Where(item => item.Name == "start"), item =>
        {
            Assert.Equal(0, item.Context.TokenIndex);
            Assert.Null(item.Context.PreviousToken);
            Assert.Equal("x", item.Context.NextToken.Input);
            Assert.Equal("  ", item.Context.NextToken.LeadingTrivia.Text);
            Assert.False(item.Context.IsEndOfInput);
        });
        Assert.All(observations.Where(item => item.Name == "middle"), item =>
        {
            Assert.Equal(1, item.Context.TokenIndex);
            Assert.Equal("x", item.Context.PreviousToken.Input);
            Assert.Equal("y", item.Context.NextToken.Input);
            Assert.Equal("\n ", item.Context.NextToken.LeadingTrivia.Text);
        });
        Assert.All(observations.Where(item => item.Name == "end"), item =>
        {
            Assert.Equal(2, item.Context.TokenIndex);
            Assert.Equal("y", item.Context.PreviousToken.Input);
            Assert.IsType<EndMarker>(item.Context.NextToken.Kind);
            Assert.True(item.Context.IsEndOfInput);
        });
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void Predicate_after_the_last_physical_token_can_accept_or_reject_eof(bool allow, bool success)
    {
        var grammar = new TestGrammar(g =>
        {
            g.Guard(g.G1, "Finish", context => context.IsEndOfInput && allow);
            g.Start.Add(g.X, g.G1);
        });
        var result = Parse(new LALRParser(grammar, true), "x");

        Assert.Equal(success, result.Success);
        if (!success)
            Assert.IsType<EndMarker>(Assert.Single(Assert.Single(result.AllErrors).ErrTokens).Kind);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(-1, true)]
    public void Consecutive_predicates_all_examine_the_same_unconsumed_token(int falseGuard, bool shortSuccess)
    {
        var contexts = new List<PredicateContext>();
        var grammar = new TestGrammar(g =>
        {
            foreach (var pair in new[] { (g.G1, 0), (g.G2, 1), (g.G3, 2) })
                g.Guard(pair.Item1, "Check" + pair.Item2, context =>
                {
                    contexts.Add(context);
                    return pair.Item2 != falseGuard;
                });
            g.Start.Add(g.X, g.G1, g.G2, g.G3, g.Y);
            g.Start.Add(g.X, g.Y, g.Z);
        });
        var parser = new LALRParser(grammar, true);

        Assert.Equal(shortSuccess, Parse(parser, "x y").Success);
        Assert.True(Parse(parser, "x y z").Success);
        Assert.NotEmpty(contexts);
        Assert.All(contexts, context =>
        {
            Assert.Equal(1, context.TokenIndex);
            Assert.Equal("x", context.PreviousToken.Input);
            Assert.Equal("y", context.NextToken.Input);
        });
    }

    [Fact]
    public void Failed_guard_does_not_change_shared_table_actions_or_poison_the_next_parse()
    {
        var allowed = false;
        var parser = new LALRParser(ChoiceGrammar(_ => allowed), true);
        var table = Assert.IsType<LRParsingTable>(parser.ParsingTable);
        var actions = table.SelectMany(row => row.MatchedValueSet.Values)
            .SelectMany(cell => cell).Select(action => (Action: action, action.Direction, action.Dest)).ToArray();

        Assert.False(Parse(parser, "x y").Success);
        allowed = true;
        Assert.True(Parse(parser, "x y").Success);
        Assert.True(Parse(parser, "x y z").Success);
        allowed = false;
        Assert.True(Parse(parser, "x y z").Success);
        Assert.False(Parse(parser, "x y").Success);
        allowed = true;
        Assert.True(Parse(parser, "x y").Success);
        Assert.Null(parser.FiredException);

        foreach (var snapshot in actions)
        {
            Assert.Equal(snapshot.Direction, snapshot.Action.Direction);
            Assert.Same(snapshot.Dest, snapshot.Action.Dest);
        }
        Assert.Equal(actions.Select(item => item.Action), table.SelectMany(row => row.MatchedValueSet.Values).SelectMany(cell => cell));
    }

    [Fact]
    public void Callback_exception_is_preserved_and_diagnosed_at_the_guard_position()
    {
        var expected = new InvalidOperationException("fixture predicate failure");
        var throwNow = true;
        var parser = new LALRParser(SingleGuardGrammar(_ => throwNow ? throw expected : true), true);
        ParsingUnit failedUnit = null;
        parser.ActionFailed += (_, unit) => failedUnit = unit;

        var result = Parse(parser, "x  y");

        Assert.False(result.Success);
        var fired = Assert.IsType<GrammarPredicateEvaluationException>(parser.FiredException);
        Assert.Equal("Allow", fired.PredicateName);
        Assert.Equal(1, fired.Context.TokenIndex);
        Assert.Contains(ExceptionChain(fired), exception => ReferenceEquals(exception, expected));
        Assert.True(result.HasError);
        Assert.NotNull(failedUnit);
        Assert.True(failedUnit.IsError);
        var error = Assert.Single(result.AllErrors);
        Assert.Contains("Allow", error.Message);
        var token = Assert.Single(error.ErrTokens);
        Assert.Equal("y", token.Input);
        Assert.Equal(3, token.StartIndex);

        throwNow = false;
        Assert.True(Parse(parser, "x y").Success);
        Assert.Null(parser.FiredException);
    }

    [Fact]
    public void Callback_exception_cannot_be_hidden_by_an_otherwise_valid_shift_alternative()
    {
        var expected = new InvalidOperationException("broken condition implementation");
        var parser = new LALRParser(ChoiceGrammar(_ => throw expected), true);

        var result = Parse(parser, "x y z");

        Assert.False(result.Success);
        var fired = Assert.IsType<GrammarPredicateEvaluationException>(parser.FiredException);
        Assert.Same(expected, fired.InnerException);
        Assert.Equal("Allow", fired.PredicateName);
        Assert.Equal("y", Assert.Single(Assert.Single(result.AllErrors).ErrTokens).Input);
    }

    [Fact]
    public void Custom_syntax_recovery_cannot_report_success_after_a_callback_exception()
    {
        var parser = new LALRParser(SingleGuardGrammar(_ => throw new InvalidOperationException("broken guard")), true);
        var recovery = new OptimisticRecovery();
        parser.AddErrorHandler(recovery);

        var result = Parse(parser, "x y");

        Assert.False(result.Success);
        Assert.True(result.HasError);
        Assert.IsType<GrammarPredicateEvaluationException>(parser.FiredException);
        Assert.Equal(0, recovery.Calls);
    }

    [Fact]
    public void Predicate_true_cannot_resolve_two_alternatives_that_accept_the_same_complete_input()
    {
        var grammar = new TestGrammar(g =>
        {
            g.Guard(g.G1, "Allow", _ => true);
            g.Start.Add(g.X, g.G1, g.Y);
            g.Start.Add(g.X, g.Y);
        });
        var parser = new LALRParser(grammar, true);

        var result = Parse(parser, "x y");

        Assert.False(result.Success);
        Assert.Equal("AmbiguousPredictionException", parser.FiredException?.GetType().Name);
        Assert.NotEmpty(result.AllErrors);
        Assert.Contains("More than one", Assert.Single(result.AllErrors).Message);
    }

    [Fact]
    public void Exception_from_a_future_predicate_is_diagnosed_at_that_token_not_the_initial_conflict()
    {
        var expected = new InvalidOperationException("later predicate implementation failed");
        var grammar = new TestGrammar(g =>
        {
            g.Guard(g.G1, "Allow", _ => true);
            g.Guard(g.G2, "Later", _ => throw expected);
            g.Start.Add(g.X, g.G1, g.Y, g.G2, g.Z);
            g.Start.Add(g.X, g.Y, g.Z, g.T);
        });
        var parser = new LALRParser(grammar, true);

        var result = Parse(parser, "x y z");

        Assert.False(result.Success);
        var fired = Assert.IsType<GrammarPredicateEvaluationException>(parser.FiredException);
        Assert.Equal("Later", fired.PredicateName);
        Assert.Same(expected, fired.InnerException);
        Assert.Equal(2, fired.Context.TokenIndex);
        Assert.Equal("y", fired.Context.PreviousToken.Input);
        Assert.Equal("z", fired.Context.NextToken.Input);
        var token = Assert.Single(Assert.Single(result.AllErrors).ErrTokens);
        Assert.Equal("z", token.Input);
        Assert.Equal(4, token.StartIndex);
        Assert.Same(fired.Context.NextToken, token);
    }

    [Fact]
    public void False_future_predicate_eliminates_its_path_without_consuming_that_path_in_the_real_parser()
    {
        var observations = new List<PredicateContext>();
        var grammar = new TestGrammar(g =>
        {
            g.Guard(g.G1, "Allow", _ => true);
            g.Guard(g.G2, "Later", context =>
            {
                observations.Add(context);
                return false;
            });
            g.Start.Add(g.X, g.G1, g.Y, g.G2, g.Z);
            g.Start.Add(g.X, g.Y, g.Z, g.T);
        });
        var parser = new LALRParser(grammar, true);

        var result = Parse(parser, "x y z t");

        Assert.True(result.Success);
        Assert.NotEmpty(observations);
        Assert.All(observations, context =>
        {
            Assert.Equal(2, context.TokenIndex);
            Assert.Equal("y", context.PreviousToken.Input);
            Assert.Equal("z", context.NextToken.Input);
        });
        Assert.DoesNotContain(result.SelectMany(block => block.Units), unit =>
            unit.Action.Direction == ActionDir.EpsilonReduce);
    }

    [Theory]
    [InlineData("x y")]
    [InlineData("x y z")]
    public void Prediction_only_creates_tree_and_ast_nodes_for_the_chosen_real_path(string input)
    {
        var grammar = ChoiceGrammar(_ => true, tagged: true);
        var parser = new LALRParser(grammar, true);
        var trees = new List<ParseTreeSymbol>();
        var asts = new List<AstSymbol>();
        parser.ParseTreeCreated += (_, args) => trees.Add(args.ParseTreeNonT);
        parser.ASTCreated += (_, ast) => asts.Add(ast);

        var result = Parse(parser, input);

        Assert.True(result.Success);
        var executedTreeActions = result.SelectMany(block => block.Units)
            .Count(unit => unit.Action.Direction is ActionDir.Shift or ActionDir.Reduce or ActionDir.EpsilonReduce);
        Assert.Equal(executedTreeActions, trees.Count);
        Assert.Equal(input.Split(' '), trees.OfType<ParseTreeTerminal>().Select(node => node.Token.Input));
        Assert.Equal(input.Split(' '), result.AstRoot.AllTokens.Select(token => token.Input));
        Assert.Equal(input.EndsWith("z", StringComparison.Ordinal) ? "B" : "A", Assert.IsType<AstNonTerminal>(result.AstRoot).Name);
        if (input.EndsWith("z", StringComparison.Ordinal))
        {
            Assert.DoesNotContain(trees.OfType<ParseTreeNonTerminal>(), node => node.ToNonTerminal.Name == "Guard1");
            Assert.DoesNotContain(asts.OfType<AstNonTerminal>(), node => node.Name == "A");
        }
    }

    [Fact]
    public void Ordinary_nullable_conflict_keeps_legacy_shift_policy_even_when_grammar_has_another_predicate()
    {
        var grammar = new TestGrammar(g =>
        {
            g.G1.Add(new Epsilon());
            g.Guard(g.G2, "OtherPath", _ => true);
            g.Start.Add(g.X, g.G1, g.Y);
            g.Start.Add(g.X, g.Y, g.Z);
            g.Start.Add(g.T, g.G2, g.Y);
        });
        var parser = new LALRParser(grammar, true);
        Assert.True(parser.HasConflicts);

        Assert.False(Parse(parser, "x y").Success);
        Assert.True(Parse(parser, "x y z").Success);
        Assert.True(Parse(parser, "t y").Success);
    }

    [Fact]
    public void Ordinary_expression_shift_reduce_conflict_after_a_guard_keeps_legacy_shift_policy()
    {
        var grammar = new TestGrammar(g =>
        {
            g.Guard(g.G1, "BeforeExpression", _ => true);
            g.Start.Add(g.X, g.G1, g.Expr);
            g.Expr.Add(g.Expr, g.Plus, g.Expr);
            g.Expr.Add(g.Y);
        });
        var parser = new LALRParser(grammar, true);
        Assert.True(parser.HasConflicts);

        var result = Parse(parser, "x y + y + y");

        Assert.True(result.Success);
        // With the legacy shift preference, the second '+' is read before reducing
        // E -> E '+' E. Predicate support must not silently change associativity.
        Assert.Contains(result[4].Units, unit => unit.Action.Direction == ActionDir.Shift);
        Assert.DoesNotContain(result[4].Units, unit => unit.Action.Direction == ActionDir.Reduce &&
            unit.Action.Dest is NonTerminalSingle production && production.Name == "E" && production.Count == 3);
        Assert.Equal(2, result.Last().Units.Count(unit => unit.Action.Direction == ActionDir.Reduce &&
            unit.Action.Dest is NonTerminalSingle production && production.Name == "E" && production.Count == 3));
    }

    [Theory]
    [InlineData("x y", "x\n y", false)]
    [InlineData("x y", "x/* comment\n */y", false)]
    [InlineData("x y", "x  y", true)]
    [InlineData("x y", "x y z", false)]
    public void Incremental_edit_rechecks_predicates_with_current_physical_tokens(string before, string after, bool success)
    {
        var observations = new List<PredicateContext>();
        var grammar = SingleGuardGrammar(context =>
        {
            observations.Add(context);
            return !context.NextToken.HasLineBreakBefore;
        });
        var lexer = CreateLexer(grammar);
        var parser = new LALRParser(grammar, true);
        var lexed = lexer.Lexing(before);
        var previous = parser.Parsing(lexed.TokensForParsing);
        Assert.True(previous.Success);
        observations.Clear();

        lexed.ReplaceToken(new Janglim.Range(0, lexed.TokensForView.Count), lexer.Lexing(after).TokensForView);
        var result = parser.Parsing(lexed, previous);

        Assert.Equal(success, result.Success);
        Assert.NotEmpty(observations);
        Assert.All(observations, context =>
        {
            Assert.Equal(1, context.TokenIndex);
            Assert.Same(lexed.TokensForParsing[0], context.PreviousToken.TokenCell);
            Assert.Same(lexed.TokensForParsing[1], context.NextToken.TokenCell);
            Assert.Equal(lexed.TokensForParsing[1].LeadingTrivia.Text, context.NextToken.LeadingTrivia.Text);
        });
    }

    [Fact]
    public void Retained_cells_with_trivia_only_edit_cannot_reuse_a_previous_guard_success()
    {
        var observations = new List<PredicateContext>();
        var grammar = SingleGuardGrammar(context =>
        {
            observations.Add(context);
            return !context.NextToken.HasLineBreakBefore;
        });
        var lexer = CreateLexer(grammar);
        var parser = new LALRParser(grammar, true);
        var lexed = lexer.Lexing("x y");
        var previous = parser.Parsing(lexed.TokensForParsing);
        Assert.True(previous.Success);
        var y = lexed.TokensForParsing[1];
        observations.Clear();

        lexed.ReplaceToken(new Janglim.Range(1, 1), lexer.Lexing("/* change\n */").TokensForView);
        var result = parser.Parsing(lexed, previous);

        Assert.False(result.Success);
        Assert.Same(y, lexed.TokensForParsing[1]);
        Assert.NotEmpty(observations);
        Assert.All(observations, context =>
        {
            Assert.Same(y, context.NextToken.TokenCell);
            Assert.True(context.NextToken.HasLineBreakBefore);
            Assert.Equal("/* change\n */", context.NextToken.LeadingTrivia.Text);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Public_unit_parsing_reports_whether_surrounding_physical_input_was_supplied(bool fullContext)
    {
        PredicateContext observed = null;
        var grammar = SingleGuardGrammar(context =>
        {
            observed = context;
            return true;
        });
        var parser = new LALRParser(grammar, false);
        var tokens = Tokens(grammar, "x y");
        var first = ParsingUnit.FirstParsingUnit;
        Assert.Equal(LRParser.SuccessedKind.Shift, parser.UnitParsing(first, tokens, 0).Item1);
        var guardedUnit = new ParsingUnit(first.AfterStack);

        var action = fullContext
            ? parser.UnitParsing(guardedUnit, tokens, 1)
            : parser.UnitParsing(guardedUnit, tokens[1]);

        Assert.Equal(LRParser.SuccessedKind.ReduceOrGoto, action.Item1);
        Assert.NotNull(observed);
        Assert.Same(tokens[1], observed.NextToken);
        Assert.Equal(fullContext ? 1 : -1, observed.TokenIndex);
        if (fullContext) Assert.Same(tokens[0], observed.PreviousToken);
        else Assert.Null(observed.PreviousToken);
    }

    [Fact]
    public void Isolated_unit_parsing_does_not_guess_a_path_when_conflict_needs_future_input()
    {
        var grammar = ChoiceGrammar(_ => true);
        var parser = new LALRParser(grammar, false);
        var tokens = Tokens(grammar, "x y");
        var first = ParsingUnit.FirstParsingUnit;
        Assert.Equal(LRParser.SuccessedKind.Shift, parser.UnitParsing(first, tokens, 0).Item1);
        var conflictUnit = new ParsingUnit(first.AfterStack);

        var action = parser.UnitParsing(conflictUnit, tokens[1]);

        Assert.Equal(LRParser.SuccessedKind.NotApplicable, action.Item1);
        Assert.True(conflictUnit.IsError);
        Assert.Contains("complete input", conflictUnit.ErrorMessage);
        Assert.Equal(ActionDir.Failed, conflictUnit.Action.Direction);
        Assert.Null(action.Item2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Recovery_predicates_observe_the_replacement_token_and_preserve_its_physical_boundary(bool hasConflict)
    {
        var observations = new List<PredicateContext>();
        Func<PredicateContext, bool> check = context =>
        {
            observations.Add(context);
            return context.NextToken.Input == "y";
        };
        var grammar = hasConflict ? ChoiceGrammar(check) : SingleGuardGrammar(check);
        var parser = new LALRParser(grammar, true);
        var replacement = Tokens(grammar, "y")[0];
        var recovery = new ReplacingRecovery(replacement);
        parser.AddErrorHandler(recovery);

        var result = Parse(parser, "x t");

        Assert.True(result.Success);
        Assert.Equal(1, recovery.Calls);
        Assert.NotEmpty(observations);
        Assert.All(observations, context =>
        {
            Assert.Equal(1, context.TokenIndex);
            Assert.Equal("x", context.PreviousToken.Input);
            Assert.Same(replacement, context.NextToken);
        });
        Assert.Equal(new[] { "x", "y" }, result.ToParseTree.AllTokens.Select(token => token.Input));
        Assert.Empty(result.AllErrors);
    }

    [Fact]
    public void Recovery_callback_exception_keeps_the_replacement_context_and_original_source_diagnostic()
    {
        var expected = new InvalidOperationException("replacement guard failed");
        var grammar = SingleGuardGrammar(_ => throw expected);
        var parser = new LALRParser(grammar, true);
        var replacement = Tokens(grammar, "y")[0];
        parser.AddErrorHandler(new ReplacingRecovery(replacement));

        var result = Parse(parser, "x  t");

        Assert.False(result.Success);
        var fired = Assert.IsType<GrammarPredicateEvaluationException>(parser.FiredException);
        Assert.Same(expected, fired.InnerException);
        Assert.Equal(1, fired.Context.TokenIndex);
        Assert.Equal("x", fired.Context.PreviousToken.Input);
        Assert.Same(replacement, fired.Context.NextToken);
        var sourceToken = Assert.Single(Assert.Single(result.AllErrors).ErrTokens);
        Assert.Equal("t", sourceToken.Input);
        Assert.Equal(3, sourceToken.StartIndex);
    }

    [Fact]
    public void Multiple_recovery_tokens_are_available_to_prediction_and_later_recovery_predicates()
    {
        var observations = new List<(string Name, PredicateContext Context)>();
        var grammar = new TestGrammar(g =>
        {
            g.Guard(g.G1, "First", context => Record("first", context));
            g.Guard(g.G2, "Second", context => Record("second", context));
            g.Start.Add(g.X, g.G1, g.Y, g.G2, g.Z);
            g.Start.Add(g.X, g.Y, g.Z, g.T);
        });
        bool Record(string name, PredicateContext context)
        {
            observations.Add((name, context));
            return true;
        }
        var parser = new LALRParser(grammar, true);
        var replacements = Tokens(grammar, "y z").Take(2).ToArray();
        parser.AddErrorHandler(new ReplacingRecovery(replacements));

        var result = Parse(parser, "x t");

        Assert.True(result.Success);
        Assert.Contains(observations, item => item.Name == "first");
        Assert.Contains(observations, item => item.Name == "second");
        Assert.All(observations.Where(item => item.Name == "first"), item =>
        {
            Assert.Equal(1, item.Context.TokenIndex);
            Assert.Equal("x", item.Context.PreviousToken.Input);
            Assert.Same(replacements[0], item.Context.NextToken);
        });
        Assert.All(observations.Where(item => item.Name == "second"), item =>
        {
            Assert.Equal(2, item.Context.TokenIndex);
            Assert.Same(replacements[0], item.Context.PreviousToken);
            Assert.Same(replacements[1], item.Context.NextToken);
        });
        Assert.Equal(new[] { "x", "y", "z" }, result.ToParseTree.AllTokens.Select(token => token.Input));
    }

    [Fact]
    public void Predicate_on_a_remaining_physical_token_remembers_the_previous_replacement()
    {
        PredicateContext observed = null;
        var grammar = new TestGrammar(g =>
        {
            g.Guard(g.G1, "AfterRecovery", context =>
            {
                observed = context;
                return context.PreviousToken?.Input == "y";
            });
            g.Start.Add(g.X, g.Y, g.G1, g.Z);
        });
        var parser = new LALRParser(grammar, true);
        var replacement = Tokens(grammar, "y")[0];
        parser.AddErrorHandler(new ReplacingRecovery(replacement));

        var result = Parse(parser, "x t z");

        Assert.True(result.Success);
        Assert.NotNull(observed);
        Assert.Same(replacement, observed.PreviousToken);
        Assert.Equal("z", observed.NextToken.Input);
        Assert.Equal(2, observed.TokenIndex);
    }

    [Fact]
    public void Predicate_at_eof_after_multiple_recovery_tokens_sees_the_last_replacement()
    {
        PredicateContext observed = null;
        var grammar = new TestGrammar(g =>
        {
            g.Guard(g.G1, "EndAfterRecovery", context =>
            {
                observed = context;
                return context.IsEndOfInput && context.PreviousToken?.Input == "z";
            });
            g.Start.Add(g.X, g.Y, g.Z, g.G1);
        });
        var parser = new LALRParser(grammar, true);
        var replacements = Tokens(grammar, "y z").Take(2).ToArray();
        parser.AddErrorHandler(new ReplacingRecovery(replacements));

        var result = Parse(parser, "x t");

        Assert.True(result.Success);
        Assert.NotNull(observed);
        Assert.Same(replacements[1], observed.PreviousToken);
        Assert.True(observed.IsEndOfInput);
    }

    private static TestGrammar ChoiceGrammar(Func<PredicateContext, bool> callback, bool tagged = false)
        => new TestGrammar(g =>
        {
            g.Guard(g.G1, "Allow", callback);
            if (tagged)
            {
                g.Start.SetItem(g.X + g.G1 + g.Y, new MeaningUnit("A"));
                g.Start.AddItem(g.X + g.Y + g.Z, new MeaningUnit("B"));
            }
            else
            {
                g.Start.Add(g.X, g.G1, g.Y);
                g.Start.Add(g.X, g.Y, g.Z);
            }
        });

    private static TestGrammar SingleGuardGrammar(Func<PredicateContext, bool> callback)
        => new TestGrammar(g =>
        {
            g.Guard(g.G1, "Allow", callback);
            g.Start.Add(g.X, g.G1, g.Y);
        });

    private static ParsingResult Parse(LALRParser parser, string input)
        => parser.Parsing(CreateLexer(parser.Grammar).Lexing(input).TokensForParsing);

    private static Lexer CreateLexer(Grammar grammar)
    {
        var lexer = new Lexer();
        foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
        return lexer;
    }

    private static TokenData[] Tokens(Grammar grammar, string input)
        => CreateLexer(grammar).Lexing(input).TokensForParsing
            .Select(cell => TokenData.CreateFromTokenCell(cell, false))
            .Append(new TokenData(new EndMarker(), new TokenCell(-1, -1, -1, "$", null)))
            .ToArray();

    private static IEnumerable<Exception> ExceptionChain(Exception exception)
    {
        for (; exception != null; exception = exception.InnerException) yield return exception;
    }

    private sealed class OptimisticRecovery : IErrorHandlable
    {
        public int Calls { get; private set; }

        public ErrorHandlingResult Call(DataForRecovery dataForRecovery)
        {
            Calls++;
            return dataForRecovery.ToErrorHandlingResult(true);
        }
    }

    private sealed class ReplacingRecovery : IErrorHandlable
    {
        private readonly TokenData[] replacements;
        public int Calls { get; private set; }

        public ReplacingRecovery(params TokenData[] replacements) => this.replacements = replacements;

        public ErrorHandlingResult Call(DataForRecovery dataForRecovery)
        {
            Calls++;
            var status = ((LRParser)dataForRecovery.Parser).RecoveryBlockParsing(dataForRecovery,
                replacements.Select(token => new ParsingRecoveryData(token, "replace unexpected token")));
            return dataForRecovery.ToErrorHandlingResult(status == LRParser.SuccessedKind.Shift);
        }
    }

    private sealed class TestGrammar : Grammar
    {
        private readonly Terminal x = new Terminal(TokenType.Operator, "x");
        private readonly Terminal y = new Terminal(TokenType.Operator, "y");
        private readonly Terminal z = new Terminal(TokenType.Operator, "z");
        private readonly Terminal t = new Terminal(TokenType.Operator, "t");
        private readonly Terminal plus = new Terminal(TokenType.Operator, "+");
        private readonly Terminal comment = new Terminal(TokenType.SpecialToken.Comment, "/\\*[\\s\\S]*?\\*/|//[^\\r\\n]*", "comment", false, true);
        private readonly NonTerminal start = new NonTerminal("S", true);
        private readonly NonTerminal g1 = new NonTerminal("Guard1");
        private readonly NonTerminal g2 = new NonTerminal("Guard2");
        private readonly NonTerminal g3 = new NonTerminal("Guard3");
        private readonly NonTerminal expression = new NonTerminal("E");

        public override NonTerminal EbnfRoot => start;
        public NonTerminal Start => start;
        public NonTerminal G1 => g1;
        public NonTerminal G2 => g2;
        public NonTerminal G3 => g3;
        public Terminal X => x;
        public Terminal Y => y;
        public Terminal Z => z;
        public Terminal T => t;
        public Terminal Plus => plus;
        public NonTerminal Expr => expression;

        public TestGrammar(Action<TestGrammar> define)
        {
            define(this);

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

        public void Guard(NonTerminal target, string name, Func<PredicateContext, bool> callback)
            => target.Add(new NonTerminalConcat(new Epsilon()) { Predicate = new GrammarPredicate(name, callback) });
    }
}
