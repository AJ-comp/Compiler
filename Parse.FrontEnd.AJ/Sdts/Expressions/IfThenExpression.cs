using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.Expressions
{
    /// ****************************************/
    /// <summary>
    /// if 문을 의미하는 표현식입니다.
    /// ex : if(CondExpr) TrueStatement
    /// </summary>
    /// ****************************************/
    public class IfThenExpression : IRIFStatement
    {
        public IRCompareOpExpr CondExpr { get; }
        public IRStatement TrueStatement { get; }

        public IfThenExpression(IConditionStatement ifStatement)
        {
            CondExpr = new ConditionExpression(ifStatement.ConditionExpression);
            TrueStatement = StatementExpression.Create(ifStatement.TrueStatement);
        }

        public override string ToString()
        {
            var result = string.Format("if ({0})", CondExpr.ToString());
            result += TrueStatement.ToString();

            return result;
        }
    }
}
