using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.AssignExprNodes
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
