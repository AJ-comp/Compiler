using Parse.FrontEnd.Ast;
using Parse.Types;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.TypeNodes
{
    public class IntNode : DataTypeNode
    {
        public IntNode(AstSymbol node, bool bSigned) : base(node)
        {
            _signed = bSigned;
        }

        public override StdType DataType => (_signed) ? StdType.UInt : StdType.Int;

        private bool _signed;
    }
}
