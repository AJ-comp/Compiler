using Parse.FrontEnd.Ast;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.TypeNodes
{
    public class IntNode : DataTypeNode
    {
        public IntNode(AstSymbol node) : base(node)
        {
        }

        public override DataType DataType => DataType.Int;
    }
}
