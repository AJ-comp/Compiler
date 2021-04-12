using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Expressions.AssignExpressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.UseVarExpressions;
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
        public ISSAForm Result { get; protected set; }

        protected IEnumerable<Instruction> ConvertToExtension(LLVMExprExpression target, StdType to)
        {
            List<Instruction> result = new List<Instruction>();

            var leftAlign = LLVMConverter.ToAlignSize(target.Result.TypeKind);
            var rightAlign = LLVMConverter.ToAlignSize(to);

            if (leftAlign >= rightAlign) return result;

            if (to == StdType.Double)
            {
                if (target.Result.TypeKind != StdType.Int)
                    result.AddRange(new LLVMCastingExpression(StdType.Int, target, _ssaTable).Build());

                result.AddRange(new LLVMCastingExpression(StdType.Double, target, _ssaTable).Build());
            }
            else
            {
                result.AddRange(new LLVMCastingExpression(to, target, _ssaTable).Build());
            }

            return result;
        }

        public static LLVMExprExpression Create(IRExpr expression, LLVMSSATable ssaTable)
        {
            if (expression is IRCompareOpExpr)
                return new LLVMCompareOpExpression(expression as IRCompareOpExpr, ssaTable);
            else if (expression is IRArithmeticOpExpr)
                return new LLVMArithmeticExpression(expression as IRArithmeticOpExpr, ssaTable);
            else if (expression is IRAssignOpExpr)
                return new LLVMAssignExpression(expression as IRAssignOpExpr, ssaTable);
            else if (expression is IRUseVarExpr)
                return new LLVMUseNormalVarExpression((expression as IRUseVarExpr).DeclareInfo, ssaTable);
            else if (expression is IRUseDeRefExpr)
                return new LLVMUseDeRefExpression(expression as IRUseDeRefExpr, ssaTable);
            else if (expression is IRIncDecExpr)
                return new LLVMIncDecExpression(expression as IRIncDecExpr, ssaTable);
            else if (expression is IRCallExpr)
                return new LLVMCallExpression((expression as IRCallExpr), ssaTable);
            else if (expression is IRBitLiteralExpr)
                return new LLVMConstantExpression((expression as IRBitLiteralExpr).Result, ssaTable);
            else if (expression is IRCharLiteralExpr)
                return new LLVMConstantExpression((expression as IRCharLiteralExpr).Result, ssaTable);
            else if (expression is IRInt8LiteralExpr)
                return new LLVMConstantExpression((expression as IRInt8LiteralExpr).Result, ssaTable);
            else if (expression is IRInt16LiteralExpr)
                return new LLVMConstantExpression((expression as IRInt16LiteralExpr).Result, ssaTable);
            else if (expression is IRInt32LiteralExpr)
                return new LLVMConstantExpression((expression as IRInt32LiteralExpr).Result, ssaTable);
            else if (expression is IRFloatLiteralExpr)
                return new LLVMConstantExpression((expression as IRFloatLiteralExpr).Result, ssaTable);
            else if (expression is IRDoubleLiteralExpr)
                return new LLVMConstantExpression((expression as IRDoubleLiteralExpr).Result, ssaTable);

            return null;
        }
    }
}
