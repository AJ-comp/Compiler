using Parse.MiddleEnd.IR.Datas;
using Parse.Types;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public abstract class LLVMExprExpression : LLVMDependencyExpression
    {
        protected LLVMExprExpression(LLVMSSATable ssaTable) : base(ssaTable)
        {
        }

        public bool IsRight { get; set; }
        public IValue Result { get; protected set; }

        protected IEnumerable<Instruction> ConvertToExtension(LLVMExprExpression target, DType to)
        {
            List<Instruction> result = new List<Instruction>();

            var leftAlign = LLVMConverter.ToAlignSize(target.Result.TypeName);
            var rightAlign = LLVMConverter.ToAlignSize(to);

            if (leftAlign >= rightAlign) return result;

            if (to == DType.Double)
            {
                if (target.Result.TypeName != DType.Int)
                    result.AddRange(new LLVMCastingExpression(DType.Int, target, _ssaTable).Build());

                result.AddRange(new LLVMCastingExpression(DType.Double, target, _ssaTable).Build());
            }
            else
            {
                result.AddRange(new LLVMCastingExpression(to, target, _ssaTable).Build());
            }

            return result;
        }
    }
}
