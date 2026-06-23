using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public sealed class CompareNode : BinaryExprNode
    {
        public IRCompareOperation Operation { get; }
        public bool AlwaysTrue { get; private set; } = false;
        public bool AlwaysFalse { get; private set; } = false;


        public CompareNode(AstSymbol node, IRCompareOperation operation) : base(node)
        {
            Operation = operation;
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            if (!IsCanParsing) return this;

            if (Operation == IRCompareOperation.EQ) EQ(LeftNode, RightNode);
            else if (Operation == IRCompareOperation.NE) NotEQ(LeftNode, RightNode);
            else if (Operation == IRCompareOperation.GT) GreaterThan(LeftNode, RightNode);
            else if (Operation == IRCompareOperation.GE) GreaterEqual(LeftNode, RightNode);
            else if (Operation == IRCompareOperation.LT) LessThan(LeftNode, RightNode);
            else if (Operation == IRCompareOperation.LE) LessEqual(LeftNode, RightNode);

            if (Type == null) Alarms.Add(AJAlarmFactory.CreateMCL0024(LeftNode.Type.FullName, RightNode.Type.FullName));

            if (RootNode.IsBuild) DBContext.Instance.Insert(this);

            return this;
        }

        public override IRExpression To()
        {
            IRBinaryExpr result = new IRBinaryExpr(Type.ToIR(), GetDebuggingData());

            if (Operation == IRCompareOperation.EQ) result.Operation = IRBinaryOperation.EQ;
            else if (Operation == IRCompareOperation.NE) result.Operation = IRBinaryOperation.NE;
            else if (Operation == IRCompareOperation.GT) result.Operation = IRBinaryOperation.GT;
            else if (Operation == IRCompareOperation.GE) result.Operation = IRBinaryOperation.GE;
            else if (Operation == IRCompareOperation.LT) result.Operation = IRBinaryOperation.LT;
            else if (Operation == IRCompareOperation.LE) result.Operation = IRBinaryOperation.LE;

            result.Left = LeftNode.To() as IRExpr;
            result.Right = RightNode.To() as IRExpr;

            result.OnlyTrue = AlwaysTrue;
            result.OnlyFalse = AlwaysFalse;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }


        public static CompareNode From(UseIdentNode node)
        {
            CompareNode result = new CompareNode(node.Ast, IRCompareOperation.EQ);
            var right = new BoolLiteralNode(true);

            result.Items.Add(node);
            result.Items.Add(right);

            return result;
        }


        public static CompareNode From(BoolLiteralNode node)
        {
            CompareNode result = new CompareNode(node.Ast, IRCompareOperation.EQ);
            var right = new BoolLiteralNode(true);

            result.Items.Add(node);
            result.Items.Add(right);

            return result;
        }


        public ExprNode EQ(ExprNode source, ExprNode target)
        {
            if (source.Type.IsArithmeticType()) ArithmeticEqual(source, target);
            if (source.Type.DataType == AJDataType.Bool) return BoolEqual(source, target);
            if (source.Type.DataType == AJDataType.String) return StringEqual(source, target);

            return this;
        }


        public ExprNode NotEQ(ExprNode source, ExprNode target)
        {
            EQ(source, target);
            if (ValueState == State.Fixed) Value = !(bool)Value;

            return this;
        }


        private ExprNode GreaterEqual(ExprNode source, ExprNode target)
        {
            if (source.Type.IsArithmeticType()) Compare(source, target, IRCompareOperation.GE);

            return this;
        }

        private ExprNode GreaterThan(ExprNode source, ExprNode target)
        {
            if (source.Type.IsArithmeticType()) Compare(source, target, IRCompareOperation.GT);

            return this;
        }

        private ExprNode LessEqual(ExprNode source, ExprNode target)
        {
            if (source.Type.IsArithmeticType()) Compare(source, target, IRCompareOperation.LE);

            return this;
        }

        private ExprNode LessThan(ExprNode source, ExprNode target)
        {
            if (source.Type.IsArithmeticType()) Compare(source, target, IRCompareOperation.LT);

            return this;
        }



        private ExprNode Compare(ExprNode source, ExprNode target, IRCompareOperation compare)
        {
            if (target.Type.IsArithmeticType())
            {
                Type = AJUtilities.CreateBooleanType(this);
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                if (compare == IRCompareOperation.GE) Value = (double)source.Value >= (double)target.Value;
                else if (compare == IRCompareOperation.GT) Value = (double)source.Value > (double)target.Value;
                else if (compare == IRCompareOperation.LE) Value = (double)source.Value <= (double)target.Value;
                else if (compare == IRCompareOperation.LT) Value = (int)source.Value < (double)target.Value;

                ValueState = State.Fixed;
            }

            return this;
        }



        private ExprNode ArithmeticEqual(ExprNode source, ExprNode target)
        {
            if (target.Type.IsArithmeticType())
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (double)source.Value == (double)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }


        private ExprNode BoolEqual(ExprNode source, ExprNode target)
        {
            if (target.Type.DataType == AJDataType.Bool)
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (bool)source.Value == (bool)target.Value;
                ValueState = State.Fixed;

                if ((bool)Value == true) AlwaysTrue = true;
                else AlwaysFalse = true;
            }

            return this;
        }


        private ExprNode StringEqual(ExprNode source, ExprNode target)
        {
            if (target.Type.DataType == AJDataType.String)
            {
                Type = target.Type;
                if (source.ValueState != State.Fixed || target.ValueState != State.Fixed) return this;

                Value = (string)source.Value == (string)target.Value;
                ValueState = State.Fixed;
            }

            return this;
        }
    }
}
