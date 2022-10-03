using Parse.Extensions;
using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.ExprExpressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using Parse.Types;
using System;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMInterpreter
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
            string result = $"define dso_local {function.ReturnType.LLVMTypeName} @{function.IRName} ";

            // argument code
            result += "(";
            for (int i = 0; i < function.Arguments.Count; i++)
            {
                var arg = function.Arguments[i];

                result += $"{arg.Type.LLVMTypeName} noundef %{arg.Name}";
                if (i < function.Arguments.Count - 1) result += ", ";
            }
            result += ") #0";
            result += Environment.NewLine;

            // statement code
            result += "{" + Environment.NewLine;
            result += "entry:" + Environment.NewLine;

            // init argument
            for (int i = function.Arguments.Count - 1; i >= 0; i--)
            {
                var arg = function.Arguments[i];

                result += $"; initialize for parameter {i}" + Environment.NewLine;
                result += $"%{arg.Name}.addr = alloca {arg.Type.LLVMTypeName}, align {arg.Type.Size}" + Environment.NewLine;
                result += $"store {arg.Type.LLVMTypeName} %{arg.Name}, {arg.Type.LLVMTypeName}* %{arg.Name}.addr, align {arg.Type.Size}" + Environment.NewLine;
                result += Environment.NewLine;
            }

            result += ToBitCode(function.Statement, function);
            result += RetCode(function);
            result += "}";
            result += Environment.NewLine;

            return result;
        }


        private static string RetCode(IRFunction function)
        {
            return "ret void" + Environment.NewLine;
        }

        public static string ToBitCode(IRStatement statement, IRFunction ownFunction)
        {
            if (statement is IRConditionStatement) return ToBitCode(statement as IRConditionStatement, ownFunction);
            else if (statement is IRCompoundStatement) return ToBitCode(statement as IRCompoundStatement, ownFunction);
            else if (statement is IRExprStatement) return ToBitCode(statement as IRExprStatement, ownFunction);
            else if (statement is IRRepeatStatement) return ToBitCode(statement as IRRepeatStatement, ownFunction);

            throw new Exception("There is no correct statement.");
        }


        public static string ToBitCode(IRCompoundStatement statement, IRFunction ownFunction)
        {
            string result = string.Empty;

            foreach (var localVar in statement.LocalVars)
            {
                result += $"; initialize for local var {localVar.Name}" + Environment.NewLine;
                result += $"%{localVar.Name} = alloca {localVar.Type.LLVMTypeName}, align {localVar.Type.Size}" + Environment.NewLine;
                if (localVar.Type.Type != StdType.Struct)
                {
                    result += $"%{ownFunction.VarIndex++} = load {localVar.Type.LLVMTypeName}, {localVar.Type.LLVMTypeName}* %{localVar.Name}, align {localVar.Type.Size}";
                    result += Environment.NewLine;
                }
                result += Environment.NewLine;
            }

            foreach (var expression in statement.Expressions)
            {
                result += ToBitCode(expression, ownFunction);
            }

            return result;
        }


        public static string ToBitCode(IRConditionStatement statement, IRFunction ownFunction)
        {
            string result = string.Empty;

            result += ToBitCode(statement.Condition, ownFunction);
            result += ToBitCode(statement.TrueStatement, ownFunction);
            if (statement.FalseStatement != null) result += ToBitCode(statement.FalseStatement, ownFunction);

            return result;
        }


        public static string ToBitCode(IRExpr expr, IRFunction ownFunction)
        {
            string result = string.Empty;

            if (expr is IRBinaryExpr) return ToBitCode(expr as IRBinaryExpr, ownFunction);
            else if (expr is IRCallExpr) return ToBitCode(expr as IRCallExpr, ownFunction);

            throw new Exception("There is no correct expr.");
        }


        public static string ToBitCode(IRBinaryExpr expr, IRFunction ownFunction)
        {
            string cmpType = (expr.Left.Type.IsIntegerType && expr.Right.Type.IsIntegerType) ? "icmp" : "fcmp";
            string unsign = (expr.Left.Type.IsUnsigned || expr.Right.Type.IsUnsigned) ? "u" : "s";
            string operation = LLVMConverter.GetInstructionName(expr.Operation);


            return $"%cmp{ownFunction.CmpVarIndex} = {cmpType} {unsign}{operation}";
        }
    }
}
