﻿using Parse.FrontEnd.AJ.Properties;
using Parse.Types;
using Parse.Types.ConstantTypes;
using System.Collections.Generic;

namespace Parse.FrontEnd.AJ.Data
{
    public partial class ConstantAJ : ISymbolData, IHasType
    {
        public int Id { get; set; }
        public AJTypeInfo Type { get; set; }
        public TokenData Token { get; set; }

        public int Block { get; set; }
        public int Offset { get; set; }

        public object Value { get; set; }
        public State ValueState { get; set; }
        public List<MeaningErrInfo> Alarms { get; } = new List<MeaningErrInfo>();

        public bool IsUnknown => ValueState == State.Unknown;

        public ConstantAJ(bool value) : this(value, State.Fixed)
        {
            Type = new AJTypeInfo(AJDataType.Bool);
            Type.Signed = false;
        }

        public ConstantAJ(byte value) : this(value, State.Fixed)
        {
            Type = new AJTypeInfo(AJDataType.Byte);
            Type.Signed = false;
        }

        public ConstantAJ(sbyte value) : this(value, State.Fixed)
        {
            Type = new AJTypeInfo(AJDataType.Byte);
        }

        public ConstantAJ(short value) : this(value, State.Fixed)
        {
            Type = new AJTypeInfo(AJDataType.Short);
        }

        public ConstantAJ(ushort value) : this(value, State.Fixed)
        {
            Type = new AJTypeInfo(AJDataType.Short);
            Type.Signed = false;
        }

        public ConstantAJ(int value) : this(value, State.Fixed)
        {
            Type = new AJTypeInfo(AJDataType.Int);
        }

        public ConstantAJ(uint value) : this(value, State.Fixed)
        {
            Type = new AJTypeInfo(AJDataType.Int);
            Type.Signed = false;
        }

        public ConstantAJ(double value) : this(value, State.Fixed)
        {
            Type = new AJTypeInfo(AJDataType.Double);
        }

        public ConstantAJ(string value) : this(value, State.Fixed)
        {
            Type = new AJTypeInfo(AJDataType.String);
        }


        public static ConstantAJ CreateTypeUnknown()
        {
            var result = new ConstantAJ();
            result.Type = new AJTypeInfo(AJDataType.Unknown);

            result.Value = null;
            result.ValueState = State.Unknown;

            return result;
        }

        public static ConstantAJ CreateValueUnknown(AJDataType type)
        {
            var result = new ConstantAJ();
            result.Type = new AJTypeInfo(type);

            result.Value = null;
            result.ValueState = State.Unknown;

            return result;
        }


        private ConstantAJ()
        {
        }

        private ConstantAJ(object value, State valueState = State.Fixed)
        {
            Value = value;
            ValueState = valueState;
        }


        private State ValueStateAfterCalc(State target)
        {
            if (ValueState == State.Unknown || target == State.Unknown) return State.Unknown;
            if (ValueState == State.Error || target == State.Error) return State.Error;

            return State.Fixed;
        }
    }
}