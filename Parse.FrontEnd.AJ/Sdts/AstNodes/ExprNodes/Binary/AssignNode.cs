using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.Binary
{
    public sealed class AssignNode : BinaryExprNode, IAssignable
    {
        public AssignNode(AstSymbol node) : base(node)
        {
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);
            CheckAssignable();

            if (!IsCanParsing) return this;
            var result = Assign(LeftNode, RightNode);
            Type = result?.Type;

            if (result == null) Alarms.Add(AJAlarmFactory.CreateMCL0023(this, "="));
            if (RootNode.IsBuild) DBContext.Instance.Insert(this);

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRBinaryExpr(Type.ToIR());

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
