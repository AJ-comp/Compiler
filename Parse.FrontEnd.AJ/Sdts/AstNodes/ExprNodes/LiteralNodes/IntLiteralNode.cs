using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes.LiteralNodes
{
    public class IntLiteralNode : LiteralNode, IRInt32LiteralExpr
    {
        public bool Signed { get; }
        public int Value => (int)Result.Value;

        public IntLiteralNode(AstSymbol node) : base(node)
        {
        }

        public IntLiteralNode(IntConstant value) : base(null)
        {
            Result = value;
        }

        public override SdtsNode Build(SdtsParams param)
        {
            var node = Items[0].Build(param) as TerminalNode;
            Token = node.Token;

            int value = 0;
            if (Token.Kind.TokenType == AJGrammar.HexNumber.TokenType)
                value = System.Convert.ToInt32(Token.Input, 16);
            else if (Token.Kind.TokenType == AJGrammar.BinNumber.TokenType)
                value = System.Convert.ToInt32(Token.Input, 2);
            else value = System.Convert.ToInt32(Token.Input);

            Result = new IntConstant(value);

            return this;
        }
    }
}
