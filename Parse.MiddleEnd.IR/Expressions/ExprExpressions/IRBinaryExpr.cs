using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRBinaryExpr : IRExpr
    {
        public IRBinaryOperation Operation { get; set; }

        public IRExpr Left { get; set; }
        public IRExpr Right { get; set; }


        public bool Compareable
        {
            get
            {
                if (Operation == IRBinaryOperation.EQ) return true;
                if (Operation == IRBinaryOperation.NE) return true;
                if (Operation == IRBinaryOperation.GE) return true;
                if (Operation == IRBinaryOperation.GT) return true;
                if (Operation == IRBinaryOperation.LE) return true;
                if (Operation == IRBinaryOperation.LT) return true;

                return false;
            }
        }
    }
}
