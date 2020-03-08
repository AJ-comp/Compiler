using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse
{
    public class SelectionTokensContainer
    {
        /// <summary>
        /// The first int : The token index to delete.
        /// The second int : The starting index to delete.
        /// The third int : The length for deleting.
        /// </summary>
        public List<Tuple<int, int, int>> PartSelectionBag { get; } = new List<Tuple<int, int, int>>();
        public List<int> WholeSelectionBag { get; private set; } = new List<int>();

        /// <summary>
        /// This property returns a range of selection index. (starting index, a count)
        /// </summary>
        public Range Range
        {
            get
            {
                int minIndex = -1;
                int maxIndex = -1;
                if (this.WholeSelectionBag.Count > 0)
                {
                    minIndex = this.WholeSelectionBag.First();
                    maxIndex = this.WholeSelectionBag.Last();
                }

                if (this.PartSelectionBag.Count > 0)
                {
                    if (minIndex == -1)
                        minIndex = this.PartSelectionBag.First().Item1;
                    else
                    {
                        if (this.PartSelectionBag.First().Item1 < minIndex)
                            minIndex = this.PartSelectionBag.First().Item1;
                    }

                    if (maxIndex == -1)
                        maxIndex = this.PartSelectionBag.Last().Item1;
                    else
                    {
                        if (this.PartSelectionBag.Last().Item1 > maxIndex)
                            maxIndex = this.PartSelectionBag.Last().Item1;
                    }
                }

                return new Range(minIndex, maxIndex - minIndex + 1);
            }
        }


        public void SortAll()
        {
            this.WholeSelectionBag.Sort();
            this.PartSelectionBag.Sort();
        }

        public bool IsEmpty()
        {
            if (this.WholeSelectionBag.Count > 0) return false;
            if (this.PartSelectionBag.Count > 0) return false;

            return true;
        }

        /// <summary>
        /// This function returns the index when tokenIndex argument is matched with the first item of the PartSelectionBag tuple.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns>The matching index.</returns>
        public int GetIndexInPartSelectionBag(int tokenIndex)
        {
            int result = -1;

            for(int i=0; i<this.PartSelectionBag.Count; i++)
            {
                if (this.PartSelectionBag[i].Item1 == tokenIndex)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }

        /// <summary>
        /// This function returns the index when tokenIndex argument is matched with the element of the WholeSelectionBag.
        /// </summary>
        /// <param name="tokenIndex"></param>
        /// <returns>The matching index.</returns>
        public int IsTokenIndexInWholeSelectionBag(int tokenIndex)
        {
            int result = -1;

            for (int i = 0; i < this.WholeSelectionBag.Count; i++)
            {
                if (this.WholeSelectionBag[i] == tokenIndex)
                {
                    result = i;
                    break;
                }
            }

            return result;
        }
    }
}
