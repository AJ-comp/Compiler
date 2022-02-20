using AJ.Common.Helpers;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public sealed class BinLogicalNode : BinaryExprNode
    {
        public IRLogicalOperation Operation { get; }

        public BinLogicalNode(AstSymbol node, IRLogicalOperation operation) : base(node)
        {
            Operation = operation;
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            if (!IsCanParsing) return this;

            try
            {
                if (Operation == IRLogicalOperation.And) Result = LeftNode.Result.And(RightNode.Result);
                else if (Operation == IRLogicalOperation.Or) Result = LeftNode.Result.Or(RightNode.Result);
            }
            catch (Exception)
            {
                Alarms.Add(AJAlarmFactory.CreateMCL0023(LeftNode.Result.Type.Name, 
                                                                                 RightNode.Result.Type.Name, 
                                                                                 Operation.ToDescription()));
            }
            finally
            {
                if (RootNode.IsBuild) DBContext.Instance.Insert(this);
            }

            return this;
        }

        public override IRExpression To()
        {
            IRBinaryExpr result = new IRBinaryExpr();

            if (Operation == IRLogicalOperation.And) result.Operation = IRBinaryOperation.And;
            else if (Operation == IRLogicalOperation.Or) result.Operation = IRBinaryOperation.Or;

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
