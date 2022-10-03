using AJ.Common.Helpers;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single
{
    public sealed class SLogicalNode : SingleExprNode
    {
        public IRSingleOperation Operation { get; }

        public SLogicalNode(AstSymbol node, IRSingleOperation operation) : base(node)
        {
            Operation = operation;
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            try
            {
                if (Operation == IRSingleOperation.Not) Not(ExprNode);
                else if (Operation == IRSingleOperation.BitNot) BitNot(ExprNode);

                if (Type == null)
                {
                    if (Operation == IRSingleOperation.Not) Alarms.Add(AJAlarmFactory.CreateMCL0022());
                }
            }
            catch (Exception)
            {
                Alarms.Add(AJAlarmFactory.CreateMCL0013(ExprNode.Type.Name, Operation.ToDescription()));
            }
            finally
            {
                if (RootNode.IsBuild) DBContext.Instance.Insert(this);
            }

            return this;
        }


        public override IRExpression To()
        {
            var result = new IRSingleExpr(Type.ToIR());

            result.Expression = ExprNode.To() as IRExpr;

            if (Operation == IRSingleOperation.Not) result.Operation = IRSingleOperation.Not;
            else throw new Exception();

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }


        public ExprNode Not(ExprNode source)
        {
            if (source.Type == null) return null;
            if (source.Type.DataType != AJDataType.Bool) return null;
            if (source.ValueState != State.Fixed) return this;

            Type = source.Type;
            Value = !(bool)source.Value;
            ValueState = State.Fixed;

            return this;
        }

        public ExprNode BitNot(ExprNode source)
        {
            if (source.Type == null) return null;
            if (!source.Type.IsIntegerType()) return null;
            if (source.ValueState != State.Fixed) return this;

            Type = source.Type;
            Value = ~(int)source.Value;
            ValueState = State.Fixed;

            return this;
        }


        public CompareNode ToCompareNode()
        {
            if (Operation != IRSingleOperation.Not) return null;
            if (ExprNode.Type.DataType != AJDataType.Bool) return null;

            CompareNode result = new CompareNode(null, IRCompareOperation.EQ);
            var right = new BoolLiteralNode(false);

            result.Items.Add(ExprNode);
            result.Items.Add(right);

            return result;
        }
    }
}
