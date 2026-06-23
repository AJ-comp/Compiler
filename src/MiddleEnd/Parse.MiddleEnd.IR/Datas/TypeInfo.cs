using Parse.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Datas
{
    public class TypeInfo
    {
        public string Name { get; }
        public StdType Type { get; }
        public uint PointerLevel { get; }
        public List<int> ArrayLengths { get; } = new List<int>();


        public TypeInfo(StdType type, uint pointerLevel)
        {
            Type = type;
            PointerLevel = pointerLevel;
        }
    }
}
