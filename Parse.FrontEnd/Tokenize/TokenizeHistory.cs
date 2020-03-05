using System.Collections.Generic;

namespace Parse.Tokenize
{
    public class TokenizeHistory
    {
        internal List<Range> deletedIndexes = new List<Range>();
        internal List<Range> addedIndexes = new List<Range>();

        public TokenStorage PreviousTokenStorage { get; }
        public IReadOnlyList<Range> DeletedIndexes => deletedIndexes;
        public IReadOnlyList<Range> AddedIndexes => addedIndexes;
    }
}
