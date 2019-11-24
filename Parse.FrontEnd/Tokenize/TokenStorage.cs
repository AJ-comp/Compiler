using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parse.Tokenize
{
    public class TokenStorage
    {
        public List<TokenCell> AllTokens { get; } = new List<TokenCell>();

        /// <summary>
        ///  This property returns a whole table has columns that have positions for each TokenPatternInfo.
        /// </summary>
        public Dictionary<TokenPatternInfo, List<int>> TableForAllPatterns
        {
            get
            {
                Dictionary<TokenPatternInfo, List<int>> result = new Dictionary<TokenPatternInfo, List<int>>();

                Parallel.For(0, this.AllTokens.Count, i =>
                {
                    var token = this.AllTokens[i];

                    if (result.ContainsKey(token.PatternInfo) == false)
                    {
                        // The threads more two can access.
                        // If lock for synchronization the result then slow. Therefore used the try~catch statement.
                        try
                        {
                            result.Add(token.PatternInfo, new List<int>());
                        }
                        catch { }
                    }

                    // Lock only column for the pattern.
                    lock (result[token.PatternInfo])
                    {
                        result[token.PatternInfo].Add(i);
                    }
                });

                return result;
            }
        }

        /// <summary>
        /// This function returns indexes for the special pattern.
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
    }
}
