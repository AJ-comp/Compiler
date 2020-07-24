using Parse.MiddleEnd.IR.Datas;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public abstract class LLVMExpression : IRExpression
    {
        public static LLVMExpression FuncDef(IRFuncData funcData, LLVMBlockExpression blockExpression)
        {
            return new LLVMFuncDefExpression();
        }

        public static LLVMExpression Variable()
        {
            return new LLVMVariableExpression();
        }

        public abstract string GeneratedCode();
    }
}
