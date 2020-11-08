using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.LLVM.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMLocalVarListExpression : LLVMDependencyExpression
    {
        public LLVMLocalVarListExpression(IEnumerable<CalculationInfo> declareInfoList, LLVMSSATable ssaTable) : base(ssaTable)
        {
            foreach (var declareInfo in declareInfoList)
            {
                var declareVar = _ssaTable.RegisterRootChainVarToLocal(declareInfo.Left);
                var expression = declareInfo.Right;

                _declaredInfos.Add(new Tuple<RootChainVar, LLVMExprExpression>(declareVar, expression));
            }
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            // alloca
            foreach (var declaredInfo in _declaredInfos)
                result.Add(Instruction.DeclareLocalVar(declaredInfo.Item1));

            // store if it was initialized
            foreach(var declaredInfo in _declaredInfos)
            {
                if (declaredInfo.Item2 == null) continue;

                LLVMExprExpression right = declaredInfo.Item2;
                result.AddRange(right.Build());

                result.Add(Instruction.Store(right.Result, declaredInfo.Item1.LinkedObject));
            }

            return result;
        }


        private List<Tuple<RootChainVar, LLVMExprExpression>> _declaredInfos = new List<Tuple<RootChainVar, LLVMExprExpression>>();

    }
}
