using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRCallExpr : IRExpr
    {
        public IRFunction Function { get; }
        public IEnumerable<IRExpr> Params => _exprs;

        public IRCallExpr(IRFunction function, IEnumerable<IRExpr> exprs) : base(function.ReturnType)
        {
            Function = function;
            _exprs.AddRange(exprs);
        }


        private List<IRExpr> _exprs = new List<IRExpr>();
    }
}
