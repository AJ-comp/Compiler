using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Tokenize
{
    public partial class TokenStorage : ICloneable
    {
        public IEnumerable<int> LineIndexes
        {
            get
            {
                List<int> result = new List<int> { 0 };

                Parallel.ForEach(TokensToView, (token) =>
                {
                    if (token.Data == "\n")
                        result.Add(token.StartIndex);
                });

                result.Sort();

                return result;
            }
        }


        /// <summary>
        /// This function returns a index of first token on line index. (in other words this function returns the token index after "\n".)
        /// if "\r" or "\n" token is after "\n" it is useless token. item2 = 0
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <returns></returns>
        public IEnumerable<Tuple<int, int>> TokenIndexByLine
        {
            get
            {
                if (_tokenIndexByLine.Count > 0) return _tokenIndexByLine;

                var list = FirstTokenIndexOnLine;

                // convert
                int offset = 0;
                for (int i = 0; i < list.Count; i++)
                {
                    var current = list[i];

                    int count = 0;
                    if (current.Item2)
                    {
                        var to = list[i].Item1;
                        count = to - offset - 1;
                    }
                    _tokenIndexByLine.Add(new Tuple<int, int>(offset, count));
                    offset = current.Item1 + 1;

                    // if there is a line that "\n" is not included (real last line)
                    if (i == list.Count - 1 && current.Item2)
                    {
                        count = TokensToView.Count - offset;
                        _tokenIndexByLine.Add(new Tuple<int, int>(offset, count));
                    }
                }

                return _tokenIndexByLine;
            }
        }


        /// <summary>
        /// This function returns tokens that there is on lineIndex.
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <returns></returns>
        public IEnumerable<TokenCell> GetTokensForLine(int lineIndex)
        {
            var list = TokenIndexByLine;

            var result = new List<TokenCell>();
            if (lineIndex >= list.Count()) return result;

            var range = list.ElementAt(lineIndex);
            if (range.Item2 <= 0) return result;

            int to = range.Item1 + range.Item2;
            for (int i = range.Item1; i < to; i++)
                result.Add(TokensToView[i]);

            return result;
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


        public int GetLineIndex(int tokenIndex)
        {
            if (tokenIndex >= TokensToView.Count) return -1;

            int result = 0;
            Parallel.For(0, tokenIndex, (i) =>
            {
                if (TokensToView[i].Data == "\n") result++;
            });

            return result;
        }


        public int GetColumnIndexOfLine(int tokenIndex)
        {
            var line = GetLineIndex(tokenIndex);
            if (line < 0) return -1;

            int lineStartIndex = 0;
            for (int i = tokenIndex; i >= 0; i--)
            {
                if (TokensToView[tokenIndex].Data == "\n") lineStartIndex = i;
            }

            return tokenIndex - lineStartIndex;
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




        /// <summary>
        /// This function returns a index of first token on line index. (in other words this function returns the token index after "\n".)
        /// if "\r" or "\n" token is after "\n" it is useless token. bool : false
        /// </summary>
        private List<Tuple<int, bool>> FirstTokenIndexOnLine
        {
            get
            {
                List<Tuple<int, bool>> result = new List<Tuple<int, bool>>();

                Parallel.For(0, TokensToView.Count, i =>
                {
                    var token = TokensToView[i];
                    bool isLast = (i == TokensToView.Count - 1);

                    if (token.Data != "\n") return;

                    // from here on out is case token.Data is "\n"
                    if (isLast)
                    {
                        lock (result) result.Add(new Tuple<int, bool>(i, false));
                        return;
                    }

                    // the token is "\n" after "\n" (useless token)
                    var nextToken = TokensToView[i + 1];
                    if (nextToken.Data == "\n")
                    {
                        lock (result) result.Add(new Tuple<int, bool>(i, false));
                        return;
                    }

                    // usefule token
                    lock (result) result.Add(new Tuple<int, bool>(i, true));
                });

                result.Sort();

                return result;
            }
        }


        private List<Tuple<int, int>> _tokenIndexByLine = new List<Tuple<int, int>>();
    }
}
