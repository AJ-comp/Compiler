using Parse.FrontEnd.Ast;
using Parse.Types;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.TypeNodes
{
    public abstract class DataTypeNode : MiniCNode
    {
        public abstract StdType DataType { get; }
        public TokenData DataTypeToken { get; protected set; }

        protected DataTypeNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            var node = Items[0].Build(param) as TerminalNode;
            DataTypeToken = node.Token;

            return this;
        }
    }
}
