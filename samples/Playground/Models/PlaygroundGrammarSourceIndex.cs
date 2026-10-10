using Janglim.FrontEnd.ParseTree;

namespace Janglim.Playground.Models;

/// <summary>
/// Indexes authored EBNF symbols for editor highlighting. It never searches input text
/// for a tree label and never treats names inside comments, literals, or regexes as refs.
/// </summary>
internal sealed class PlaygroundGrammarSourceIndex
{
    private readonly Dictionary<string, List<PlaygroundTextRange>> _nonTerminals = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<PlaygroundTextRange>> _namedTerminals = new(StringComparer.Ordinal);
    private readonly Dictionary<string, List<PlaygroundTextRange>> _literals = new(StringComparer.Ordinal);

    public PlaygroundGrammarSourceIndex(string text)
    {
        var tokens = Scan(text);
        var nonTerminalNames = new HashSet<string>(StringComparer.Ordinal);
        var terminalNames = new HashSet<string>(StringComparer.Ordinal);
        for (var i = 0; i + 1 < tokens.Count; i++)
        {
            if (tokens[i].Kind != SourceKind.Identifier) continue;
            if (tokens[i + 1].Kind == SourceKind.Colon) nonTerminalNames.Add(tokens[i].Value);
            else if (tokens[i + 1].Kind == SourceKind.ColonEquals) terminalNames.Add(tokens[i].Value);
        }

        for (var i = 0; i < tokens.Count; i++)
        {
            var token = tokens[i];
            if (token.Kind == SourceKind.Literal)
                Add(_literals, token.Value, token.Range);
            else if (token.Kind == SourceKind.Pattern && i >= 2 &&
                tokens[i - 1].Kind == SourceKind.ColonEquals && tokens[i - 2].Kind == SourceKind.Identifier)
                Add(_namedTerminals, tokens[i - 2].Value, token.Range);
            else if (token.Kind == SourceKind.Identifier)
            {
                var next = i + 1 < tokens.Count ? tokens[i + 1].Kind : SourceKind.Other;
                if (next == SourceKind.Colon)
                    Add(_nonTerminals, token.Value, token.Range);
                else if (next == SourceKind.ColonEquals)
                    Add(_namedTerminals, token.Value, token.Range);
                // EbnfGrammar resolves a bare name as a non-terminal before a named token.
                else if (nonTerminalNames.Contains(token.Value))
                    Add(_nonTerminals, token.Value, token.Range);
                else if (terminalNames.Contains(token.Value))
                    Add(_namedTerminals, token.Value, token.Range);
            }
        }
    }

    public IReadOnlyList<PlaygroundTextRange> For(ParseTreeSymbol symbol) => symbol switch
    {
        ParseTreeNonTerminal nonTerminal => Find(_nonTerminals, nonTerminal.SignPost.Name),
        ParseTreeTerminal terminal when terminal.Token.Kind.IsWordPattern =>
            Find(_namedTerminals, terminal.Token.Kind.Caption),
        ParseTreeTerminal terminal => Find(_literals, terminal.Token.Kind.Value),
        _ => []
    };

    private static IReadOnlyList<PlaygroundTextRange> Find(
        Dictionary<string, List<PlaygroundTextRange>> index, string value) =>
        index.TryGetValue(value, out var ranges) ? ranges : [];

    private static void Add(Dictionary<string, List<PlaygroundTextRange>> index,
        string value, PlaygroundTextRange range)
    {
        if (!index.TryGetValue(value, out var ranges)) index.Add(value, ranges = []);
        ranges.Add(range);
    }

    private static List<SourceToken> Scan(string text)
    {
        var tokens = new List<SourceToken>();
        for (var i = 0; i < text.Length;)
        {
            var c = text[i];
            if (char.IsWhiteSpace(c)) { i++; continue; }
            if (c == '#')
            {
                while (i < text.Length && text[i] != '\n') i++;
                continue;
            }

            var start = i++;
            if (c is '\'' or '"')
            {
                var contentStart = i;
                // Match EbnfGrammarReader's token boundaries: it retains backslashes and
                // closes at the next same quote. Escaped quote delimiters are rejected by
                // that reader before this index is built; quoted names are never refs.
                while (i < text.Length && text[i] != c) i++;
                var value = text[contentStart..i];
                if (i < text.Length) i++;
                tokens.Add(new(c == '\'' ? SourceKind.Literal : SourceKind.Pattern,
                    value, new(start, i)));
            }
            else if (char.IsLetter(c) || c == '_')
            {
                while (i < text.Length && (char.IsLetterOrDigit(text[i]) || text[i] == '_')) i++;
                tokens.Add(new(SourceKind.Identifier, text[start..i], new(start, i)));
            }
            else if (c == ':')
            {
                var kind = i < text.Length && text[i] == '=' ? SourceKind.ColonEquals : SourceKind.Colon;
                if (kind == SourceKind.ColonEquals) i++;
                tokens.Add(new(kind, string.Empty, new(start, i)));
            }
            else tokens.Add(new(SourceKind.Other, string.Empty, new(start, i)));
        }
        return tokens;
    }

    private enum SourceKind { Identifier, Literal, Pattern, Colon, ColonEquals, Other }
    private sealed record SourceToken(SourceKind Kind, string Value, PlaygroundTextRange Range);
}
