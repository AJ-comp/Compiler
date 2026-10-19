using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Xunit;

namespace Janglim.Mcp.Tests;

/// <summary>Exercises the immutable C baseline through the actual MCP HTTP tool contract.</summary>
[Collection(McpCollection.Name)]
public sealed class GrammarPresetHttpTests(McpServerFixture server)
{
    private const string PresetId = "c11-core";
    private static string CanonicalGrammar => File.ReadAllText(
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "C", "c11-core.ebnf")).ReplaceLineEndings("\n");

    [Fact]
    public async Task Preset_tools_are_discoverable_read_only_closed_world_and_idempotent()
    {
        var response = await server.RpcAsync("tools/list", new { });
        var tools = response.GetProperty("result").GetProperty("tools").EnumerateArray()
            .ToDictionary(tool => tool.GetProperty("name").GetString()!, tool => tool);

        foreach (var name in new[] { "list_grammars", "get_grammar" })
        {
            Assert.True(tools.TryGetValue(name, out var tool));
            var annotations = tool.GetProperty("annotations");
            Assert.True(annotations.GetProperty("readOnlyHint").GetBoolean());
            Assert.False(annotations.GetProperty("destructiveHint").GetBoolean());
            Assert.True(annotations.GetProperty("idempotentHint").GetBoolean());
            Assert.False(annotations.GetProperty("openWorldHint").GetBoolean());
            var schema = tool.GetProperty("inputSchema");
            Assert.Equal("object", schema.GetProperty("type").GetString());
            Assert.True(!schema.TryGetProperty("properties", out var properties) ||
                !properties.TryGetProperty("grammar", out _));
        }

        var getSchema = tools["get_grammar"].GetProperty("inputSchema");
        Assert.Contains(getSchema.GetProperty("required").EnumerateArray(), item => item.GetString() == "grammarId");
        var idSchema = getSchema.GetProperty("properties").GetProperty("grammarId");
        var idType = idSchema.GetProperty("type");
        Assert.True(idType.ValueKind == JsonValueKind.String && idType.GetString() == "string" ||
            idType.ValueKind == JsonValueKind.Array &&
            idType.EnumerateArray().Any(item => item.GetString() == "string"));
    }

    [Fact]
    public async Task Listing_and_fetching_return_the_canonical_versioned_scoped_grammar_with_matching_hash()
    {
        var listed = await server.CallToolAsync("list_grammars", new { });
        Assert.Equal("ok", listed.GetProperty("status").GetString());
        var metadata = Assert.Single(listed.GetProperty("grammars").EnumerateArray());
        AssertMetadata(metadata);
        Assert.False(metadata.TryGetProperty("grammar", out _));

        var fetched = await server.CallToolAsync("get_grammar", new { grammarId = PresetId });
        Assert.Equal("ok", fetched.GetProperty("status").GetString());
        var preset = fetched.GetProperty("preset");
        AssertMetadata(preset);
        Assert.Equal(CanonicalGrammar, preset.GetProperty("grammar").GetString());
        foreach (var field in metadata.EnumerateObject())
            Assert.Equal(field.Value.GetRawText(), preset.GetProperty(field.Name).GetRawText());

        var source = preset.GetProperty("grammar").GetString()!;
        Assert.InRange(source.Length, 1, McpLimits.MaxGrammarChars);
        var computedHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source))).ToLowerInvariant();
        Assert.Equal(computedHash, preset.GetProperty("sha256").GetString());

        var compiled = await server.CallToolAsync("validate_grammar", new { grammar = source });
        Assert.Equal("ok", compiled.GetProperty("status").GetString());
        var report = compiled.GetProperty("grammar");
        Assert.True(report.GetProperty("isValid").GetBoolean());
        Assert.False(report.GetProperty("hasConflicts").GetBoolean());
        Assert.Empty(report.GetProperty("diagnostics").EnumerateArray());
        Assert.Empty(report.GetProperty("conflicts").EnumerateArray());
    }

    [Fact]
    public async Task Editing_a_fetched_copy_changes_only_that_request_and_other_clients_get_the_unchanged_baseline()
    {
        var fetched = await server.CallToolAsync("get_grammar", new { grammarId = PresetId });
        var baseline = fetched.GetProperty("preset");
        var source = baseline.GetProperty("grammar").GetString()!;
        const string before = "if_header : 'if' '(' expression ')' ;";
        const string after = "if_header : 'unless' '(' expression ')' ;";
        Assert.Contains(before, source);
        var edited = source.Replace(before, after, StringComparison.Ordinal);
        Assert.NotEqual(source, edited);

        var run = await server.CallToolAsync("run_tests", new
        {
            grammar = edited,
            cases = new[]
            {
                new { name = "new unless syntax", input = "int f(int x) { unless (x) return 1; else return 0; }", expectSuccess = true },
                new { name = "old if syntax rejected by edited copy", input = "int f(int x) { if (x) return 1; else return 0; }", expectSuccess = false },
                new { name = "other C syntax remains valid", input = "int f(int x) { while (x) { x--; } return x; }", expectSuccess = true }
            }
        });
        Assert.Equal("ok", run.GetProperty("status").GetString());
        var tests = run.GetProperty("tests");
        Assert.Equal(3, tests.GetProperty("passed").GetInt32());
        Assert.Equal(0, tests.GetProperty("failed").GetInt32());

        var fetchedAgain = await server.CallToolAsync("get_grammar", new { grammarId = PresetId });
        AssertBaselineEqual(baseline, fetchedAgain);

        // A separate authenticated HTTP client also observes the same immutable built-in resource.
        using var otherClient = server.CreateAuthenticatedClient();
        var otherResponse = await CallToolWithClientAsync(otherClient, "get_grammar", new { grammarId = PresetId });
        AssertBaselineEqual(baseline, otherResponse);

        var baselineRun = await server.CallToolAsync("run_tests", new
        {
            grammar = fetchedAgain.GetProperty("preset").GetProperty("grammar").GetString(),
            cases = new[]
            {
                new { name = "baseline retains if", input = "int f(int x) { if (x) return 1; else return 0; }", expectSuccess = true },
                new { name = "baseline does not adopt unless", input = "int f(int x) { unless (x) return 1; else return 0; }", expectSuccess = false }
            }
        });
        Assert.Equal("ok", baselineRun.GetProperty("status").GetString());
        Assert.Equal(2, baselineRun.GetProperty("tests").GetProperty("passed").GetInt32());
        Assert.Equal(0, baselineRun.GetProperty("tests").GetProperty("failed").GetInt32());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t\r\n")]
    [InlineData("C11-CORE")]
    [InlineData("c11-core ")]
    [InlineData("missing-preset")]
    [InlineData("../c11-core")]
    [InlineData("../../grammars/c/c11-core.ebnf")]
    [InlineData("C:\\Windows\\win.ini")]
    [InlineData("/etc/passwd")]
    public async Task Unknown_or_invalid_ids_return_a_generic_bounded_diagnostic_without_loading_files(string? grammarId)
    {
        var unknown = await server.CallToolAsync("get_grammar", new { grammarId = "unregistered-preset" });
        var response = await server.CallToolAsync("get_grammar", new { grammarId });
        Assert.Equal("unknown_grammar", response.GetProperty("status").GetString());
        Assert.True(!response.TryGetProperty("preset", out var preset) || preset.ValueKind == JsonValueKind.Null);
        var message = response.GetProperty("message").GetString()!;
        Assert.InRange(message.Length, 1, 512);
        Assert.Equal(unknown.GetProperty("message").GetString(), message);
    }

    private static void AssertMetadata(JsonElement metadata)
    {
        Assert.Equal(PresetId, metadata.GetProperty("id").GetString());
        Assert.Equal("1.0.0", metadata.GetProperty("version").GetString());
        Assert.Equal("C", metadata.GetProperty("language").GetString());
        Assert.Equal("C11", metadata.GetProperty("standard").GetString());
        Assert.False(string.IsNullOrWhiteSpace(metadata.GetProperty("name").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(metadata.GetProperty("description").GetString()));
        Assert.NotEmpty(metadata.GetProperty("supportedFeatures").EnumerateArray());
        var limitations = metadata.GetProperty("limitations").EnumerateArray()
            .Select(item => item.GetString()!).ToArray();
        Assert.NotEmpty(limitations);
        Assert.Contains(limitations, item => item.Contains("typedef", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(limitations, item => item.Contains("preprocess", StringComparison.OrdinalIgnoreCase));
        Assert.Matches("^[0-9a-f]{64}$", metadata.GetProperty("sha256").GetString()!);
    }

    private static void AssertBaselineEqual(JsonElement baseline, JsonElement response)
    {
        Assert.Equal("ok", response.GetProperty("status").GetString());
        var preset = response.GetProperty("preset");
        Assert.Equal(baseline.GetProperty("grammar").GetString(), preset.GetProperty("grammar").GetString());
        Assert.Equal(baseline.GetProperty("sha256").GetString(), preset.GetProperty("sha256").GetString());
        Assert.Equal(baseline.GetProperty("version").GetString(), preset.GetProperty("version").GetString());
    }

    private async Task<JsonElement> CallToolWithClientAsync(HttpClient client, string name, object arguments)
    {
        using var request = server.CreateRpcRequest("tools/call", new { name, arguments });
        using var response = await client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}: {body}");
        if (response.Content.Headers.ContentType?.MediaType == "text/event-stream")
        {
            var payload = body.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(frame => string.Join("\n", frame.Split('\n')
                    .Where(line => line.StartsWith("data:", StringComparison.Ordinal))
                    .Select(line => line[5..].TrimStart())))
                .First(item => item.Length > 0);
            body = payload;
        }
        using var document = JsonDocument.Parse(body);
        var envelope = document.RootElement;
        Assert.False(envelope.TryGetProperty("error", out _), envelope.GetRawText());
        var result = envelope.GetProperty("result");
        Assert.False(result.TryGetProperty("isError", out var isError) && isError.GetBoolean(), result.GetRawText());
        return result.GetProperty("structuredContent").Clone();
    }
}
