using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRSingleExpr : IRExpr
    {
        public IRSingleOperation Operation { get; set; }

        public IRExpr Expression { get; set; }


        public bool Compareable => Operation == IRSingleOperation.Not;
    }
}
