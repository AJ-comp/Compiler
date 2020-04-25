using System;
using System.Collections.Generic;

namespace Parse
{
    [Flags]
    public enum DrawOption { None = 0, Underline = 1, EndPointUnderline = 2, Selected = 4 }

    public abstract class TokenType
    {
        private string _originalValue;
        private readonly int _hashCode;
        private static Dictionary<int, TokenType> _totalHashValue = new Dictionary<int, TokenType>();

        protected static int GetHashCode(string value) => 2018552787 + EqualityComparer<string>.Default.GetHashCode(value);
        protected static TokenType GetTokenType(int hashValue) => (_totalHashValue.ContainsKey(hashValue)) ? _totalHashValue[hashValue] : null;

        protected TokenType(int hashCode, string value)
        {
            _hashCode = hashCode;
            _originalValue = value;

            var bExist = _totalHashValue.ContainsKey(hashCode);
            if (bExist == false) _totalHashValue.Add(hashCode, this);
            else
            {
                // check a key duplication.
                var cacheType = _totalHashValue[hashCode];
                if (cacheType._originalValue != value)
                {
                    // if key was duplicated (same key, different TokenType) then assign a new value to the hashCode and add.
                    do
                    {
                        hashCode++;
                    } while (_totalHashValue.ContainsKey(hashCode));

                    _totalHashValue.Add(hashCode, this);
                }
            }
        }

        public static Keyword Keyword
        {
            get
            {
                var data = "Keyword";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Keyword(hashCode, data) : cacheType as Keyword;
            }
        }

        public static Digit Digit
        {
            get
            {
                var data = "Digit";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Digit(hashCode, data) : cacheType as Digit;
            }
        }

        public static Operator Operator
        {
            get
            {
                var data = "Operator";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Operator(hashCode, data) : cacheType as Operator;
            }
        }

        public static SpecialToken SpecialToken
        {
            get
            {
                var data = "SpecialToken";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new SpecialToken(hashCode, data) : cacheType as SpecialToken;
            }
        }

        public static Identifier Identifier
        {
            get
            {
                var data = "Identifier";
                var hashCode = GetHashCode(data);
                var cacheType = GetTokenType(hashCode);

                return (cacheType == null) ? new Identifier(hashCode, data) : cacheType as Identifier;
            }
        }

        public static bool operator ==(TokenType left, TokenType right)
        {
            return EqualityComparer<TokenType>.Default.Equals(left, right);
        }

        public static bool operator !=(TokenType left, TokenType right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is TokenType type &&
                   _hashCode == type._hashCode;
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString() => _originalValue;
    }

    #region Keyword related
    public class Keyword : TokenType
    {
        internal Keyword(int hashCode, string value) : base(hashCode, value) { }

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
    #endregion

    #region Digit related
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
    #endregion

    #region Operator related
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
    #endregion

    #region SpecialToken related
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

    public class EpsilonType : SpecialToken
    {
        internal EpsilonType(int hashCode, string value) : base(hashCode, value) { }
    }

    public class Marker : SpecialToken
    {
        internal Marker(int hashCode, string value) : base(hashCode, value) { }
    }
    #endregion

    #region Identifier related
    public class Identifier : TokenType
    {
        internal Identifier(int hashCode, string value) : base(hashCode, value) { }
    }
    #endregion
}
