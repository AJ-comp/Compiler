using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using System.Dynamic;

namespace Parse.FrontEnd.Grammars.MiniC.AstNodes
{
    public abstract class AstNode
    {
        public MiniCSymbolTable SymbolTable { get; }
        public bool Result { get; } = false;
    }
}
