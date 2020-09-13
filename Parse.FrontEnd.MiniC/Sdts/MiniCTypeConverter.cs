using Parse.Types;
using static Parse.FrontEnd.Grammars.MiniC.Sdts.Datas.Variables.VariableMiniC;

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

        public static MiniCDataType ToMiniCDataType(IDataTypeSpec dataType)
        {
            MiniCDataType result = MiniCDataType.Unknown;

            if (dataType is IInt) result = MiniCDataType.Int;

            return result;
        }

        public static MiniCDataType ToMiniCDataType(DType dType)
        {
            MiniCDataType result = MiniCDataType.Unknown;

            if (dType == DType.Bit) result = MiniCDataType.Int;
            else if (dType == DType.Byte) result = MiniCDataType.Int;
            else if (dType == DType.Short) result = MiniCDataType.Int;
            else if (dType == DType.Int) result = MiniCDataType.Int;
            else if (dType == DType.Double) result = MiniCDataType.Int;

            return result;
        }
    }
}
