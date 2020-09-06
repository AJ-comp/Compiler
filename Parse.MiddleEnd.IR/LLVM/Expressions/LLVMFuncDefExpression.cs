using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMFuncDefExpression : LLVMFirstLayerExpression
    {
        public IRFuncData FuncData { get; private set; }
        public LLVMBlockExpression BlockExpression { get; private set; }
        public int FuncIndex { get; private set; }

        public LLVMFuncDefExpression(IRFuncData funcData, 
                                                        LLVMBlockExpression blockExpression, 
                                                        LLVMSSATable ssaTable,
                                                        int funcIndex) : base(ssaTable)
        {
            FuncData = funcData;
            BlockExpression = blockExpression;
            FuncIndex = funcIndex;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            // generate param code
            string argumentString = "(";
            foreach (var argument in FuncData.Arguments)
                argumentString += LLVMConverter.ToInstructionName(argument.TypeName) + ",";

            if (FuncData.Arguments.Count() > 0) argumentString = argumentString.Substring(0, argumentString.Length - 1);
            argumentString += ")";


            result.Add(new Instruction(string.Format("define {0} @{1}{2} #{3}",
                                                                        LLVMConverter.ToInstructionName(FuncData.ReturnType),
                                                                        FuncData.Name,
                                                                        argumentString,
                                                                        FuncIndex)));

            // generate block code
            result.Add(new Instruction("{"));
            result.AddRange(BlockExpression.Build());
            if (FuncData.ReturnType == ReturnType.Void) result.Add(new Instruction("ret void"));
            result.Add(new Instruction("}"));
            result.Add(Instruction.EmptyLine());

            if(FuncData.Name == "main")
            {
                LLVMAttribute attribute = new LLVMAttribute(FuncIndex);
                attribute.NounwindReadnone = true;
                attribute.StackProtectorBufferSize = 8;
                result.Add(new Instruction(attribute.ToString()));
            }

            return result;
        }
    }
}
