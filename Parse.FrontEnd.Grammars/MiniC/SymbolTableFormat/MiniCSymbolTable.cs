namespace Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat
{
    public class MiniCSymbolTable : SymbolTable
    {
        public MiniCSymbolTable Base { get; }
        public VarDataList VarDataList { get; } = new VarDataList();
        public FuncDataList FuncDataList { get; } = new FuncDataList();

        public VarDataList AllVarList
        {
            get
            {
                VarDataList result = new VarDataList();

                MiniCSymbolTable currentTable = this;
                while(currentTable != null)
                {
                    result.AddRange(currentTable.VarDataList);
                    currentTable = currentTable.Base;
                }

                return result;
            }
        }

        public MiniCSymbolTable(MiniCSymbolTable @base = null)
        {
            Base = @base;
        }
    }
}
