using Parse.FrontEnd.Ast;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public class IntLiteralNode : LiteralNode
    {
        public IntLiteralData LiteralData
        {
            get
            {
                if (Token.Kind.TokenType == MiniCGrammar.HexNumber.TokenType)
                    return new IntLiteralData(System.Convert.ToInt32(Token.Input, 16), Token);
                else if(Token.Kind.TokenType == MiniCGrammar.BinNumber.TokenType)
                    return new IntLiteralData(System.Convert.ToInt32(Token.Input, 2), Token);

                return new IntLiteralData(System.Convert.ToInt32(Token.Input), Token);
            }
        }

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
