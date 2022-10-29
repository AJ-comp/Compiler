using Parse.MiddleEnd.IR.Datas;
using Parse.Types;

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

        public IRLiteralExpr(int value) : base(new IRType(StdType.Int, 0))
        {
            Value = value;
        }
        public IRLiteralExpr(double value) : base(new IRType(StdType.Double, 0))
        {
            Value = value;
        }


        public override string ToString() => Value.ToString();
    }
}
