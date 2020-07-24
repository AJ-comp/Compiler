using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.TypeNodes
{
    public class VoidNode : DataTypeNode
    {
        public VoidNode(AstSymbol node) : base(node)
        {
        }

        public override DataType DataType => DataType.Void;
    }
}
