using Parse.Extensions;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Tokenize
{
    public partial class LexingData
    {
        /***************************************************************/
        /// <summary>
        /// This function returns the impact range on the basis of the tokenIndex argument.
        /// tokenIndex 파라메터를 기반으로 한 영향 범위를 가져옵니다.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns>The first value : index, The second value : count </returns>
        /***************************************************************/
        public Range FindImpactRange(int tokenIndex) => this.FindImpactRange(tokenIndex, tokenIndex);


        /***************************************************************/
        /// <summary>
        /// This function returns the impact range on the basis of the arguments.
        /// </summary>
        /// <param name="startTokenIndex"></param>
        /// <param name="endTokenIndex"></param>
        /// <returns>The first value : index, The second value : count </returns>
        /***************************************************************/
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


        /****************************************************/
        /// <summary>
        /// This function returns view token index for parsing token index.
        /// 파싱 토큰 인덱스에 매치되는 뷰 토큰 인덱스를 가져옵니다.
        /// </summary>
        /// <param name="pTokenIndex"></param>
        /// <returns></returns>
        /****************************************************/
        public int GetVIndexFromPIndex(int pTokenIndex)
        {
            int result = -1;

            Parallel.For(0, _indexMap.Count, (i, option) =>
            {
                if (_indexMap[i] != pTokenIndex) return;

                result = i;
                option.Stop();
            });

            return result;
        }


        public int GetPIndexFromVIndex(int vTokenIndex) => (vTokenIndex < 0) ? -1 : _indexMap[vTokenIndex];

        public int GetNearestPIndexFromVIndex(int vTokenIndex)
        {
            var result = GetPIndexFromVIndex(vTokenIndex);
            if (result < 0)
                result = _indexMap.FindFirstParsingIndexToForward(vTokenIndex);

            return result;
        }
    }
}
