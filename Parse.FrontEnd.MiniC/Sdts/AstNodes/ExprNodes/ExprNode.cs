﻿using Parse.FrontEnd.Ast;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes
{
    public abstract class ExprNode : MiniCNode
    {
        public IConstant Result { get; protected set; }
        public bool AlwaysTrue { get; } = false;

        protected ExprNode(AstSymbol node) : base(node)
        {
        }
    }
}
