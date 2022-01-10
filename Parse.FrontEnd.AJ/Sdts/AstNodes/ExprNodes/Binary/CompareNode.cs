using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Single;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public sealed class CompareNode : BinaryExprNode
    {
        public IRCompareOperation Operation { get; }

        public CompareNode(AstSymbol node, IRCompareOperation operation) : base(node)
        {
            Operation = operation;
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            try
            {
                if (LeftNode.Result == null) return this;
                if (RightNode.Result == null) return this;

                else if (Operation == IRCompareOperation.EQ) Result = LeftNode.Result.EQ(RightNode.Result);
                else if (Operation == IRCompareOperation.NE) Result = LeftNode.Result.NotEQ(RightNode.Result);
                else if (Operation == IRCompareOperation.GT) Result = LeftNode.Result > RightNode.Result;
                else if (Operation == IRCompareOperation.GE) Result = LeftNode.Result >= RightNode.Result;
                else if (Operation == IRCompareOperation.LT) Result = LeftNode.Result < RightNode.Result;
                else if (Operation == IRCompareOperation.LE) Result = LeftNode.Result <= RightNode.Result;
            }
            catch (Exception)
            {
                AJAlarmFactory.CreateMCL0024(LeftNode.Result.Type.Name, RightNode.Result.Type.Name);
            }
            finally
            {
                if (param.Build) DBContext.Instance.Insert(this);
            }

            return this;
        }

        public override IRExpression To()
        {
            IRBinaryExpr result = new IRBinaryExpr();

            if (Operation == IRCompareOperation.EQ) result.Operation = IRBinaryOperation.EQ;
            else if (Operation == IRCompareOperation.NE) result.Operation = IRBinaryOperation.NE;
            else if (Operation == IRCompareOperation.GT) result.Operation = IRBinaryOperation.GT;
            else if (Operation == IRCompareOperation.GE) result.Operation = IRBinaryOperation.GE;
            else if (Operation == IRCompareOperation.LT) result.Operation = IRBinaryOperation.LT;
            else if (Operation == IRCompareOperation.LE) result.Operation = IRBinaryOperation.LE;

            result.Left = LeftNode.To() as IRExpr;
            result.Right = RightNode.To() as IRExpr;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }



        public static CompareNode From(SLogicalNode node)
        {
            if (node.Operation != IRLogicalOperation.Not) return null;
            if (node.ExprNode.Result.Type.DataType != Data.AJDataType.Bool) return null;

            CompareNode result = new CompareNode(null, IRCompareOperation.EQ);
            var right = new BoolLiteralNode(null);
            right.Result = new Data.ConstantAJ(false);

            result.Items.Add(node.ExprNode);
            result.Items.Add(right);

            return result;
        }


        public static CompareNode From(UseIdentNode node)
        {
            CompareNode result = new CompareNode(null, IRCompareOperation.EQ);
            var right = new BoolLiteralNode(null);
            right.Result = new Data.ConstantAJ(true);

            result.Items.Add(node);
            result.Items.Add(right);

            return result;
        }


        public static CompareNode From(BoolLiteralNode node)
        {
            CompareNode result = new CompareNode(null, IRCompareOperation.EQ);
            var right = new BoolLiteralNode(null);
            right.Result = new Data.ConstantAJ(true);

            result.Items.Add(node);
            result.Items.Add(right);

            return result;
        }
    }
}
