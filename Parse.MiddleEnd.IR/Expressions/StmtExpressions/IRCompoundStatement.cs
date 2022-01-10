using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.StmtExpressions
{
    public class IRCompoundStatement : IRStatement
    {
        public List<IRVariable> LocalVars { get; } = new List<IRVariable>();

        // statement or expr (the expr statement does not exist it replaces to the expr)
        public List<IRExpression> Expressions { get; } = new List<IRExpression>();
    }
}
