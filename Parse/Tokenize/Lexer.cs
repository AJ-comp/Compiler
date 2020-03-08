using Parse.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parse.Tokenize
{
    public class Lexer
    {
        private int key = 1;
        private List<TokenPatternInfo> tokenPatternList = new List<TokenPatternInfo>();
        private string tokenizeRule = string.Empty;
        private Tokenizer tokenizeTeam = new Tokenizer();

        public IReadOnlyList<TokenPatternInfo> TokenPatternList => tokenPatternList;
        public TokenizeImpactRanges ImpactRanges { get; } = new TokenizeImpactRanges();

        public Lexer()
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
        }

        /// <summary>
        /// This function process the following algorithm.
        /// 1. Generate tokens by tokenizing the mergeString argument.
        /// 2. Replace token of the replaceIndex argument to the generated tokens.
        /// 3. Work the RangeMergeAndTokenizeProcess.
        /// </summary>
        /// <param name="targetStorage"></param>
        /// <param name="replaceIndex"></param>
        /// <param name="mergeString"></param>
        /// <returns></returns>
        private TokenStorage TokenizeAfterReplace(TokenStorage targetStorage, int replaceIndex, string mergeString)
        {
            var impactRange = targetStorage.FindImpactRange(replaceIndex);

            TokenStorage result = targetStorage.Clone() as TokenStorage;
            var firstString = targetStorage.GetMergeStringOfRange(new Range(impactRange.StartIndex, replaceIndex - impactRange.StartIndex));
            var lastString = targetStorage.GetMergeStringOfRange(new Range(replaceIndex + 1, impactRange.EndIndex - replaceIndex));
            mergeString = firstString + mergeString + lastString;

            // If a basisIndex is used then it can increase the performance of the ReplaceToken function because of need not arrange.
            // But the logic is not written yet.
            var tokenList = this.tokenizeTeam.Tokenize(this.tokenizeRule, mergeString);
            result.ReplaceToken(impactRange, tokenList);
            this.ImpactRanges.PrevRanges.Add(impactRange);
            this.ImpactRanges.CurRanges.Add(new Range(impactRange.StartIndex, tokenList.Count));

            /// ex : void main()\r\n{}\r\n -> void main(//)\r\n{}\r\n
            ///       process 1 : "void", " ", "main", "(", "//)", "\r", "\n"
            while (true)
            {
                impactRange = result.FindImpactRange(impactRange.StartIndex - 1, impactRange.EndIndex + 1);
                var beforeTokens = result.AllTokens.Skip(impactRange.StartIndex).Take(impactRange.Count).ToList();
                var processedTokens = this.tokenizeTeam.Tokenize(this.tokenizeRule, result.GetMergeStringOfRange(new Range(impactRange.StartIndex, impactRange.Count)));
                result.ReplaceToken(impactRange, processedTokens);

                if (beforeTokens.IsEqual(processedTokens)) break;
                this.ImpactRanges.PrevRanges.Add(impactRange);
                this.ImpactRanges.CurRanges.Add(new Range(impactRange.StartIndex, processedTokens.Count));
            }

            return result;
        }

        /// <summary>
        /// This function returns token list after lexing a string data.
        /// </summary>
        /// <param name="data">The string data to lex</param>
        /// <returns></returns>
        public TokenStorage Lexing(string data)
        {
            TokenStorage result = new TokenStorage(this.tokenPatternList);

            this.tokenizeTeam.Tokenize(this.tokenizeRule, data).ForEach(i => result.allTokens.Add(i));
            result.UpdateTableForAllPatterns();

            if(result.allTokens.Count > 0)  this.ImpactRanges.CurRanges.Add(new Range(0, result.allTokens.Count));

            return result;
        }

        /// <summary>
        /// This function returns token list after add dataToAdd at the end of prevTokens.
        /// </summary>
        /// <param name="prevTokens">The target tokens</param>
        /// <param name="dataToAdd">The data to add</param>
        /// <see cref="https://www.lucidchart.com/documents/edit/f4366425-61f9-4b4f-9abc-72ce4efe864c/ZYtDEhwbkIBA?beaconFlowId=53B1C199D7307981"/>
        /// <returns></returns>
        public TokenStorage Lexing(TokenStorage prevTokens, string dataToAdd)
        {
            if (prevTokens == null) return this.Lexing(dataToAdd);

            int offset = prevTokens.AllTokens.Count - 1;
            return this.Lexing(prevTokens, offset, dataToAdd);
        }

        /// <summary>
        /// This function returns token list after add dataToAdd at the offset of prevTokens.
        /// </summary>
        /// <param name="prevTokens">The target tokens</param>
        /// <param name="offset">The position to add</param>
        /// <param name="dataToAdd">The data to add</param>
        /// <see cref="https://www.lucidchart.com/documents/edit/f4366425-61f9-4b4f-9abc-72ce4efe864c/ZYtDEhwbkIBA?beaconFlowId=53B1C199D7307981"/>
        /// <returns></returns>
        public TokenStorage Lexing(TokenStorage prevTokens, int offset, string dataToAdd)
        {
            if (prevTokens == null) return this.Lexing(dataToAdd);
            if (prevTokens.AllTokens.Count == 0) return this.Lexing(dataToAdd);

            TokenStorage result = prevTokens.Clone() as TokenStorage;

            RecognitionWay recognitionWay = RecognitionWay.Back;
            int curTokenIndex = result.TokenIndexForOffset(offset, recognitionWay);

            if(curTokenIndex == -1)
            {
                TokenCell token = result.AllTokens[0];
                result = this.TokenizeAfterReplace(result, 0, token.MergeString(offset, dataToAdd, recognitionWay));
            }
            else
            {
                TokenCell token = result.AllTokens[curTokenIndex];
                result = this.TokenizeAfterReplace(result, curTokenIndex, token.MergeString(offset, dataToAdd, recognitionWay));
            }


            return result;
        }

        /// <summary>
        /// This function processes the deletion order.
        /// </summary>
        /// <param name="targetStorage"></param>
        /// <param name="delInfos"></param>
        /// /// <returns></returns>
        public TokenStorage Lexing(TokenStorage targetStorage, SelectionTokensContainer delInfos)
        {
            if (delInfos.IsEmpty()) return targetStorage;

            TokenStorage result = targetStorage.Clone() as TokenStorage;

            var indexInfo = delInfos.Range;

            ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
            // The following process needs because of the following.
            // Ex 1 : void main()A
            //       1. () tokens deletion.
            //       2. It have to merge "main" token with "A" token.
            //       3. The merge result "mainA" is another token.
            //       4. The deletion result is "void", " ", "mainA".
            // Ex 2 : void main(){}
            //       1. () tokens deletion.
            //       2. It have to merge "main" token with "{}" token.            ( "}" token also be included because impactRange algorithm, see the impactRange algorithm)
            //       3. The merge result "main{}" is not token therefore it will be separate to the multiple tokens.
            //       4. The separate result is "main", "{", "}".
            //       5. The separated tokens replace the "main{}" token.
            //       6. The deletion result is "void", " ", "main", "{", "}".
            var impactRange = result.FindImpactRange(indexInfo.StartIndex, indexInfo.EndIndex);
            string mergeString = result.GetMergeStringOfRange(new Range(impactRange.StartIndex, indexInfo.StartIndex - impactRange.StartIndex));
            mergeString += result.GetPartString(indexInfo.StartIndex, delInfos);

            if (indexInfo.StartIndex != indexInfo.EndIndex) mergeString += result.GetPartString(indexInfo.EndIndex, delInfos);
            int endIndex = indexInfo.EndIndex + 1;
            mergeString += result.GetMergeStringOfRange(new Range(endIndex, impactRange.EndIndex - indexInfo.EndIndex));

            var toInsertTokens = this.tokenizeTeam.Tokenize(this.tokenizeRule, mergeString);
            result.ReplaceToken(impactRange, toInsertTokens);
            this.ImpactRanges.PrevRanges.Add(impactRange);
            this.ImpactRanges.CurRanges.Add(new Range(impactRange.StartIndex, toInsertTokens.Count));
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            return result;

            // The Rectangle Deletion operation need to write other algorithm also the algorithm will very complicate so I don't write it yet.
            // (The above data struct can be used on the Rectangle Deletion operation.)
        }

    }
}
