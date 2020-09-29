using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.TypeNodes
{
    public class AddressNode : DataTypeNode
    {
        public AddressNode(AstSymbol node) : base(node)
        {
        }

        public override DataType DataType => DataType.Address;
    }
}
