using System;
using System.Collections.Generic;

namespace Parse.WpfControls.Models
{
    public class CompletionItem
    {
        public string ImageSource { get; internal set; }
        public string ItemName { get; set; }
        public List<UInt32> MatchedIndexes { get; set; }

        public CompletionItemType ItemType { get; set; }
    }
}
