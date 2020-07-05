using Parse.MiddleEnd.IR.Datas.Types;

namespace Parse.MiddleEnd.IR.Datas.ValueDatas
{
    public interface IRValue : IRData, IValue
    {
        bool IsZero { get; }

        IRValue<Bit> LogicalOp(IRValue t, IRCondition cond);

        bool? IsEqual(IRValue t);
        bool? IsNotEqual(IRValue t);
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
        IRValue BinOp(IRValue t, IROperation operation);
        //{
        //    IRValue result = null;
        //    if (operation == IROperation.Add) result = Add(t);
        //    else if (operation == IROperation.Sub) result = Sub(t);
        //    else if (operation == IROperation.Mul) result = Mul(t);
        //    else if (operation == IROperation.Div) result = Div(t);
        //    else if (operation == IROperation.Mod) result = Mod(t);

        //    return result;
        //}

        IRValue Add(IRValue t);
        IRValue Sub(IRValue t);
        IRValue Mul(IRValue t);
        IRValue Div(IRValue t);
        IRValue Mod(IRValue t);
    }

    public interface IRValue<out T> : IRValue where T : DataType
    {
    }
}
