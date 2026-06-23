namespace Parse
{
    public class Operator : TokenType
    {
        internal Operator(int hashCode, string value) : base(hashCode, value) { }

        public NormalOperator NormalOperator
        {
            get
            {
                var data = "NormalOperator";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new NormalOperator(hashCode, data) : cacheType as NormalOperator;
            }
        }

        public Comma Comma
        {
            get
            {
                var data = "Comma";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Comma(hashCode, data) : cacheType as Comma;
            }
        }

        /// <summary> open token of pair token </summary>
        public PairOpen PairOpen
        {
            get
            {
                var data = "PairOpen";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new PairOpen(hashCode, data) : cacheType as PairOpen;
            }
        }

        /// <summary> close token of pair token </summary>
        public PairClose PairClose
        {
            get
            {
                var data = "PairClose";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new PairClose(hashCode, data) : cacheType as PairClose;
            }
        }

        public ArraySymbol ArraySymbol
        {
            get
            {
                var data = "ArraySymbol";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new ArraySymbol(hashCode, data) : cacheType as ArraySymbol;
            }
        }
    }



    public class NormalOperator : Operator
    {
        internal NormalOperator(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Comma : Operator
    {
        internal Comma(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Square : Operator
    {
        internal Square(int hashCode, string value) : base(hashCode, value) { }
    }

    public class PairOpen : Operator
    {
        internal PairOpen(int hashCode, string value) : base(hashCode, value) { }
    }

    public class PairClose : Operator
    {
        internal PairClose(int hashCode, string value) : base(hashCode, value) { }
    }

    public class ArraySymbol : Operator
    {
        internal ArraySymbol(int hashCode, string value) : base(hashCode, value) { }
    }
}
