using Parse.FrontEnd.Ast;
using Parse.Types;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.TypeNodes
{
    public class AddressNode : DataTypeNode
    {
        // This type can be changed according to system
        public override StdType DataType => StdType.Int;

        public AddressNode(AstSymbol node) : base(node)
        {
        }
    }
}
