using AJ.Common.Helpers;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.LLVM
{
    public partial class LLVMInterpreter
    {
        private static string Add(IRBinaryExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            if (expr.Operation != IRBinaryOperation.Add) throw new Exception();

            var recentVars = new List<LLVMVar>();
            recentVars.AddRange(ownFunction.GetRecentVars(2));
            var maximumType = LLVMChecker.MaximumType(expr.Left.Type, expr.Right.Type);

            var newVar = new LLVMVar(LLVMVarType.AddVar, maximumType);
            ownFunction.AddVar(newVar);

            return $"{newVar.NameInLLVMFunction} add {newVar.TypeName} {recentVars[0].NameInLLVMFunction}, {recentVars[1].NameInLLVMFunction} {Environment.NewLine}";
        }


        private static string Sub(IRBinaryExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            if (expr.Operation != IRBinaryOperation.Sub) throw new Exception();

            var recentVars = new List<LLVMVar>();
            recentVars.AddRange(ownFunction.GetRecentVars(2));
            var maximumType = LLVMChecker.MaximumType(expr.Left.Type, expr.Right.Type);

            var newVar = new LLVMVar(LLVMVarType.SubVar, maximumType);
            ownFunction.AddVar(newVar);

            return $"{newVar.NameInLLVMFunction} sub {newVar.TypeName} {recentVars[0].NameInLLVMFunction}, {recentVars[1].NameInLLVMFunction} {Environment.NewLine}";
        }


        private static string Assign(IRBinaryExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            if (expr.Operation != IRBinaryOperation.Assign) throw new Exception();

            var namedVar = ownFunction.GetNamedVar((expr.Left as IRUseIdentExpr).Variable);
            var rightVar = ownFunction.GetRecentVar();
//            var maximumType = LLVMChecker.MaximumType(expr.Left.Type, expr.Right.Type);

            return $"store {namedVar.TypeName} {rightVar.NameInLLVMFunction} {namedVar.TypeName}* {namedVar.NameInLLVMFunction}, align {namedVar.Size} {Environment.NewLine}";
        }


        public static string ToBitCode(IRBinaryExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            string result = option.NoComment ? string.Empty : $"; {expr.Left} {expr.Operation.ToDescription()} {expr.Right} {Environment.NewLine}";

            // assign category is not load left var
            if(expr.Operation != IRBinaryOperation.Assign)  result += ToBitCode(expr.Left, ownFunction, option);
            result += ToBitCode(expr.Right, ownFunction, option);

            var recentVars = new List<LLVMVar>();
            recentVars.AddRange(ownFunction.GetRecentVars(2));
            var maximumType = LLVMChecker.MaximumType(expr.Left.Type, expr.Right.Type);

            if (expr.Operation == IRBinaryOperation.Add) result += Add(expr, ownFunction, option);
            else if (expr.Operation == IRBinaryOperation.Sub) result += Sub(expr, ownFunction, option);
            else if (expr.Operation == IRBinaryOperation.Assign) result += Assign(expr, ownFunction, option);
            else
            {
                string cmpType = (expr.Left.Type.IsIntegerType && expr.Right.Type.IsIntegerType) ? "icmp" : "fcmp";
                string unsign = (expr.Left.Type.IsUnsigned || expr.Right.Type.IsUnsigned) ? "u" : "s";
                string operation = LLVMConverter.GetInstructionName(expr.Operation);

                var cmpVar = LLVMVar.CreateCmpVar();
                ownFunction.AddVar(cmpVar);

                result += $"{cmpVar.NameInLLVMFunction} = {cmpType} {unsign}{operation} {maximumType.LLVMTypeName} " +
                        $"{recentVars[0].NameInLLVMFunction}, {recentVars[1].NameInLLVMFunction} {Environment.NewLine}";
            }

            return result;
        }
    }
}
