using System;
using System.Collections.Generic;

namespace Parse.WpfControls.Algorithms
{
    /// <summary>
    /// 
    /// </summary>
    public class LikeVSSimilarityComparison : ISimilarityComparison
    {
        private uint GetPremium(List<uint> matchedIndexes)
        {
            uint result = 0;
            uint prevIndex = 0;

            foreach(var index in matchedIndexes)
            {
                if (prevIndex + 1 == index) result++;

                prevIndex = index;
            }

            return result;
        }

        private double CalculateSimilarityValue(string target, string source, List<uint> matchedIndexes)
        {
            if (source.Length != matchedIndexes.Count) return 0;

//            +
            double c = (double)matchedIndexes.Count / (double)target.Length;
            return c + this.GetPremium(matchedIndexes);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <see cref="https://www.lucidchart.com/documents/edit/efa4cdca-e22c-48de-8e7c-86854c7ce64c/dVJUlhUd6rTp?beaconFlowId=E95A0A68EF9373CA"/>
        /// <returns></returns>
        public double SimilarityValue(string target, string source, out List<uint> matchedIndexes)
        {
            matchedIndexes = new List<UInt32>();

            if (source.Length <= 0 || target.Length <= 0) return 0;
            if (source.Length > target.Length) return 0;
            if (char.ToUpper(source[0]) != char.ToUpper(target[0])) return 0;

            bool prefixMode = false;

            int x = 1;
            int y = 1;
            matchedIndexes.Add(0); // already s[0] and t[0] is compared.
            while (true)
            {
                if (x >= target.Length) break;
                if (y >= source.Length) break;
                if (matchedIndexes.Count >= source.Length) break;

                var t = target[x];
                var s = source[y];

                if(prefixMode)
                {
                    if (t == char.ToUpper(s))
                    {
                        y++;
                        matchedIndexes.Add((uint)x);
                        prefixMode = false;
                    }
                }
                else
                {
                    if (t == s)
                    {
                        y++;
                        matchedIndexes.Add((uint)x);
                    }
                    else if(matchedIndexes.Count > 0) prefixMode = true;
                }

                x++;
            }

            return this.CalculateSimilarityValue(target, source, matchedIndexes);
        }
    }
}
