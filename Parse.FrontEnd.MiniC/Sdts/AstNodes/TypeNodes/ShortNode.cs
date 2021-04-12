using Parse.FrontEnd.Ast;
using Parse.Types;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.TypeNodes
{
    public class ShortNode : DataTypeNode
    {
        public override StdType DataType => _signed ? StdType.UShort : StdType.Short;

        public ShortNode(AstSymbol node, bool signed) : base(node)
        {
            _signed = signed;
        }

        private bool _signed;
    }
}
