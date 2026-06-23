using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Tokenize
{
    public class IndexMap : IndexList
    {
        public int FindFirstParsingIndexToForward(int index)
        {
            int result = -1;

            for (int i = index - 1; i >= 0; i--)
            {
                result = this[i];
                if (result >= 0) break;
            }

            return result;
        }

        public int FindFirstParsingIndexToBackward(int index, int endIndex)
        {
            int result = -1;

            for (int i = index + 1; i <= endIndex; i++)
            {
                result = this[i];
                if (result >= 0) break;
            }

            return result;
        }

        public Range GetParsingRange()
        {
            List<int> result = new List<int>();

            foreach (var item in this)
            {
                if (item >= 0) result.Add(item);
            }

            return (result.Count > 0) ? new Range(0, result.Count) : new Range(-1, 0);
        }

        public Range GetRangeForParsing(Range range)
        {
            List<int> indexes = new List<int>();

            for (int i = range.StartIndex; i <= range.EndIndex; i++)
            {
                var parsingTokenIndex = this[i];
                if (parsingTokenIndex < 0) continue;

                indexes.Add(parsingTokenIndex);
            }

            return (indexes.Count > 0) ? new Range(indexes.First(), indexes.Count())
                                                    : Range.EmptyRange;
        }

        public void InsertParallel(int startIndex, IEnumerable<bool> propertyTokensToAdd)
        {
            int nextIndex = FindFirstParsingIndexToForward(startIndex) + 1;

            int propertyTokenCount = 0;
            foreach (var propertyToken in propertyTokensToAdd)
            {
                if (propertyToken)
                {
                    propertyTokenCount++;
                    this.Insert(startIndex++, nextIndex++);
                }
                else this.Insert(startIndex++, -1);
            }

            // post process
            Parallel.For(startIndex, Count, i =>
            {
                if (this[i] < 0) return;

                this[i] += propertyTokenCount;
            });
        }
    }
}
