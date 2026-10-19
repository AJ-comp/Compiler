using Janglim.FrontEnd;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.RegularGrammar;
using Janglim.FrontEnd.Tokenize;

namespace Janglim.Mcp.Engine;

/// <summary>Executes one bounded request inside a disposable worker process.</summary>
public static class EngineExecutor
{
    public static WorkerResponse Execute(WorkerRequest request)
    {
        if (request is null) return new("invalid_request", "A request is required.");
        if (RequestInputValidator.Validate(request) is { } invalid) return invalid;

        try
        {
            var read = EbnfGrammarReader.Read(request.Grammar);
            if (!read.Success)
            {
                var errors = read.Errors.Take(McpLimits.MaxDiagnostics)
                    .Select(message => new DiagnosticInfo("invalid_grammar", SafeGrammarMessage(message)))
                    .ToArray();
                return new("invalid_grammar", "The EBNF grammar could not be read.",
                    new(false, false, 0, 0, [], errors));
            }

            var grammar = read.Grammar;
            if (grammar.NonTerminalSingles.Count > McpLimits.MaxRules)
                return Limit("The normalized grammar exceeds the production limit.");

            var lexer = new Lexer();
            try
            {
                foreach (var terminal in grammar.TerminalSet) lexer.AddTokenRule(terminal);
            }
            catch (ArgumentException exception)
            {
                return new("invalid_grammar", "A token pattern is invalid.",
                    new(false, false, 0, grammar.NonTerminalSingles.Count, [],
                        [new("invalid_regex", Clip(exception.Message))]));
            }

            var parser = new LALRParser(grammar, bLogging: false);
            var stateCount = parser.Canonical.IndexStateDic.Count;
            var ruleCount = grammar.NonTerminalSingles.Count;
            if (stateCount > McpLimits.MaxStates || ruleCount > McpLimits.MaxRules)
                return Limit("The compiled grammar exceeds the state or production limit.");

            var detectedConflicts = parser.Conflicts;
            var conflicts = detectedConflicts.Take(McpLimits.MaxDiagnostics)
                .Select(conflict => new ConflictInfo(conflict.CanonicalLine.CurrentStatusIndex,
                    Clip(conflict.AmbiguityContent),
                    Clip(conflict.CanonicalLine.SeeingMarkSymbol?.ToString())))
                .ToArray();
            var report = new GrammarReport(true, detectedConflicts.Count > 0, stateCount, ruleCount,
                conflicts, [], detectedConflicts.Count > conflicts.Length);

            if (request.Operation == "validate_grammar") return new("ok", Grammar: report);
            if (report.HasConflicts)
                return new("grammar_conflict", "Resolve the grammar conflicts before parsing.", report);

            if (request.Operation == "parse_input")
            {
                var outcome = Parse(grammar, parser, lexer, request.Input!, request.IncludeTree);
                return new(outcome.Status, outcome.Message, report, outcome.Report);
            }

            var cases = new List<TestCaseReport>(request.Tests!.Length);
            var status = "ok";
            foreach (var test in request.Tests)
            {
                var outcome = Parse(grammar, parser, lexer, test.Input, includeTree: false);
                var completed = outcome.Status == "ok";
                var passed = completed && outcome.Report.Success == test.ExpectSuccess;
                cases.Add(new(test.Name, test.ExpectSuccess, outcome.Report.Success, passed,
                    outcome.Report.Diagnostics));
                // A resource or worker failure is never a successful negative test.
                if (!completed && status == "ok") status = outcome.Status;
            }

            var passedCount = cases.Count(test => test.Passed);
            return new(status, status == "ok" ? null : "Some cases could not be evaluated normally.",
                report, Tests: new(cases.Count, passedCount, cases.Count - passedCount, cases.ToArray()));
        }
        catch
        {
            // Do not expose exception details, stack traces, or host paths through the public contract.
            return new("worker_error", "The parser could not complete this request.");
        }
    }

    private static ParseOutcome Parse(EbnfGrammar grammar, LALRParser parser, Lexer lexer, string input, bool includeTree)
    {
        try
        {
            var lexed = lexer.Lexing(input);
            var tokens = lexed.TokensForParsing;
            if (tokens.Count > McpLimits.MaxTokens || lexed.TokensForView.Count > McpLimits.MaxTokens)
                return ParseFailure("runtime_limit", "The input exceeds the token limit.", tokens.Count);

            var returnedTokens = tokens.Take(McpLimits.MaxReturnedTokens)
                .Select(token => new TokenInfo(TokenSymbol(token.PatternInfo?.Terminal), token.Data,
                    token.StartIndex, token.StartIndex + token.Data.Length))
                .ToArray();
            var tokensTruncated = tokens.Count > returnedTokens.Length;
            var lexicalErrors = new List<DiagnosticInfo>();
            var invalidPattern = false;
            foreach (var token in lexed.TokensForView)
            {
                var terminal = token.PatternInfo?.Terminal;
                if (token.Data.Length == 0)
                {
                    invalidPattern = true;
                    AddDiagnostic(lexicalErrors, new("zero_length_token",
                        "A token pattern matched an empty string.", token.StartIndex, token.StartIndex));
                }
                else if (terminal is null or NotDefined || grammar.IsRejectedToken(terminal))
                    AddDiagnostic(lexicalErrors, new("unrecognized_input",
                        "The input contains invalid or unrecognized text.",
                        token.StartIndex, token.StartIndex + token.Data.Length));
                else if (terminal is EndMarker || token.Data == new EndMarker().Value)
                    AddDiagnostic(lexicalErrors, new("reserved_end_marker",
                        "The engine's end marker cannot be used as an input token.",
                        token.StartIndex, token.StartIndex + token.Data.Length));
            }
            if (lexicalErrors.Count > 0)
                return new(invalidPattern ? "invalid_grammar" : "ok", null,
                    new(false, tokens.Count, lexicalErrors.ToArray(), returnedTokens, tokensTruncated));

            // LRParser short-circuits zero tokens as a success. Lex a synthetic EOF so the actual
            // table decides whether the start symbol accepts empty input. The engine appends its
            // own EOF as well; only the first marker is expected to be processed in this branch.
            var emptyInput = tokens.Count == 0;
            var parsingTokens = emptyInput ? lexer.Lexing(new EndMarker().Value).TokensForParsing : tokens;
            var previousException = parser.FiredException;
            var result = parser.Parsing(parsingTokens);
            var diagnostics = ParseDiagnostics(result, input);
            if (parser.FiredException is not ParsingException && parser.FiredException is { } exception &&
                !ReferenceEquals(exception, previousException))
                return ParseFailure("worker_error", "The parser could not evaluate this input.",
                    tokens.Count, returnedTokens, tokensTruncated);
            var accept = FindAccept(result, emptyInput ? 0 : result.Count - 1);
            var success = result.Success && diagnostics.Count == 0 && accept is not null;

            if (!success)
            {
                if (diagnostics.Count == 0 && result.Success)
                    return ParseFailure("runtime_limit", "Parsing stopped before accepting the complete input.",
                        tokens.Count, returnedTokens, tokensTruncated);
                if (diagnostics.Count == 0)
                    diagnostics.Add(new("parse_error", "The input does not match the grammar.",
                        input.Length, input.Length));
            }

            TreeNodeInfo[]? tree = null;
            var treeTruncated = false;
            if (success && includeTree)
            {
                var root = accept!.AfterStack.Stack.OfType<ParseTreeSymbol>().FirstOrDefault();
                if (root is not null) (tree, treeTruncated) = ProjectTree(root, input);
                else tree = [];
            }

            return new("ok", null, new(success, tokens.Count, diagnostics.ToArray(), returnedTokens,
                tokensTruncated, tree, treeTruncated));
        }
        catch
        {
            return ParseFailure("worker_error", "The parser could not evaluate this input.", 0);
        }
    }

    private static ParsingUnit? FindAccept(ParsingResult result, int blockIndex)
    {
        if (blockIndex < 0 || blockIndex >= result.Count) return null;
        return result[blockIndex].Units.LastOrDefault(unit => unit.Action.Action is ParseAction.Accept);
    }

    private static List<DiagnosticInfo> ParseDiagnostics(ParsingResult result, string input)
    {
        var diagnostics = new List<DiagnosticInfo>();
        foreach (var block in result)
        {
            foreach (var error in block.ErrorInfos)
            {
                var token = error.ErrTokens.FirstOrDefault();
                var (start, end) = TokenRange(token, input);
                if (token?.Kind is EndMarker || token?.StartIndex < 0) start = end = input.Length;
                AddDiagnostic(diagnostics, new(Clip(error.Code) ?? "parse_error", Clip(error.Message), start, end));
                if (diagnostics.Count == McpLimits.MaxDiagnostics) return diagnostics;
            }
        }
        return diagnostics;
    }

    private static (TreeNodeInfo[], bool) ProjectTree(ParseTreeSymbol root, string input)
    {
        var originals = new List<ParseTreeSymbol> { root };
        var ids = new Dictionary<ParseTreeSymbol, int>(ReferenceEqualityComparer.Instance) { [root] = 0 };
        var children = new List<int[]>();
        var complete = new List<bool>();
        var truncated = false;
        // Breadth-first allocation gives each child an ID after its parent and never recurses into
        // engine properties such as AllTokens/IsVirtual, whose implementations traverse the tree.
        for (var index = 0; index < originals.Count; index++)
        {
            var childIds = new List<int>();
            var nodeComplete = true;
            if (originals[index] is ParseTreeNonTerminal parent)
            {
                foreach (var child in parent.Items)
                {
                    if (!ids.TryGetValue(child, out var id))
                    {
                        if (originals.Count == McpLimits.MaxTreeNodes)
                        {
                            truncated = true;
                            nodeComplete = false;
                            continue;
                        }
                        id = originals.Count;
                        ids.Add(child, id);
                        originals.Add(child);
                    }
                    childIds.Add(id);
                }
            }
            children.Add(childIds.ToArray());
            complete.Add(nodeComplete);
        }

        var nodes = new TreeNodeInfo[originals.Count];
        for (var index = originals.Count - 1; index >= 0; index--)
        {
            var original = originals[index];
            if (original is ParseTreeTerminal leaf)
            {
                var (start, end) = TokenRange(leaf.Token, input);
                nodes[index] = new(index, "terminal", TokenSymbol(leaf.Token.Kind), leaf.Token.Input,
                    start, end, []);
                continue;
            }

            int? rangeStart = null;
            int? rangeEnd = null;
            foreach (var child in children[index])
            {
                // A malformed engine graph must not trigger a recursive walk or an invalid span.
                if (child <= index || !complete[child]) complete[index] = false;
                if (child <= index || nodes[child] is not { Start: { } start, End: { } end }) continue;
                rangeStart = rangeStart is null ? start : Math.Min(rangeStart.Value, start);
                rangeEnd = rangeEnd is null ? end : Math.Max(rangeEnd.Value, end);
            }
            if (!complete[index]) rangeStart = rangeEnd = null;
            var name = (original as ParseTreeNonTerminal)?.SignPost?.Name ?? string.Empty;
            nodes[index] = new(index, "nonterminal", Clip(name), null, rangeStart, rangeEnd, children[index]);
        }
        return (nodes, truncated);
    }

    private static (int? Start, int? End) TokenRange(TokenData? token, string input)
    {
        if (token is null || token.IsVirtual || token.IsStubCode || token.StartIndex < 0) return (null, null);
        var text = token.Input ?? string.Empty;
        var start = token.StartIndex;
        if (text.Length == 0 || start > input.Length - text.Length ||
            !input.AsSpan(start, text.Length).SequenceEqual(text.AsSpan())) return (null, null);
        return (start, start + text.Length);
    }

    private static string TokenSymbol(Terminal? terminal) => terminal is null or NotDefined ? "unknown" :
        Clip(terminal.IsWordPattern ? terminal.Caption : terminal.Value);

    private static void AddDiagnostic(List<DiagnosticInfo> diagnostics, DiagnosticInfo diagnostic)
    {
        if (diagnostics.Count < McpLimits.MaxDiagnostics) diagnostics.Add(diagnostic);
    }

    private static WorkerResponse Limit(string message) => new("runtime_limit", message);

    private static ParseOutcome ParseFailure(string status, string message, int tokenCount,
        TokenInfo[]? tokens = null, bool tokensTruncated = false) =>
        new(status, message, new(false, tokenCount, [new(status, message)], tokens ?? [],
            tokensTruncated || tokenCount > (tokens?.Length ?? 0)));

    private static string SafeGrammarMessage(string message) =>
        message.StartsWith("internal error:", StringComparison.OrdinalIgnoreCase)
            ? "The grammar could not be constructed." : Clip(message);

    private static string Clip(string? text) => string.IsNullOrEmpty(text) ? string.Empty :
        text.Length <= McpLimits.MaxMessageChars ? text : text[..McpLimits.MaxMessageChars];

    private sealed record ParseOutcome(string Status, string? Message, ParseReport Report);
}
