using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using System;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions
{
    public class LLVMLocalVarListExpression : LLVMDependencyExpression
    {
        public LLVMLocalVarListExpression(IEnumerable<IRDeclareVar> initialVarList, LLVMSSATable ssaTable) : base(ssaTable)
        {
            foreach (var initialVar in initialVarList)
            {
                var declareVar = _ssaTable.RegisterRootChainVarToLocal(initialVar);
                var expression = initialVar.InitialExpr;

                _declaredInfos.Add(new Tuple<RootChainVarContainer, LLVMExprExpression>(declareVar, 
                                                                                                                    LLVMExprExpression.Create(expression, _ssaTable)));
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


        private List<Tuple<RootChainVarContainer, LLVMExprExpression>> _declaredInfos = new List<Tuple<RootChainVarContainer, LLVMExprExpression>>();

    }
}
