using Parse.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Tokenize
{
    public partial class LexingData
    {
        /// <summary> 
        /// The tokens to view (This include all tokens) 
        /// </summary>
        public IReadOnlyList<TokenCell> TokensForView => _tokensForView;
        public IReadOnlyList<TokenCell> TokensForParsing => _tokensForParsing;


        /// <summary> This property returns a whole table has columns that have positions for each TokenPatternInfo. </summary>
        public Dictionary<TokenPatternInfo, List<int>> TableForAllPatternsOnView => this._tableForAllPatternsOnView;


        /// <summary>
        /// This function initializes the token table into the argument value.
        /// </summary>
        /// <param name="tokenPatternList">The value of the token pattern list to initialize.</param>
        internal void InitTokenTable(IEnumerable<TokenPatternInfo> tokenPatternList)
        {
            this._tableForAllPatternsOnView.Clear();

            foreach (var item in tokenPatternList)
            {
                this._tableForAllPatternsOnView.Add(item, new List<int>());
            }

            this._tableForAllPatternsOnView.Add(TokenPatternInfo.NotDefinedToken, new List<int>());
        }

        private LexingData() { }

        public LexingData(IEnumerable<TokenPatternInfo> tokenPatternList)
        {
            this.InitTokenTable(tokenPatternList);
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

                result = this.TokensForView[tokenIndex].Data.Remove(startIndexToRemove, removeLength);
            }

            return result;
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
            //            var indexes = this.GetIndexesForSpecialPattern(" ", "\t", "\r", "\n");
            var indexes = _lineIndexer;
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
            toIndex = (toIndex == -1) ? this._tokensForView.Count - 1 : toIndex;

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
                result += this._tokensForView[i].Data;

            return result;
        }

        public void InitToken(IEnumerable<TokenCell> tokenCells)
        {
            for (int i = 0; i < tokenCells.Count(); i++)
            {
                var tCell = tokenCells.ElementAt(i);
                _tokensForView.Add(tCell);

                // if it is not token for parsing then index is -1
                var tokenType = tCell.PatternInfo.Terminal.TokenType;
                int index = (tokenType == TokenType.SpecialToken.Comment) ? -1
                              : (tokenType == TokenType.SpecialToken.Delimiter) ? -1
                              : i;

                // add only if it is token for parsing.
                if (index >= 0)
                {
                    _indexMap.Add(_tokensForParsing.Count);
                    _tokensForParsing.Add(tCell);
                }
                else _indexMap.Add(index);
            }

            InitRange(tokenCells);
            InitAllPatternsTable();
        }

        /// <summary>
        /// This function replaces an exist token to new tokens (replaceTokenList)
        /// </summary>
        /// <param name="range">The range of token for replacing.</param>
        /// <param name="replaceTokenList">The new tokens to replace.</param>
        public void ReplaceToken(Range range, IEnumerable<TokenCell> replaceTokenList)
        {
            var updateRange = UpdateViewToken(range, replaceTokenList);
            var updateRange2 = UpdateParsingToken(range, replaceTokenList);

            ClearUpdateRanges();
            _lexedRanges.Add(updateRange);
            _forParsingRanges.Add(updateRange2);
            InitAllPatternsTable();
        }



        private Dictionary<TokenPatternInfo, List<int>> _tableForAllPatternsOnView = new Dictionary<TokenPatternInfo, List<int>>();
        private List<TokenCell> _tokensForView = new List<TokenCell>();
        private List<TokenCell> _tokensForParsing = new List<TokenCell>();
        private IndexMap _indexMap = new IndexMap();



        private RangePair UpdateViewToken(Range rangeToRemove, IEnumerable<TokenCell> tokenCellsToAdd)
        {
            // prepare
            int removeTotalLen = 0;
            for (int i = rangeToRemove.StartIndex; i <= rangeToRemove.EndIndex; i++)
                removeTotalLen += TokensForView[i].Data.Length;

            // remove process on view tokens
            Parallel.For(rangeToRemove.EndIndex + 1, TokensForView.Count, i =>
            {
                _tokensForView[i].StartIndex -= removeTotalLen;
            });
            _tokensForView.RemoveRange(rangeToRemove.StartIndex, rangeToRemove.Count);


            // prepare
            int totalLen = 0;
            foreach (var token in tokenCellsToAdd)
                totalLen += token.Data.Length;

            // insert process on view tokens
            int startIndex = rangeToRemove.StartIndex;
            _tokensForView.InsertRange(startIndex, tokenCellsToAdd);
            Parallel.Invoke(() =>
            {
                // adjust index for added tokens
                if (startIndex == 0) return;
                var adjustIndex = _tokensForView[startIndex - 1].EndIndex + 1;
                Parallel.For(startIndex, startIndex + tokenCellsToAdd.Count(), i =>
                {
                    _tokensForView[i].StartIndex += adjustIndex;
                });

            }, () =>
            {
                // adjust index for tokens behind added tokens
                Parallel.For(startIndex + tokenCellsToAdd.Count(), TokensForView.Count, i =>
                {
                    _tokensForView[i].StartIndex += removeTotalLen;
                });
            });

            return new RangePair(rangeToRemove, new Range(rangeToRemove.StartIndex, tokenCellsToAdd.Count()));
        }

        private RangePair UpdateParsingToken(Range range, IEnumerable<TokenCell> tokenCells)
        {
            // remove process on parsing tokens
            var rangeForParsing = GetRangeForParsing(range);
            _tokensForParsing.RemoveRange(rangeForParsing.StartIndex, rangeForParsing.Count);
            Range removedRange = new Range(rangeForParsing.StartIndex, rangeForParsing.Count);

            // remove process on index map
            Parallel.For(range.EndIndex + 1, _indexMap.Count, i =>
            {
                _indexMap[i] -= rangeForParsing.Count;
            });
            _indexMap.RemoveRange(range.StartIndex, range.Count);

            // insert process on parsing tokens
            int startIndex = range.StartIndex;
            var toAddTokensForParsing = FilterForParsingToken(tokenCells);
            int insertPos = rangeForParsing.StartIndex;
//            int insertPos = FindFirstParsingIndexToForward(startIndex) + 1;
            _tokensForParsing.InsertRange(insertPos, toAddTokensForParsing);

            // insert process on index map
            var indexMapSnippet = CreateIndexMapSnippet(insertPos, tokenCells);
            Parallel.For(startIndex, _indexMap.Count, i =>
            {
                _indexMap[i] += toAddTokensForParsing.Count();
            });
            _indexMap.InsertRange(startIndex, indexMapSnippet);

            return new RangePair(removedRange, new Range(insertPos, toAddTokensForParsing.Count()));
        }


        private IEnumerable<int> CreateIndexMapSnippet(int parsingStartIndex, IEnumerable<TokenCell> tokenCells)
        {
            List<int> result = new List<int>();

            foreach (var token in tokenCells)
            {
                var tokenType = token.PatternInfo.Terminal.TokenType;

                if (tokenType == TokenType.SpecialToken.Comment)
                {
                    result.Add(-1);
                    continue;
                }
                if (tokenType == TokenType.SpecialToken.Delimiter)
                {
                    result.Add(-1);
                    continue;
                }

                result.Add(parsingStartIndex++);
            }

            return result;
        }

        private IEnumerable<TokenCell> FilterForParsingToken(IEnumerable<TokenCell> tokenCells)
        {
            List<TokenCell> result = new List<TokenCell>();

            foreach (var token in tokenCells)
            {
                if (!IsParsingToken(token)) continue;

                result.Add(token);
            }

            return result;
        }

        private bool IsParsingToken(TokenCell token)
        {
            bool result = true;
            var tokenType = token.PatternInfo.Terminal.TokenType;

            if (tokenType == TokenType.SpecialToken.Comment) result = false;
            if (tokenType == TokenType.SpecialToken.Delimiter) result = false;

            return result;
        }

        private int FindFirstParsingIndexToForward(int index)
        {
            int result = -1;

            for (int i = index - 1; i >= 0; i--)
            {
                result = _indexMap[i];
                if (result >= 0) break;
            }

            return result;
        }

        private int FindFirstParsingIndexToBackward(int index)
        {
            int result = -1;

            for (int i = index + 1; i < TokensForView.Count; i++)
            {
                result = _indexMap[i];
                if (result >= 0) break;
            }

            return result;
        }


        private Range GetRangeForParsing(Range range)
        {
            List<int> indexes = new List<int>();

            for (int i = range.StartIndex; i <= range.EndIndex; i++)
            {
                var parsingTokenIndex = _indexMap[i];
                if (parsingTokenIndex == -1) continue;

                indexes.Add(parsingTokenIndex);
            }

            return new Range(indexes.First(), indexes.Count());
        }
    }
}
