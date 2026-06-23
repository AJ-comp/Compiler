using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.StmtExpressions
{
    public class IRRepeatStatement : IRStatement
    {
        public IRBinaryExpr Condition { get; set; }

        public IRStatement TrueStatement { get; set; }
        public bool IncludeBreak { get; set; } = false;
    }
}
