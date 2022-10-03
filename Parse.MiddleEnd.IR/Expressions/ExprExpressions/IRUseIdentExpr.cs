using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRUseIdentExpr : IRExpr
    {
        public string Name { get; set; }


        public IRUseIdentExpr(IRType type) : base(type)
        {
        }
    }
}
