using Parse.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Datas
{
    public class IRType
    {
        public int Id { get; }
        public StdType Type { get; set; }
        public string Name { get; set; }
        public uint PointerLevel { get; set; }
        public uint Size { get; set; }
        public bool Signed { get; set; }
        public bool Nan { get; set; }
        public List<int> ArrayLength { get; set; } = new List<int>();


        public string LLVMTypeName
        {
            get
            {
                string result = string.Empty;

                if (Type == StdType.Void) result = "void";
                else if (Type == StdType.Bit) result = "i1";
                else if (Type == StdType.Char) result = "i8";
                else if (Type == StdType.Short) result = "i16";
                else if (Type == StdType.Int) result = "i32";
                else if (Type == StdType.Double) result = "double";
                else if (Type == StdType.Struct) result = $"%struct.{Name}";

                return result;
            }
        }


        public IRType(StdType type, uint pointerLevel)
        {
            Type = type;
            PointerLevel = pointerLevel;
        }
    }
}
