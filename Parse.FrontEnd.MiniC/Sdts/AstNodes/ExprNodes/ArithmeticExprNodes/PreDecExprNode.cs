using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class PreDecExprNode : IncDecExprNode
    {
        public PreDecExprNode(AstSymbol node) : base(node, "--")
        {
        }
    }
}
