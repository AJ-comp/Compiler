using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.Types.ConstantTypes
{
    public class EnumConstant : Constant, IEnum
    {
        public EnumConstant(object value, State valueState) : base(value, valueState)
        {
        }

        public override StdType TypeKind => StdType.Enum;
        public override bool AlwaysTrue => throw new NotImplementedException();
        public override bool AlwaysFalse => throw new NotImplementedException();

        public int Size => throw new NotImplementedException();

        public IConstant BitAnd(IConstant operand)
        {
            throw new NotImplementedException();
        }

        public IConstant BitOr(IConstant operand)
        {
            throw new NotImplementedException();
        }

        public IConstant Equal(IConstant operand)
        {
            throw new NotImplementedException();
        }

        public IConstant NotEqual(IConstant operand)
        {
            throw new NotImplementedException();
        }


        public override Constant Casting(StdType to)
        {
            throw new NotImplementedException();
        }
    }
}
