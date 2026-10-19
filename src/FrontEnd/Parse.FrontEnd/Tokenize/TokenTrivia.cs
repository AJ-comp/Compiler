using System;

namespace Janglim.FrontEnd.Tokenize
{
    /// <summary>
    /// An immutable snapshot of source text excluded from the parsing token stream.
    /// Offsets and lengths are measured in UTF-16 code units, like <see cref="TokenCell.StartIndex"/>.
    /// </summary>
    public sealed class TokenTrivia
    {
        private static readonly char[] LineBreakCharacters = { '\r', '\n', '\u2028', '\u2029' };

        internal static TokenTrivia Empty { get; } = new TokenTrivia(-1, string.Empty);

        /// <summary>The exact skipped source text, including whitespace and whole comments.</summary>
        public string Text { get; }

        /// <summary>The inclusive source offset at which this trivia starts.</summary>
        public int StartIndex { get; }

        /// <summary>The number of UTF-16 code units in <see cref="Text"/>.</summary>
        public int Length => Text.Length;

        /// <summary>The exclusive end offset. An empty range has equal start and end offsets.</summary>
        public int EndIndexExclusive => StartIndex + Length;

        /// <summary>
        /// Whether this skipped text contains CR, LF, U+2028 or U+2029.
        /// This is source information; it does not insert tokens or impose a language's newline rules.
        /// </summary>
        public bool HasLineBreak { get; }

        internal TokenTrivia(int startIndex, string text)
        {
            StartIndex = startIndex;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            HasLineBreak = Text.IndexOfAny(LineBreakCharacters) >= 0;
        }

        public override string ToString() => Text;
    }
}
