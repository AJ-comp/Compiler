using System.Collections.Generic;
using System.Linq;
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


        /*********************************************************/
        /// <summary>
        /// This function replaces an existing token to new tokens (replaceTokenList)
        /// </summary>
        /// <param name="range">The range of token for replacing.</param>
        /// <param name="replaceTokenList">The new tokens to replace.</param>
        /*********************************************************/
        public void ReplaceToken(Range range, IEnumerable<TokenCell> replaceTokenList)
        {
            var updateRange = UpdateViewToken(range, replaceTokenList);
            var updateRange2 = UpdateIndexMapAndParsingToken(range, replaceTokenList);

            ClearUpdateRanges();
            _lexedRanges.Add(updateRange);
            _forParsingRanges.Add(updateRange2);
            InitAllPatternsTable();
        }


        public bool IsLastVisibleTokenInLine(int index)
        {
            bool result = true;

            for (int i = index; i < _tokensForView.Count; i++)
            {
                var tokenCell = _tokensForView[i];

                if (tokenCell.Data == "\n") break;

                if (tokenCell.Data == " ") continue;
                if (tokenCell.Data == "\t") continue;
                if (tokenCell.Data == "\r") continue;

                result = false;
            }

            return result;
        }



        private Dictionary<TokenPatternInfo, List<int>> _tableForAllPatternsOnView = new Dictionary<TokenPatternInfo, List<int>>();
        private TokenCellList _tokensForView = new TokenCellList();
        private TokenCellList _tokensForParsing = new TokenCellList();
        private IndexMap _indexMap = new IndexMap();


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

        private IEnumerable<bool> IsParsingList(IEnumerable<TokenCell> tokenCells)
        {
            List<bool> result = new List<bool>();

            foreach (var token in tokenCells) result.Add(IsParsingToken(token));

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

        private void CheckRight()
        {
            if (_indexMap.Count != _tokensForView.Count) throw new System.Exception();

            int errorIndex = -1;

            Parallel.For(0, _tokensForView.Count, (i, option) =>
            {
                var isParsingToken = IsParsingToken(_tokensForView[i]);

                if (isParsingToken && _indexMap[i] < 0)
                {
                    errorIndex = i;
                    option.Stop();
                }
                else if (!isParsingToken && _indexMap[i] >= 0)
                {
                    errorIndex = i;
                    option.Stop();
                }
            });

            if (errorIndex >= 0) throw new System.Exception();
        }
    }
}
