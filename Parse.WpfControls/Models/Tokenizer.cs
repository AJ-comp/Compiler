using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parse.WpfControls.Models
{
    public class Tokenizer
    {
        List<TokenPatternInfo> tokenPatternList = new List<TokenPatternInfo>();
        string rule = string.Empty;


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
        /// <param name="addString">The string for tokenizing.</param>
        /// <param name="basisIndex">The basis index that uses to set up a starting index of a token.</param>
        public List<TokenCell> Tokenize(string addString, int basisIndex)
        {
            List<TokenCell> result = new List<TokenCell>();
            var coreResult = this.TokenizeCore(addString, basisIndex);

            // It would attach the label (TokenPatternInfo).
            Parallel.For(0, coreResult.Count, index =>
            {
                var item = coreResult[index];
                TokenPatternInfo patternInfo = TokenPatternInfo.NotDefinedToken;

                if(item.Item3 == null)
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
    }
}
