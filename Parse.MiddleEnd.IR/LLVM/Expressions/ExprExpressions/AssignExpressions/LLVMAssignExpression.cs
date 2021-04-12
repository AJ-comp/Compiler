using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Interfaces;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions.UseVarExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using System.Collections.Generic;
using System.Linq;

namespace Parse.MiddleEnd.IR.LLVM.Expressions.AssignExpressions
{
    public class LLVMAssignExpression : LLVMExprExpression
    {
        public VariableLLVM LeftVar { get; protected set; }
        public LLVMExpression LeftExpr { get; private set; }
        public LLVMExprExpression RightExpr { get; protected set; }

        public bool IsDeRef { get; private set; } = false;

        public LLVMAssignExpression(IRAssignOpExpr expression,
                                                    LLVMSSATable ssaTable) : base(ssaTable)
        {
            _left = expression.Var;
            LeftVar = _ssaTable.Find(_left).LinkedObject as VariableLLVM;

            InitCommon(null, _right);
        }

        public LLVMAssignExpression(LLVMExprExpression left,
                                                    LLVMExprExpression right,
                                                    LLVMSSATable ssaTable) : base(ssaTable)
        {
            InitCommon(left, right);

            if (left is LLVMUseNormalVarExpression)
            {
                var useVarExpr = left as LLVMUseNormalVarExpression;

                LeftVar = _ssaTable.Find(useVarExpr.OriginalVar).LinkedObject as VariableLLVM;
            }
            else if (left is LLVMUseDeRefExpression)
            {
                var useVarExpr = left as LLVMUseDeRefExpression;
                LeftVar = useVarExpr.DeRefVar;

                IsDeRef = true;
            }
        }


        // sample format
        // a(t) = b(s);
        // if both a and b is local var --------
        // assume [%1 = a, %2 = b]--------
        // %3 = load i32, i32* %2, align 4
        // store i32 %3, i32* %1, align 4
        // ---------------------------------------
        // if a is global var --------------------
        // %3 = load i32, i32* %2, align 4
        // store i32 %3, i32* @a, align 4
        // ---------------------------------------
        public override IEnumerable<Instruction> Build()
        {
            List<Instruction> result = new List<Instruction>();
            result.AddRange(RightExpr.Build());
            result.AddRange(ConvertToExtension(RightExpr, StdType.Int));

            if (IsDeRef) result.AddRange(LeftIsDeRefExprProcess());
            else result.AddRange(LeftIsVarProcess());

            Result = result.Last().NewSSAVar;

            return result;
        }


        private void InitCommon(LLVMExpression left, LLVMExprExpression right)
        {
            LeftExpr = left;

            RightExpr = right;
            RightExpr.IsRight = true;

            if (RightExpr is LLVMUseNormalVarExpression)
                (RightExpr as LLVMUseNormalVarExpression).IsUseVar = true;
        }

        private IEnumerable<Instruction> LeftIsVarProcess()
        {
            List<Instruction> result = new List<Instruction>();

            if (RightExpr is LLVMUseNormalVarExpression)
            {
                var cRight = RightExpr as LLVMUseNormalVarExpression;

                result.Add(Instruction.Store(cRight.Result, LeftVar));
            }
            else if (RightExpr is LLVMConstantExpression)
            {
                var rightConstant = RightExpr as LLVMConstantExpression;

                result.Add(Instruction.Store(rightConstant.Result, LeftVar));
            }
            else // expr
            {
                result.Add(Instruction.Store(RightExpr.Result, LeftVar));
            }

            return result;
        }

        private IEnumerable<Instruction> LeftIsDeRefExprProcess()
        {
            List<Instruction> result = new List<Instruction>();

            var deRefExpr = LeftExpr as LLVMUseDeRefExpression;

            var loadInstruction = Instruction.Load(deRefExpr.Var, _ssaTable);
            result.Add(loadInstruction);
            var ssaVar = loadInstruction.NewSSAVar;

            if (RightExpr is LLVMUseNormalVarExpression)
                result.Add(Instruction.Store(RightExpr.Result, ssaVar, true));
            else if (RightExpr is LLVMConstantExpression)
            {
                ssaVar.PointerLevel--;
                result.Add(Instruction.Store(RightExpr.Result, ssaVar));
                ssaVar.PointerLevel++;
            }
            else // expr
            {
                var deRefVar = _ssaTable.NewLinkAsDeRef(ssaVar);
                result.Add(Instruction.Store(RightExpr.Result, deRefVar));
            }

            return result;
        }


        private IRDeclareVar _left;
        private LLVMExprExpression _right;
    }
}
