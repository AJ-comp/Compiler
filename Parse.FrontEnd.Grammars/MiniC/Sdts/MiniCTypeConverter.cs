using Parse.FrontEnd.Grammars.MiniC.Sdts.Datas;
using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.FrontEnd.Grammars.MiniC
{
    internal class MiniCTypeConverter
    {
        public static DType ToIRDataType(MiniCDataType dataType)
        {
            DType result = DType.Unknown;

            if (dataType == MiniCDataType.Int)
                result = DType.Int;

            return result;
        }

        public static MiniCDataType ToMiniCDataType(string data)
        {
            MiniCDataType result = MiniCDataType.Unknown;

            if (data == MiniCGrammar.Void.Value) result = MiniCDataType.Void;
            else if (data == MiniCGrammar.Int.Value) result = MiniCDataType.Int;

            return result;
        }
    }
}
