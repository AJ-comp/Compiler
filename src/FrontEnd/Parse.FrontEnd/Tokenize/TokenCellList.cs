using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Tokenize
{
    public class TokenCellList : List<TokenCell>
    {
        public void RemoveParallel(Range range)
        {
            // prepare
            int removeTotalLen = 0;
            for (int i = range.StartIndex; i <= range.EndIndex; i++)
                removeTotalLen += this[i].Data.Length;

            // remove process on view tokens
            Parallel.For(range.EndIndex + 1, Count, i =>
            {
                this[i].StartIndex -= removeTotalLen;
            });
            RemoveRange(range.StartIndex, range.Count);
        }


        public void InsertParallel(int startIndex, IEnumerable<TokenCell> tokenCellsToAdd)
        {
            // prepare
            int totalLen = 0;
            foreach (var token in tokenCellsToAdd)
                totalLen += token.Data.Length;

            InsertRange(startIndex, tokenCellsToAdd);

            Parallel.Invoke(() =>
            {
                // adjust index for added tokens
                if (startIndex == 0) return;
                var adjustIndex = this[startIndex - 1].EndIndex + 1;

                Parallel.For(startIndex, startIndex + tokenCellsToAdd.Count(), i =>
                {
                    this[i].StartIndex += adjustIndex;
                });
            },
            () =>
            {
                // adjust index for tokens behind added tokens
                Parallel.For(startIndex + tokenCellsToAdd.Count(), Count, i =>
                {
                    this[i].StartIndex += totalLen;
                });
            });
        }
    }
}
