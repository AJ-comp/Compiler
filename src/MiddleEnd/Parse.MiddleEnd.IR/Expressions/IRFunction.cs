using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class IRFunction : IRExpression
    {
        public string Name { get; set; }
        public IRType ReturnType { get; set; }
        public List<IRVariable> Arguments { get; } = new List<IRVariable>();
        public IRCompoundStatement Statement { get; set; }

        private string GetDebuggerDisplay()
        {
            string result = $"{ReturnType.Name} {Name}({Arguments.ItemsString()}";

            return result;
        }
    }
}
