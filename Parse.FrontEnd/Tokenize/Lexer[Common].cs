using Parse.FrontEnd.RegularGrammar;
using Parse.Utilities;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Tokenize
{
    public partial class Lexer
    {
        public IReadOnlyList<TokenPatternInfo> TokenPatternList => _tokenPatternList;

        public Lexer()
        {
            this._tokenizer.TokenizeCompleted = TokenizeCompletedWork;
        }

        private void TokenizeCompletedWork(IReadOnlyList<TokenCell> tokenCells)
        {
            // It would attach the label (TokenPatternInfo).
            Parallel.For(0, tokenCells.Count, index =>
            {
                var patternInfo = this.GetMatchedPattern(tokenCells[index].MatchData);

                tokenCells[index].PatternInfo = patternInfo;
            });
        }

        /// <summary>
        /// This function returns the tokenize rule.
        /// </summary>
        /// <param name="tokenPatternList"></param>
        public string GetTokenizeRule(List<TokenPatternInfo> tokenPatternList)
        {
            this._tokenPatternList = tokenPatternList;

            string result = string.Empty;
            HashSet<string> allHashCode = new HashSet<string>();
            foreach (var pattern in tokenPatternList)
            {
                if (pattern.OriginalPattern == string.Empty) continue;

                string key = string.Empty;
                do
                {
                    key = StringUtility.RandomString(3, false);
                } while (allHashCode.Contains(key));

                result += string.Format("(?<{0}>{1})|", key, pattern.Pattern);
                //                    patternSum += string.Format("({0})|", pattern.Pattern);
                allHashCode.Add(key);
            }

            return result[0..^1];
        }

        /// <summary>
        /// This function returns the TokenPatternInfo for Match argument.
        /// </summary>
        /// <param name="matchInfo"></param>
        /// <returns></returns>
        private TokenPatternInfo GetMatchedPattern(Match matchInfo)
        {
            TokenPatternInfo result = TokenPatternInfo.NotDefinedToken;

            if (matchInfo == null) return result;

            // The number of elements in the group is 1 more than the number of elements in the TokenPatternList.
            for (int i = 0; i < this._tokenPatternList.Count + 1; i++)
            {
                // The 0 index of the groups is always matched so it doesn't need to check.
                if (matchInfo.Groups[i + 1].Length > 0)
                {
                    result = this._tokenPatternList[i];
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This function sorts the TokenPatterList accoding to the following principle.
        /// 1st : Delimitable == false, CanDerived == false 
        /// 2nd : Delimitable == false, CanDerived == true
        /// 3rd : Delimitable == true, CanDerived == false
        /// </summary>
        private void SortTokenPatternList()
        {
            this._tokenPatternList.Sort((TokenPatternInfo t, TokenPatternInfo td) =>
            {
                if (t.Operator != td.Operator)
                {
                    return (t.Operator) ? 1 : -1;
                }
                else if (t.IsWord != td.IsWord)
                {
                    return (t.IsWord) ? 1 : -1;
                }

                if (t.OriginalPattern.Length > td.OriginalPattern.Length) return -1;
                else if (t.OriginalPattern.Length <= td.OriginalPattern.Length) return 1;
                return 0;
            });
        }

        private string GetImpactedStringFromDelInfo(TokenStorage result, SelectionTokensContainer delInfos, string replaceString = "")
        {
            var indexInfo = delInfos.Range;

            // A string that exists on the front on the basis of the deletion token.
            var impactRange = result.FindImpactRange(indexInfo.StartIndex, indexInfo.EndIndex);
            string mergeString = result.GetMergeStringOfRange(new Range(impactRange.StartIndex, indexInfo.StartIndex - impactRange.StartIndex));

            // A string that remained after deleted.
            mergeString += result.GetPartString(indexInfo.StartIndex, delInfos);
            // A string replaced.
            mergeString += replaceString;

            // A string that exists on the back on the basis of the deletion token.
            if (indexInfo.StartIndex != indexInfo.EndIndex) mergeString += result.GetPartString(indexInfo.EndIndex, delInfos);
            int endIndex = indexInfo.EndIndex + 1;
            mergeString += result.GetMergeStringOfRange(new Range(endIndex, impactRange.EndIndex - indexInfo.EndIndex));

            return mergeString;
        }


        public TokenPatternInfo GetPatternInfo(string pattern)
        {
            TokenPatternInfo result = null;

            Parallel.For(0, this._tokenPatternList.Count, (i, loopOption) =>
            {
                var patternInfo = this._tokenPatternList[i];

                if (patternInfo.OriginalPattern == pattern)
                {
                    result = patternInfo;
                    loopOption.Stop();
                }
            });

            return result;
        }

        /// <summary>
        /// This function adds a tokenize rule to the sub-modules of the TokenizeFactory.
        /// </summary>
        /// <param name="terminal"></param>
        public void AddTokenRule(Terminal terminal)
        {
            foreach (var item in this._tokenPatternList)
            {
                if (item.Terminal.TokenType == terminal.TokenType &&
                    item.Terminal.Value == terminal.Value) return;
            }

            this._tokenPatternList.Add(new TokenPatternInfo(this._key++, terminal));
            this.SortTokenPatternList();

            this._tokenizeRule = this.GetTokenizeRule(this._tokenPatternList);
        }


        private int _key = 1;
        private List<TokenPatternInfo> _tokenPatternList = new List<TokenPatternInfo>();
        private string _tokenizeRule = string.Empty;
        private Tokenizer _tokenizer = new Tokenizer();
    }
}
