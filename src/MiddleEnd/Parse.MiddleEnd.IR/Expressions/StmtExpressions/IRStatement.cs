using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.StmtExpressions
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public abstract class IRStatement : IRExpression
    {
        private string GetDebuggerDisplay() => GetType().Name;
    }
}
