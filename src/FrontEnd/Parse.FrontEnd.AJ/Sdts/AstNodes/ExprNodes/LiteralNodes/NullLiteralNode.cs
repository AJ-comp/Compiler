using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public class NullLiteralNode : LiteralNode
    {
        public NullLiteralNode(AstSymbol node) : base(node)
        {
            var dataType = AJDataType.Null;
            Type = new AJPreDefType(dataType, GetDefineForPreDefType(dataType));
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            var node = Items[0].Compile(param) as TerminalNode;
            Token = node.Token;

            try
            {
                Value = null;
                ValueState = State.Fixed;
            }
            catch(Exception)
            {
            }
            finally
            {
                if (param.Build) DBContext.Instance.Insert(this);
            }

            return this;
        }

        public override IRExpression To() => new IRLiteralExpr(Type.ToIR(), null);

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
