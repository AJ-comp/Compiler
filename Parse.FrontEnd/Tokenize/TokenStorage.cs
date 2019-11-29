using Parse.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parse.Tokenize
{
    public class TokenStorage
    {
        private Dictionary<TokenPatternInfo, List<int>> tableForAllPatterns = new Dictionary<TokenPatternInfo, List<int>>();
        public List<TokenCell> AllTokens { get; } = new List<TokenCell>();

        /// <summary>
        ///  This property returns a whole table has columns that have positions for each TokenPatternInfo.
        /// </summary>
        public Dictionary<TokenPatternInfo, List<int>> TableForAllPatterns => this.tableForAllPatterns;

        internal void UpdateTableForAllPatterns()
        {
            foreach (KeyValuePair<TokenPatternInfo, List<int>> item in this.tableForAllPatterns) item.Value.Clear();

            Parallel.For(0, this.AllTokens.Count, i =>
            {
                var token = this.AllTokens[i];

                lock (this.tableForAllPatterns[token.PatternInfo])
                    this.tableForAllPatterns[token.PatternInfo].Add(i);
            });
        }

        internal void InitTokenTable(List<TokenPatternInfo> tokenPatternList)
        {
            this.tableForAllPatterns.Clear();

            foreach(var item in tokenPatternList)
            {
                this.tableForAllPatterns.Add(item, new List<int>());
            }

            this.tableForAllPatterns.Add(TokenPatternInfo.NotDefinedToken, new List<int>());
        }

        /// <summary>
        /// This function returns indexes for the special pattern from the TokenPatternInfo.
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public List<int> GetIndexesForSpecialPattern(TokenPatternInfo pattern)
        {
            var table = this.TableForAllPatterns;
            List<int> result = new List<int>();

            if (table.ContainsKey(pattern))
                result = table[pattern];

            return result;
        }


        public KeyValuePair<TokenPatternInfo, List<int>> GetTokenPatternInfoFromString(string pattern)
        {
            var table = this.TableForAllPatterns;
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
            var table = this.TableForAllPatterns;
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
            int minDiffer = 0xff;
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
            int minDiffer = 0xff;
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
            int minDiffer = 0xff;
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
        /// This function returns a token index from the caretIndex.
        /// </summary>
        /// <param name="offset">The offset index</param>
        /// <param name="recognWay">The standard index for recognition a token</param>
        /// <returns>a token index</returns>
        public int TokenIndexForOffset(int offset, RecognitionWay recognWay = RecognitionWay.Back)
        {
            int index = -1;

            Parallel.For(0, this.AllTokens.Count, (i, loopState) =>
            {
                TokenCell tokenInfo = this.AllTokens[i];

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
        public Tuple<int, int> FindImpactRange(int tokenIndex)
        {
            var indexes = this.GetIndexesForSpecialPattern(" ", "\n");
            var fromIndex = indexes.GetIndexNearestLessThanValue(tokenIndex);
            var toIndex = indexes.GetIndexNearestMoreThanValue(tokenIndex);

            fromIndex = (fromIndex == -1) ? 0 : fromIndex;
            toIndex = (toIndex == -1) ? this.AllTokens.Count : toIndex;

            return new Tuple<int, int>(fromIndex, toIndex - fromIndex + 1);
        }

        /// <summary>
        /// This function returns the impact range on the basis of the arguments.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns>The first value : index, The second value : count </returns>
        public Tuple<int, int> FindImpactRange(int startTokenIndex, int endTokenIndex)
        {
            var indexes = this.GetIndexesForSpecialPattern(" ", "\n");
            var fromIndex = indexes.GetIndexNearestLessThanValue(startTokenIndex);
            var toIndex = indexes.GetIndexNearestMoreThanValue(endTokenIndex);

            fromIndex = (fromIndex == -1) ? 0 : fromIndex;
            toIndex = (toIndex == -1) ? this.AllTokens.Count : toIndex;

            return new Tuple<int, int>(fromIndex, toIndex - fromIndex + 1);
        }

        /// <summary>
        /// This function replaces an exist token to new tokens (replaceTokenList)
        /// </summary>
        /// <param name="startIndex">The start index of token for replacing.</param>
        /// <param name="count">The token number for replacing.</param>
        /// <param name="replaceTokenList">The new tokens to replace.</param>
        public void ReplaceToken(int startIndex, int count, List<TokenCell> replaceTokenList)
        {
            int addLength = 0;
            foreach (var item in replaceTokenList) addLength += item.Data.Length;

            for (int i = startIndex; i < startIndex + count; i++)
                addLength = addLength - this.AllTokens[i].Data.Length;

            Parallel.For(startIndex + count, this.AllTokens.Count, i =>
            {
                this.AllTokens[i].StartIndex += addLength;
            });

            int contentIndex = (startIndex == 0) ? 0 : this.AllTokens[startIndex].StartIndex;
            for (int i = startIndex; i < startIndex + count; i++) this.AllTokens.RemoveAt(startIndex);

            replaceTokenList.ForEach(i =>
            {
                i.StartIndex = contentIndex;
                contentIndex = i.EndIndex + 1;

                this.AllTokens.Insert(startIndex++, i);
            });
        }
    }
}
