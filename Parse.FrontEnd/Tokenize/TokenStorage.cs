﻿using Parse.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Tokenize
{
    public class TokenStorage : ICloneable
    {
        /// <summary> The tokens to view (This include all tokens) </summary>
        public IReadOnlyList<TokenCell> TokensToView => _tokensToView;

        /// <summary> This property returns a whole table has columns that have positions for each TokenPatternInfo. </summary>
        public Dictionary<TokenPatternInfo, List<int>> TableForAllPatternsOnParsing => this.TableForAllPatternsOnParsing;
        public Dictionary<TokenPatternInfo, List<int>> TableForAllPatternsOnView => this.tableForAllPatternsOnView;


        internal void UpdateTableForAllPatterns()
        {
            foreach (KeyValuePair<TokenPatternInfo, List<int>> item in this.tableForAllPatternsOnView) item.Value.Clear();

            Parallel.For(0, this._tokensToView.Count, i =>
            {
                var token = this._tokensToView[i];

                lock (this.tableForAllPatternsOnView[token.PatternInfo])
                    this.tableForAllPatternsOnView[token.PatternInfo].Add(i);
            });
        }

        /// <summary>
        /// This function initializes the token table into the argument value.
        /// </summary>
        /// <param name="tokenPatternList">The value of the token pattern list to initialize.</param>
        internal void InitTokenTable(IReadOnlyList<TokenPatternInfo> tokenPatternList)
        {
            this.tableForAllPatternsOnView.Clear();

            foreach(var item in tokenPatternList)
            {
                this.tableForAllPatternsOnView.Add(item, new List<int>());
            }

            this.tableForAllPatternsOnView.Add(TokenPatternInfo.NotDefinedToken, new List<int>());
        }

        private TokenStorage() { }

        public TokenStorage(IReadOnlyList<TokenPatternInfo> tokenPatternList)
        {
            this.InitTokenTable(tokenPatternList);
        }

        /// <summary>
        /// This function returns indexes for the special pattern from the TokenPatternInfo.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public List<int> GetIndexesForSpecialPattern(TokenPatternInfo pattern)
        {
            var table = this.TableForAllPatternsOnView;
            List<int> result = new List<int>();

            if (table.ContainsKey(pattern))
                result = table[pattern];

            return result;
        }


        public KeyValuePair<TokenPatternInfo, List<int>> GetTokenPatternInfoFromString(string pattern)
        {
            var table = this.TableForAllPatternsOnView;
            KeyValuePair<TokenPatternInfo, List<int>> result = new KeyValuePair<TokenPatternInfo, List<int>>();

            foreach (KeyValuePair<TokenPatternInfo, List<int>> items in table)
            {
                if (items.Key.OriginalPattern == pattern)
                {
                    result = items;
                    break;
                }
            }

            return result;
        }


        /// <summary>
        /// This function returns indexes for the special pattern from the string.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public List<int> GetIndexesForSpecialPattern(string pattern)
        {
            return this.GetTokenPatternInfoFromString(pattern).Value;
        }

        public List<int> GetIndexesForSpecialPattern(params string[] patterns)
        {
            var table = this.TableForAllPatternsOnView;
            List<int> result = new List<int>();

            foreach (KeyValuePair<TokenPatternInfo, List<int>> items in table)
            {
                for (int i = 0; i < patterns.Length; i++)
                {
                    if (items.Key.OriginalPattern == patterns[i])
                        result.AddRange(items.Value);
                }
            }

            return result;
        }

        public int GetTopFrontIndexFromTokenIndex(string pattern, int tokenIndex)
        {
            var indexes = this.GetIndexesForSpecialPattern(pattern);

            int result = -1;
            int minDiffer = int.MaxValue;
            indexes.ForEach(i =>
            {
                int differ = tokenIndex - i;
                if (differ < minDiffer)
                {
                    result = i;
                    minDiffer = differ;
                }

                if (i > tokenIndex) return;
            });

            return result;
        }

        public int GetTopFrontIndexFromTokenIndex(TokenPatternInfo findPattern, int tokenIndex)
        {
            var indexes = this.GetIndexesForSpecialPattern(findPattern);

            int result = -1;
            int minDiffer = int.MaxValue;
            indexes.ForEach(i => 
            {
                int differ = tokenIndex - i;
                if (differ < minDiffer)
                {
                    result = i;
                    minDiffer = differ;
                }

                if (i > tokenIndex) return;
            });

            return result;
        }


        public int GetTopBackIndexFromTokenIndex(TokenPatternInfo findPattern, int tokenIndex)
        {
            var indexes = this.GetIndexesForSpecialPattern(findPattern);

            int result = -1;
            int minDiffer = int.MaxValue;
            indexes.ForEach(i =>
            {
                if (i > tokenIndex)
                {
                    int differ = i - tokenIndex;
                    if (differ < minDiffer)
                    {
                        result = i;
                        minDiffer = differ;
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// This function returns a part string that applied the PartSelectionBag in the SelectionTokensContainer.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <param name="rangeInfo"></param>
        /// <returns>The merged string.</returns>
        public string GetPartString(int tokenIndex, SelectionTokensContainer rangeInfo)
        {
            string result = string.Empty;

            int index = rangeInfo.GetIndexInPartSelectionBag(tokenIndex);
            if (index >= 0)
            {
                var startIndexToRemove = rangeInfo.PartSelectionBag[index].Item2;
                var removeLength = rangeInfo.PartSelectionBag[index].Item3;

                result = this.TokensToView[tokenIndex].Data.Remove(startIndexToRemove, removeLength);
            }

            return result;
        }


        /// <summary>
        /// This function returns a token index from the caretIndex.
        /// </summary>
        /// <param name="offset">The offset index</param>
        /// <param name="recognWay">The standard index for recognition a token</param>
        /// <returns>a token index</returns>
        public int TokenIndexForOffset(int offset, RecognitionWay recognWay = RecognitionWay.Back)
        {
            int index = -1;

            Parallel.For(0, this._tokensToView.Count, (i, loopState) =>
            {
                TokenCell tokenInfo = this._tokensToView[i];

                if (tokenInfo.Contains(offset, recognWay))
                {
                    index = i;
                    loopState.Stop();
                }
            });

            return index;
        }


        /// <summary>
        /// This function returns the impact range on the basis of the tokenIndex argument.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns>The first value : index, The second value : count </returns>
        public Range FindImpactRange(int tokenIndex) => this.FindImpactRange(tokenIndex, tokenIndex);

        /// <summary>
        /// This function returns the impact range on the basis of the arguments.
        /// </summary>
        /// <param name="startTokenIndex"></param>
        /// <param name="endTokenIndex"></param>
        /// <returns>The first value : index, The second value : count </returns>
        public Range FindImpactRange(int startTokenIndex, int endTokenIndex)
        {
            var indexes = this.GetIndexesForSpecialPattern(" ", "\t", "\r", "\n");
            var fromIndex = -1;
            var toIndex = -1;

            Parallel.Invoke(
                () =>
                {
                    fromIndex = indexes.GetIndexNearestLessThanValue(startTokenIndex);
                }, 
                () => 
                {
                    toIndex = indexes.GetIndexNearestMoreThanValue(endTokenIndex);
                });

            // Except PerfactDelimiter
            //            fromIndex = (fromIndex == 0 || fromIndex == -1) ? 0 : fromIndex - 1;
            //            toIndex = (toIndex == -1 || toIndex >= this.tokensToView.Count - 1) ? this.tokensToView.Count - 1 : toIndex + 1;

            fromIndex = (fromIndex == -1) ? 0 : fromIndex;
            toIndex = (toIndex == -1) ? this._tokensToView.Count - 1 : toIndex;

            return new Range(fromIndex, toIndex - fromIndex + 1);
        }

        /// <summary>
        /// This function returns the merged string of all tokens in the range.
        /// </summary>
        /// <param name="range">The range to get the merged string.</param>
        /// <returns>The merged string.</returns>
        public string GetMergeStringOfRange(Range range)
        {
            string result = string.Empty;

            for (int i = range.StartIndex; i <= range.EndIndex; i++)
                result += this._tokensToView[i].Data;

            return result;
        }

        /// <summary>
        /// This function replaces an exist token to new tokens (replaceTokenList)
        /// </summary>
        /// <param name="range">The range of token for replacing.</param>
        /// <param name="replaceTokenList">The new tokens to replace.</param>
        public void ReplaceToken(Range range, IReadOnlyList<TokenCell> replaceTokenList)
        {
            int addLength = 0;
            foreach (var item in replaceTokenList) addLength += item.Data.Length;

            for (int i = range.StartIndex; i <= range.EndIndex; i++)
                addLength -= this._tokensToView[i].Data.Length;

            int contentIndex = (range.StartIndex == 0) ? 0 : this._tokensToView[range.StartIndex].StartIndex;
            this._tokensToView.RemoveRange(range.StartIndex, range.Count);

            int startIndex = range.StartIndex;
            foreach(var item in replaceTokenList)
            {
                item.StartIndex = contentIndex;
                contentIndex = item.EndIndex + 1;

                this._tokensToView.Insert(startIndex++, item);
            }

            Parallel.Invoke(
                () =>
                {
                    this.UpdateTableForAllPatterns();
                },
                () =>
                {
                    Parallel.For(startIndex, this._tokensToView.Count, i =>
                    {
                        this._tokensToView[i].StartIndex += addLength;
                    });
                });
        }

        public object Clone()
        {
            TokenStorage result = new TokenStorage();

            Parallel.Invoke(
                () =>
                {
                    result._tokensToView = new List<TokenCell>(this._tokensToView);
                },
                () =>
                {
                    result.tableForAllPatternsOnView = new Dictionary<TokenPatternInfo, List<int>>(this.tableForAllPatternsOnView);
                });

            return result;
        }





        private Dictionary<TokenPatternInfo, List<int>> tableForAllPatternsOnView = new Dictionary<TokenPatternInfo, List<int>>();
        internal List<TokenCell> _tokensToView = new List<TokenCell>();
    }
}