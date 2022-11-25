using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.StmtExpressions
{
    public class IRExprStatement : IRStatement
    {
        public IRExpr Expr { get; internal set; }

        public IRExprStatement(IRExpr expr)
        {
            expr.Parent = this;
            Expr = expr;
        }
    }
}
