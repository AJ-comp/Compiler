using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;
using Xunit;

namespace Janglim.FrontEnd.Parsers.Tests;

/// <summary>
/// Exercises the versioned C syntax profile itself: lexical boundaries, accepted/rejected
/// programs, and the concrete tree structure that grammar editing must preserve.
/// These are syntax tests, not a C type checker or a compiler conformance claim.
/// </summary>
[Trait("Category", "C11Core")]
public class C11CoreGrammarTests : IClassFixture<C11CoreGrammarFixture>
{
    private readonly C11CoreGrammarFixture fixture;

    public C11CoreGrammarTests(C11CoreGrammarFixture fixture) => this.fixture = fixture;

    public static IEnumerable<object[]> CorpusCases()
        => C11CoreGrammarFixture.LoadCases().Select(test =>
            new object[] { test.Name, test.Input, test.ExpectSuccess });

    [Fact]
    public void Canonical_grammar_is_conflict_free_and_within_remote_engine_limits()
    {
        Assert.Empty(fixture.Parser.Conflicts);
        Assert.InRange(fixture.GrammarSource.Length, 1, 8192);
        Assert.InRange(fixture.Grammar.NonTerminalSingles.Count, 1, 512);
        Assert.InRange(fixture.Parser.Canonical.IndexStateDic.Count, 1, 512);
    }

    [Fact]
    public void Corpus_has_unique_named_positive_and_negative_examples()
    {
        var cases = C11CoreGrammarFixture.LoadCases();
        Assert.NotEmpty(cases);
        Assert.All(cases, test => Assert.False(string.IsNullOrWhiteSpace(test.Name)));
        Assert.Equal(cases.Count, cases.Select(test => test.Name).Distinct(StringComparer.Ordinal).Count());
        Assert.Contains(cases, test => test.ExpectSuccess);
        Assert.Contains(cases, test => !test.ExpectSuccess);
    }

    [Fact]
    public void Editing_one_named_rule_changes_the_language_without_engine_changes()
    {
        const string original = "if_header : 'if' '(' expression ')' ;";
        const string replacement = "if_header : 'unless' '(' expression ')' ;";
        Assert.Contains(original, fixture.GrammarSource);
        var edited = new C11CoreGrammarFixture(fixture.GrammarSource.Replace(original, replacement));
        Assert.Empty(edited.Parser.Conflicts);
        Assert.True(edited.Parse("int f(int x) { unless (x) return 1; else return 0; }").Success);
        Assert.False(edited.Parse("int f(int x) { if (x) return 1; else return 0; }").Success);
        Assert.True(edited.Parse("int f(int x) { while (x) x--; return x; }").Success);
    }

    [Theory]
    [MemberData(nameof(CorpusCases))]
    public void Corpus_program_has_expected_syntax(string name, string input, bool expectSuccess)
    {
        var result = fixture.Parse(input);
        Assert.True(result.Success == expectSuccess,
            $"{name}: expected {(expectSuccess ? "accept" : "reject")}; {result.Details}");
        if (expectSuccess)
        {
            Assert.NotNull(result.Root);
            Assert.Equal("translation_unit", result.Root.SignPost.Name);
        }
    }

    [Fact]
    public void Lexer_preserves_literal_values_offsets_and_skips_input_comments()
    {
        const string input = "/* first */ char *s = \"https://x/*text*/ \\\"quoted\\\"\"; // next\nint c = '\\n';";
        var lexed = fixture.Lexer.Lexing(input);
        var tokens = lexed.TokensForParsing;
        Assert.Equal(new[] { "char", "*", "s", "=", "\"https://x/*text*/ \\\"quoted\\\"\"", ";",
            "int", "c", "=", "'\\n'", ";" }, tokens.Select(token => token.Data));
        Assert.Contains(lexed.TokensForView, token => token.PatternInfo.Terminal.TokenType is Comment);
        Assert.DoesNotContain(tokens, token => token.PatternInfo.Terminal.TokenType is Comment);
        Assert.Equal(TokenType.Literal.StringLiteral, tokens[4].PatternInfo.Terminal.TokenType);
        Assert.Equal(TokenType.Literal.CharLiteral, tokens[9].PatternInfo.Terminal.TokenType);
        Assert.All(tokens, token => Assert.Equal(token.Data, input.Substring(token.StartIndex, token.Data.Length)));
    }

    [Fact]
    public void Overlapping_operators_and_numeric_forms_remain_whole_tokens()
    {
        const string input = "int f(int x) { x >>= 2; x <<= 1; x++; return x >= .5 && x != 0xFFu; }";
        var tokens = fixture.Lexer.Lexing(input).TokensForParsing.Select(token => token.Data).ToArray();
        Assert.Equal(new[] { "int", "f", "(", "int", "x", ")", "{", "x", ">>=", "2", ";",
            "x", "<<=", "1", ";", "x", "++", ";", "return", "x", ">=", ".5", "&&", "x", "!=", "0xFFu", ";", "}" }, tokens);
    }

    [Fact]
    public void Multiplication_is_nested_inside_addition()
    {
        var root = ParseTree("int f(void) { return 1 + 2 * 3; }");
        var addition = Find(root, "additive_expression", "1 + 2 * 3");
        var multiplication = Find(addition, "multiplicative_expression", "2 * 3");
        Assert.NotSame(addition, multiplication);
        Assert.DoesNotContain(Descendants(root), node =>
            node.SignPost.Name == "multiplicative_expression" && Tokens(node) == "1 + 2 * 3");
    }

    [Fact]
    public void Parentheses_move_addition_inside_multiplication()
    {
        var root = ParseTree("int f(void) { return (1 + 2) * 3; }");
        var multiplication = Find(root, "multiplicative_expression", "( 1 + 2 ) * 3");
        var addition = Find(multiplication, "additive_expression", "1 + 2");
        Assert.NotSame(multiplication, addition);
    }

    [Fact]
    public void Subtraction_nests_on_the_left()
    {
        var root = ParseTree("int f(void) { return 8 - 3 - 1; }");
        var outer = Find(root, "additive_expression", "8 - 3 - 1");
        Assert.NotSame(outer, Find(outer, "additive_expression", "8 - 3"));
        Assert.DoesNotContain(Descendants(outer), node =>
            node.SignPost.Name == "additive_expression" && Tokens(node) == "3 - 1");
    }

    [Fact]
    public void Assignment_nests_on_the_right()
    {
        var root = ParseTree("int f(int a, int b, int c) { a = b = c; return a; }");
        var outer = Find(root, "assignment_expression", "a = b = c");
        Assert.NotSame(outer, Find(outer, "assignment_expression", "b = c"));
        Assert.DoesNotContain(Descendants(outer), node =>
            node.SignPost.Name == "assignment_expression" && Tokens(node) == "a = b");
    }

    [Fact]
    public void Else_binds_to_the_nearest_unbraced_if()
    {
        var root = ParseTree("int f(int a, int b) { if (a) if (b) return 1; else return 2; return 3; }");
        var outer = Find(root, "open_statement", "if ( a ) if ( b ) return 1 ; else return 2 ;");
        var inner = Find(outer, "closed_statement", "if ( b ) return 1 ; else return 2 ;");
        Assert.NotSame(outer, inner);
        Assert.DoesNotContain(Descendants(root), node =>
            node.SignPost.Name == "closed_statement" && Tokens(node) == Tokens(outer));
    }

    [Fact]
    public void Braces_allow_else_to_bind_to_the_outer_if()
    {
        var root = ParseTree("int f(int a, int b) { if (a) { if (b) return 1; } else return 2; return 3; }");
        var outer = Find(root, "closed_statement", "if ( a ) { if ( b ) return 1 ; } else return 2 ;");
        Assert.NotSame(outer, Find(outer, "open_statement", "if ( b ) return 1 ;"));
    }

    [Fact]
    public void Array_of_pointers_and_pointer_to_array_have_different_declarator_trees()
    {
        var arrayRoot = ParseTree("int *items[3];");
        var arrayDeclarator = Find(arrayRoot, "declarator", "* items [ 3 ]");
        Find(arrayDeclarator, "pointer", "*");
        Find(arrayDeclarator, "direct_declarator", "items [ 3 ]");

        var pointerRoot = ParseTree("int (*items)[3];");
        var pointerDeclarator = Find(pointerRoot, "direct_declarator", "( * items ) [ 3 ]");
        Find(pointerDeclarator, "declarator", "* items");
        Assert.DoesNotContain(Descendants(pointerDeclarator), node =>
            node.SignPost.Name == "direct_declarator" && Tokens(node) == "items [ 3 ]");
    }

    private ParseTreeNonTerminal ParseTree(string input)
    {
        var result = fixture.Parse(input);
        Assert.True(result.Success, result.Details);
        return Assert.IsType<ParseTreeNonTerminal>(result.Root);
    }

    private static ParseTreeNonTerminal Find(ParseTreeNonTerminal root, string symbol, string tokens)
    {
        var node = Descendants(root).FirstOrDefault(candidate =>
            candidate.SignPost.Name == symbol && Tokens(candidate) == tokens);
        Assert.True(node != null, $"Expected {symbol}: {tokens}");
        return node;
    }

    private static string Tokens(ParseTreeNonTerminal node)
        => string.Join(" ", node.AllTokens.Select(token => token.Input));

    private static IEnumerable<ParseTreeNonTerminal> Descendants(ParseTreeNonTerminal root)
    {
        yield return root;
        foreach (var child in root.Items.OfType<ParseTreeNonTerminal>())
            foreach (var descendant in Descendants(child)) yield return descendant;
    }
}

/// <summary>One compiled table is reused sequentially; the test assembly disables parallel grammar construction.</summary>
public sealed class C11CoreGrammarFixture
{
    public string GrammarSource { get; }
    public EbnfGrammar Grammar { get; }
    public LALRParser Parser { get; }
    public Lexer Lexer { get; }

    public C11CoreGrammarFixture() : this(File.ReadAllText(FixturePath("c11-core.ebnf"))) { }

    internal C11CoreGrammarFixture(string grammarSource)
    {
        GrammarSource = grammarSource;
        var read = EbnfGrammarReader.Read(GrammarSource);
        Assert.True(read.Success, string.Join("; ", read.Errors));
        Grammar = read.Grammar;
        Lexer = new Lexer();
        foreach (var terminal in Grammar.TerminalSet) Lexer.AddTokenRule(terminal);
        Parser = new LALRParser(Grammar, bLogging: false);
    }

    public static IReadOnlyList<C11CorpusCase> LoadCases()
        => JsonSerializer.Deserialize<List<C11CorpusCase>>(File.ReadAllText(FixturePath("c11-core.cases.json")),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("The C corpus must be a JSON array.");

    public C11ParseOutcome Parse(string input)
    {
        var lexed = Lexer.Lexing(input);
        var unknown = lexed.TokensForView.Where(token => token.PatternInfo.Terminal is NotDefined ||
            Grammar.IsRejectedToken(token.PatternInfo.Terminal)).ToArray();
        if (unknown.Length > 0)
            return new C11ParseOutcome(false, null, "unrecognized tokens: " + string.Join(" | ", unknown.Select(token => token.Data)));

        var empty = lexed.TokensForParsing.Count == 0;
        // Require the table to decide empty-input acceptance rather than the parser's zero-token shortcut.
        var tokens = empty ? Lexer.Lexing(new EndMarker().Value).TokensForParsing : lexed.TokensForParsing;
        var previousException = Parser.FiredException;
        var parsed = Parser.Parsing(tokens);
        if (Parser.FiredException is not ParsingException && Parser.FiredException != null &&
            !ReferenceEquals(previousException, Parser.FiredException))
            throw new InvalidOperationException("A parser failure must not count as a successful negative case.", Parser.FiredException);
        var finalIndex = empty ? 0 : parsed.Count - 1;
        var accept = finalIndex >= 0 && finalIndex < parsed.Count
            ? parsed[finalIndex].Units.LastOrDefault(unit => unit.Action.Action is ParseAction.Accept) : null;
        var success = parsed.Success && !parsed.HasError && accept != null;
        var root = success ? accept.AfterStack.Stack.OfType<ParseTreeNonTerminal>().FirstOrDefault() : null;
        var details = success ? "accepted" : string.Join("; ", parsed.AllErrors.Select(error => error.Message));
        if (!success && string.IsNullOrEmpty(details)) details = "no final Accept action";
        return new C11ParseOutcome(success, root, details);
    }

    private static string FixturePath(string name)
        => Path.Combine(AppContext.BaseDirectory, "Fixtures", "C", name);
}

public sealed class C11CorpusCase
{
    [JsonRequired]
    public string Name { get; set; }
    [JsonRequired]
    public string Input { get; set; }
    [JsonRequired]
    public bool ExpectSuccess { get; set; }
}

public sealed class C11ParseOutcome
{
    public bool Success { get; }
    public ParseTreeNonTerminal Root { get; }
    public string Details { get; }

    public C11ParseOutcome(bool success, ParseTreeNonTerminal root, string details)
    {
        Success = success;
        Root = root;
        Details = details;
    }
}
