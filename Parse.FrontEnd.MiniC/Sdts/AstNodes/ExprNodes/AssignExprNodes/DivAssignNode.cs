using Parse.FrontEnd.Ast;
using Parse.Types.Operations;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class DivAssignNode : OperationAssignExprNode
    {
        public DivAssignNode(AstSymbol node) : base(node)
        {
        }

        private void AddAssignNode_RightIsLiteralHandler(object sender, EventArgs e)
        {
            if (!CheckCondRightIsLiteral()) return;

            var left = GetIArithmeticType();
            var leftVarData = (Left as UseIdentNode).VarData;

            var constant = (leftVarData as IArithmeticOperation).Div(Right.Result);
            leftVarData.Assign(constant);
        }

        private void AddAssignNode_RightIsVarHandler(object sender, EventArgs e)
        {
            if (!CheckCondRightIsVar()) return;

            var leftVarData = (Left as UseIdentNode).VarData;
            var rightVarData = (Right as UseIdentNode).VarData;

            var constant = (leftVarData as IArithmeticOperation).Div(rightVarData);
            leftVarData.Assign(constant);
        }

        private void AddAssignNode_RightIsExprHandler(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
