using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMFuncDefExpression : LLVMFirstLayerExpression
    {
        public IRFuncDefInfo FuncData { get; private set; }

        public LLVMFuncDefExpression(IRFuncDefInfo funcData,
                                                        LLVMSSATable ssaTable) : base(ssaTable)
        {
            FuncData = funcData;

//            Items.Add(new LLVMLocalVarListExpression(funcData.Arguments, _ssaTable));
//            Items.Add(new LLVMCompoundStmtExpression(funcData.Statement, _ssaTable));
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            result.AddRange(Instruction.DefineFunc(FuncData, _ssaTable));

            if(FuncData.Name == "main")
            {
                LLVMAttribute attribute = new LLVMAttribute(FuncData.Index)
                {
                    NounwindReadnone = true,
                    StackProtectorBufferSize = 8
                };

                result.Add(new Instruction(attribute.ToString()));
            }

            return result;
        }
    }
}
