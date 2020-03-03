using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public enum DataType { UnKnown = -1, VOID = 0, INT = 1, }


    public class MiniCItemInfoSnippet : SymbolItemSnippet
    {
        public DataType Type { get; internal set; } = DataType.UnKnown;
        public int BlockLevel { get; internal set; } = 0;

        public override object Clone()
        {
            MiniCItemInfoSnippet result = new MiniCItemInfoSnippet();

            result.Type = this.Type;
            result.BlockLevel = this.BlockLevel;

            return result;
        }

        public void BaseCopy(MiniCItemInfoSnippet snippet)
        {
            this.Type = snippet.Type;
            this.BlockLevel = snippet.BlockLevel;
        }
    }

    public class MiniCVarInfoSnippet : MiniCItemInfoSnippet
    {
        public bool Const { get; internal set; } = false;

        public int RelativeAddress { get; internal set; } = -1;
        public int Dimension { get; internal set; } = -1;

        public override object Clone()
        {
            MiniCVarInfoSnippet result = new MiniCVarInfoSnippet();

            result.BaseCopy(this);
            result.Const = this.Const;
            result.RelativeAddress = this.RelativeAddress;
            result.Dimension = this.Dimension;

            return result;
        }
    }

    public class MiniCFuncInfoSnippet : MiniCItemInfoSnippet
    {
        public bool Const { get; internal set; } = false;
        public int RelativeAddress { get; internal set; } = -1;
        public int Dimension { get; internal set; } = -1;

        public override object Clone()
        {
            MiniCFuncInfoSnippet result = new MiniCFuncInfoSnippet();

            result.BaseCopy(this);
            result.Const = this.Const;
            result.RelativeAddress = this.RelativeAddress;
            result.Dimension = this.Dimension;

            return result;
        }
    }

    public class MiniCLocalItemInfo
    {
        public string SymbolName { get; internal set; } = string.Empty;
        public bool Const { get; internal set; } = false;
        public DataType Type { get; internal set; } = DataType.UnKnown;
        public int BlockLevel { get; internal set; } = -1;
        public int RelativeAddress { get; internal set; } = -1;
        public int Dimension { get; internal set; } = -1;

        /// <summary>
        /// This function merges this with target.
        /// If a collision property exist then this property value is used.
        /// A collision condition is in case this property value is not initial value and this property value is not equals with target property value.
        /// </summary>
        /// <param name="target">The target item to merge</param>
        public void Merge(MiniCLocalItemInfo target)
        {
            // If this property value is an initial value and target property value is not an inital value then target property value is used.

            if (this.SymbolName == string.Empty && target.SymbolName != string.Empty) this.SymbolName = target.SymbolName;
            if (this.Const == false && target.Const != false) this.Const = target.Const;
            if (this.Type == DataType.UnKnown && target.Type != DataType.UnKnown) this.Type = target.Type;
            if (this.BlockLevel == -1 && target.BlockLevel != -1) this.BlockLevel = target.BlockLevel;
            if (this.RelativeAddress == -1 && target.RelativeAddress != -1) this.RelativeAddress = target.RelativeAddress;
            if (this.Dimension == -1 && target.Dimension != -1) this.Dimension = target.Dimension;
        }

        public override string ToString() => string.Format("{0},{1},{2},{3},{4},{5}", SymbolName, Const, Type, BlockLevel, RelativeAddress, Dimension);
    }

    public class MiniCSymbolTable : SymbolTable
    {
        private List<MiniCLocalItemInfo> symbolItemTable = new List<MiniCLocalItemInfo>();
        private List<int> levelTable = new List<int>();

        public IReadOnlyList<MiniCLocalItemInfo> SymbolItemTable => this.symbolItemTable;

        public void AddItem(MiniCLocalItemInfo item)
        {
            this.symbolItemTable.Add(item);
        }

        public void IncreaseBlock()
        {
            this.levelTable.Add(this.symbolItemTable.Count - 1);
        }

        public void DecreaseBlock()
        {
            this.levelTable.RemoveAt(this.levelTable.Count - 1);
        }

        public override SymbolTable Merge(SymbolTable target)
        {
            MiniCSymbolTable targetTable = target as MiniCSymbolTable;

            // It can't merge in case n : n
            if (this.SymbolItemTable.Count > 1 && targetTable.SymbolItemTable.Count > 1) return null;

            MiniCSymbolTable criteria = (this.SymbolItemTable.Count <= 1) ? targetTable : this.Clone() as MiniCSymbolTable;
            targetTable = (this.SymbolItemTable.Count <= 1) ? this.Clone() as MiniCSymbolTable : targetTable;

            Parallel.For(0, criteria.SymbolItemTable.Count, (i) =>
            {
                var item = criteria.SymbolItemTable[i];

                foreach (var targetItem in targetTable.SymbolItemTable) item.Merge(targetItem);
            });

            return criteria;
        }

        public override object Clone()
        {
            MiniCSymbolTable result = new MiniCSymbolTable();

            this.symbolItemTable.ForEach(item =>
            {
                result.symbolItemTable.Add(item);
            });

            return result;
        }
    }

    public class MiniCExternSymbolTable
    {

    }
}
