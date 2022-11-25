using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRExpr : IRExpression
    {
        public IRType Type { get; set; }

        public IRExpr(IRType type) : this(type, DebuggingData.CreateDummy())
        {
        }

        public IRExpr(IRType type, DebuggingData debuggingData) : base(debuggingData)
        {
            Type = type;
        }
    }
}
