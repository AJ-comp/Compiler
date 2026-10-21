using Janglim.FrontEnd.RegularGrammar;
using Janglim.Mcp.Engine;
using Xunit;

namespace Janglim.Mcp.Tests;

/// <summary>
/// Verifies the MCP engine contract affected by shared longest-match lexing,
/// without relying on a web server or worker-process startup to classify errors.
/// </summary>
public sealed class MaximalMunchEngineTests
{
    private const string IdentifierGrammar = """
        Statement : 'if' id ';' | id ';' ;
        id := Identifier "[$A-Za-z_][$0-9A-Za-z_]*" ;
        """;

    // The lookahead prevents the regex engine from optimizing a simple failing
    // suffix check into a cheap scan, and forces overlapping-alternative backtracking.
    private const string SlowGrammar = """
        Start : id ;
        id := Identifier "(?=(a|aa)+a$)a" ;
        """;

    [Theory]
    [InlineData("if value;", "if", "id", "value", 3)]
    [InlineData("if $name;", "if", "id", "$name", 3)]
    [InlineData("if name$;", "if", "id", "name$", 3)]
    [InlineData("ifx;", "id", "id", "ifx", 0)]
    [InlineData("if$;", "id", "id", "if$", 0)]
    [InlineData("$name;", "id", "id", "$name", 0)]
    [InlineData("name$;", "id", "id", "name$", 0)]
    public void Engine_returns_complete_dollar_identifiers_and_keeps_keywords_reserved(
        string input, string firstSymbol, string identifierSymbol, string identifierText, int identifierStart)
    {
        var result = EngineExecutor.Execute(new("parse_input", IdentifierGrammar, input, IncludeTree: true));

        Assert.Equal("ok", result.Status);
        Assert.True(result.Grammar!.IsValid);
        Assert.False(result.Grammar.HasConflicts);
        var parse = Assert.IsType<ParseReport>(result.Parse);
        Assert.True(parse.Success);
        Assert.Empty(parse.Diagnostics);
        Assert.Equal(firstSymbol, parse.Tokens[0].Symbol);
        var identifier = Assert.Single(parse.Tokens, token => token.Text == identifierText);
        Assert.Equal(identifierSymbol, identifier.Symbol);
        Assert.Equal(identifierStart, identifier.Start);
        Assert.Equal(identifierStart + identifierText.Length, identifier.End);
        Assert.NotEmpty(parse.Tree!);
        var leaf = Assert.Single(parse.Tree!, node => node.Kind == "terminal" && node.Text == identifierText);
        Assert.Equal(identifierStart, leaf.Start);
        Assert.Equal(identifierStart + identifierText.Length, leaf.End);
    }

    [Theory]
    [InlineData("Start : 'a'? ;", "", true)]
    [InlineData("Start : 'a'? ;", " \t\r\n", true)]
    [InlineData("Start : 'a' ;", "", false)]
    [InlineData("Start : 'a' ;", " \t\r\n", false)]
    [InlineData("Start : 'a'? ; comment := Comment \"/\\*[\\s\\S]*?\\*/\" ;", " /* only a comment */ ", true)]
    [InlineData("Start : 'a' ; comment := Comment \"/\\*[\\s\\S]*?\\*/\" ;", " /* only a comment */ ", false)]
    public void Engine_uses_the_parse_table_to_decide_empty_or_trivia_only_inputs(
        string grammar, string input, bool expected)
    {
        var result = EngineExecutor.Execute(new("parse_input", grammar, input));

        Assert.Equal("ok", result.Status);
        var parse = Assert.IsType<ParseReport>(result.Parse);
        Assert.Equal(expected, parse.Success);
        Assert.Equal(0, parse.TokenCount);
        Assert.Empty(parse.Tokens);
        if (expected) Assert.Empty(parse.Diagnostics);
        else Assert.NotEmpty(parse.Diagnostics);
    }

    [Fact]
    public void Physical_end_marker_in_user_input_still_reports_the_reserved_marker_diagnostic()
    {
        var result = EngineExecutor.Execute(new("parse_input", "Start : 'a'? ;", new EndMarker().Value));

        Assert.Equal("ok", result.Status);
        Assert.False(result.Parse!.Success);
        Assert.Contains(result.Parse.Diagnostics, diagnostic => diagnostic.Code == "reserved_end_marker");
    }

    [Theory]
    [InlineData("Start : id ; id := \"(?=x)\" ;", "x")]
    [InlineData("Start : id ; id := \"x*\" ;", "y")]
    public void Zero_length_token_matches_keep_the_invalid_grammar_diagnostic_contract(string grammar, string input)
    {
        var result = EngineExecutor.Execute(new("parse_input", grammar, input));

        Assert.Equal("invalid_grammar", result.Status);
        Assert.False(result.Parse!.Success);
        Assert.Contains(result.Parse.Diagnostics, diagnostic => diagnostic.Code == "zero_length_token");
    }

    [Fact]
    public void Regex_execution_timeout_is_a_runtime_limit_and_does_not_break_the_next_request()
    {
        var result = EngineExecutor.Execute(new("parse_input", SlowGrammar, new string('a', 128) + "!"));

        Assert.Equal("runtime_limit", result.Status);
        Assert.False(result.Parse!.Success);
        Assert.Contains(result.Parse.Diagnostics, diagnostic => diagnostic.Code == "runtime_limit");

        var next = EngineExecutor.Execute(new("parse_input", "Start : 'a' ;", "a"));
        Assert.Equal("ok", next.Status);
        Assert.True(next.Parse!.Success);
        Assert.Empty(next.Parse.Diagnostics);
    }

    [Fact]
    public void Regex_execution_timeout_cannot_pass_a_negative_grammar_test()
    {
        var result = EngineExecutor.Execute(new("run_tests", SlowGrammar, Tests:
            [new("bounded regex execution", new string('a', 128) + "!", ExpectSuccess: false)]));

        Assert.Equal("runtime_limit", result.Status);
        var tests = Assert.IsType<TestRunReport>(result.Tests);
        Assert.Equal(1, tests.Total);
        Assert.Equal(0, tests.Passed);
        Assert.Equal(1, tests.Failed);
        var test = Assert.Single(tests.Cases);
        Assert.False(test.ActualSuccess);
        Assert.False(test.Passed);
        Assert.Contains(test.Diagnostics, diagnostic => diagnostic.Code == "runtime_limit");
    }
}
