using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.UseVarExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    // constant <binop> constant can't come because optimization logic convert to one constant.
    // ex : 10 + 15 => 25
    // so this expression always has at least one variable.
    public class LLVMBinOpExpression : LLVMExprExpression
    {
        public LLVMBinOpExpression(IRBinOpExpr expression, LLVMSSATable ssaTable) : base(ssaTable)
        {
            _expression = expression;
            _left = Create(expression.Left, ssaTable);
            _right = Create(expression.Right, ssaTable);
        }


        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();

            if (_left is LLVMUseNormalVarExpression)
                (_left as LLVMUseNormalVarExpression).IsUseVar = true;

            if (_right is LLVMUseNormalVarExpression)
                (_right as LLVMUseNormalVarExpression).IsUseVar = true;

            result.AddRange(_left.Build());
            result.AddRange(_right.Build());

            // Convert to equal the left type and right type.
            var toType = LLVMChecker.MaximumType(_left.Result.TypeKind, _right.Result.TypeKind);

            if (toType != StdType.Double) toType = StdType.Int;

            result.AddRange(ConvertToExtension(_left, toType));
            result.AddRange(ConvertToExtension(_right, toType));



            return result;
        }

        protected bool IsDoubleType(ISSAForm value) => (value.TypeKind == StdType.Double);
        protected IEnumerable<Instruction> ExtensionToI32Inst(IntegerVarLLVM value, LLVMSSATable ssTable)
        {
            List<Instruction> result = new List<Instruction>();
            if (value.TypeKind == StdType.Int) return result;

            result.Add(Instruction.ConvertType(value, StdType.Int, ssTable));
            return result;
        }

        protected IEnumerable<Instruction> ConvertToFpInst(IntVariableLLVM var, LLVMSSATable ssTable)
        {
            List<Instruction> result = new List<Instruction>();

            if (!IsDoubleType(_right.Result)) return result;
            if (!(_left.Result.TypeKind == StdType.Double) && !(_right.Result.TypeKind == StdType.Double)) return result;
            
            Instruction.IToFp(var, ssTable);

            return result;
        }

        private IRBinOpExpr _expression;
        private LLVMExprExpression _left;
        private LLVMExprExpression _right;
    }
}
