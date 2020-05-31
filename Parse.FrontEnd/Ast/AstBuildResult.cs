using System.Collections.Generic;

namespace Parse.FrontEnd.Ast
{
    public class AstBuildResult
    {
        public object Data { get; }
        public SymbolTable SymbolTable { get; }
        public bool Result { get; internal set; }
        public List<AstSymbol> Nodes { get; } = new List<AstSymbol>();

        public AstBuildResult(object data, SymbolTable symbolTable, bool result = false)
        {
            Data = data;
            SymbolTable = symbolTable;
            Result = result;
        }

        public AstBuildResult(object data, SymbolTable symbolTable, IReadOnlyList<AstSymbol> nodes, bool result = false)
            : this(data, symbolTable, result)
        {
            if (nodes != null) Nodes.AddRange(nodes);
        }
    }
}
