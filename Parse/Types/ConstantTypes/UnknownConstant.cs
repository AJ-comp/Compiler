using System;

namespace Parse.Types.ConstantTypes
{
    public class UnknownConstant : IConstant
    {
        public StdType TypeKind => StdType.Unknown;
        public object Value => throw new NotImplementedException();
        public State ValueState => State.Unknown;

        public UnknownConstant()
        {
        }

        public Constant Casting(StdType to)
        {
            throw new NotImplementedException();
        }
    }
}
