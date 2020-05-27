namespace Parse.FrontEnd.Grammars.MiniC.AstNodes.ExprNodes.LiteralNodes
{
    public class StringLiteralNode : LiteralNode
    {
        public string Value { get; }

        public StringLiteralNode(TokenData token, string value) : base(token)
        {
            Value = value;
        }
    }
}
