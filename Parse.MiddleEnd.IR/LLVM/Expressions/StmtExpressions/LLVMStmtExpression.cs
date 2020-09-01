namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public abstract class LLVMStmtExpression : LLVMDependencyExpression
    {
        protected LLVMStmtExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }
    }
}
