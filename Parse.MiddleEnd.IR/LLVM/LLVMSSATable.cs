using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMSSATable : SSATable
    {
        public int Offset { get; set; }


        public LLVMSSATable()
        {
        }

        public RootChainVar RegisterRootChainVarToGlobal(IRVar toRegist)
        {
            if (FindFromGlobalList(toRegist) != null) return null;

            var rootChainVar = new RootChainVar(toRegist);
            rootChainVar.Link(VariableLLVM.From(toRegist));

            _globalRootChainVars.Add(rootChainVar);

            return rootChainVar;
        }

        public RootChainVar RegisterRootChainVarToLocal(IRVar toRegist)
        {
            if (FindFromLocalList(toRegist) != null) return null;

            var rootChainVar = new RootChainVar(toRegist);
            rootChainVar.Link(VariableLLVM.From(Offset++, toRegist));

            _localRootChainVars.Add(rootChainVar);

            return rootChainVar;
        }

        public BitVariableLLVM RegisterLabel()
        {
            var rootChainVar = new RootChainVar(new BitVariableLLVM(0, null));
            rootChainVar.Link(new BitVariableLLVM(Offset++, null));

            _localRootChainVars.Add(rootChainVar);

            return rootChainVar.LinkedObject as BitVariableLLVM;
        }

        public DependencyChainVar NewLink(params UseDefChainVar[] targets)
            => VariableLLVM.From(Offset++, targets.First(), targets.First().TypeName);

        public DependencyChainVar NewLink(DType toType, params UseDefChainVar[] targets)
        {
            var result = VariableLLVM.From(Offset++, targets.First(), toType);

            foreach (var target in targets) target.Link(result);

            return result;
        }

        public override SSATable Clone()
        {
            var result = new LLVMSSATable();
            result._globalRootChainVars.AddRange(GlobalRootChainVars);

            return result;
        }
    }
}
