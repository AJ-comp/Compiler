using System.Diagnostics;
using System.Text.Json.Serialization;
using Janglim.FrontEnd.Grammars.Ebnf;
using Janglim.FrontEnd.Parsers.Datas;
using Janglim.FrontEnd.Parsers.LR;
using Janglim.FrontEnd.ParseTree;
using Janglim.FrontEnd.Tokenize;

namespace Janglim.Playground.Models;

/// <summary>
/// A presentation-only projection of the existing parser result. The parser's success,
/// actions, conflicts, and stack snapshots are retained without serializing parent links.
/// </summary>
public sealed record PlaygroundSession(
    [property: JsonPropertyName("success")] bool Success,
    [property: JsonPropertyName("grammarErrors")] IReadOnlyList<string> GrammarErrors,
    [property: JsonPropertyName("conflicts")] IReadOnlyList<PlaygroundConflict> Conflicts,
    [property: JsonPropertyName("steps")] IReadOnlyList<PlaygroundStep> Steps,
    [property: JsonPropertyName("entries")] IReadOnlyList<PlaygroundEntry> Entries,
    [property: JsonPropertyName("tokenCount")] int TokenCount,
    [property: JsonPropertyName("stateCount")] int StateCount,
    [property: JsonPropertyName("elapsedMs")] double ElapsedMs)
{
    public static PlaygroundSession Create(string grammarText, string inputText)
    {
        var started = Stopwatch.GetTimestamp();
        try
        {
            var read = EbnfGrammarReader.Read(grammarText);
            if (!read.Success)
                return Failed(read.Errors, started);

            var grammar = read.Grammar;
            var lexer = new Lexer();
            foreach (var terminal in grammar.TerminalSet)
                lexer.AddTokenRule(terminal);

            var parser = new LALRParser(grammar, bLogging: true);
            var tokens = lexer.Lexing(inputText).TokensForParsing;
            var result = parser.Parsing(tokens);

            var projection = new TreeProjection(grammarText, inputText);
            var steps = result.Trace.Steps.Select(step => new PlaygroundStep(
                step.Index,
                ToAction(step.Action),
                step.Lookahead?.Input,
                projection.ToFrames(step.StackBefore),
                projection.ToFrames(step.StackAfter),
                step.IsError,
                step.ErrorMessage,
                step.RecoveryMessage)).ToArray();

            var entries = parser.ParsingTable.Entries.Select(entry =>
            {
                var action = ToAction(entry.Action)!;
                return new PlaygroundEntry(entry.State, entry.Symbol.ToString() ?? string.Empty,
                    action.Kind, action.Target);
            }).ToArray();

            var conflicts = parser.CheckAmbiguity()
                .Where(conflict => conflict.IsShiftReduceConflict || conflict.IsReduceReduceConflict)
                .Select(conflict => new PlaygroundConflict(
                    conflict.CanonicalLine.CurrentStatusIndex, conflict.AmbiguityContent))
                .ToArray();

            return new PlaygroundSession(result.Success, [], conflicts, steps, entries,
                tokens.Count, parser.Canonical.IndexStateDic.Count,
                Stopwatch.GetElapsedTime(started).TotalMilliseconds);
        }
        catch (Exception ex)
        {
            // Surface grammar construction and lexer errors in the same editor diagnostics area.
            var message = string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message;
            return Failed([$"Unable to process the grammar and input: {message}"], started);
        }
    }

    private static PlaygroundSession Failed(IReadOnlyList<string> errors, long started) =>
        new(false, errors, [], [], [], 0, 0, Stopwatch.GetElapsedTime(started).TotalMilliseconds);

    private static PlaygroundAction? ToAction(ParseAction? action) => action switch
    {
        ParseAction.Shift shift => new("shift", $"I{shift.State}"),
        ParseAction.Goto go => new("goto", $"I{go.State}"),
        ParseAction.Reduce reduce => new(reduce.IsEpsilon ? "ε-reduce" : "reduce",
            reduce.Production.ToGrammarString()),
        ParseAction.Accept => new("accept", string.Empty),
        _ => null
    };

    private sealed class TreeProjection(string grammarText, string inputText)
    {
        // ParseTreeSymbol.Equals compares flags rather than node identity, so use references here.
        private readonly Dictionary<ParseTreeSymbol, PlaygroundTreeNode> _nodes =
            new(ReferenceEqualityComparer.Instance);
        private readonly PlaygroundGrammarSourceIndex _grammarSource = new(grammarText);

        public IReadOnlyList<PlaygroundStackFrame> ToFrames(IReadOnlyList<ParseStackFrame> frames) =>
            frames.Select(frame => new PlaygroundStackFrame(frame.State,
                frame.Symbol is null ? null : ToNode(frame.Symbol))).ToArray();

        private PlaygroundTreeNode ToNode(ParseTreeSymbol symbol)
        {
            if (_nodes.TryGetValue(symbol, out var existing)) return existing;

            // Reserve the ID before walking children so it remains stable in all trace snapshots.
            var id = $"n{_nodes.Count + 1}";
            var node = new PlaygroundTreeNode(id, symbol.ToString() ?? string.Empty,
                symbol.IsTerminal, [])
            {
                SymbolName = symbol switch
                {
                    ParseTreeTerminal terminal => terminal.Token.Kind.IsWordPattern
                        ? terminal.Token.Kind.Caption : terminal.Token.Kind.Value,
                    ParseTreeNonTerminal nonTerminal => nonTerminal.SignPost.Name,
                    _ => string.Empty
                },
                GrammarRanges = _grammarSource.For(symbol)
            };
            _nodes.Add(symbol, node);
            if (symbol is ParseTreeNonTerminal parent)
            {
                node.Children = parent.Items.Select(ToNode).ToArray();
                var start = int.MaxValue;
                var end = -1;
                foreach (var child in node.Children)
                {
                    if (child.InputRange is not { } range) continue;
                    start = Math.Min(start, range.Start);
                    end = Math.Max(end, range.End);
                }
                if (end >= 0) node.InputRange = new(start, end);
            }
            else if (symbol is ParseTreeTerminal leaf)
            {
                var token = leaf.Token;
                var start = token.StartIndex;
                var length = token.Input?.Length ?? 0;
                // TokenCell's offsets and .NET string lengths are UTF-16, like JS and Monaco.
                // Epsilon, EOF, inserted tokens, and missing source spans have no real input range.
                if (!token.IsVirtual && !token.IsStubCode && start >= 0 && length > 0 &&
                    start <= inputText.Length - length &&
                    inputText.AsSpan(start, length).SequenceEqual(token.Input.AsSpan()))
                    node.InputRange = new(start, start + length);
            }

            return node;
        }
    }
}

public sealed record PlaygroundConflict(
    [property: JsonPropertyName("state")] int State,
    [property: JsonPropertyName("kind")] string Kind);

public sealed record PlaygroundAction(
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("target")] string Target);

public sealed record PlaygroundEntry(
    [property: JsonPropertyName("state")] int State,
    [property: JsonPropertyName("symbol")] string Symbol,
    [property: JsonPropertyName("kind")] string Kind,
    [property: JsonPropertyName("target")] string Target);

public sealed record PlaygroundStep(
    [property: JsonPropertyName("index")] int Index,
    [property: JsonPropertyName("action")] PlaygroundAction? Action,
    [property: JsonPropertyName("lookahead")] string? Lookahead,
    [property: JsonPropertyName("stackBefore")] IReadOnlyList<PlaygroundStackFrame> StackBefore,
    [property: JsonPropertyName("stackAfter")] IReadOnlyList<PlaygroundStackFrame> StackAfter,
    [property: JsonPropertyName("isError")] bool IsError,
    [property: JsonPropertyName("errorMessage")] string? ErrorMessage,
    [property: JsonPropertyName("recoveryMessage")] string? RecoveryMessage);

public sealed record PlaygroundStackFrame(
    [property: JsonPropertyName("state")] int State,
    [property: JsonPropertyName("symbol")] PlaygroundTreeNode? Symbol);

public sealed class PlaygroundTreeNode(string id, string label, bool isTerminal,
    IReadOnlyList<PlaygroundTreeNode> children)
{
    [JsonPropertyName("id")] public string Id { get; } = id;
    [JsonPropertyName("label")] public string Label { get; } = label;
    [JsonPropertyName("isTerminal")] public bool IsTerminal { get; } = isTerminal;
    [JsonPropertyName("symbolName")] public string SymbolName { get; internal set; } = string.Empty;
    [JsonPropertyName("inputRange")] public PlaygroundTextRange? InputRange { get; internal set; }
    [JsonPropertyName("grammarRanges")] public IReadOnlyList<PlaygroundTextRange> GrammarRanges { get; internal set; } = [];
    [JsonPropertyName("children")] public IReadOnlyList<PlaygroundTreeNode> Children { get; internal set; } = children;
}

/// <summary>A UTF-16 source range with an exclusive end offset.</summary>
public sealed record PlaygroundTextRange(
    [property: JsonPropertyName("start")] int Start,
    [property: JsonPropertyName("end")] int End);

// Return the JSON as a string across JSInterop so deeply nested CSTs do not hit its
// smaller generic serializer depth limit. Source generation also preserves the DTO
// properties when the WebAssembly application is trimmed for publishing.
[JsonSourceGenerationOptions(MaxDepth = 4096)]
[JsonSerializable(typeof(PlaygroundSession))]
internal sealed partial class PlaygroundJsonContext : JsonSerializerContext;
