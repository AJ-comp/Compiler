using Parse.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Parse.FrontEnd.AJ.Data
{
    public partial class ConstantAJ
    {
        public static ConstantAJ operator !(ConstantAJ source)
        {
            if (source.Type.DataType == AJDataType.Bool)
            {
                if (source.ValueState == State.Fixed) return new ConstantAJ(!(bool)source.Value);
                else return new ConstantAJ(AJDataType.Bool);
            }

            throw new Exception();
        }


        public ConstantAJ And(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.DataType == AJDataType.Bool)
            {
                if (state == State.Fixed) return new ConstantAJ((bool)Value && (bool)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }

        public ConstantAJ Or(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (target.Type.DataType == AJDataType.Bool)
            {
                if (state == State.Fixed) return new ConstantAJ((bool)Value || (bool)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }


        public ConstantAJ LeftShift(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (Type.IsIntegerType() && target.Type.IsIntegerType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value << (int)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }


        public ConstantAJ RightShift(ConstantAJ target)
        {
            var state = ValueStateAfterCalc(target.ValueState);

            if (Type.IsIntegerType() && target.Type.IsIntegerType())
            {
                if (state == State.Fixed) return new ConstantAJ((int)Value >> (int)target.Value);
                else return CreateValueUnknown(AJDataType.Bool);
            }

            throw new Exception();
        }
    }
}
