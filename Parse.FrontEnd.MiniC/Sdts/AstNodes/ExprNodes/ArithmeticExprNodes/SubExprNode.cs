﻿using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes;
using System;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class SubExprNode : ArithmeticExprNode
    {
        public SubExprNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            base.Build(param);

            try
            {
                if (!(Left is LiteralNode) || !(Right is LiteralNode)) return this;

                if (Left is IntLiteralNode)
                    (Left as IntLiteralNode).LiteralData.Sub(Right.Result);
            }
            catch (Exception ex)
            {

            }

            return this;
        }
    }
}
