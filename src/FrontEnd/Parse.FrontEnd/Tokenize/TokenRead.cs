using System;

namespace Janglim.FrontEnd.Tokenize
{
    /// <summary>
    /// A lookahead token and the checkpoint to adopt after consuming it.
    /// EOF keeps the token ordinal and previous token while including trailing trivia.
    /// <para><see href="../../../../docs/ko/parser-driven-lexing.html#exchange">Parser-driven lexing: token exchange</see></para>
    /// </summary>
    public sealed class TokenRead
    {
        public TokenData Token { get; }
        public TokenCursor Next { get; }

        public TokenRead(TokenData token, TokenCursor next)
        {
            Token = token ?? throw new ArgumentNullException(nameof(token));
            Next = next ?? throw new ArgumentNullException(nameof(next));
        }
    }
}
