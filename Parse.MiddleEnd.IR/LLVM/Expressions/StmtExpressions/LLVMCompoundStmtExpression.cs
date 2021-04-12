using Parse.MiddleEnd.IR.Interfaces;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public class LLVMCompoundStmtExpression : LLVMStmtExpression
    {
        public LLVMCompoundStmtExpression(IRCompoundStatement statement, 
                                                                LLVMSSATable ssaTable) : base(ssaTable)
        {
            Items.Add(new LLVMLocalVarListExpression(statement.LocalVars, _ssaTable));

            foreach (var subStatement in statement.Statements)
            {
                Items.Add(Create(subStatement, _ssaTable));
            }
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            foreach(var child in Items)
            {
                result.AddRange(child.Build());
            }

            return result;
        }
    }
}
