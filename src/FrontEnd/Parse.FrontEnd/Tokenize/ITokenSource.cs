namespace Janglim.FrontEnd.Tokenize
{
    /// <summary>
    /// Provides a parsing token at a source checkpoint under an explicit lexical goal.
    /// Peeking does not consume input; the parser adopts <see cref="TokenRead.Next"/> after Shift.
    /// <para><see href="../../../../docs/ko/parser-driven-lexing.html#exchange">Parser-driven lexing: token exchange</see></para>
    /// </summary>
    public interface ITokenSource
    {
        TokenCursor Start { get; }
        int Length { get; }
        /// <summary>
        /// The immutable policy used to prepare this source and select its lexical goals.
        /// Peek must support every goal registered in this policy. The parser captures the policy
        /// once per parse and shares it with all prediction candidates.
        /// </summary>
        LexicalPolicy Policy { get; }

        /// <summary>
        /// Reads without changing the source or checkpoint. The goal must be the same instance
        /// registered in <see cref="LexicalPolicy.Goals"/> on <see cref="Policy"/>;
        /// terminal membership also uses object identity.
        /// </summary>
        /// <remarks>
        /// A real token must advance the source offset to its EndIndex + 1, increment TokenIndex
        /// by one, and become Next.PreviousToken. LeadingTrivia must cover exactly the source gap
        /// from start.SourceOffset to the token's StartIndex. EOF has empty text at Length;
        /// its next cursor stays at Length and preserves TokenIndex and PreviousToken.
        /// Every returned cursor must retain the source identity of the supplied checkpoint.
        /// Repeated reads must not share mutable token objects across independent parses.
        /// </remarks>
        TokenRead Peek(TokenCursor start, LexicalGoal goal);
    }
}
