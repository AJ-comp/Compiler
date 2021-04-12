using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.Types.ConstantTypes
{
    public class ErrorConstant : IConstant
    {
        public StdType TypeKind => StdType.Error;
        public object Value => throw new NotImplementedException();
        public State ValueState => throw new NotImplementedException();

        public Constant Casting(StdType to)
        {
            throw new NotImplementedException();
        }
    }
}
