using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parse.Tokenize
{
    public class TokenizeFactory
    {
        private int key = 1;
        private List<TokenPatternInfo> tokenPatternList = new List<TokenPatternInfo>();
        private string tokenizeRule = string.Empty;

        private Tokenizer tokenizeTeam = new Tokenizer();
        public TokenStorage StorageTeam { get; } = new TokenStorage();

        public TokenizeFactory()
        {
            this.tokenizeTeam.TokenizeCompleted = TokenizeCompletedWork;
        }

        private void TokenizeCompletedWork(List<TokenCell> tokenCells)
        {
            // It would attach the label (TokenPatternInfo).
            Parallel.For(0, tokenCells.Count, index =>
            {
                var patternInfo = this.GetMatchedPattern(tokenCells[index].matchData);

                tokenCells[index].PatternInfo = patternInfo;
            });
        }

        /// <summary>
        /// This function returns the tokenize rule.
        /// </summary>
        /// <param name="tokenPatternList"></param>
        public string GetTokenizeRule(List<TokenPatternInfo> tokenPatternList)
        {
            this.tokenPatternList = tokenPatternList;

            string result = string.Empty;
            string generateString = string.Empty;
            foreach (var pattern in tokenPatternList)
            {
                if (pattern.OriginalPattern == string.Empty) continue;

                generateString += "a";
                result += string.Format("(?<{1}>{0})|", pattern.Pattern, generateString);
                //                    patternSum += string.Format("({0})|", pattern.Pattern);
            }

            return result.Substring(0, result.Length - 1);
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
            for (int i = 0; i < this.tokenPatternList.Count + 1; i++)
            {
                // The 0 index of the groups is always matched so it doesn't need to check.
                if (matchInfo.Groups[i + 1].Length > 0)
                {
                    result = this.tokenPatternList[i];
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
            this.tokenPatternList.Sort(delegate (TokenPatternInfo t, TokenPatternInfo td)
            {
                if (t.Operator != td.Operator)
                {
                    return (t.Operator) ? 1 : -1;
                }
                else if (t.CanDerived != td.CanDerived)
                {
                    return (t.CanDerived) ? 1 : -1;
                }

                if (t.OriginalPattern.Length > td.OriginalPattern.Length) return -1;
                else if (t.OriginalPattern.Length <= td.OriginalPattern.Length) return 1;
                return 0;
            });
        }

        /// <summary>
        /// This function returns a part string that applied the PartSelectionBag in the SelectionTokensContainer.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <param name="rangeInfo"></param>
        /// <returns>The merged string.</returns>
        private string GetPartString(int tokenIndex, SelectionTokensContainer rangeInfo)
        {
            string result = string.Empty;

            int index = rangeInfo.GetIndexInPartSelectionBag(tokenIndex);
            if (index >= 0)
            {
                var startIndexToRemove = rangeInfo.PartSelectionBag[index].Item2;
                var removeLength = rangeInfo.PartSelectionBag[index].Item3;

                result = this.StorageTeam.AllTokens[tokenIndex].Data.Remove(startIndexToRemove, removeLength);
            }

            return result;
        }


        public TokenPatternInfo GetPatternInfo(string pattern)
        {
            TokenPatternInfo result = null;

            Parallel.For(0, this.tokenPatternList.Count, (i, loopOption) =>
            {
                var patternInfo = this.tokenPatternList[i];

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
        /// <param name="text">The pattern to tokenize.</param>
        /// <param name="optionData">If exist additional data then would add to this.</param>
        /// <param name="bCanDerived">This argument means the first argument is the derived pattern or is a fixed pattern.</param>
        /// <param name="bOperator">true = delimiter, false = not delimiter.</param>
        public void AddTokenRule(string text, object optionData = null, bool bCanDerived = false, bool bOperator = false)
        {
            foreach (var item in this.tokenPatternList)
            {
                if (item.OriginalPattern == text) return;
            }

            this.tokenPatternList.Add(new TokenPatternInfo(this.key++, text, optionData, bCanDerived, bOperator));
            this.SortTokenPatternList();

            this.tokenizeRule = this.GetTokenizeRule(this.tokenPatternList);
            this.StorageTeam.InitTokenTable(this.tokenPatternList);
        }

        /// <summary>
        /// This function works the merge and the tokenize. (from fromIndex to the toIndex[fromIndex + count - 1])
        /// </summary>
        /// <param name="fromIndex"></param>
        /// <param name="count"></param>
        private void RangeMergeAndTokenizeProcess(int fromIndex, int count)
        {
            if (fromIndex + count > this.StorageTeam.AllTokens.Count) return;

            string mergeString = string.Empty;
            foreach(var item in this.StorageTeam.AllTokens.Skip(fromIndex).Take(count))
                mergeString += item.Data;

            var tokenList = this.tokenizeTeam.Tokenize(this.tokenizeRule, mergeString);
            this.StorageTeam.ReplaceToken(new Range(fromIndex, count), tokenList);
        }

        /// <summary>
        /// This function process the following algorithm.
        /// 1. Generate tokens by tokenizing the mergeString argument.
        /// 2. Replace token of the replaceIndex argument to the generated tokens.
        /// 3. Work the RangeMergeAndTokenizeProcess.
        /// </summary>
        /// <param name="replaceIndex"></param>
        /// <param name="mergeString"></param>
        private void TokenizeAfterReplace(int replaceIndex, string mergeString)
        {
            TokenCell token = this.StorageTeam.AllTokens[replaceIndex];

            var impactRange = this.StorageTeam.FindImpactRange(replaceIndex);

            var firstString = this.StorageTeam.GetMergeStringOfRange(new Range(impactRange.StartIndex, replaceIndex - impactRange.StartIndex));
            var lastString = this.StorageTeam.GetMergeStringOfRange(new Range(replaceIndex + 1, impactRange.EndIndex - replaceIndex));
            mergeString = firstString + mergeString + lastString;

            // If to use the basisIndex then it can increase the performance of the ReplaceToken function because of need not arrange.
            // But the logic is not written yet.
            var tokenList = this.tokenizeTeam.Tokenize(this.tokenizeRule, mergeString);
            this.StorageTeam.ReplaceToken(impactRange, tokenList);

            this.RangeMergeAndTokenizeProcess(impactRange.StartIndex, impactRange.Count);
        }

        /// <summary>
        /// This function adds a token from the offset. The token is generated after tokenize the rawString.
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="rawString"></param>
        public void ReceiveOrder(int offset, string rawString)
        {
            RecognitionWay recognitionWay = RecognitionWay.Back;
            this.StorageTeam.UpdateTableForAllPatterns();

            int curTokenIndex = this.StorageTeam.TokenIndexForOffset(offset, recognitionWay);
            int nextTokenIndex = curTokenIndex + 1;
            if (this.StorageTeam.AllTokens.Count == 0)
            {
                this.tokenizeTeam.Tokenize(this.tokenizeRule, rawString, nextTokenIndex).ForEach(i => this.StorageTeam.AllTokens.Add(i));

                #region The method of the second tokenize. (delete duplicate element after all match)
                /*
                foreach(var item in data)
                {
                    if (item.Length == 0) continue;
                    tokens.Add(item);
                }*/

                /*
                 *                 List<Match> tokens = new List<Match>();
                foreach(var pattern in this.TokenPatternList)
                {
                    foreach(var match in Regex.Matches(addString, pattern))
                    {
                        var matchData = match as Match;
                        bool bExist = tokens.Any((i) => (i.Index < matchData.Index && matchData.Index <= i.Length - 1 + i.Index));
                        if (bExist) continue;
                        tokens.Add(matchData);
                    }
                }

                tokens.Sort(delegate (Match a, Match b)
                {
                    if (a.Index > b.Index) return 1;
                    else if (a.Index < b.Index) return -1;
                    return 0;
                });
                */
                #endregion

                #region The method of the Third tokenize. (sequentially match from the first element)
                /*
                while(addString.Length > 0)
                {
                    string topCandidate = string.Empty;

                    foreach (var pattern in this.TokenPatternList)
                    {
                        var matchResult = Regex.Match(addString, pattern.Pattern);
                        if (matchResult.Index != 0) continue;

                        if (matchResult.Length > topCandidate.Length) topCandidate = matchResult.Value;
                    }

                    if (topCandidate == string.Empty)
                    {
                        this.Tokens.Add(new TokenInfo(0, addString[0].ToString());
                        addString = addString.Substring(1);
                    }
                    else
                    {
                        this.Tokens.Add(topCandidate);
                        addString = addString.Substring(topCandidate.Length);
                    }
                }
                */
                #endregion

            }
            else
            {
                if (curTokenIndex == -1)
                {
                    TokenCell nextToken = this.StorageTeam.AllTokens[nextTokenIndex];
                    this.TokenizeAfterReplace(nextTokenIndex, nextToken.MergeStringToFront(rawString));
                }
                else
                {
                    TokenCell token = this.StorageTeam.AllTokens[curTokenIndex];
                    this.TokenizeAfterReplace(curTokenIndex, token.MergeString(offset, rawString, recognitionWay));
                }
            }
        }

        /// <summary>
        /// This function processes the deletion order.
        /// </summary>
        /// <param name="delInfos"></param>
        public void ReceiveOrder(SelectionTokensContainer delInfos)
        {
            if (delInfos.IsEmpty()) return;

            var indexInfo = delInfos.Range;
            var impactRange = this.StorageTeam.FindImpactRange(indexInfo.StartIndex, indexInfo.EndIndex);

            string mergeString = this.StorageTeam.GetMergeStringOfRange(new Range(impactRange.StartIndex, indexInfo.StartIndex - impactRange.StartIndex));
            mergeString += this.GetPartString(indexInfo.StartIndex, delInfos);

            if (indexInfo.StartIndex != indexInfo.EndIndex) mergeString += this.GetPartString(indexInfo.EndIndex, delInfos);
            int endIndex = indexInfo.EndIndex + 1;
            mergeString += this.StorageTeam.GetMergeStringOfRange(new Range(endIndex, impactRange.EndIndex - indexInfo.EndIndex));

            var toInsertTokens = this.tokenizeTeam.Tokenize(this.tokenizeRule, mergeString);
            this.StorageTeam.ReplaceToken(impactRange, toInsertTokens);

            this.StorageTeam.UpdateTableForAllPatterns();

            // The Rectangle Deletion operation need to write other algorithm also the algorithm will very complicate so I don't write it yet.
            // (Can use the above data struct on the Rectangle Deletion operation.)
        }

    }
}
