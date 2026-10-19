using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Janglim.Mcp.Tests;

public sealed class WorkerLifecycleTests
{
    private const string SlowGrammar = "Start : id ; id := \"(a|aa)+$\" ;";
    private static readonly WorkerRequest SlowRequest = new("parse_input", SlowGrammar, new string('a', 64) + "!");

    [Fact]
    public async Task Deadline_terminates_synchronous_regex_work_and_allows_the_next_job()
    {
        using var runner = CreateRunner();
        var response = await runner.ExecuteAsync(SlowRequest, CancellationToken.None)
            .WaitAsync(TimeSpan.FromSeconds(10));
        Assert.Equal("runtime_limit", response.Status);
        var next = await runner.ExecuteAsync(new("parse_input", "Start : 'a' ;", "a"), CancellationToken.None);
        Assert.Equal("ok", next.Status);
        Assert.True(next.Parse!.Success);
    }

    [Fact]
    public async Task Cancellation_stops_the_worker_and_releases_its_slot()
    {
        using var runner = CreateRunner();
        using var cancellation = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => runner.ExecuteAsync(SlowRequest, cancellation.Token));
        var next = await runner.ExecuteAsync(new("parse_input", "Start : 'b' ;", "b"), CancellationToken.None);
        Assert.Equal("ok", next.Status);
        Assert.True(next.Parse!.Success);
    }

    private static ProcessWorkerRunner CreateRunner() => new(
        Options.Create(new McpServiceOptions { MaxConcurrentWorkers = 1, WorkerTimeoutSeconds = 2 }),
        NullLogger<ProcessWorkerRunner>.Instance);
}
