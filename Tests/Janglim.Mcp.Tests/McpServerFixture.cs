using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Janglim.Mcp.Tests;

public sealed class McpServerFixture : WebApplicationFactory<global::Program>, IAsyncLifetime
{
    // Deliberately public test data, never a deployment credential.
    public const string ApiKey = "janglim-mcp-integration-test-key-0000000000000000";
    public const string ProtocolVersion = "2025-06-18";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private long _requestId;

    public HttpClient Client { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Production");
        builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["AllowedHosts"] = "localhost",
                ["JanglimMcp:ApiKey"] = ApiKey,
                ["JanglimMcp:AllowedOrigins:0"] = "https://janglim.mythosia.net",
                ["JanglimMcp:RequestsPerMinute"] = "600",
                ["JanglimMcp:MaxConcurrentRequests"] = "4",
                ["JanglimMcp:MaxConcurrentWorkers"] = "2",
                ["JanglimMcp:WorkerTimeoutSeconds"] = "20"
            }));
    }

    public async Task InitializeAsync()
    {
        Client = CreateAuthenticatedClient();
        var response = await RpcAsync("initialize", new
        {
            protocolVersion = ProtocolVersion,
            capabilities = new { },
            clientInfo = new { name = "janglim-integration-tests", version = "1.0" }
        });
        Assert.Equal(ProtocolVersion, response.GetProperty("result").GetProperty("protocolVersion").GetString());

        using var request = CreateRpcRequest("notifications/initialized", new { }, includeId: false);
        using var initialized = await Client.SendAsync(request);
        initialized.EnsureSuccessStatusCode();
    }

    Task IAsyncLifetime.DisposeAsync()
    {
        Client?.Dispose();
        Dispose();
        return Task.CompletedTask;
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost"), AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);
        return client;
    }

    public HttpRequestMessage CreateRpcRequest(string method, object parameters, bool includeId = true)
    {
        var envelope = new Dictionary<string, object?>
        {
            ["jsonrpc"] = "2.0", ["method"] = method, ["params"] = parameters
        };
        if (includeId) envelope["id"] = Interlocked.Increment(ref _requestId);
        var request = new HttpRequestMessage(HttpMethod.Post, "/mcp")
        {
            Content = new StringContent(JsonSerializer.Serialize(envelope, JsonOptions), Encoding.UTF8, "application/json")
        };
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));
        request.Headers.Add("MCP-Protocol-Version", ProtocolVersion);
        return request;
    }

    public async Task<JsonElement> RpcAsync(string method, object parameters)
    {
        using var request = CreateRpcRequest(method, parameters);
        using var response = await Client.SendAsync(request);
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.IsSuccessStatusCode, $"HTTP {(int)response.StatusCode}: {body}");

        if (response.Content.Headers.ContentType?.MediaType == "text/event-stream")
        {
            var payloads = body.Replace("\r\n", "\n", StringComparison.Ordinal)
                .Split("\n\n", StringSplitOptions.RemoveEmptyEntries)
                .Select(frame => string.Join("\n", frame.Split('\n')
                    .Where(line => line.StartsWith("data:", StringComparison.Ordinal))
                    .Select(line => line[5..].TrimStart())))
                .Where(payload => payload.Length > 0);
            foreach (var payload in payloads)
            {
                using var eventDocument = JsonDocument.Parse(payload);
                if (eventDocument.RootElement.TryGetProperty("id", out _))
                    return eventDocument.RootElement.Clone();
            }
            throw new Xunit.Sdk.XunitException($"No JSON-RPC response in SSE stream: {body}");
        }

        using var document = JsonDocument.Parse(body);
        return document.RootElement.Clone();
    }

    public async Task<JsonElement> CallToolAsync(string name, object arguments)
    {
        var envelope = await RpcAsync("tools/call", new { name, arguments });
        Assert.False(envelope.TryGetProperty("error", out _), envelope.GetRawText());
        var result = envelope.GetProperty("result");
        Assert.False(result.TryGetProperty("isError", out var isError) && isError.GetBoolean(), result.GetRawText());
        var structured = result.GetProperty("structuredContent");
        Assert.Equal(JsonValueKind.Object, structured.ValueKind);
        return structured;
    }
}

[CollectionDefinition(Name)]
public sealed class McpCollection : ICollectionFixture<McpServerFixture>
{
    public const string Name = "MCP HTTP integration";
}
