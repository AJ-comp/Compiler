using Parse.MiddleEnd.IR.Datas;
using Parse.Types.ConstantTypes;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMConstantExpression : LLVMExprExpression
    {
        public LLVMConstantExpression(IConstant value, LLVMSSATable ssaTable) : base(ssaTable)
        {
            Result = new SSAConst(value);
        }

        public override IEnumerable<Instruction> Build() => new List<Instruction>();
    }
}
