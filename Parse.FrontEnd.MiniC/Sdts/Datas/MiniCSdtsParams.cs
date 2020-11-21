using System.Diagnostics;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class MiniCSdtsParams : SdtsParams
    {
        public MiniCSymbolTable SymbolTable { get; private set; } = new MiniCSymbolTable();

        public MiniCSdtsParams(int blockLevel, int offset) : base(blockLevel, offset)
        {
        }

        public MiniCSdtsParams(int blockLevel, int offset, MiniCSymbolTable baseTable) : this(blockLevel, offset)
        {
            SymbolTable = new MiniCSymbolTable(baseTable);
        }

        public override SdtsParams Clone()
        {
            var result = new MiniCSdtsParams(BlockLevel, Offset)
            {
                SymbolTable = SymbolTable
            };

            return result;
        }

        public override SdtsParams CloneForNewBlock()
        {
            var result = new MiniCSdtsParams(BlockLevel, Offset, SymbolTable);
            result.BlockLevel++;

            return result;
        }

        public string DebuggerDisplay
            => string.Format("Block: {0}, Offset: {1}, Symbol table: [{2}]", BlockLevel, Offset, SymbolTable.DebuggerDisplay);
    }
}
