using Parse.Extensions;
using Parse.MiddleEnd.IR.LLVM;
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

        public string LLVMTypeName => LLVMConverter.GetTypeName(this);

        public bool IsIntegerType
        {
            get
            {
                // yet, pointer type also is classified to integer type.
                if (PointerLevel > 0) return true;
                if (Type == StdType.Char || Type == StdType.UChar) return true;
                if (Type == StdType.Short || Type == StdType.UShort) return true;
                if (Type == StdType.Int || Type == StdType.UInt) return true;

                return false;
            }
        }


        public bool IsUnsigned
        {
            get
            {
                if (PointerLevel > 0) return true;

                if (Type == StdType.UChar) return true;
                if (Type == StdType.UShort) return true;
                if (Type == StdType.UInt) return true;

                return false;
            }
        }


        public IRType(StdType type, uint pointerLevel)
        {
            Type = type;
            PointerLevel = pointerLevel;

            if (PointerLevel > 0) Size = 8;
        }
    }
}
