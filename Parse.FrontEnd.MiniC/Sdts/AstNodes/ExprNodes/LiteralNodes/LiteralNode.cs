﻿using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public abstract class LiteralNode : ExprNode
    {
        public TokenData Token { get; protected set; }

        protected LiteralNode(AstSymbol node) : base(node)
        {
        }
    }
}
