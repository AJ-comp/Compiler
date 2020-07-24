using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes
{
    public abstract class ExprNode : MiniCNode
    {
        protected ExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
