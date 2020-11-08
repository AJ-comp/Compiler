using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public class CalculationInfo
    {
        public IRVar Left { get; }
        public LLVMExprExpression Right { get; }

        public CalculationInfo(IRVar left, LLVMExprExpression right)
        {
            Left = left;
            Right = right;
        }
    }
}
