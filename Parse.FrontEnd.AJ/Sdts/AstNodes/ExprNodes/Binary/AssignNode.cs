using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public sealed class AssignNode : AssignExprNode
    {
        public AssignNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            base.Compile(param);

            try
            {
                if (!IsCanParsing) return this;

                LeftNode.Result.Assign(RightNode.Result);
            }
            catch (Exception)
            {
                Alarms.Add(AJAlarmFactory.CreateMCL0023(LeftNode.Result.Type.Name,
                                                                                RightNode.Result.Type.Name, "="));
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

            result.Operation = IRBinaryOperation.Assign;
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
