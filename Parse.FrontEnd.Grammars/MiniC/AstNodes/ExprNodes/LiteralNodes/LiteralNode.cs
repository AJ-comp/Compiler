namespace Parse.FrontEnd.Grammars.MiniC.AstNodes.ExprNodes.LiteralNodes
{
    public abstract class LiteralNode : ExprNode
    {
        public TokenData Token { get; }

        protected LiteralNode(TokenData token)
        {
            Token = token;
        }
    }
}
