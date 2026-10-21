using System;
using System.Collections.Generic;
using System.Linq;
using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Exercises the EBNF, source lexer and LR parser together. The small regex pattern is a fixture
/// for contextual token boundaries; the grammar deliberately models only the syntax needed here.
/// </summary>
[Trait("Category", "ContextualLexing")]
public class ParserDrivenLexingTests
{
    private const string Tokens = """
        id := Identifier "[a-z]+" ;
        num := NumberLiteral "[0-9]+" ;
        rx := StringLiteral "/[a-z]+/[a-z]*" ;
        comment := Comment "/\*[\s\S]*?\*/" ;
        """;

    private const string Expressions = """
        S : id '=' Expr ';' ;
        Expr : Expr '+' Term | Term ;
        Term : Term '/' Atom | Atom ;
        Atom : id | num | rx ;
        """;

    [Theory]
    [InlineData("a=b/c;", "a = b / c ;")]
    [InlineData("a=b+c;", "a = b + c ;")]
    [InlineData("a=b;", "a = b ;")]
    [InlineData("a=/ab/g;", "a = /ab/g ;")]
    [InlineData("a=x;", "a = x ;")]
    [InlineData("a=2;", "a = 2 ;")]
    [InlineData("a=/ab/g+c;", "a = /ab/g + c ;")]
    [InlineData("a=b\n/c/g;", "a = b / c / g ;")]
    public void Automatic_goals_read_common_tokens_and_distinguish_division_from_regex(
        string input, string expectedTokens)
    {
        var fixture = new Fixture(Expressions + Tokens);
        var source = fixture.Source(input);

        var result = fixture.Parser.Parsing(source);

        AssertSuccess(fixture.Parser, result);
        Assert.Same(fixture.Policy, source.Policy);
        Assert.Equal(expectedTokens.Split(' '), Shifted(result).Select(token => token.Input));
        Assert.Equal(input.Contains("/ab/g", StringComparison.Ordinal),
            Shifted(result).Any(token => token.Kind == fixture.RegexTerminal));
    }

    [Fact]
    public void An_explicit_goal_also_reads_common_tokens_that_the_current_row_does_not_expect()
    {
        var grammar = Read("S : id ; id := Identifier \"[a-z]+\" ; plus := StringLiteral \"\\+\" ;");
        var plus = Assert.Single(grammar.TerminalSet, terminal => terminal.Caption == "plus");
        var goal = new LexicalGoal(Scannable(grammar));
        var policy = new LexicalPolicy(new[] { goal }, _ => goal);
        var source = Source(grammar, "+", policy);
        var parser = new LALRParser(grammar, true);

        var result = parser.Parsing(source);

        Assert.False(result.Success);
        var errorToken = Assert.Single(Assert.Single(result.AllErrors).ErrTokens);
        Assert.Same(plus, errorToken.Kind);
        Assert.Equal("+", errorToken.Input);
        Assert.Equal(0, errorToken.StartIndex);
    }

    [Fact]
    public void Reductions_and_gotos_keep_the_same_peek_and_only_shift_commits_the_next_cursor()
    {
        var grammar = Read("S : A ';' ; A : B ; B : id ; id := Identifier \"[a-z]+\" ;");
        var goal = new LexicalGoal(Scannable(grammar));
        var parser = new LALRParser(grammar, true);
        var source = new RecordingSource(Source(grammar, "  value ;  ", new LexicalPolicy(goal)));

        var result = parser.Parsing(source);

        AssertSuccess(parser, result);
        var shifted = Shifted(result);
        Assert.Equal(new[] { "value", ";" }, shifted.Select(token => token.Input));
        Assert.Equal(3, source.Reads.Count); // Each physical token and EOF is requested once.
        for (int index = 0; index < source.Reads.Count; index++)
        {
            var read = source.Reads[index];
            Assert.Equal(index, read.Cursor.TokenIndex);
            if (index > 0)
                Assert.Equal(source.Reads[index - 1].Read.Next.SourceOffset, read.Cursor.SourceOffset);
        }

        var semicolon = source.Reads[1].Read.Token;
        var heldSteps = result.Trace.Steps.Where(step => step.Lookahead?.Input == ";").ToArray();
        Assert.Contains(heldSteps, step => step.Action is ParseAction.Reduce);
        Assert.Contains(heldSteps, step => step.Action is ParseAction.Goto);
        Assert.IsType<ParseAction.Shift>(heldSteps.Last().Action);
        Assert.All(heldSteps, step => Assert.Same(semicolon, step.Lookahead));
        Assert.IsType<EndMarker>(source.Reads.Last().Read.Token.Kind);
        Assert.DoesNotContain(shifted, token => token.Kind is EndMarker);
    }

    [Theory]
    [InlineData(" /* trailing */ ", false)]
    [InlineData(" /* trailing\n comment */\t", true)]
    [InlineData("\r\n", true)]
    public void Beginning_and_eof_predicates_receive_exact_tokens_and_trailing_trivia(
        string trailing, bool lineBreak)
    {
        var observations = new List<(string Name, PredicateContext Context)>();
        var options = new EbnfReaderOptions()
            .RegisterPredicate("Begin", context => { observations.Add(("begin", context)); return true; })
            .RegisterPredicate("Finish", context => { observations.Add(("finish", context)); return true; });
        var grammar = Read("S : {Begin()}? 'x' {Finish()}? ; " + Tokens, options);
        var goal = new LexicalGoal(Scannable(grammar));
        var source = Source(grammar, "  x" + trailing, new LexicalPolicy(goal));
        var parser = new LALRParser(grammar, true);

        var result = parser.Parsing(source);

        AssertSuccess(parser, result);
        Assert.Contains(observations, item => item.Name == "begin");
        Assert.All(observations.Where(item => item.Name == "begin"), item =>
        {
            Assert.Null(item.Context.PreviousToken);
            Assert.Equal(0, item.Context.TokenIndex);
            Assert.Equal("x", item.Context.NextToken.Input);
            Assert.Equal("  ", item.Context.NextToken.LeadingTrivia.Text);
        });
        Assert.Contains(observations, item => item.Name == "finish");
        Assert.All(observations.Where(item => item.Name == "finish"), item =>
        {
            Assert.Equal("x", item.Context.PreviousToken.Input);
            Assert.Equal(1, item.Context.TokenIndex);
            Assert.True(item.Context.IsEndOfInput);
            Assert.IsType<EndMarker>(item.Context.NextToken.Kind);
            Assert.Equal(trailing, item.Context.NextToken.LeadingTrivia.Text);
            Assert.Equal(3, item.Context.NextToken.LeadingTrivia.StartIndex);
            Assert.Equal(lineBreak, item.Context.NextToken.HasLineBreakBefore);
            Assert.Equal(source.Length, item.Context.NextToken.StartIndex);
        });
        Assert.Single(Shifted(result));
        Assert.Single(result.Trace.Steps, step => step.Action is ParseAction.Accept);
    }

    [Theory]
    [InlineData("")]
    [InlineData("  /* only trivia\n */ ")]
    public void A_nullable_start_rule_can_accept_eof_without_shifting_a_physical_token(string input)
    {
        var seen = new List<PredicateContext>();
        var options = new EbnfReaderOptions().RegisterPredicate("End", context =>
        {
            seen.Add(context);
            return context.IsEndOfInput;
        });
        var grammar = Read("S : {End()}? ; " + Tokens, options);
        var goal = new LexicalGoal(Scannable(grammar));
        var parser = new LALRParser(grammar, true);

        var result = parser.Parsing(Source(grammar, input, new LexicalPolicy(goal)));

        AssertSuccess(parser, result);
        Assert.Empty(Shifted(result));
        Assert.NotEmpty(seen);
        Assert.All(seen, context =>
        {
            Assert.Null(context.PreviousToken);
            Assert.Equal(0, context.TokenIndex);
            Assert.True(context.IsEndOfInput);
            Assert.Equal(input, context.NextToken.LeadingTrivia.Text);
        });
        Assert.Single(result.Trace.Steps, step => step.Action is ParseAction.Accept);
    }

    [Theory]
    [InlineData("throw value;", true)]
    [InlineData("throw /* comment */ value;", true)]
    [InlineData("throw\nvalue;", false)]
    [InlineData("throw/* comment\n */value;", false)]
    public void A_predicate_uses_the_next_requested_tokens_preserved_source_gap(string input, bool success)
    {
        var seen = new List<PredicateContext>();
        var options = new EbnfReaderOptions().RegisterPredicate("NoLineBreak", context =>
        {
            seen.Add(context);
            return !context.NextToken.HasLineBreakBefore;
        });
        var grammar = Read("S : 'throw' {NoLineBreak()}? id ';' ; " + Tokens, options);
        var goal = new LexicalGoal(Scannable(grammar));
        var parser = new LALRParser(grammar, true);

        var result = parser.Parsing(Source(grammar, input, new LexicalPolicy(goal)));

        Assert.Equal(success, result.Success);
        Assert.NotEmpty(seen);
        Assert.All(seen, context =>
        {
            Assert.Equal("throw", context.PreviousToken.Input);
            Assert.Equal("value", context.NextToken.Input);
            Assert.Equal(1, context.TokenIndex);
            Assert.Equal(input.Substring(5, input.IndexOf("value", StringComparison.Ordinal) - 5),
                context.NextToken.LeadingTrivia.Text);
        });
        if (!success)
            Assert.DoesNotContain(Shifted(result), token => token.Input == "value");
    }

    [Fact]
    public void An_unresolved_goal_reports_its_position_before_requesting_a_token()
    {
        var fixture = new Fixture("S : rx ';' | '/' id '/' id ';' ; " + Tokens);
        var source = new RecordingSource(fixture.Source("/a/g;"));

        var result = fixture.Parser.Parsing(source);

        Assert.False(result.Success);
        var error = Assert.IsType<ContextualLexingException>(fixture.Parser.FiredException);
        Assert.Equal(0, error.SourceOffset);
        Assert.Equal(0, error.TokenIndex);
        Assert.Empty(source.Reads);
        Assert.NotEmpty(result.AllErrors);
    }

    [Theory]
    [InlineData(true, "/a/g ;")]
    [InlineData(false, "/ a / g ;")]
    public void An_explicit_goal_resolves_an_intentionally_ambiguous_lexical_position(
        bool regex, string expected)
    {
        var fixture = new Fixture("S : rx ';' | '/' id '/' id ';' ; " + Tokens);
        var selected = regex ? fixture.RegExp : fixture.Div;
        var policy = new LexicalPolicy(new[] { fixture.Div, fixture.RegExp }, _ => selected);
        var source = fixture.Source("/a/g;", policy);

        var result = fixture.Parser.Parsing(source);

        AssertSuccess(fixture.Parser, result);
        Assert.Same(policy, source.Policy);
        Assert.Equal(expected.Split(' '), Shifted(result).Select(token => token.Input));
    }

    [Fact]
    public void A_reused_parser_uses_each_sources_own_policy_for_the_same_input()
    {
        var fixture = new Fixture("S : rx ';' | '/' id '/' id ';' ; " + Tokens);
        var goals = new[] { fixture.Div, fixture.RegExp };
        int regexSelections = 0;
        int divisionSelections = 0;
        var regexPolicy = new LexicalPolicy(goals, _ =>
        {
            regexSelections++;
            return fixture.RegExp;
        });
        var divisionPolicy = new LexicalPolicy(goals, _ =>
        {
            divisionSelections++;
            return fixture.Div;
        });
        var regexSource = fixture.Source("/a/g;", regexPolicy);
        var divisionSource = fixture.Source("/a/g;", divisionPolicy);

        var regexResult = fixture.Parser.Parsing(regexSource);
        AssertSuccess(fixture.Parser, regexResult);
        int regexSelectionsAfterFirstParse = regexSelections;
        var divisionResult = fixture.Parser.Parsing(divisionSource);

        AssertSuccess(fixture.Parser, divisionResult);
        Assert.Same(regexPolicy, regexSource.Policy);
        Assert.Same(divisionPolicy, divisionSource.Policy);
        Assert.Equal(new[] { "/a/g", ";" }, Shifted(regexResult).Select(token => token.Input));
        Assert.Equal(new[] { "/", "a", "/", "g", ";" }, Shifted(divisionResult).Select(token => token.Input));
        Assert.True(regexSelectionsAfterFirstParse > 0);
        Assert.True(divisionSelections > 0);
        Assert.Equal(regexSelectionsAfterFirstParse, regexSelections);
    }

    [Fact]
    public void An_unregistered_goal_is_diagnosed_before_peek_and_does_not_poison_parser_reuse()
    {
        var fixture = new Fixture(Expressions + Tokens);
        var unregistered = new LexicalGoal(fixture.Div.Terminals);
        var policy = new LexicalPolicy(new[] { fixture.Div, fixture.RegExp }, _ => unregistered);
        var source = new RecordingSource(fixture.Source("a=b;", policy));

        var failed = fixture.Parser.Parsing(source);

        Assert.False(failed.Success);
        var error = Assert.IsType<ContextualLexingException>(fixture.Parser.FiredException);
        Assert.Equal(0, error.SourceOffset);
        Assert.Equal(0, error.TokenIndex);
        Assert.Empty(source.Reads);
        AssertSuccess(fixture.Parser,
            fixture.Parser.Parsing(fixture.Source("a=/ab/g;")));
    }

    [Theory]
    [InlineData("if(x) /a/.test(y);", "/a/")]
    [InlineData("f(x) / 2;", "/")]
    [InlineData("if(f(x)) /a/.test(y);", "/a/")]
    [InlineData("f((x)) / 2;", "/")]
    public void The_consumed_parentheses_role_distinguishes_if_headers_from_call_results(
        string input, string slashToken)
    {
        const string syntax = """
            Program : Statement+ ;
            Statement : IfStatement | Expr ';' ;
            IfStatement : 'if' '(' Expr ')' Statement ;
            Expr : Expr '+' Term | Term ;
            Term : Term '/' Primary | Primary ;
            Primary : id | num | rx | Primary '(' Expr? ')' | Primary '.' id | '(' Expr ')' ;
            """;
        var fixture = new Fixture(syntax + Tokens);
        var contexts = new List<LexicalContext>();
        var policy = new LexicalPolicy(new[] { fixture.Div, fixture.RegExp }, context =>
        {
            contexts.Add(context);
            return GoalFromConsumedSyntax(context, fixture);
        });

        var result = fixture.Parser.Parsing(fixture.Source(input, policy));

        AssertSuccess(fixture.Parser, result);
        var actualSlash = Assert.Single(Shifted(result), token => token.Input.StartsWith('/'));
        Assert.Equal(slashToken, actualSlash.Input);
        Assert.Equal(slashToken != "/", actualSlash.Kind == fixture.RegexTerminal);
        var afterClosingParen = Assert.Single(contexts, context =>
            context.SourceOffset == input.IndexOf(" /", StringComparison.Ordinal));
        Assert.Equal(")", afterClosingParen.PreviousToken.Input);
        Assert.Equal(slashToken == "/" ? fixture.Div : fixture.RegExp,
            GoalFromConsumedSyntax(afterClosingParen, fixture));
    }

    [Fact]
    public void Selector_history_is_a_snapshot_of_shifted_tokens_and_the_source_can_be_reused()
    {
        var fixture = new Fixture(Expressions + Tokens);
        var observations = new List<(LexicalContext Context, TokenData[] Snapshot)>();
        var policy = new LexicalPolicy(new[] { fixture.Div, fixture.RegExp }, context =>
        {
            observations.Add((context, context.ConsumedTokens.ToArray()));
            Assert.Equal(context.TokenIndex, context.ConsumedTokens.Count);
            Assert.Equal(context.PreviousToken, context.ConsumedTokens.LastOrDefault());
            Assert.NotEmpty(context.StateStack);
            return GoalFromConsumedSyntax(context, fixture);
        });
        var source = fixture.Source("a=/ab/g+c;", policy);

        var first = fixture.Parser.Parsing(source);
        AssertSuccess(fixture.Parser, first);
        var second = fixture.Parser.Parsing(source);
        AssertSuccess(fixture.Parser, second);

        Assert.Equal(Shifted(first).Select(token => token.Input), Shifted(second).Select(token => token.Input));
        Assert.All(observations, item => Assert.Equal(item.Snapshot, item.Context.ConsumedTokens));
        Assert.Empty(observations[0].Context.ConsumedTokens);
    }

    [Theory]
    [InlineData("a=b@;", 3, "@")]
    [InlineData("a=b/;", 4, ";")]
    [InlineData("a=/unclosed;", 2, "/")]
    public void Invalid_input_reports_the_original_source_position_and_allows_a_later_valid_parse(
        string input, int errorOffset, string tokenText)
    {
        var fixture = new Fixture(Expressions + Tokens);

        var failed = fixture.Parser.Parsing(fixture.Source(input));

        Assert.False(failed.Success);
        Assert.NotNull(fixture.Parser.FiredException);
        Assert.Contains(failed.AllErrors.SelectMany(error => error.ErrTokens),
            token => token.StartIndex == errorOffset && token.Input == tokenText);
        if (input == "a=/unclosed;")
        {
            // The selected regex goal cannot recognize this slash as a complete regex. Falling
            // back to Div would hide a lexical-goal error and change the token's meaning.
            Assert.DoesNotContain(Shifted(failed), token => token.Input == "/");
            Assert.Contains(failed, block => block.Token?.Kind is NotDefined);
        }
        AssertSuccess(fixture.Parser,
            fixture.Parser.Parsing(fixture.Source("a=b/c;")));
    }

    [Fact]
    public void A_selector_exception_is_preserved_in_a_positioned_diagnostic_before_peek()
    {
        var fixture = new Fixture(Expressions + Tokens);
        var expected = new InvalidOperationException("goal callback failed");
        var policy = new LexicalPolicy(new[] { fixture.Div, fixture.RegExp }, _ => throw expected);
        var source = new RecordingSource(fixture.Source("a=b;", policy));

        var failed = fixture.Parser.Parsing(source);

        Assert.False(failed.Success);
        var error = Assert.IsType<ContextualLexingException>(fixture.Parser.FiredException);
        Assert.Same(expected, error.InnerException);
        Assert.Equal(0, error.SourceOffset);
        Assert.Equal(0, error.TokenIndex);
        Assert.Empty(source.Reads);
        AssertSuccess(fixture.Parser,
            fixture.Parser.Parsing(fixture.Source("a=b;")));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Prediction_keeps_a_shorter_tokenization_that_accepts_before_the_other_origin_finishes(
        bool reverseAlternatives)
    {
        string shortRule = "'p' {Allow()}? 'x' rx ';'";
        string longRule = "'p' 'x' '/' id '/' id ';' '!'";
        var grammar = "S : " + (reverseAlternatives ? longRule + " | " + shortRule : shortRule + " | " + longRule) + " ; ";
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => true);
        var fixture = new Fixture(grammar + Tokens, options);

        var result = fixture.Parser.Parsing(fixture.Source("p x /a/g;"));

        AssertSuccess(fixture.Parser, result);
        Assert.Equal(new[] { "p", "x", "/a/g", ";" }, Shifted(result).Select(token => token.Input));
        Assert.Single(Shifted(result), token => token.Kind == fixture.RegexTerminal);
    }

    [Fact]
    public void Prediction_reports_ambiguity_when_different_tokenizations_accept_the_same_source()
    {
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => true);
        var fixture = new Fixture("S : 'p' {Allow()}? 'x' rx ';' | 'p' 'x' '/' id '/' id ';' ; " + Tokens, options);

        var result = fixture.Parser.Parsing(fixture.Source("p x /a/g;"));

        Assert.False(result.Success);
        Assert.IsType<AmbiguousPredictionException>(fixture.Parser.FiredException);
        // Prediction can scan far ahead, but actual parsing has only shifted the common prefix p.
        Assert.Equal(new[] { "p" }, Shifted(result).Select(token => token.Input));
    }

    [Fact]
    public void A_false_guard_keeps_the_division_path_without_requesting_regex_tokens()
    {
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => false);
        var fixture = new Fixture("S : 'p' {Allow()}? 'x' rx ';' | 'p' 'x' '/' id '/' id ';' ; " + Tokens, options);
        var source = new RecordingSource(fixture.Source("p x /a/g;"));

        var result = fixture.Parser.Parsing(source);

        AssertSuccess(fixture.Parser, result);
        Assert.Equal(new[] { "p", "x", "/", "a", "/", "g", ";" },
            Shifted(result).Select(token => token.Input));
        Assert.DoesNotContain(source.Reads, read => read.Read.Token.Kind == fixture.RegexTerminal);
    }

    [Fact]
    public void Prediction_predicates_observe_each_branches_own_token_count_previous_token_and_gap()
    {
        var seen = new List<(string Name, PredicateContext Context)>();
        var options = new EbnfReaderOptions().RegisterPredicate("Allow", _ => true)
            .RegisterPredicate("AfterRegex", context => { seen.Add(("regex", context)); return true; })
            .RegisterPredicate("AfterDiv", context => { seen.Add(("div", context)); return true; });
        var fixture = new Fixture("""
            S : 'p' {Allow()}? 'x' rx {AfterRegex()}? ';'
              | 'p' 'x' '/' id '/' id {AfterDiv()}? ';' ;
            """ + Tokens, options);
        const string input = "p x /a/g /* gap\n */ ;";

        var result = fixture.Parser.Parsing(fixture.Source(input));

        Assert.False(result.Success);
        Assert.IsType<AmbiguousPredictionException>(fixture.Parser.FiredException);
        Assert.Contains(seen, item => item.Name == "regex");
        Assert.Contains(seen, item => item.Name == "div");
        Assert.All(seen, item =>
        {
            Assert.Equal(item.Name == "regex" ? 3 : 6, item.Context.TokenIndex);
            Assert.Equal(item.Name == "regex" ? "/a/g" : "g", item.Context.PreviousToken.Input);
            Assert.Equal(";", item.Context.NextToken.Input);
            Assert.Equal(input.IndexOf(';'), item.Context.NextToken.StartIndex);
            Assert.Equal(" /* gap\n */ ", item.Context.NextToken.LeadingTrivia.Text);
            Assert.True(item.Context.NextToken.HasLineBreakBefore);
        });
    }

    private static LexicalGoal GoalFromConsumedSyntax(LexicalContext context, Fixture fixture)
    {
        var history = context.ConsumedTokens;
        if (history.Count == 0) return fixture.RegExp;
        var last = history[history.Count - 1];
        if (last.Input == ")")
        {
            // Matching the actual opening parenthesis distinguishes an if header from a call,
            // even if an inner call or parenthesized expression ends with the same ')' token.
            int depth = 0;
            for (int index = history.Count - 1; index >= 0; index--)
            {
                if (history[index].Input == ")") depth++;
                else if (history[index].Input == "(" && --depth == 0)
                    return index > 0 && history[index - 1].Input == "if" ? fixture.RegExp : fixture.Div;
            }
            throw new InvalidOperationException("The fixture's consumed parentheses are unbalanced.");
        }
        return last.Kind.Caption is "id" or "num" or "rx" ? fixture.Div : fixture.RegExp;
    }

    private static EbnfGrammar Read(string text, EbnfReaderOptions options = null)
    {
        var result = EbnfGrammarReader.Read(text, options);
        Assert.True(result.Success, string.Join("; ", result.Errors));
        return result.Grammar;
    }

    private static IEnumerable<Terminal> Scannable(EbnfGrammar grammar)
        => grammar.TerminalSet.Where(terminal => terminal is not EndMarker && terminal is not Epsilon);

    private static ITokenSource Source(EbnfGrammar grammar, string input, LexicalPolicy policy)
    {
        var lexer = new Lexer();
        foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
        return lexer.CreateTokenSource(input, policy);
    }

    private static TokenData[] Shifted(ParsingResult result)
        => result.Trace.Steps.Where(step => step.Action is ParseAction.Shift)
            .Select(step => step.Lookahead).ToArray();

    private static void AssertSuccess(LALRParser parser, ParsingResult result)
    {
        Assert.True(result.Success, parser.FiredException?.ToString());
        Assert.Null(parser.FiredException);
        Assert.Empty(result.AllErrors);
    }

    private sealed class Fixture
    {
        public EbnfGrammar Grammar { get; }
        public LALRParser Parser { get; }
        public LexicalGoal Div { get; }
        public LexicalGoal RegExp { get; }
        public LexicalPolicy Policy { get; }
        public Terminal RegexTerminal { get; }

        public Fixture(string text, EbnfReaderOptions options = null)
        {
            Grammar = Read(text, options);
            Parser = new LALRParser(Grammar, true);
            RegexTerminal = Assert.Single(Grammar.TerminalSet, terminal => terminal.Caption == "rx");
            var common = Scannable(Grammar).Where(terminal => terminal != RegexTerminal && terminal.Value != "/").ToArray();
            Div = new LexicalGoal(common.Concat(Scannable(Grammar).Where(terminal => terminal.Value == "/")));
            RegExp = new LexicalGoal(common.Concat(new[] { RegexTerminal }));
            Policy = new LexicalPolicy(Div, RegExp);
        }

        public ITokenSource Source(string input, LexicalPolicy policy = null)
            => ParserDrivenLexingTests.Source(Grammar, input, policy ?? Policy);
    }

    private sealed class RecordingSource : ITokenSource
    {
        private readonly ITokenSource inner;
        public List<(TokenCursor Cursor, LexicalGoal Goal, TokenRead Read)> Reads { get; } = new();
        public TokenCursor Start => inner.Start;
        public int Length => inner.Length;
        public LexicalPolicy Policy => inner.Policy;

        public RecordingSource(ITokenSource inner)
        {
            this.inner = inner;
        }

        public TokenRead Peek(TokenCursor start, LexicalGoal goal)
        {
            var result = inner.Peek(start, goal);
            Reads.Add((start, goal, result));
            return result;
        }
    }
}
