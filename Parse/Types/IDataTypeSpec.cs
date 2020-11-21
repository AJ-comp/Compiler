using Parse.Types.Operations;
using System;
using System.ComponentModel;

namespace Parse.Types
{
    public enum DType 
    { 
        [Description("Unknown")] Unknown, 
        [Description("bit")] Bit, 
        [Description("byte")] Byte, 
        [Description("short")] Short, 
        [Description("int")] Int, 
        [Description("double")] Double,
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
