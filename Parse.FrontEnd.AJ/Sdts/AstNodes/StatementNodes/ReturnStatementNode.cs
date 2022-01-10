using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class ReturnStatementNode : StatementNode
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
        public override SdtsNode Compile(CompileParameter param)
        {
            Expr = Items[1].Compile(param) as ExprNode;

            return this;
        }

        public override IRExpression To()
        {
            var result = new IRReturnExpr();

            result.ReturnExpr = Expr.To() as IRExpr;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
