using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.LiteralNodes
{
    public class IntLiteralNode : LiteralNode
    {
        public int Value => System.Convert.ToInt32(Token.Input);

        public IntLiteralNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            var node = Items[0].Build(param) as TerminalNode;
            Token = node.Token;

            return this;
        }
    }
}
