using Parse.Extensions;
using Parse.FrontEnd.RegularGrammar;
using Parse.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Tokenize
{
    public class Lexer
    {
        private int _key = 1;
        private List<TokenPatternInfo> _tokenPatternList = new List<TokenPatternInfo>();
        private string _tokenizeRule = string.Empty;
        private Tokenizer _tokenizer = new Tokenizer();

        public IReadOnlyList<TokenPatternInfo> TokenPatternList => _tokenPatternList;
        public TokenizeImpactRanges ImpactRanges { get; } = new TokenizeImpactRanges();

        public Lexer()
        {
            this._tokenizer.TokenizeCompleted = TokenizeCompletedWork;
        }

        private void TokenizeCompletedWork(IReadOnlyList<TokenCell> tokenCells)
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
                else if (t.bWord != td.bWord)
                {
                    return (t.bWord) ? 1 : -1;
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
            var tokenList = this._tokenizer.Tokenize(this._tokenizeRule, mergeString);
            result.ReplaceToken(impactRange, tokenList);
            var rangePairToRegist = new RangePair(impactRange, new Range(impactRange.StartIndex, tokenList.Count));

            /// The comment can include all tokens therefore if a comment exists the process is performed as below.
            /// ex : void main()\r\n{}\r\n -> void main(/*)\r\n{}\r\n
            ///       process 1 : "void", " ", "main", "(", "/*)", "\r", "\n" -> The tokens in the impact range.
            ///                        "void main(/*)\r\n"                            -> The string that merged.
            ///                        "void", " ", "main", "(", "/*)\r\n"          -> The tokens after splite.
            ///       process 2 : "void", " ", "main", "(", "/*)\r\n", "{", "}", "\r", "\n" -> The tokens in the impact range.
            ///                        "void main(/*)\r\n{}\r\n"                                     -> The string that merged.
            ///                        "void", " ", "main", "(", "/*)\r\n{}\r\n"                   -> The tokens after splite.
            while (true)
            {
                var impactRangeToParse = result.FindImpactRange(rangePairToRegist.Item1.StartIndex, rangePairToRegist.Item1.EndIndex);
                var beforeTokens = result.TokensToView.Skip(impactRangeToParse.StartIndex).Take(impactRangeToParse.Count).ToList();
                var basisIndex = (beforeTokens.Count > 0) ? beforeTokens[0].StartIndex : 0;
                var processedTokens = this._tokenizer.Tokenize(this._tokenizeRule, result.GetMergeStringOfRange(impactRangeToParse), basisIndex);

                if (beforeTokens.IsEqual(processedTokens)) break;
                result.ReplaceToken(impactRangeToParse, processedTokens);
                rangePairToRegist = new RangePair(impactRangeToParse, new Range(impactRangeToParse.StartIndex, processedTokens.Count));
            }

            this.ImpactRanges.Add(rangePairToRegist);

            return result;
        }

        /// <summary>
        /// This function returns token list after lexing a string data.
        /// </summary>
        /// <param name="data">The string data to lex</param>
        /// <returns></returns>
        public TokenStorage Lexing(string data)
        {
            TokenStorage result = new TokenStorage(this._tokenPatternList);

            this._tokenizer.Tokenize(this._tokenizeRule, data).ForEach(i => result._tokensToView.Add(i));
            result.UpdateTableForAllPatterns();

            if(result._tokensToView.Count > 0)  this.ImpactRanges.Add(new RangePair(new Range(-1, 0), new Range(0, result._tokensToView.Count)));

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

            int offset = prevTokens.TokensToView.Count - 1;
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
            if (prevTokens.TokensToView.Count == 0) return this.Lexing(dataToAdd);

            TokenStorage result = prevTokens.Clone() as TokenStorage;

            RecognitionWay recognitionWay = RecognitionWay.Back;
            int curTokenIndex = result.TokenIndexForOffset(offset, recognitionWay);

            if(curTokenIndex == -1)
            {
                TokenCell token = result.TokensToView[0];
                result = this.TokenizeAfterReplace(result, 0, token.MergeString(offset, dataToAdd, RecognitionWay.Front));
            }
            else
            {
                TokenCell token = result.TokensToView[curTokenIndex];
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

            var indexInfo = delInfos.Range;
            var impactRange = result.FindImpactRange(indexInfo.StartIndex, indexInfo.EndIndex);
            string mergeString = this.GetImpactedStringFromDelInfo(result, delInfos);

            var toInsertTokens = this._tokenizer.Tokenize(this._tokenizeRule, mergeString);
            result.ReplaceToken(impactRange, toInsertTokens);
            this.ImpactRanges.Add(new RangePair(impactRange, new Range(impactRange.StartIndex, toInsertTokens.Count)));
            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            return result;

            // The Rectangle Deletion operation need to write other algorithm also the algorithm will very complicate so I don't write it yet.
            // (The above data struct can be used on the Rectangle Deletion operation.)
        }

        public TokenStorage Lexing(TokenStorage targetStorage, SelectionTokensContainer delInfos, string replaceString)
        {
            if (delInfos.IsEmpty()) return targetStorage;
            if (replaceString.Length == 0) return targetStorage;

            TokenStorage result = targetStorage.Clone() as TokenStorage;

            var indexInfo = delInfos.Range;
            var impactRange = result.FindImpactRange(indexInfo.StartIndex, indexInfo.EndIndex);
            string mergeString = this.GetImpactedStringFromDelInfo(result, delInfos, replaceString);

            var toInsertTokens = this._tokenizer.Tokenize(this._tokenizeRule, mergeString);
            result.ReplaceToken(impactRange, toInsertTokens);
            this.ImpactRanges.Add(new RangePair(impactRange, new Range(impactRange.StartIndex, toInsertTokens.Count)));

            return result;
        }

    }
}
