using Parse.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parse
{
    public class Range
    {
        public int StartIndex { get; }
        public int Count { get; }
        public int EndIndex { get; }

        public Range(int startIndex, int count)
        {
            this.StartIndex = startIndex;
            this.Count = count;
            this.EndIndex = this.StartIndex + this.Count - 1;
        }

        /// <summary>
        /// This function judgment whether a parameter range includes in this range.
        /// </summary>
        /// <param name="range">The range to compare</param>
        /// <returns>returns true if be included else false</returns>
        public bool IsInclude(Range range) => (this.StartIndex <= range.StartIndex && this.EndIndex >= range.EndIndex) ? true : false;

        /// <summary>
        /// This function judgment whether a parameter range intersects with this range.
        /// </summary>
        /// <param name="range">The range to compare</param>
        /// <returns>returns true if be intersect else false</returns>
        public bool IsIntersect(Range range)
        {
            if (this.StartIndex <= range.StartIndex && range.StartIndex <= this.EndIndex) return true;
            if (this.StartIndex >= range.EndIndex && range.EndIndex >= this.EndIndex) return true;

            return false;
        }

        public static Range Merge(Range target1, Range target2)
        {
            var startIndex = (target1.StartIndex <= target2.StartIndex) ? target1.StartIndex : target2.StartIndex;
            var endIndex = (target1.EndIndex >= target2.EndIndex) ? target1.EndIndex : target2.EndIndex;

            var count = endIndex - startIndex + 1;

            return new Range(startIndex, count);
        }

        public static bool operator > (Range target1, Range target2)
        {
            if (target1.StartIndex > target2.StartIndex) return true;
            if (target1.EndIndex < target2.EndIndex) return true;

            return false;
        }
        public static bool operator < (Range target1, Range target2) => !(target1 > target2);

        public static bool operator >= (Range target1, Range target2)
        {
            if (target1 > target2) return true;
            if (target1 == target2) return true;

            return false;
        }

        public static bool operator <= (Range target1, Range target2)
        {
            if (target1 < target2) return true;
            if (target1 == target2) return true;

            return false;
        }

        public static bool operator ==(Range target1, Range target2) => target1.Equals(target2);
        public static bool operator !=(Range target1, Range target2) => !target1.Equals(target2);

        public override string ToString() => string.Format("{0}~{1}:{2}", this.StartIndex, this.EndIndex, this.Count);

        public override bool Equals(object obj)
        {
            return obj is Range range &&
                   StartIndex == range.StartIndex &&
                   Count == range.Count;
        }

        public override int GetHashCode()
        {
            var hashCode = 1523823687;
            hashCode = hashCode * -1521134295 + StartIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + Count.GetHashCode();
            return hashCode;
        }
    }


    /// <summary>
    /// This class defines a list of the Range that not permit repetition.
    /// </summary>
    public class RangeList : IList<Range>
    {
        private List<Range> ranges = new List<Range>();
        public Range this[int index] { get => ((IList<Range>)ranges)[index]; set => ((IList<Range>)ranges)[index] = value; }
        public int Count => this.ranges.Count;
        public bool IsReadOnly => false;

        private bool OverWriteOnCondition(Range item)
        {
            bool result = false;

            for (int i = 0; i < this.Count; i++)
            {
                var element = this[i];

                if (item.IsInclude(element))
                {
                    // overwrite
                    this[i] = item;
                    result = true;
                    break;
                }
            }

            return result;
        }

        private void Arrange()
        {
            for (int i = 0; i < this.Count; i++)
            {
                var ei = this[i];

                for (int j = i + 1; j < this.Count; j++)
                {
                    var ej = this[j];
                    if (ei > ej) Arranger.Swap(ref ei, ref ej);
                }
            }
        }

        /// <summary>
        /// This function returns a mergeable index block list.
        /// </summary>
        /// <returns>A mergeable index block list</returns>
        private List<List<int>> GetMergeableIndexBlockList()
        {
            // ex return shape : {0,1},{2},{3,4,5}
            List<List<int>> result = new List<List<int>>();

            List<int> mergeBlock = new List<int>();
            for (int i = 0; i < this.Count; i++)
            {
                mergeBlock.Add(i);

                if (i == this.Count - 1)
                {
                    result.Add(mergeBlock);
                    continue;
                }

                var element = this[i];
                var nextElement = this[i + 1];

                if (element.IsIntersect(nextElement) == false)
                {
                    result.Add(mergeBlock);
                    mergeBlock = new List<int>();
                }
            }

            return result;
        }

        private void MergeIntersect()
        {
            // ex mergeable index block list : {0,1},{2},{3,4,5} 
            //       1. 0~1 merge
            //       2. 2~2 merge
            //       3. 3~5 merge
            List<Range> newRanges = new List<Range>();
            foreach (var block in this.GetMergeableIndexBlockList())
            {
                newRanges.Add(Range.Merge(this[block.First()], this[block.Last()]));
            }

            this.Clear();
            this.ranges = newRanges;
        }

        public void Add(Range item)
        {
            if (this.IsInclude(item)) return;

            // If it exists the element overlapped by item then overwrite the element with item.
            if (this.OverWriteOnCondition(item) == false) this.ranges.Add(item);

            this.Arrange();
            this.MergeIntersect();
        }

        public void Clear() => this.ranges.Clear();

        public bool Contains(Range item)
        {
            bool result = false;

            Parallel.ForEach(this, (thisItem, loopOption) =>
            {
                if (thisItem.Equals(item))
                {
                    result = true;
                    loopOption.Stop();
                }
            });

            return result;
        }

        public void CopyTo(Range[] array, int arrayIndex) => this.ranges.CopyTo(array, arrayIndex);
        public IEnumerator<Range> GetEnumerator() => ((IList<Range>)ranges).GetEnumerator();
        public int IndexOf(Range item) => this.IndexOf(item);
        public void Insert(int index, Range item) => this.Insert(index, item);
        public bool Remove(Range item) => this.Remove(item);
        public void RemoveAt(int index) => this.RemoveAt(index);
        IEnumerator IEnumerable.GetEnumerator() => ((IList<Range>)ranges).GetEnumerator();

        public bool IsInclude(Range range)
        {
            bool result = false;

            Parallel.ForEach(this, (item, loopOption) =>
            {
                if (item.IsInclude(range))
                {
                    result = true;
                    loopOption.Stop();
                }
            });

            return result;
        }
    }
}
