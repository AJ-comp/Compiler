using Parse.FrontEnd.MiniC.Sdts.Datas.Variables;
using Parse.Types;

namespace Parse.FrontEnd.MiniC
{
    internal class MiniCTypeConverter
    {
        public static StdType ToStdDataType(MiniCDataType dataType)
        {
            StdType result = StdType.Unknown;

            if (dataType == MiniCDataType.Void) result = StdType.Void;
            else if (dataType == MiniCDataType.Int) result = StdType.Int;

            return result;
        }

        public static MiniCDataType ToMiniCDataType(string data)
        {
            MiniCDataType result = MiniCDataType.Unknown;

            if (data == MiniCGrammar.Void.Value) result = MiniCDataType.Void;
            else if (data == MiniCGrammar.Int.Value) result = MiniCDataType.Int;
            else if (data == MiniCGrammar.Address.Value) result = MiniCDataType.Address;

            return result;
        }

        public static MiniCDataType ToMiniCDataType(IDataTypeSpec dataType)
        {
            MiniCDataType result = MiniCDataType.Unknown;

            if (dataType is IInt) result = MiniCDataType.Int;

            return result;
        }

        public static MiniCDataType ToMiniCDataType(StdType dType)
        {
            MiniCDataType result = MiniCDataType.Unknown;

            if (dType == StdType.Bit) result = MiniCDataType.Int;
            else if (dType == StdType.Byte) result = MiniCDataType.Int;
            else if (dType == StdType.Short) result = MiniCDataType.Int;
            else if (dType == StdType.Int) result = MiniCDataType.Int;
            else if (dType == StdType.Double) result = MiniCDataType.Int;

            return result;
        }
    }
}
