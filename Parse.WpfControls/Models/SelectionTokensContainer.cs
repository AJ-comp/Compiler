using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Parse.WpfControls.Models
{
    public class SelectionTokensContainer
    {
        public List<int> WholeSelectionBag { get; private set; } = new List<int>();

        /// <summary>
        /// The first int : The token index to delete.
        /// The second int : The starting index to delete.
        /// The third int : The length for deleting.
        /// </summary>
        public List<Tuple<int, int, int>> PartSelectionBag { get; } = new List<Tuple<int, int, int>>();


        public void SortAll()
        {
            this.WholeSelectionBag.Sort();
            this.PartSelectionBag.Sort();
        }
    }
}
