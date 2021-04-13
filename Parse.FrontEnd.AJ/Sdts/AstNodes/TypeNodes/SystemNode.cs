using Parse.FrontEnd.Ast;
using Parse.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public class SystemNode : DataTypeNode
    {
        // This type can be changed according to system.
        public override StdType DataType => StdType.Int;

        public SystemNode(AstSymbol node) : base(node)
        {
        }
    }
}
