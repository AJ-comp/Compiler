using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.Expressions.ExprExpressions
{
    public class IRLiteralExpr : IRExpr
    {
        public TypeInfo Type { get; set; }
        public object Value { get; set; }

    }
}
