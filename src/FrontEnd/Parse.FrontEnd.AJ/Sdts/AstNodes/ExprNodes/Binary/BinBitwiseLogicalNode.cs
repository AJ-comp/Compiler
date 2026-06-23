using AJ.Common.Helpers;
using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public class BinBitwiseLogicalNode : BinaryExprNode
    {
        public IRBitwiseOperation Operation { get; }

        public BinBitwiseLogicalNode(AstSymbol node, IRBitwiseOperation operation) : base(node)
        {
            Operation = operation;
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            if (Operation == IRBitwiseOperation.LeftShift) LeftShift(LeftNode, RightNode);
            else if (Operation == IRBitwiseOperation.RightShift) RightShift(LeftNode, RightNode);
            else if (Operation == IRBitwiseOperation.BitAnd) BitAnd(LeftNode, RightNode);
            else if (Operation == IRBitwiseOperation.BitOr) BitOr(LeftNode, RightNode);

            if(Type == null) Alarms.Add(AJAlarmFactory.CreateMCL0023(this, Operation.ToDescription()));

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRBinaryExpr(Type.ToIR(), GetDebuggingData());

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


        #region BitOperation
        public ExprNode BitAnd(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (source.Type.DataType == AJDataType.Bool) BoolBitAnd(source, target);
            else if (source.Type.IsIntegerType()) IntegerBitAnd(source, target);

            return this;
        }

        public ExprNode BitOr(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (source.Type.DataType == AJDataType.Bool) BoolBitOr(source, target);
            else if (source.Type.IsIntegerType()) IntegerBitOr(source, target);

            return this;
        }


        public ExprNode LeftShift(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (source.Type.IsIntegerType() && target.Type.IsIntegerType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value << (int)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        public ExprNode RightShift(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (source.Type.IsIntegerType() && target.Type.IsIntegerType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value >> (int)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        public ExprNode BitXor(ExprNode source, ExprNode target)
        {
            if (source.Type == null || target.Type == null) return null;

            if (source.Type.DataType == AJDataType.Bool) BoolBitXor(source, target);
            else if (source.Type.IsIntegerType()) IntegerBitXor(source, target);

            return this;
        }




        private ExprNode BoolBitAnd(ExprNode source, ExprNode target)
        {
            // bool & bool => bool
            if (target.Type.DataType == AJDataType.Bool)
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (bool)source.Value & (bool)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        private ExprNode IntegerBitAnd(ExprNode source, ExprNode target)
        {
            // integer & interget => interger
            if (target.Type.IsIntegerType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value & (int)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        private ExprNode BoolBitOr(ExprNode source, ExprNode target)
        {
            // bool & bool => bool
            if (target.Type.DataType == AJDataType.Bool)
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (bool)source.Value | (bool)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        private ExprNode IntegerBitOr(ExprNode source, ExprNode target)
        {
            // integer & interget => interger
            if (target.Type.IsIntegerType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value | (int)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        private ExprNode BoolBitXor(ExprNode source, ExprNode target)
        {
            // bool & bool => bool
            if (target.Type.DataType == AJDataType.Bool)
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (bool)source.Value ^ (bool)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        private ExprNode IntegerBitXor(ExprNode source, ExprNode target)
        {
            // integer & interget => interger
            if (target.Type.IsIntegerType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (int)source.Value ^ (int)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }
        #endregion
    }
}
