namespace Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat
{
    public class MiniCSymbolTable : SymbolTable
    {
        public VarDataList VarDataList { get; } = new VarDataList();
        public FuncDataList FuncDataList { get; } = new FuncDataList();

        public override object Clone()
        {
            MiniCSymbolTable result = new MiniCSymbolTable();

            result.VarDataList.AddRange(this.VarDataList);
            result.FuncDataList.AddRange(this.FuncDataList);

            return result;
        }
    }
}
