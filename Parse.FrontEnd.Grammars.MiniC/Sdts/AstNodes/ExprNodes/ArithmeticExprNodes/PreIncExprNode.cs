using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class PreIncExprNode : IncDecExprNode
    {
        public PreIncExprNode(AstSymbol node) : base(node, "++")
        {
        }
    }
}
