using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public enum IROperation
    {
        [Description("add")] Add,
        [Description("sub")] Sub,
        [Description("mul")] Mul,
        [Description("sdiv")] Div,
        [Description("srem")] Mod
    };

    public class LLVMArithmeticExpression : LLVMBinOpExpression
    {
        public IROperation Operation { get; }

        public LLVMArithmeticExpression(LLVMExprExpression left, 
                                                        LLVMExprExpression right, 
                                                        IROperation operation,
                                                        LLVMSSATable ssaTable) : base(left, right, ssaTable)
        {
            Operation = operation;
        }


        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();
            result.AddRange(base.Build());
            result.AddRange(GenerateOperInstructions(Operation));

            Result = result.Last().NewSSAVar;

            return result;
        }


        // sample format
        // %3 <op> %1 nsw [%2 | <constant>]
        private IEnumerable<Instruction> GenerateOperInstructions(IROperation operation)
        {
            List<Instruction> instructionList = new List<Instruction>();

            instructionList.AddRange(ArithmeticPrevProcess(Left, Right));
            instructionList.AddRange(ArithmeticPrevProcess(Right, Left));

            if (Left.Result is VariableLLVM)
            {
                if (Right.Result is VariableLLVM)
                {
                    instructionList.Add(Instruction.BinOp(Left.Result as VariableLLVM,
                                                                         Right.Result as VariableLLVM,
                                                                         _ssaTable,
                                                                         operation));
                }
                else
                {
                    instructionList.Add(Instruction.BinOp(Left.Result as VariableLLVM,
                                                                         Right.Result as IConstant,
                                                                         _ssaTable,
                                                                         operation));
                }
            }

            return instructionList;
        }
    }
}
