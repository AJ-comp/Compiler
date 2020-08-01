using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System.Collections.Generic;
using System.ComponentModel;

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


        // sample format
        // [<result> = sext <ty> <op> to i32]*
        // [<result> = sitofp i32 <op> to double] || [<result> = uitofp i32 <op> to double]
        // <result> = icmp <cond> i32 <op1>, <op2> || <result> = fcmp <cond> <ty> <left>, <right>
        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> instructionList = new List<Instruction>();
            instructionList.AddRange(base.Build());

            instructionList.AddRange(Left.Build());
            instructionList.AddRange(Right.Build());

            // extension to int type
            if (Left.Result is IntegerVariableLLVM)
                instructionList.AddRange(ExtensionToI32Inst(Left.Result as IntegerVariableLLVM, _ssaTable));
            if(Right.Result is IntegerVariableLLVM)
                instructionList.AddRange(ExtensionToI32Inst(Right.Result as IntegerVariableLLVM, _ssaTable));

            // convert to double type
            if (Left.Result.TypeName == DType.Double || Right.Result.TypeName == DType.Double)
            {
                if (Left.Result is IntegerVariableLLVM)
                    instructionList.AddRange(ExtensionToI32Inst(Left.Result as IntegerVariableLLVM, _ssaTable));
                if (Right.Result is IntegerVariableLLVM)
                    instructionList.AddRange(ExtensionToI32Inst(Right.Result as IntegerVariableLLVM, _ssaTable));
            }

            return instructionList;
        }
    }
}
