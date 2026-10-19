using System;

namespace Janglim.FrontEnd.RegularGrammar
{
    /// <summary>The input-token boundary being examined by a grammar predicate.</summary>
    public sealed class PredicateContext
    {
        public TokenData PreviousToken { get; }
        public TokenData NextToken { get; }
        /// <summary>The zero-based input index, including the synthetic EOF position.
        /// During recovery this indexes the logical input containing replacement tokens.
        /// Isolated parsing-unit calls without an explicit context use -1.</summary>
        public int TokenIndex { get; }
        public bool IsEndOfInput => NextToken.Kind is EndMarker;

        public PredicateContext(TokenData previousToken, TokenData nextToken, int tokenIndex)
        {
            if (tokenIndex < -1) throw new ArgumentOutOfRangeException(nameof(tokenIndex));
            PreviousToken = previousToken;
            NextToken = nextToken ?? throw new ArgumentNullException(nameof(nextToken));
            TokenIndex = tokenIndex;
        }
    }
}
