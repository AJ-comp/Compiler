namespace Parse.FrontEnd.InterLanguages.Datas
{
    public abstract class IRLiteralData : IRData
    {
        public abstract DataType Type { get; }
        public abstract object Value { get; }

        public static DataType GreaterType(IRLiteralData t1, IRLiteralData t2)
        {
            var t1TypeSize = IRConverter.ToAlign(t1.Type);
            var t2TypeSize = IRConverter.ToAlign(t2.Type);

            return (t1TypeSize >= t2TypeSize) ? t1.Type : t2.Type;
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
