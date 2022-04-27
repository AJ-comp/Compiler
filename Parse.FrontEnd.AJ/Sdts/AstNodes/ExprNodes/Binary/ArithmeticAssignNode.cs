using AJ.Common.Helpers;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    /// <summary>
    /// This class parsing +=, -=, *=, /=, etc ...
    /// </summary>
    public sealed class ArithmeticAssignNode : ArithmeticNode, IAssignable
    {
        public ArithmeticAssignNode(AstSymbol node, IRArithmeticOperation operation) : base(node, operation)
        {
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);
            CheckAssignable();

            if (!IsCanParsing) return this;

            if (Operation == IRArithmeticOperation.Add) Add(LeftNode, RightNode);
            else if (Operation == IRArithmeticOperation.Sub) Sub(LeftNode, RightNode);
            else if (Operation == IRArithmeticOperation.Mul) Mul(LeftNode, RightNode);
            else if (Operation == IRArithmeticOperation.Div) Div(LeftNode, RightNode);
            else if (Operation == IRArithmeticOperation.Mod) Mod(LeftNode, RightNode);

            Assign(LeftNode, RightNode);

            if(Type == null) Alarms.Add(AJAlarmFactory.CreateMCL0023(this, Operation.ToDescription()));
            if (RootNode.IsBuild) DBContext.Instance.Insert(this);

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRBinaryExpr();

            if (Operation == IRArithmeticOperation.Add) result.Operation = IRBinaryOperation.AddAssign;
            else if (Operation == IRArithmeticOperation.Sub) result.Operation = IRBinaryOperation.SubAssign;
            else if (Operation == IRArithmeticOperation.Mul) result.Operation = IRBinaryOperation.MulAssign;
            else if (Operation == IRArithmeticOperation.Div) result.Operation = IRBinaryOperation.DivAssign;
            else if (Operation == IRArithmeticOperation.Mod) result.Operation = IRBinaryOperation.ModAssign;

            result.Left = LeftNode.To() as IRExpr;
            result.Right = RightNode.To() as IRExpr;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
