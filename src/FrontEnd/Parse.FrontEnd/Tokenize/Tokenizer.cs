using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Parse.FrontEnd.Tokenize
{
    public class Tokenizer
    {
        public Action<IReadOnlyList<TokenCell>> TokenizeCompleted = null;
        public Action<Match> Matched = null;

        private IReadOnlyList<TokenCell> AddToken(int index, int basisIndex, int lineIndex, int colIndexInLine, string targetString, Match matchData)
        {
            List<TokenCell> result = new List<TokenCell>();

            // Not defined pattern
            if (index < matchData.Index)
                result.Add(new TokenCell(basisIndex + index, lineIndex, colIndexInLine, targetString.Substring(index, matchData.Index - index), null));

//            var tokenPatternInfo = this.GetMatchedPattern(matchData);
            result.Add(new TokenCell(basisIndex + matchData.Index, lineIndex, colIndexInLine, matchData.Value, matchData));

            return result;
        }

        /// <summary>
        /// This function register after tokenizing string into the multiple tokens.
        /// </summary>
        /// <param name="tokenizeRule">The rule to tokenize.</param>
        /// <param name="targetString">The string to tokenize.</param>
        /// <param name="basisIndex">The basis index that uses to set up a starting index of a token.</param>
        /// <returns></returns>
        public IReadOnlyList<TokenCell> Tokenize(string tokenizeRule, string targetString, int basisIndex = 0)
        {
            List<TokenCell> result = new List<TokenCell>();
            int prevEI = 0; // the end index of prev token.
            int lineIndex = 0;
            int columnIndexInLine = 0;

            // If tokenizeRule is not defined then don't work tokenize.
            if (string.IsNullOrEmpty(tokenizeRule))
            {
                result.Add(new TokenCell(0, 0, 0, targetString, null));
                return result;
            }

            // If tokenizeRule exists
            foreach (var data in Regex.Matches(targetString, tokenizeRule, RegexOptions.Multiline | RegexOptions.ExplicitCapture))
            {
                var matchData = data as Match;

                this.Matched?.Invoke(matchData);
                result.AddRange(this.AddToken(prevEI, basisIndex, lineIndex, columnIndexInLine, targetString, matchData));
                prevEI = matchData.Index + matchData.Length;
                columnIndexInLine += matchData.Length;

                var newLine = "\n";
                if (matchData.Value == newLine || matchData.Value == "\r\n")
                {
                    lineIndex++;
                    columnIndexInLine = 0;
                }
                else if (matchData.Value.Contains(newLine))        // ex : /* dfasdfasdf \r\n fsdfasdf \n asdf sdf \r\n */
                {
                    lineIndex += Regex.Matches(matchData.Value, "\n").Count;
                    columnIndexInLine = matchData.Value.Skip(matchData.Value.LastIndexOf(newLine) + newLine.Length).Count();
                }
            }

            // if a string is remained then add to the token.
            if (prevEI < targetString.Length)
                result.Add(new TokenCell(basisIndex + prevEI, lineIndex, columnIndexInLine, targetString.Substring(prevEI, targetString.Length - prevEI), null));

            this.TokenizeCompleted?.Invoke(result);

            return result;
        }

    }
}
