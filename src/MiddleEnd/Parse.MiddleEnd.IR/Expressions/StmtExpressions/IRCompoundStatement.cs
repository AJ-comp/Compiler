using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.StmtExpressions
{
    public class IRCompoundStatement : IRStatement
    {
        public IEnumerable<IRVariable> LocalVars
        {
            get
            {
                List<IRVariable> result = new List<IRVariable>();

                foreach (var statement in Expressions)
                {
                    if (!(statement is IRDclVarStatement)) continue;

                    result.AddRange((statement as IRDclVarStatement).Vars);
                }

                return result;
            }
        }

        // statement or expr (the expr statement does not exist it replaces to the expr)
        public List<IRStatement> Expressions { get; } = new List<IRStatement>();
    }
}
