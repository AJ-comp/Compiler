using Parse.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parse.Tokenize
{
    public class TokenizeFactory
    {
        private int key = 1;
        private List<TokenPatternInfo> tokenPatternList = new List<TokenPatternInfo>();

        private Tokenizer tokenizeTeam = new Tokenizer();
        public TokenStorage StorageTeam { get; } = new TokenStorage();


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

        public void AddTokenRule(string text, object optionData = null, bool bCanDerived = false, bool bOperator = false)
        {
            foreach (var item in this.tokenPatternList)
            {
                if (item.OriginalPattern == text) return;
            }

            this.tokenPatternList.Add(new TokenPatternInfo(this.key++, text, optionData, bCanDerived, bOperator));
            this.SortTokenPatternList();

            this.tokenizeTeam.RegistTokenizeRule(this.tokenPatternList);
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

            var tokenList = this.tokenizeTeam.Tokenize(mergeString);
            this.StorageTeam.ReplaceToken(fromIndex, count, tokenList);
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
            // If to use the basisIndex then it can increase the performance of the ReplaceToken function because of need not arrange.
            // But the logic is not written yet.
            var tokenList = this.tokenizeTeam.Tokenize(mergeString);
            this.StorageTeam.ReplaceToken(replaceIndex, 1, tokenList);

            var indexInfo = this.StorageTeam.FindImpactRange(replaceIndex + tokenList.Count - 1);

            this.RangeMergeAndTokenizeProcess(indexInfo.Item1, indexInfo.Item2);
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
                this.tokenizeTeam.Tokenize(rawString, nextTokenIndex).ForEach(i => this.StorageTeam.AllTokens.Add(i));

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

        public void ReceiveOrder(SelectionTokensContainer delInfos)
        {
            if (delInfos.WholeSelectionBag.Count != 0)
            {
                this.StorageTeam.UpdateTableForAllPatterns();

                int delIndex = delInfos.WholeSelectionBag.First();

                // The calculation that influences the length of the array never use a parallel calculation without synchronization. (ex : insert, delete)
                Parallel.For(0, delInfos.WholeSelectionBag.Count, i =>
                {
                    // for synchronization
                    lock (this.StorageTeam.AllTokens) this.StorageTeam.AllTokens.RemoveAt(delIndex);
                });

                Parallel.For(0, delInfos.PartSelectionBag.Count, i =>
                {
                    var delInfo = delInfos.PartSelectionBag[i];

//                    this.StorageTeam.AllTokens[i]. this.StorageTeam.AllTokens[i].Data.Remove(delInfo.Item2, delInfo.Item3);
                });




                // The Rectangle Deletion operation need to write other algorithm also the algorithm will very complicate so I don't write it yet.
                // (Can use the above data struct on the Rectangle Deletion operation.)
            }
        }

    }
}
