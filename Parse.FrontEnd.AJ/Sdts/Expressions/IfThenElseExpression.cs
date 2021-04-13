using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.Expressions
{
    /// ****************************************/
    /// <summary>
    /// if else 문을 의미하는 표현식입니다.
    /// ex : 
    /// if(CondExpr) TrueStatement
    /// else FalseStatement
    /// </summary>
    /// ****************************************/
    public class IfThenElseExpression : AJExpression, IRIFStatement
    {
        public IRCompareOpExpr CondExpr { get; }
        public IRStatement TrueStatement { get; }
        public IRStatement FalseStatement { get; }

        public IfThenElseExpression(IConditionStatement statement)
        {
            CondExpr = new ConditionExpression(statement.ConditionExpression);
            TrueStatement = StatementExpression.Create(statement.TrueStatement);
            FalseStatement = StatementExpression.Create(statement.FalseStatement);
        }

        public override string ToString()
        {
            var result = string.Format("if ({0})", CondExpr.ToString());
            result += TrueStatement.ToString();
            result += "else" + Environment.NewLine;
            result += FalseStatement.ToString();

            return result;
        }
    }
}
