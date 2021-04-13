using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Sdts.Expressions
{
    public class StatementExpression
    {
        public static IRStatement Create(IStmtExpression stmtExpression)
        {
            if (stmtExpression is ICompoundStmtExpression)
                return new BlockExpression(stmtExpression as ICompoundStmtExpression);
            else if (stmtExpression is IConditionStatement)
                return new IfThenExpression(stmtExpression as IConditionStatement);

            throw new Exception();
        }
    }
}
