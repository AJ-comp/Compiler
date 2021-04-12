using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class AddAssignNode : OperationAssignExprNode
    {
        public override IROperation Operation => IROperation.Add;

        public AddAssignNode(AstSymbol node) : base(node)
        {
        }
    }
}
