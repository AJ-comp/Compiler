using System.Text.Json;
using Xunit;

namespace Janglim.Mcp.Tests;

/// <summary>Verifies the canonical C profile through MCP HTTP and the real isolated worker.</summary>
[Collection(McpCollection.Name)]
public sealed class C11CoreHttpTests(McpServerFixture server)
{
    private static string Grammar => File.ReadAllText(FixturePath("c11-core.ebnf"));

    [Fact]
    public async Task Canonical_C_profile_compiles_without_conflicts_within_service_limits()
    {
        var source = Grammar;
        Assert.InRange(source.Length, 1, McpLimits.MaxGrammarChars);
        var result = await server.CallToolAsync("validate_grammar", new { grammar = source });

        AssertCompiledGrammar(result);
    }

    [Fact]
    public async Task C_literals_remain_whole_positioned_tokens_and_comments_are_skipped()
    {
        const string input = """
            /* heading */ char *s = u8"hi😀/*text*/"; // note
            int c = L'\x41'; double n = .5;
            """;
        var result = await server.CallToolAsync("parse_input", new
        {
            grammar = Grammar, input, includeTree = true
        });

        AssertCompiledGrammar(result);
        var parse = result.GetProperty("parse");
        Assert.True(parse.GetProperty("success").GetBoolean());
        Assert.Empty(parse.GetProperty("diagnostics").EnumerateArray());
        Assert.False(parse.GetProperty("tokensTruncated").GetBoolean());
        Assert.False(parse.GetProperty("treeTruncated").GetBoolean());
        var tokens = parse.GetProperty("tokens").EnumerateArray().ToArray();
        Assert.Equal(tokens.Length, parse.GetProperty("tokenCount").GetInt32());
        Assert.DoesNotContain(tokens, token => token.GetProperty("symbol").GetString() == "comment");
        AssertTokenSpan(tokens, input, "string_literal", "u8\"hi😀/*text*/\"");
        AssertTokenSpan(tokens, input, "character_constant", "L'\\x41'");
        AssertTokenSpan(tokens, input, "number", ".5");
        Assert.All(tokens, token =>
        {
            var start = token.GetProperty("start").GetInt32();
            var end = token.GetProperty("end").GetInt32();
            Assert.Equal(input[start..end], token.GetProperty("text").GetString());
        });
        var nodes = parse.GetProperty("tree").EnumerateArray().ToArray();
        Assert.Equal("translation_unit", nodes[0].GetProperty("symbol").GetString());
        foreach (var text in new[] { "u8\"hi😀/*text*/\"", "L'\\x41'", ".5" })
        {
            var leaf = Assert.Single(nodes, node => node.GetProperty("kind").GetString() == "terminal" &&
                node.GetProperty("text").GetString() == text);
            var token = Assert.Single(tokens, token => token.GetProperty("text").GetString() == text);
            Assert.Equal(token.GetProperty("start").GetInt32(), leaf.GetProperty("start").GetInt32());
            Assert.Equal(token.GetProperty("end").GetInt32(), leaf.GetProperty("end").GetInt32());
        }
    }

    [Fact]
    public async Task C_tree_binds_else_to_the_nearest_unbraced_if()
    {
        const string input = "int f(int a, int b) { if (a) if (b) return 1; else return 2; return 3; }";
        var result = await server.CallToolAsync("parse_input", new
        {
            grammar = Grammar, input, includeTree = true
        });

        AssertCompiledGrammar(result);
        var parse = result.GetProperty("parse");
        Assert.True(parse.GetProperty("success").GetBoolean());
        Assert.Empty(parse.GetProperty("diagnostics").EnumerateArray());
        Assert.False(parse.GetProperty("treeTruncated").GetBoolean());
        var nodes = parse.GetProperty("tree").EnumerateArray().ToArray();
        var outer = Assert.Single(nodes, node => HasSpan(node, input, "open_statement",
            "if (a) if (b) return 1; else return 2;"));
        var inner = Assert.Single(nodes, node => HasSpan(node, input, "closed_statement",
            "if (b) return 1; else return 2;"));
        Assert.Contains(inner.GetProperty("id").GetInt32(), DescendantIds(nodes, outer));
        Assert.DoesNotContain(nodes, node => HasSpan(node, input, "closed_statement",
            "if (a) if (b) return 1; else return 2;"));
    }

    [Theory]
    [InlineData("int f(void) { return 1/*x; }")]
    [InlineData("int value; /* unterminated")]
    public async Task Unterminated_block_comments_are_positioned_lexical_errors_instead_of_operators(string input)
    {
        var result = await server.CallToolAsync("parse_input", new
        {
            grammar = Grammar, input, includeTree = true
        });

        AssertCompiledGrammar(result);
        var parse = result.GetProperty("parse");
        Assert.False(parse.GetProperty("success").GetBoolean());
        var error = Assert.Single(parse.GetProperty("diagnostics").EnumerateArray());
        Assert.Equal("unrecognized_input", error.GetProperty("code").GetString());
        Assert.Equal(input.IndexOf("/*", StringComparison.Ordinal), error.GetProperty("start").GetInt32());
        Assert.Equal(input.Length, error.GetProperty("end").GetInt32());
        Assert.True(!parse.TryGetProperty("tree", out var tree) || tree.ValueKind == JsonValueKind.Null);
    }

    [Fact]
    public async Task Entire_canonical_C_corpus_passes_in_bounded_real_worker_batches()
    {
        using var corpus = JsonDocument.Parse(File.ReadAllText(FixturePath("c11-core.cases.json")));
        var cases = corpus.RootElement.EnumerateArray().ToArray();
        Assert.NotEmpty(cases);
        Assert.Contains(cases, test => test.GetProperty("expectSuccess").GetBoolean());
        Assert.Contains(cases, test => !test.GetProperty("expectSuccess").GetBoolean());
        Assert.Equal(cases.Length, cases.Select(test => test.GetProperty("name").GetString())
            .Distinct(StringComparer.Ordinal).Count());
        var source = Grammar;
        var evaluated = 0;
        foreach (var batch in cases.Chunk(McpLimits.MaxTests))
        {
            Assert.InRange(batch.Length, 1, McpLimits.MaxTests);
            var result = await server.CallToolAsync("run_tests", new { grammar = source, cases = batch });

            // A timeout/resource/worker failure must fail this assertion, even for negative cases.
            AssertCompiledGrammar(result);
            var tests = result.GetProperty("tests");
            Assert.Equal(batch.Length, tests.GetProperty("total").GetInt32());
            Assert.Equal(batch.Length, tests.GetProperty("passed").GetInt32());
            Assert.Equal(0, tests.GetProperty("failed").GetInt32());
            var reports = tests.GetProperty("cases").EnumerateArray().ToArray();
            Assert.Equal(batch.Length, reports.Length);
            for (var index = 0; index < batch.Length; index++)
            {
                var expected = batch[index].GetProperty("expectSuccess").GetBoolean();
                var report = reports[index];
                Assert.Equal(batch[index].GetProperty("name").GetString(), report.GetProperty("name").GetString());
                Assert.Equal(expected, report.GetProperty("expectSuccess").GetBoolean());
                Assert.Equal(expected, report.GetProperty("actualSuccess").GetBoolean());
                Assert.True(report.GetProperty("passed").GetBoolean(), report.GetRawText());
                var diagnostics = report.GetProperty("diagnostics").EnumerateArray().ToArray();
                Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.GetProperty("code").GetString()
                    is "runtime_limit" or "worker_error" or "output_limit" or "busy" or "invalid_grammar");
                if (expected) Assert.Empty(diagnostics);
                else Assert.NotEmpty(diagnostics);
            }
            evaluated += reports.Length;
        }
        Assert.Equal(cases.Length, evaluated);
    }

    private static void AssertCompiledGrammar(JsonElement result)
    {
        Assert.Equal("ok", result.GetProperty("status").GetString());
        var report = result.GetProperty("grammar");
        Assert.True(report.GetProperty("isValid").GetBoolean());
        Assert.False(report.GetProperty("hasConflicts").GetBoolean());
        Assert.InRange(report.GetProperty("stateCount").GetInt32(), 1, McpLimits.MaxStates);
        Assert.InRange(report.GetProperty("ruleCount").GetInt32(), 1, McpLimits.MaxRules);
        Assert.Empty(report.GetProperty("conflicts").EnumerateArray());
        Assert.Empty(report.GetProperty("diagnostics").EnumerateArray());
    }

    private static void AssertTokenSpan(JsonElement[] tokens, string input, string symbol, string text)
    {
        var token = Assert.Single(tokens, item => item.GetProperty("text").GetString() == text);
        var start = input.IndexOf(text, StringComparison.Ordinal);
        Assert.Equal(symbol, token.GetProperty("symbol").GetString());
        Assert.Equal(start, token.GetProperty("start").GetInt32());
        Assert.Equal(start + text.Length, token.GetProperty("end").GetInt32());
    }

    private static bool HasSpan(JsonElement node, string input, string symbol, string text)
        => node.GetProperty("symbol").GetString() == symbol &&
            node.GetProperty("start").ValueKind == JsonValueKind.Number &&
            node.GetProperty("end").ValueKind == JsonValueKind.Number &&
            input[node.GetProperty("start").GetInt32()..node.GetProperty("end").GetInt32()] == text;

    private static HashSet<int> DescendantIds(JsonElement[] nodes, JsonElement parent)
    {
        var byId = nodes.ToDictionary(node => node.GetProperty("id").GetInt32());
        var found = new HashSet<int>();
        var pending = new Stack<int>(parent.GetProperty("children").EnumerateArray().Select(id => id.GetInt32()));
        while (pending.TryPop(out var id))
        {
            if (!found.Add(id)) continue;
            foreach (var child in byId[id].GetProperty("children").EnumerateArray())
                pending.Push(child.GetInt32());
        }
        return found;
    }

    private static string FixturePath(string name)
        => Path.Combine(AppContext.BaseDirectory, "Fixtures", "C", name);
}
