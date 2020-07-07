using Parse.MiddleEnd.IR;
using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System;

namespace Parse.FrontEnd.Grammars.MiniC.SymbolDataFormat.LiteralDataFormat
{
    public class BitLiteralData : LiteralData, IRValue<Bit>
    {
        public BitLiteralData(object value)
        {
            Value = value;
        }

        public override bool IsZero => ((bool)Value == false);
        public override DType TypeName => DType.Bit;
        public override object Value { get; }
        public override bool Signed => false;
        public override bool IsNan => false;

        public override IRValue Add(IRValue t)
        {
            throw new NotImplementedException();
        }

        public override IRValue BinOp(IRValue t, IROperation operation)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override IRValue Div(IRValue t)
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

        public override IRValue<Bit> LogicalOp(IRValue t, IRCondition cond)
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
}
