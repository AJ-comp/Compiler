using System.Text.Json.Serialization;

namespace Janglim.Mcp;

public sealed record WorkerRequest(string Operation, string Grammar, string? Input = null,
    bool IncludeTree = false, GrammarTestCase[]? Tests = null);

public sealed record GrammarTestCase([property: JsonRequired] string Name,
    [property: JsonRequired] string Input, [property: JsonRequired] bool ExpectSuccess);

public sealed record WorkerResponse(string Status, string? Message = null,
    GrammarReport? Grammar = null, ParseReport? Parse = null, TestRunReport? Tests = null);

public sealed record GrammarReport(bool IsValid, bool HasConflicts, int StateCount, int RuleCount,
    ConflictInfo[] Conflicts, DiagnosticInfo[] Diagnostics, bool ConflictsTruncated = false);

/// <summary>Input offsets are UTF-16 code units; End is exclusive.</summary>
public sealed record DiagnosticInfo(string Code, string Message, int? Start = null, int? End = null);
public sealed record ConflictInfo(int State, string Kind, string? Symbol);

public sealed record ParseReport(bool Success, int TokenCount, DiagnosticInfo[] Diagnostics,
    TokenInfo[] Tokens, bool TokensTruncated, TreeNodeInfo[]? Tree = null, bool TreeTruncated = false);
public sealed record TokenInfo(string Symbol, string Text, int Start, int End);

/// <summary>A flat concrete syntax tree, rooted at node 0, with child node IDs.</summary>
public sealed record TreeNodeInfo(int Id, string Kind, string Symbol, string? Text,
    int? Start, int? End, int[] Children);

public sealed record TestRunReport(int Total, int Passed, int Failed, TestCaseReport[] Cases);
public sealed record TestCaseReport(string Name, bool ExpectSuccess, bool ActualSuccess,
    bool Passed, DiagnosticInfo[] Diagnostics);

public static class McpLimits
{
    public const int MaxRequestBytes = 128 * 1024;
    public const int MaxGrammarChars = 8192;
    public const int MaxInputChars = 16384;
    public const int MaxTests = 16;
    public const int MaxTestInputChars = 32768;
    public const int MaxTokens = 2048;
    public const int MaxStates = 512;
    public const int MaxRules = 512;
    public const int MaxTreeNodes = 512;
    public const int MaxReturnedTokens = 128;
    public const int MaxDiagnostics = 16;
    public const int MaxMessageChars = 512;
    public const int MaxWorkerOutputChars = 512 * 1024;
}
