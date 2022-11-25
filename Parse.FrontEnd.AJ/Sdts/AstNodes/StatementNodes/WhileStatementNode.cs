using Parse.FrontEnd.AJ.Data;
using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class WhileStatementNode : RoopStatementNode
    {
        public WhileStatementNode(AstSymbol node) : base(node)
        {
        }

        public override IRExpression To()
        {
            var result = new IRRepeatStatement();

            result.Condition = CompareCondition.To() as IRBinaryExpr;
            result.TrueStatement = TrueStatement.To() as IRStatement;

            result.Condition.Parent = result;
            result.TrueStatement.Parent = result;
            result.IncludeBreak = IncludeBreak;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
