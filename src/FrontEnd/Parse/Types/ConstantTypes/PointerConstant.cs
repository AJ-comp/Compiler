using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.Types.ConstantTypes
{
    public class PointerConstant : Constant
    {
        public uint PointerLevel { get; }
        public override StdType TypeKind { get; }
        public override bool AlwaysTrue { get; }
        public override bool AlwaysFalse { get; }

        public PointerConstant(StdType typeKind, int value, State valueState, uint pointerLevel) : base(value, valueState)
        {
            TypeKind = typeKind;

            if (pointerLevel == 0) pointerLevel = 1;
            PointerLevel = pointerLevel;
        }

        public override Constant Casting(StdType to)
        {
            throw new NotImplementedException();
        }
    }
}
