using Parse.FrontEnd.Ast;
using Parse.FrontEnd.MiniC.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.MiniC.Sdts.Datas;

namespace Parse.FrontEnd.MiniC.Sdts.AstNodes.StatementNodes
{
    public class ReturnStatementNode : StatementNode, IExprTypeStatement
    {
        /// <summary>
        /// return expression
        /// </summary>
        public ExprNode Expr { get; private set; }

        public ReturnStatementNode(AstSymbol node) : base(node)
        {
        }


        // [0] : return (AstTerminal)
        // [1] : ExpSt (AstNonTerminal)
        public override SdtsNode Build(SdtsParams param)
        {
            Expr = Items[1].Build(param) as ExprNode;

            return this;
        }
    }
}
