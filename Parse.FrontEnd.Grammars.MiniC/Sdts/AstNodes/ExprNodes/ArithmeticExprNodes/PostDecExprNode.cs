﻿using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.ArithmeticExprNodes
{
    public class PostDecExprNode : IncDecExprNode
    {
        public PostDecExprNode(AstSymbol node) : base(node, "--")
        {
        }
    }
}