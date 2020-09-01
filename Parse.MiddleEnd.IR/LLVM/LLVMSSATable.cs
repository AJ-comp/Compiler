﻿using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMSSATable : SSATable
    {
        public int Offset { get; set; } = 1;


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
        public DependencyChainVar NewLink(DType toType, params UseDefChainVar[] targets)
        {
            var result = VariableLLVM.From(Offset++, targets.First(), toType);

            foreach (var target in targets) target.Link(result);

            return result;
        }

        public DependencyChainVar NewLink(IRFuncData irFuncData)
        {
            return VariableLLVM.From(Offset++, irFuncData.Name, LLVMConverter.ToDType(irFuncData.ReturnType));

            // The variable that generated by reference the FuncData doesn't has to linked.
        }

        public override SSATable Clone()
        {
            var result = new LLVMSSATable();
            result._globalRootChainVars.AddRange(GlobalRootChainVars);

            return result;
        }
    }
}
