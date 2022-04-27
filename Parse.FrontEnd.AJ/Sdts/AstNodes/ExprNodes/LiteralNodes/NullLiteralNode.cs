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
            Type = new AJPreDefType(AJDataType.Null);
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

        public override IRExpression To()
        {
            var result = new IRLiteralExpr
            {
                Type = new TypeInfo(StdType.Null, 0),
                Value = null
            };
            //            result.DebuggingInfo.ColumnIndex = Token.

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
