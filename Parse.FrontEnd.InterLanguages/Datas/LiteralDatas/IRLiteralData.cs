using Parse.FrontEnd.InterLanguages.LLVM.Models;

namespace Parse.FrontEnd.InterLanguages.Datas
{
    public abstract class IRLiteralData : IRData
    {
        public abstract object Value { get; }

        public static DataType GreaterType(IRLiteralData t1, IRLiteralData t2)
        {
            var t1TypeSize = IRConverter.ToAlign(t1.Type);
            var t2TypeSize = IRConverter.ToAlign(t2.Type);

            return (t1TypeSize >= t2TypeSize) ? t1.Type : t2.Type;
        }


        public DecidedCondVar LogicalOp(IRLiteralData t, IRCondition cond)
        {
            DecidedCondVar result = null;
            if (cond == IRCondition.EQ) result = IsEqual(t);
            else if (cond == IRCondition.NE) result = IsNotEqual(t);
            else if (cond == IRCondition.SGT) result = IsGreaterThan(t);
            else if (cond == IRCondition.UGT) result = IsGreaterThan(t);
            else if (cond == IRCondition.SLE) result = IsLessThan(t);
            else if (cond == IRCondition.ULE) result = IsLessThan(t);

            return result;
        }

        public DecidedCondVar IsEqual(IRLiteralData t)
        {
            var result = (Value == t.Value) ? true : false;

            return new DecidedCondVar(result);
        }

        public DecidedCondVar IsNotEqual(IRLiteralData t)
        {
            var result = (Value != t.Value) ? true : false;

            return new DecidedCondVar(result);
        }

        public DecidedCondVar IsGreaterThan(IRLiteralData t)
        {
            // if it is compared by maximum type then problem doesn't exist because data loss is not fired.
            var result = ((double)Value > (double)t.Value) ? true : false;

            return new DecidedCondVar(result);
        }

        public DecidedCondVar IsLessThan(IRLiteralData t)
        {
            // if it is compared by maximum type then problem doesn't exist because data loss is not fired.
            var result = ((double)Value < (double)t.Value) ? true : false;

            return new DecidedCondVar(result);
        }


        public IRLiteralData BinOp(IRLiteralData t, IROperation operation)
        {
            IRLiteralData result = null;
            if (operation == IROperation.Add) result = Add(t);
            else if (operation == IROperation.Sub) result = Sub(t);
            else if (operation == IROperation.Mul) result = Mul(t);
            else if (operation == IROperation.Div) result = Div(t);
            else if (operation == IROperation.Mod) result = Mod(t);

            return result;
        }

        public abstract IRLiteralData Add(IRLiteralData t);
        public abstract IRLiteralData Sub(IRLiteralData t);
        public abstract IRLiteralData Mul(IRLiteralData t);
        public abstract IRLiteralData Div(IRLiteralData t);
        public abstract IRLiteralData Mod(IRLiteralData t);
    }
}
