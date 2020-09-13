using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class PreDecExprNode : IncDecExprNode
    {
        public PreDecExprNode(AstSymbol node) : base(node, "--")
        {
        }
    }
}
