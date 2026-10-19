using System.ComponentModel.DataAnnotations;

namespace Janglim.Mcp;

public sealed class McpServiceOptions
{
    public string? ApiKey { get; set; }
    public string[] AllowedOrigins { get; set; } = [];
    [Range(1, 4)] public int MaxConcurrentWorkers { get; set; } = 2;
    [Range(1, 30)] public int WorkerTimeoutSeconds { get; set; } = 10;
    [Range(1, 600)] public int RequestsPerMinute { get; set; } = 60;
    [Range(1, 16)] public int MaxConcurrentRequests { get; set; } = 4;
}
