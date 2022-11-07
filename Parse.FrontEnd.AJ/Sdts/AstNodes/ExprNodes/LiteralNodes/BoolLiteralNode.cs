using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public sealed class BoolLiteralNode : LiteralNode
    {
        public BoolLiteralNode(AstSymbol node) : base(node)
        {
        }

        public BoolLiteralNode(bool value) : base(null)
        {
            Value = value;
            ValueState = State.Fixed;

            StubCode = true;
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            try
            {
                if(!StubCode)
                {
                    var node = Items[0].Compile(param) as TerminalNode;
                    Token = node.Token;

                    Value = System.Convert.ToBoolean(Token.Input);
                    ValueState = State.Fixed;
                }

                Type = AJUtilities.CreateBooleanType(this);
            }
            catch (Exception)
            {

            }
            finally
            {
                if (param.Build) DBContext.Instance.Insert(this);
            }

            return this;
        }

        public override IRExpression To() => new IRLiteralExpr(Type.ToIR(), Value);

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
