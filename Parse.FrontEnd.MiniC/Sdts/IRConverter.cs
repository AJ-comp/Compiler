using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables;
using Parse.MiddleEnd.IR;

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
