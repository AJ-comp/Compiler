using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.MiddleEnd.IR.Datas.ValueDatas
{
    public interface IRValue : IRData, IValue
    {
        bool IsZero { get; }

        IRValue<Bit> LogicalOp(IRValue t, IRCondition cond);

        bool? IsEqual(IRValue t);
        bool? IsGreaterThan(IRValue t);
        //{
        //    // if it is compared by maximum type then problem doesn't exist because data loss is not fired.
        //    return ((double)Value > (double)t.Value);
        //}
        bool? IsLessThan(IRValue t);
        //{
        //    // if it is compared by maximum type then problem doesn't exist because data loss is not fired.
        //    return ((double)Value < (double)t.Value);
        //}

        IRValue BinOp(IRValue t, IROperation oper)
        {
            if (oper == IROperation.Add)
                return Add(t);

            return null;
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
