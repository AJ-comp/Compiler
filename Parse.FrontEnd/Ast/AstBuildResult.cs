namespace Parse.FrontEnd.Ast
{
    public class AstBuildResult
    {
        public object Data { get; }
        public SymbolTable SymbolTable { get; }
        public bool Result { get; internal set; }

        public AstBuildResult(object data, SymbolTable symbolTable, bool result = false)
        {
            Data = data;
            SymbolTable = symbolTable;
            Result = result;
        }
    }
}
