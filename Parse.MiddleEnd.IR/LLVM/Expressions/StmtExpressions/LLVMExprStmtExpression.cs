using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.StmtExpressions
{
    public class LLVMExprStmtExpression : LLVMStmtExpression
    {
        public LLVMExprStmtExpression(IRExprStatement statement, 
                                                        LLVMSSATable ssaTable) : base(ssaTable)
        {
            _expr = LLVMExprExpression.Create(statement.Expression, ssaTable);
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            if (_expr != null) result.AddRange(_expr.Build());

            return result;
        }

        private LLVMExprExpression _expr;
    }
}
