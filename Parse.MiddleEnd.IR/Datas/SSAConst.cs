using Parse.Types;
using Parse.Types.ConstantTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.MiddleEnd.IR.Datas
{
    public class SSAConst : ISSAForm, IConstant
    {
        public StdType TypeKind { get; }
        public object Value { get; }
        public State ValueState { get; }

        public SSAConst(IConstant constValue)
        {
            TypeKind = constValue.TypeKind;
            Value = constValue.Value;
            ValueState = constValue.ValueState;
        }


        public Constant Casting(StdType to)
        {
            throw new NotImplementedException();
        }
    }
}
