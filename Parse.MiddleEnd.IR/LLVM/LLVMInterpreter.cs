using Parse.MiddleEnd.IR.Expressions;
using Parse.MiddleEnd.IR.Expressions.StmtExpressions;
using Parse.Types;
using System;
using System.Collections.Generic;

namespace Parse.MiddleEnd.IR.LLVM
{
    public class LLVMInterpreter
    {
        public static Dictionary<IRFunction, int> NextIndexByFunction { get; } = new Dictionary<IRFunction, int>();


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
            string result = $"define {function.ReturnType.LLVMTypeName} @{function.Name} ";

            // argument code
            result += "(";
            for (int i = 0; i < function.Arguments.Count; i++)
            {
                var arg = function.Arguments[i];

                result += $"{arg.Type.LLVMTypeName} %{arg.Name}";
                if (i < function.Arguments.Count - 1) result += ", ";
            }
            result += ")";
            result += Environment.NewLine;

            // statement code
            result += "{";
            result += ToBitCode(function.Statement);
            result += "}";

            return result;
        }


        public static string ToBitCode(IRCompoundStatement statement)
        {
            return string.Empty;
        }
    }
}
