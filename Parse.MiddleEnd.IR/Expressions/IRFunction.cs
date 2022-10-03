using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public int VarIndex { get; internal set; } = 0;
        public int CmpVarIndex { get; internal set; } = 0;

        public string IRName => Name.Replace(".", "_").Replace("~", "_");

        private string GetDebuggerDisplay()
        {
            string result = $"{ReturnType.Name} {Name}({Arguments.ItemsString()}";

            return result;
        }
    }
}
