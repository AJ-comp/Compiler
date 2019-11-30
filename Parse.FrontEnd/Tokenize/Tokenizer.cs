using Parse.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parse.Tokenize
{
    public enum MergeDirection { Front, End };

    public class Tokenizer
    {
        private List<TokenPatternInfo> tokenPatternList = new List<TokenPatternInfo>();
        private string rule = string.Empty;


        private List<Tuple<int, string, Match>> TokenizeCore(string addString, int basisIndex)
        {
            List<Tuple<int, string, Match>> result = new List<Tuple<int, string, Match>>();

            int prevEI = 0;
            foreach (var data in Regex.Matches(addString, this.rule, RegexOptions.Multiline | RegexOptions.ExplicitCapture))
            {
                var matchData = data as Match;

                // Not defined pattern
                TokenPatternInfo patternInfo = TokenPatternInfo.NotDefinedToken;
                if (prevEI < matchData.Index)
                    result.Add(new Tuple<int, string, Match>(basisIndex + prevEI, addString.Substring(prevEI, matchData.Index - prevEI), null));

                result.Add(new Tuple<int, string, Match>(basisIndex + matchData.Index, matchData.Value, matchData));
                prevEI = matchData.Index + matchData.Length;
            }

            // if a string is remained then add to the token.
            if (prevEI < addString.Length)
                result.Add(new Tuple<int, string, Match>(basisIndex + prevEI, addString.Substring(prevEI, addString.Length - prevEI), null));

            return result;
        }

        /// <summary>
        /// This function register the tokenize rule.
        /// </summary>
        /// <param name="tokenPatternList"></param>
        public void RegistTokenizeRule(List<TokenPatternInfo> tokenPatternList)
        {
            this.tokenPatternList = tokenPatternList;

            this.rule = string.Empty;
            string generateString = string.Empty;
            foreach (var pattern in tokenPatternList)
            {
                if (pattern.OriginalPattern == string.Empty) continue;

                generateString += "a";
                this.rule += string.Format("(?<{1}>{0})|", pattern.Pattern, generateString);
                //                    patternSum += string.Format("({0})|", pattern.Pattern);
            }

            this.rule = this.rule.Substring(0, this.rule.Length - 1);
        }


        /// <summary>
        /// This function register after tokenizing string into the multiple tokens.
        /// </summary>
        /// <param name="toTokenizeString">The string to tokenize.</param>
        /// <param name="basisIndex">The basis index that uses to set up a starting index of a token.</param>
        public List<TokenCell> Tokenize(string toTokenizeString, int basisIndex = 0)
        {
            List<TokenCell> result = new List<TokenCell>();
            var coreResult = this.TokenizeCore(toTokenizeString, basisIndex);

            // It would attach the label (TokenPatternInfo).
            Parallel.For(0, coreResult.Count, index =>
            {
                var item = coreResult[index];
                TokenPatternInfo patternInfo = TokenPatternInfo.NotDefinedToken;

                if (item.Item3 == null)
                {
                    lock (result) result.Add(new TokenCell(item.Item1, item.Item2, patternInfo));
                    return;
                }

                // The number of elements in the group is 1 more than the number of elements in the TokenPatternList.
                for (int i = 0; i < this.tokenPatternList.Count + 1; i++)
                {
                    // The 0 index of the groups is always matched so it doesn't need to check.
                    if (item.Item3.Groups[i + 1].Length > 0)
                    {
                        patternInfo = this.tokenPatternList[i];
                        break;
                    }
                }

                lock (result) result.Add(new TokenCell(item.Item1, item.Item2, patternInfo));
            });

            result.Sort((t, td) =>
            {
                if (t.StartIndex < td.StartIndex) return -1;
                else if (t.StartIndex > td.StartIndex) return 1;
                return 0;
            });

            return result;
        }



        /*
        private List<int> MergeCore(List<TokenCell> tokens, int tokenIndex, int len)
        {
            List<int> result = new List<int>();

            if (tokenIndex + len >= tokens.Count - 1) return result;

            int endIndex = tokenIndex + len;
            string mergeString = string.Empty;
            for (int i = tokenIndex; i <= endIndex; i++)
            {
                var targetToken = tokens[i];
                mergeString += targetToken.Data;
            }

            int basisIndex = tokens[tokenIndex].StartIndex;
            List<TokenCell> temp = this.Tokenize(mergeString, basisIndex);

            if (temp.IsEqual(tokens.Skip(tokenIndex).Take(len).ToList())) return result;

            for (int i = 0; i <= len; i++) tokens.RemoveAt(tokenIndex);

            temp.ForEach(i => 
            {
                result.Add(tokenIndex);
                tokens.Insert(tokenIndex++, i);
            });

            return result;
        }

        /// <summary>
        /// This function merges token into direction MergeDirection and returns a number that is inserted a merged token.
        /// </summary>
        /// <param name="tokens">The target list</param>
        /// <param name="tokenIndex">The starting index to merge</param>
        /// <param name="len">The length to merge</param>
        /// <param name="mergeDirection">Direction to merge</param>
        /// <returns>The inserted indexes after merge</returns>
        public List<int> Merge(List<TokenCell> tokens, int tokenIndex, int len, MergeDirection mergeDirection)
        {
            List<int> result = new List<int>();
            if (tokenIndex >= tokens.Count) return result;

            if (mergeDirection == MergeDirection.Front)
            {
                if (tokenIndex == 0) return result;
                if (tokenIndex - len < 0) return result;

                int startIndex = tokenIndex - len;

                result = this.MergeCore(tokens, startIndex, len);
            }
            else result = this.MergeCore(tokens, tokenIndex, len);

            return result;
        }

        /// <summary>
        /// This function tokenize after merges the token of the token index-1 with the token of the token index until there's no change.
        /// </summary>
        /// <param name="tokens"> token list. </param>
        /// <param name="tokenIndex"></param>
        public void ContinousMergeAndTokenize(List<TokenCell> tokens, int tokenIndex, MergeDirection mergeDirection)
        {
            if(mergeDirection == MergeDirection.Front)
            {
                while (true)
                {
                    // If tokenIndex is the last of the token then break.
                    if (tokenIndex == tokens.Count - 1) break;

                    // Check next token
                    var nextToken = tokens[tokenIndex];
                    string mergeString = nextToken.MergeStringToEnd(tokens[tokenIndex + 1].Data);

                    int basisIndex = (tokenIndex == 0) ? 0 : tokens[tokenIndex].StartIndex;
                    List<TokenCell> result = this.Tokenize(mergeString, basisIndex);
                    if (result[0].Data == tokens[tokenIndex].Data) break;

                    tokens.RemoveAt(tokenIndex);
                    tokens.RemoveAt(tokenIndex);

                    result.ForEach(i => tokens.Insert(tokenIndex++, i));
                    tokenIndex--;
                }
            }

            if(mergeDirection == MergeDirection.End)
            {
                while (true)
                {
                    // If tokenIndex is the last of the token then break.
                    if (tokenIndex == tokens.Count - 1) break;

                    // Check next token
                    var nextToken = tokens[tokenIndex];
                    string mergeString = nextToken.MergeStringToEnd(tokens[tokenIndex + 1].Data);

                    int basisIndex = (tokenIndex == 0) ? 0 : tokens[tokenIndex].StartIndex;
                    List<TokenCell> result = this.Tokenize(mergeString, basisIndex);
                    if (result[0].Data == tokens[tokenIndex].Data) break;

                    tokens.RemoveAt(tokenIndex);
                    tokens.RemoveAt(tokenIndex);

                    result.ForEach(i => tokens.Insert(tokenIndex++, i));
                    tokenIndex--;
                }
            }
        }

        public List<int> MergeAndTokenize(List<TokenCell> tokens, int fromIndex, int len)
        {
            List<int> result = new List<int>();

            if (len == 0) return result;
            if (fromIndex< 0) return result;
            if (fromIndex + len >= tokens.Count) return result;

            return this.MergeCore(tokens, fromIndex, len);
        }
        */
    }
}
