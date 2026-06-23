using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parse
{
    public class IndexList : List<int>
    {
        public void RemoveParallel(Range range, int adjustVal)
        {
            // remove process on index map
            Parallel.For(range.EndIndex + 1, Count, i =>
            {
                this[i] -= adjustVal;
            });
            RemoveRange(range.StartIndex, range.Count);
        }

        public void InsertParallel(int startIndex, IEnumerable<int> snippetToInsert, int adjustVal)
        {
            Parallel.For(startIndex, Count, i =>
            {
                this[i] += adjustVal;
            });
            this.InsertRange(startIndex, snippetToInsert);
        }
    }
}
