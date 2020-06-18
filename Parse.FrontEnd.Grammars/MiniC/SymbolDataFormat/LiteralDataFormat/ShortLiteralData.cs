using System;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public class ShortLiteralData : DecidedLiteralData
    {
        private short _value = 0;

        public short Value => (IsVirtual) ? _value : System.Convert.ToInt16(ValueToken?.Input);

        public ShortLiteralData(object value, TokenData valueToken) : base(valueToken)
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
