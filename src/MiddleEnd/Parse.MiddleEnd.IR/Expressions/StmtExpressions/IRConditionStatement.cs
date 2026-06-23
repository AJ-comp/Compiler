using Parse.MiddleEnd.IR.Expressions.ExprExpressions;

namespace Parse.MiddleEnd.IR.Expressions.StmtExpressions
{
    public class IRConditionStatement : IRStatement
    {
        public IRBinaryExpr Condition { get; set; }

        public IRStatement TrueStatement { get; set; }
        public IRStatement FalseStatement { get; set; }
    }
}
