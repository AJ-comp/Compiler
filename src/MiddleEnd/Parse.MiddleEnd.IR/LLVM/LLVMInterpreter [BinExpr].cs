using AJ.Common.Helpers;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM
{
    public partial class LLVMInterpreter
    {
        private static void Arithmetic(IRBinaryExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            var recentVars = new List<LLVMVar>();
            recentVars.AddRange(ownFunction.GetRecentVars(2));
            var maximumType = LLVMChecker.MaximumType(expr.Left.Type, expr.Right.Type);

            LLVMVarType type = (expr.Operation == IRBinaryOperation.Add) ? LLVMVarType.AddVar
                                        : (expr.Operation == IRBinaryOperation.Sub) ? LLVMVarType.SubVar
                                        : (expr.Operation == IRBinaryOperation.Mul) ? LLVMVarType.MulVar
                                        : (expr.Operation == IRBinaryOperation.Div) ? LLVMVarType.DivVar
                                        : (expr.Operation == IRBinaryOperation.Mod) ? LLVMVarType.RemVar
                                        : throw new Exception("");

            var newVar = new LLVMVar(type, maximumType);
            ownFunction.AddVar(newVar);
            ownFunction.Code.AddArithmetic(newVar, recentVars[0], recentVars[1], expr.Operation);
        }

        private static void Assign(IRBinaryExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            if (expr.Operation != IRBinaryOperation.Assign) throw new Exception();

            var namedVar = ownFunction.GetNamedVar((expr.Left as IRUseIdentExpr).Variable);
            var rightVar = ownFunction.GetRecentVar();
            //            var maximumType = LLVMChecker.MaximumType(expr.Left.Type, expr.Right.Type);

            ownFunction.Code.AddStore(namedVar, rightVar);
        }


        public static void ToBitCode(IRBinaryExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            if(!option.NoComment) ownFunction.Code.AddComment($"{expr.Left} {expr.Operation.ToDescription()} {expr.Right}");

            // assign category is not load left var
            if(expr.Operation != IRBinaryOperation.Assign)  ToBitCode(expr.Left, ownFunction, option);
            ToBitCode(expr.Right, ownFunction, option);

            var recentVars = new List<LLVMVar>();
            recentVars.AddRange(ownFunction.GetRecentVars(2));

            if (IRChecker.IsArithmetic(expr.Operation)) Arithmetic(expr, ownFunction, option);
            else if (expr.Operation == IRBinaryOperation.Assign) Assign(expr, ownFunction, option);
            else
            {
                if (expr.OnlyTrue || expr.OnlyFalse)
                {
                    ownFunction.Code.AddComment($"{expr} is always true or false so cmp logic is not genereated.");
                    return;
                }

                var cmpVar = LLVMVar.CreateCmpVar();
                ownFunction.AddVar(cmpVar);

                ownFunction.Code.AddCmp(expr, cmpVar, recentVars[0], recentVars[1]);
            }
        }
    }
}
