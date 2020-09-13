using Parse.MiddleEnd.IR;
using static Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables.VariableMiniC;

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
    }
}
