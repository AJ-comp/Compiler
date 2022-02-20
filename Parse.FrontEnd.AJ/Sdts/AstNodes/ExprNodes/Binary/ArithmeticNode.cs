using AJ.Common.Helpers;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public sealed class ArithmeticNode : BinaryExprNode
    {
        public IRArithmeticOperation Operation { get; }

        public ArithmeticNode(AstSymbol node, IRArithmeticOperation operation) : base(node)
        {
            Operation = operation;
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            if (!IsCanParsing) return this;

            try
            {
                if (Operation == IRArithmeticOperation.Add) Result = LeftNode.Result + RightNode.Result;
                else if (Operation == IRArithmeticOperation.Sub) Result = LeftNode.Result - RightNode.Result;
                else if (Operation == IRArithmeticOperation.Mul) Result = LeftNode.Result * RightNode.Result;
                else if (Operation == IRArithmeticOperation.Div) Result = LeftNode.Result / RightNode.Result;
                else if (Operation == IRArithmeticOperation.Mod) Result = LeftNode.Result % RightNode.Result;
            }
            catch (Exception)
            {
                Alarms.Add(AJAlarmFactory.CreateMCL0023(LeftNode.Result.Type.Name, 
                                                                                 RightNode.Result.Type.Name, 
                                                                                 Operation.ToDescription()));

                RootNode.UnLinkedSymbols.Add(this);
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

            if (Operation == IRArithmeticOperation.Add) result.Operation = IRBinaryOperation.Add;
            else if (Operation == IRArithmeticOperation.Sub) result.Operation = IRBinaryOperation.Sub;
            else if (Operation == IRArithmeticOperation.Mul) result.Operation = IRBinaryOperation.Mul;
            else if (Operation == IRArithmeticOperation.Div) result.Operation = IRBinaryOperation.Div;
            else if (Operation == IRArithmeticOperation.Mod) result.Operation = IRBinaryOperation.Mod;

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
