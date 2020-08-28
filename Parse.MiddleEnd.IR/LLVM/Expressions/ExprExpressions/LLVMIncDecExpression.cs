using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types.ConstantTypes;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMIncDecExpression : LLVMExprExpression
    {
        public enum Info { PreInc, PreDec, PostInc, PostDec };

        public IRVar UseVar { get; }
        public Info ProcessInfo { get; }


        public LLVMIncDecExpression(IRVar var, Info processInfo, LLVMSSATable ssaTable) : base(ssaTable)
        {
            UseVar = var;
            ProcessInfo = processInfo;
        }


        // sample format
        // a++
        // <first> = load i32, i32* <UseVar>, align 4 ; <-- use this.
        // <second> = add nsw i32 <first>, 1
        // store i32 <second>, i32* <first>, align 4
        // <Result> = <first> or <second>
        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> instructionList = new List<Instruction>();

            var ssVar = _ssaTable.Find(UseVar).LinkedObject as VariableLLVM;
            instructionList.Add(Instruction.Load(ssVar, _ssaTable));
            var firstVar = instructionList.Last().NewSSAVar;

            if (ProcessInfo == Info.PreInc || ProcessInfo == Info.PostInc)
                instructionList.Add(Instruction.BinOp(firstVar, new IntConstant(1), _ssaTable, IROperation.Add));
            else
                instructionList.Add(Instruction.BinOp(firstVar, new IntConstant(1), _ssaTable, IROperation.Sub));

            instructionList.Add(Instruction.Store(firstVar, instructionList.Last().NewSSAVar));

            return instructionList;
        }
    }
}
