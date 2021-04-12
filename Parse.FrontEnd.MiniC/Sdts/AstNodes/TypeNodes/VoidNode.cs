using Parse.FrontEnd.Ast;
using Parse.Types;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.TypeNodes
{
    public class VoidNode : DataTypeNode
    {
        public override StdType DataType => StdType.Void;

        public VoidNode(AstSymbol node) : base(node)
        {
        }
    }
}
