using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public class StringLiteralNode : LiteralNode
    {
        public string Value { get; }

        public StringLiteralNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            throw new System.NotImplementedException();
        }
    }
}
