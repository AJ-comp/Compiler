using Parse.Types.Operations;
using System.ComponentModel;

namespace Parse.Types
{
    public enum StdType 
    { 
        [Description("error")] Error,
        [Description("?")] Unknown,
        [Description("void")] Void,
        [Description("bit")] Bit, 
        [Description("byte")] Byte,
        [Description("sbyte")] SByte,
        [Description("short")] Short,
        [Description("ushort")] UShort,
        [Description("int")] Int,
        [Description("uint")] UInt,
        [Description("double")] Double,
        [Description("string")] String,
        [Description("struct")] Struct,
        [Description("enum")] Enum,
    }

    public interface IDataTypeSpec : IEqualOperation
    {
        public int Size { get; }
    }
}
