using System.Text.Json;
using Xunit;

namespace Janglim.Mcp.Tests;

[Collection(McpCollection.Name)]
public sealed class TypedTokenHttpTests(McpServerFixture server)
{
    // Doubled double quotes belong to the EBNF notation; regex backslashes are preserved verbatim.
    // These example patterns recognize quoted tokens, rather than the entire C literal specification.
    private const string QuotedPair = """"
        Pair : text ch ;
        text := StringLiteral """(?:[^""\\\r\n]|\\[^\r\n])*""" ;
        ch := CharLiteral "'(?:[^'\\\r\n]|\\[^\r\n])'" ;
        """";

    private const string EscapedText = """
        "hello \"quote\" \\ path"
        """;

    [Fact]
    public async Task Typed_quoted_tokens_compile_through_the_mcp_worker()
    {
        var result = await server.CallToolAsync("validate_grammar", new { grammar = QuotedPair });

        Assert.Equal("ok", result.GetProperty("status").GetString());
        var grammar = result.GetProperty("grammar");
        Assert.True(grammar.GetProperty("isValid").GetBoolean());
        Assert.False(grammar.GetProperty("hasConflicts").GetBoolean());
        Assert.True(grammar.GetProperty("stateCount").GetInt32() > 0);
        Assert.Empty(grammar.GetProperty("diagnostics").EnumerateArray());
    }

    [Theory]
    [InlineData("Start : id ; id := \"[a-z]+\" ;")]
    [InlineData("Start : id ; id := Identifier \"[a-z]+\" ;")]
    public async Task Explicit_and_default_identifier_kinds_keep_named_token_output(string grammar)
    {
        var result = await server.CallToolAsync("parse_input", new { grammar, input = "name" });

        Assert.Equal("ok", result.GetProperty("status").GetString());
        var parse = result.GetProperty("parse");
        Assert.True(parse.GetProperty("success").GetBoolean());
        var token = Assert.Single(parse.GetProperty("tokens").EnumerateArray());
        AssertSpan(token, "id", "name", 0, 4);
    }

    [Fact]
    public async Task Quoted_tokens_preserve_text_and_utf16_spans_in_tokens_and_tree()
    {
        const string input = "  \"hi😀\" '한'  ";
        var result = await server.CallToolAsync("parse_input", new
        {
            grammar = QuotedPair, input, includeTree = true
        });

        Assert.Equal("ok", result.GetProperty("status").GetString());
        var parse = result.GetProperty("parse");
        Assert.True(parse.GetProperty("success").GetBoolean());
        Assert.Equal(2, parse.GetProperty("tokenCount").GetInt32());
        Assert.Empty(parse.GetProperty("diagnostics").EnumerateArray());
        Assert.False(parse.GetProperty("tokensTruncated").GetBoolean());
        Assert.False(parse.GetProperty("treeTruncated").GetBoolean());

        var tokens = parse.GetProperty("tokens").EnumerateArray().ToArray();
        Assert.Equal(2, tokens.Length);
        AssertSpan(tokens[0], "text", "\"hi😀\"", 2, 8);
        AssertSpan(tokens[1], "ch", "'한'", 9, 12);

        var nodes = parse.GetProperty("tree").EnumerateArray().ToArray();
        var root = Assert.Single(nodes, node => node.GetProperty("id").GetInt32() == 0);
        Assert.Equal("Pair", root.GetProperty("symbol").GetString());
        Assert.Equal(2, root.GetProperty("start").GetInt32());
        Assert.Equal(12, root.GetProperty("end").GetInt32());
        var children = root.GetProperty("children").EnumerateArray().Select(id => id.GetInt32()).ToArray();
        Assert.Equal(2, children.Length);
        for (var index = 0; index < children.Length; index++)
        {
            var leaf = Assert.Single(nodes, node => node.GetProperty("id").GetInt32() == children[index]);
            Assert.Equal("terminal", leaf.GetProperty("kind").GetString());
            Assert.Empty(leaf.GetProperty("children").EnumerateArray());
            Assert.Equal(tokens[index].GetProperty("symbol").GetString(), leaf.GetProperty("symbol").GetString());
            Assert.Equal(tokens[index].GetProperty("text").GetString(), leaf.GetProperty("text").GetString());
            Assert.Equal(tokens[index].GetProperty("start").GetInt32(), leaf.GetProperty("start").GetInt32());
            Assert.Equal(tokens[index].GetProperty("end").GetInt32(), leaf.GetProperty("end").GetInt32());
        }
    }

    [Fact]
    public async Task Escaped_quotes_and_backslashes_remain_in_the_returned_token_text()
    {
        const string text = EscapedText;
        const string character = """'\n'""";
        var input = text + " " + character;
        var result = await server.CallToolAsync("parse_input", new
        {
            grammar = QuotedPair, input, includeTree = true
        });

        Assert.Equal("ok", result.GetProperty("status").GetString());
        var parse = result.GetProperty("parse");
        Assert.True(parse.GetProperty("success").GetBoolean());
        var tokens = parse.GetProperty("tokens").EnumerateArray().ToArray();
        Assert.Equal(2, tokens.Length);
        AssertSpan(tokens[0], "text", text, 0, text.Length);
        AssertSpan(tokens[1], "ch", character, text.Length + 1, input.Length);
        var treeText = parse.GetProperty("tree").EnumerateArray()
            .Where(node => node.GetProperty("kind").GetString() == "terminal")
            .Select(node => node.GetProperty("text").GetString()).ToArray();
        Assert.Equal(new[] { text, character }, treeText);
    }

    [Fact]
    public async Task Typed_token_regressions_distinguish_complete_quotes_from_malformed_input()
    {
        var cases = new[]
        {
            new { name = "ordinary pair", input = "\"hello\" 'a'", expectSuccess = true },
            new { name = "empty string and unicode character", input = "\"\" '한'", expectSuccess = true },
            new { name = "escaped string and character", input = EscapedText + " '\\n'", expectSuccess = true },
            new { name = "adjacent quoted tokens", input = "\"a\"'b'", expectSuccess = true },
            new { name = "unclosed string", input = "\"hello 'a'", expectSuccess = false },
            new { name = "unclosed character", input = "\"hello\" 'a", expectSuccess = false },
            new { name = "empty character", input = "\"hello\" ''", expectSuccess = false },
            new { name = "multiple character contents", input = "\"hello\" 'ab'", expectSuccess = false },
            new { name = "raw newline in string", input = "\"hello\nworld\" 'a'", expectSuccess = false },
            new { name = "raw newline in character", input = "\"hello\" '\n'", expectSuccess = false },
            new { name = "missing character", input = "\"hello\"", expectSuccess = false },
            new { name = "unrecognized trailing input", input = "\"hello\" 'a' @", expectSuccess = false }
        };
        var result = await server.CallToolAsync("run_tests", new { grammar = QuotedPair, cases });

        Assert.Equal("ok", result.GetProperty("status").GetString());
        var tests = result.GetProperty("tests");
        Assert.Equal(cases.Length, tests.GetProperty("total").GetInt32());
        Assert.Equal(cases.Length, tests.GetProperty("passed").GetInt32());
        Assert.Equal(0, tests.GetProperty("failed").GetInt32());
        var reportedCases = tests.GetProperty("cases").EnumerateArray().ToArray();
        Assert.Equal(cases.Length, reportedCases.Length);
        for (var index = 0; index < cases.Length; index++)
        {
            Assert.Equal(cases[index].name, reportedCases[index].GetProperty("name").GetString());
            Assert.Equal(cases[index].expectSuccess, reportedCases[index].GetProperty("actualSuccess").GetBoolean());
            Assert.True(reportedCases[index].GetProperty("passed").GetBoolean());
            if (!cases[index].expectSuccess)
                Assert.NotEmpty(reportedCases[index].GetProperty("diagnostics").EnumerateArray());
        }
    }

    [Theory]
    [InlineData("validate_grammar")]
    [InlineData("parse_input")]
    [InlineData("run_tests")]
    public async Task Unsupported_token_kind_is_a_grammar_error_for_every_tool(string tool)
    {
        const string grammar = "Value : text ;\ntext := UnsupportedKind \"[a-z]+\" ;";
        object arguments = tool switch
        {
            "parse_input" => new { grammar, input = "hello", includeTree = true },
            "run_tests" => new { grammar, cases = new[] { new { name = "hello", input = "hello", expectSuccess = true } } },
            _ => new { grammar }
        };
        var result = await server.CallToolAsync(tool, arguments);

        Assert.Equal("invalid_grammar", result.GetProperty("status").GetString());
        var report = result.GetProperty("grammar");
        Assert.False(report.GetProperty("isValid").GetBoolean());
        var diagnostic = Assert.Single(report.GetProperty("diagnostics").EnumerateArray());
        Assert.Equal("invalid_grammar", diagnostic.GetProperty("code").GetString());
        Assert.Contains("UnsupportedKind", diagnostic.GetProperty("message").GetString());
        Assert.True(!result.TryGetProperty("parse", out var parse) || parse.ValueKind == JsonValueKind.Null);
        Assert.True(!result.TryGetProperty("tests", out var tests) || tests.ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task Invalid_typed_regex_preserves_the_invalid_regex_diagnostic_contract()
    {
        const string grammar = "Value : text ; text := StringLiteral \"[\" ;";
        var result = await server.CallToolAsync("validate_grammar", new { grammar });

        Assert.Equal("invalid_grammar", result.GetProperty("status").GetString());
        var report = result.GetProperty("grammar");
        Assert.False(report.GetProperty("isValid").GetBoolean());
        var diagnostic = Assert.Single(report.GetProperty("diagnostics").EnumerateArray());
        Assert.Equal("invalid_regex", diagnostic.GetProperty("code").GetString());
    }

    private static void AssertSpan(JsonElement token, string symbol, string text, int start, int end)
    {
        Assert.Equal(symbol, token.GetProperty("symbol").GetString());
        Assert.Equal(text, token.GetProperty("text").GetString());
        Assert.Equal(start, token.GetProperty("start").GetInt32());
        Assert.Equal(end, token.GetProperty("end").GetInt32());
    }
}
