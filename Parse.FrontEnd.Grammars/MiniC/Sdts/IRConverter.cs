using Parse.FrontEnd.Grammars.MiniC.Sdts.AstNodes;
using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.Sdts
{
    public class IRConverter
    {
        public static ReturnType ToIRReturnType(MiniCDataType returnType)
        {
            ReturnType result = ReturnType.Void;

            if (returnType == MiniCDataType.Void) result = ReturnType.Void;
            else if (returnType == MiniCDataType.Int) result = ReturnType.i32;

            return result;
        }

        public static IRFuncData ToIRData(FuncDefNode funcData)
        {
            List<IRVar> paramVars = new List<IRVar>();

            foreach (var varData in funcData.SymbolTable.AllVarList)
            {
                if (varData.Etc == Datas.EtcInfo.Param) paramVars.Add(varData);
            }

            FuncHeadNode funcHead = funcData.FuncHead;

            return new IRFuncData(paramVars,
                                                funcHead.ReturnType.Const,
                                                IRConverter.ToIRReturnType(funcHead.ReturnType.DataType),
                                                funcHead.Name
                                                );
        }
    }
}
