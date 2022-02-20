using AJ.Common.Helpers;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public sealed class BinBitwiseLogicalNode : BinaryExprNode
    {
        IRBitwiseOperation Operation { get; }

        public BinBitwiseLogicalNode(AstSymbol node, IRBitwiseOperation operation) : base(node)
        {
            Operation = operation;
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            try
            {
                if (Operation == IRBitwiseOperation.LeftShift) Result = LeftNode.Result.LeftShift(RightNode.Result);
                else if (Operation == IRBitwiseOperation.RightShift) Result = LeftNode.Result.RightShift(RightNode.Result);
                else if (Operation == IRBitwiseOperation.BitAnd) Result = LeftNode.Result & RightNode.Result;
                else if (Operation == IRBitwiseOperation.BitOr) Result = LeftNode.Result | RightNode.Result;
            }
            catch (Exception)
            {
                AJAlarmFactory.CreateMCL0023(LeftNode.Result.Type.Name, RightNode.Result.Type.Name, Operation.ToDescription());
            }
            finally
            {
                if (RootNode.IsBuild) DBContext.Instance.Insert(this);
            }

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRBinaryExpr();

            if (Operation == IRBitwiseOperation.LeftShift) result.Operation = IRBinaryOperation.LeftShift;
            else if (Operation == IRBitwiseOperation.RightShift) result.Operation = IRBinaryOperation.RightShift;
            else if (Operation == IRBitwiseOperation.BitAnd) result.Operation = IRBinaryOperation.BitAnd;
            else if (Operation == IRBitwiseOperation.BitOr) result.Operation = IRBinaryOperation.BitOr;

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
