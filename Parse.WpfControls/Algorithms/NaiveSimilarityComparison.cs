using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parse.WpfControls.Algorithms
{
    public class NaiveSimilarityComparison : ISimilarityComparison
    {
        public double MinSimilarityValue(string source)
        {
            double result = 0;

            for (int i = 0; i < source.Length; i++)
            {
                result += (int)Math.Pow(2, i);
            }

            return result - 1;
        }

        public string SimilarityPattern(string s)
        {
            string result = string.Empty;

            for (int i = 0; i < s.Length; i++)
            {
                char lower = char.ToLower(s[i]);
                char upper = char.ToUpper(s[i]);

                result += "([^" + lower + "]*" + lower + "|";
                result += "[^" + upper + "]*" + upper + ")";
            }

            return result;
        }

        public double SimilarityValue(string target, string source, out List<uint> matchedIndexes)
        {
            matchedIndexes = new List<uint>();
            double result = 0;

            foreach (var s in source)
            {
                int findIndex = target.IndexOf(s);

                result += (int)Math.Pow(2, (target.Length - 1) - findIndex);
            }

            return result;
        }
    }
}
