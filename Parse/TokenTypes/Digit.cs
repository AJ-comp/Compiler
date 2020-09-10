namespace Parse
{
    public class Digit : TokenType
    {
        internal Digit(int hashCode, string value) : base(hashCode, value) { }

        public Digit2 Digit2
        {
            get
            {
                var data = "Digit2";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Digit2(hashCode, data) : cacheType as Digit2;
            }
        }

        public Digit8 Digit8
        {
            get
            {
                var data = "Digit8";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Digit8(hashCode, data) : cacheType as Digit8;
            }
        }

        public Digit10 Digit10
        {
            get
            {
                var data = "Digit10";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Digit10(hashCode, data) : cacheType as Digit10;
            }
        }

        public Digit16 Digit16
        {
            get
            {
                var data = "Digit16";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Digit16(hashCode, data) : cacheType as Digit16;
            }
        }
    }


    public class Digit2 : Digit
    {
        internal Digit2(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Digit8 : Digit
    {
        internal Digit8(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Digit10 : Digit
    {
        internal Digit10(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Digit16 : Digit
    {
        internal Digit16(int hashCode, string value) : base(hashCode, value) { }
    }
}
