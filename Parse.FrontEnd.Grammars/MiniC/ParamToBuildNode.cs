using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;

namespace Parse.FrontEnd.Grammars.MiniC
{
    public class ParamToBuildNode
    {
        public int BlockLevel { get; internal set; }
        public int Offset { get; internal set; }
        public MiniCSymbolTable BaseSymbolTable { get; }

        public ParamToBuildNode(int blockLevel, int offset, MiniCSymbolTable baseSymbolTable = null)
        {
            BlockLevel = blockLevel;
            Offset = offset;
            BaseSymbolTable = baseSymbolTable;
        }
    }
}
