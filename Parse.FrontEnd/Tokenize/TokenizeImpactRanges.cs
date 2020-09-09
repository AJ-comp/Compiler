using System.Collections.Generic;

namespace Parse.FrontEnd.Tokenize
{
    public class TokenizeImpactRanges : List<RangePair>
    {
        /// PrevRange : CurRange
        /// The following property pair means PrevRange : CurRange
        /// Therefore the start index of PrevRange equals the start index of CurRagne.

        /*
        public void Add(IReadOnlyList<TokenCell> allTokens, Range rangeToSee, IReadOnlyList<TokenCell> tokensToReplace)
        {
            List<int> diffIndexes = new List<int>();

            for(int i=rangeToSee.StartIndex; i<rangeToSee.EndIndex; i++)
            {
                var convertIndex = i - rangeToSee.StartIndex;
                if (convertIndex >= tokensToReplace.Count) break;

                if(allTokens[i].Equals(tokensToReplace[convertIndex]))
                {
                    if(diffIndexes.Count > 0)
                    {
                        var range = new Range(diffIndexes.First(), diffIndexes.Count);
                        diffIndexes.Clear();

                        this.DelRanges.Add(range);
                        this.AddRanges.Add(range);
                    }
                }
                else diffIndexes.Add(convertIndex);
            }

            // if allTokens with rangeToSee = {2,3,4,5,6}, tokensToReplace = {2,3,4}
            //    because the count of the allTokens more than the count of the tokensToReplace, {5,6} enters into delRange.
            if(rangeToSee.Count > tokensToReplace.Count)
            {
                var sIndex = tokensToReplace.Count + rangeToSee.StartIndex;
                var eIndex = sIndex + (rangeToSee.Count - tokensToReplace.Count) - 1;

                this.DelRanges.Add(new Range(sIndex, (eIndex - sIndex + 1)));
            }
            // The opposition case.
            else if (rangeToSee.Count < tokensToReplace.Count)
            {
                var sIndex = rangeToSee.EndIndex + 1;
                var eIndex = sIndex + (tokensToReplace.Count- rangeToSee.Count) - 1;

                this.AddRanges.Add(new Range(sIndex, (eIndex - sIndex + 1)));
            }
        }
        */

    }
}