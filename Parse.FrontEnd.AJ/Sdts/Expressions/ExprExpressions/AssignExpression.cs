using Parse.FrontEnd.AJ.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.FrontEnd.AJ.Sdts.Expressions.ExprExpressions
{
    /// ********************************************/
    /// <summary>
    /// 할당식을 의미하는 표현식입니다.
    /// ex : Var = Right
    /// </summary>
    /// ********************************************/
    public class AssignExpression : BinaryExpression, IRAssignOpExpr
    {
        public IRDeclareVar Var { get; }
        public IRExpr Right { get; }

        public AssignExpression(IAssignExpression expr)
        {
            Var = VariableExpression.Create(expr.Left);
            Right = Create(expr.Right);

            Result = expr.Result;
        }
    }
}
