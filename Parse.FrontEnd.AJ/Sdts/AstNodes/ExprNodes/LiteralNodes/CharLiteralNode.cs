﻿using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.Types;
using System;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public class CharLiteralNode : LiteralNode
    {
        public char Value => (char)Result.Value;

        public CharLiteralNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            try
            {
                var node = Items[0].Compile(param) as TerminalNode;
                Token = node.Token;

                Result = new ConstantAJ(System.Convert.ToChar(Token.Input));
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
            var result = new IRLiteralExpr();

            result.Type = new TypeInfo(StdType.Char, 0);
            result.Value = Value;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
