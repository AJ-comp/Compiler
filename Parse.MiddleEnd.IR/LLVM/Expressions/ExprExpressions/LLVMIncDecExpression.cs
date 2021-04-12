using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types.ConstantTypes;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMIncDecExpression : LLVMExprExpression
    {
        public LLVMIncDecExpression(IRIncDecExpr expression, LLVMSSATable ssaTable) : base(ssaTable)
        {
            _expression = expression;
            //            _useVar = (expression.Var as IRUseVarExpr).DeclareInfo;
            _useVar = expression.Var;
        }


        // sample format
        // a++
        // <first> = load i32, i32* <UseVar>, align 4 ; <-- use this.
        // <second> = add nsw i32 <first>, 1
        // store i32 <second>, i32* <UseVar>, align 4
        // <Result> = <first> or <second>
        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> instructionList = new List<Instruction>();

            var ssVar = _ssaTable.Find(_useVar).LinkedObject as VariableLLVM;
            instructionList.Add(Instruction.Load(ssVar, _ssaTable));
            var firstVar = instructionList.Last().NewSSAVar;

            if (_expression.ProcessInfo == Info.PreInc || _expression.ProcessInfo == Info.PostInc)
                instructionList.Add(Instruction.BinOp(firstVar as VariableLLVM, new IntConstant(1), _ssaTable, IROperation.Add));
            else
                instructionList.Add(Instruction.BinOp(firstVar as VariableLLVM, new IntConstant(1), _ssaTable, IROperation.Sub));

            Result = (_expression.ProcessInfo == Info.PreInc || _expression.ProcessInfo == Info.PreDec)
                      ? instructionList.Last().NewSSAVar : firstVar;

            instructionList.Add(Instruction.Store(instructionList.Last().NewSSAVar, ssVar));

            return instructionList;
        }


        private IRIncDecExpr _expression;
        private IRDeclareVar _useVar;
    }
}
