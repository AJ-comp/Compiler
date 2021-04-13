using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    /// *******************************************/
    /// <summary>
    /// char 타입 리터럴 문자열에 대한 표현식입니다.
    /// 리터럴 문자열은 해당 문자열의 값이 
    /// 적절하게 변환되어 Value에 할당됩니다.
    /// </summary>
    /// *******************************************/
    public class LiteralCharExpression : ExprExpression, IRCharLiteralExpr
    {
        public char Value { get; }

        public LiteralCharExpression(IRCharLiteralExpr expr) : base(expr)
        {
            Value = expr.Value;
            Result = new ByteConstant((byte)Value);
        }

        public LiteralCharExpression(char value)
        {
            Value = value;
            Result = new ByteConstant((byte)Value);
        }
    }
}
