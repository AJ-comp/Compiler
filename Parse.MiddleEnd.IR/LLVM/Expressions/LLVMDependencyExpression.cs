namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public abstract class LLVMDependencyExpression : LLVMExpression
    {
        protected LLVMDependencyExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }
    }
}
