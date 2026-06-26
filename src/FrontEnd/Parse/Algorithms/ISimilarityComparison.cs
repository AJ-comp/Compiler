using System.Collections.Generic;

namespace Janglim.Algorithms
{
    public interface ISimilarityComparison
    {
        double SimilarityValue(string target, string source, out List<uint> matchedIndexes);
    }
}
