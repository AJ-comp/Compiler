using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.AssignExpressions
{
    public class LLVMAssignExpression : LLVMAssignableExpression
    {
        public LLVMAssignExpression(IRVar left, 
                                                    LLVMExprExpression right, 
                                                    LLVMSSATable ssaTable) : base(left, right, ssaTable)
        {
        }


        // sample format
        // a(t) = b(s);
        // if both a and b is local var --------
        // assume [%1 = a, %2 = b]--------
        // %3 = load i32, i32* %2, align 4
        // store i32 %3, i32* %1, align 4
        // ---------------------------------------
        // if a is global var --------------------
        // %3 = load i32, i32* %2, align 4
        // store i32 %3, i32* @a, align 4
        // ---------------------------------------
        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();
            result.AddRange(base.Build());
            result.AddRange(ConvertToExtension(Right, DType.Int));

            if (Right is LLVMUseVarExpression)
            {
                var cRight = Right as LLVMUseVarExpression;

                result.Add(Instruction.Store(cRight.Var, Left));
            }
            else if (Right is LLVMConstantExpression)
            {
                var rightConstant = Right.Result as IConstant;

                result.Add(Instruction.Store(rightConstant, Left));
            }
            else // expr
            {
                result.Add(Instruction.Store(Right.Result as VariableLLVM, Left));
            }

            return result;
        }
    }
}
