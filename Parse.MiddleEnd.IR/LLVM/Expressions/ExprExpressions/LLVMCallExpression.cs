using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMCallExpression : LLVMExprExpression
    {


        public LLVMCallExpression(IRFuncData funcData, IEnumerable<LLVMExprExpression> paramDatas, LLVMSSATable ssaTable) : base(ssaTable)
        {
            _funcData = funcData;
            _paramDatas = paramDatas;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<IValue> toPassParam = new List<IValue>();
            List<Instruction> result = new List<Instruction>();

            foreach (var paramData in _paramDatas)
            {
                if (paramData is LLVMUseVarExpression)
                    (paramData as LLVMUseVarExpression).IsUseVar = true;

                result.AddRange(paramData.Build());
                toPassParam.Add(paramData.Result);
            }

            result.Add(Instruction.Call(_funcData, toPassParam, _ssaTable));

            return result;
        }


        private IRFuncData _funcData;
        private IEnumerable<LLVMExprExpression> _paramDatas;
    }
}
