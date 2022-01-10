using Parse.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public partial class ConstantAJ
    {
        public ConstantAJ Assign(ConstantAJ target)
        {
            if (Type.DataType == AJDataType.Bool) return BoolAssign(target);

            throw new Exception();
        }


        private ConstantAJ BoolAssign(ConstantAJ target)
        {
            if (target.Type.DataType == AJDataType.Bool)
            {
                if (target.ValueState == State.Fixed) return new ConstantAJ((bool)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }
    }
}
