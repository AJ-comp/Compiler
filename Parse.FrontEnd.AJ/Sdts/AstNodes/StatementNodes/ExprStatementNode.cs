using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class ExprStatementNode : StatementNode
    {
        public ExprNode Expr { get; private set; }

        public ExprStatementNode(AstSymbol node) : base(node)
        {
        }


        // format summary
        // (AddAssign | SubAssign | MulAssign | DivAssign | ...).Optional ;
        public override SdtsNode Compile(CompileParameter param)
        {
            if (Items.Count > 0)
            {
                Expr = Items[0].Compile(param) as ExprNode;
            }

            return this;
        }

        public override IRExpression To()
        {
            throw new System.NotImplementedException();
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
