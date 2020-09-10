namespace Parse
{
    public class SpecialToken : TokenType
    {
        internal SpecialToken(int hashCode, string value) : base(hashCode, value) { }

        public Comment Comment
        {
            get
            {
                var data = "Comment";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Comment(hashCode, data) : cacheType as Comment;
            }
        }

        public Delimiter Delimiter
        {
            get
            {
                var data = "Delimiter";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Delimiter(hashCode, data) : cacheType as Delimiter;
            }
        }

        public NotDefinedType NotDefined
        {
            get
            {
                var data = "NotDefinedType";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new NotDefinedType(hashCode, data) : cacheType as NotDefinedType;
            }
        }

        public CustomType Custom
        {
            get
            {
                var data = "CustomType";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new CustomType(hashCode, data) : cacheType as CustomType;
            }
        }

        public EpsilonType Epsilon
        {
            get
            {
                var data = "EpsilonType";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new EpsilonType(hashCode, data) : cacheType as EpsilonType;
            }
        }

        public Marker Marker
        {
            get
            {
                var data = "Marker";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Marker(hashCode, data) : cacheType as Marker;
            }
        }
    }



    public class Comment : SpecialToken
    {
        internal Comment(int hashCode, string value) : base(hashCode, value) { }

        public LineComment LineComment
        {
            get
            {
                var data = "LineComment";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new LineComment(hashCode, data) : cacheType as LineComment;
            }
        }

        public ScopeComment ScopeComment
        {
            get
            {
                var data = "ScopeComment";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new ScopeComment(hashCode, data) : cacheType as ScopeComment;
            }
        }
    }

    public class LineComment : Comment
    {
        internal LineComment(int hashCode, string value) : base(hashCode, value) { }
    }

    public class ScopeComment : Comment
    {
        internal ScopeComment(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Delimiter : SpecialToken
    {
        internal Delimiter(int hashCode, string value) : base(hashCode, value) { }
    }

    public class NotDefinedType : SpecialToken
    {
        internal NotDefinedType(int hashCode, string value) : base(hashCode, value) { }
    }

    public class CustomType : SpecialToken
    {
        internal CustomType(int hashCode, string value) : base(hashCode, value) { }
    }

    public class EpsilonType : SpecialToken
    {
        internal EpsilonType(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Marker : SpecialToken
    {
        internal Marker(int hashCode, string value) : base(hashCode, value) { }
    }
}
