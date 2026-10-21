using System;
using System.Runtime.CompilerServices;

namespace Janglim.FrontEnd.Tokenize
{
    /// <summary>
    /// An immutable input checkpoint, including its source owner and branch-local token context.
    /// Offsets use UTF-16 code units; the previous token is compared by reference.
    /// <para><see href="../../../../docs/ko/parser-driven-lexing.html#exchange">Parser-driven lexing: token exchange</see></para>
    /// </summary>
    public sealed class TokenCursor : IEquatable<TokenCursor>
    {
        public object SourceIdentity { get; }
        public int SourceOffset { get; }
        public int TokenIndex { get; }
        public TokenData PreviousToken { get; }
        public string LexicalState { get; }

        public TokenCursor(object sourceIdentity, int sourceOffset, int tokenIndex = 0,
            TokenData previousToken = null, string lexicalState = "")
        {
            SourceIdentity = sourceIdentity ?? throw new ArgumentNullException(nameof(sourceIdentity));
            if (sourceOffset < 0) throw new ArgumentOutOfRangeException(nameof(sourceOffset));
            if (tokenIndex < 0) throw new ArgumentOutOfRangeException(nameof(tokenIndex));
            SourceOffset = sourceOffset;
            TokenIndex = tokenIndex;
            PreviousToken = previousToken;
            LexicalState = lexicalState ?? throw new ArgumentNullException(nameof(lexicalState));
        }

        public bool Equals(TokenCursor other)
            => !ReferenceEquals(other, null) && ReferenceEquals(SourceIdentity, other.SourceIdentity)
               && SourceOffset == other.SourceOffset && TokenIndex == other.TokenIndex
               && ReferenceEquals(PreviousToken, other.PreviousToken)
               && string.Equals(LexicalState, other.LexicalState, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is TokenCursor other && Equals(other);

        public override int GetHashCode()
            => HashCode.Combine(RuntimeHelpers.GetHashCode(SourceIdentity), SourceOffset, TokenIndex,
                ReferenceEquals(PreviousToken, null) ? 0 : RuntimeHelpers.GetHashCode(PreviousToken),
                StringComparer.Ordinal.GetHashCode(LexicalState));
    }
}
