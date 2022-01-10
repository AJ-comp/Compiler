using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRCallExpr : IRExpr
    {
        public IRFunction Function { get; set; }
    }
}
