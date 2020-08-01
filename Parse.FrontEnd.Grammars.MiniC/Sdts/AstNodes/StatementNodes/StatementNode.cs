using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes
{
    public abstract class StatementNode : MiniCNode
    {
        protected StatementNode(AstSymbol node) : base(node)
        {
        }
    }
}
