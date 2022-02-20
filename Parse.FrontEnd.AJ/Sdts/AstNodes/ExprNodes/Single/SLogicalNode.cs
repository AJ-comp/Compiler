using AJ.Common.Helpers;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single
{
    public sealed class SLogicalNode : SingleExprNode
    {
        public IRLogicalOperation Operation { get; }

        public SLogicalNode(AstSymbol node, IRLogicalOperation operation) : base(node)
        {
            Operation = operation;
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            try
            {
                if (ExprNode.Result == null) return this;

                if (Operation == IRLogicalOperation.Not) Result = !ExprNode.Result;
            }
            catch (Exception)
            {
                if (Operation == IRLogicalOperation.Not) Alarms.Add(AJAlarmFactory.CreateMCL0022());
                else Alarms.Add(AJAlarmFactory.CreateMCL0013(ExprNode.Result.Type.Name, Operation.ToDescription()));
            }
            finally
            {
                if (RootNode.IsBuild) DBContext.Instance.Insert(this);
            }

            return this;
        }


        public override IRExpression To()
        {
            var result = new IRSingleExpr();

            result.Expression = ExprNode.To() as IRExpr;

            if (Operation == IRLogicalOperation.Not)
                result.Operation = IRSingleOperation.Not;
            else throw new Exception();

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
