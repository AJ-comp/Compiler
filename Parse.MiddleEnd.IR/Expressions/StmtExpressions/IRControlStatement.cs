using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Expressions.StmtExpressions
{
    public enum IRControlType
    {
        Unknown,
        Continue,
        Break,
    }

    public class IRControlStatement : IRStatement
    {
        public IRControlStatement(IRControlType controlType)
        {
            ControlType = controlType;
        }

        public IRControlType ControlType { get; } = IRControlType.Unknown;
    }
}
