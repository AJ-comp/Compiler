namespace Parse.FrontEnd.Ast
{
    public class AstBuildParams
    {
        public AstNonTerminal CurNode { get; }
        public SymbolTable BaseSymbolTable { get; }

        public AstBuildParams(AstNonTerminal curNode, SymbolTable baseSymbolTable)
        {
            CurNode = curNode;
            BaseSymbolTable = baseSymbolTable;
        }
    }
}
