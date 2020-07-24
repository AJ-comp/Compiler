using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.LiteralNodes
{
    public abstract class LiteralNode : MiniCNode
    {
        public TokenData Token { get; protected set; }

        protected LiteralNode(AstSymbol node) : base(node)
        {
        }
    }
}
