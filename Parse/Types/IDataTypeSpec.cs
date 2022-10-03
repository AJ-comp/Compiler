using Parse.Types.Operations;
using System.ComponentModel;

namespace Parse.Types
{
    public enum StdType
    {
        [Description("error")] Error,
        [Description("?")] Unknown,
        [Description("null")] Null,
        [Description("void")] Void,
        [Description("bool")] Bit, 
        [Description("char")] Char,
        [Description("unsigned char")] UChar,
        [Description("short")] Short,
        [Description("unsigned short")] UShort,
        [Description("int")] Int,
        [Description("unsigned int")] UInt,
        [Description("float")] Float,
        [Description("double")] Double,
        [Description("struct")] Struct,
        [Description("enum")] Enum,
    }

    public interface IDataTypeSpec : IEqualOperation
    {
        public int Size { get; }
    }
}
