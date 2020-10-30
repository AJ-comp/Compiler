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

            DeRefVar = ssaTable.NewLinkAsDeRef(Var) as VariableLLVM;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> instructionList = new List<Instruction>();

            instructionList.AddRange(_useVarExpression.Build());
            instructionList.Add(Instruction.Load(DeRefVar, _ssaTable));
            Result = instructionList.Last().NewSSAVar;

            return instructionList;
        }


        private LLVMUseVarExpression _useVarExpression;
    }
}
