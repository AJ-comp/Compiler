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
            Type = new AJPreDefType(AJDataType.Bool);
        }

        public BoolLiteralNode(bool value) : base(null)
        {
            Value = value;
            ValueState = State.Fixed;

            Type = new AJPreDefType(AJDataType.Bool);
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            try
            {
                var node = Items[0].Compile(param) as TerminalNode;
                Token = node.Token;

                Value = System.Convert.ToBoolean(Token.Input);
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
                Type = new TypeInfo(StdType.Bit, 0),
                Value = Value
            };

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
