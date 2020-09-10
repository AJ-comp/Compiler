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

        /// <summary> '[' or ']' </summary>
        public Square Square
        {
            get
            {
                var data = "Square";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Square(hashCode, data) : cacheType as Square;
            }
        }

        /// <summary> '(' or ')' </summary>
        public Parenthesis Parenthesis
        {
            get
            {
                var data = "Parenthesis";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Parenthesis(hashCode, data) : cacheType as Parenthesis;
            }
        }

        /// <summary> '{' or '}' </summary>
        public CurlyBrace CurlyBrace
        {
            get
            {
                var data = "CurlyBrace";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new CurlyBrace(hashCode, data) : cacheType as CurlyBrace;
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

    public class Parenthesis : Operator
    {
        internal Parenthesis(int hashCode, string value) : base(hashCode, value) { }
    }

    public class CurlyBrace : Operator
    {
        internal CurlyBrace(int hashCode, string value) : base(hashCode, value) { }
    }
}
