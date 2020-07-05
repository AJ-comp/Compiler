using Parse.FrontEnd.Grammars.MiniC.SymbolTableFormat;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars
{
    public class IRConverter
    {
        public static ReturnType ToIRReturnType(DataType returnType)
        {
            ReturnType result = ReturnType.Void;

            if (returnType == DataType.Void) result = ReturnType.Void;
            else if (returnType == DataType.Int) result = ReturnType.i32;

            return result;
        }

        public static IRFuncData ToIRData(FuncData funcData)
        {
            List<IRVar> paramVars = new List<IRVar>();

            foreach (var param in funcData.ParamVars)
            {
                paramVars.Add(param);
            }

            return new IRFuncData(paramVars,
                                                funcData.DclSpecData.Const,
                                                IRConverter.ToIRReturnType(funcData.DclSpecData.DataType),
                                                funcData.Name
                                                );
        }
    }
}
