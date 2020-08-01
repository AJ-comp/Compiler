using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.AssignExpressions
{
    public abstract class LLVMAssignableExpression : LLVMDependencyExpression
    {
        public VariableLLVM Left { get; protected set; }
        public LLVMExprExpression Right { get; protected set; }


        protected LLVMAssignableExpression(IRVar left,
                                                                LLVMExprExpression right, 
                                                                LLVMSSATable ssaTable) : base(ssaTable)
        {
            Left = _ssaTable.Find(left).LinkedObject as VariableLLVM;
            Right = right;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            if (Right is LLVMUseVarExpression)
                (Right as LLVMUseVarExpression).IsUseVar = true;

            result.AddRange(Right.Build());

            return result;
        }
    }
}
