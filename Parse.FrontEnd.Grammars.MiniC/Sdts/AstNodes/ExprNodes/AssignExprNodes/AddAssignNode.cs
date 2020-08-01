using Parse.FrontEnd.Ast;
using Parse.Types.ConstantTypes;
using Parse.Types.Operations;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public class AddAssignNode : OperationAssignExprNode
    {
        public AddAssignNode(AstSymbol node) : base(node)
        {
            RightIsLiteralHandler += AddAssignNode_RightIsLiteralHandler;
            RightIsVarHandler += AddAssignNode_RightIsVarHandler;
            RightIsExprHandler += AddAssignNode_RightIsExprHandler;
        }

        private void AddAssignNode_RightIsLiteralHandler(object sender, EventArgs e)
        {
            if (!CheckCondRightIsLiteral())
            {
                if (!(Left.Result is StringConstant)) return;

                (Left.Result as StringConstant).Add(Right.Result);
            }

            var left = GetIArithmeticType();
            var leftVarData = (Left as UseIdentNode).VarData;

            var constant = left.Add(Right.Result);
            leftVarData.Assign(constant);
        }

        private void AddAssignNode_RightIsVarHandler(object sender, EventArgs e)
        {
            if (!CheckCondRightIsVar()) return;

            var leftVarData = (Left as UseIdentNode).VarData;
            var rightVarData = (Right as UseIdentNode).VarData;

            var constant = (leftVarData as IArithmeticOperation).Add(rightVarData);
            leftVarData.Assign(constant);
        }

        private void AddAssignNode_RightIsExprHandler(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}
