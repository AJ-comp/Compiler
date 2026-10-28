using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Janglim.FrontEnd.Grammars.JavaScript;

namespace Janglim.FrontEnd.Grammars.TypeScript
{
    // This scanner is used only for pure syntactic decisions, never as the product
    // token source. The selected EBNF production still parses every original token.
    internal static class TypeScriptLookahead
    {
        private static readonly Regex AssertionTarget = new Regex(
            @"\G(?<trivia>(?:\s|\ufeff|//[^\r\n\u2028\u2029]*|/\*[\s\S]*?\*/)*)" +
            "(?<identifier>" + JavaScriptTokenPatterns.IdentifierName + ")", RegexOptions.CultureInvariant);

        // Like the official parser's nextTokenIsIdentifierOrKeywordOnSameLine:
        // inspect trivia and the next identifier-shaped token before choosing the
        // assertion branch. The ordinary type-name branch handles lone 'asserts'.
        internal static bool AssertsTypePredicate(string source, int afterKeyword)
            => HasSameLineTarget(AssertionTarget.Match(source, afterKeyword));

        // Return annotations first interpret 'asserts is T' as an ordinary type
        // predicate whose parameter is named 'asserts', matching parseTypeOrTypePredicate.
        internal static bool AssertsNameTypePredicate(string source, int afterKeyword)
        {
            var match = AssertionTarget.Match(source, afterKeyword);
            return HasSameLineTarget(match) && match.Groups["identifier"].Value == "is";
        }

        private static bool HasSameLineTarget(Match match)
            => match.Success && match.Groups["trivia"].Value.IndexOfAny(new[] { '\r', '\n', '\u2028', '\u2029' }) < 0;

        internal static List<string> Scan(string source, int start, int maximum = int.MaxValue)
        {
            var tokens = new List<string>();
            for (int i = Math.Max(0, start); i < source.Length && tokens.Count < maximum;)
            {
                char c = source[i];
                if (char.IsWhiteSpace(c) || c == '\ufeff') { i++; continue; }
                if (c == '/' && i + 1 < source.Length && source[i + 1] == '/')
                { while (i < source.Length && source[i] != '\n' && source[i] != '\r') i++; continue; }
                if (c == '/' && i + 1 < source.Length && source[i + 1] == '*')
                { i += 2; while (i + 1 < source.Length && !(source[i] == '*' && source[i + 1] == '/')) i++; i = Math.Min(source.Length, i + 2); continue; }
                int begin = i++;
                if (c == '\'' || c == '"' || c == '`')
                {
                    while (i < source.Length)
                    { char next = source[i++]; if (next == '\\' && i < source.Length) i++; else if (next == c) break; }
                    tokens.Add(source.Substring(begin, i - begin)); continue;
                }
                if (IsNameStart(c) || char.IsDigit(c))
                {
                    while (i < source.Length && (IsNameStart(source[i]) || char.IsDigit(source[i]))) i++;
                    tokens.Add(source.Substring(begin, i - begin)); continue;
                }
                if (c == '.' && i + 1 < source.Length && source[i] == '.' && source[i + 1] == '.')
                { i += 2; tokens.Add("..."); continue; }
                if (i < source.Length && ((c == '=' && source[i] == '>') || (c == '?' && source[i] == '.') ||
                    ((c == '&' || c == '|') && source[i] == c))) i++;
                tokens.Add(source.Substring(begin, i - begin));
            }
            return tokens;
        }

        internal static bool IsNameStart(char c) => char.IsLetter(c) || c == '_' || c == '$' || c > 127 || c == '\\';
        internal static bool IsName(string value) => value.Length != 0 && IsNameStart(value[0]);
        internal static string At(IReadOnlyList<string> tokens, int index)
            => index >= 0 && index < tokens.Count ? tokens[index] : string.Empty;

        internal static int Close(IReadOnlyList<string> tokens, int start, string open, string close)
        {
            int depth = 0;
            for (int i = start; i < tokens.Count; i++)
            { if (tokens[i] == open) depth++; else if (tokens[i] == close && --depth == 0) return i; }
            return -1;
        }

        internal static bool FunctionType(IReadOnlyList<string> tokens)
        {
            string first = At(tokens, 0);
            if (first == "<" || first == "new" || (first == "abstract" && At(tokens, 1) == "new")) return true;
            if (first != "(") return false;
            int end = Close(tokens, 0, "(", ")");
            return end >= 0 && At(tokens, end + 1) == "=>";
        }

        internal static bool MappedType(IReadOnlyList<string> tokens)
        {
            int i = 0;
            if (At(tokens, i++) != "{") return false;
            if (At(tokens, i) == "+" || At(tokens, i) == "-") i++;
            if (At(tokens, i) == "readonly") i++;
            return At(tokens, i) == "[" && IsName(At(tokens, i + 1)) && At(tokens, i + 2) == "in";
        }

        internal static bool NamedTuple(IReadOnlyList<string> tokens)
        {
            int i = At(tokens, 0) == "..." ? 1 : 0;
            if (!IsName(At(tokens, i++))) return false;
            if (At(tokens, i) == "?") i++;
            return At(tokens, i) == ":";
        }

        internal static bool IndexSignature(IReadOnlyList<string> tokens)
        {
            int i = At(tokens, 0) == "readonly" ? 1 : 0;
            return At(tokens, i) == "[" && IsName(At(tokens, i + 1)) && At(tokens, i + 2) == ":";
        }
    }
}
