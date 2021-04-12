using Parse.MiddleEnd.IR.Interfaces;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public abstract class LLVMStmtExpression : LLVMDependencyExpression
    {
        protected LLVMStmtExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }

        public static LLVMStmtExpression Create(IRStatement statement, LLVMSSATable ssaTable)
        {
            if (statement is IRCompoundStatement)
                return new LLVMCompoundStmtExpression(statement as IRCompoundStatement, ssaTable);
            else if (statement is IRIFStatement)
                return new LLVMIFExpression(statement as IRIFStatement, ssaTable);
            else if (statement is IRIFElseStatement)
                return new LLVMIFExpression(statement as IRIFElseStatement, ssaTable);
            else if (statement is IRWhileStatement)
                return new LLVMWhileExpression(statement as IRWhileStatement, ssaTable);
            else if (statement is IRExprStatement)    // artimetic, assign, artimetic assign, inc dec, call, return, compare, bit etc
                return new LLVMExprStmtExpression(statement as IRExprStatement, ssaTable);

            return null;
        }
    }
}
