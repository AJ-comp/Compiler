﻿using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System;
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
            rootChainVar.Link(VariableLLVM.From(toRegist, Offset++));

            _localRootChainVars.Add(rootChainVar);

            return rootChainVar;
        }

        public BitVariableLLVM RegisterLabel()
        {
            var rootChainVar = new RootChainVar(new BitVariableLLVM(0));
            rootChainVar.Link(new BitVariableLLVM(Offset++));

            _localRootChainVars.Add(rootChainVar);

            return rootChainVar.LinkedObject as BitVariableLLVM;
        }

        public DependencyChainVar NewLink(params UseDefChainVar[] targets)
        {
            var target = targets.First();
            
            return VariableLLVM.From(target, Offset++);
        }

        public DependencyChainVar NewLinkAsDeRef(params UseDefChainVar[] targets)
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
        public DependencyChainVar NewLink(DType toType, params UseDefChainVar[] targets)
        {
            var firstTarget = targets.First();
            firstTarget.Offset = Offset++;
            var result = VariableLLVM.From(firstTarget, toType);

            foreach (var target in targets) target.Link(result);

            return result;
        }

        public DependencyChainVar NewLink(IRFuncData irFuncData)
        {
            //            Offset++;
            //            return VariableLLVM.From(irFuncData, LLVMConverter.ToDType(irFuncData.ReturnType), true);

            return null;

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
