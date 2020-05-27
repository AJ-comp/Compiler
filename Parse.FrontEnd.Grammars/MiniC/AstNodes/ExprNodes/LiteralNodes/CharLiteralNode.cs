using System.Security.Cryptography.X509Certificates;

namespace Parse.FrontEnd.Grammars.MiniC.AstNodes.ExprNodes.LiteralNodes
{
    public class CharLiteralNode : LiteralNode
    {
        public char Value { get; }

        public CharLiteralNode(TokenData token, char value) : base(token)
        {
            Value = value;
        }
    }
}
