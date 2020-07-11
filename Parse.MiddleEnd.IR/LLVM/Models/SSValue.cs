using Parse.MiddleEnd.IR.Datas.Types;
using Parse.MiddleEnd.IR.Datas.ValueDatas;
using System;

namespace Parse.MiddleEnd.IR.LLVM.Models
{
    public abstract class SSValue : IRValue
    {
        public bool IsZero => throw new NotImplementedException();

        public abstract DType TypeName { get; }
        public abstract object Value { get; }
        public abstract bool Signed { get; }
        public abstract bool IsNan { get; }

        public abstract IRValue Add(IRValue t);
        public abstract IRValue Div(IRValue t);
        public abstract IRValue Mod(IRValue t);
        public abstract IRValue Mul(IRValue t);
        public abstract IRValue Sub(IRValue t);
    }

    public class SSValue<T> : SSValue, IRValue<T> where T : DataType
    {
        public override DType TypeName => DataType.GetTypeName(typeof(T));

        public override object Value { get; }
        public override bool Signed { get; }
        public override bool IsNan { get; }

        public SSValue()
        {
            Value = null;
        }

        public SSValue(object value)
        {
            Value = value;
        }


        public static SSValue FromIRValue(IRValue irValue)
        {
            return new SSValue<T>(irValue.Value);
        }

        public override IRValue Add(IRValue t)
        {
            if (this.TypeName == DType.Double || t.TypeName == DType.Double)
                return new SSValue<DoubleType>((double)Value + (double)t.Value);
            else
                return new SSValue<Int>((int)Value + (int)t.Value);
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
}
