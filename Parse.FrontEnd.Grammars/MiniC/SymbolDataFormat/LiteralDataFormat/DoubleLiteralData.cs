using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System;
using System.Collections.Generic;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public class DoubleLiteralData : LiteralData, IRValue<DoubleType>
    {
        private double _value = 0;

        public override bool IsZero => throw new NotImplementedException();
        public override DType TypeName => throw new NotImplementedException();
        public override object Value => throw new NotImplementedException();
        public override bool Signed => throw new NotImplementedException();
        public override bool IsNan => throw new NotImplementedException();

        //        public double Value => (IsVirtual) ? _value : System.Convert.ToDouble(ValueToken?.Input);

        public DoubleLiteralData(object value, IReadOnlyList<TokenData> valueToken) : base(valueToken)
        {
        }

        public override IRValue Add(IRValue right)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue Div(IRValue right)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue Mod(IRValue right)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue Mul(IRValue right)
        {
            throw new System.NotImplementedException();
        }

        public override IRValue Sub(IRValue right)
        {
            throw new System.NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override IRValue<Bit> LogicalOp(IRValue t, IRCondition cond)
        {
            throw new NotImplementedException();
        }

        public override bool? IsEqual(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override bool? IsGreaterThan(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override bool? IsLessThan(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue BinOp(IRValue t, IROperation operation)
        {
            throw new NotImplementedException();
        }
    }
}
