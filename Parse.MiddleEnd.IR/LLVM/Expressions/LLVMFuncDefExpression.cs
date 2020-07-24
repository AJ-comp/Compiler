using Parse.MiddleEnd.IR.Datas;
using System;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMFuncDefExpression : LLVMExpression
    {
        public IRFuncData funcData;
        public LLVMBlockExpression blockExpression;

        public override string GeneratedCode()
        {
            var result = string.Format("define {0} @{1}", LLVMConverter.ToInstructionName(funcData.ReturnType), funcData.Name);

            result += "(";
            foreach (var argument in funcData.Arguments)
                result += LLVMConverter.ToInstructionName(argument.TypeName) + ",";

            if (funcData.Arguments.Count > 0) result = result.Substring(0, result.Length - 1);
            result += ")" + Environment.NewLine;

            result += blockExpression.GeneratedCode();

            return result;
        }
    }
}
