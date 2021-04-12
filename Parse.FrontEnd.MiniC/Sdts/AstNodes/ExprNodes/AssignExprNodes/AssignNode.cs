using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class AssignNode : AssignExprNode, IAssignExpression
    {
        /* for interface *************************/
        public IDeclareVarExpression Left => LeftVar;
        public IExprExpression Right { get; private set; }
        /************************************/


        public AssignNode(AstSymbol node) : base(node)
        {
        }

        public override void LeftIsIdentProcess()
        {
            LeftVar.Assign(RightNode.Result);
        }
    }
}
