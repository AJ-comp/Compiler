namespace Parse
{
    public class Preprocesser : TokenType
    {
        internal Preprocesser(int hashCode, string value) : base(hashCode, value) { }

        public DefinePreprocesser Define
        {
            get
            {
                var data = "DefinePreprocesser";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new DefinePreprocesser(hashCode, data) : cacheType as DefinePreprocesser;
            }
        }
    }

    public class DefinePreprocesser : Preprocesser
    {
        internal DefinePreprocesser(int hashCode, string value) : base(hashCode, value) { }
    }
}
