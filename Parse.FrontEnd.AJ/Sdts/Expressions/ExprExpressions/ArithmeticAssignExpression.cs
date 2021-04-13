using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    /// ********************************************/
    /// <summary>
    /// 산술연산 후 할당식을 의미하는 표현식입니다.
    /// 예를 들어 Operation 값이 Add 인 경우 : Var += Right
    /// </summary>
    /// ********************************************/
    public class ArithmeticAssignExpression : AssignExpression, IRArithmeticAssignOpExpr
    {
        public IROperation Operation { get; }

        public ArithmeticAssignExpression(IArithmeticAssignExpression node) : base(node)
        {
            Operation = node.Operation;
        }
    }
}
