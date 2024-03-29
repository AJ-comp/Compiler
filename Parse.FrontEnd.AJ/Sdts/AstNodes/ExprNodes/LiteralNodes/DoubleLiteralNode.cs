﻿using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public class DoubleLiteralNode : LiteralNode
    {
        public DoubleLiteralNode(AstSymbol node) : base(node)
        {
        }

        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            try
            {
                var node = Items[0].Compile(param) as TerminalNode;
                Token = node.Token;

                Value = System.Convert.ToDouble(Token.Input);
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

        public override IRExpression To() => new IRLiteralExpr(Type.ToIR(), Value);

        public override IRExpression To(IRExpression from)
        {
            throw new NotImplementedException();
        }
    }
}
