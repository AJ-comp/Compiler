using System.Text;

namespace Janglim.FrontEnd.Tokenize
{
    public partial class LexingData
    {
        /// <summary>
        /// Skipped source text after the last parsing token. If there are no parsing tokens,
        /// this contains all skipped input. The lexer does not create an EOF token.
        /// </summary>
        public TokenTrivia TrailingTrivia { get; private set; } = new TokenTrivia(0, string.Empty);

        private void RebuildTrivia()
        {
            var skippedText = new StringBuilder();
            int triviaStartIndex = _tokensForView.Count == 0 ? 0 : _tokensForView[0].StartIndex;

            // Rebuild after filtering and offset adjustment. A trivia-only edit can change
            // the next parsing token's context even when that token itself was not replaced.
            foreach (var token in _tokensForView)
            {
                if (!IsParsingToken(token))
                {
                    // LeadingTrivia describes parsing tokens, not individual skipped tokens.
                    token.LeadingTrivia = new TokenTrivia(token.StartIndex, string.Empty);
                    skippedText.Append(token.Data);
                    continue;
                }

                token.LeadingTrivia = new TokenTrivia(triviaStartIndex, skippedText.ToString());
                skippedText.Clear();
                triviaStartIndex = token.EndIndex + 1;
            }

            TrailingTrivia = new TokenTrivia(triviaStartIndex, skippedText.ToString());
        }
    }
}
