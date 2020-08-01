using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.TypeNodes
{
    public enum DataType { Unknown, Void, Int }

    public abstract class DataTypeNode : MiniCNode
    {
        public abstract DataType DataType { get; }
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
