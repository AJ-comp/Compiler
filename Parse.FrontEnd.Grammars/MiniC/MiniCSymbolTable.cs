using System.Collections.Generic;
using System.Threading.Tasks;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class MiniCSymbolItems
    {
        public enum DataType { UnKnown=-1, VOID=0, INT=1, }

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
        public void Merge(MiniCSymbolItems target)
        {
            // If this property value is an initial value and target property value is not an inital value then target property value is used.

            if (this.SymbolName == string.Empty && target.SymbolName != string.Empty) this.SymbolName = target.SymbolName;
            if (this.Const == false && target.Const != false) this.Const = target.Const;
            if (this.Type == DataType.UnKnown && target.Type != DataType.UnKnown) this.Type = target.Type;
            if (this.BlockLevel == -1 && target.BlockLevel != -1) this.BlockLevel = target.BlockLevel;
            if (this.RelativeAddress == -1 && target.RelativeAddress != -1) this.RelativeAddress = target.RelativeAddress;
            if (this.Dimension == -1 && target.Dimension != -1) this.Dimension = target.Dimension;
        }
    }

    public class MiniCSymbolTable : SymbolTable
    {
        private List<MiniCSymbolItems> symbolItemTable = new List<MiniCSymbolItems>();
        private List<int> levelTable = new List<int>();

        public IReadOnlyList<MiniCSymbolItems> SymbolItemTable => this.symbolItemTable;

        public void AddItem(MiniCSymbolItems item)
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
            MiniCSymbolTable result = this.Clone() as MiniCSymbolTable;

            Parallel.For(0, result.SymbolItemTable.Count, (i) =>
            {
                var item = result.SymbolItemTable[i];

                foreach (var targetItem in (target as MiniCSymbolTable).SymbolItemTable) item.Merge(targetItem);
            });

            return result;
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
