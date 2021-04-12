using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMSSATable : SSATable
    {
        public int Offset { get; set; } = 0;


        public LLVMSSATable()
        {
        }

        public RootChainVarContainer RegisterRootChainVarToGlobal(IRDeclareVar toRegist)
        {
            var result = FindFromGlobalList(toRegist);
            if (result != null) return result;

            var rootChainVar = new RootChainVarContainer(toRegist);
            rootChainVar.Link(VariableLLVM.From(toRegist));

            _globalRootChainVars.Add(rootChainVar);

            return rootChainVar;
        }

        public RootChainVarContainer RegisterRootChainVarToLocal(IRDeclareVar toRegist)
        {
            var result = FindFromLocalList(toRegist);
            if (result != null) return result;

            var rootChainVar = new RootChainVarContainer(toRegist);
            rootChainVar.Link(VariableLLVM.From(toRegist, Offset++));

            _localRootChainVars.Add(rootChainVar);

            return rootChainVar;
        }

        public RootChainVarContainer RegisterReturn()
        {
            var rootChainVar = new RootChainVarContainer(new BitVariableLLVM(0));
            rootChainVar.Link(new BitVariableLLVM(Offset++));

            _localRootChainVars.Add(rootChainVar);

            return rootChainVar;
        }

        public BitVariableLLVM RegisterLabel()
        {
            var rootChainVar = new RootChainVarContainer(new BitVariableLLVM(0));
            rootChainVar.Link(new BitVariableLLVM(Offset++));

            _localRootChainVars.Add(rootChainVar);

            return rootChainVar.LinkedObject as BitVariableLLVM;
        }

        public SSAVar NewLink(params SSAVar[] targets)
        {
            var firstTaget = targets.First();
            var result = VariableLLVM.From(firstTaget, Offset++);

            foreach (var target in targets) target.Link(result);

            return result;
        }

        public SSAVar NewLinkAsDeRef(params SSAVar[] targets)
        {
            var result = NewLink(targets);
            if (result.PointerLevel == 0) throw new Exception();
            result.PointerLevel--;

            return result;
        }

        /// <summary>
        /// This function means as follow.
        /// targets[0]  targets[1]
        ///      |               |
        ///               |
        ///            result
        /// </summary>
        /// <param name="toType"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public SSAVar NewLink(StdType toType, params SSAVar[] targets)
        {
            var firstTarget = targets.First();
            var result = VariableLLVM.From(firstTarget, toType, Offset++);

            foreach (var target in targets) target.Link(result);

            return result;
        }

        public SSAVar NewLink(StdType toType)
        {
            Offset++;
            return VariableLLVM.From(Offset, toType);
        }

        public override SSATable Clone()
        {
            var result = new LLVMSSATable();
            result._globalRootChainVars.AddRange(GlobalRootChainVars);

            return result;
        }
    }
}
