using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRUseIdentExpr : IRExpr
    {
        public IRVariable Variable { get; }
        public string Name => Variable.Name;


        public IRUseIdentExpr(IRVariable variable) : base(variable.Type)
        {
            Variable = variable;
        }


        public override string ToString() => Name;
    }
}
