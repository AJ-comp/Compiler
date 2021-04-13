using Parse.FrontEnd.Ast;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.AssignExprNodes
{
    public abstract class OperationAssignExprNode : AssignExprNode, IArithmeticAssignExpression
    {
        protected Action LiteralHandlerAction = null;
        protected Action VarHandlerAction = null;
        protected Action ExprHandlerAction = null;

        public abstract IROperation Operation { get; }
        public IDeclareVarExpression Left => LeftVar;
        public IExprExpression Right { get; private set; }

        protected OperationAssignExprNode(AstSymbol node) : base(node)
        {
        }


        protected bool CheckCondRightIsLiteral()
        {
            if (!AJUtilities.IsArithmeticType(LeftNode.DataKind))
            {
                AddMCL0011Exception();  // The calculate is not supported
                return false;
            }

            return true;
        }


        protected bool CheckCondRightIsVar()
        {
            var leftVarData = (LeftNode as UseIdentNode).VarData;
            var rightVarData = (RightNode as UseIdentNode).VarData;

            if (!AJChecker.IsAllArithmetic(leftVarData.DataType, rightVarData.DataType))
            {
                AddMCL0011Exception();
                return false;
            }

            return true;
        }

        public override void LeftIsIdentProcess()
        {
            if (!CheckCondRightIsLiteral()) return;

            var rightVarData = (RightNode as UseIdentNode).VarData;

            var constant = ArithimeticOperation(Operation);
            if (constant is UnknownConstant) return;

            LeftVar.Assign(constant);

            /*
            if (rightVarData != null)
            {
                VarHandlerAction?.Invoke();
            }
            */
        }
    }
}
