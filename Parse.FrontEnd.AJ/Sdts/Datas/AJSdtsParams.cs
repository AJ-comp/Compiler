using System.Diagnostics;

namespace Parse.FrontEnd.AJ.Sdts.Datas
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class AJSdtsParams : SdtsParams
    {
        public AJSymbolTable SymbolTable { get; private set; } = new AJSymbolTable();
        public AssemblyInfo AssemblyInfo { get; }
        public RootData RootData { get; }

        public AJSdtsParams(int blockLevel, int offset, AssemblyInfo assemblyInfo, RootData rootData) : base(blockLevel, offset)
        {
            AssemblyInfo = assemblyInfo;
            RootData = rootData;
        }

        public override SdtsParams Clone()
        {
            var result = new AJSdtsParams(BlockLevel, Offset, AssemblyInfo, RootData)
            {
                SymbolTable = SymbolTable
            };

            return result;
        }

        public string DebuggerDisplay
            => string.Format("Block: {0}, Offset: {1}, Symbol table: [{2}]", BlockLevel, Offset, SymbolTable.DebuggerDisplay);
    }
}
