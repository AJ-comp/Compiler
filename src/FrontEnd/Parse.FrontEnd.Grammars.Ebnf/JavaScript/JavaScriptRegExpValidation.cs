using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Validates ECMAScript 2026 RegExpPattern syntax after the parser has selected
    /// a concrete regular-expression literal. Ordinary, Unicode and Unicode-sets
    /// modes keep their own grammar and early-error rules. No expression is matched
    /// or compiled with a host regex engine. Explicit stacks support nested groups.
    /// <para><see href="../../../../docs/ko/javascript-regexp.html">정규식 본문 문법과 검사 순서</see></para>
    /// </summary>
    internal static class JavaScriptRegExpValidation
    {
        internal static void Validate(TokenData token, List<JavaScriptDiagnostic> diagnostics)
        {
            string literal = token.Input;
            int slash = literal.LastIndexOf('/');
            string body = literal.Substring(1, slash - 1);
            string flags = literal.Substring(slash + 1);
            try { new Reader(body, flags).Read(); }
            catch (SyntaxFailure error)
            {
                int start = Math.Max(0, Math.Min(body.Length, error.Offset));
                int length = Math.Max(0, Math.Min(body.Length - start, error.Length));
                diagnostics.Add(new JavaScriptDiagnostic("regexp_syntax", error.Message,
                    token.StartIndex + 1 + start, length));
            }
        }

        private sealed class Reader
        {
            private readonly string body;
            private readonly bool unicode;
            private readonly bool sets;
            private readonly bool named;
            private readonly int captureCount;
            private readonly Stack<Frame> groups = new Stack<Frame>();
            private readonly HashSet<string> names = new HashSet<string>(StringComparer.Ordinal);
            private readonly List<Reference> references = new List<Reference>();
            private int index;

            internal Reader(string body, string flags)
            {
                this.body = body;
                sets = flags.IndexOf('v') >= 0;
                unicode = sets || flags.IndexOf('u') >= 0;
                CountCaptures(body, sets, out captureCount, out named);
                groups.Push(new Frame(0, 1, null));
            }

            internal void Read()
            {
                while (index < body.Length)
                {
                    var frame = groups.Peek();
                    int start = index;
                    char ch = body[index];
                    if (ch == '(') { Open(); continue; }
                    if (ch == ')')
                    {
                        if (groups.Count == 1) Fail(start, 1, "Unmatched closing parenthesis in regular expression.");
                        index++;
                        var completed = groups.Pop();
                        var groupNames = FinishNames(completed);
                        var parent = groups.Peek();
                        parent.Sequence = Merge(parent.Sequence, groupNames, true, completed.Start);
                        parent.Term = completed.Kind;
                        continue;
                    }
                    if (ch == '|')
                    {
                        frame.Alternatives = Merge(frame.Alternatives, frame.Sequence, false, start);
                        frame.Sequence = null;
                        frame.Term = 0;
                        index++;
                        continue;
                    }
                    if (ch == '^' || ch == '$') { frame.Term = 0; index++; continue; }
                    if (ch == '*' || ch == '+' || ch == '?')
                    {
                        Quantifiable(frame, start);
                        index++;
                        if (index < body.Length && body[index] == '?') index++;
                        frame.Term = 0;
                        continue;
                    }
                    if (ch == '{')
                    {
                        if (ReadQuantifier())
                        {
                            Quantifiable(frame, start);
                            if (index < body.Length && body[index] == '?') index++;
                            frame.Term = 0;
                            continue;
                        }
                        if (unicode) Fail(start, 1, "Invalid quantifier in Unicode regular expression.");
                    }
                    if (ch == '[')
                    {
                        if (sets)
                        {
                            if (!JavaScriptRegExpSets.TryRead(body, ref index, out _,
                                out int offset, out int length, out string message)) Fail(offset, length, message);
                        }
                        else ReadClass();
                        frame.Term = 1;
                        continue;
                    }
                    if (ch == '\\')
                    {
                        if (index + 1 < body.Length && (body[index + 1] == 'b' || body[index + 1] == 'B'))
                        { index += 2; frame.Term = 0; continue; }
                        ReadEscape(false);
                        frame.Term = 1;
                        continue;
                    }
                    if (unicode && (ch == ']' || ch == '}'))
                        Fail(start, 1, "Unescaped syntax character in Unicode regular expression.");
                    index++;
                    frame.Term = 1;
                }
                if (groups.Count != 1) Fail(groups.Peek().Start, 1, "Unterminated regular-expression group.");
                FinishNames(groups.Pop());
                foreach (var reference in references)
                    if (!names.Contains(reference.Name))
                        Fail(reference.Start, reference.Length, "The named backreference has no matching capture group.");
            }

            private void Open()
            {
                int start = index++;
                int kind = 1; // 1 atom, 2 legacy quantifiable lookahead, 0 assertion.
                string name = null;
                if (index < body.Length && body[index] == '?')
                {
                    index++;
                    if (index >= body.Length) Fail(start, index - start, "Unterminated group prefix.");
                    char prefix = body[index++];
                    if (prefix == ':') { }
                    else if (prefix == '=' || prefix == '!') kind = 2;
                    else if (prefix == '<')
                    {
                        if (index < body.Length && (body[index] == '=' || body[index] == '!'))
                        { index++; kind = 0; }
                        else { name = ReadName(start); names.Add(name); }
                    }
                    else
                    {
                        index--; // Modifier lists start at the first i/m/s or '-'.
                        int add = 0, remove = 0;
                        ReadModifiers(ref add, start);
                        if (index < body.Length && body[index] == '-') { index++; ReadModifiers(ref remove, start); }
                        if ((add | remove) == 0 || (add & remove) != 0 || index >= body.Length || body[index] != ':')
                            Fail(start, Math.Max(1, index - start), "Invalid regular-expression modifier group.");
                        index++;
                    }
                }
                groups.Push(new Frame(start, kind, name));
            }

            private void ReadModifiers(ref int bits, int start)
            {
                while (index < body.Length)
                {
                    int bit = body[index] == 'i' ? 1 : body[index] == 'm' ? 2 : body[index] == 's' ? 4 : 0;
                    if (bit == 0) break;
                    if ((bits & bit) != 0) Fail(start, index - start + 1, "A modifier flag cannot appear twice.");
                    bits |= bit;
                    index++;
                }
            }

            private string ReadName(int start)
            {
                var result = new StringBuilder();
                bool first = true;
                while (index < body.Length && body[index] != '>')
                {
                    int point;
                    if (body[index] == '\\')
                    {
                        if (!TryUnicode(index, true, out point, out int end))
                            Fail(index, Math.Min(2, body.Length - index), "Invalid Unicode escape in capture group name.");
                        index = end;
                    }
                    else point = ReadPoint(true);
                    if (!(first ? JavaScriptUnicode.IsStart(point) : JavaScriptUnicode.IsPart(point)))
                        Fail(start, Math.Max(1, index - start), "Invalid identifier in capture group name.");
                    result.Append(char.ConvertFromUtf32(point));
                    first = false;
                }
                if (first || index >= body.Length)
                    Fail(start, Math.Max(1, index - start), "A named capture or backreference requires a nonempty terminated name.");
                index++; // '>'
                return result.ToString();
            }

            private bool ReadQuantifier()
            {
                int start = index;
                int cursor = start + 1;
                int lower = cursor;
                while (cursor < body.Length && Digit(body[cursor])) cursor++;
                if (cursor == lower) return false;
                int lowerEnd = cursor;
                int upper = cursor, upperEnd = cursor;
                bool bounded = false;
                if (cursor < body.Length && body[cursor] == ',')
                {
                    upper = ++cursor;
                    while (cursor < body.Length && Digit(body[cursor])) cursor++;
                    upperEnd = cursor;
                    bounded = upperEnd != upper;
                }
                if (cursor >= body.Length || body[cursor] != '}') return false;
                if (bounded && CompareDigits(lower, lowerEnd, upper, upperEnd) > 0)
                    Fail(start, cursor - start + 1, "A quantifier's minimum exceeds its maximum.");
                index = cursor + 1;
                return true;
            }

            private void Quantifiable(Frame frame, int start)
            {
                if (frame.Term != 1 && !(frame.Term == 2 && !unicode))
                    Fail(start, 1, "A quantifier must follow a quantifiable atom.");
            }

            private void ReadClass()
            {
                int start = index++;
                if (index < body.Length && body[index] == '^') index++;
                while (index < body.Length && body[index] != ']')
                {
                    int atomStart = index;
                    var left = ReadClassAtom();
                    if (index < body.Length - 1 && body[index] == '-' && body[index + 1] != ']')
                    {
                        index++;
                        var right = ReadClassAtom();
                        if (!left.Character || !right.Character)
                        {
                            if (unicode) Fail(atomStart, index - atomStart, "A Unicode character-class range requires character endpoints.");
                        }
                        else if (left.Point > right.Point)
                            Fail(atomStart, index - atomStart, "Character-class range is out of order.");
                    }
                }
                if (index >= body.Length) Fail(start, body.Length - start, "Unterminated regular-expression character class.");
                index++;
            }

            private Atom ReadClassAtom()
            {
                if (index >= body.Length || body[index] == ']') Fail(index, 0, "A character-class range endpoint is missing.");
                return body[index] == '\\' ? ReadEscape(true) : new Atom(ReadPoint(unicode));
            }

            private Atom ReadEscape(bool inClass)
            {
                int start = index++;
                if (index >= body.Length) Fail(start, 1, "A regular-expression escape is incomplete.");
                char ch = body[index++];
                if (ch == 'd' || ch == 'D' || ch == 's' || ch == 'S' || ch == 'w' || ch == 'W') return Atom.Set;
                if (ch == 'b' && inClass) return new Atom(8);
                if (ch == 'f') return new Atom(12);
                if (ch == 'n') return new Atom(10);
                if (ch == 'r') return new Atom(13);
                if (ch == 't') return new Atom(9);
                if (ch == 'v') return new Atom(11);
                if (ch == 'c')
                {
                    if (index < body.Length && (AsciiLetter(body[index]) || !unicode && inClass && (Digit(body[index]) || body[index] == '_')))
                        return new Atom(body[index++] % 32);
                    if (unicode) Fail(start, index - start, "A control escape requires an ASCII letter.");
                    // Annex B: only the backslash was consumed. The 'c' is a
                    // subsequent class atom, which matters for range ordering.
                    index = start + 1;
                    return new Atom('\\');
                }
                if (ch == 'x')
                {
                    if (ReadHex(index, 2, out int value)) { index += 2; return new Atom(value); }
                    if (unicode) Fail(start, index - start, "A hexadecimal escape requires two hexadecimal digits.");
                    return new Atom('x');
                }
                if (ch == 'u')
                {
                    if (TryUnicode(start, unicode, out int value, out int end)) { index = end; return new Atom(value); }
                    if (unicode) Fail(start, index - start, "Invalid Unicode regular-expression escape.");
                    return new Atom('u');
                }
                if ((ch == 'p' || ch == 'P') && unicode)
                {
                    if (index >= body.Length || body[index] != '{') Fail(start, index - start, "A Unicode property escape requires braces.");
                    int valueStart = ++index;
                    while (index < body.Length && body[index] != '}') index++;
                    if (index >= body.Length || !JavaScriptRegExpProperties.IsValid(body.Substring(valueStart, index - valueStart), sets, ch == 'P', out _))
                        Fail(start, Math.Max(1, index - start), "Unknown or invalid Unicode property escape.");
                    index++;
                    return Atom.Set;
                }
                if (ch == 'k' && (unicode || named))
                {
                    if (inClass || index >= body.Length || body[index] != '<')
                        Fail(start, index - start, "Invalid named backreference.");
                    index++;
                    string name = ReadName(start);
                    references.Add(new Reference(name, start, index - start));
                    return Atom.Set;
                }
                if (Digit(ch))
                {
                    if (unicode)
                    {
                        if (ch == '0')
                        {
                            if (index < body.Length && Digit(body[index])) Fail(start, index - start + 1, "A null escape cannot be followed by a decimal digit in Unicode mode.");
                            return new Atom(0);
                        }
                        if (inClass) Fail(start, index - start, "Decimal escapes are not allowed in Unicode character classes.");
                        while (index < body.Length && Digit(body[index])) index++;
                        if (CompareCapture(start + 1, index) > 0) Fail(start, index - start, "The numeric backreference exceeds the number of captures.");
                        return Atom.Set;
                    }
                    if (!inClass && ch != '0')
                    {
                        int end = index;
                        while (end < body.Length && Digit(body[end])) end++;
                        if (CompareCapture(start + 1, end) <= 0) { index = end; return Atom.Set; }
                    }
                    if (ch >= '0' && ch <= '7')
                    {
                        int value = ch - '0';
                        int remaining = ch <= '3' ? 2 : 1;
                        while (remaining-- > 0 && index < body.Length && body[index] >= '0' && body[index] <= '7')
                            value = value * 8 + body[index++] - '0';
                        return new Atom(value);
                    }
                    return new Atom(ch);
                }
                if (unicode && "^$\\.*+?()[]{}|/".IndexOf(ch) < 0 && !(inClass && ch == '-'))
                    Fail(start, index - start, "Invalid identity escape in Unicode regular expression.");
                return new Atom(ch);
            }

            private bool TryUnicode(int start, bool codePoints, out int point, out int end)
            {
                point = 0; end = start;
                if (start + 2 >= body.Length || body[start] != '\\' || body[start + 1] != 'u') return false;
                int cursor = start + 2;
                if (codePoints && body[cursor] == '{')
                {
                    int digits = ++cursor;
                    while (cursor < body.Length && Hex(body[cursor]) >= 0)
                    {
                        point = point > 0x10FFFF ? 0x110000 : point * 16 + Hex(body[cursor]);
                        cursor++;
                    }
                    if (cursor == digits || cursor >= body.Length || body[cursor] != '}' || point > 0x10FFFF) return false;
                    end = cursor + 1;
                    return true;
                }
                if (!ReadHex(cursor, 4, out point)) return false;
                end = cursor + 4;
                // Only a pair of fixed-width Unicode escapes forms this grammar's
                // surrogate-pair production. A braced escape stays one code point.
                if (codePoints && point >= 0xD800 && point <= 0xDBFF && end + 5 < body.Length &&
                    body[end] == '\\' && body[end + 1] == 'u' && ReadHex(end + 2, 4, out int low) && low >= 0xDC00 && low <= 0xDFFF)
                {
                    point = 0x10000 + ((point - 0xD800) << 10) + low - 0xDC00;
                    end += 6;
                }
                return true;
            }

            private bool ReadHex(int start, int count, out int value)
            {
                value = 0;
                if (start + count > body.Length) return false;
                for (int offset = 0; offset < count; offset++)
                {
                    int digit = Hex(body[start + offset]);
                    if (digit < 0) return false;
                    value = value * 16 + digit;
                }
                return true;
            }

            private int ReadPoint(bool codePoints)
            {
                int point = body[index++];
                if (codePoints && point >= 0xD800 && point <= 0xDBFF && index < body.Length && char.IsLowSurrogate(body[index]))
                    point = 0x10000 + ((point - 0xD800) << 10) + body[index++] - 0xDC00;
                return point;
            }

            private int CompareCapture(int start, int end)
            {
                string count = captureCount.ToString(CultureInfo.InvariantCulture);
                if (end - start != count.Length) return (end - start).CompareTo(count.Length);
                for (int offset = 0; offset < count.Length; offset++)
                    if (body[start + offset] != count[offset]) return body[start + offset].CompareTo(count[offset]);
                return 0;
            }

            private int CompareDigits(int left, int leftEnd, int right, int rightEnd)
            {
                while (left < leftEnd && body[left] == '0') left++;
                while (right < rightEnd && body[right] == '0') right++;
                int length = (leftEnd - left).CompareTo(rightEnd - right);
                if (length != 0) return length;
                while (left < leftEnd)
                {
                    int difference = body[left++].CompareTo(body[right++]);
                    if (difference != 0) return difference;
                }
                return 0;
            }

            private static HashSet<string> FinishNames(Frame frame)
            {
                var result = Merge(frame.Alternatives, frame.Sequence, false, frame.Start);
                if (frame.Name != null)
                {
                    if (result == null) result = new HashSet<string>(StringComparer.Ordinal);
                    if (!result.Add(frame.Name)) Fail(frame.Start, 1, "Captures with the same name may both participate in a match.");
                }
                return result;
            }

            private static HashSet<string> Merge(HashSet<string> left, HashSet<string> right, bool sequence, int start)
            {
                if (left == null) return right;
                if (right == null) return left;
                // Ownership passes from a completed child to its parent. Merge the
                // smaller set into the larger set instead of copying at every depth.
                if (left.Count < right.Count) { var temporary = left; left = right; right = temporary; }
                foreach (string name in right)
                    if (!left.Add(name) && sequence) Fail(start, 1, "Captures with the same name may both participate in a match.");
                return left;
            }

            private static void CountCaptures(string body, bool sets, out int captures, out bool named)
            {
                captures = 0; named = false;
                int depth = 0;
                for (int cursor = 0; cursor < body.Length; cursor++)
                {
                    char ch = body[cursor];
                    if (ch == '\\') { cursor++; continue; }
                    if (ch == '[' && (depth == 0 || sets)) { depth++; continue; }
                    if (ch == ']' && depth != 0) { depth--; continue; }
                    if (depth != 0 || ch != '(') continue;
                    if (cursor + 1 >= body.Length || body[cursor + 1] != '?') captures++;
                    else if (cursor + 2 < body.Length && body[cursor + 2] == '<' &&
                        (cursor + 3 >= body.Length || body[cursor + 3] != '=' && body[cursor + 3] != '!'))
                    { captures++; named = true; }
                }
            }

            private static bool Digit(char ch) => ch >= '0' && ch <= '9';
            private static bool AsciiLetter(char ch) => ch >= 'A' && ch <= 'Z' || ch >= 'a' && ch <= 'z';
            private static int Hex(char ch) => ch >= '0' && ch <= '9' ? ch - '0' : ch >= 'a' && ch <= 'f' ? ch - 'a' + 10 : ch >= 'A' && ch <= 'F' ? ch - 'A' + 10 : -1;
            private static void Fail(int start, int length, string message) => throw new SyntaxFailure(start, length, message);
        }

        private sealed class Frame
        {
            internal Frame(int start, int kind, string name)
            { Start = start; Kind = kind; Name = name; }
            internal int Start { get; }
            internal int Kind { get; }
            internal string Name { get; }
            internal int Term { get; set; }
            internal HashSet<string> Sequence { get; set; }
            internal HashSet<string> Alternatives { get; set; }
        }

        private readonly struct Atom
        {
            internal static readonly Atom Set = default;
            internal Atom(int point) { Character = true; Point = point; }
            internal bool Character { get; }
            internal int Point { get; }
        }

        private sealed class Reference
        {
            internal Reference(string name, int start, int length) { Name = name; Start = start; Length = length; }
            internal string Name { get; }
            internal int Start { get; }
            internal int Length { get; }
        }

        private sealed class SyntaxFailure : Exception
        {
            internal SyntaxFailure(int offset, int length, string message) : base(message) { Offset = offset; Length = length; }
            internal int Offset { get; }
            internal int Length { get; }
        }
    }
}
