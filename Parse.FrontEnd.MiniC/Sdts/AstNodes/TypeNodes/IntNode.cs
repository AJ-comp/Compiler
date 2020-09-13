using Parse.FrontEnd.Ast;

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
