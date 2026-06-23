using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRReturnExpr : IRExpr
    {
        public IRExpr ReturnExpr { get; set; }

        public IRReturnExpr(IRExpr returnExpr) : base(returnExpr.Type)
        {
            ReturnExpr = returnExpr;
        }

//        public override string ToString() => ReturnExpr.SourceCode;
    }
}
