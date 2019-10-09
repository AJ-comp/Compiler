using System.Collections.Generic;

namespace Parse.WpfControls.Algorithms
{
    public interface ISimilarityComparison
    {
        double SimilarityValue(string target, string source, out List<uint> matchedIndexes);
    }
}
