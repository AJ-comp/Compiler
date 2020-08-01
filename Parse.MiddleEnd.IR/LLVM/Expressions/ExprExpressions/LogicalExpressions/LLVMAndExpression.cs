using Parse.MiddleEnd.IR.LLVM.Models;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.LogicalExpressions
{
    //public class LLVMAndExpression : LLVMLogicalOpExpression
    //{
    //    public LLVMAndExpression(BitVariableLLVM left, IValue right, IRCondition condition) : base(left, right, condition)
    //    {
    //    }

    //    // sample format
    //    // <result> = load i32, <ty>* <op>, align <align>
    //    // <result> = load i32, <ty>* <op>, align <align>
    //    // <result> = <cond>

    //    // [<result> = sext <ty> <op> to i32]*
    //    // [<result> = sitofp i32 <op> to double] || [<result> = uitofp i32 <op> to double]
    //    // <result> = icmp <cond> i32 <op1>, <op2> || <result> = fcmp <cond> <ty> <left>, <right>
    //    public override string GeneratedCode(GlobalTable globalTable, LocalTable localTable)
    //    {
    //        List<Instruction> instructionList = new List<Instruction>();
    //        instructionList.Add(Instruction.Load(Left, localTable));

    //        if(Right is BitVariableLLVM)
    //        {
    //            instructionList.Add(Instruction.Load(Right as BitVariableLLVM, localTable));
    //            var loadedVar1 = instructionList.First().Result;
    //            var loadedVar2 = instructionList.Last().Result;



    //            instructionList.Add(Instruction.BinOp(loadedVar1.SSF,
    //                                                                 loadedVar2.SSF,
    //                                                                 localTable,
    //                                                                 Condition));
    //        }
    //        else
    //        {

    //        }

    //        return string.Empty;
    //    }
    //}
}
