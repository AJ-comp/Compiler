using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.WpfControls.Algorithms
{
    public interface ISimilarityComparison
    {
        double SimilarityValue(string target, string source);
    }
}
