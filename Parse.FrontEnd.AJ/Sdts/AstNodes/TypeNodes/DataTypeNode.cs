﻿using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public abstract class DataTypeNode : AJNode
    {
        public abstract AJDataType Type { get; }
        public TokenData DataTypeToken { get; protected set; }
        public abstract uint Size { get; }
        public abstract string Name { get; }

        protected DataTypeNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Compile(CompileParameter param)
        {
            var node = Items[0].Compile(param) as TerminalNode;
            DataTypeToken = node.Token;

            return this;
        }
    }
}