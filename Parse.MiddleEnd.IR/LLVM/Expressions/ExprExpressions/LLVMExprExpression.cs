using Parse.Types;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public abstract class LLVMExprExpression : LLVMDependencyExpression
    {
        protected LLVMExprExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }

        public IValue Result { get; protected set; }
    }
}
