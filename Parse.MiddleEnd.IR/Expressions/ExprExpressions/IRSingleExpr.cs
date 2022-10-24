using AJ.Common.Helpers;
using Parse.MiddleEnd.IR.Datas;
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

        public IRSingleExpr(IRType type) : base(type)
        {
        }


        public override string ToString()
        {
            string result = string.Empty;

            if (Operation == IRSingleOperation.PostInc) result += $"{Items[0]}++";
            else if (Operation == IRSingleOperation.PostDec) result += $"{Items[0]}--";
            else result += $"{Operation.ToDescription()}{Items[0]}";

            return result;
        }
    }
}
