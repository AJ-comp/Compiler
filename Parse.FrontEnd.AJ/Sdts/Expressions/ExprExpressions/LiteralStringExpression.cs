using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    /// ********************************************/
    /// <summary>
    /// string 타입 리터럴 문자열에 대한 표현식입니다.
    /// 리터럴 문자열은 해당 문자열의 값이 
    /// 적절하게 변환되어 Value에 할당됩니다.
    /// </summary>
    /// ********************************************/
    public class LiteralStringExpression : ExprExpression, IRStringLiteralExpr
    {
        public string Value { get; }

        public LiteralStringExpression(IRStringLiteralExpr expr) : base(expr)
        {
            Value = expr.Value;
            Result = new StringConstant(Value);
        }

        public LiteralStringExpression(string value)
        {
            Value = value;
            Result = new StringConstant(Value);
        }
    }
}
