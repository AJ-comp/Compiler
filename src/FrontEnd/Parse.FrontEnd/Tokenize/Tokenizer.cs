using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Janglim.FrontEnd.Tokenize
{
    public class Tokenizer
    {
        public Action<IReadOnlyList<TokenCell>> TokenizeCompleted = null;
        public Action<Match> Matched = null;

        /// <summary>
        /// Tokenizes registered rules by longest actual match, with rule priority for ties.
        /// Unknown runs remain grouped for the editor, and skipped rules remain visible here;
        /// LexingData supplies the filtered parsing stream and its trivia afterwards.
        /// </summary>
        internal IReadOnlyList<TokenCell> Tokenize(IReadOnlyList<LexicalRule> rules, string targetString, int basisIndex = 0)
        {
            if (targetString == null) throw new ArgumentNullException(nameof(targetString));
            var result = new List<TokenCell>();
            int offset = 0, line = 0, column = 0;
            int unknownStart = -1, unknownLine = 0, unknownColumn = 0;
            while (offset < targetString.Length)
            {
                var match = LexicalRuleMatcher.Select(rules, targetString, offset, out var rule);
                if (match == null)
                {
                    if (unknownStart < 0)
                    {
                        unknownStart = offset;
                        unknownLine = line;
                        unknownColumn = column;
                    }
                    AdvancePosition(targetString, offset, 1, ref line, ref column);
                    offset++;
                    continue;
                }

                if (unknownStart >= 0)
                {
                    result.Add(Unknown(targetString, basisIndex, unknownStart, offset, unknownLine, unknownColumn));
                    unknownStart = -1;
                }
                Matched?.Invoke(match);
                result.Add(new TokenCell(basisIndex + offset, line, column, match.Value, match)
                    { PatternInfo = rule.Pattern });
                AdvancePosition(targetString, offset, match.Length, ref line, ref column);
                offset += match.Length;
            }
            if (unknownStart >= 0)
                result.Add(Unknown(targetString, basisIndex, unknownStart, offset, unknownLine, unknownColumn));
            TokenizeCompleted?.Invoke(result);
            return result;
        }

        private static TokenCell Unknown(string source, int basisIndex, int start, int end, int line, int column)
            => new TokenCell(basisIndex + start, line, column, source.Substring(start, end - start), null)
                { PatternInfo = TokenPatternInfo.NotDefinedToken };

        private static void AdvancePosition(string source, int start, int length, ref int line, ref int column)
        {
            for (int i = start; i < start + length; i++)
            {
                char ch = source[i];
                if (ch == '\r' || ch == '\u2028' || ch == '\u2029')
                {
                    line++;
                    column = 0;
                }
                else if (ch == '\n')
                {
                    if (i == 0 || source[i - 1] != '\r') line++;
                    column = 0;
                }
                else column++;
            }
        }

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
        /// <remarks>
        /// The argument is one .NET regex, whose internal alternatives retain their normal
        /// ordered semantics. For longest-match selection between registered token rules,
        /// use <see cref="Lexer.Lexing(string)"/> or <see cref="Lexer.CreateTokenSource"/>.
        /// </remarks>
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
