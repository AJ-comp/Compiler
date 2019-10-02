using System.Text.RegularExpressions;

namespace Parse.WpfControls.Algorithms
{
    /// <summary>
    /// 
    /// </summary>
    /// <see cref="https://www.lucidchart.com/documents/edit/efa4cdca-e22c-48de-8e7c-86854c7ce64c/dVJUlhUd6rTp?beaconFlowId=E95A0A68EF9373CA"/>
    public class VSLikeSimilarityComparison : ISimilarityComparison
    {
        private double CalculateSimilarityValue(string target, string source, int firstMatchIndex)
        {
            return (double)source.Length / target.Length;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public double SimilarityValue(string target, string source)
        {
            if (source.Length > target.Length) return 0;

            bool prefixMode = false;

            int x = 0;
            int y = 0;
            int checkValue = 0;
            int firstMatchIndex = -1;
            while (true)
            {
                if (x >= target.Length) break;
                if (y >= source.Length) break;
                if (checkValue >= source.Length) break;

                var t = target[x++];
                var s = source[y];

                if(prefixMode)
                {
                    if (t == char.ToUpper(s))
                    {
                        y++;
                        checkValue++;
                        prefixMode = false;
                    }
                }
                else
                {
                    if (t == s)
                    {
                        if (checkValue == 0) firstMatchIndex = x - 1;
                        y++;
                        checkValue++;
                    }
                    else if(checkValue > 0) prefixMode = true;
                }
            }

            return (checkValue >= source.Length) ? (double)this.CalculateSimilarityValue(target, source, firstMatchIndex) : 0;
        }
    }
}
