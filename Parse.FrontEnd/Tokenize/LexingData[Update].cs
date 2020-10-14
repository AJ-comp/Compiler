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
            var rangePairForAll = new RangePair(Range.EmptyRange, new Range(0, tokenCells.Count()));
            var rangePairForParsing = new RangePair(Range.EmptyRange, _indexMap.GetParsingRange());

            _lexedRanges.Add(rangePairForAll);
            _forParsingRanges.Add(rangePairForParsing);
        }


        private RangePair UpdateViewToken(Range rangeToRemove, IEnumerable<TokenCell> tokenCellsToAdd)
        {
            // remove process on view tokens
            _tokensForView.RemoveParallel(rangeToRemove);

            // insert process on view tokens
            int startIndex = rangeToRemove.StartIndex;
            _tokensForView.InsertParallel(startIndex, tokenCellsToAdd);

            return new RangePair(rangeToRemove, new Range(rangeToRemove.StartIndex, tokenCellsToAdd.Count()));
        }

        private RangePair UpdateIndexMapAndParsingToken(Range range, IEnumerable<TokenCell> tokenCells)
        {
            // get range to remove on parsing tokens
            var parsingRangeToRemove = _indexMap.GetRangeForParsing(range);

            // remove process on index map
            _indexMap.RemoveParallel(range, parsingRangeToRemove.Count);

            // insert process on index map
            var propertyTokens = IsParsingList(tokenCells);
            _indexMap.InsertParallel(range.StartIndex, propertyTokens);

            // for test
            CheckRight();


            Range removedRange = RemoveParsingTokens(parsingRangeToRemove);
            int startIndex = (parsingRangeToRemove.IsEmpty)
                                ? _indexMap.FindFirstParsingIndexToForward(range.StartIndex) + 1
                                : parsingRangeToRemove.StartIndex;

            Range insertedRange = InsertParsingTokens(startIndex, tokenCells);

            return new RangePair(removedRange, insertedRange);
        }


        private Range RemoveParsingTokens(Range range)
        {
            Range removedRange = Range.EmptyRange;
            if (!range.IsEmpty)
            {
                // remove
                _tokensForParsing.RemoveRange(range.StartIndex, range.Count);
                removedRange = new Range(range.StartIndex, range.Count);
            }

            return removedRange;
        }

        private Range InsertParsingTokens(int startIndex, IEnumerable<TokenCell> tokenCells)
        {
            // insert
            var toAddTokensForParsing = FilterForParsingToken(tokenCells);
            _tokensForParsing.InsertRange(startIndex, toAddTokensForParsing);

            return new Range(startIndex, toAddTokensForParsing.Count());
        }
    }
}
