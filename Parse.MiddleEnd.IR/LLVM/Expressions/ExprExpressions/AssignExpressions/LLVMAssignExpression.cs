using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.LLVM.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.LLVM.Models.VariableModels;
using Parse.Types;
using Parse.Types.ConstantTypes;
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

        public LLVMAssignExpression(IRVar left,
                                                    LLVMExprExpression right,
                                                    LLVMSSATable ssaTable) : base(ssaTable)
        {
            LeftVar = _ssaTable.Find(left).LinkedObject as VariableLLVM;

            InitCommon(null, right);
        }

        public LLVMAssignExpression(LLVMExprExpression left,
                                                    LLVMExprExpression right,
                                                    LLVMSSATable ssaTable) : base(ssaTable)
        {
            InitCommon(left, right);

            if (left is LLVMUseVarExpression)
            {
                var useVarExpr = left as LLVMUseVarExpression;

                LeftVar = _ssaTable.Find(useVarExpr.OriginalVar).LinkedObject as VariableLLVM;
            }
            else if (left is LLVMDeRefExpression)
            {
                var useVarExpr = left as LLVMDeRefExpression;
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
            result.AddRange(ConvertToExtension(RightExpr, DType.Int));

            if (IsDeRef) result.AddRange(LeftIsDeRefExprProcess());
            else result.AddRange(LeftIsVarProcess());

            return result;
        }


        private void InitCommon(LLVMExpression left, LLVMExprExpression right)
        {
            LeftExpr = left;

            RightExpr = right;
            RightExpr.IsRight = true;

            if (RightExpr is LLVMUseVarExpression)
                (RightExpr as LLVMUseVarExpression).IsUseVar = true;
        }

        private IEnumerable<Instruction> LeftIsVarProcess()
        {
            List<Instruction> result = new List<Instruction>();

            if (RightExpr is LLVMUseVarExpression)
            {
                var cRight = RightExpr as LLVMUseVarExpression;

                result.Add(Instruction.Store(cRight.SSAVar, LeftVar));
            }
            else if (RightExpr is LLVMConstantExpression)
            {
                var rightConstant = RightExpr.Result as IConstant;

                result.Add(Instruction.Store(rightConstant, LeftVar));
            }
            else // expr
            {
                result.Add(Instruction.Store(RightExpr.Result as VariableLLVM, LeftVar));
            }

            return result;
        }

        private IEnumerable<Instruction> LeftIsDeRefExprProcess()
        {
            List<Instruction> result = new List<Instruction>();

            var deRefExpr = LeftExpr as LLVMDeRefExpression;

            var loadInstruction = Instruction.Load(deRefExpr.Var, _ssaTable);
            result.Add(loadInstruction);
            var newSSAVar = loadInstruction.NewSSAVar;

            if (RightExpr is LLVMUseVarExpression)
            {
                var cRight = RightExpr as LLVMUseVarExpression;

                result.Add(Instruction.Store(cRight.SSAVar, newSSAVar, true));
            }
            else if (RightExpr is LLVMConstantExpression)
            {
                var rightConstant = RightExpr.Result as IConstant;

                newSSAVar.PointerLevel--;
                result.Add(Instruction.Store(rightConstant, newSSAVar));
                newSSAVar.PointerLevel++;
            }
            else // expr
            {
                var deRefVar = _ssaTable.NewLinkAsDeRef(newSSAVar);
                result.Add(Instruction.Store(RightExpr.Result as VariableLLVM, deRefVar));
            }

            return result;
        }
    }
}
