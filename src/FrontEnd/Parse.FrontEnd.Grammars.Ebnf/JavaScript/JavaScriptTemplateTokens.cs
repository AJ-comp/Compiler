using Janglim.FrontEnd.Tokenize;
using System;
using System.Globalization;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Template segment boundaries and branch-local interpolation brace depths.
    /// The parser's cursor carries the state, so Peek never mutates another parse or prediction.
    /// Invalid escapes remain lexically recognizable: the tree validator rejects them only
    /// in untagged templates, as required by ECMAScript 2026 clauses 12.9.6 and 13.2.8.
    /// <para><see href="../../../../docs/ko/javascript-expressions.html#templates">템플릿 구간과 표현식 사이의 전환</see></para>
    /// </summary>
    internal static class JavaScriptTemplateTokens
    {
        // These alternatives are disjoint: a backslash protects exactly the next source
        // character, and only an unescaped `${` or backtick terminates a text segment.
        // Escape validity is checked later, when the tree identifies tagged/untagged use.
        private const string Characters = @"(?:[^`\\$]|\$(?!\{)|\\[\s\S])*";
        internal const string NoSubstitutionTemplate = "`" + Characters + "`";
        internal const string TemplateHead = "`" + Characters + @"\$\{";
        internal const string TemplateMiddle = @"\}" + Characters + @"\$\{";
        internal const string TemplateTail = @"\}" + Characters + "`";

        internal static bool IsAtTemplateBoundary(string state)
        {
            if (state.Length == 0) return false;
            int top = state.LastIndexOf(',') + 1;
            return state.Length - top == 1 && state[top] == '0';
        }

        internal static TokenRead WithNextState(TokenRead read, string state)
        {
            string next = state;
            string caption = read.Token.Kind.Caption;
            if (caption == "TemplateHead") next = state.Length == 0 ? "0" : state + ",0";
            else if (caption == "TemplateMiddle" || caption == "TemplateTail")
            {
                if (!IsAtTemplateBoundary(state))
                    throw new JavaScriptSyntaxException("Unexpected template continuation.",
                        read.Token.StartIndex, read.Token.Input.Length);
                if (caption == "TemplateTail")
                {
                    int separator = state.LastIndexOf(',');
                    next = separator < 0 ? string.Empty : state.Substring(0, separator);
                }
            }
            else if (state.Length != 0 && (read.Token.Kind.Value == "{" || read.Token.Kind.Value == "}"))
            {
                int top = state.LastIndexOf(',') + 1;
                int depth = int.Parse(state.Substring(top), CultureInfo.InvariantCulture);
                depth = read.Token.Kind.Value == "{" ? checked(depth + 1) : depth - 1;
                if (depth < 0)
                    throw new JavaScriptSyntaxException("A template substitution requires a template continuation.",
                        read.Token.StartIndex, read.Token.Input.Length);
                next = state.Substring(0, top) + depth.ToString(CultureInfo.InvariantCulture);
            }
            if (next == state) return read;
            var cursor = read.Next;
            return new TokenRead(read.Token, new TokenCursor(cursor.SourceIdentity, cursor.SourceOffset,
                cursor.TokenIndex, cursor.PreviousToken, next));
        }

        /// <summary>Returns the first invalid template escape, without evaluating template values.</summary>
        internal static JavaScriptDiagnostic GetInvalidEscape(TokenData token)
        {
            string text = token.Input;
            int end = text.Length - (token.Kind.Caption == "TemplateHead" || token.Kind.Caption == "TemplateMiddle" ? 2 : 1);
            for (int index = 1; index < end; index++)
            {
                if (text[index] != '\\') continue;
                int begin = index;
                char escaped = text[++index];
                if (escaped >= '1' && escaped <= '9' ||
                    escaped == '0' && index + 1 < end && text[index + 1] >= '0' && text[index + 1] <= '9')
                    return Invalid(token, begin, index + 1);
                if (escaped == 'x')
                {
                    for (int digit = 0; digit < 2; digit++)
                    {
                        if (++index >= end || Hex(text[index]) < 0) return Invalid(token, begin, Math.Min(end, index + 1));
                    }
                }
                else if (escaped == 'u')
                {
                    if (index + 1 < end && text[index + 1] == '{')
                    {
                        index += 2;
                        int count = 0, value = 0;
                        while (index < end && Hex(text[index]) >= 0)
                        {
                            value = value > 0x10ffff ? 0x110000 : value * 16 + Hex(text[index]);
                            index++;
                            count++;
                        }
                        if (count == 0 || index >= end || text[index] != '}' || value > 0x10ffff)
                            return Invalid(token, begin, Math.Min(end, index + 1));
                    }
                    else
                    {
                        for (int digit = 0; digit < 4; digit++)
                        {
                            if (++index >= end || Hex(text[index]) < 0) return Invalid(token, begin, Math.Min(end, index + 1));
                        }
                    }
                }
                else if (escaped == '\r' && index + 1 < end && text[index + 1] == '\n') index++;
            }
            return null;
        }

        private static int Hex(char value)
            => value >= '0' && value <= '9' ? value - '0'
             : value >= 'a' && value <= 'f' ? value - 'a' + 10
             : value >= 'A' && value <= 'F' ? value - 'A' + 10 : -1;

        private static JavaScriptDiagnostic Invalid(TokenData token, int begin, int end)
            => new JavaScriptDiagnostic("invalid_template_escape", "Invalid escape sequence in an untagged template literal.",
                token.StartIndex + begin, Math.Max(1, end - begin));
    }
}
