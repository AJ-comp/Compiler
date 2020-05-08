namespace Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat
{
    public class MiniCSymbolTable : SymbolTable
    {
        public MiniCSymbolTable Base { get; }
        public VarDataList VarDataList { get; } = new VarDataList();
        public FuncDataList FuncDataList { get; } = new FuncDataList();

        public MiniCSymbolTable(MiniCSymbolTable @base = null)
        {
            Base = @base;
        }
    }
}
