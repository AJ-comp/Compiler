using Parse.Types.Operations;
using System;

namespace Parse.Types
{
    public enum DType 
    { 
        Unknown, 
        Bit, Byte, Short, Int, Double,
    }

    public interface IDataTypeSpec : ICompareOperation
    {
        public int Size { get; }

        public static DType GetTypeName(Type type)
        {
            if (type == typeof(IDouble)) return DType.Double;
            if (type == typeof(IInt)) return DType.Int;
            if (type == typeof(IShort)) return DType.Short;
            if (type == typeof(IByte)) return DType.Byte;
            if (type == typeof(IBit)) return DType.Bit;

            return DType.Unknown;
        }
    }
}
