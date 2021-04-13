using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    /// ***********************************************/
    /// <summary>
    /// integer (8, 16, 32) 타입 리터럴 문자열에 대한 표현식입니다.
    /// 리터럴 문자열은 해당 문자열의 값이 
    /// 적절하게 변환되어 Value에 할당됩니다.
    /// </summary>
    /// ***********************************************/
    public class LiteralIntegerExpression : ExprExpression, IRInt32LiteralExpr
    {
        public bool Signed { get; }
        public int Value { get; }

        public LiteralIntegerExpression(IRInt32LiteralExpr expr) : base(expr)
        {
            Signed = expr.Signed;
            Value = expr.Value;
        }

        public LiteralIntegerExpression(uint value)
        {
            Signed = false;
            Value = (int)value;

            Result = new IntConstant(Value);
        }

        public LiteralIntegerExpression(int value)
        {
            Signed = true;
            Value = value;

            Result = new IntConstant(Value);
        }
    }
}
