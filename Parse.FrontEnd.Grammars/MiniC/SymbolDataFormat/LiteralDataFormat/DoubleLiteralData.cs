﻿using System;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public class DoubleLiteralData : DecidedLiteralData
    {
        public DoubleLiteralData(object value, TokenData valueToken) : base(valueToken)
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
