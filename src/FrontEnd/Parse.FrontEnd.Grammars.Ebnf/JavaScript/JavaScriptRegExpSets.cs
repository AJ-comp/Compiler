using System;
using System.Collections.Generic;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Reads the ECMAScript Unicode-sets character-class grammar. A stack of class
    /// frames keeps deeply nested input from consuming the C# call stack.
    /// See <see href="../../../../docs/ko/javascript-regexp.html#unicode-sets">Unicode 집합 검사</see>.
    /// </summary>
    internal static class JavaScriptRegExpSets
    {
        internal static bool TryRead(string body, ref int index,
            out bool mayContainStrings, out int errorOffset, out int errorLength,
            out string message)
        {
            var reader = new Reader(body, index);
            bool success = reader.Read(out mayContainStrings);
            if (success) index = reader.Index;
            errorOffset = reader.ErrorOffset;
            errorLength = reader.ErrorLength;
            message = reader.Message;
            return success;
        }

        private enum Operation { None, Union, Intersection, Subtraction }

        private sealed class Frame
        {
            internal int Start;
            internal bool Negated;
            internal bool NeedsOperand = true;
            internal bool HasOperand;
            internal bool HasRange;
            internal bool MayContainStrings;
            internal Operation Operation;
        }

        private readonly struct Operand
        {
            internal readonly bool IsCharacter;
            internal readonly int Character;
            internal readonly bool IsRange;
            internal readonly bool MayContainStrings;
            internal Operand(int character)
            {
                IsCharacter = true;
                Character = character;
                IsRange = false;
                MayContainStrings = false;
            }
            internal Operand(bool mayContainStrings, bool isRange = false)
            {
                IsCharacter = false;
                Character = 0;
                IsRange = isRange;
                MayContainStrings = mayContainStrings;
            }
        }

        private sealed class Reader
        {
            private readonly string body;
            private readonly List<Frame> frames = new List<Frame>();
            internal int Index { get; private set; }
            internal int ErrorOffset { get; private set; }
            internal int ErrorLength { get; private set; }
            internal string Message { get; private set; }

            internal Reader(string body, int index)
            {
                this.body = body;
                Index = index;
                ErrorOffset = index;
            }

            internal bool Read(out bool mayContainStrings)
            {
                mayContainStrings = false;
                if (body == null || Index < 0 || Index >= body.Length || body[Index] != '[')
                    return Fail(Index, 1, "A Unicode-sets class must start with '['.");
                Open();
                while (frames.Count > 0)
                {
                    var frame = frames[frames.Count - 1];
                    if (Index >= body.Length)
                        return Fail(frame.Start, body.Length - frame.Start,
                            "Unterminated Unicode-sets character class.");
                    char current = body[Index];
                    if (current == ']')
                    {
                        if (frame.NeedsOperand && frame.HasOperand)
                            return Fail(Index, 1, "A set operator requires a right-hand operand.");
                        if (frame.Negated && frame.MayContainStrings)
                            return Fail(frame.Start, Index - frame.Start + 1,
                                "A negated Unicode-sets class may not contain strings.");
                        Index++;
                        frames.RemoveAt(frames.Count - 1);
                        bool strings = !frame.Negated && frame.MayContainStrings;
                        if (frames.Count == 0)
                        {
                            mayContainStrings = strings;
                            return true;
                        }
                        if (!Add(frames[frames.Count - 1], new Operand(strings))) return false;
                        continue;
                    }
                    if (!frame.NeedsOperand)
                    {
                        Operation operation;
                        if (At("&&")) operation = Operation.Intersection;
                        else if (At("--")) operation = Operation.Subtraction;
                        else
                        {
                            if (frame.Operation == Operation.Intersection ||
                                frame.Operation == Operation.Subtraction)
                                return Fail(Index, 1,
                                    "Intersection or subtraction operands must use the same set operator.");
                            frame.Operation = Operation.Union;
                            frame.NeedsOperand = true;
                            continue;
                        }
                        if (frame.HasRange || (frame.Operation != Operation.None &&
                            frame.Operation != operation))
                            return Fail(Index, 2,
                                "Combining union, intersection or subtraction requires nested character classes.");
                        // The && production excludes a third unescaped ampersand.
                        if (operation == Operation.Intersection && Index + 2 < body.Length &&
                            body[Index + 2] == '&')
                            return Fail(Index, 3, "The intersection operator is '&&' and must not be followed by '&'.");
                        frame.Operation = operation;
                        frame.NeedsOperand = true;
                        Index += 2;
                        continue;
                    }
                    if (current == '[')
                    {
                        Open();
                        continue;
                    }
                    int start = Index;
                    if (!ReadOperand(out var operand)) return false;
                    if (Index < body.Length && body[Index] == '-' && !At("--"))
                    {
                        if (!operand.IsCharacter)
                            return Fail(start, Index - start + 1,
                                "A character range requires single-character endpoints.");
                        Index++;
                        int lastStart = Index;
                        if (!ReadCharacter(out int last)) return false;
                        if (operand.Character > last)
                            return Fail(start, Index - start,
                                "The character range is out of order.");
                        if (frame.Operation == Operation.Intersection ||
                            frame.Operation == Operation.Subtraction)
                            return Fail(lastStart, Math.Max(1, Index - lastStart),
                                "A range used in intersection or subtraction requires a nested character class.");
                        operand = new Operand(false, true);
                    }
                    if (!Add(frame, operand)) return false;
                }
                return false;
            }

            private void Open()
            {
                int start = Index++;
                bool negated = Index < body.Length && body[Index] == '^';
                if (negated) Index++;
                frames.Add(new Frame { Start = start, Negated = negated });
            }

            private bool Add(Frame frame, Operand operand)
            {
                if (!frame.NeedsOperand)
                    return Fail(Index, 1, "An appropriate set operator is required between these operands.");
                if (!frame.HasOperand) frame.MayContainStrings = operand.MayContainStrings;
                else if (frame.Operation == Operation.Intersection)
                    frame.MayContainStrings &= operand.MayContainStrings;
                else if (frame.Operation != Operation.Subtraction)
                    frame.MayContainStrings |= operand.MayContainStrings;
                frame.HasOperand = true;
                frame.HasRange |= operand.IsRange;
                frame.NeedsOperand = false;
                return true;
            }

            private bool ReadOperand(out Operand operand)
            {
                operand = default;
                if (At("\\q{"))
                {
                    Index += 3;
                    int characters = 0;
                    bool strings = false;
                    while (Index < body.Length)
                    {
                        if (body[Index] == '|' || body[Index] == '}')
                        {
                            strings |= characters != 1;
                            if (body[Index++] == '}')
                            {
                                operand = new Operand(strings);
                                return true;
                            }
                            characters = 0;
                            continue;
                        }
                        if (!ReadCharacter(out _)) return false;
                        // More than one code point is enough for MayContainStrings;
                        // counting the whole string is unnecessary.
                        if (characters < 2) characters++;
                    }
                    return Fail(Index, 1, "Unterminated class string disjunction.");
                }
                if (Index + 1 < body.Length && body[Index] == '\\')
                {
                    char escaped = body[Index + 1];
                    if ("dDsSwW".IndexOf(escaped) >= 0)
                    {
                        Index += 2;
                        operand = new Operand(false);
                        return true;
                    }
                    if (escaped == 'p' || escaped == 'P')
                    {
                        int start = Index;
                        Index += 2;
                        if (Index >= body.Length || body[Index++] != '{')
                            return Fail(start, Index - start,
                                "A Unicode property escape requires a property name in braces.");
                        int payloadStart = Index;
                        while (Index < body.Length && body[Index] != '}') Index++;
                        if (Index >= body.Length)
                            return Fail(start, Index - start,
                                "Unterminated Unicode property escape.");
                        string payload = body.Substring(payloadStart, Index - payloadStart);
                        Index++;
                        if (!JavaScriptRegExpProperties.IsValid(payload, true, escaped == 'P',
                            out bool strings))
                            return Fail(start, Index - start,
                                "This Unicode property escape is not valid in the current mode.");
                        operand = new Operand(strings);
                        return true;
                    }
                }
                if (!ReadCharacter(out int character)) return false;
                operand = new Operand(character);
                return true;
            }

            private bool ReadCharacter(out int value)
            {
                value = 0;
                int start = Index;
                if (Index >= body.Length)
                    return Fail(Index, 1, "A character is required in this set expression.");
                char current = body[Index++];
                if (current != '\\')
                {
                    if ("()[]{}/-|".IndexOf(current) >= 0)
                        return Fail(start, 1,
                            "A Unicode-sets syntax character must be escaped when used as a character.");
                    if (Index < body.Length && body[Index] == current &&
                        "&!#$%*+,.:;<=>?@^`~".IndexOf(current) >= 0)
                        return Fail(start, 2,
                            "Reserved double punctuators must be escaped in a Unicode-sets class.");
                    // This is the RegExp Pattern grammar, whose SourceCharacter can
                    // include line terminators. The JavaScript literal lexer owns
                    // the separate restriction on raw line terminators in /.../.
                    value = current;
                    if (char.IsHighSurrogate(current) && Index < body.Length &&
                        char.IsLowSurrogate(body[Index]))
                        value = char.ConvertToUtf32(current, body[Index++]);
                    return true;
                }
                if (Index >= body.Length)
                    return Fail(start, 1, "An escape requires a following character.");
                char escaped = body[Index++];
                switch (escaped)
                {
                    case 'b': value = 8; return true;
                    case 'f': value = 12; return true;
                    case 'n': value = 10; return true;
                    case 'r': value = 13; return true;
                    case 't': value = 9; return true;
                    case 'v': value = 11; return true;
                    case '0':
                        if (Index < body.Length && body[Index] >= '0' && body[Index] <= '9')
                            return Fail(start, Index - start + 1,
                                "A Unicode null escape must not be followed by a decimal digit.");
                        return true;
                    case 'c':
                        if (Index >= body.Length || !AsciiLetter(body[Index]))
                            return Fail(start, Index - start,
                                "A control escape requires an ASCII letter.");
                        value = body[Index++] % 32;
                        return true;
                    case 'x':
                        return ReadHex(2, start, out value);
                    case 'u':
                        if (Index < body.Length && body[Index] == '{')
                        {
                            Index++;
                            int digits = 0;
                            while (Index < body.Length && Hex(body[Index]) >= 0)
                            {
                                int digit = Hex(body[Index++]);
                                if (value > (0x10FFFF - digit) / 16)
                                    return Fail(start, Index - start,
                                        "A Unicode code point must not exceed 0x10FFFF.");
                                value = value * 16 + digit;
                                digits++;
                            }
                            if (digits == 0 || Index >= body.Length || body[Index++] != '}')
                                return Fail(start, Math.Max(1, Index - start),
                                    "A Unicode code-point escape requires hexadecimal digits and a closing brace.");
                            return true;
                        }
                        if (!ReadHex(4, start, out value)) return false;
                        // Only two adjacent four-digit Unicode escapes form this
                        // grammar production; a brace escape remains a code point.
                        if (value >= 0xD800 && value <= 0xDBFF && At("\\u") &&
                            Index + 6 <= body.Length)
                        {
                            int trail = 0;
                            bool valid = true;
                            for (int offset = 2; offset < 6; offset++)
                            {
                                int digit = Hex(body[Index + offset]);
                                if (digit < 0) { valid = false; break; }
                                trail = trail * 16 + digit;
                            }
                            if (valid && trail >= 0xDC00 && trail <= 0xDFFF)
                            {
                                value = 0x10000 + ((value - 0xD800) << 10) + trail - 0xDC00;
                                Index += 6;
                            }
                        }
                        return true;
                    default:
                        if ("^$\\.*+?()[]{}|/&-!#%,:;<=>@`~".IndexOf(escaped) >= 0)
                        {
                            value = escaped;
                            return true;
                        }
                        return Fail(start, Index - start,
                            "Invalid character escape in a Unicode-sets class.");
                }
            }

            private bool ReadHex(int count, int start, out int value)
            {
                value = 0;
                for (int offset = 0; offset < count; offset++)
                {
                    if (Index >= body.Length || Hex(body[Index]) < 0)
                        return Fail(start, Math.Max(1, Index - start),
                            "The escape has an invalid number of hexadecimal digits.");
                    value = value * 16 + Hex(body[Index++]);
                }
                return true;
            }

            private bool At(string text)
            {
                if (Index + text.Length > body.Length) return false;
                for (int offset = 0; offset < text.Length; offset++)
                    if (body[Index + offset] != text[offset]) return false;
                return true;
            }

            private bool Fail(int offset, int length, string message)
            {
                ErrorOffset = Math.Max(0, Math.Min(offset, body?.Length ?? 0));
                ErrorLength = Math.Max(1, Math.Min(length, (body?.Length ?? 0) - ErrorOffset));
                Message = message;
                return false;
            }

            private static bool AsciiLetter(char value) =>
                (value >= 'a' && value <= 'z') || (value >= 'A' && value <= 'Z');

            private static int Hex(char value)
            {
                if (value >= '0' && value <= '9') return value - '0';
                if (value >= 'a' && value <= 'f') return value - 'a' + 10;
                if (value >= 'A' && value <= 'F') return value - 'A' + 10;
                return -1;
            }
        }
    }
}
