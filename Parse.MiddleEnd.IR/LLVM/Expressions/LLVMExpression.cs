using Parse.Extensions;
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
                result += Items.ItemsString();
            }

            return result;
        }

        public abstract IEnumerable<Instruction> Build();


        protected LLVMSSATable _ssaTable;

        protected LLVMExpression(LLVMSSATable ssaTable)
        {
            _ssaTable = ssaTable;
        }
    }
}
