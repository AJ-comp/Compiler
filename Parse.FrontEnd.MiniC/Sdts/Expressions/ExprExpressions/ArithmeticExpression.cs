using Parse.FrontEnd.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.Types.ConstantTypes;

namespace Parse.FrontEnd.MiniC.Sdts.Expressions.ExprExpressions
{
    /// ********************************************/
    /// <summary>
    /// 산술 연산식을 의미하는 표현식입니다.
    /// 예를 들어 Operation 값이 Add 인 경우 : Left + Right
    /// </summary>
    /// ********************************************/
    public class ArithmeticExpression : BinaryExpression, IRArithmeticOpExpr
    {
        /* for interface ******************************/
        public IROperation Operation { get; private set; }
        public IRExpr Left { get; private set; }
        public IRExpr Right { get; private set; }
        /*****************************************/


        public bool Virtual { get; } = false;

        public ArithmeticExpression(IArithmeticExpression expr) : base()
        {
            Operation = expr.Operation;
            Left = Create(expr.Left);
            Right = Create(expr.Right);
        }

        public ArithmeticExpression(ExprExpression left, ExprExpression right, IROperation operation)
        {
            Left = left;
            Right = right;
            Operation = operation;

            Result = new UnknownConstant();
        }

        public static ArithmeticExpression Create(ExprExpression left, ExprExpression right, IROperation operation)
        {
            var result = new ArithmeticExpression();

            result.Left = left;
            result.Right = right;
            result.Operation = operation;

            return result;
        }


        private ArithmeticExpression()
        {
        }
    }
}
