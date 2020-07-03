namespace Parse.FrontEnd.InterLanguages.Datas.Types
{
    public class DoubleType : DataType
    {
        public override int Size => 64;

        public static DataType GreaterType(DoubleType t1, DoubleType t2) => (t1.Size >= t2.Size) ? t1 : t2;

        public override string ToString() => "double";
    }
}
