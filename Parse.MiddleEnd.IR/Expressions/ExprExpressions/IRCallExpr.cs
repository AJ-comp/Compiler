using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRCallExpr : IRExpr
    {
        public IRFunction Function { get; set; }

        public IRCallExpr(IRFunction function) : base(function.ReturnType)
        {
            Function = function;
        }
    }
}
