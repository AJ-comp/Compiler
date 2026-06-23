using System;
using System.Collections.Generic;

namespace Parse.WpfControls.Models
{
    class BlockManager
    {
        public LineBlockPatternCollection LineBlockPatterns { get; } = new LineBlockPatternCollection();
        public MultiLineBlockPatternCollection MultiLineBlockPatterns { get; } = new MultiLineBlockPatternCollection();

        // selected dictionary data struct for speed
        public Dictionary<int, BlockInfo> BlockInfos { get; } = new Dictionary<int, BlockInfo>();

        /*
        public BlockInfo GetActiveBlockInfo(int lineIndex)
        {
            
        }
        */
    }


    class MultiLineBlockPattern
    {
        public string StartString { get; }
        public string EndString { get; }

        public MultiLineBlockPattern(string startString, string endString)
        {
            this.StartString = startString;
            this.EndString = endString;
        }
    }

    class MultiLineBlockPatternCollection
    {
        private HashSet<MultiLineBlockPattern> content = new HashSet<MultiLineBlockPattern>();

        public void Add(string startString, string endString)
        {
            this.content.Add(new MultiLineBlockPattern(startString, endString));
        }
    }


    class LineBlockPatternCollection
    {
        private HashSet<string> content = new HashSet<string>();

        public void Add(string startString)
        {
            this.content.Add(startString);
        }
    }


    class BlockInfo
    {
        public enum BlockType { Line, MultiLine };

        public int SLineIndex { get; }
        public int STokenIndex { get; }
        public string StartString { get; }

        public int ELineIndex { get; }
        public int ETokenIndex { get; }
        public string EndString { get; }

        public BlockType Type { get; }

        public bool Active { get; }
    }
}
