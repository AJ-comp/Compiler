using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Collections;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;
using static Janglim.FrontEnd.Parsers.Datas.LR.LRParsingRowDataFormat;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// A custom source is allowed to implement its own scanner, but it must preserve the parser's
/// cursor, token and lexical-goal contracts. These tests deliberately violate one rule at a time.
/// </summary>
[Trait("Category", "ContextualLexing")]
public class ParserDrivenContractTests
{
    [Fact]
    public void Null_source_is_an_invalid_call_and_null_source_policy_is_a_contract_error()
    {
        var fixture = new Fixture();
        Assert.Throws<ArgumentNullException>(() => fixture.Parser.Parsing((ITokenSource)null));
        fixture.Source.Policy = null;

        var result = fixture.Parse();

        Assert.False(result.Success);
        Assert.IsAssignableFrom<ArgumentException>(fixture.Parser.FiredException);
        Assert.NotEmpty(result.AllErrors);
        Assert.Equal(0, fixture.Source.Calls);
    }

    [Theory]
    [InlineData("null-goals")]
    [InlineData("empty-goals")]
    [InlineData("null-goal")]
    [InlineData("duplicate-goal")]
    public void Invalid_goal_collections_are_rejected_when_the_policy_is_created(string violation)
    {
        var fixture = new Fixture();
        LexicalGoal[] goals = violation switch
        {
            "null-goals" => null,
            "empty-goals" => Array.Empty<LexicalGoal>(),
            "null-goal" => new LexicalGoal[] { null },
            "duplicate-goal" => new[] { fixture.Goal, fixture.Goal },
            _ => throw new InvalidOperationException("Unknown policy metadata case.")
        };

        Assert.ThrowsAny<ArgumentException>(() => new LexicalPolicy(goals));
        Assert.Equal(0, fixture.Source.Calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Token_list_recovery_and_backtracking_are_explicitly_rejected_before_reading(bool recovery)
    {
        var fixture = new Fixture();
        if (recovery) fixture.Parser.AddErrorHandler(new DefaultLRErrorHandler());
        else fixture.Parser.UseBackTrackingOnConflict();

        Assert.Throws<NotSupportedException>(() => fixture.Parse());

        Assert.Equal(0, fixture.Source.Calls);
        // Installing either option remains supported by the original token-list API.
        Assert.True(fixture.Parser.Parsing(fixture.Lexer.Lexing("x y").TokensForParsing).Success);
    }

    [Theory]
    [InlineData("null-start")]
    [InlineData("negative-length")]
    [InlineData("start-after-end")]
    [InlineData("nonzero-ordinal")]
    [InlineData("previous-token-at-start")]
    public void Invalid_source_metadata_fails_before_peek_with_a_positioned_result(string violation)
    {
        var fixture = new Fixture();
        var original = fixture.Source.Start;
        switch (violation)
        {
            case "null-start": fixture.Source.Start = null; break;
            case "negative-length": fixture.Source.Length = -1; break;
            case "start-after-end": fixture.Source.Start = new TokenCursor(original.SourceIdentity, 4); break;
            case "nonzero-ordinal": fixture.Source.Start = new TokenCursor(original.SourceIdentity, 0, 1); break;
            case "previous-token-at-start":
                fixture.Source.Start = new TokenCursor(original.SourceIdentity, 0, 0, Token(fixture.X, 0, "x"));
                break;
            default: throw new InvalidOperationException("Unknown source metadata case.");
        }

        var result = fixture.Parse();

        Assert.False(result.Success);
        Assert.IsAssignableFrom<ArgumentException>(fixture.Parser.FiredException);
        Assert.Equal(0, fixture.Source.Calls);
        Assert.NotEmpty(result.AllErrors);
        Assert.Empty(Shifted(result));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Null_or_foreign_selector_goals_are_not_sent_to_the_source(bool returnNull)
    {
        var fixture = new Fixture();
        var foreign = new LexicalGoal(fixture.Goal.Terminals);

        fixture.Source.Policy = new LexicalPolicy(new[] { fixture.Goal }, _ => returnNull ? null : foreign);

        var result = fixture.Parse();

        AssertContractFailure(fixture, result, 0, 0);
        Assert.Equal(0, fixture.Source.Calls);
    }

    [Theory]
    [InlineData("null-read")]
    [InlineData("null-kind")]
    [InlineData("missing-cell")]
    [InlineData("foreign-cursor")]
    [InlineData("cursor-after-end")]
    [InlineData("zero-width-token")]
    [InlineData("wrong-ordinal")]
    [InlineData("wrong-previous")]
    [InlineData("wrong-next-offset")]
    [InlineData("foreign-terminal-reference")]
    public void Invalid_reads_are_rejected_before_their_token_can_be_shifted(string violation)
    {
        var fixture = new Fixture();
        fixture.Source.Rewrite = (cursor, _, read) =>
        {
            TokenData token = read.Token;
            TokenCursor next = read.Next;
            switch (violation)
            {
                case "null-read": return null;
                case "null-kind": token = new TokenData(null, token.TokenCell); break;
                case "missing-cell": token = new TokenData(fixture.X, null); break;
                case "foreign-cursor":
                    next = new TokenCursor(new object(), next.SourceOffset, next.TokenIndex, token);
                    break;
                case "cursor-after-end":
                    next = new TokenCursor(cursor.SourceIdentity, fixture.Source.Length + 1, next.TokenIndex, token);
                    break;
                case "zero-width-token":
                    token = Token(fixture.X, cursor.SourceOffset, "");
                    next = new TokenCursor(cursor.SourceIdentity, cursor.SourceOffset, 1, token);
                    break;
                case "wrong-ordinal":
                    next = new TokenCursor(cursor.SourceIdentity, next.SourceOffset, 2, token);
                    break;
                case "wrong-previous":
                    next = new TokenCursor(cursor.SourceIdentity, next.SourceOffset, next.TokenIndex);
                    break;
                case "wrong-next-offset":
                    next = new TokenCursor(cursor.SourceIdentity, next.SourceOffset + 1, next.TokenIndex, token);
                    break;
                case "foreign-terminal-reference":
                    // InputTerminal preserves the grammar key, so table lookup alone would
                    // accept it. Goal membership must still require the registered object.
                    token = Token(new InputTerminal(fixture.X), 0, "x");
                    next = new TokenCursor(cursor.SourceIdentity, next.SourceOffset, next.TokenIndex, token);
                    break;
                default: throw new InvalidOperationException("Unknown token-source violation.");
            }
            return new TokenRead(token, next);
        };

        var result = fixture.Parse();

        AssertContractFailure(fixture, result, 0, 0);
        Assert.Equal(1, fixture.Source.Calls);
        Assert.Empty(Shifted(result));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Null_token_text_is_a_positioned_contract_error_for_physical_tokens_and_eof(bool atEof)
    {
        var fixture = new Fixture("S : 'x' ;", "x");
        fixture.Source.Rewrite = (cursor, _, read) =>
        {
            if ((read.Token.Kind is EndMarker) != atEof) return read;
            // TokenCell's public constructor permits null text. The parser must reject it
            // before asking EndIndex or EOF's text length to inspect that invalid value.
            var token = new TokenData(read.Token.Kind,
                new TokenCell(read.Token.StartIndex, 0, read.Token.StartIndex, null, null));
            return new TokenRead(token, new TokenCursor(cursor.SourceIdentity, read.Next.SourceOffset,
                read.Next.TokenIndex, atEof ? cursor.PreviousToken : token, read.Next.LexicalState));
        };

        var result = fixture.Parse();

        AssertContractFailure(fixture, result, atEof ? 1 : 0, atEof ? 1 : 0);
        Assert.Equal(atEof ? 1 : 0, Shifted(result).Length);
        Assert.DoesNotContain(result.Trace.Steps, step => step.Action is ParseAction.Accept);
    }

    [Fact]
    public void A_token_cannot_point_before_the_current_source_cursor()
    {
        var fixture = new Fixture();
        fixture.Source.Rewrite = (cursor, _, read) =>
        {
            if (cursor.TokenIndex == 0) return read;
            var token = Token(fixture.Y, 0, "y");
            return new TokenRead(token,
                new TokenCursor(cursor.SourceIdentity, read.Next.SourceOffset, read.Next.TokenIndex, token));
        };

        var result = fixture.Parse();

        AssertContractFailure(fixture, result, 1, 1);
        Assert.Equal(new[] { "x" }, Shifted(result).Select(token => token.Input));
    }

    [Theory]
    [InlineData("before-end")]
    [InlineData("consumes-ordinal")]
    [InlineData("changes-previous")]
    [InlineData("nonempty-token")]
    public void Eof_is_zero_width_at_the_source_end_and_preserves_ordinal_and_previous(string violation)
    {
        var fixture = new Fixture("S : 'x' ;", "x");
        fixture.Source.Rewrite = (cursor, _, read) =>
        {
            if (!(read.Token.Kind is EndMarker)) return read;
            var token = read.Token;
            var next = read.Next;
            switch (violation)
            {
                case "before-end":
                    token = Token(new EndMarker(), 0, "");
                    next = new TokenCursor(cursor.SourceIdentity, 0, cursor.TokenIndex, cursor.PreviousToken);
                    break;
                case "consumes-ordinal":
                    next = new TokenCursor(cursor.SourceIdentity, cursor.SourceOffset, cursor.TokenIndex + 1, cursor.PreviousToken);
                    break;
                case "changes-previous":
                    next = new TokenCursor(cursor.SourceIdentity, cursor.SourceOffset, cursor.TokenIndex, token);
                    break;
                case "nonempty-token": token = Token(new EndMarker(), cursor.SourceOffset, "x"); break;
                default: throw new InvalidOperationException("Unknown EOF contract violation.");
            }
            return new TokenRead(token, next);
        };

        var result = fixture.Parse();

        AssertContractFailure(fixture, result, 1, 1);
        Assert.Single(Shifted(result));
        Assert.DoesNotContain(result.Trace.Steps, step => step.Action is ParseAction.Accept);
    }

    [Fact]
    public void A_real_early_eof_is_rejected_even_if_its_cursor_could_advance_to_the_end()
    {
        var fixture = new Fixture();
        fixture.Source.Rewrite = (cursor, _, _) => new TokenRead(Token(new EndMarker(), 0, ""),
            new TokenCursor(cursor.SourceIdentity, fixture.Source.Length, 0));

        var result = fixture.Parse();

        AssertContractFailure(fixture, result, 0, 0);
        Assert.Empty(Shifted(result));
    }

    [Theory]
    [InlineData("physical-missing", 1, 1)]
    [InlineData("physical-too-long", 1, 1)]
    [InlineData("eof-missing", 3, 2)]
    public void Leading_trivia_must_cover_the_gap_from_the_current_cursor_to_the_token(
        string violation, int offset, int ordinal)
    {
        var fixture = new Fixture(input: "x y ");
        fixture.Source.Rewrite = (cursor, _, read) =>
        {
            bool atEof = read.Token.Kind is EndMarker;
            if (violation == "eof-missing" ? !atEof : read.Token.Input != "y") return read;
            string trivia = violation == "physical-too-long" ? "  " : "";
            var original = read.Token;
            var token = new TokenData(original.Kind, new TokenCell(original.StartIndex, 0,
                original.StartIndex, original.Input, null, trivia));
            var next = new TokenCursor(cursor.SourceIdentity, read.Next.SourceOffset, read.Next.TokenIndex,
                atEof ? cursor.PreviousToken : token);
            return new TokenRead(token, next);
        };

        var result = fixture.Parse();

        var error = AssertContractFailure(fixture, result, offset, ordinal);
        Assert.Contains("trivia", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(ordinal, Shifted(result).Length);
    }

    [Fact]
    public void A_custom_source_can_construct_physical_and_eof_tokens_with_public_trivia_information()
    {
        const string grammar = """
            S : 'x' 'y' ;
            comment := Comment "/\*[\s\S]*?\*/" ;
            """;
        var fixture = new Fixture(grammar, "  x\n y /* trailing */ ");
        fixture.Source.Rewrite = (cursor, _, read) =>
        {
            var original = read.Token;
            var token = new TokenData(original.Kind, new TokenCell(original.StartIndex,
                original.TokenCell.StartLineIndex, original.TokenCell.StartColumnIndex,
                original.Input, null, original.LeadingTrivia.Text));
            return new TokenRead(token, new TokenCursor(cursor.SourceIdentity, read.Next.SourceOffset,
                read.Next.TokenIndex, original.Kind is EndMarker ? cursor.PreviousToken : token));
        };

        var result = fixture.Parse();

        Assert.True(result.Success, fixture.Parser.FiredException?.ToString());
        Assert.Equal("  ", result[0].Token.LeadingTrivia.Text);
        Assert.Equal("\n ", result[1].Token.LeadingTrivia.Text);
        Assert.Equal(" /* trailing */ ", result.Last().Token.LeadingTrivia.Text);
        Assert.IsType<EndMarker>(result.Last().Token.Kind);
    }

    [Fact]
    public void A_terminal_outside_the_selected_goal_is_a_contract_error()
    {
        var fixture = new Fixture();
        var xOnly = new LexicalGoal(new[] { fixture.X });
        var source = new AlteredSource(fixture.Lexer.CreateTokenSource("x y", new LexicalPolicy(xOnly)));
        source.Rewrite = (cursor, _, read) =>
        {
            var token = Token(fixture.Y, 0, "x");
            return new TokenRead(token, new TokenCursor(cursor.SourceIdentity, 1, 1, token));
        };

        var result = fixture.Parser.Parsing(source);

        Assert.False(result.Success);
        var error = Assert.IsType<ContextualLexingException>(fixture.Parser.FiredException);
        Assert.Contains("outside", error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(0, error.SourceOffset);
        Assert.Empty(Shifted(result));
    }

    [Fact]
    public void An_undefined_source_character_remains_a_positioned_syntax_error()
    {
        var fixture = new Fixture("S : 'x' ;", "@");

        var result = fixture.Parse();

        Assert.False(result.Success);
        Assert.IsType<ParsingException>(fixture.Parser.FiredException);
        var error = Assert.Single(Assert.Single(result.AllErrors).ErrTokens);
        Assert.IsType<NotDefined>(error.Kind);
        Assert.Equal("@", error.Input);
        Assert.Equal(0, error.StartIndex);
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 3)]
    public void A_source_exception_retains_its_cause_and_the_actual_failed_request_position(
        int failAtOrdinal, int offset)
    {
        var fixture = new Fixture();
        var cause = new InvalidOperationException("scanner failed at this request");
        fixture.Source.Fail = cursor => cursor.TokenIndex == failAtOrdinal ? cause : null;

        var result = fixture.Parse();

        var error = AssertContractFailure(fixture, result, offset, failAtOrdinal);
        Assert.Same(cause, error.InnerException);
        Assert.Equal(failAtOrdinal, Shifted(result).Length);
    }

    [Fact]
    public void A_selector_failure_after_a_shift_does_not_blame_or_consume_the_next_source_character()
    {
        var fixture = new Fixture();
        var cause = new InvalidOperationException("mode selection failed");

        fixture.Source.Policy = new LexicalPolicy(new[] { fixture.Goal }, context =>
            context.TokenIndex == 1 ? throw cause : fixture.Goal);

        var result = fixture.Parse();

        var error = AssertContractFailure(fixture, result, 1, 1);
        Assert.Same(cause, error.InnerException);
        Assert.Equal(1, fixture.Source.Calls);
        Assert.Single(Shifted(result));
    }

    [Fact]
    public void The_source_policy_is_captured_once_and_selects_each_fresh_actual_read()
    {
        var fixture = new Fixture();
        var contexts = new List<LexicalContext>();
        fixture.Source.Policy = new LexicalPolicy(new[] { fixture.Goal }, context =>
        {
            contexts.Add(context);
            return fixture.Goal;
        });

        var result = fixture.Parse();

        Assert.True(result.Success, fixture.Parser.FiredException?.ToString());
        Assert.Equal(1, fixture.Source.PolicyReads);
        Assert.Equal(new[] { 0, 1, 2 }, contexts.Select(context => context.TokenIndex));
        Assert.Equal(contexts.Count, fixture.Source.Calls);
        Assert.All(contexts, context => Assert.Same(fixture.Goal, Assert.Single(context.Goals)));
    }

    [Fact]
    public void Actual_parsing_and_prediction_share_the_policy_owned_by_the_source()
    {
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => true);
        var fixture = new Fixture("S : 'x' {Allow()}? 'y' | 'x' 'y' 'z' ;", "x y", options);
        var contexts = new List<LexicalContext>();
        fixture.Source.Policy = new LexicalPolicy(new[] { fixture.Goal }, context =>
        {
            contexts.Add(context);
            return fixture.Goal;
        });

        var result = fixture.Parse();

        Assert.True(result.Success, fixture.Parser.FiredException?.ToString());
        Assert.Equal(1, fixture.Source.PolicyReads);
        // The actual input has x, y and EOF. Additional requests belong to prediction forks.
        Assert.True(contexts.Count > 3);
        Assert.Equal(contexts.Count, fixture.Source.Calls);
        Assert.All(contexts, context => Assert.Same(fixture.Goal, Assert.Single(context.Goals)));
        Assert.Equal(new[] { "x", "y" }, Shifted(result).Select(token => token.Input));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Actual_actions_cannot_shift_eof_or_accept_a_non_eof_token(bool shiftEof)
    {
        var fixture = new Fixture("S : 'x' ;", "x");
        var table = Assert.IsType<LRParsingTable>(fixture.Parser.ParsingTable);
        var accept = table.SelectMany(row => row.MatchedValueSet.Values).SelectMany(cell => cell)
            .First(action => action.Direction == ActionDir.Accept);
        if (shiftEof)
        {
            var row = Assert.Single(table, item => item.MatchedValueSet.TryGetValue(fixture.Eof, out var actions) &&
                actions.Any(action => action.Direction == ActionDir.Accept));
            row.MatchedValueSet[fixture.Eof] = Actions(new ActionData(ActionDir.Shift, 0));
        }
        else table[0].MatchedValueSet[fixture.X] = Actions(new ActionData(ActionDir.Accept, accept.Dest));

        var result = fixture.Parse();

        AssertContractFailure(fixture, result, shiftEof ? 1 : 0, shiftEof ? 1 : 0);
        Assert.DoesNotContain(result.Trace.Steps, step => step.Action is ParseAction.Accept);
        Assert.DoesNotContain(Shifted(result), token => token.Kind is EndMarker);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void An_invalid_actual_shift_or_goto_destination_reports_the_current_state_before_execution(bool atGoto)
    {
        var fixture = new Fixture("S : 'x' ;", "x");
        var table = Assert.IsType<LRParsingTable>(fixture.Parser.ParsingTable);
        if (atGoto)
        {
            var cell = Assert.Single(table[0].MatchedValueSet, item =>
                item.Key is NonTerminal && item.Value.Any(action => action.Direction == ActionDir.Goto));
            table[0].MatchedValueSet[cell.Key] = Actions(new ActionData(ActionDir.Goto, int.MaxValue));
        }
        else table[0].MatchedValueSet[fixture.X] = Actions(new ActionData(ActionDir.Shift, int.MaxValue));

        var result = fixture.Parse();

        var error = AssertContractFailure(fixture, result, atGoto ? 1 : 0, atGoto ? 1 : 0);
        Assert.Equal(0, error.State);
        Assert.Equal(atGoto ? 1 : 0, Shifted(result).Length);
        Assert.DoesNotContain(result.Trace.Steps, step => step.Action is ParseAction.Accept);
    }

    [Fact]
    public void The_goal_keeps_longer_operators_even_when_the_current_state_expects_shorter_ones()
    {
        var fixture = new Fixture("S : 'p' '=' '=' | 'q' '==' ;", "p ==");

        var result = fixture.Parse();

        Assert.False(result.Success);
        var error = Assert.Single(Assert.Single(result.AllErrors).ErrTokens);
        Assert.Equal("==", error.Input);
        Assert.Equal(2, error.StartIndex);
        Assert.Equal(new[] { "p" }, Shifted(result).Select(token => token.Input));
    }

    [Fact]
    public void Source_and_list_parses_can_reuse_one_parser_after_a_predicate_exception()
    {
        bool fail = true;
        var cause = new InvalidOperationException("condition failed");
        var observed = new List<PredicateContext>();
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", context =>
        {
            observed.Add(context);
            if (fail) throw cause;
            return true;
        });
        var fixture = new Fixture("S : 'x' {Allow()}? 'y' | 'x' 'y' 'z' ;", "x y", options);

        var first = fixture.Parse();
        Assert.False(first.Success);
        var error = Assert.IsType<GrammarPredicateEvaluationException>(fixture.Parser.FiredException);
        Assert.Same(cause, error.InnerException);

        fail = false;
        var legacy = fixture.Parser.Parsing(fixture.Lexer.Lexing("x y z").TokensForParsing);
        Assert.True(legacy.Success, fixture.Parser.FiredException?.ToString());
        Assert.Null(fixture.Parser.FiredException);
        var contextual = fixture.Parse();
        Assert.True(contextual.Success, fixture.Parser.FiredException?.ToString());
        Assert.Null(fixture.Parser.FiredException);
        Assert.All(observed, context =>
        {
            Assert.Equal("x", context.PreviousToken.Input);
            Assert.Equal("y", context.NextToken.Input);
            Assert.Equal(1, context.TokenIndex);
        });
        Assert.NotNull(contextual.ToParseTree);
    }

    [Fact]
    public void A_false_eof_predicate_preserves_trailing_trivia_and_allows_a_later_parse_of_the_same_source()
    {
        bool allowed = false;
        var contexts = new List<PredicateContext>();
        var options = new EbnfReaderOptions().RegisterPredicate("Finish", context =>
        {
            contexts.Add(context);
            return allowed;
        });
        const string grammar = """
            S : 'x' {Finish()}? ;
            comment := Comment "/\*[\s\S]*?\*/" ;
            """;
        const string trailing = " /* trailing\n comment */ ";
        var fixture = new Fixture(grammar, "x" + trailing, options);

        var rejected = fixture.Parse();

        Assert.False(rejected.Success);
        Assert.IsType<ParsingException>(fixture.Parser.FiredException);
        var errorToken = Assert.Single(Assert.Single(rejected.AllErrors).ErrTokens);
        Assert.IsType<EndMarker>(errorToken.Kind);
        Assert.Same(rejected.Last().Token, errorToken);
        Assert.Equal(fixture.Source.Length, errorToken.StartIndex);
        Assert.Equal(trailing, errorToken.LeadingTrivia.Text);
        Assert.Single(Shifted(rejected));
        Assert.DoesNotContain(rejected.Trace.Steps, step => step.Action is ParseAction.Accept);

        allowed = true;
        var accepted = fixture.Parse();

        Assert.True(accepted.Success, fixture.Parser.FiredException?.ToString());
        Assert.Null(fixture.Parser.FiredException);
        Assert.Empty(accepted.AllErrors);
        Assert.Single(accepted.Trace.Steps, step => step.Action is ParseAction.Accept);
        Assert.NotEmpty(contexts);
        Assert.All(contexts, context =>
        {
            Assert.Equal("x", context.PreviousToken.Input);
            Assert.Equal(1, context.TokenIndex);
            Assert.True(context.IsEndOfInput);
            Assert.Equal(trailing, context.NextToken.LeadingTrivia.Text);
        });
    }

    [Fact]
    public void History_snapshots_preserve_indexing_and_order_after_later_appends()
    {
        var terminal = new Terminal(TokenType.Operator, "x");
        var first = Token(terminal, 0, "first");
        var second = Token(terminal, 5, "second");
        var snapshot = TokenHistory.Empty.Append(first);
        var later = snapshot.Append(second);

        Assert.Empty(TokenHistory.Empty);
        Assert.Single(snapshot);
        Assert.Same(first, snapshot[0]);
        Assert.Equal(new[] { first }, snapshot.ToArray());
        Assert.Equal(new[] { first, second }, later.ToArray());
        Assert.Same(second, later[1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => later[-1]);
        Assert.Throws<ArgumentOutOfRangeException>(() => later[2]);
    }

    [Fact]
    public void Repeated_last_token_access_does_not_allocate_and_retain_every_prefix_array()
    {
        const int count = 4096;
        var token = Token(new Terminal(TokenType.Operator, "x"), 0, "x");
        var history = TokenHistory.Empty.Append(token);
        _ = history[0]; // Initialize the accessor before measuring its allocation behavior.
        long before = GC.GetAllocatedBytesForCurrentThread();
        TokenData last = null;
        for (int index = 0; index < count; index++)
        {
            history = history.Append(token);
            last = history[history.Count - 1];
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Same(token, last);
        Assert.Equal(count + 1, history.Count);
        Assert.Same(token, history[0]);
        // This generous per-node budget allows runtime overhead. A full array cached at
        // every prefix allocates over 60 MB for this input instead of linear storage.
        Assert.InRange(allocated, 0, count * 256L);
        GC.KeepAlive(history);
    }

    private static ContextualLexingException AssertContractFailure(Fixture fixture, ParsingResult result,
        int offset, int ordinal)
    {
        Assert.False(result.Success);
        var error = Assert.IsType<ContextualLexingException>(fixture.Parser.FiredException);
        Assert.Equal(offset, error.SourceOffset);
        Assert.Equal(ordinal, error.TokenIndex);
        Assert.Contains(result.AllErrors.SelectMany(item => item.ErrTokens), token => token.StartIndex == offset);
        return error;
    }

    private static TokenData[] Shifted(ParsingResult result)
        => result.Trace.Steps.Where(step => step.Action is ParseAction.Shift).Select(step => step.Lookahead).ToArray();

    private static TokenData Token(Terminal terminal, int offset, string text)
        => new TokenData(terminal, new TokenCell(offset, 0, offset, text, null));

    private static ActionDataList Actions(params ActionData[] actions)
    {
        var result = new ActionDataList();
        result.AddRange(actions);
        return result;
    }

    private sealed class Fixture
    {
        public EbnfGrammar Grammar { get; }
        public LALRParser Parser { get; }
        public Lexer Lexer { get; }
        public LexicalGoal Goal { get; }
        public AlteredSource Source { get; }
        public Terminal X => Grammar.TerminalSet.Single(token => token.Value == "x");
        public Terminal Y => Grammar.TerminalSet.Single(token => token.Value == "y");
        public Terminal Eof => Grammar.TerminalSet.Single(token => token is EndMarker);

        public Fixture(string grammarText = "S : 'x' 'y' ;", string input = "x y", EbnfReaderOptions options = null)
        {
            var read = EbnfGrammarReader.Read(grammarText, options);
            Assert.True(read.Success, string.Join("; ", read.Errors));
            Grammar = read.Grammar;
            Parser = new LALRParser(Grammar, true);
            Lexer = new Lexer();
            foreach (var terminal in Grammar.TerminalSet) Lexer.AddTokenRule(terminal);
            Goal = new LexicalGoal(Grammar.TerminalSet.Where(token => token is not EndMarker && token is not Epsilon));
            Source = new AlteredSource(Lexer.CreateTokenSource(input, new LexicalPolicy(Goal)));
        }

        public ParsingResult Parse() => Parser.Parsing(Source);
    }

    private sealed class AlteredSource : ITokenSource
    {
        private readonly ITokenSource inner;
        public TokenCursor Start { get; set; }
        public int Length { get; set; }
        private LexicalPolicy policy;
        public LexicalPolicy Policy
        {
            get { PolicyReads++; return policy; }
            set { policy = value; }
        }
        public int PolicyReads { get; private set; }
        public int Calls { get; private set; }
        public Func<TokenCursor, Exception> Fail { get; set; }
        public Func<TokenCursor, LexicalGoal, TokenRead, TokenRead> Rewrite { get; set; }

        public AlteredSource(ITokenSource inner)
        {
            this.inner = inner;
            Start = inner.Start;
            Length = inner.Length;
            policy = inner.Policy;
        }

        public TokenRead Peek(TokenCursor start, LexicalGoal goal)
        {
            Calls++;
            var error = Fail?.Invoke(start);
            if (error != null) throw error;
            var read = inner.Peek(start, goal);
            return Rewrite == null ? read : Rewrite(start, goal, read);
        }
    }
}
