using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMParamListAndReturnExpression : LLVMDependencyExpression
    {
        public IEnumerable<RootChainVar> ParamVars => _paramVars;

        public LLVMParamListAndReturnExpression(IEnumerable<IRVar> paramVars, 
                                                                       IRVar returnVar, 
                                                                       LLVMSSATable ssaTable) : base(ssaTable)
        {
            // register value
            foreach (var paramVar in paramVars)
            {
                var param = RootChainVar.Copy(paramVar, "!param" + paramVar.Offset);
                _paramValues.Add(_ssaTable.RegisterRootChainVarToLocal(param));
            }

            // register return variable
            ssaTable.RegisterRootChainVarToLocal(RootChainVar.EmptyRootChainVar);

            // register param variable
            foreach (var paramVar in paramVars)
                _paramVars.Add(_ssaTable.RegisterRootChainVarToLocal(paramVar));
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            int index = 0;
            foreach (var paramVar in _paramVars)
            {
                result.Add(Instruction.DeclareLocalVar(paramVar));
                result.Add(Instruction.Store(_paramValues[index].LinkedObject, paramVar.LinkedObject));
            }

            return result;
        }

        private List<RootChainVar> _paramValues = new List<RootChainVar>();
        private List<RootChainVar> _paramVars = new List<RootChainVar>();
    }
}
