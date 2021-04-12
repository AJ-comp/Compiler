using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.Types.ConstantTypes
{
    public class PointerConstant : IConstant
    {
        public StdType TypeKind { get; }
        public object Value { get; }
        public State ValueState { get; }
        public uint PointerLevel { get; }

        public Constant Casting(StdType to)
        {
            throw new NotImplementedException();
        }

        public PointerConstant(StdType typeKind, int value, uint pointerLevel)
        {
            TypeKind = typeKind;
            Value = value;

            if (pointerLevel == 0) pointerLevel = 1;
            PointerLevel = pointerLevel;
        }
    }
}
