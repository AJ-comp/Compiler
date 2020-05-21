namespace Parse.FrontEnd.Ast
{
    public class AstParams
    {
        public AstNonTerminal CurNode { get; }
        public SymbolTable BaseSymbolTable { get; }

        public AstParams(AstNonTerminal curNode, SymbolTable baseSymbolTable)
        {
            CurNode = curNode;
            BaseSymbolTable = baseSymbolTable;
        }
    }
}
