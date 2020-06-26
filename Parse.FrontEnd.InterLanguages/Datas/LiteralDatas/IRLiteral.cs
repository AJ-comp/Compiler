using Parse.FrontEnd.InterLanguages.LLVM.Models;

namespace Parse.FrontEnd.InterLanguages.Datas
{
    public abstract class IRLiteral : IRData
    {
        public bool IsZero => (double)Value == 0;

        public abstract object Value { get; }
        public abstract DataType Type { get; }
        public abstract bool IsSigned { get; }
        public abstract bool IsNan { get; }

        public static DataType GreaterType(IRLiteral t1, IRLiteral t2)
        {
            var t1TypeSize = IRConverter.ToAlign(t1.Type);
            var t2TypeSize = IRConverter.ToAlign(t2.Type);

            return (t1TypeSize >= t2TypeSize) ? t1.Type : t2.Type;
        }


        public Integer LogicalOp(IRLiteral t, IRCondition cond)
        {
            Integer result = null;
            if (cond == IRCondition.EQ) result = IsEqual(t);
            else if (cond == IRCondition.NE) result = IsNotEqual(t);
            else if (cond == IRCondition.GT) result = IsGreaterThan(t);
            else if (cond == IRCondition.LE) result = IsLessThan(t);

            return result;
        }

        public Integer IsEqual(IRLiteral t)
        {
            var result = (Value == t.Value);

            return new Integer(result);
        }

        public Integer IsNotEqual(IRLiteral t)
        {
            var result = (Value != t.Value);

            return new Integer(result);
        }

        public Integer IsGreaterThan(IRLiteral t)
        {
            // if it is compared by maximum type then problem doesn't exist because data loss is not fired.
            var result = ((double)Value > (double)t.Value);

            return new Integer(result);
        }

        public Integer IsLessThan(IRLiteral t)
        {
            // if it is compared by maximum type then problem doesn't exist because data loss is not fired.
            var result = ((double)Value < (double)t.Value);

            return new Integer(result);
        }


        public IRLiteral BinOp(IRLiteral t, IROperation operation)
        {
            IRLiteral result = null;
            if (operation == IROperation.Add) result = Add(t);
            else if (operation == IROperation.Sub) result = Sub(t);
            else if (operation == IROperation.Mul) result = Mul(t);
            else if (operation == IROperation.Div) result = Div(t);
            else if (operation == IROperation.Mod) result = Mod(t);

            return result;
        }

        public abstract IRLiteral Add(IRLiteral t);
        public abstract IRLiteral Sub(IRLiteral t);
        public abstract IRLiteral Mul(IRLiteral t);
        public abstract IRLiteral Div(IRLiteral t);
        public abstract IRLiteral Mod(IRLiteral t);
    }
}
