using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.FrontEnd.MiniC.Sdts.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.MiniC.Sdts.Expressions
{
    /// ****************************************/
    /// <summary>
    /// while 문을 의미하는 표현식입니다.
    /// ex : while(CondExpr) TrueStatement
    /// </summary>
    /// ****************************************/
    public class WhileExpression : IRWhileStatement
    {
        public IRCompareOpExpr CondExpr { get; }
        public IRStatement TrueStatement { get; }

        public WhileExpression(IConditionStatement statement)
        {
            CondExpr = new ConditionExpression(statement.ConditionExpression);
            TrueStatement = StatementExpression.Create(statement.TrueStatement);
        }


        public override string ToString()
        {
            var result = string.Format("while ({0})", CondExpr.ToString());
            result += TrueStatement.ToString();

            return result;
        }
    }
}
