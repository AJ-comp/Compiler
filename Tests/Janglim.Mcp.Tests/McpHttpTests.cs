using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Xunit;

namespace Janglim.Mcp.Tests;

[Collection(McpCollection.Name)]
public sealed class McpHttpTests(McpServerFixture server)
{
    private const string Arithmetic = """
        Expr : Expr '+' Term | Term ;
        Term : Term '*' Factor | Factor ;
        Factor : '(' Expr ')' | id ;
        id := "[a-zA-Z]+" ;
        """;

    [Fact]
    public async Task Tools_discoverable_with_read_only_metadata_and_required_schemas()
    {
        var response = await server.RpcAsync("tools/list", new { });
        var tools = response.GetProperty("result").GetProperty("tools").EnumerateArray()
            .ToDictionary(tool => tool.GetProperty("name").GetString()!, tool => tool);
        Assert.Equal(new[] { "get_grammar", "list_grammars", "parse_input", "run_tests", "validate_grammar" },
            tools.Keys.Order().ToArray());
        foreach (var tool in tools.Values)
        {
            Assert.True(tool.GetProperty("annotations").GetProperty("readOnlyHint").GetBoolean());
            var schema = tool.GetProperty("inputSchema");
            Assert.Equal("object", schema.GetProperty("type").GetString());
            if (tool.GetProperty("name").GetString() is "parse_input" or "run_tests" or "validate_grammar")
            {
                Assert.Contains(schema.GetProperty("required").EnumerateArray(), item => item.GetString() == "grammar");
                Assert.Equal("string", schema.GetProperty("properties").GetProperty("grammar").GetProperty("type").GetString());
            }
        }
        Assert.True(tools["parse_input"].GetProperty("inputSchema").GetProperty("properties").TryGetProperty("includeTree", out _));
        Assert.True(tools["run_tests"].GetProperty("inputSchema").GetProperty("properties").TryGetProperty("cases", out _));
    }

    [Fact]
    public async Task Valid_grammar_returns_a_compiled_conflict_free_report()
    {
        var result = await server.CallToolAsync("validate_grammar", new { grammar = Arithmetic });
        Assert.Equal("ok", result.GetProperty("status").GetString());
        var grammar = result.GetProperty("grammar");
        Assert.True(grammar.GetProperty("isValid").GetBoolean());
        Assert.False(grammar.GetProperty("hasConflicts").GetBoolean());
        Assert.True(grammar.GetProperty("stateCount").GetInt32() > 0);
        Assert.True(grammar.GetProperty("ruleCount").GetInt32() > 0);
        Assert.Empty(grammar.GetProperty("diagnostics").EnumerateArray());
    }

    [Fact]
    public async Task Undefined_symbol_returns_actionable_grammar_diagnostics()
    {
        var result = await server.CallToolAsync("validate_grammar", new { grammar = "Start : undefined_symbol ;" });
        Assert.Equal("invalid_grammar", result.GetProperty("status").GetString());
        var grammar = result.GetProperty("grammar");
        Assert.False(grammar.GetProperty("isValid").GetBoolean());
        Assert.Contains(grammar.GetProperty("diagnostics").EnumerateArray(), diagnostic =>
            diagnostic.GetProperty("message").GetString()!.Contains("undefined", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Parse_returns_tokens_without_a_tree_by_default()
    {
        var result = await server.CallToolAsync("parse_input", new { grammar = Arithmetic, input = "a + b * c" });
        Assert.Equal("ok", result.GetProperty("status").GetString());
        var parse = result.GetProperty("parse");
        Assert.True(parse.GetProperty("success").GetBoolean());
        Assert.True(parse.GetProperty("tokenCount").GetInt32() > 0);
        Assert.False(parse.GetProperty("tokensTruncated").GetBoolean());
        Assert.True(!parse.TryGetProperty("tree", out var tree) || tree.ValueKind == JsonValueKind.Null);
        var plus = Assert.Single(parse.GetProperty("tokens").EnumerateArray(), token => token.GetProperty("text").GetString() == "+");
        Assert.Equal(2, plus.GetProperty("start").GetInt32());
        Assert.Equal(3, plus.GetProperty("end").GetInt32());
    }

    [Fact]
    public async Task Requested_tree_has_connected_nodes_and_input_spans()
    {
        var result = await server.CallToolAsync("parse_input", new { grammar = Arithmetic, input = "a + b * c", includeTree = true });
        var parse = result.GetProperty("parse");
        Assert.True(parse.GetProperty("success").GetBoolean());
        Assert.False(parse.GetProperty("treeTruncated").GetBoolean());
        var nodes = parse.GetProperty("tree").EnumerateArray().ToArray();
        Assert.NotEmpty(nodes);
        Assert.Equal(0, nodes[0].GetProperty("id").GetInt32());
        var ids = nodes.Select(node => node.GetProperty("id").GetInt32()).ToHashSet();
        foreach (var child in nodes.SelectMany(node => node.GetProperty("children").EnumerateArray()))
            Assert.Contains(child.GetInt32(), ids);
        var leaf = Assert.Single(nodes, node => node.TryGetProperty("text", out var text) && text.GetString() == "b");
        Assert.Equal(4, leaf.GetProperty("start").GetInt32());
        Assert.Equal(5, leaf.GetProperty("end").GetInt32());
    }

    [Theory]
    [InlineData("a +")]
    [InlineData("a @")]
    public async Task Invalid_input_including_unknown_characters_is_rejected(string input)
    {
        var result = await server.CallToolAsync("parse_input", new { grammar = Arithmetic, input });
        Assert.Equal("ok", result.GetProperty("status").GetString());
        var parse = result.GetProperty("parse");
        Assert.False(parse.GetProperty("success").GetBoolean());
        Assert.NotEmpty(parse.GetProperty("diagnostics").EnumerateArray());
    }

    [Theory]
    [InlineData("Start : 'a' ;", false)]
    [InlineData("Start : 'a'? ;", true)]
    public async Task Empty_input_is_decided_by_the_grammar(string grammar, bool expected)
    {
        var result = await server.CallToolAsync("parse_input", new { grammar, input = "" });
        Assert.Equal("ok", result.GetProperty("status").GetString());
        Assert.Equal(expected, result.GetProperty("parse").GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task Regression_cases_distinguish_expected_rejection_from_failed_expectations()
    {
        var result = await server.CallToolAsync("run_tests", new
        {
            grammar = Arithmetic,
            cases = new[]
            {
                new { name = "valid expression", input = "a + b * c", expectSuccess = true },
                new { name = "missing operand", input = "a +", expectSuccess = false },
                new { name = "incorrect expectation", input = "a +", expectSuccess = true }
            }
        });
        Assert.Equal("ok", result.GetProperty("status").GetString());
        var tests = result.GetProperty("tests");
        Assert.Equal(3, tests.GetProperty("total").GetInt32());
        Assert.Equal(2, tests.GetProperty("passed").GetInt32());
        Assert.Equal(1, tests.GetProperty("failed").GetInt32());
        var cases = tests.GetProperty("cases").EnumerateArray().ToArray();
        Assert.False(cases[1].GetProperty("actualSuccess").GetBoolean());
        Assert.True(cases[1].GetProperty("passed").GetBoolean());
        Assert.False(cases[2].GetProperty("passed").GetBoolean());
    }

    [Fact]
    public async Task Missing_test_expectation_is_an_error_not_an_implicit_negative_test()
    {
        var response = await server.RpcAsync("tools/call", new
        {
            name = "run_tests",
            arguments = new
            {
                grammar = Arithmetic,
                cases = new[] { new { name = "expectation omitted", input = "a +" } }
            }
        });
        Assert.True(response.TryGetProperty("error", out _) ||
            response.GetProperty("result").TryGetProperty("isError", out var isError) && isError.GetBoolean(),
            response.GetRawText());
    }

    [Fact]
    public async Task Resource_failure_does_not_pass_a_negative_test()
    {
        var result = await server.CallToolAsync("run_tests", new
        {
            grammar = "Start : 'a'+ ;",
            cases = new[]
            {
                new
                {
                    name = "too many tokens", input = string.Join(" ", Enumerable.Repeat("a", McpLimits.MaxTokens + 1)),
                    expectSuccess = false
                }
            }
        });
        Assert.Equal("runtime_limit", result.GetProperty("status").GetString());
        var tests = result.GetProperty("tests");
        Assert.Equal(0, tests.GetProperty("passed").GetInt32());
        Assert.Equal(1, tests.GetProperty("failed").GetInt32());
        var test = Assert.Single(tests.GetProperty("cases").EnumerateArray());
        Assert.False(test.GetProperty("passed").GetBoolean());
        Assert.Contains(test.GetProperty("diagnostics").EnumerateArray(), diagnostic =>
            diagnostic.GetProperty("code").GetString() == "runtime_limit");
    }

    [Fact]
    public async Task Table_conflicts_are_reported_and_prevent_parsing()
    {
        const string grammar = "Expr : Expr '+' Expr | id ; id := \"[a-zA-Z]+\" ;";
        var result = await server.CallToolAsync("parse_input", new { grammar, input = "a + b + c" });
        Assert.Equal("grammar_conflict", result.GetProperty("status").GetString());
        Assert.True(result.GetProperty("grammar").GetProperty("hasConflicts").GetBoolean());
        Assert.NotEmpty(result.GetProperty("grammar").GetProperty("conflicts").EnumerateArray());
        Assert.True(!result.TryGetProperty("parse", out var parse) || parse.ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task Oversized_input_is_rejected_before_compilation()
    {
        var result = await server.CallToolAsync("parse_input", new
        {
            grammar = "Start : 'a'+ ;", input = new string('a', McpLimits.MaxInputChars + 1)
        });
        Assert.Equal("invalid_request", result.GetProperty("status").GetString());
        Assert.True(!result.TryGetProperty("grammar", out var grammar) || grammar.ValueKind == JsonValueKind.Null);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("unknown-key")]
    public async Task Tools_require_authentication_but_health_remains_available(string? key)
    {
        using var client = server.CreateClient();
        if (key is not null) client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
        using var request = server.CreateRpcRequest("tools/list", new { });
        using var tools = await client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, tools.StatusCode);
        using var health = await client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, health.StatusCode);
    }

    [Fact]
    public async Task Untrusted_browser_origin_is_rejected_even_with_valid_credentials()
    {
        using var request = server.CreateRpcRequest("tools/list", new { });
        request.Headers.Add("Origin", "https://untrusted.example");
        using var response = await server.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Oversized_http_body_is_rejected_before_mcp_dispatch()
    {
        using var request = server.CreateRpcRequest("tools/list", new { });
        request.Content = new StringContent(new string('x', McpLimits.MaxRequestBytes + 1));
        using var response = await server.Client.SendAsync(request);
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Concurrent_different_grammars_have_isolated_parser_state()
    {
        var responses = await Task.WhenAll(
            server.CallToolAsync("parse_input", new { grammar = "First : 'a'? 'x' ;", input = "x", includeTree = true }),
            server.CallToolAsync("parse_input", new { grammar = "Second : 'b'? 'y' ;", input = "b y", includeTree = true }));
        var symbols = new[] { "First", "Second" };
        for (var index = 0; index < responses.Length; index++)
        {
            Assert.Equal("ok", responses[index].GetProperty("status").GetString());
            var parse = responses[index].GetProperty("parse");
            Assert.True(parse.GetProperty("success").GetBoolean());
            Assert.Contains(parse.GetProperty("tree").EnumerateArray(), node => node.GetProperty("symbol").GetString() == symbols[index]);
        }
    }
}
