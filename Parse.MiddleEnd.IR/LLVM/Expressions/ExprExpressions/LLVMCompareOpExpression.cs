using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions
{
    public class LLVMCompareOpExpression : LLVMBinOpExpression
    {
        // if condition statement is not logical statement, it has to convert to logical statement.
        // (ex : (a + b)  convert to  (a + b) != 0)
        /*
        logicalOp = new LLVMCompareOpExpression(irExpression as LLVMExprExpression,
                                                                                    new LLVMConstantExpression(new IntConstant(0), ssaTable), 
                                                                                    IRCompareSymbol.NE, 
                                                                                    ssaTable);
        */

        public LLVMCompareOpExpression(IRCompareOpExpr expression, LLVMSSATable ssaTable)
            : base(expression, ssaTable)
        {
            _condition = expression.CompareSymbol;
            _left = expression.Left;
            _right = expression.Right;
        }

        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();
            result.AddRange(base.Build());
            result.AddRange(GenerateInstructions());

            Result = result.Last().NewSSAVar as BitVariableLLVM;

            return result;
        }


        // icmp or fcmp
        private IEnumerable<Instruction> GenerateInstructions()
        {
            List<Instruction> result = new List<Instruction>();

            // At here both left type and right type is equal
            // At here the type is int or double.
            if (_left.Result.TypeKind == StdType.Double)
                result.Add(Instruction.FcmpA(_condition, _left.Result, _right.Result, _ssaTable));
            else
                result.Add(Instruction.IcmpA(_condition, _left.Result, _right.Result, _ssaTable));

            return result;
        }

        private IRCompareSymbol _condition;
        private IRExpr _left, _right;
    }
}
