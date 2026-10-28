using System;
using System.Text;

namespace Janglim.FrontEnd.Grammars.JavaScript
{
    /// <summary>
    /// Token patterns for the Script and Module profiles of ECMAScript 2026.
    /// The EBNF asset supplies the syntactic productions; these patterns expand its lexical
    /// placeholders. Identifier properties are pinned to Unicode 17.0.0, including astral
    /// code points, instead of depending on the host's Unicode categories or word boundaries.
    /// <para><see href="https://tc39.es/ecma262/2026/multipage/ecmascript-language-lexical-grammar.html">ECMAScript 2026 lexical grammar, clauses 12.2–12.9.5</see></para>
    /// </summary>
    internal static class JavaScriptTokenPatterns
    {
        // Identifier escapes are recognized here, then their decoded code point is checked
        // below: a syntactically correct \u0030 still cannot start an identifier.
        private const string UnicodeEscape = @"\\u(?:[0-9A-Fa-f]{4}|\{[0-9A-Fa-f]+\})";
        private const string DecimalDigits = @"[0-9](?:_?[0-9])*";
        private const string DecimalInteger = @"(?:0|[1-9](?:_?[0-9])*)";
        private const string Exponent = @"[eE][+-]?[0-9](?:_?[0-9])*";
        private const string RadixInteger = @"(?:0[xX][0-9A-Fa-f](?:_?[0-9A-Fa-f])*|0[bB][01](?:_?[01])*|0[oO][0-7](?:_?[0-7])*)";

        internal static readonly string IdentifierName =
            "(?:" + JavaScriptUnicode.StartPattern + "|" + UnicodeEscape + ")" +
            "(?:" + JavaScriptUnicode.PartPattern + "|" + UnicodeEscape + ")*";

        // Longest matching alternatives come first within the authored .NET regex.
        // The atomic group prevents an invalid `1.e` from falling back to `1` plus `.e`.
        // Clause 12.9.3 forbids an IdentifierStart or decimal digit immediately after a number.
        // Reject any backslash there as well, so malformed escaped suffixes cannot split a number.
        internal static readonly string NumberLiteral =
            "(?>(?:" + RadixInteger + "n?|" + DecimalInteger + "n|" +
            DecimalInteger + "(?:\\.(?:" + DecimalDigits + ")?)?(?:" + Exponent + ")?|" +
            "\\." + DecimalDigits + "(?:" + Exponent + ")?))" +
            "(?!(?:" + JavaScriptUnicode.StartPattern + @"|[0-9\\]))";

        internal static string NumericLiteral => NumberLiteral;

        // A leading-zero decimal must contain 8 or 9; unlike a legacy octal integer,
        // it can have a decimal fraction or exponent. Neither legacy integer spelling
        // accepts separators or a BigInt suffix (12.9.3 and its strict-code early error).
        private const string NonOctalDecimalInteger = @"0[0-7]*[89][0-9]*";
        private static readonly string NonStrictNumericLiteral =
            "(?>(?:" + RadixInteger + "n?|" + DecimalInteger + "n|" +
            NonOctalDecimalInteger + "(?:\\.(?:" + DecimalDigits + ")?)?(?:" + Exponent + ")?|" +
            "0[0-7]+|" + DecimalInteger + "(?:\\.(?:" + DecimalDigits + ")?)?(?:" + Exponent + ")?|" +
            "\\." + DecimalDigits + "(?:" + Exponent + ")?))" +
            "(?!(?:" + JavaScriptUnicode.StartPattern + @"|[0-9\\]))";

        internal static string NumericLiteralFor(JavaScriptMode mode)
            => mode == JavaScriptMode.NonStrict ? NonStrictNumericLiteral : NumericLiteral;

        // Identity escapes (for example \q), Unicode escapes and line continuations are
        // allowed. Decimal/octal escapes are excluded by this profile's strict-code rules.
        private const string StringEscape = @"\\(?:0(?![0-9])|x[0-9A-Fa-f]{2}|u(?:[0-9A-Fa-f]{4}|\{[0-9A-Fa-f]+\})|\r\n|[\r\n\u2028\u2029]|[^0-9xu\r\n\u2028\u2029])";
        internal static readonly string StringLiteral =
            "(?:\"(?:[^\"\\\\\\r\\n]|" + StringEscape + ")*\"|'(?:[^'\\\\\\r\\n]|" + StringEscape + ")*')";

        // Every decimal escape is lexically allowed in non-strict strings. Reading
        // one escaped digit here is enough to find the quoted token boundary; the
        // exact octal escape length matters when decoding or locating a strict error.
        private const string NonStrictStringEscape = @"\\(?:[0-9]|x[0-9A-Fa-f]{2}|u(?:[0-9A-Fa-f]{4}|\{[0-9A-Fa-f]+\})|\r\n|[\r\n\u2028\u2029]|[^0-9xu\r\n\u2028\u2029])";
        private static readonly string NonStrictStringLiteral =
            "(?:\"(?:[^\"\\\\\\r\\n]|" + NonStrictStringEscape + ")*\"|'(?:[^'\\\\\\r\\n]|" + NonStrictStringEscape + ")*')";

        internal static string StringLiteralFor(JavaScriptMode mode)
            => mode == JavaScriptMode.NonStrict ? NonStrictStringLiteral : StringLiteral;

        private const string RegExpEscape = @"\\[^\r\n\u2028\u2029]";
        private const string RegExpClass = @"\[(?:[^\\\]\r\n\u2028\u2029]|\\[^\r\n\u2028\u2029])*\]";
        // The lexical grammar only finds the literal's boundary. The stricter RegExpPattern
        // grammar (clause 22.2) is a separate parse; a .NET Regex is not an ECMAScript validator.
        internal static readonly string RegularExpressionLiteral =
            "/(?:[^*/\\\\[\\r\\n\\u2028\\u2029]|" + RegExpEscape + "|" + RegExpClass + ")" +
            "(?:[^/\\\\[\\r\\n\\u2028\\u2029]|" + RegExpEscape + "|" + RegExpClass + ")*/" +
            JavaScriptUnicode.PartPattern + "*";

        // ECMAScript whitespace is deliberately narrower than the host regex \s class:
        // U+0085 NEXT LINE is not whitespace in ECMAScript.
        internal const string WhiteSpace = @"[\u0009\u000B\u000C\u0020\u00A0\u1680\u2000-\u200A\u202F\u205F\u3000\uFEFF\r\n\u2028\u2029]+";
        internal const string Comment = @"/\*[\s\S]*?\*/|//[^\r\n\u2028\u2029]*|\A\#\![^\r\n\u2028\u2029]*";

        private const string HorizontalWhiteSpace = @"[\u0009\u000B\u000C\u0020\u00A0\u1680\u2000-\u200A\u202F\u205F\u3000\uFEFF]";
        private const string SingleLineBlockComment = @"/\*(?:(?!\*/)[^\r\n\u2028\u2029])*\*/";
        private const string HtmlClosePrefix = HorizontalWhiteSpace + "*(?:" + SingleLineBlockComment + HorizontalWhiteSpace + "*)*";
        private const string HtmlCloseBody = @"-->[^\r\n\u2028\u2029]*";
        private static readonly string CommentWithHtml =
            // A block comment containing a line terminator may absorb an HTML close
            // comment after its closing delimiter, even when it began after code.
            @"(?=/\*(?:(?!\*/)[\s\S])*[\r\n\u2028\u2029])/\*[\s\S]*?\*/(?:" + HtmlClosePrefix + HtmlCloseBody + ")?|" +
            Comment + @"|<!--[^\r\n\u2028\u2029]*|" +
            // The token source skips whitespace/comments before this rule is tried.
            // Inspect the original prefix rather than replacing source characters,
            // preserving offsets, exact trivia and line-break-sensitive predicates.
            @"(?=-->)(?<=(?:\A|[\r\n\u2028\u2029])" + HtmlClosePrefix + ")" + HtmlCloseBody;

        /// <summary>Annex B.1.1 adds HTML-like comments to Script input, including
        /// strict Script code. Module input always uses the standard comment rules.</summary>
        internal static string CommentFor(bool webCompatibility, bool module)
            => webCompatibility && !module ? CommentWithHtml : Comment;

        /// <summary>Checks lexical early errors after the common lexer returns a token.</summary>
        internal static void Validate(TokenData token)
            => Validate(token, JavaScriptMode.Strict);

        /// <summary>Checks mode-independent errors immediately. Non-strict tokenization
        /// defers strict-only legacy literal errors until the containing tree's mode is known.</summary>
        internal static void Validate(TokenData token, JavaScriptMode mode)
        {
            if (token?.Input == null) return;
            switch (token.Kind?.Caption)
            {
                case "IdentifierName":
                    DecodeIdentifier(token.Input, token.StartIndex);
                    break;
                case "PrivateIdentifier":
                    DecodeIdentifier(token.Input.Substring(1), token.StartIndex + 1);
                    break;
                case "StringLiteral":
                    ValidateStringCodePoints(token);
                    break;
                case "RegularExpressionLiteral":
                    ValidateRegExpFlags(token);
                    break;
            }
            if (mode == JavaScriptMode.Strict)
            {
                var diagnostic = GetStrictModeDiagnostic(token);
                if (diagnostic != null)
                    throw new JavaScriptSyntaxException(diagnostic.Message, diagnostic.SourceOffset, diagnostic.Length);
            }
        }

        /// <summary>Reports the first legacy literal form prohibited by strict code.
        /// The caller decides whether the containing production is strict, including
        /// class bodies and literals before a later Use Strict Directive (12.9.4.1).</summary>
        internal static JavaScriptDiagnostic GetStrictModeDiagnostic(TokenData token)
        {
            string text = token?.Input;
            if (text == null) return null;
            if (token.Kind?.Caption == "NumericLiteral" && text.Length > 1 &&
                text[0] == '0' && text[1] >= '0' && text[1] <= '9')
                return new JavaScriptDiagnostic("strict_legacy_numeric_literal",
                    "Legacy leading-zero numeric literals are not allowed in strict code.",
                    token.StartIndex, text.Length);
            if (token.Kind?.Caption != "StringLiteral") return null;
            for (int index = 1; index < text.Length - 1; index++)
            {
                if (text[index] != '\\') continue;
                int begin = index;
                if (++index >= text.Length - 1) break;
                char digit = text[index];
                if (digit < '0' || digit > '9') continue;
                // A bare \\0 is the ordinary NUL escape. Only a following decimal
                // digit makes that spelling a legacy escape, including \\08/\\09.
                if (digit == '0' && (index + 1 >= text.Length - 1 ||
                    text[index + 1] < '0' || text[index + 1] > '9')) continue;
                int length = 2;
                if (digit <= '7')
                {
                    int maximumDigits = digit <= '3' ? 3 : 2;
                    for (int consumed = 1; consumed < maximumDigits && index + 1 < text.Length - 1 &&
                        text[index + 1] >= '0' && text[index + 1] <= '7'; consumed++)
                    {
                        index++;
                        length++;
                    }
                }
                return new JavaScriptDiagnostic("strict_legacy_string_escape",
                    "Legacy octal and decimal escapes are not allowed in strict string literals.",
                    token.StartIndex + begin, length);
            }
            return null;
        }

        internal static string DecodeIdentifier(string text) => DecodeIdentifier(text, 0);

        internal static string DecodeIdentifier(string text, int sourceOffset)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            var decoded = new StringBuilder(text.Length);
            bool first = true;
            for (int index = 0; index < text.Length;)
            {
                int begin = index;
                int codePoint;
                if (text[index] == '\\') codePoint = ReadUnicodeEscape(text, ref index, sourceOffset);
                else if (char.IsHighSurrogate(text[index]) && index + 1 < text.Length &&
                    char.IsLowSurrogate(text[index + 1]))
                {
                    codePoint = char.ConvertToUtf32(text[index], text[index + 1]);
                    index += 2;
                }
                else codePoint = text[index++];

                if (!(first ? JavaScriptUnicode.IsStart(codePoint) : JavaScriptUnicode.IsPart(codePoint)))
                    throw new JavaScriptSyntaxException(
                        first ? "The escaped code point cannot start an identifier." :
                        "The escaped code point cannot continue an identifier.",
                        sourceOffset + begin, index - begin);
                decoded.Append(char.ConvertFromUtf32(codePoint));
                first = false;
            }
            return decoded.ToString();
        }

        private static void ValidateStringCodePoints(TokenData token)
        {
            string text = token.Input;
            for (int index = 1; index < text.Length - 1; index++)
            {
                if (text[index] != '\\') continue;
                if (index + 1 < text.Length - 1 && text[index + 1] == 'u')
                {
                    ReadUnicodeEscape(text, ref index, token.StartIndex);
                    index--;
                }
                else index++; // Escaped backslashes must not turn the next plain 'u' into an escape.
            }
        }

        private static int ReadUnicodeEscape(string text, ref int index, int sourceOffset)
        {
            int begin = index;
            if (index + 2 >= text.Length || text[index] != '\\' || text[index + 1] != 'u')
                throw new JavaScriptSyntaxException("Invalid Unicode escape.", sourceOffset + begin, 1);
            index += 2;
            bool braced = text[index] == '{';
            if (braced) index++;
            int digits = 0;
            int value = 0;
            while (index < text.Length && (!braced ? digits < 4 : text[index] != '}'))
            {
                int digit = HexDigit(text[index]);
                if (digit < 0) break;
                // Leading zeros have no arbitrary length cap; saturate large values instead
                // of overflowing an int for escapes such as \u{FFFFFFFFFFFFFFFFFFFFFFFF}.
                value = value > 0x10FFFF ? 0x110000 : value * 16 + digit;
                index++;
                digits++;
            }
            if (digits == 0 || (!braced && digits != 4) ||
                (braced && (index >= text.Length || text[index] != '}')) || value > 0x10FFFF)
                throw new JavaScriptSyntaxException("Invalid Unicode escape code point.",
                    sourceOffset + begin, Math.Max(1, index - begin));
            if (braced) index++;
            return value;
        }

        private static int HexDigit(char value)
        {
            if (value >= '0' && value <= '9') return value - '0';
            if (value >= 'A' && value <= 'F') return value - 'A' + 10;
            if (value >= 'a' && value <= 'f') return value - 'a' + 10;
            return -1;
        }

        private static void ValidateRegExpFlags(TokenData token)
        {
            string text = token.Input;
            int closingSlash = text.LastIndexOf('/');
            int seen = 0;
            const string valid = "dgimsuvy";
            for (int index = closingSlash + 1; index < text.Length; index++)
            {
                int bit = valid.IndexOf(text[index]);
                if (bit < 0 || (seen & (1 << bit)) != 0)
                    throw new JavaScriptSyntaxException("Invalid or duplicate regular-expression flag.",
                        token.StartIndex + index,
                        char.IsHighSurrogate(text[index]) && index + 1 < text.Length && char.IsLowSurrogate(text[index + 1]) ? 2 : 1);
                seen |= 1 << bit;
            }
            if ((seen & (1 << valid.IndexOf('u'))) != 0 && (seen & (1 << valid.IndexOf('v'))) != 0)
                throw new JavaScriptSyntaxException("Regular-expression flags 'u' and 'v' cannot be combined.",
                    token.StartIndex + closingSlash + 1, text.Length - closingSlash - 1);
        }
    }
}
