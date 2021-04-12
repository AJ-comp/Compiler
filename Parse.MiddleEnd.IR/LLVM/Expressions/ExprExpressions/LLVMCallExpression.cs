using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.UseVarExpressions;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMCallExpression : LLVMExprExpression
    {
        public LLVMCallExpression(IRCallExpr expression, 
                                               LLVMSSATable ssaTable) : base(ssaTable)
        {
            _expression = expression;

            foreach (var paramData in _expression.Params)
            {
                _params.Add(Create(paramData, ssaTable));
            }
        }

        public override IEnumerable<Instruction> Build()
        {
            List<ISSAForm> toPassParam = new List<ISSAForm>();
            List<Instruction> result = new List<Instruction>();

            foreach (var paramData in _params)
            {
                if (paramData is LLVMUseNormalVarExpression)
                    (paramData as LLVMUseNormalVarExpression).IsUseVar = true;

                result.AddRange(paramData.Build());
                toPassParam.Add(paramData.Result);
            }

            result.Add(Instruction.Call(_expression.FuncDefInfo, toPassParam, _ssaTable));

            return result;
        }

        private IRCallExpr _expression;
        private List<LLVMExprExpression> _params;
    }
}
