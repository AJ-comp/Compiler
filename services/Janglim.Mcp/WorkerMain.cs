using System.Text;
using System.Text.Json;
using Janglim.Mcp.Engine;

namespace Janglim.Mcp;

public static class WorkerMain
{
    public static async Task<int> RunAsync()
    {
        var output = Console.Out;
        var errorOutput = Console.Error;
        Console.SetOut(TextWriter.Null);
        Console.SetError(TextWriter.Null);
        WorkerResponse response;
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web) { MaxDepth = 32 };
        try
        {
            var text = await ReadRequestAsync();
            if (text is null)
                response = new("invalid_request", "The worker request exceeds the size limit.");
            else
            {
                var request = JsonSerializer.Deserialize<WorkerRequest>(text, options);
                response = request is null ? new("invalid_request", "A JSON request is required.") :
                    EngineExecutor.Execute(request);
            }
        }
        catch (Exception exception) when (exception is JsonException or DecoderFallbackException)
        {
            response = new("invalid_request", "The worker request must be valid JSON.");
        }
        catch
        {
            response = new("worker_error", "The worker could not complete this request.");
        }

        try
        {
            var json = JsonSerializer.Serialize(response, options);
            if (json.Length > McpLimits.MaxWorkerOutputChars)
                json = JsonSerializer.Serialize(new WorkerResponse("output_limit",
                    "The result exceeds the response size limit."), options);
            await output.WriteLineAsync(json);
            await output.FlushAsync();
            return 0;
        }
        catch
        {
            // A closed pipe cannot carry a response; the host treats this as a worker failure.
            return 1;
        }
        finally
        {
            Console.SetOut(output);
            Console.SetError(errorOutput);
        }
    }

    private static async Task<string?> ReadRequestAsync()
    {
        var text = new StringBuilder();
        var buffer = new char[4096];
        using var input = new StreamReader(Console.OpenStandardInput(), new UTF8Encoding(false, true),
            detectEncodingFromByteOrderMarks: true, bufferSize: 4096, leaveOpen: true);
        int read;
        while ((read = await input.ReadAsync(buffer.AsMemory())) > 0)
        {
            if (text.Length + read > McpLimits.MaxRequestBytes) return null;
            text.Append(buffer, 0, read);
        }
        var request = text.ToString();
        return Encoding.UTF8.GetByteCount(request) > McpLimits.MaxRequestBytes ? null : request;
    }
}
