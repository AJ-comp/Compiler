using Parse.FrontEnd.Ast;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.TypeNodes
{
    public class SystemNode : DataTypeNode
    {
        public override DataType DataType => DataType.Int; // not fixed

        public SystemNode(AstSymbol node) : base(node)
        {
        }
    }
}
