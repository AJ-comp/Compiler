using Parse.Extensions;
using System;
using System.Diagnostics;

namespace Parse.Types.ConstantTypes
{
    [DebuggerDisplay("{DebuggerDisplay, nq}")]
    public class NotInitConstant : IConstant
    {
        public StdType TypeKind { get; }
        public object Value => throw new NotImplementedException();
        public State ValueState => State.NotInit;


        public NotInitConstant(StdType typeKind)
        {
            TypeKind = typeKind;
        }

        public Constant Casting(StdType to)
        {
            throw new NotImplementedException();
        }


        private string DebuggerDisplay
            => $"Type : {TypeKind.ToDescription()}, Not Initialized";
    }
}
