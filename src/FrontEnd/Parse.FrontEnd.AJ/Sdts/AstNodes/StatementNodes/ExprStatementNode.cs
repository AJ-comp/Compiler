using Parse.FrontEnd.AJ.Properties;
using Parse.FrontEnd.AJ.Sdts.AstNodes.ExprNodes;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;

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
        protected override SdtsNode CompileLogic(CompileParameter param)
        {
            base.CompileLogic(param);

            if (Items.Count > 0)
            {
                Expr = Items[0].Compile(param) as ExprNode;
                if (!(Expr is ICanbeStatement)) AddCantStatementAlarm();
            }

            return this;
        }

        public override IRExpression To() => new IRExprStatement(Expr.To() as IRExpr);

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }



        private void AddCantStatementAlarm()
        {
            Alarms.Add(new MeaningErrInfo(Expr.AllTokens, nameof(AlarmCodes.AJ0038), AlarmCodes.AJ0038));
        }
    }
}
