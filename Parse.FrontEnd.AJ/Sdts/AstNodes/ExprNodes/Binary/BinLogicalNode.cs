using AJ.Common.Helpers;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
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

            if (Operation == IRLogicalOperation.And) And(LeftNode, RightNode);
            else if (Operation == IRLogicalOperation.Or) Or(LeftNode, RightNode);

            Alarms.Add(AJAlarmFactory.CreateMCL0023(this, Operation.ToDescription()));
            if (RootNode.IsBuild) DBContext.Instance.Insert(this);

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


        public ExprNode And(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (target.Type.DataType == AJDataType.Bool)
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (bool)source.Value && (bool)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }

        public ExprNode Or(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (target.Type.DataType == AJDataType.Bool)
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (bool)source.Value || (bool)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }
    }
}
