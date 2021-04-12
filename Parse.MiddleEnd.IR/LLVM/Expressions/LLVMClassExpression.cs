using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMClassExpression : LLVMFirstLayerExpression
    {
        public LLVMClassExpression(IRStructDefInfo structData,
                                                 LLVMSSATable ssaTable) : base(ssaTable)
        {
            _structData = structData;

//            Items.Add(new LLVMLocalVarListExpression(structData.MemberVarList, _ssaTable));

            foreach (var funcDef in structData.FuncDefList)
            {
                Items.Add(new LLVMFuncDefExpression(funcDef, _ssaTable));
            }
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            result.AddRange(Instruction.DefineStruct(_structData, _ssaTable));

            return result;
        }

        private IRStructDefInfo _structData;
    }
}
