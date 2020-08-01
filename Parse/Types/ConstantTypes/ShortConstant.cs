using System;

namespace Parse.Types.ConstantTypes
{
    public class ShortConstant : IntegerConstant, IShort
    {
        public ShortConstant(short value) : base(value)
        {
        }

        public ShortConstant(ushort value) : base(value)
        {
            Signed = false;
        }

        public ShortConstant(uint pointerLevel, short value) : base(pointerLevel, value)
        {
        }

        public ShortConstant(short value, State valueState, uint pointerLevel) : base(value, valueState, pointerLevel)
        {
        }

        public ShortConstant(ShortConstant t) : base((short)t.Value, t.ValueState, t.PointerLevel)
        {
        }

        public override int Size => 16;
        public override DType TypeName => DType.Short;
    }
}
