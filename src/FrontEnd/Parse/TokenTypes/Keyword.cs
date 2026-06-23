namespace Parse
{
    public class Keyword : TokenType
    {
        internal Keyword(int hashCode, string value) : base(hashCode, value) { }

        public CategoryKeyword CategoryKeyword
        {
            get
            {
                var data = "CategoryKeyword";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new CategoryKeyword(hashCode, data) : cacheType as CategoryKeyword;
            }
        }

        public NormalKeyword NormalKeyword
        {
            get
            {
                var data = "NormalKeyword";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new NormalKeyword(hashCode, data) : cacheType as NormalKeyword;
            }
        }

        public Repeateword Repeateword
        {
            get
            {
                var data = "Repeateword";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Repeateword(hashCode, data) : cacheType as Repeateword;
            }
        }

        public Controlword Controlword
        {
            get
            {
                var data = "Controlword";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Controlword(hashCode, data) : cacheType as Controlword;
            }
        }

        public Accessword Accessword
        {
            get
            {
                var data = "Accessword";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Accessword(hashCode, data) : cacheType as Accessword;
            }
        }

        public DefinedDataType DefinedDataType
        {
            get
            {
                var data = "DefinedDataType";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new DefinedDataType(hashCode, data) : cacheType as DefinedDataType;
            }
        }
    }


    public class CategoryKeyword : Keyword
    {
        internal CategoryKeyword(int hashCode, string value) : base(hashCode, value) { }
    }

    public class NormalKeyword : Keyword
    {
        internal NormalKeyword(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Repeateword : Keyword
    {
        internal Repeateword(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Controlword : Keyword
    {
        internal Controlword(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Accessword : Keyword
    {
        internal Accessword(int hashCode, string value) : base(hashCode, value) { }
    }

    public class DefinedDataType : Keyword
    {
        internal DefinedDataType(int hashCode, string value) : base(hashCode, value) { }
    }
}
