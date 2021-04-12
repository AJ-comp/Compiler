using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class DivAssignNode : OperationAssignExprNode
    {
        public override IROperation Operation => IROperation.Div;

        public DivAssignNode(AstSymbol node) : base(node)
        {
        }
    }
}
