using Parse.FrontEnd.Ast;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public abstract class OperationAssignExprNode : AssignExprNode
    {
        protected Action LiteralHandlerAction = null;
        protected Action VarHandlerAction = null;
        protected Action ExprHandlerAction = null;

        protected OperationAssignExprNode(AstSymbol node) : base(node)
        {
            RightIsVarHandler += OperationAssignNode_RightIsVarHandler;
            RightIsExprHandler += OperationAssignNode_RightIsExprHandler;
        }


        protected bool CheckCondRightIsLiteral()
        {
            var left = GetIArithmeticType();
            if (left == null)
            {
                AddMCL0011Exception();  // The calculate is not supported
                return false;
            }

            return true;
        }


        protected bool CheckCondRightIsVar()
        {
            var leftVarData = (Left as UseIdentNode).VarData;
            var rightVarData = (Right as UseIdentNode).VarData;

            if (!MiniCChecker.IsAllArithmetic(leftVarData.DataType, rightVarData.DataType))
            {
                AddMCL0011Exception();
                return false;
            }

            return true;
        }

        private void OperationAssignNode_RightIsVarHandler(object sender, EventArgs e)
        {
            var leftVarData = (Left as UseIdentNode).VarData;
            var rightVarData = (Right as UseIdentNode).VarData;

            if (!MiniCChecker.IsAllArithmetic(leftVarData.DataType, rightVarData.DataType))
            {
                AddMCL0011Exception(); // The calculate is not supported
                return;
            }

            if (rightVarData != null)
            {
                VarHandlerAction?.Invoke();
            }
        }

        private void OperationAssignNode_RightIsExprHandler(object sender, EventArgs e)
        {
            try
            {
                ExprHandlerAction?.Invoke();
            }
            catch (FormatException ex)
            {

            }
            catch (NotSupportedException ex)
            {

            }
        }
    }
}
