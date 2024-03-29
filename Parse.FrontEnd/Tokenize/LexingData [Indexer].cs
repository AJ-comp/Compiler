﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Tokenize
{
    public partial class LexingData
    {
        /**********************************/
        /// <summary>
        /// <para>This returns the line index for the caret.</para>
        /// <para>캐럿에 대한 라인 인덱스를 가져옵니다.</para>
        /// </summary>
        /**********************************/
        public IEnumerable<int> LineIndexesForCaret
        {
            get
            {
                List<int> result = new List<int> { 0 };

                Parallel.ForEach(_lineIndexer, (tokenIndex) =>
                {
                    result.Add(TokensForView[tokenIndex].StartIndex);
                });

                result.Sort();

                return result;
            }
        }


        private void InitAllPatternsTable()
        {
            _lineIndexer.Clear();

            Parallel.For(0, _tokensForView.Count, i =>
            {
                var token = _tokensForView[i];

                if (token.Data != "\n") return;

                lock (_lineIndexer)
                    _lineIndexer.Add(i);
            });

            _lineIndexer.Sort();
        }


        /**********************************************/
        /// <summary>
        /// <para>This function returns the token list of the lineIndex.</para>
        /// <para>라인 인덱스의 뷰 토큰 리스트를 가져옵니다.</para>
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <returns></returns>
        /**********************************************/
        public IEnumerable<TokenCell> GetTokensForLine(int lineIndex)
        {
            var result = new List<TokenCell>();
            if (lineIndex > _lineIndexer.Count) return result;

            // get tokens of last line
            if (lineIndex == _lineIndexer.Count)
            {
                // check if last line is 0
                // example 
                // if line count is 1 (last line index is 0) get tokens from 0 ~ to TokensToView.Count.
                // else line count is 2 more (last line index is not 0) get tokens from last line token ~ to TokensToView.Count.
                var fromIndex = (_lineIndexer.Count == 0) ? 0 : _lineIndexer.Last() + 1;
                for (int i = fromIndex; i < TokensForView.Count; i++) result.Add(TokensForView[i]);
            }
            else
            {
                var fromIndex = (lineIndex == 0) ? 0 : _lineIndexer[lineIndex - 1] + 1;
                var toIndex = _lineIndexer[lineIndex];

                for (int i = fromIndex; i < toIndex; i++) result.Add(TokensForView[i]);
            }

            return result;
        }


        /*****************************************/
        /// <summary>
        /// <para>This returns absolute offset for line offset</para>
        /// <para>라인 offset에 해당하는 절대 offset 을 반환합니다.</para>
        /// </summary>
        /// <param name="lineIndex"></param>
        /// <param name="offsetOnLine"></param>
        /// <returns></returns>
        /*****************************************/
        public int GetAbsOffsetForLineOffset(int lineIndex, int offsetOnLine)
        {
            var tokensOnLine = GetTokensForLine(lineIndex);
            if (tokensOnLine.Count() == 0) return -1;

            int result = tokensOnLine.Last().EndIndex + 1;
            int indexOnLine = 0;
            foreach (var token in tokensOnLine)
            {
                indexOnLine += token.Data.Length;

                if (offsetOnLine > indexOnLine) continue;

                result = token.StartIndex + token.Data.Length - (indexOnLine - offsetOnLine);
                break;
            }

            return result;
        }


        /*************************************************************/
        /// <summary>
        /// This function returns indexes for the special pattern from the TokenPatternInfo.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        /*************************************************************/
        public List<int> GetIndexesForSpecialPattern(TokenPatternInfo pattern)
        {
            var table = this.TableForAllPatternsOnView;
            List<int> result = new List<int>();

            if (table.ContainsKey(pattern))
                result = table[pattern];

            return result;
        }


        /**************************************/
        /// <summary>
        /// <para>This returns the line index for the token index.</para>
        /// <para>토큰 인덱스의 라인 인덱스를 가져옵니다.</para>
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        /**************************************/
        public int GetLineIndex(int tokenIndex)
        {
            if (TokensForView.Count == 0) return -1;
            if (tokenIndex > TokensForView.Last().EndIndex) return -1;

            var subset = SubSetTo(tokenIndex);

            int result = 0;
            Parallel.ForEach(subset, item =>
            {
                if (item.Data == "\n") Interlocked.Increment(ref result);
            });

            return result;
        }


        public IEnumerable<TokenCell> SubSetTo(int charIndex)
        {
            List<TokenCell> result = new List<TokenCell>();

            foreach (var item in TokensForView)
            {
                if (item.StartIndex > charIndex) break;

                result.Add(item);
            }

            return result;
        }


        public int GetLineCount() => _lineIndexer.Count() + 1;

        /// <summary>
        /// Get character start index for the token index.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns></returns>
        public int GetCharStartIndex(int tokenIndex) => TokensForView[tokenIndex].StartIndex;
        public int GetLineCharIndex(int line) => GetCharStartIndex(_lineIndexer[line]);


        public TokenPos GetTokenPos(TokenData targetToken)
        {
            int charStartIndex = targetToken.StartIndex;
            int charEndIndex = targetToken.EndIndex;

            var result = new TokenPos();
            result.Line = GetLineIndex(charStartIndex);
            result.EndLine = GetLineIndex(charEndIndex);
            
            if (result.Line < 0) return result;

            var tokensInLine = GetTokensForLine(result.Line);
            int tokenColumn = 0;
            foreach(var token in tokensInLine)
            {
                if (token.Contains(charStartIndex)) break;

                tokenColumn++;
            }
            result.TokenColumn = tokenColumn;

            if (result.Line == 0) result.CharColumn = charStartIndex;
            else result.CharColumn = charStartIndex - GetLineCharIndex(result.Line - 1) - 1;

            if (result.EndLine == 0) result.EndColumn = charEndIndex;
            else result.EndColumn = charEndIndex - GetLineCharIndex(result.EndLine - 1) - 1;

            return result;
        }

        public int GetTopFrontIndexFromTokenIndex(string pattern, int tokenIndex)
        {
            var indexes = this.GetIndexesForSpecificPattern(pattern);

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


        /*******************************************************************/
        /// <summary>
        /// <para>This function returns a token index from the caretIndex.</para>
        /// <para>캐럿위치에 해당하는 토큰 인덱스를 가져옵니다.</para>
        /// </summary>
        /// <param name="offset">The offset index</param>
        /// <param name="recognWay">The standard index for recognition a token</param>
        /// <returns>a token index</returns>
        /*******************************************************************/
        public int TokenIndexForOffset(int offset, RecognitionWay recognWay = RecognitionWay.Back)
        {
            int index = -1;

            Parallel.For(0, this._tokensForView.Count, (i, loopState) =>
            {
                TokenCell tokenInfo = this._tokensForView[i];

                if (tokenInfo.Contains(offset, recognWay))
                {
                    index = i;
                    loopState.Stop();
                }
            });

            return index;
        }


        /******************************************************/
        /// <summary>
        /// This function returns indexes for the specific pattern from the string.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        /******************************************************/
        public List<int> GetIndexesForSpecificPattern(string pattern)
        {
            return this.GetTokenPatternInfoFromString(pattern).Value;
        }

        public IEnumerable<int> GetIndexesForSpecificPatterns(params string[] patterns)
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




        /******************************************************/
        /// <summary>
        /// This function returns a index of first token on line index. 
        /// (in other words this function returns the token index after "\n".)
        /// if "\r" or "\n" token is after "\n" it is useless token. bool : false
        /// </summary>
        /******************************************************/
        private List<Tuple<int, bool>> FirstTokenIndexOnLine
        {
            get
            {
                List<Tuple<int, bool>> result = new List<Tuple<int, bool>>();

                Parallel.For(0, TokensForView.Count, i =>
                {
                    var token = TokensForView[i];
                    bool isLast = (i == TokensForView.Count - 1);

                    if (token.Data != "\n") return;

                    // from here on out is case token.Data is "\n"
                    if (isLast)
                    {
                        lock (result) result.Add(new Tuple<int, bool>(i, false));
                        return;
                    }

                    // the token is "\n" after "\n" (useless token)
                    var nextToken = TokensForView[i + 1];
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


        // The token index list for lines
        private List<int> _lineIndexer = new List<int>();
        private Dictionary<int, int> _tokenIndexByLine = new Dictionary<int, int>();
    }
}
