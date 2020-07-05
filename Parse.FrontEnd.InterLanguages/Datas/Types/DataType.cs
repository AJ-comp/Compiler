using System;

namespace Parse.MiddleEnd.IR.Datas.Types
{
    public enum DType { Unknown, Bit, Byte, Short, Int, Double }

    public abstract class DataType
    {
        public abstract int Size { get; }

        public static DType GetTypeName(Type type)
        {
            if (type == typeof(DoubleType)) return DType.Double;
            else if (type == typeof(Int)) return DType.Int;
            else if (type == typeof(Short)) return DType.Short;
            else if (type == typeof(Byte)) return DType.Byte;
            else if (type == typeof(Bit)) return DType.Bit;

            return DType.Unknown;
        }
    }
}
