﻿using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public abstract class UserDefTypeNode : DataTypeNode
    {
        public abstract string FullName { get; }

        protected UserDefTypeNode(AstSymbol node) : base(node)
        {
        }
    }
}