using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMDeRefExpression : LLVMExprExpression
    {
        public VariableLLVM DeRefVar { get; private set; }
        public VariableLLVM Var => _useVarExpression.SSAVar;

        public LLVMDeRefExpression(LLVMUseVarExpression useVarExpression, LLVMSSATable ssaTable) : base(ssaTable)
        {
            _useVarExpression = useVarExpression;
            _useVarExpression.IsUseVar = true;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> instructionList = new List<Instruction>();
            DeRefVar = _ssaTable.NewLinkAsDeRef(Var) as VariableLLVM;

            instructionList.AddRange(_useVarExpression.Build());
            instructionList.Add(Instruction.Load(DeRefVar, _ssaTable));
            Result = instructionList.Last().NewSSAVar;

            return instructionList;
        }


        private LLVMUseVarExpression _useVarExpression;
    }
}
