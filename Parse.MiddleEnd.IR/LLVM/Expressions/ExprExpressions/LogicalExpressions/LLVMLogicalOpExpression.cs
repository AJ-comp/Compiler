using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.LogicalExpressions
{
    public abstract class LLVMLogicalOpExpression : LLVMBinOpExpression
    {
        public IRCondition Condition { get; }


        public LLVMLogicalOpExpression(LLVMExprExpression left, 
                                                        LLVMExprExpression right, 
                                                        IRCondition condition,
                                                        LLVMSSATable ssaTable) : base(left, right, ssaTable)
        {
            Condition = condition;
        }

        public override IEnumerable<Instruction> Build() => base.Build();
    }
}
