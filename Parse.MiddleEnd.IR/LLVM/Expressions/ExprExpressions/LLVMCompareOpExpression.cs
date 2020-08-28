using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.LogicalExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public enum IRCondition
    {
        [Description("eq")] EQ,
        [Description("ne")] NE,
        [Description("gt")] GT,
        [Description("ge")] GE,
        [Description("lt")] LT,
        [Description("le")] LE
    };

    public class LLVMCompareOpExpression : LLVMLogicalOpExpression
    {
        public LLVMCompareOpExpression(LLVMExprExpression left, 
                                                            LLVMExprExpression right, 
                                                            IRCondition condition,
                                                            LLVMSSATable ssaTable) : base(left, right, condition, ssaTable)
        {
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();
            result.AddRange(base.Build());
            result.AddRange(GenerateInstructions());

            Result = result.Last().NewSSAVar as BitVariableLLVM;

            return result;
        }


        // icmp or fcmp
        private IEnumerable<Instruction> GenerateInstructions()
        {
            List<Instruction> result = new List<Instruction>();

            // At here both left type and right type is equal
            // At here the type is int or double.
            if (Left.Result.TypeName == Types.DType.Double)
                result.Add(Instruction.FcmpA(Condition, Left.Result, Right.Result, _ssaTable));
            else
                result.Add(Instruction.IcmpA(Condition, Left.Result, Right.Result, _ssaTable));

            return result;
        }
    }
}
