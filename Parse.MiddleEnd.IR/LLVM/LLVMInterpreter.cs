using Parse.Extensions;
using Parse.MiddleEnd.IR.Datas;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using Parse.Types;
using System;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM
{
    public partial class LLVMInterpreter
    {
        public static Dictionary<IRFunction, int> NextIndexByFunction { get; } = new Dictionary<IRFunction, int>();


        public static string ToBitCode(IRProgramRoot root)
        {
            string result = string.Empty;

            foreach (var structDef in root.StructDefs) result += ToBitCode(structDef);
            result += Environment.NewLine;

            foreach (var function in root.Functions) result += ToBitCode(function) + Environment.NewLine;
            result += Environment.NewLine;

            return result;
        }


        /**********************************************************/
        /// <summary>
        /// %struct.{Name} = type { members }    <br/>
        /// members ex: <br/>
        /// int: i32    <br/>
        /// int[5]: [5 x i32]   <br/> 
        /// struct*[2]: [2 x %struct.B*]    <br/>
        /// </summary>
        /// <param name="structDef"></param>
        /// <returns></returns>
        /**********************************************************/
        public static string ToBitCode(IRStructDef structDef)
        {
            string result = $"%struct.{structDef.IRName} = type ";

            result += "{";
            result += structDef.Members.ItemsString(PrintType.Property, "LLVMTypeName");
            result += "}";
            result += Environment.NewLine;

            return result;
        }


        /**********************************************************/
        /// <summary>
        /// This function returns a data format as below <br/>
        /// IsGlobal is true: 
        /// <b><i>@{name} = common global {type} {initValue}, align {size} </i></b>
        /// <br/>
        /// IsGlobal is false: 
        /// <b><i>%{index} = alloca {type}, allign {size}</i></b> <br/>
        /// </summary>
        /// <param name="irVar"></param>
        /// <returns></returns>
        /**********************************************************/
        public static string ToBitCode(IRFunction function)
        {
            var llvmFunction = new LLVMFunction(function);
            string result = $"define dso_local {llvmFunction.ReturnType.LLVMTypeName} @{llvmFunction.IRName} ";

            // argument code
            result += "(";
            for (int i = 0; i < llvmFunction.Arguments.Count; i++)
            {
                var arg = llvmFunction.Arguments[i];

                var param = new LLVMNamedVar(arg);
                llvmFunction.AddVar(param);

                result += $"{param.TypeName} noundef %{arg.Name}";
                if (i < llvmFunction.Arguments.Count - 1) result += ", ";
            }
            result += ") #0";
            result += Environment.NewLine;

            // statement code
            result += "{" + Environment.NewLine;
            result += "entry:" + Environment.NewLine;

            // init argument
            for (int i = llvmFunction.Arguments.Count - 1; i >= 0; i--)
            {
                var arg = llvmFunction.Arguments[i];

                result += $"; initialize for parameter {i}" + Environment.NewLine;
                result += $"%{arg.Name}.addr = alloca {arg.Type.LLVMTypeName}, align {arg.Type.Size}" + Environment.NewLine;
                result += $"store {arg.Type.LLVMTypeName} %{arg.Name}, {arg.Type.LLVMTypeName}* %{arg.Name}.addr, align {arg.Type.Size}" + Environment.NewLine;
                result += Environment.NewLine;
            }

            result += ToBitCode(llvmFunction.Statement, llvmFunction);
            result += RetCode(llvmFunction);
            result += "}";
            result += Environment.NewLine;

            return result;
        }


        private static string RetCode(LLVMFunction function)
        {
            return "ret void" + Environment.NewLine;
        }

        public static string ToBitCode(IRStatement statement, LLVMFunction ownFunction)
        {
            if (statement is IRConditionStatement) return ToBitCode(statement as IRConditionStatement, ownFunction);
            else if (statement is IRCompoundStatement) return ToBitCode(statement as IRCompoundStatement, ownFunction);
            else if (statement is IRExprStatement) return ToBitCode(statement as IRExprStatement, ownFunction);
            else if (statement is IRRepeatStatement) return ToBitCode(statement as IRRepeatStatement, ownFunction);

            throw new Exception("There is no correct statement.");
        }


        public static string ToBitCode(IRConditionStatement statement, LLVMFunction ownFunction)
        {
            string result = string.Empty;

            result += ToBitCode(statement.Condition, ownFunction, new LLVMBuildOption(false));

            var cmpVar = ownFunction.GetRecentVar(LLVMVarType.CmpVar);
            var ifVar = LLVMVar.CreateLabelVar(LLVMVarType.IfVar);
            var elseVar = (statement.FalseStatement != null) ? LLVMVar.CreateLabelVar(LLVMVarType.IfElseVar) : null;
            var endVar = LLVMVar.CreateLabelVar(LLVMVarType.IfEndVar);
            ownFunction.AddVars(ifVar, elseVar, endVar);

            var elseOrEnd = (elseVar == null) ? endVar.NameInLLVMFunction : elseVar.NameInLLVMFunction;

            result += $"br {cmpVar.TypeName} {cmpVar.NameInLLVMFunction}, label {ifVar.NameInLLVMFunction}, label {elseOrEnd} {Environment.NewLine}";
            result += Environment.NewLine;
            result += $"{ifVar.NameInLLVMFunction} : \t\t\t ; {statement.Condition} is true {Environment.NewLine}";
            result += ToBitCode(statement.TrueStatement, ownFunction);
            result += $"br label {endVar.NameInLLVMFunction} {Environment.NewLine}";
            result += Environment.NewLine;

            if (elseVar != null)
            {
                result += $"{elseVar.NameInLLVMFunction} : \t\t\t ; {statement.Condition} is false {Environment.NewLine}";
                result += ToBitCode(statement.FalseStatement, ownFunction);
                result += $"br label {endVar.NameInLLVMFunction} {Environment.NewLine}";
                result += Environment.NewLine;
            }

            result += $"{endVar.NameInLLVMFunction} : \t\t\t ; cmp is end {Environment.NewLine}";

            return result;
        }


        private static string ToBitCode(IRVariable localVar, LLVMFunction ownFunction)
        {
            string result = string.Empty;

            var namedVar = new LLVMNamedVar(localVar);
            ownFunction.AddVar(namedVar);

            result += $"; {localVar}" + Environment.NewLine;
            result += $"{namedVar.NameInLLVMFunction} = alloca {localVar.Type.LLVMTypeName}, align {localVar.Type.Size}" + Environment.NewLine;

            if (localVar.Type.Type == StdType.Struct)
            {

            }
            else
            {
                if (localVar.InitValue is IRLiteralExpr)
                {
                    var literalVar = localVar.InitValue as IRLiteralExpr;

                    result += $"store {namedVar.TypeName} {literalVar.Value} {namedVar.TypeName}* {namedVar.NameInLLVMFunction}, align {namedVar.Size} {Environment.NewLine}";
                }
                else
                {
                    result += ToBitCode(localVar.InitValue, ownFunction, new LLVMBuildOption(true));
                    var exprVar = ownFunction.GetRecentVar();

                    result += $"store {namedVar.TypeName} {exprVar.NameInLLVMFunction} {namedVar.TypeName}* {namedVar.NameInLLVMFunction}, align {namedVar.Size} {Environment.NewLine}";
                }
            }
            return result + Environment.NewLine;
        }


        public static string ToBitCode(IRCompoundStatement statement, LLVMFunction ownFunction)
        {
            string result = string.Empty;

            foreach (var item in statement.Items)
            {
                if (item is IRVariable) result += ToBitCode(item as IRVariable, ownFunction);
                else result += ToBitCode(item as IRStatement, ownFunction);
            }

            return result;
        }


        public static string ToBitCode(IRExprStatement statement, LLVMFunction ownFunction)
        {
            string result = $"; {statement.Expr} {Environment.NewLine}";
            return result + ToBitCode(statement.Expr, ownFunction, new LLVMBuildOption(true));
        }


        public static string ToBitCode(IRExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            string result = string.Empty;

            if (expr is IRBinaryExpr) return ToBitCode(expr as IRBinaryExpr, ownFunction, option);
            else if (expr is IRCallExpr) return ToBitCode(expr as IRCallExpr, ownFunction, option);
            else if (expr is IRSingleExpr) return ToBitCode(expr as IRSingleExpr, ownFunction, option);
            else if (expr is IRLiteralExpr) return ToBitCode(expr as IRLiteralExpr, ownFunction, option);
            else if (expr is IRUseIdentExpr) return ToBitCode(expr as IRUseIdentExpr, ownFunction, option);

            throw new Exception("There is no correct expr.");
        }

        public static string ToBitCode(IRCallExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            return string.Empty;
        }

        public static string ToBitCode(IRSingleExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            string result = (option.NoComment) ? string.Empty : $"; {expr} {Environment.NewLine}";
            result += ToBitCode(expr.Items[0] as IRExpr, ownFunction, option);
            var recentVar = ownFunction.GetRecentVar(LLVMVarType.NormalVar);

            if (expr.Operation == IRSingleOperation.PostInc || expr.Operation == IRSingleOperation.PreInc ||
                expr.Operation == IRSingleOperation.PostDec || expr.Operation == IRSingleOperation.PreDec)
            {
                // postinc, postdec, preinc, predec has only IRUseIdentExpr
                // ex: a++ = ok, (a+b)++ = no
                var namedVar = ownFunction.GetNamedVar((expr.Items[0] as IRUseIdentExpr).Variable);
                var value = LLVMChecker.IsInc(expr.Operation) ? 1 : -1;
                var type = LLVMChecker.IsInc(expr.Operation) ? LLVMVarType.IncVar : LLVMVarType.DecVar;
                var incDecVar = new LLVMVar(type, expr.Type);
                ownFunction.AddVar(incDecVar);

                result += $"{incDecVar.NameInLLVMFunction} = add {incDecVar.TypeName} {recentVar.NameInLLVMFunction}, {value} {Environment.NewLine}";
                result += $"store {incDecVar.TypeName} {incDecVar.NameInLLVMFunction}, {incDecVar.TypeName}* {namedVar.NameInLLVMFunction}, align {recentVar.Size} {Environment.NewLine}";
            }

            return result;
        }

        public static string ToBitCode(IRLiteralExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            LLVMVar var = new LLVMLiteralVar(expr);
            ownFunction.AddVar(var);

            return string.Empty;
        }


        public static string ToBitCode(IRUseIdentExpr expr, LLVMFunction ownFunction, LLVMBuildOption option)
        {
            var namedVar = ownFunction.GetNamedVar(expr.Variable);
            var llvmVar = new LLVMVar(LLVMVarType.NormalVar, expr);
            ownFunction.AddVar(llvmVar);

            // namedVar.Typename == llvmVar.TypeName
            return $"{llvmVar.NameInLLVMFunction} = load {llvmVar.TypeName}, {llvmVar.TypeName}* {namedVar.NameInLLVMFunction}, align {namedVar.Size} {Environment.NewLine}";
        }
    }
}
