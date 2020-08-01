using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMFuncDefExpression : LLVMFirstLayerExpression
    {
        public IRFuncData FuncData { get; private set; }
        public LLVMBlockExpression BlockExpression { get; private set; }

        public LLVMFuncDefExpression(IRFuncData funcData, 
                                                        LLVMBlockExpression blockExpression, 
                                                        LLVMSSATable ssaTable) : base(ssaTable)
        {
            FuncData = funcData;
            BlockExpression = blockExpression;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            result.Add(new Instruction(string.Format("define {0} @{1}", 
                                                                        LLVMConverter.ToInstructionName(FuncData.ReturnType), FuncData.Name)));

            // generate param code
            string argumentString = "(";
            foreach (var argument in FuncData.Arguments)
                argumentString += LLVMConverter.ToInstructionName(argument.TypeName) + ",";

            if (FuncData.Arguments.Count() > 0) argumentString = argumentString.Substring(0, argumentString.Length - 1);
            argumentString += ")";

            result.Add(new Instruction(argumentString));

            // generate block code
            result.Add(new Instruction("{"));
            result.AddRange(BlockExpression.Build());
            result.Add(new Instruction("}"));

            return result;
        }
    }
}
