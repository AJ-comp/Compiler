using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public class CharLiteralNode : LiteralNode
    {
        public char Value { get; }

        public CharLiteralNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            throw new System.NotImplementedException();
        }
    }
}
