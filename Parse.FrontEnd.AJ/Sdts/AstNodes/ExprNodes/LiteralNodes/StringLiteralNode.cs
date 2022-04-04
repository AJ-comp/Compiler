using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public class StringLiteralNode : LiteralNode
    {
        public StringLiteralNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            try
            {
                var node = Items[0].Compile(param) as TerminalNode;
                Token = node.Token;

                Value = Token.Input;
                ValueState = State.Fixed;
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

        public override IRExpression To()
        {
            var result = new IRLiteralExpr
            {
                Type = new TypeInfo(StdType.Char, 1),
                Value = null
            };

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
