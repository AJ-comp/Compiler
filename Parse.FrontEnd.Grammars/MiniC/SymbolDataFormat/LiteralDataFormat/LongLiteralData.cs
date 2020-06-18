using System;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public class LongLiteralData : DecidedLiteralData
    {
        private long _value = 0;

        public long Value => (IsVirtual) ? _value : System.Convert.ToInt64(ValueToken?.Input);

        public LongLiteralData(object value, TokenData valueToken) : base(valueToken)
        {
        }

        public override LiteralData Add(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Div(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mod(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Mul(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override LiteralData Sub(LiteralData right)
        {
            throw new System.NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
