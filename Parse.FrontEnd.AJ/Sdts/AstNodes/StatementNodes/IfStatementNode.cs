using Parse.FrontEnd.Ast;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;

namespace Parse.FrontEnd.AJ.Sdts.AstNodes.StatementNodes
{
    public class IfStatementNode : CondStatementNode
    {
        public IfStatementNode(AstSymbol node) : base(node)
        {
        }


        public override IRExpression To()
        {
            var result = new IRConditionStatement();

            if (CompareCondition == null) throw new System.Exception();

            result.Condition = CompareCondition.To() as IRBinaryExpr;
            result.TrueStatement = TrueStatement.To() as IRStatement;
            result.FalseStatement = FalseStatement?.To() as IRStatement;

            result.Condition.Parent = result;
            result.TrueStatement.Parent = result;
            if(result.FalseStatement != null) result.FalseStatement.Parent = result;

            return result;
        }

        public override IRExpression To(IRExpression from)
        {
            throw new System.NotImplementedException();
        }
    }
}
