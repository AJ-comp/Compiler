using Parse.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public partial class ConstantAJ
    {
        public ConstantAJ PreInc()
        {
            if (Type.IsIntegerType())
            {
                if (ValueState == State.Fixed) return new ConstantAJ((int)Value + 1);
                else return CreateValueUnknown(AJDataType.Int);
            }

            throw new Exception();
        }

        public ConstantAJ PostInc()
        {
            if (Type.IsIntegerType())
            {
                if (ValueState == State.Fixed) return new ConstantAJ((int)Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            throw new Exception();
        }

        public ConstantAJ PreDec()
        {
            if (Type.IsIntegerType())
            {
                if (ValueState == State.Fixed) return new ConstantAJ((int)Value - 1);
                else return CreateValueUnknown(AJDataType.Int);
            }

            throw new Exception();
        }

        public ConstantAJ PostDec()
        {
            if (Type.IsIntegerType())
            {
                if (ValueState == State.Fixed) return new ConstantAJ((int)Value);
                else return CreateValueUnknown(AJDataType.Int);
            }

            throw new Exception();
        }
    }
}
