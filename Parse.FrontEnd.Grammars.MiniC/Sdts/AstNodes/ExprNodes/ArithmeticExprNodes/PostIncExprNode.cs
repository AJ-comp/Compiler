using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class PostIncExprNode : IncDecExprNode
    {
        public PostIncExprNode(AstSymbol node) : base(node, "++")
        {
        }
    }
}
