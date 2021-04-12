using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.MiniC.Sdts.Expressions.ExprExpressions
{
    /// *******************************************/
    /// <summary>
    /// double 타입 리터럴 문자열에 대한 표현식입니다.
    /// 리터럴 문자열은 해당 문자열의 값이 
    /// 적절하게 변환되어 Value에 할당됩니다.
    /// </summary>
    /// *******************************************/
    public class LiteralDoubleExpression : ExprExpression, IRDoubleLiteralExpr
    {
        public double Value { get; }

        public LiteralDoubleExpression(IRDoubleLiteralExpr expr) : base(expr)
        {
            Value = expr.Value;
            Result = new DoubleConstant(Value);
        }

        public LiteralDoubleExpression(double value)
        {
            Value = value;
            Result = new DoubleConstant(value);
        }
    }
}
