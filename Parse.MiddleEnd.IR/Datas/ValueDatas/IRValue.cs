using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.MiddleEnd.IR.Datas.ValueDatas
{
    public interface IRValue : IRData, IValue
    {
        bool IsZero { get; }

        bool? IsEqual(IRValue t)
        {
            if (TypeName != t.TypeName) return false;
            if (Value != t.Value)
            {
                if (Value == null || t.Value == null) return null;
                else return false;
            }

            return true;
        }

        bool? IsGreaterThan(IRValue t)
        {
            if (Value == null || t.Value == null) return null;

            return ((double)Value > (double)t.Value);
        }

        bool? IsLessThan(IRValue t)
        {
            if (Value == null || t.Value == null) return null;

            return ((double)Value < (double)t.Value);
        }

        IRValue BinOp(IRValue t, IROperation oper)
        {
            return (oper == IROperation.Add) ? Add(t) :
                        (oper == IROperation.Sub) ? Sub(t) :
                        (oper == IROperation.Mul) ? Mul(t) :
                        (oper == IROperation.Div) ? Div(t) :
                        (oper == IROperation.Mod) ? Mod(t) : null;
        }


        IRValue Add(IRValue t);
        IRValue Sub(IRValue t);
        IRValue Mul(IRValue t);
        IRValue Div(IRValue t);
        IRValue Mod(IRValue t);
    }

    public interface IRValue<out T> : IRValue, IRData<T> where T : DataType
    {
    }
}
