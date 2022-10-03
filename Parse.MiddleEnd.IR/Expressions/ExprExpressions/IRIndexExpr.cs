using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRIndexExpr : IRExpr
    {
        public int Index { get; }

        public IRIndexExpr(IRType type) : base(type)
        {
        }
    }
}
