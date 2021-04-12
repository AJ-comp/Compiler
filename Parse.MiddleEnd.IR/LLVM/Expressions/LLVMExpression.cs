using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public abstract class LLVMExpression : IRExpression
    {
        public List<LLVMExpression> Items = new List<LLVMExpression>();

        public override string ToString()
        {
            string result = string.Empty;

            if (Items.Count > 0)
            {
                result += string.Format(", Expression: {0} -> ", Name);
                result += Items.ItemsString(PrintType.Type);
            }

            return result;
        }

        public abstract IEnumerable<Instruction> Build();

        public static LLVMExpression Create(IRStructDefInfo structDefInfo, LLVMSSATable ssaTable)
            => new LLVMClassExpression(structDefInfo, ssaTable);


        protected LLVMSSATable _ssaTable;

        protected LLVMExpression(LLVMSSATable ssaTable)
        {
            _ssaTable = ssaTable;
        }
    }
}
