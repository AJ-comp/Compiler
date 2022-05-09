using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions
{
    public class IRFunction : IRExpression
    {
        public string Name { get; }
        public IRType ReturnType { get; }
        public List<IRVariable> Arguments { get; } = new List<IRVariable>();
        public IRCompoundStatement Statement { get; set; }
    }
}
