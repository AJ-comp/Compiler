using Parse.MiddleEnd.IR.Expressions;
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
    }
}
