using Parse.FrontEnd.Grammars.Properties;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat
{
    public enum UnknownState { NotInitialized = 1, DynamicAllocation = 2 }

    public class LiteralData : ValueData, ICloneable<LiteralData>
    {
        public string LiteralName => ValueToken.Input;

        public MeaningErrInfoList MeaningErrs
        {
            get
            {
                MeaningErrInfoList result = new MeaningErrInfoList();

                if (IsUnknown)
                {
                    if (IsOnlyNotInit)
                        result.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0005),
                                                                        string.Format(AlarmCodes.MCL0005, LiteralName),
                                                                        ErrorType.Error));
                    else if (IsNotInitAndDynamicAlloc)
                        result.Add(new MeaningErrInfo(nameof(AlarmCodes.MCL0005),
                                                                        string.Format(AlarmCodes.MCL0005, LiteralName),
                                                                        ErrorType.Warning));
                }

                return result;
            }
        }

        public TokenData ValueToken { get; }

        public bool IsVirtual { get; protected set; } = false;
        public override bool IsZero => throw new NotImplementedException();
        public override DType TypeName => DType.Unknown;
        public override object Value => throw new NotImplementedException();
        public override bool Signed => false;
        public override bool IsNan => false;

        protected LiteralData(TokenData valueToken)
        {
            ValueToken = valueToken;
        }

        public virtual LiteralData Clone()
        {
            return null;
        }

        public override IRValue Add(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Div(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Mod(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Mul(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Sub(IRValue t)
        {
            throw new NotImplementedException();
        }
    }








    public class LiteralData<T> : LiteralData, IRValue<T> where T : DataType
    {
        public LiteralData(TokenData tokenList, object value) : base(tokenList)
        {
            Value = value;
        }

        public LiteralData(TokenData tokenList, bool isNan) : base(tokenList)
        {
            IsNan = isNan;
        }

        public LiteralData(object value) : base(null)
        {
            Value = value;
            IsVirtual = true;
        }

        public override bool IsZero { get; }
        public override DType TypeName => DataType.GetTypeName(typeof(T));
        public override object Value { get; }
        public override bool Signed { get; }
        public override bool IsNan { get; }

        public override IRValue Add(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override LiteralData Clone() => (IsVirtual) ? new LiteralData<T>(Value) : new LiteralData<T>(ValueToken);

        public override IRValue Div(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Mod(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Mul(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue Sub(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override string ToString() => Value.ToString();
    }
}
