using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace Janglim.Mcp;

/// <summary>
/// Each job runs in a fresh process: legacy grammar globals cannot cross requests,
/// and cancellation terminates actual engine work, including synchronous regex/table generation.
/// </summary>
public sealed class ProcessWorkerRunner : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    private readonly McpServiceOptions _options;
    private readonly SemaphoreSlim _slots;
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 64 });
    private readonly ILogger<ProcessWorkerRunner> _logger;

    public ProcessWorkerRunner(IOptions<McpServiceOptions> options, ILogger<ProcessWorkerRunner> logger)
    {
        _options = options.Value;
        _slots = new SemaphoreSlim(_options.MaxConcurrentWorkers);
        _logger = logger;
    }

    public async Task<WorkerResponse> ExecuteAsync(WorkerRequest request, CancellationToken cancellationToken)
    {
        var invalid = RequestInputValidator.Validate(request);
        if (invalid is not null) return invalid;
        cancellationToken.ThrowIfCancellationRequested();

        var payload = JsonSerializer.Serialize(request, JsonOptions);
        var key = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
        if (_cache.TryGetValue(key, out WorkerResponse? cached) && cached is not null) return cached;
        if (!await _slots.WaitAsync(0, cancellationToken))
            return new("busy", "All parser workers are busy. Retry after a short delay.");

        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(_options.WorkerTimeoutSeconds));
        using var process = new Process { StartInfo = CreateStartInfo() };
        try
        {
            if (!process.Start()) return Failed();
            var outputTask = ReadBoundedAsync(process.StandardOutput, McpLimits.MaxWorkerOutputChars, deadline.Token);
            var errorTask = ReadBoundedAsync(process.StandardError, 16 * 1024, deadline.Token);
            await process.StandardInput.WriteAsync(payload.AsMemory(), deadline.Token);
            process.StandardInput.Close();
            // Observe every pipe as it completes, including an overflow after the other pipe closes.
            var pending = new List<Task> { process.WaitForExitAsync(deadline.Token), outputTask, errorTask };
            while (pending.Count > 0)
            {
                var completed = await Task.WhenAny(pending);
                await completed;
                pending.Remove(completed);
            }

            if (process.ExitCode != 0) return Failed();
            var output = await outputTask;
            var response = JsonSerializer.Deserialize<WorkerResponse>(output, JsonOptions) ?? Failed();
            // Cache immutable response DTOs, never mutable parser/grammar instances.
            if (response.Status == "ok" && output.Length <= 64 * 1024)
                _cache.Set(key, response, new MemoryCacheEntryOptions
                {
                    Size = 1,
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                });
            return response;
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return new("runtime_limit", $"Parser work exceeded {_options.WorkerTimeoutSeconds} seconds and was terminated.");
        }
        catch (OperationCanceledException) { throw; }
        catch (WorkerOutputLimitException)
        {
            return new("output_limit", "Parser output exceeded the service limit.");
        }
        catch (Exception exception)
        {
            // Log the failure class only: grammar/input, process output, and credentials are private.
            _logger.LogWarning("Parser worker failed ({FailureType})", exception.GetType().Name);
            return Failed();
        }
        finally
        {
            // HTTP cancellation/timeouts release a slot only after the engine has stopped.
            // If OS-level termination fails, keep the slot occupied rather than admit more work.
            if (await EnsureStoppedAsync(process)) _slots.Release();
        }
    }

    private static ProcessStartInfo CreateStartInfo()
    {
        var start = new ProcessStartInfo
        {
            FileName = "dotnet",
            UseShellExecute = false,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            StandardInputEncoding = new UTF8Encoding(false),
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
        start.ArgumentList.Add(typeof(ProcessWorkerRunner).Assembly.Location);
        start.ArgumentList.Add("--worker");
        // Workers need the runtime and assemblies, not host credentials or user environment.
        start.Environment.TryGetValue("PATH", out var path);
        start.Environment.TryGetValue("SystemRoot", out var systemRoot);
        start.Environment.TryGetValue("TEMP", out var temp);
        start.Environment.TryGetValue("TMP", out var tmp);
        start.Environment.Clear();
        if (path is not null) start.Environment["PATH"] = path;
        if (systemRoot is not null) start.Environment["SystemRoot"] = systemRoot;
        if (temp is not null) start.Environment["TEMP"] = temp;
        if (tmp is not null) start.Environment["TMP"] = tmp;
        start.Environment["DOTNET_GCHeapHardLimit"] = "10000000"; // hexadecimal: 256 MiB managed heap
        start.Environment["DOTNET_EnableDiagnostics"] = "0";
        return start;
    }

    private static async Task<string> ReadBoundedAsync(StreamReader reader, int limit, CancellationToken token)
    {
        var result = new StringBuilder();
        var buffer = new char[4096];
        int count;
        while ((count = await reader.ReadAsync(buffer.AsMemory(), token)) != 0)
        {
            if (result.Length + count > limit) throw new WorkerOutputLimitException();
            result.Append(buffer, 0, count);
        }
        return result.ToString();
    }

    private static WorkerResponse Failed() => new("worker_error", "Parser worker could not complete this request.");

    private async Task<bool> EnsureStoppedAsync(Process process)
    {
        try
        {
            if (process.HasExited) return true;
            try { process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) { /* The worker exited between checking and killing. */ }
            catch (System.ComponentModel.Win32Exception)
            {
                // A tree enumeration can race with process exit. Check termination below.
            }
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(2));
            return true;
        }
        catch (InvalidOperationException) { return true; /* The process never started. */ }
        catch (Exception exception)
        {
            _logger.LogError("Parser worker termination failed ({FailureType}); its slot remains occupied",
                exception.GetType().Name);
            return false;
        }
    }

    public void Dispose() { _cache.Dispose(); _slots.Dispose(); }
    private sealed class WorkerOutputLimitException : Exception;
}
