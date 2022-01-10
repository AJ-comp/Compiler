namespace Parse.Types
{
    public interface IDouble : IArithmetic
    {
        public bool Nan { get; }

        public static IDataTypeSpec GreaterType(IDouble t1, IDouble t2) => (t1.Size >= t2.Size) ? t1 : t2;
    }

    public interface IDouble<T> : IDouble
    {
    }
}
