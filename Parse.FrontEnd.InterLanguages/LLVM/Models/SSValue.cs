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

        public IRValue BinOp(IRValue t, IROperation operation)
        {
            throw new NotImplementedException();
        }

        public IRValue Div(IRValue t)
        {
            throw new NotImplementedException();
        }

        public bool? IsEqual(IRValue t)
        {
            throw new NotImplementedException();
        }

        public bool? IsGreaterThan(IRValue t)
        {
            throw new NotImplementedException();
        }

        public bool? IsLessThan(IRValue t)
        {
            throw new NotImplementedException();
        }

        public bool? IsNotEqual(IRValue t)
        {
            throw new NotImplementedException();
        }

        public IRValue<Bit> LogicalOp(IRValue t, IRCondition cond)
        {
            throw new NotImplementedException();
        }

        public IRValue Mod(IRValue t)
        {
            throw new NotImplementedException();
        }

        public IRValue Mul(IRValue t)
        {
            throw new NotImplementedException();
        }

        public IRValue Sub(IRValue t)
        {
            throw new NotImplementedException();
        }
    }

    public class SSValue<T> : SSValue, IRValue<T> where T : DataType
    {
        public override DType TypeName => DataType.GetTypeName(typeof(T));

        public override object Value { get; }
        public override bool Signed { get; }
        public override bool IsNan { get; }

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
    }
}
