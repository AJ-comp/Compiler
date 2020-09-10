using System;
using System.Collections.Generic;
using System.ComponentModel;

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

    #region Identifier related
    public class Identifier : TokenType
    {
        internal Identifier(int hashCode, string value) : base(hashCode, value) { }
    }
    #endregion
}
