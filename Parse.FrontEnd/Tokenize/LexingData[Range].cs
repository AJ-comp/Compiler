using System.Collections.Generic;
using System.Linq;

namespace Parse.FrontEnd.Tokenize
{
    public partial class LexingData
    {
        public IReadOnlyList<RangePair> RangeToLex => _lexedRanges;
        public IReadOnlyList<RangePair> RangeToParsing => _forParsingRanges;


        private List<RangePair> _lexedRanges = new List<RangePair>();
        private List<RangePair> _forParsingRanges = new List<RangePair>();


        private void ClearUpdateRanges()
        {
            _lexedRanges.Clear();
            _forParsingRanges.Clear();
        }

        private void InitRange(IEnumerable<TokenCell> tokenCells)
        {
            ClearUpdateRanges();
            var rangePairForAll = new RangePair(new Range(-1, 0), new Range(0, tokenCells.Count()));
            var rangePairForParsing = new RangePair(new Range(-1, 0), _indexMap.GetParsingRange());

            _lexedRanges.Add(rangePairForAll);
            _forParsingRanges.Add(rangePairForParsing);
        }

        private void UpdateRanges(Range range, IEnumerable<TokenCell> replaceTokenList)
        {
            ClearUpdateRanges();
            var rangePairForAll = new RangePair(range, new Range(range.StartIndex, replaceTokenList.Count()));
            var rangePairForParsing = new RangePair(range, _indexMap.GetParsingRange());

            _lexedRanges.Add(rangePairForAll);
            _forParsingRanges.Add(rangePairForParsing);
        }
    }
}
