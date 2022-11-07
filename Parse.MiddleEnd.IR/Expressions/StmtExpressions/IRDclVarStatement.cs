using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.StmtExpressions
{
    public class IRDclVarStatement : IRStatement
    {
        public IRDclVarStatement(IEnumerable<IRVariable> vars)
        {
            Vars.AddRange(vars);
        }

        internal List<IRVariable> Vars { get; } = new List<IRVariable>();
    }
}
