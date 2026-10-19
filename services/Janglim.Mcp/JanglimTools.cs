using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Janglim.Mcp;

[McpServerToolType]
public sealed class JanglimTools(ProcessWorkerRunner runner)
{
    [McpServerTool(Name = "list_grammars", ReadOnly = true, Destructive = false,
        Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("List read-only built-in grammar baselines with IDs, versions, SHA-256 hashes, supported features and limitations. Use get_grammar to retrieve a baseline copy. Client edits never update these release assets; this service does not store user grammar projects.")]
    public GrammarListResponse ListGrammars() => GrammarCatalog.List();

    [McpServerTool(Name = "get_grammar", ReadOnly = true, Destructive = false,
        Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Retrieve an immutable built-in grammar baseline as EBNF text with version, scope and a SHA-256 hash of its LF-normalized UTF-8 text. c11-core is a C11 core syntax profile, not full C: no preprocessing, typedef-name resolution or semantic type checking. Edit your client-side copy and pass the edited text to validate_grammar, parse_input and run_tests. User requests cannot modify the server baseline. Supply a catalog ID, not a file path.")]
    public GrammarPresetResponse GetGrammar(
        [Description("Exact built-in grammar ID from list_grammars, e.g. c11-core.")] string? grammarId)
        => GrammarCatalog.Get(grammarId);

    [McpServerTool(Name = "validate_grammar", ReadOnly = true, Destructive = false,
        Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Validate EBNF syntax and compile a LALR(1) table. Reports grammar errors, state/rule counts, and shift/reduce or reduce/reduce conflicts. A table conflict is not proof of grammar ambiguity. First rule is the start rule; literals use single quotes, token regexes double quotes. Token rules optionally specify Identifier, StringLiteral, CharLiteral, NumberLiteral, Comment, or Reject before the regex; the default is Identifier. Comment tokens are skipped; Reject matches cause input diagnostics. Both are lexical-only and cannot be referenced in productions. Inside regex patterns double embedded double quotes and preserve backslashes. Maximum grammar: 8192 UTF-16 code units.")]
    public Task<WorkerResponse> ValidateGrammar(
        [Description("Janglim EBNF text, e.g. Expr : Expr '+' id | id ; id := \"[a-z]+\" ;")] string grammar,
        CancellationToken cancellationToken)
        => runner.ExecuteAsync(new("validate_grammar", grammar), cancellationToken);

    [McpServerTool(Name = "parse_input", ReadOnly = true, Destructive = false,
        Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Parse an input with a conflict-free Janglim EBNF grammar. Returns success, bounded tokens and positioned diagnostics; optionally a flat concrete syntax tree (root ID 0, children are IDs). Input offsets are UTF-16, end exclusive. Refuses conflicted grammars. No semantic evaluation or code generation. Maximum input: 16384 UTF-16 code units.")]
    public Task<WorkerResponse> ParseInput(
        [Description("Janglim EBNF grammar.")] string grammar,
        [Description("Text to parse, including an empty string to test empty input.")] string input,
        [Description("Include a flat syntax tree, up to 512 nodes, with truncation explicitly indicated.")] bool includeTree = false,
        CancellationToken cancellationToken = default)
        => runner.ExecuteAsync(new("parse_input", grammar, input, includeTree), cancellationToken);

    [McpServerTool(Name = "run_tests", ReadOnly = true, Destructive = false,
        Idempotent = true, OpenWorld = false, UseStructuredContent = true)]
    [Description("Check 1–16 named positive/negative examples against one conflict-free EBNF grammar. Each case supplies name, input and expectSuccess. Reports actual parsing success and whether it matched the expectation. Resource/worker failures are not passing negative tests. Combined input limit: 32768 UTF-16 code units. Does not read or write project files.")]
    public Task<WorkerResponse> RunTests(
        [Description("Janglim EBNF grammar.")] string grammar,
        [Description("Named examples: [{name: 'valid', input: 'a', expectSuccess: true}].")] GrammarTestCase[] cases,
        CancellationToken cancellationToken)
        => runner.ExecuteAsync(new("run_tests", grammar, Tests: cases), cancellationToken);
}
