namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public abstract class LLVMFirstLayerExpression : LLVMExpression, ICanBeFirstLayer
    {
        protected LLVMFirstLayerExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }
    }
}
