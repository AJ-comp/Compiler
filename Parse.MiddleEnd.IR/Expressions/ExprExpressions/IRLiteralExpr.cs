using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRLiteralExpr : IRExpr
    {
        public object Value { get; set; }

        public IRLiteralExpr(IRType type) : base(type)
        {
        }

        public IRLiteralExpr(IRType type, object value) : base(type)
        {
            Value = value;
        }
    }
}
