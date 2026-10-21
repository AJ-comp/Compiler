using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Janglim;
using Janglim.FrontEnd;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// A lexical request is speculative until its Next cursor is adopted. Different goals can
/// partition the same source differently without sharing mutable tokens or consuming input.
/// </summary>
[Trait("Category", "Common")]
public class ContextualLexerTests
{
    [Fact]
    public void Different_goals_can_read_one_regex_literal_or_several_division_tokens()
    {
        var lexer = new Lexer();
        var identifier = new Terminal(TokenType.Identifier, "[a-z]+", "id", true, true);
        var slash = new Terminal(TokenType.Operator, "/", false);
        var regex = Raw("regex", "/[a-z]+/[a-z]*");
        foreach (var terminal in new[] { identifier, slash, regex }) lexer.AddTokenRule(terminal);
        var division = new LexicalGoal(new[] { identifier, slash });
        var expression = new LexicalGoal(new[] { regex });
        var policy = new LexicalPolicy(division, expression);
        var source = lexer.CreateTokenSource("/a/g", policy);

        var asRegex = source.Peek(source.Start, expression);
        var asDivision = source.Peek(source.Start, division);

        Assert.Same(regex, asRegex.Token.Kind);
        Assert.Equal("/a/g", asRegex.Token.Input);
        Assert.Equal(4, asRegex.Next.SourceOffset);
        Assert.Equal(1, asRegex.Next.TokenIndex);
        Assert.Same(slash, asDivision.Token.Kind);
        Assert.Equal("/", asDivision.Token.Input);
        Assert.Equal(1, asDivision.Next.SourceOffset);
        Assert.Equal(new[] { "/", "a", "/", "g" }, ReadAll(source, division).Select(token => token.Input));
        Assert.Equal(0, source.Start.SourceOffset);
        Assert.Equal(0, source.Start.TokenIndex);
        Assert.Null(source.Start.PreviousToken);
        Assert.Same(policy, source.Policy);
        Assert.Equal(new[] { division, expression }, source.Policy.Goals);
    }

    [Fact]
    public void Skipped_comments_and_delimiters_are_available_even_when_not_listed_in_the_goal()
    {
        var lexer = CreateLexer(out var id);
        var goal = new LexicalGoal(new[] { id });
        const string text = " /* head\n*/ item /* tail */\r\n";
        var source = lexer.CreateTokenSource(text, new LexicalPolicy(goal));
        var first = source.Peek(source.Start, goal);
        int start = text.IndexOf("item", StringComparison.Ordinal);

        Assert.Equal("item", first.Token.Input);
        Assert.Equal(start, first.Token.StartIndex);
        Assert.Equal(text.Substring(0, start), first.Token.LeadingTrivia.Text);
        Assert.Equal(0, first.Token.LeadingTrivia.StartIndex);
        Assert.True(first.Token.HasLineBreakBefore);
        Assert.Equal(1, first.Token.TokenCell.StartLineIndex);
        Assert.Same(first.Token, first.Next.PreviousToken);

        var eof = source.Peek(first.Next, goal);
        Assert.IsType<EndMarker>(eof.Token.Kind);
        Assert.Equal(text.Substring(start + 4), eof.Token.LeadingTrivia.Text);
        Assert.Equal(start + 4, eof.Token.LeadingTrivia.StartIndex);
        Assert.True(eof.Token.HasLineBreakBefore);
        Assert.Equal(text.Length, eof.Token.StartIndex);
        Assert.Equal(text.Length - 1, eof.Token.EndIndex);
        Assert.Equal(string.Empty, eof.Token.Input);
        Assert.Equal(text.Length, eof.Next.SourceOffset);
        Assert.Equal(1, eof.Next.TokenIndex);
        Assert.Same(first.Token, eof.Next.PreviousToken);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" \t")]
    [InlineData("/* only a comment */\r\n")]
    public void Eof_keeps_the_entire_trivia_and_does_not_add_a_consumed_token(string text)
    {
        var source = SourceUsingAllRules(CreateLexer(out _), text);

        var eof = source.Peek(source.Start, source.Policy.Goals[0]);

        Assert.IsType<EndMarker>(eof.Token.Kind);
        Assert.Equal(text, eof.Token.LeadingTrivia.Text);
        Assert.Equal(text.Length, eof.Token.StartIndex);
        Assert.Equal(text.Length - 1, eof.Token.EndIndex);
        Assert.Equal(text.Length, eof.Next.SourceOffset);
        Assert.Equal(0, eof.Next.TokenIndex);
        Assert.Null(eof.Next.PreviousToken);
        var again = source.Peek(eof.Next, source.Policy.Goals[0]);
        Assert.Equal(eof.Next, again.Next);
        Assert.IsType<EndMarker>(again.Token.Kind);
        Assert.Empty(again.Token.LeadingTrivia.Text);
    }

    [Fact]
    public void Scope_comment_delimiters_remain_syntactic_tokens()
    {
        var lexer = CreateLexer(out _, comments: false);
        var open = new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "/*", false);
        var close = new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "*/", false);
        lexer.AddTokenRule(open);
        lexer.AddTokenRule(close);
        var source = SourceUsingAllRules(lexer, "a /*\nb */ c");

        var tokens = ReadAll(source, source.Policy.Goals[0]);

        Assert.Equal(new[] { "a", "/*", "b", "*/", "c" }, tokens.Select(token => token.Input));
        Assert.Same(open, tokens[1].Kind);
        Assert.Same(close, tokens[3].Kind);
        Assert.True(tokens[2].HasLineBreakBefore);
    }

    [Fact]
    public void A_scope_comment_is_not_silently_skipped_when_the_goal_excludes_it()
    {
        var lexer = CreateLexer(out var id, comments: false);
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment.ScopeComment, "/*", false));
        var goal = new LexicalGoal(new[] { id });
        var source = lexer.CreateTokenSource("/*a", new LexicalPolicy(goal));

        var read = source.Peek(source.Start, goal);

        Assert.IsType<NotDefined>(read.Token.Kind);
        Assert.Equal("/", read.Token.Input);
        Assert.Empty(read.Token.LeadingTrivia.Text);
    }

    [Fact]
    public void A_goal_with_all_rules_selects_the_longest_match_like_whole_input_lexing()
    {
        var lexer = new Lexer();
        var first = Raw("first", "a(?:)");
        var longer = Raw("longer", "ab");
        lexer.AddTokenRule(first);
        lexer.AddTokenRule(longer);
        var source = SourceUsingAllRules(lexer, "ab");

        var read = source.Peek(source.Start, source.Policy.Goals[0]);

        Assert.Same(longer, read.Token.Kind);
        Assert.Equal("ab", read.Token.Input);
        Assert.Equal(lexer.Lexing("ab").TokensForParsing.Select(token => token.Data),
            ReadAll(source, source.Policy.Goals[0]).Select(token => token.Input));
    }

    [Theory]
    [InlineData("a + b")]
    [InlineData(" /* heading */ a\r\n+b /* ending */")]
    public void A_goal_with_all_rules_preserves_existing_token_kinds_positions_and_trivia(string text)
    {
        var lexer = CreateLexer(out _);
        lexer.AddTokenRule(new Terminal(TokenType.Operator, "+", false));
        var legacy = lexer.Lexing(text);
        var source = SourceUsingAllRules(lexer, text);

        var actual = ReadAll(source, source.Policy.Goals[0]);

        Assert.Equal(legacy.TokensForParsing.Count, actual.Count);
        for (int i = 0; i < actual.Count; i++)
        {
            Assert.Same(legacy.TokensForParsing[i].PatternInfo.Terminal, actual[i].Kind);
            Assert.Equal(legacy.TokensForParsing[i].Data, actual[i].Input);
            Assert.Equal(legacy.TokensForParsing[i].StartIndex, actual[i].StartIndex);
            Assert.Equal(legacy.TokensForParsing[i].LeadingTrivia.Text, actual[i].LeadingTrivia.Text);
        }
    }

    [Fact]
    public void A_request_does_not_search_ahead_past_unrecognized_input()
    {
        var lexer = new Lexer();
        lexer.AddTokenRule(Raw("word", "abc"));
        var source = SourceUsingAllRules(lexer, "?abc");

        var unknown = source.Peek(source.Start, source.Policy.Goals[0]);
        var word = source.Peek(unknown.Next, source.Policy.Goals[0]);

        Assert.IsType<NotDefined>(unknown.Token.Kind);
        Assert.Equal("?", unknown.Token.Input);
        Assert.Equal(0, unknown.Token.StartIndex);
        Assert.Equal(1, unknown.Next.SourceOffset);
        Assert.Equal("abc", word.Token.Input);
        Assert.Equal(1, word.Token.StartIndex);
    }

    [Theory]
    [InlineData(@"\bfoo", "xfoo", 1, "f", false)]
    [InlineData("^foo", "xfoo", 1, "f", false)]
    [InlineData("^foo", "x\nfoo", 2, "foo", true)]
    [InlineData("(?<=x)foo", "xfoo", 1, "foo", true)]
    public void Word_boundaries_anchors_and_lookbehind_use_the_whole_source(
        string pattern, string text, int offset, string expected, bool matched)
    {
        var lexer = new Lexer();
        var terminal = Raw("word", pattern);
        lexer.AddTokenRule(terminal);
        var source = SourceUsingAllRules(lexer, text);
        var cursor = new TokenCursor(source.Start.SourceIdentity, offset);

        var read = source.Peek(cursor, source.Policy.Goals[0]);

        Assert.Equal(expected, read.Token.Input);
        Assert.Equal(offset, read.Token.StartIndex);
        if (matched) Assert.Same(terminal, read.Token.Kind);
        else Assert.IsType<NotDefined>(read.Token.Kind);
    }

    [Fact]
    public void Unknown_input_always_consumes_one_utf16_code_unit()
    {
        const string text = "\U0001F600";
        var source = SourceUsingAllRules(new Lexer(), text);
        var first = source.Peek(source.Start, source.Policy.Goals[0]);
        var second = source.Peek(first.Next, source.Policy.Goals[0]);
        var eof = source.Peek(second.Next, source.Policy.Goals[0]);

        Assert.IsType<NotDefined>(first.Token.Kind);
        Assert.IsType<NotDefined>(second.Token.Kind);
        Assert.Equal(1, first.Token.Input.Length);
        Assert.Equal(1, second.Token.Input.Length);
        Assert.Equal(1, first.Next.SourceOffset);
        Assert.Equal(2, second.Next.SourceOffset);
        Assert.Equal(text, first.Token.Input + second.Token.Input);
        Assert.IsType<EndMarker>(eof.Token.Kind);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Zero_width_matches_are_rejected_even_after_trivia(bool comment)
    {
        var lexer = new Lexer();
        var terminal = comment
            ? new Terminal(TokenType.SpecialToken.Comment, "(?=x)", "empty", false, true)
            : Raw("empty", "(?=x)");
        lexer.AddTokenRule(terminal);
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Delimiter, " ", false));
        var source = SourceUsingAllRules(lexer, " x");

        var error = Assert.Throws<EmptyTokenMatchException>(() => source.Peek(source.Start, source.Policy.Goals[0]));

        Assert.Contains("matched no input", error.Message);
        Assert.Contains("offset 1", error.Message);
        Assert.Equal(0, source.Start.SourceOffset);
    }

    [Fact]
    public void A_zero_width_rule_in_another_goal_is_not_evaluated_for_the_selected_goal()
    {
        var lexer = new Lexer();
        var token = Raw("x", "x");
        var zeroWidth = Raw("empty", "(?=x)");
        lexer.AddTokenRule(token);
        lexer.AddTokenRule(zeroWidth);
        var tokenGoal = new LexicalGoal(new[] { token });
        var zeroWidthGoal = new LexicalGoal(new[] { zeroWidth });
        var source = lexer.CreateTokenSource("x", new LexicalPolicy(tokenGoal, zeroWidthGoal));

        var read = source.Peek(source.Start, tokenGoal);

        Assert.Same(token, read.Token.Kind);
        Assert.Equal("x", read.Token.Input);
        Assert.Equal(1, read.Next.SourceOffset);
        Assert.Throws<EmptyTokenMatchException>(() => source.Peek(source.Start, zeroWidthGoal));
        Assert.Equal(0, source.Start.SourceOffset);
    }

    [Fact]
    public void Regex_timeout_is_reported_instead_of_falling_back_to_another_rule()
    {
        var lexer = new Lexer();
        // A plain (a+)+$ can be optimized into a cheap failed match by newer regex
        // engines. A lookahead containing overlapping alternatives and a trailing a
        // forces the ambiguous search, while the outer token still begins with a.
        lexer.AddTokenRule(Raw("expensive", "(?=(a|aa)+a$)a"));
        lexer.AddTokenRule(Raw("fallback", "a"));
        var source = SourceUsingAllRules(lexer, new string('a', 128) + "!");

        Assert.Throws<RegexMatchTimeoutException>(() => source.Peek(source.Start, source.Policy.Goals[0]));
        Assert.Equal(0, source.Start.SourceOffset);
    }

    [Fact]
    public void Token_source_rules_are_a_snapshot_of_the_lexer()
    {
        var lexer = new Lexer();
        lexer.AddTokenRule(Raw("a", "a"));
        var before = SourceUsingAllRules(lexer, "ab");
        lexer.AddTokenRule(Raw("ab", "ab"));
        var after = SourceUsingAllRules(lexer, "ab");

        Assert.Equal("a", before.Peek(before.Start, before.Policy.Goals[0]).Token.Input);
        Assert.Equal("ab", after.Peek(after.Start, after.Policy.Goals[0]).Token.Input);
    }

    [Fact]
    public void Adding_a_common_comment_rule_does_not_change_an_existing_source_with_the_same_policy()
    {
        var lexer = CreateLexer(out var identifier, comments: false);
        var goal = new LexicalGoal(new[] { identifier });
        var policy = new LexicalPolicy(goal);
        const string input = "/*c*/a";
        var before = lexer.CreateTokenSource(input, policy);
        lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment,
            @"/\*[\s\S]*?\*/", "comment", false, true));
        var after = lexer.CreateTokenSource(input, policy);

        var earlierRead = before.Peek(before.Start, goal);
        var laterRead = after.Peek(after.Start, goal);

        Assert.Same(policy, before.Policy);
        Assert.Same(policy, after.Policy);
        Assert.IsType<NotDefined>(earlierRead.Token.Kind);
        Assert.Equal("/", earlierRead.Token.Input);
        Assert.Empty(earlierRead.Token.LeadingTrivia.Text);
        Assert.Equal(1, earlierRead.Next.SourceOffset);
        Assert.Same(identifier, laterRead.Token.Kind);
        Assert.Equal("a", laterRead.Token.Input);
        Assert.Equal("/*c*/", laterRead.Token.LeadingTrivia.Text);
        Assert.Equal(input.Length, laterRead.Next.SourceOffset);
        Assert.IsType<NotDefined>(before.Peek(before.Start, goal).Token.Kind);
    }

    [Fact]
    public void A_policy_and_its_goals_keep_defensive_snapshots_through_source_creation()
    {
        var lexer = new Lexer();
        var a = Raw("a", "a");
        var b = Raw("b", "b");
        lexer.AddTokenRule(a);
        lexer.AddTokenRule(b);
        var terminals = new[] { a };
        var goal = new LexicalGoal(terminals);
        var goals = new[] { goal };
        var policy = new LexicalPolicy(goals);
        var source = lexer.CreateTokenSource("a", policy);
        terminals[0] = b;
        goals[0] = new LexicalGoal(new[] { b });

        Assert.Same(a, Assert.Single(goal.Terminals));
        Assert.Same(policy, source.Policy);
        Assert.Same(goal, Assert.Single(source.Policy.Goals));
        Assert.Same(a, source.Peek(source.Start, goal).Token.Kind);
        Assert.Throws<NotSupportedException>(() => ((IList<Terminal>)goal.Terminals)[0] = b);
        Assert.Throws<NotSupportedException>(() => ((IList<LexicalGoal>)source.Policy.Goals)[0] = goals[0]);
    }

    [Fact]
    public void Repeated_peeks_do_not_share_mutable_token_data_or_cells()
    {
        var source = SourceUsingAllRules(CreateLexer(out _), "a");
        var first = source.Peek(source.Start, source.Policy.Goals[0]);
        first.Token.IsNotUsed = true;
        first.Token.TokenCell.ValueOptionData = 42;
        first.Token.TokenCell.RefOptionalData = new object();

        var second = source.Peek(source.Start, source.Policy.Goals[0]);

        Assert.NotSame(first.Token, second.Token);
        Assert.NotSame(first.Token.TokenCell, second.Token.TokenCell);
        Assert.False(second.Token.IsNotUsed);
        Assert.Equal(0, second.Token.TokenCell.ValueOptionData);
        Assert.Null(second.Token.TokenCell.RefOptionalData);
        Assert.Same(second.Token, second.Next.PreviousToken);
    }

    [Fact]
    public void Goals_with_different_terminals_produce_different_tokenizations()
    {
        var lexer = new Lexer();
        var shortToken = Raw("short", "a");
        var longToken = Raw("long", "ab");
        lexer.AddTokenRule(shortToken);
        lexer.AddTokenRule(longToken);
        var shortGoal = new LexicalGoal(new[] { shortToken });
        var longGoal = new LexicalGoal(new[] { longToken });
        var source = lexer.CreateTokenSource("ab", new LexicalPolicy(shortGoal, longGoal));

        Assert.Equal("a", source.Peek(source.Start, shortGoal).Token.Input);
        Assert.Equal("ab", source.Peek(source.Start, longGoal).Token.Input);
    }

    [Fact]
    public void Foreign_cursors_unregistered_goals_and_out_of_range_offsets_are_rejected()
    {
        var lexer = CreateLexer(out var id);
        var goal = new LexicalGoal(new[] { id });
        var policy = new LexicalPolicy(goal);
        var first = lexer.CreateTokenSource("a", policy);
        var second = lexer.CreateTokenSource("a", policy);
        var foreignGoal = new LexicalGoal(new[] { id });

        Assert.Throws<ArgumentException>(() => first.Peek(second.Start, goal));
        Assert.Throws<ArgumentException>(() => first.Peek(first.Start, foreignGoal));
        Assert.Throws<ArgumentOutOfRangeException>(() => first.Peek(
            new TokenCursor(first.Start.SourceIdentity, first.Length + 1), goal));
        Assert.Throws<ArgumentNullException>(() => first.Peek(null, goal));
        Assert.Throws<ArgumentNullException>(() => first.Peek(first.Start, null));
    }

    [Fact]
    public void Unallocated_symbol_keys_do_not_make_an_unregistered_terminal_valid()
    {
        var lexer = new Lexer();
        var registered = Raw("registered", "a");
        var foreign = Raw("foreign", "a");
        lexer.AddTokenRule(registered);
        Assert.Equal(registered.UniqueKey, foreign.UniqueKey);

        Assert.Throws<ArgumentException>(() => lexer.CreateTokenSource("a",
            new LexicalPolicy(new LexicalGoal(new[] { foreign }))));
    }

    [Fact]
    public void Input_control_symbols_are_synthetic_and_not_lexical_characters()
    {
        var lexer = new Lexer();
        var marker = new EndMarker();
        var epsilon = new Epsilon();
        lexer.AddTokenRule(marker);
        lexer.AddTokenRule(epsilon);
        var source = SourceUsingAllRules(lexer, marker.Value + epsilon.Value);

        var tokens = ReadAll(source, source.Policy.Goals[0]);

        Assert.Equal(new[] { marker.Value, epsilon.Value }, tokens.Select(token => token.Input));
        Assert.All(tokens, token => Assert.IsType<NotDefined>(token.Kind));
    }

    [Fact]
    public void Cursor_context_is_preserved_and_eof_does_not_replace_the_previous_token()
    {
        var source = SourceUsingAllRules(CreateLexer(out _), "a");
        var cursor = new TokenCursor(source.Start.SourceIdentity, 0, 7, lexicalState: "mode:2");

        var read = source.Peek(cursor, source.Policy.Goals[0]);
        var eof = source.Peek(read.Next, source.Policy.Goals[0]);

        Assert.Equal(8, read.Next.TokenIndex);
        Assert.Equal("mode:2", read.Next.LexicalState);
        Assert.Same(read.Token, read.Next.PreviousToken);
        Assert.Equal(7, cursor.TokenIndex);
        Assert.Equal(8, eof.Next.TokenIndex);
        Assert.Same(read.Token, eof.Next.PreviousToken);
        Assert.Equal("mode:2", eof.Next.LexicalState);
    }

    [Fact]
    public void Token_start_positions_follow_crlf_cr_and_unicode_line_breaks()
    {
        const string text = "a\r\n  b\rc\u2028d\u2029e\nf";
        var source = SourceUsingAllRules(CreateLexer(out _), text);

        var tokens = ReadAll(source, source.Policy.Goals[0]);

        Assert.Equal(new[] { 0, 1, 2, 3, 4, 5 }, tokens.Select(token => token.TokenCell.StartLineIndex));
        Assert.Equal(new[] { 0, 2, 0, 0, 0, 0 }, tokens.Select(token => token.TokenCell.StartColumnIndex));
        Assert.All(tokens.Skip(1), token => Assert.True(token.HasLineBreakBefore));
    }

    [Fact]
    public void Cursor_equality_keeps_owner_history_and_lexical_state_distinct()
    {
        var owner = new object();
        var terminal = Raw("id", "a");
        var previous = new TokenData(terminal, new TokenCell(0, 0, 0, "a", null));
        var sameValueDifferentToken = new TokenData(terminal, new TokenCell(0, 0, 0, "a", null));
        var first = new TokenCursor(owner, 1, 1, previous, "state");
        var same = new TokenCursor(owner, 1, 1, previous, "state");

        Assert.Equal(first, same);
        Assert.Equal(first.GetHashCode(), same.GetHashCode());
        Assert.NotEqual(first, new TokenCursor(new object(), 1, 1, previous, "state"));
        Assert.NotEqual(first, new TokenCursor(owner, 2, 1, previous, "state"));
        Assert.NotEqual(first, new TokenCursor(owner, 1, 1, sameValueDifferentToken, "state"));
        Assert.NotEqual(first, new TokenCursor(owner, 1, 2, previous, "state"));
        Assert.NotEqual(first, new TokenCursor(owner, 1, 1, previous, "other"));
    }

    [Fact]
    public void Invalid_public_contract_arguments_are_rejected()
    {
        var lexer = new Lexer();
        var policy = new LexicalPolicy(new LexicalGoal(Array.Empty<Terminal>()));
        Assert.Throws<ArgumentNullException>(() => lexer.CreateTokenSource(null, policy));
        Assert.Throws<ArgumentNullException>(() => lexer.CreateTokenSource("", null));
        Assert.Throws<ArgumentNullException>(() => new LexicalGoal(null));
        Assert.Throws<ArgumentException>(() => new LexicalGoal(new Terminal[] { null }));
        Assert.Throws<ArgumentNullException>(() => new TokenCursor(null, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TokenCursor(new object(), -1));
        Assert.Throws<ArgumentOutOfRangeException>(() => new TokenCursor(new object(), 0, -1));
        Assert.Throws<ArgumentNullException>(() => new TokenRead(null, new TokenCursor(new object(), 0)));
    }

    private static Terminal Raw(string name, string pattern)
        => new Terminal(TokenType.Literal.StringLiteral, pattern, name, true, true);

    private static ITokenSource SourceUsingAllRules(Lexer lexer, string input)
    {
        var goal = new LexicalGoal(lexer.TokenPatternList.Select(pattern => pattern.Terminal));
        var policy = new LexicalPolicy(goal);
        return lexer.CreateTokenSource(input, policy);
    }

    private static Lexer CreateLexer(out Terminal identifier, bool comments = true)
    {
        var lexer = new Lexer();
        identifier = new Terminal(TokenType.Identifier, "[a-z]+", "id", true, true);
        lexer.AddTokenRule(identifier);
        foreach (char delimiter in new[] { ' ', '\t', '\r', '\n', '\u2028', '\u2029' })
            lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Delimiter, delimiter.ToString(), false));
        if (comments)
            lexer.AddTokenRule(new Terminal(TokenType.SpecialToken.Comment,
                @"/\*[\s\S]*?\*/|//[^\r\n\u2028\u2029]*", "comment", false, true));
        return lexer;
    }

    private static List<TokenData> ReadAll(ITokenSource source, LexicalGoal goal)
    {
        var result = new List<TokenData>();
        var cursor = source.Start;
        for (int i = 0; i <= source.Length; i++)
        {
            var read = source.Peek(cursor, goal);
            if (read.Token.Kind is EndMarker) return result;
            Assert.True(read.Next.SourceOffset > cursor.SourceOffset);
            result.Add(read.Token);
            cursor = read.Next;
        }
        throw new InvalidOperationException("The token source failed to reach EOF.");
    }
}
