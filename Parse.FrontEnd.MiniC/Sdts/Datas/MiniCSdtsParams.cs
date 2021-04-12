using System.Diagnostics;

namespace Parse.FrontEnd.MiniC.Sdts.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class MiniCSdtsParams : SdtsParams
    {
        public MiniCSymbolTable SymbolTable { get; private set; } = new MiniCSymbolTable();
        public AssemblyInfo AssemblyInfo { get; }
        public RootData RootData { get; }

        public MiniCSdtsParams(int blockLevel, int offset, AssemblyInfo assemblyInfo, RootData rootData) : base(blockLevel, offset)
        {
            AssemblyInfo = assemblyInfo;
            RootData = rootData;
        }

        public override SdtsParams Clone()
        {
            var result = new MiniCSdtsParams(BlockLevel, Offset, AssemblyInfo, RootData)
            {
                SymbolTable = SymbolTable
            };

            return result;
        }

        public string DebuggerDisplay
            => string.Format("Block: {0}, Offset: {1}, Symbol table: [{2}]", BlockLevel, Offset, SymbolTable.DebuggerDisplay);
    }
}
