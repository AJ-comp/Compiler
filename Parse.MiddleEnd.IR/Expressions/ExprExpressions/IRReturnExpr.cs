using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRReturnExpr : IRExpr
    {
        public IRExpr ReturnExpr { get; set; }
    }
}
