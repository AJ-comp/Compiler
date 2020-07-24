using Parse.FrontEnd.Ast;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes.StatementNodes
{
    public class ExprStatementNode : StatementNode
    {
        public ExprStatementNode(AstSymbol node) : base(node)
        {
        }


        // format summary
        // (AddAssign | SubAssign | MulAssign | DivAssign | ...) ;
        public override SdtsNode Build(SdtsParams param)
        {
            var node = Items[0].Build(param);

            return this;
        }
    }
}
