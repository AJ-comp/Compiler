using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.LogicalExpressions
{
    public class LLVMCompareOpExpression : LLVMBinOpExpression
    {
        public IRCondition Condition { get; }

        public LLVMCompareOpExpression(LLVMExprExpression left, 
                                                            LLVMExprExpression right, 
                                                            IRCondition condition,
                                                            LLVMSSATable ssaTable) : base(left, right, ssaTable)
        {
            Condition = condition;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            if (Condition == IRCondition.EQ) result.AddRange(GenerateEqCode());
            else if (Condition == IRCondition.NE) result.AddRange(GenerateNeCode());


            return result;
        }


        // icmp or fcmp
        private IEnumerable<Instruction> GenerateEqCode()
        {
            List<Instruction> result = new List<Instruction>();

//            if(Left.Result.TypeName != Right.Result.TypeName)
//            Instruction.Icmp(Left.Result)

            return result;
        }

        private IEnumerable<Instruction> GenerateNeCode()
        {
            List<Instruction> result = new List<Instruction>();



            return result;
        }
    }
}
