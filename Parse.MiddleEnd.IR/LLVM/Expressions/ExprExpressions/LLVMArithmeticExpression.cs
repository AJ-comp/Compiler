using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types.ConstantTypes;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMArithmeticExpression : LLVMBinOpExpression
    {
        public LLVMArithmeticExpression(IRArithmeticOpExpr expression,
                                                        LLVMSSATable ssaTable) : base(expression, ssaTable)
        {
            _operation = expression.Operation;

            _left = Create(expression.Left, ssaTable);
            _right = Create(expression.Right, ssaTable);
        }


        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();
            result.AddRange(base.Build());
            result.AddRange(GenerateOperInstructions(_operation));

            Result = result.Last().NewSSAVar;

            return result;
        }


        // sample format
        // %3 <op> %1 nsw [%2 | <constant>]
        private IEnumerable<Instruction> GenerateOperInstructions(IROperation operation)
        {
            List<Instruction> instructionList = new List<Instruction>();

            if (_left.Result is VariableLLVM)
            {
                if (_right.Result is VariableLLVM)
                {
                    instructionList.Add(Instruction.BinOp(_left.Result as VariableLLVM,
                                                                         _right.Result as VariableLLVM,
                                                                         _ssaTable,
                                                                         operation));
                }
                else
                {
                    instructionList.Add(Instruction.BinOp(_left.Result as VariableLLVM,
                                                                         _right.Result as IConstant,
                                                                         _ssaTable,
                                                                         operation));
                }
            }
            else // Constant
            {
                if (_right.Result is VariableLLVM)
                {
                    instructionList.Add(Instruction.BinOp(_right.Result as VariableLLVM,
                                                                         _left.Result as IConstant,
                                                                         _ssaTable, 
                                                                         operation));
                }

                // Constant binop Constant expression never come to this. 
            }

            return instructionList;
        }

        private IROperation _operation;
        private LLVMExprExpression _left;
        private LLVMExprExpression _right;
    }
}
