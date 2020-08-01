using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public class IntLiteralNode : LiteralNode
    {
        public IntLiteralData LiteralData => new IntLiteralData(System.Convert.ToInt32(Token.Input), Token);

        public IntLiteralNode(AstSymbol node) : base(node)
        {
        }

        public override SdtsNode Build(SdtsParams param)
        {
            var node = Items[0].Build(param) as TerminalNode;
            Token = node.Token;
            Result = LiteralData;

            return this;
        }
    }
}
